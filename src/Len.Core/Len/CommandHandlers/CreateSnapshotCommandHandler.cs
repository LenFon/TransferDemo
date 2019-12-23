using Len.Commands;
using Len.Domain.Persistence.Memento;
using Len.Domain.Persistence.Repositories;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Reflection;

namespace Len.CommandHandlers
{
    [CommandHandler]
    public class CreateSnapshotCommandHandler : ICommandHandler<ICreateSnapshot>
    {
        private readonly IRepository _repository;
        private readonly IMementoStore _mementoStore;
        private readonly ITypeFinder _typeFinder;

        public CreateSnapshotCommandHandler(IRepository repository, IMementoStore mementoStore, ITypeFinder typeFinder)
        {
            _repository = repository;
            _mementoStore = mementoStore;
            _typeFinder = typeFinder;
        }

        public async Task HandleAsync(ICreateSnapshot command)
        {
            var aggregateType = _typeFinder.Types.Single(w => w.FullName == command.AggregateTypeFullName);
            var account = await _repository.GetByIdAsync(aggregateType, command.AggregateId, command.Version);

            var memento = (account as IOriginator)?.CreateMemento();

            if (memento != null)
            {
                await _mementoStore.SaveAsync(memento);
            }
        }
    }
}
