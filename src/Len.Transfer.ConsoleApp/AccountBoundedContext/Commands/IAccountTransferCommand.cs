using System;
using System.Collections.Generic;
using System.Text;

namespace Len.Transfer.AccountBoundedContext.Commands
{
    public interface IAccountTransferCommand : Len.Commands.ICommand
    {
        Guid FromAccountId { get; }

        Guid ToAccountId { get; }

        decimal Amount { get; }
    }
}
