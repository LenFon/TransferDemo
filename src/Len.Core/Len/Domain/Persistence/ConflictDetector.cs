using System;
using System.Collections.Generic;
using System.Linq;

namespace Len.Domain.Persistence
{
    public class ConflictDetector : IConflictDetector, Volo.Abp.DependencyInjection.ISingletonDependency
    {
        private delegate bool ConflictPredicate(object uncommitted, object committed);

        private readonly IDictionary<Type, IDictionary<Type, ConflictPredicate>> actions =
            new Dictionary<Type, IDictionary<Type, ConflictPredicate>>();

        /// <summary>
        /// 检测是否有冲突
        /// </summary>
        /// <param name="uncommittedEvents"></param>
        /// <param name="committedEvents"></param>
        /// <returns></returns>
        public bool ConflictsWith(IEnumerable<object> uncommittedEvents, IEnumerable<object> committedEvents)
        {
            var query = from uncommitted in uncommittedEvents
                        from committed in committedEvents
                        where Conflicts(uncommitted, committed)
                        select uncommittedEvents;

            return query.Any();
        }

        public void Register<TUncommitted, TCommitted>(ConflictDelegate<TUncommitted, TCommitted> handler)
            where TUncommitted : class
            where TCommitted : class
        {
            var uncommittedType = typeof(TUncommitted);
            if (!actions.TryGetValue(uncommittedType, out var inner))
            {
                actions[uncommittedType] = inner = new Dictionary<Type, ConflictPredicate>();
            }

            inner[typeof(TCommitted)] = (uncommitted, committed) => handler(uncommitted as TUncommitted, committed as TCommitted);
        }

        /// <summary>
        /// 是否冲突
        /// </summary>
        /// <param name="uncommitted"></param>
        /// <param name="committed"></param>
        /// <returns></returns>
        private bool Conflicts(object uncommitted, object committed)
        {
            var committedType = committed.GetType();

            if (!actions.TryGetValue(uncommitted.GetType(), out var registration))
            {
                return uncommitted.GetType() == committedType;
            }

            if (!registration.TryGetValue(committedType, out var callback))
            {
                return true;
            }

            return callback(uncommitted, committed);
        }

    }
}
