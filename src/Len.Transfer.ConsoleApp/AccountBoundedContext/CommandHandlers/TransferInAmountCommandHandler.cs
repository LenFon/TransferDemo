using Len.Commands;
using Len.Domain.Repositories;
using Len.Transfer.AccountBoundedContext.Commands;
using MassTransit;
using System.Threading.Tasks;

namespace Len.Transfer.AccountBoundedContext.CommandHandlers
{
    public class TransferInAmountCommandHandler : IHandler<ITransferInAmount>, IConsumer<ITransferInAmount>
    {
        private readonly IRepository _repository;

        public TransferInAmountCommandHandler(IRepository repository)
        {

            _repository = repository;
        }

        public async Task Consume(ConsumeContext<ITransferInAmount> context)
        {
            await HandleAsync(context.Message);
        }

        public async Task HandleAsync(ITransferInAmount command)
        {
            var account = await _repository.GetByIdAsync<Account>(command.AccountId, int.MaxValue);

            account.TransferIn(command.Amount, command.FromAccountId, command.CorrelationId);

            await _repository.SaveAsync(account);
        }
    }
}
