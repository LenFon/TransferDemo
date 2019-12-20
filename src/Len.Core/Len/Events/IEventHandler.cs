using Len.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Len.Events
{
    public interface IEventHandler<in TEvent> : IMessageHandler<TEvent>
       where TEvent : IEvent
    {
        new Task HandleAsync(TEvent @event);
    }
}
