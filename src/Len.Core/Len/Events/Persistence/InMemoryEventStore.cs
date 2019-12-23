using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Len.Events.Persistence
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly ConcurrentDictionary<string, List<IEvent>> _stores = new ConcurrentDictionary<string, List<IEvent>>();

        public Task<IEnumerable<IEvent>> GetEventsAsync(Guid aggregateId, int minVersion = int.MinValue, int maxVersion = int.MaxValue)
        {
            var key = aggregateId.ToString();

            if (_stores.TryGetValue(key, out var events))
            {
                return Task.FromResult((IEnumerable<IEvent>)events);
            }

            return Task.FromResult(Enumerable.Empty<IEvent>());
        }

        public Task SaveAsync(Guid aggregateId, IEnumerable<IEvent> events, IDictionary<string, object> headers = null)
        {
            var key = aggregateId.ToString();

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
