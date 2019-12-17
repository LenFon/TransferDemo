using Len.Domain.Persistence.Memento;

namespace Len.Domain
{
    public interface IAggregateFactory
    {
        IAggregate Build<TAggregate>(IMemento snapshot = null) where TAggregate : IAggregate, new();
    }
}
