using Len.Commands;
using Len.Domain.Repositories;
using Len.Transfer.AccountBoundedContext.Commands;
using Len.Transfer.AccountBoundedContext.Events;
using MassTransit;
using MassTransit.Definition;
using System;
using System.Threading.Tasks;

namespace Len.Transfer.Consomers
{
    public class StartAccountTransferConsmer : IConsumer<IAccountTransferCommand>
    {
        public async Task Consume(ConsumeContext<IAccountTransferCommand> context)
        {
            var response = new CommandResponse
            {
                //CommandId = _expressionToGetTheMessageId.Compile()(context.Message).ToString()
                CommandId = context.Message.Id,
            };

            try
            {
                await context.Publish(new TransferStarted
                {
                    Id = Guid.NewGuid(),
                    Amount = context.Message.Amount,
                    FromAccountId = context.Message.FromAccountId,
                    ToAccountId = context.Message.ToAccountId,
                });

                response.CommandStatus = CommandStatus.Succeeded;
                response.ContainsException = false;
                response.ExceptionDetail = string.Empty;
                response.Message = "转账申请提交成功，请等待转账结果通知";
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

    public class StartAccountTransferConsmerDefinition : ConsumerDefinition<StartAccountTransferConsmer>
    {
        public StartAccountTransferConsmerDefinition()
        {
            EndpointName = "start-account-transfer";
        }
    }
}
