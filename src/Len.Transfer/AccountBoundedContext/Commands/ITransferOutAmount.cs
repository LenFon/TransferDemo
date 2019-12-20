using System;

namespace Len.Transfer.AccountBoundedContext.Commands
{
    public interface ITransferOutAmount : Len.Commands.ICommand
    {
        Guid AccountId { get; }

        Guid ToAccountId { get; }

        decimal Amount { get; }

        Guid CorrelationId { get; }
    }
}
