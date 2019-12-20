using Len.Domain.Persistence.Memento;

namespace Len.Domain
{
    public interface IAggregateFactory
    {
        TAggregate Build<TAggregate>(IMemento snapshot = null) where TAggregate : IAggregate, new();
    }
}
