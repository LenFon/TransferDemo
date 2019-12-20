using System;
using System.Threading.Tasks;

namespace Len.Domain.Persistence.Repositories
{
    public interface IRepository
    {
        Task SaveAsync<TAggregate>(TAggregate aggregate)
            where TAggregate : IAggregate, new();

        Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, int version = int.MaxValue)
            where TAggregate : IAggregate, new();

        Task<IAggregate> GetByIdAsync(Type aggregateType, Guid id, int version = int.MaxValue);
    }

    public interface IRepository<TAggregate> where TAggregate : IAggregate, new()
    {
        Task SaveAsync(TAggregate aggregate);

        Task<TAggregate> GetByIdAsync(Guid id, int version = int.MaxValue);
    }
}
