using System.Threading;
using System.Threading.Tasks;

namespace Synthesizer.Shared.Domain
{
    internal class MessageSendingService : IMessageSender
    {
        private readonly IMessageRepository _messageRepository;

        public MessageSendingService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public void Send<T>(T message) where T : Message
        {
            _messageRepository.SendMessage(message);
        }

        public Task SendAsync<T>(T message, CancellationToken cancellationToken) where T : Message
        {
            return _messageRepository.SendMessageAsync(message, cancellationToken);
        }
    }
}