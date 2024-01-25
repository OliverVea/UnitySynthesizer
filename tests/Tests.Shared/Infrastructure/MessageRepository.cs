using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;
using Synthesizer.Shared;
using Synthesizer.Shared.Domain;

namespace Tests.Shared.Infrastructure;

public class MessageRepository : ServerClientIT
{
    [Test]
    public async Task CanTransferMessage()
    {
        // Arrange
        var clientMessageRepository = ClientServiceProvider.GetRequiredService<IMessageRepository>();
        var serverMessageRepository = ServerServiceProvider.GetRequiredService<IMessageRepository>();
        
        var message = new TestMessage
        {
            Content = "Hello World!"
        };
        
        // Act
        var sendTask = clientMessageRepository.SendMessageAsync(message, CancellationToken.None);
        var receiveTask = serverMessageRepository.ReceiveMessageAsync(CancellationToken.None);
        
        await Task.WhenAll(sendTask, receiveTask);
        
        var actual = receiveTask.Result;
        
        // Assert
        Assert.That(actual, Is.TypeOf<TestMessage>());
        
        var actualTestMessage = (TestMessage)actual;
        Assert.That(actualTestMessage.Content, Is.EqualTo(message.Content));
    }
    
    [ProtoContract]
    private class TestMessage : Message
    {
        [ProtoMember(1)]
        public string Content { get; set; } = string.Empty;
    }
}