using System;
using System.Collections.Generic;
using System.Text;

namespace Len.Domain.Persistence.Memento
{
    public interface IMemento
    {
        Guid Id { get; }

        int Version { get; }
    }
}
