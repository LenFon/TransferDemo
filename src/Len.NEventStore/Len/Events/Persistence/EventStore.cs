using Len.Domain.Persistence;
using NEventStore;
using NEventStore.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Len.Events.Persistence
{
    public class EventStore : IEventStore, Volo.Abp.DependencyInjection.ITransientDependency
    {
        private readonly IStoreEvents _storeEvents;
        private readonly IConflictDetector _conflictDetector;

        public EventStore(IStoreEvents storeEvents, IConflictDetector conflictDetector)
        {
            _storeEvents = storeEvents ?? throw new ArgumentNullException(nameof(storeEvents));
            _conflictDetector = conflictDetector ?? throw new ArgumentNullException(nameof(conflictDetector));
        }

        public Task<IEnumerable<IEvent>> GetEventsAsync(Guid aggregateId, int minVersion = int.MinValue, int maxVersion = int.MaxValue)
        {
            try
            {
                var stream = _storeEvents.OpenStream(Bucket.Default, aggregateId, minVersion, maxVersion);
                var events = stream.CommittedEvents.Select(x => x.Body as IEvent);

                return Task.FromResult(events);
            }
            catch (StreamNotFoundException ex)
            {
                return Task.FromResult(Enumerable.Empty<IEvent>());
            }

        }

        public Task SaveAsync(Guid aggregateId, IEnumerable<IEvent> events, IDictionary<string, object> headers = null)
        {

            while (true)
            {
                var stream = _storeEvents.OpenStream(Bucket.Default, aggregateId);

                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        stream.UncommittedHeaders.Add(item);
                    }
                }

                events
                    .Select(s => new EventMessage { Body = s })
                    .ToList()
                    .ForEach(stream.Add);

                int commitEventCount = stream.CommittedEvents.Count;

                try
                {
                    stream.CommitChanges(Guid.NewGuid());

                    return Task.CompletedTask;
                }
                catch (DuplicateCommitException ex)
                {
                    stream.ClearChanges();

                    throw ex;
                }
                catch (ConcurrencyException e)
                {
                    var conflict = ThrowOnConflict(stream, commitEventCount);
                    stream.ClearChanges();

                    if (conflict)
                    {
                        throw new ConflictingCommandException(e.Message, e);
                    }
                }
                catch (StorageException e)
                {
                    throw new PersistenceException(e.Message, e);
                }
            }

        }

        private bool ThrowOnConflict(IEventStream stream, int skip)
        {
            IEnumerable<object> committed = stream.CommittedEvents.Skip(skip).Select(x => x.Body);
            IEnumerable<object> uncommitted = stream.UncommittedEvents.Select(x => x.Body);

            return _conflictDetector.ConflictsWith(uncommitted, committed);
        }
    }
}
