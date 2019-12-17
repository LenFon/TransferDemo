using Len.Domain;
using Len.Domain.Persistence;
using Len.Domain.Persistence.Memento;
using Len.Events;
using NEventStore;
using NEventStore.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Len.Transfer
{
    class Repository : Domain.Repositories.IRepository
    {
        private const string AggregateTypeHeader = "AggregateType";

        private readonly IStoreEvents _storeEvents;
        private readonly IAggregateFactory _factory;
        private readonly IConflictDetector _conflictDetector;

        private readonly IDictionary<string, ISnapshot> _snapshots = new Dictionary<string, ISnapshot>();
        private readonly IDictionary<string, IEventStream> _streams = new Dictionary<string, IEventStream>();

        public Repository(IStoreEvents storeEvents, IAggregateFactory factory, IConflictDetector conflictDetector)
        {
            _storeEvents = storeEvents;
            _factory = factory;
            _conflictDetector = conflictDetector;
        }

        public Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, int revisionNumber) where TAggregate : class, IAggregate, new()
        {
            var snapshot = GetSnapshot(Bucket.Default, id, revisionNumber);
            var stream = OpenStream(Bucket.Default, id, revisionNumber, snapshot);
            var aggregate = GetAggregate<TAggregate>(snapshot);

            ApplyEventsToAggregate(revisionNumber, stream, aggregate);

            return Task.FromResult(aggregate as TAggregate);
        }

        public Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : IAggregate, new()
        {
            Save(Bucket.Default, aggregate, Guid.NewGuid(), null);

            return Task.CompletedTask;
        }

        private void Save(string bucketId, IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
        {
            var headers = PrepareHeaders(aggregate, updateHeaders);

            while (true)
            {
                IEventStream stream = PrepareStream(bucketId, aggregate, headers);
                int commitEventCount = stream.CommittedEvents.Count;

                try
                {
                    stream.CommitChanges(commitId);
                    aggregate.MarkChangesAsCommitted();
                    return;
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

        private IEventStream PrepareStream(string bucketId, IAggregate aggregate, Dictionary<string, object> headers)
        {
            var streamId = bucketId + "+" + aggregate.Id;
            if (!_streams.TryGetValue(streamId, out IEventStream stream))
            {
                _streams[streamId] = stream = _storeEvents.CreateStream(bucketId, aggregate.Id);
            }

            foreach (var item in headers)
            {
                stream.UncommittedHeaders[item.Key] = item.Value;
            }

            aggregate.GetUncommittedChanges()
                     .Select(x => new EventMessage { Body = x })
                     .ToList()
                     .ForEach(stream.Add);

            return stream;
        }

        private static Dictionary<string, object> PrepareHeaders(
            IAggregate aggregate, Action<IDictionary<string, object>> updateHeaders)
        {
            var headers = new Dictionary<string, object>
            {
                [AggregateTypeHeader] = aggregate.GetType().FullName
            };
            updateHeaders?.Invoke(headers);

            return headers;
        }

        private bool ThrowOnConflict(IEventStream stream, int skip)
        {
            IEnumerable<object> committed = stream.CommittedEvents.Skip(skip).Select(x => x.Body);
            IEnumerable<object> uncommitted = stream.UncommittedEvents.Select(x => x.Body);

            return _conflictDetector.ConflictsWith(uncommitted, committed);
        }
        private void ApplyEventsToAggregate(int versionToLoad, IEventStream stream, IAggregate aggregate)
        {
            if (versionToLoad == 0 || aggregate.LastEventVersion < versionToLoad)
            {
                var st = stream.CommittedEvents.ToList();

                var events = stream.CommittedEvents.Select(x => x.Body as IEvent);

                aggregate.Initialize(events);
            }
        }

        private IAggregate GetAggregate<TAggregate>(ISnapshot snapshot) where TAggregate : IAggregate, new()
        {
            var memento = snapshot == null ? null : snapshot.Payload as IMemento;

            return _factory.Build<TAggregate>(memento);
        }

        private ISnapshot GetSnapshot(string bucketId, Guid aggregateId, int version)
        {
            var snapshotId = bucketId + aggregateId;
            if (!_snapshots.TryGetValue(snapshotId, out ISnapshot snapshot))
            {
                _snapshots[snapshotId] = snapshot = _storeEvents.Advanced.GetSnapshot(bucketId, aggregateId, version);
            }

            return snapshot;
        }

        private IEventStream OpenStream(string bucketId, Guid aggregateId, int version, ISnapshot snapshot)
        {
            var streamId = bucketId + "+" + aggregateId;
            if (_streams.TryGetValue(streamId, out IEventStream stream))
            {
                return stream;
            }

            stream = snapshot == null
                ? _storeEvents.OpenStream(bucketId, aggregateId, 0, version)
                : _storeEvents.OpenStream(snapshot, version);

            return _streams[streamId] = stream;
        }
    }
}
