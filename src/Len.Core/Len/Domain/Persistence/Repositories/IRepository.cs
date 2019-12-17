using System;
using System.Threading.Tasks;

namespace Len.Domain.Repositories
{
    public interface IRepository
    {
        Task SaveAsync<TAggregate>(TAggregate aggregate)
            where TAggregate : IAggregate, new();

        Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, int revisionNumber)
            where TAggregate : class, IAggregate, new();
    }
}
