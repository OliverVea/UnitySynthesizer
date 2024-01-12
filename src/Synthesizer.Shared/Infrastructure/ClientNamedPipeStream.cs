using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Synthesizer.Shared.Infrastructure
{
    internal class ClientNamedPipeStream : INamedPipeStream
    {
        private readonly NamedPipeClientStream _namedPipeClientStream = new NamedPipeClientStream(
            Constants.LocalServerName,
            Constants.LocalPipeName,    
            PipeDirection.InOut,
            PipeOptions.Asynchronous);
        
        private readonly byte[] _buffer = new byte[Constants.BufferSize];
        
        public void Write(ReadOnlyMemory<byte> serializedMessage)
        {
            _namedPipeClientStream.Write(serializedMessage.Span);
        }
        
        public Task WriteAsync(ReadOnlyMemory<byte> serializedMessage, CancellationToken cancellationToken)
        {
            return _namedPipeClientStream.WriteAsync(serializedMessage, cancellationToken).AsTask();
        }

        public async Task<ReadOnlyMemory<byte>> ReadAsync(CancellationToken cancellationToken)
        {
            var bytesRead = await _namedPipeClientStream.ReadAsync(_buffer, cancellationToken);
            return _buffer[..bytesRead];
        }

        public Task ConnectAsync(CancellationToken cancellationToken)
        {
            return _namedPipeClientStream.ConnectAsync(cancellationToken);
        }

        public void Dispose()
        {
            _namedPipeClientStream.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _namedPipeClientStream.DisposeAsync();
        }
    }
}