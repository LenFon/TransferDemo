using System;
using Len.Events;
using Len.Message;

namespace Len.Transfer.AccountBoundedContext.Events
{
    public class AccountCreated : Len.Events.IEvent
    {
        public AccountCreated(Guid eventId, Guid accountId, decimal amount)
        {
            Id = eventId;
            AccountId = accountId;
            InitialAmount = amount;
        }

        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public decimal InitialAmount { get; set; }

        public int Version { get; set; }
    }
}
