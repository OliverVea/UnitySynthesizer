using System.Threading;
using System.Threading.Tasks;
using Synthesizer.Shared.Infrastructure;

namespace Synthesizer.Shared.Domain
{
    internal class ConnectionManagementService : IConnectionManager
    {
        private readonly INamedPipeStream _namedPipeStream;

        public ConnectionManagementService(INamedPipeStream namedPipeStream)
        {
            _namedPipeStream = namedPipeStream;
        }

        public Task ConnectAsync(CancellationToken cancellationToken)
        {
            return _namedPipeStream.ConnectAsync(cancellationToken);
            
        }

        public Task DisconnectAsync()
        {
            return _namedPipeStream.DisposeAsync().AsTask();
        }
    }
}