using Len.Domain;
using Len.Transfer.AccountBoundedContext.Events;
using System;

namespace Len.Transfer.AccountBoundedContext
{
    public class Account : Aggregate<AccountState>
    {
        public Account()
        {
        }

        public Account(Guid id, decimal amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("初始金额不能小于0");
            }

            ApplyEvent(new AccountCreated(Guid.NewGuid(), id, amount));
        }

        public void TransferOut(decimal amount, Guid toAccountId, Guid correlationId)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("转出金额不能小于0");
            }

            if (State.Amount < amount)
            {
                throw new ArgumentOutOfRangeException("账户余额不足");
            }

            ApplyEvent(new TransferOutAmountCompleted(Guid.NewGuid(), State.Id, toAccountId, amount, correlationId));
        }

        public void TransferIn(decimal amount, Guid fromAccountId, Guid correlationId)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("转入金额不能小于0");
            }

            ApplyEvent(new TransferInAmountCompleted(Guid.NewGuid(), State.Id, fromAccountId, amount, correlationId));
        }

        public override string ToString()
        {

            return Newtonsoft.Json.JsonConvert.SerializeObject(State);
        }
    }

    public class AccountState : AggregateState
    {
        public AccountState()
        {
            Id = Guid.Parse("359ca1d5-e315-4b92-b0e9-e1832749aa88");
            Register<AccountCreated>(evt => When(evt));
            Register<TransferOutAmountCompleted>(evt => When(evt));
            Register<TransferInAmountCompleted>(evt => When(evt));
        }

        public decimal Amount { get; private set; }

        private void When(AccountCreated evt)
        {
            Id = evt.AccountId;
            Amount = evt.InitialAmount;
        }

        private void When(TransferOutAmountCompleted evt)
        {
            Amount -= evt.Amount;
        }

        private void When(TransferInAmountCompleted evt)
        {
            Amount += evt.Amount;
        }
    }
}
