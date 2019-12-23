using Len.Domain.Persistence.Memento;
using Len.Events.Persistence;
using System;
using System.Threading.Tasks;

namespace Len.Domain.Persistence.Repositories
{
    public class Repository : IRepository
    {
        public const string AggregateType = "AggregateType";
        private readonly IAggregateFactory _factory;
        private readonly IEventStore _eventStore;

        public Repository(IAggregateFactory factory, IEventStore eventStore)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));

            MementoStore = NullMementoStore.Instance;
        }

        public IMementoStore MementoStore { get; set; }

        public async Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : IAggregate, new()
        {
            var events = aggregate.GetUncommittedChanges();
            var headers = new System.Collections.Generic.Dictionary<string, object>
            {
                [AggregateType] = aggregate.GetType().FullName
            };

            await _eventStore.SaveAsync(aggregate.Id, events, headers);

            aggregate.MarkChangesAsCommitted();
        }

        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, int version = int.MaxValue)
             where TAggregate : IAggregate, new()
        {
            var memento = await MementoStore.GetMementoAsync(id, version);
            var aggregate = _factory.Build<TAggregate>(memento);

            await ApplyEventsAsync(id, version, aggregate);

            return aggregate;
        }

        public async Task<IAggregate> GetByIdAsync(Type aggregateType, Guid id, int version = int.MaxValue)
        {
            var memento = await MementoStore.GetMementoAsync(id, version);
            var aggregate = _factory.Build(aggregateType, memento);

            await ApplyEventsAsync(id, version, aggregate);

            return aggregate;
        }

        private async Task ApplyEventsAsync(Guid id, int version, IAggregate aggregate)
        {
            if (aggregate.LastEventVersion <= version)
            {
                var events = await _eventStore.GetEventsAsync(id, aggregate.LastEventVersion + 1, version);

                aggregate.Initialize(events);
            }
        }
    }

    public class Repository<TAggregate> : IRepository<TAggregate>
        where TAggregate : IAggregate, new()
    {
        private readonly IRepository _innerRepository;

        public Repository(IRepository innerRepository)
        {
            _innerRepository = innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));
        }

        public Task<TAggregate> GetByIdAsync(Guid id, int version = int.MaxValue) =>
            _innerRepository.GetByIdAsync<TAggregate>(id, version);

        public Task SaveAsync(TAggregate aggregate) =>
            _innerRepository.SaveAsync(aggregate);
    }
}
