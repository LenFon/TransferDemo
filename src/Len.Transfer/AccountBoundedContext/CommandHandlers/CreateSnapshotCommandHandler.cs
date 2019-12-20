using Len.Commands;
using Len.Domain.Persistence.Memento;
using Len.Domain.Persistence.Repositories;
using Len.Transfer.AccountBoundedContext.Commands;
using System;
using System.Threading.Tasks;

namespace Len.Transfer.AccountBoundedContext.CommandHandlers
{
    [CommandHandler]
    public class CreateSnapshotCommandHandler : ICommandHandler<ICreateSnapshot>
    {
        private readonly IRepository _repository;
        private readonly IMementoStore _mementoStore;

        public CreateSnapshotCommandHandler(IRepository repository, IMementoStore mementoStore)
        {
            _repository = repository;
            _mementoStore = mementoStore;
        }

        public async Task HandleAsync(ICreateSnapshot command)
        {
            var account = await _repository.GetByIdAsync(typeof(Account), command.AggregateId, command.Version);

            var memento = (account as IOriginator)?.CreateMemento();

            if (memento != null)
            {
                await _mementoStore.SaveAsync(memento);
            }
        }
    }
}
