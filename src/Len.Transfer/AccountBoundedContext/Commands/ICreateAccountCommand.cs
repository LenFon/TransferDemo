using System;

namespace Len.Transfer.AccountBoundedContext.Commands
{
    public interface ICreateAccountCommand : Len.Commands.ICommand
    {
        Guid AccountId { get; }

        decimal InitialAmount { get; }
    }
}
