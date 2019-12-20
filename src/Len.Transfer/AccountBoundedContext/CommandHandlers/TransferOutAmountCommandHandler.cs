using Len.Commands;
using Len.Domain.Repositories;
using Len.Transfer.AccountBoundedContext.Commands;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Len.Transfer.AccountBoundedContext.CommandHandlers
{
    public class TransferOutAmountCommandHandler : ICommandHandler<ITransferOutAmount>
    {
        private readonly ILogger<TransferOutAmountCommandHandler> _log;
        private readonly IRepository _repository;

        public TransferOutAmountCommandHandler(ILogger<TransferOutAmountCommandHandler> log, IRepository repository)
        {
            _log = log;
            _repository = repository;
        }

        public async Task HandleAsync(ITransferOutAmount command)
        {
            var account = await _repository.GetByIdAsync<Account>(command.AccountId, int.MaxValue);

            account.TransferOut(command.Amount, command.ToAccountId, command.CorrelationId);

            await _repository.SaveAsync(account);
        }
    }
}
