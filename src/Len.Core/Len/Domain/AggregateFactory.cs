using Len.Domain.Persistence.Memento;
using System;

namespace Len.Domain
{
    public class AggregateFactory : IAggregateFactory, Volo.Abp.DependencyInjection.ISingletonDependency
    {
        public TAggregate Build<TAggregate>(IMemento memento = null) where TAggregate : IAggregate, new()
        {
            var aggregate = new TAggregate();

            ApplyMemento(aggregate as IOriginator, memento);

            return aggregate;
        }

        public IAggregate Build(Type aggregateType, IMemento memento = null)
        {
            try
            {
                var instance = Activator.CreateInstance(aggregateType);

                ApplyMemento(instance as IOriginator, memento);

                return instance as IAggregate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ApplyMemento(IOriginator originator, IMemento memento) =>
            originator?.RestoreMemento(memento);
    }
}
