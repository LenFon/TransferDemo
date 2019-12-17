using Len.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Len.Events.Storage
{
    public interface IEventStore
    {
        Task SaveAsync(Guid aggregateId, Type aggregateType, IEnumerable<IEvent> events);

        Task<IEnumerable<IEvent>> GetForAggregateAsync(Guid aggregateId, Type aggregateType, int revisionNumber = int.MaxValue);
    }
}
