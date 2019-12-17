using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Len.Commands
{
    public interface IHandler<in TCommand, out TResult> where TCommand : ICommand
    {
        TResult Handle(TCommand command);
    }

    public interface IHandler<in TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command);
    }
}
