using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Len.Events.Persistence
{
    public interface IEventStore
    {
        Task SaveAsync(Guid aggregateId, IEnumerable<IEvent> events);

        Task<IEnumerable<IEvent>> GetEventsAsync(Guid aggregateId, int minVersion = int.MinValue, int maxVersion = int.MaxValue);
    }
}
