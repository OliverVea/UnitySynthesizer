using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;
using Synthesizer.Shared;
using Synthesizer.Shared.Domain;
using Synthesizer.Shared.Extensions;

namespace Tests.Shared.Domain;

public class MessageHandling : ServerClientIT
{
    [Test]
    public async Task CanHandleMessage()
    {
        var receivedMessage = false;
        
        ServerServiceProvider.RegisterMessageHandler<TestMessage>(_ => receivedMessage = true);
        
        var messageReadingServer = ServerServiceProvider.GetRequiredService<IMessageReadingJob>();
        
        var _ = messageReadingServer.RunAsync(CancellationToken.None);
        
        var clientMessageRepository = ClientServiceProvider.GetRequiredService<IMessageRepository>();
        
        // Act
        var message = new TestMessage();
        await clientMessageRepository.SendMessageAsync(message, CancellationToken.None);
        
        await Task.Delay(10);
        
        // Assert
        Assert.That(receivedMessage, Is.True);
    }

    [ProtoContract]
    private class TestMessage : Message;
}