using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Len.Events.Storage
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly ConcurrentDictionary<string, List<IEvent>> _stores = new ConcurrentDictionary<string, List<IEvent>>();

        public Task<IEnumerable<IEvent>> GetForAggregateAsync(Guid aggregateId, Type aggregateType, int revisionNumber = int.MaxValue)
        {
            var key = $"{aggregateType.FullName}-{aggregateId.ToString()}";

            if (_stores.TryGetValue(key, out var events))
            {
                return Task.FromResult((IEnumerable<IEvent>)events);
            }

            return Task.FromResult(Enumerable.Empty<IEvent>());
        }

        public Task SaveAsync(Guid aggregateId, Type aggregateType, IEnumerable<IEvent> events)
        {
            var key = $"{aggregateType.FullName}-{aggregateId.ToString()}";

            if (!_stores.TryGetValue(key, out var localEvents))
            {
                localEvents = new List<IEvent>();
                _stores.TryAdd(key, localEvents);
            }

            localEvents.AddRange(events);

            return Task.CompletedTask;
        }
    }
}
