using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Len.Commands
{
    public interface ISendCommandsAndWaitForAResponse
    {
        Task<CommandResponse> SendAsync<TCommand>(object command, CancellationToken cancellationToken = default) where TCommand : class, ICommand;
    }

    public static class SendCommandsAndWaitForAResponseExtensions
    {
        public async static Task<CommandResponse> SendAsync<TCommand>(this ISendCommandsAndWaitForAResponse sender, TCommand command, CancellationToken cancellationToken = default) where TCommand : class, ICommand
        {
            return await sender.SendAsync<TCommand>(command, cancellationToken);
        }
    }
}