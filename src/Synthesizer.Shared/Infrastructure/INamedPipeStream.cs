using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synthesizer.Shared.Infrastructure
{
    internal interface INamedPipeStream : IDisposable, IAsyncDisposable
    {
        void Write(ReadOnlyMemory<byte> serializedMessage);
        Task WriteAsync(ReadOnlyMemory<byte> serializedMessage, CancellationToken cancellationToken);
        Task<ReadOnlyMemory<byte>> ReadAsync(CancellationToken cancellationToken);
        Task ConnectAsync(CancellationToken cancellationToken);
    }
}