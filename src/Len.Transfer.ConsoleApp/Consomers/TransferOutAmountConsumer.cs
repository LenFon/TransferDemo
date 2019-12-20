using Len.Commands;
using Len.Transfer.AccountBoundedContext.Commands;
using MassTransit;
using MassTransit.Definition;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Len.Transfer.Consomers
{
    public class TransferOutAmountConsumer : IConsumer<ITransferOutAmount>, IConsumer<Fault<ITransferOutAmount>>
    {
        private readonly ILogger<TransferOutAmountConsumer> _log;
        private readonly ICommandHandler<ITransferOutAmount> _handler;

        public TransferOutAmountConsumer(ILogger<TransferOutAmountConsumer> log, ICommandHandler<ITransferOutAmount> handler)
        {
            _log = log;
            _handler = handler;
        }

        public async Task Consume(ConsumeContext<ITransferOutAmount> context)
        {
            await _handler.HandleAsync(context.Message);
        }

        public async Task Consume(ConsumeContext<Fault<ITransferOutAmount>> context)
        {
            //_log.LogInformation("TransferOutAmountCommandHandler info", context);
            await System.Console.Out.WriteLineAsync(context.Message.Exceptions[0].Message);
            //throw new System.NotImplementedException();
        }
    }

    public class TransferOutAmountConsumerDefinition : ConsumerDefinition<TransferOutAmountConsumer>
    {
        public TransferOutAmountConsumerDefinition()
        {
            EndpointName = "transfer-out-ammount";
        }
    }
}
