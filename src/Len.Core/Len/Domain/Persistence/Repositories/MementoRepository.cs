using Len.Domain.Persistence.Memento;
using Len.Events.Persistence;
using System;
using System.Threading.Tasks;

namespace Len.Domain.Persistence.Repositories
{
    public class MementoRepository : IRepository
    {
        private readonly IAggregateFactory _factory;
        private readonly IEventStore _eventStore;
        public readonly IMementoStore _mementoStore;

        public MementoRepository(IAggregateFactory factory, IEventStore eventStore, IMementoStore mementoStore)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _mementoStore = mementoStore ?? throw new ArgumentNullException(nameof(mementoStore));
        }

        public async Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : IAggregate, new()
        {
            var events = aggregate.GetUncommittedChanges();

            await _eventStore.SaveAsync(aggregate.Id, events);

            aggregate.MarkChangesAsCommitted();
        }

        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, int version = int.MaxValue)
             where TAggregate : IAggregate, new()
        {
            var memento = await _mementoStore.GetMementoAsync(id, version);
            var aggregate = _factory.Build<TAggregate>(memento);

            await ApplyEventsAsync(id, version, aggregate);

            return aggregate;
        }

        public async Task<IAggregate> GetByIdAsync(Type aggregateType, Guid id, int version = int.MaxValue)
        {
            var memento = await _mementoStore.GetMementoAsync(id, version);
            var aggregate = _factory.Build(aggregateType, memento);

            await ApplyEventsAsync(id, version, aggregate);

            return aggregate;
        }

        private async Task ApplyEventsAsync(Guid id, int version, IAggregate aggregate)
        {
            var events = await _eventStore.GetEventsAsync(id, aggregate.LastEventVersion + 1, version);

            aggregate.Initialize(events);
        }
    }

    public class MementoRepository<TAggregate> : IRepository<TAggregate>
        where TAggregate : IAggregate, new()
    {
        private readonly IRepository _innerRepository;

        public MementoRepository(IRepository innerRepository)
        {
            _innerRepository = innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));
        }

        public Task<TAggregate> GetByIdAsync(Guid id, int version = int.MaxValue) =>
            _innerRepository.GetByIdAsync<TAggregate>(id, version);

        public Task SaveAsync(TAggregate aggregate) =>
            _innerRepository.SaveAsync(aggregate);
    }
}
