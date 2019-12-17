using Len.Domain.Persistence.Memento;

namespace Len.Domain
{
    public class AggregateFactory : IAggregateFactory
    {
        public IAggregate Build<TAggregate>(IMemento memento = null) where TAggregate : IAggregate, new()
        {
            var aggregate = new TAggregate();

            if (memento != null)
            {
                (aggregate as IOriginator)?.RestoreMemento(memento);
            }

            return aggregate;
        }
    }
}
