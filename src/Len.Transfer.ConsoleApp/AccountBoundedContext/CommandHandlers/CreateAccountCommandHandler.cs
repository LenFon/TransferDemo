using Len.Commands;
using Len.Domain.Repositories;
using Len.Transfer.AccountBoundedContext.Commands;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Len.Transfer.AccountBoundedContext.CommandHandlers
{
    public class CreateAccountCommandHandler : IHandler<ICreateAccountCommand>, IConsumer<ICreateAccountCommand>
    {
        private readonly IRepository _repository;

        public CreateAccountCommandHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<ICreateAccountCommand> context)
        {
            var response = new CommandResponse
            {
                CommandId = context.Message.AccountId,
            };
            try
            {
                await HandleAsync(context.Message);

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

        public async Task HandleAsync(ICreateAccountCommand command)
        {
            var account =new Account(command.AccountId,command.InitialAmount);

            await _repository.SaveAsync(account);
        }
    }
}
