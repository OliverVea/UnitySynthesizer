using System.Threading;
using System.Threading.Tasks;
using Synthesizer.Shared.Domain;

namespace Synthesizer.Shared.Infrastructure
{
    internal class MessageRepository : IMessageRepository
    {
        private readonly IMessageSerializer _messageSerializer;
        private readonly IConnectionManager _connectionManager;

        public MessageRepository(IMessageSerializer messageSerializer, IConnectionManager connectionManager)
        {
            _messageSerializer = messageSerializer;
            _connectionManager = connectionManager;
        }

        public void SendMessage<T>(T message) where T : Message
        {
            var serializedMessage = _messageSerializer.Serialize(message);
            _connectionManager.NamedPipeStream.Write(serializedMessage);
        }

        public Task SendMessageAsync<T>(T message, CancellationToken cancellationToken) where T : Message
        {
            var serializedMessage = _messageSerializer.Serialize(message);
            return _connectionManager.NamedPipeStream.WriteAsync(serializedMessage, cancellationToken);
        }

        public async Task<Message> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            var serializedMessage = await _connectionManager.NamedPipeStream.ReadAsync(cancellationToken);
            return _messageSerializer.Deserialize(serializedMessage);
        }
    }
}