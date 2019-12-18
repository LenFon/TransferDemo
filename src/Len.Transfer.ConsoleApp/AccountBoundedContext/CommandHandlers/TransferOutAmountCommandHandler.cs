using Len.Commands;
using Len.Domain.Repositories;
using Len.Transfer.AccountBoundedContext.Commands;
using MassTransit;
using System.Threading.Tasks;

namespace Len.Transfer.AccountBoundedContext.CommandHandlers
{
    public class TransferOutAmountCommandHandler : IHandler<ITransferOutAmount>, IConsumer<ITransferOutAmount>, IConsumer<Fault<ITransferOutAmount>>
    {
        private readonly IRepository _repository;

        public TransferOutAmountCommandHandler(IRepository repository)
        {

            _repository = repository;
        }

        public async Task Consume(ConsumeContext<ITransferOutAmount> context)
        {
            await HandleAsync(context.Message);
        }

        public async Task Consume(ConsumeContext<Fault<ITransferOutAmount>> context)
        {
            await System.Console.Out.WriteLineAsync(context.Message.Exceptions[0].Message);
            //throw new System.NotImplementedException();
        }

        public async Task HandleAsync(ITransferOutAmount command)
        {
            var account = await _repository.GetByIdAsync<Account>(command.AccountId, int.MaxValue);

            account.TransferOut(command.Amount, command.ToAccountId, command.CorrelationId);

            await _repository.SaveAsync(account);
        }
    }
}
