using Len.Commands;
using Len.Domain.Repositories;
using Len.Transfer.AccountBoundedContext.Commands;
using System.Threading.Tasks;

namespace Len.Transfer.AccountBoundedContext.CommandHandlers
{
    [CommandHandler]
    public class TransferInAmountCommandHandler : ICommandHandler<ITransferInAmount>
    {
        private readonly IRepository _repository;

        public TransferInAmountCommandHandler(IRepository repository)
        {

            _repository = repository;
        }

        public async Task HandleAsync(ITransferInAmount command)
        {
            var account = await _repository.GetByIdAsync<Account>(command.AccountId, int.MaxValue);

            account.TransferIn(command.Amount, command.FromAccountId, command.CorrelationId);

            await _repository.SaveAsync(account);
        }
    }
}
