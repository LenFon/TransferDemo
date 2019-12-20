using System.Threading.Tasks;

namespace Len.Message
{
    public interface IMessageHandler<in TMessage>
        where TMessage : IMessage
    {
        Task HandleAsync(TMessage message);
    }
}
