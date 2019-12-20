using System.Threading.Tasks;

namespace Len.Message
{
    public interface IMessageHandler<in TMessage>
        where TMessage : IMessage
    {
        Task HandleAsync(TMessage message);
    }

    public interface IMessageHandler<in TMessage, TResult>
        where TMessage : IMessage
    {
        Task<TResult> HandleAsync(TMessage message);
    }
}
