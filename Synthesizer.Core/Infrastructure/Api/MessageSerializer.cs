using System.Xml.Serialization;

namespace Synthesizer.Core.Infrastructure.Api;

public class MessageSerializer<T> : IMessageSerializer where T : Message
{
    public MessageSerializerKey MessageSerializerKey { get; } = MessageSerializerKeyHelper.GetSerializerKey<T>();
    public XmlSerializer XmlSerializer { get; } = new(typeof(T));
}