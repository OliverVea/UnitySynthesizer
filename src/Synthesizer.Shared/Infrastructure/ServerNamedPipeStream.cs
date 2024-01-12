using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Synthesizer.Shared.Infrastructure
{
    internal class ServerNamedPipeStream : INamedPipeStream
    {
        private readonly NamedPipeServerStream _namedPipeServerStream = new NamedPipeServerStream(
            Constants.LocalPipeName,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        
        private readonly byte[] _buffer = new byte[Constants.BufferSize];

        public void Write(ReadOnlyMemory<byte> serializedMessage)
        {
            _namedPipeServerStream.Write(serializedMessage.Span);
        }

        public Task WriteAsync(ReadOnlyMemory<byte> serializedMessage, CancellationToken cancellationToken)
        {
            return _namedPipeServerStream.WriteAsync(serializedMessage, cancellationToken).AsTask();
        }

        public async Task<ReadOnlyMemory<byte>> ReadAsync(CancellationToken cancellationToken)
        {
            var bytesRead = await _namedPipeServerStream.ReadAsync(_buffer, cancellationToken);
            return _buffer[..bytesRead];
        }

        public Task ConnectAsync(CancellationToken cancellationToken)
        {
            return _namedPipeServerStream.WaitForConnectionAsync(cancellationToken);
        }

        public void Dispose()
        {
            _namedPipeServerStream.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _namedPipeServerStream.DisposeAsync();
        }
    }
}