using Len.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Len.Domain
{
    public abstract class Aggregate : IAggregate
    {
        private readonly List<IEvent> _changes = new List<IEvent>();

        public abstract Guid Id { get; }
        public int LastEventVersion { get; protected set; }

        public bool HasUncommittedChanges => _changes.Any();

        protected void ApplyEvent(IEvent @event, Action<IEvent> applyDelegate)
        {
            _changes.Add(@event);
            @event.Version = ++LastEventVersion;
            applyDelegate(@event);
        }

        public IEnumerable<IEvent> GetUncommittedChanges() => _changes;

        public void MarkChangesAsCommitted() => _changes.Clear();

        public void Initialize(IEnumerable<IEvent> eventHistories)
        {
            if (HasUncommittedChanges)
                throw new InvalidOperationException("Cannot intialize an aggregate root that contains uncommitted changes.");

            InitializeFromHistory(eventHistories);
        }

        protected abstract void InitializeFromHistory(IEnumerable<IEvent> eventHistories);

        public IEnumerable<IEvent> NoEvents => new IEvent[0];
    }

    public class Aggregate<TAggregateState> : Aggregate
        where TAggregateState : AggregateState, new()
    {
        public Aggregate()
        {
            State = new TAggregateState();
        }

        public override Guid Id => State.Id;

        protected TAggregateState State { get; }

        protected override void InitializeFromHistory(IEnumerable<IEvent> eventHistories)
        {
            State.Initialize(eventHistories);
            LastEventVersion = State.Version;
        }

        protected void ApplyEvent(IEvent @event)
        {
            ApplyEvent(@event, evt => State.Apply(evt));
        }
    }
}
