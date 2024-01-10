namespace Synthesizer.Core.Infrastructure.Api;

public interface IMessageSender
{
    Task SendAsync(string message, CancellationToken cancellationToken);
    Task SendAsync(byte[] message, CancellationToken cancellationToken);
}