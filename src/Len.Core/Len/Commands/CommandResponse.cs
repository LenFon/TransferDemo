using System;
using System.Collections.Generic;
using System.Text;

namespace Len.Commands
{
    public class CommandResponse
    {
        public Guid CommandId { get; set; }

        public CommandStatus CommandStatus { get; set; }

        public string Message { get; set; }

        public bool ContainsException { get; set; }

        public string ExceptionDetail { get; set; }
    }

    public enum CommandStatus
    {
        Succeeded = 0,
        Failed = 1,
    }
}
