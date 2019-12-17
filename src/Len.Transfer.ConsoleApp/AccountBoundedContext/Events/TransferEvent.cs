using System;
using System.Collections.Generic;
using System.Text;

namespace Len.Transfer.AccountBoundedContext.Events
{
    public class TransferStarted : Len.Events.IEvent
    {
        public Guid Id { get; set; }

        public Guid FromAccountId { get; set; }

        public Guid ToAccountId { get; set; }

        public decimal Amount { get; set; }

        public int Version { get; set; }
    }

    public class TransferOutAmountCompleted : Len.Events.IEvent
    {

        public TransferOutAmountCompleted(Guid id, Guid accountId, Guid toAccountId, decimal amount, Guid correlationId)
        {
            Id = id;
            AccountId = accountId;
            Amount = amount;
            ToAccountId = toAccountId;
            CorrelationId = correlationId;
        }

        public Guid Id { get; }

        public Guid AccountId { get; }

        public decimal Amount { get; }

        public Guid ToAccountId { get; }

        public Guid CorrelationId { get; }

        public int Version { get; set; }
    }

    public class TransferInAmountCompleted : Len.Events.IEvent
    {
        public TransferInAmountCompleted(Guid id, Guid accountId, Guid fromAccountId, decimal amount, Guid correlationId)
        {
            Id = id;
            AccountId = accountId;
            Amount = amount;
            FromAccountId = fromAccountId;
            CorrelationId = correlationId;
        }

        public Guid Id { get; }

        public Guid AccountId { get; }

        public decimal Amount { get; }

        public Guid FromAccountId { get; }

        public Guid CorrelationId { get; }

        public int Version { get; set; }
    }
}
