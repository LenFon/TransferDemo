using System;

namespace Len.Commands
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CommandHandlerAttribute : Attribute
    {
    }
}
