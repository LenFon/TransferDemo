using System;
using System.Collections.Generic;
using System.Text;

namespace Len.Events
{
    public interface IHandler<TEvent> where TEvent : IEvent
    {
        void Handle(TEvent @event);
    }
}
