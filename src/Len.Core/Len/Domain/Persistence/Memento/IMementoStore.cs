using System;
using System.Threading.Tasks;

namespace Len.Domain.Persistence.Memento
{
    public interface IMementoStore
    {
        Task<IMemento> GetMementoAsync(Guid aggregateId, int version = int.MaxValue);

        Task SaveAsync(IMemento memento);
    }
}
