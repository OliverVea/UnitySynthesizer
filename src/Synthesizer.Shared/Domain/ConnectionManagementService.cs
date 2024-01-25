using System;
using System.Threading;
using System.Threading.Tasks;
using Synthesizer.Shared.Infrastructure;

namespace Synthesizer.Shared.Domain
{
    internal class ConnectionManagementService<T> : IConnectionManager where T : class, INamedPipeStream, new()
    {
        private T? _namedPipeStream;
        INamedPipeStream IConnectionManager.NamedPipeStream => _namedPipeStream ?? throw new ConnectionNotEstablishedException();

        public Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (_namedPipeStream != null) return Task.CompletedTask;

            _namedPipeStream = new T();
            return _namedPipeStream.ConnectAsync(cancellationToken);
        }

        public Task DisconnectAsync()
        {
            if (_namedPipeStream == null) return Task.CompletedTask;

            var namedPipeStream = _namedPipeStream;
            _namedPipeStream = null;
            return namedPipeStream.DisposeAsync().AsTask();
        }
    }

    internal class ConnectionNotEstablishedException : Exception
    {
    }
}