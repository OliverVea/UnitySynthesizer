namespace Synthesizer.Core.Infrastructure.Api;

public static class MessageSerializerKeyHelper
{
    public static MessageSerializerKey GetSerializerKey<T>() where T : Message
    {
        var key = typeof(T).AssemblyQualifiedName ?? throw new ArgumentException("Could not get valid AssemblyQualifiedName from type T", nameof(T));
        
        return new MessageSerializerKey(key);
    }
}