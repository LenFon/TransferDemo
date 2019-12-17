using Len.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Len.Domain
{
    public abstract class AggregateState
    {
        private readonly Dictionary<Type, Action<IEvent>> _eventHandlerRoutes = new Dictionary<Type, Action<IEvent>>();

        public Guid Id { get; protected set; }

        public int Version { get; protected set; }

        public void Initialize(IEnumerable<IEvent> eventHistories)
        {
            foreach (var @event in eventHistories)
            {
                HandleEvent(@event);
            }
        }

        protected void Register<TEvent>(Action<TEvent> handlerDelegate) where TEvent : IEvent
        {
            if (handlerDelegate == null) throw new ArgumentNullException("handlerDelegate");

            _eventHandlerRoutes.Add(typeof(TEvent), evnt => handlerDelegate((TEvent)evnt));
        }

        public void Apply(IEvent @event) => HandleEvent(@event);

        private void HandleEvent(IEvent @event)
        {
            var type = @event.GetType();

            if (_eventHandlerRoutes.TryGetValue(type, out var eventHandlingDelegate))
            {
                eventHandlingDelegate(@event);
                Version = @event.Version;
            }
        }
    }
}
