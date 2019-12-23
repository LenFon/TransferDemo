using Len.Commands;
using MassTransit;
using MassTransit.Definition;
using System.Threading.Tasks;

namespace Len.Transfer.SnapshotService.Consomers
{
    public class CreateSnapshotCommandConsumer : IConsumer<ICreateSnapshot>
    {
        private readonly ICommandHandler<ICreateSnapshot> _handler;

        public CreateSnapshotCommandConsumer(ICommandHandler<ICreateSnapshot> handler)
        {
            _handler = handler;
        }

        public async Task Consume(ConsumeContext<ICreateSnapshot> context)
        {
            await _handler.HandleAsync(context.Message);
        }
    }

    public class CreateSnapshotCommandConsumerDefinition : ConsumerDefinition<CreateSnapshotCommandConsumer>
    {
        public CreateSnapshotCommandConsumerDefinition()
        {
            EndpointName = "create-snapshot";
        }
    }
}
