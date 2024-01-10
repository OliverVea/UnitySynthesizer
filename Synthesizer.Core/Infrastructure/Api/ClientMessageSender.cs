using System.IO.Pipes;
using Synthesizer.Core.Extensions;

namespace Synthesizer.Core.Infrastructure.Api;

public class ClientMessageSender(NamedPipeClientStream namedPipeClientStream) : IMessageSender
{
    public Task SendAsync(string message, CancellationToken cancellationToken)
    {
        return SendAsync(message.AsByteArray(), cancellationToken);
    }

    public async Task SendAsync(byte[] message, CancellationToken cancellationToken)
    {
        await namedPipeClientStream.WriteAsync(message, cancellationToken);
    }
}