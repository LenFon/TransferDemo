using System;
using System.Collections.Generic;
using System.Text;

namespace Len.Transfer.AccountBoundedContext.Commands
{
    public interface ITransferInAmount : Len.Commands.ICommand
    {
        Guid AccountId { get; }

        Guid FromAccountId { get; }

        decimal Amount { get; }

        Guid CorrelationId { get; }
    }
}
