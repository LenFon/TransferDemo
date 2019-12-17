using Len.Events;
using System;
using System.Collections.Generic;

namespace Len.Domain
{
    public interface IAggregate
    {
        Guid Id { get; }

        int LastEventVersion { get; }

        IEnumerable<IEvent> GetUncommittedChanges();

        void Initialize(IEnumerable<IEvent> eventHistories);

        void MarkChangesAsCommitted();
    }
}
