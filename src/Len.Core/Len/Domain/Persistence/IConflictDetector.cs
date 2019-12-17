using System;
using System.Collections.Generic;
using System.Text;

namespace Len.Domain.Persistence
{
    public interface IConflictDetector
    {
        void Register<TUncommitted, TCommitted>(ConflictDelegate<TUncommitted, TCommitted> handler)
            where TUncommitted : class
            where TCommitted : class;

        bool ConflictsWith(IEnumerable<object> uncommittedEvents, IEnumerable<object> committedEvents);
    }

    public delegate bool ConflictDelegate<in TUncommitted, in TCommitted>(TUncommitted uncommitted, TCommitted committed)
        where TUncommitted : class
        where TCommitted : class;
}
