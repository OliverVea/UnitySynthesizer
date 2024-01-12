using System.Threading;
using System.Threading.Tasks;

namespace Synthesizer.Shared.Domain
{
    internal class MessageReadingJob : IMessageReadingJob
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMessageHandlerService _messageHandlerService;

        public MessageReadingJob(
            IMessageRepository messageRepository,
            IMessageHandlerService messageHandlerService)
        {
            _messageRepository = messageRepository;
            _messageHandlerService = messageHandlerService;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await _messageRepository.ReceiveMessageAsync(cancellationToken);
                await _messageHandlerService.HandleMessageAsync(message);
            }
        }
    }
}