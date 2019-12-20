using Len.Domain.Persistence.Memento;
using System;

namespace Len.Domain
{
    public interface IAggregateFactory
    {
        TAggregate Build<TAggregate>(IMemento snapshot = null) where TAggregate : IAggregate, new();

        IAggregate Build(Type aggregateType, IMemento memento = null);
    }
}
