using System;

namespace Len.Events
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class EventHandlerAttribute : Attribute
    {
    }
}
