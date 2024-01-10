using System.IO.Pipes;
using Synthesizer.Core.Extensions;

namespace Synthesizer.Core.Infrastructure.Api;

public class ServerMessageSender(NamedPipeServerStream namedPipeServerStream) : IMessageSender
{
    public Task SendAsync(string message, CancellationToken cancellationToken)
        => SendAsync(message.AsByteArray(), cancellationToken);

    public async Task SendAsync(byte[] message, CancellationToken cancellationToken)
    {
        await namedPipeServerStream.WriteAsync(message, cancellationToken);
    }
}