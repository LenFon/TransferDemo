using Len.Commands;
using Len.Transfer.AccountBoundedContext.Commands;
using MassTransit;
using MassTransit.Definition;
using System.Threading.Tasks;

namespace Len.Transfer.Consomers
{
    public class TransferInAmountConsomer : IConsumer<ITransferInAmount>
    {
        private readonly ICommandHandler<ITransferInAmount> _handler;
        public TransferInAmountConsomer(ICommandHandler<ITransferInAmount> handler)
        {
            _handler = handler;
        }

        public async Task Consume(ConsumeContext<ITransferInAmount> context)
        {
            await _handler.HandleAsync(context.Message);
        }
    }

    public class TransferInAmountConsomerDefinition : ConsumerDefinition<TransferInAmountConsomer>
    {
        public TransferInAmountConsomerDefinition()
        {
            EndpointName = "transfer-in-ammount";
        }
    }
}
