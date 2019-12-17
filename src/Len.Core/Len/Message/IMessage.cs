using System;
using System.Collections.Generic;
using System.Text;

namespace Len.Message
{
    public interface IMessage
    {
        Guid Id { get; }
    }
}
