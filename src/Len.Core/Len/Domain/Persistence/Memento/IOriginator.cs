using System;
using System.Collections.Generic;
using System.Text;

namespace Len.Domain.Persistence.Memento
{
    public interface IOriginator
    {
        IMemento CreateMemento();

        void RestoreMemento(IMemento memento);
    }
}
