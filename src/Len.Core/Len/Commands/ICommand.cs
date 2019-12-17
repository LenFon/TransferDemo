using Len.Message;

namespace Len.Commands
{
    public interface ICommand : IMessage
    {
    }

    public interface ICommand<T> : ICommand
    {
        T Data { get; }
    }
}
