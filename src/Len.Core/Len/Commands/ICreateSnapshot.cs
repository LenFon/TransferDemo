using System;
using System.Collections.Generic;
using System.Text;

namespace Len.Commands
{
    public interface ICreateSnapshot : ICommand
    {
        Guid AggregateId { get; }

        int Version { get; }
    }
}
