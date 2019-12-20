using Len.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Len.Commands
{
    public interface ICommandHandler<in TCommand, TResult> : IMessageHandler<TCommand, TResult>
        where TCommand : ICommand
    {
        new Task<TResult> HandleAsync(TCommand command);
    }

    public interface ICommandHandler<in TCommand> : IMessageHandler<TCommand>
        where TCommand : ICommand
    {
        new Task HandleAsync(TCommand command);
    }
}
