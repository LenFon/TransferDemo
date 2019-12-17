using Len.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace Len.Events
{
    public interface IEvent : IMessage
    {
        int Version { get; set; }
    }
}
