using System.Threading;
using System.Threading.Tasks;

namespace Synthesizer.Shared.Domain
{
    internal interface IMessageRepository
    {
        void SendMessage<T>(T message) where T : Message;
        Task SendMessageAsync<T>(T message, CancellationToken cancellationToken) where T : Message;
        Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken);
    }
}