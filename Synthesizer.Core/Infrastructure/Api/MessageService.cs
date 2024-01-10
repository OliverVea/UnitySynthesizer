using System.Text;
using System.Xml;
using Synthesizer.Core.Extensions;

namespace Synthesizer.Core.Infrastructure.Api;

public class MessageService(
    IMessageSender messageSender,
    MessageSerializerResolver messageSerializerResolver)
{
    private const char KeyDelimiter = '\0';
    
    public async Task WriteMessageAsync<T>(T message, CancellationToken cancellationToken) where T : Message
    {
        var messageSerializer = messageSerializerResolver.ResolveSerializer<T>();

        var messageStream = new MemoryStream();
        messageSerializer.XmlSerializer.Serialize(messageStream, message);

        var sb = new StringBuilder();
        sb.Append(messageSerializer.MessageSerializerKey.Key);
        sb.Append(KeyDelimiter);

        var xmlWriter = XmlWriter.Create(sb);
        messageSerializer.XmlSerializer.Serialize(xmlWriter, message);

        var messageString = sb.ToString();
        
        Console.WriteLine($"> {messageString}");

        await messageSender.SendAsync(messageString, cancellationToken);
    }

    public Task<Message> ReadMessageAsync(byte[] fullMessageBytes, CancellationToken cancellationToken)
    {
        byte[] serializerKeyBytes = Array.Empty<byte>();
        byte[] messageBytes = Array.Empty<byte>();
        
        for (var i = 0; i < fullMessageBytes.Length; i++)
        {
            if (fullMessageBytes[i] != KeyDelimiter) continue;

            serializerKeyBytes = fullMessageBytes[..i];
            messageBytes = fullMessageBytes[(i + 2)..];

            break;
        }

        var serializerKey = new MessageSerializerKey(serializerKeyBytes.AsString());
        var messageString = messageBytes.AsString();
        
        var serializer = messageSerializerResolver.ResolveSerializer(serializerKey);

        var xtr = XmlTextReader.Create()
            XmlString
        var w = XmlReader.Create(messageString);
        
        var message = serializer.XmlSerializer.Deserialize(w);

        if (message is not Message validatedMessage) throw new Exception();

        Console.WriteLine($"< {messageBytes.AsString()}");

        return Task.FromResult(validatedMessage);
    }
}