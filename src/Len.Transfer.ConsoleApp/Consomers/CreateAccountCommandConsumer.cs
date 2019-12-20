using Len.Commands;
using Len.Transfer.AccountBoundedContext.Commands;
using MassTransit;
using MassTransit.Definition;
using System;
using System.Threading.Tasks;

namespace Len.Transfer.Consomers
{
    public class CreateAccountCommandConsumer : IConsumer<ICreateAccountCommand>
    {
        private readonly ICommandHandler<ICreateAccountCommand> _handler;

        public CreateAccountCommandConsumer(ICommandHandler<ICreateAccountCommand> handler)
        {
            _handler = handler;
        }

        public async Task Consume(ConsumeContext<ICreateAccountCommand> context)
        {
            var response = new CommandResponse
            {
                CommandId = context.Message.AccountId,
            };

            try
            {
                await _handler.HandleAsync(context.Message);

                response.CommandStatus = CommandStatus.Succeeded;
                response.ContainsException = false;
                response.ExceptionDetail = string.Empty;
                response.Message = string.Empty;
            }
            catch (Exception ex)
            {
                response.CommandStatus = CommandStatus.Failed;
                response.ContainsException = true;
                response.ExceptionDetail = ex.ToString();
                response.Message = ex.Message;
            }


            await context.RespondAsync(response);
        }
    }

    public class CreateAccountCommandConsumerDefinition : ConsumerDefinition<CreateAccountCommandConsumer>
    {
        public CreateAccountCommandConsumerDefinition()
        {
            EndpointName = "create-account";
        }
    }
}
