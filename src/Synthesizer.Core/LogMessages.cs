using Microsoft.Extensions.Logging;

namespace Synthesizer.Core;

public partial class LogMessages
{
    [LoggerMessage(LogLevel.Information, "Client connected")]
    public static partial void ClientConnected(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Received message: {MessageName}")]
    public static partial void ReceivedMessage(ILogger logger, string messageName);
    
    [LoggerMessage(LogLevel.Information, "Echoing message: {Message}")]
    public static partial void EchoMessage(ILogger logger, string message);
}