using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Len.Domain.Persistence.Memento
{
    public class NullMementoStore : IMementoStore
    {
        public static IMementoStore Instance = new NullMementoStore();

        public Task<IMemento> GetMementoAsync(Guid aggregateId, int version = int.MaxValue)
        {
            return Task.FromResult(default(IMemento));
        }

        public Task SaveAsync(IMemento memento)
        {
            return Task.CompletedTask;
        }
    }
}
