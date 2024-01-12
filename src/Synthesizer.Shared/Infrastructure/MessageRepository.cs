using System.Threading;
using System.Threading.Tasks;
using Synthesizer.Shared.Domain;

namespace Synthesizer.Shared.Infrastructure
{
    internal class MessageRepository : IMessageRepository
    {
        private readonly IMessageSerializer _messageSerializer;
        private readonly INamedPipeStream _namedPipeStream;

        public MessageRepository(IMessageSerializer messageSerializer, INamedPipeStream namedPipeStream)
        {
            _messageSerializer = messageSerializer;
            _namedPipeStream = namedPipeStream;
        }

        public void SendMessage<T>(T message) where T : Message
        {
            var serializedMessage = _messageSerializer.Serialize(message);
            _namedPipeStream.Write(serializedMessage);
        }

        public Task SendMessageAsync<T>(T message, CancellationToken cancellationToken) where T : Message
        {
            var serializedMessage = _messageSerializer.Serialize(message);
            return _namedPipeStream.WriteAsync(serializedMessage, cancellationToken);
        }

        public async Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            var serializedMessage = await _namedPipeStream.ReadAsync(cancellationToken);
            return _messageSerializer.Deserialize(serializedMessage);
        }
    }
}