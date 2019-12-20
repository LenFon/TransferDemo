using Len.Domain;
using Len.Domain.Persistence.Memento;
using Len.Transfer.AccountBoundedContext.Events;
using System;

namespace Len.Transfer.AccountBoundedContext
{
    public class Account : Aggregate<AccountState>,IOriginator
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

        IMemento IOriginator.CreateMemento() => new AccountMemento(State.Id, State.Version, State.Amount);

        void IOriginator.RestoreMemento(IMemento memento)
        {
            if (memento is AccountMemento accountMemento)
            {
                State.RestoreMemento(accountMemento);
                LastEventVersion = State.Version;
            }
        }
    }

    public class AccountState : AggregateState
    {
        public AccountState()
        {
            Register<AccountCreated>(evt => When(evt));
            Register<TransferOutAmountCompleted>(evt => When(evt));
            Register<TransferInAmountCompleted>(evt => When(evt));
        }

        public AccountState(AccountMemento memento) : this()
        {
            RestoreMemento(memento);
        }

        public decimal Amount { get; private set; }

        public void RestoreMemento(AccountMemento memento)
        {
            if (memento == null)
            {
                throw new ArgumentNullException(nameof(memento));
            }

            Id = memento.Id;
            Version = memento.Version;
            Amount = memento.Amount;
        }

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

    public class AccountMemento : IMemento
    {
        public AccountMemento(Guid id, int version, decimal amount)
        {
            Id = id;
            Version = version;
            Amount = amount;
        }

        public Guid Id { get; private set; }

        public decimal Amount { get; private set; }
        public int Version { get; private set; }
    }
}
