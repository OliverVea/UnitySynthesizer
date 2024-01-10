using System.IO.Pipes;
using Synthesizer.Core.Infrastructure.Api;
using Synthesizer.Core.Infrastructure.NAudio;

namespace Synthesizer.Core;

public class ModularSynthesizerServer(
    PlaybackController playbackController,
    MessageService messageService,
    NamedPipeServerStream namedPipeServerStream)
{
    private readonly byte[] _messageBuffer = new byte[MessageConstants.MaxMessageByteSize];
    
    public const string ServerPipeName = "SynthesizerServerPipe";
    
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        playbackController.Start();
        
        await namedPipeServerStream.WaitForConnectionAsync(cancellationToken);
        
        Console.WriteLine("Connected!");

        while (!cancellationToken.IsCancellationRequested)
        {
            var bytesReceivedCount = await namedPipeServerStream.ReadAsync(_messageBuffer, cancellationToken);
            var messageBytes = _messageBuffer.Take(bytesReceivedCount).ToArray();

            var message = await messageService.ReadMessageAsync(messageBytes, cancellationToken);

            switch (message)
            {
                case EchoMessage echoMessage:
                    Console.WriteLine($"EchoMessage: {echoMessage.Message}");
                    break;
                default:
                    throw new Exception();
            }

        }
        
        playbackController.Stop();
    }
}