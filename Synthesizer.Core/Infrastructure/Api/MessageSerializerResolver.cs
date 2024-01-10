namespace Synthesizer.Core.Infrastructure.Api;

public class MessageSerializerResolver(IEnumerable<IMessageSerializer> messageSerializers)
{
    private readonly Dictionary<MessageSerializerKey, IMessageSerializer> _messageSerializerLookup =
        messageSerializers.ToDictionary(x => x.MessageSerializerKey);
    
    public IMessageSerializer ResolveSerializer<T>() where T : Message
    {
        var serializerKey = MessageSerializerKeyHelper.GetSerializerKey<T>();
        return ResolveSerializer(serializerKey);
    }

    public IMessageSerializer ResolveSerializer(MessageSerializerKey serializerKey)
    {
        if (_messageSerializerLookup.TryGetValue(serializerKey, out var serializer)) return serializer;

        throw new ArgumentException($"Could not find serializer for serializer key {serializerKey}",
            nameof(serializerKey));
    }
}