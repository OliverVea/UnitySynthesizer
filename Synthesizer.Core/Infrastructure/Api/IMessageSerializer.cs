using System.Xml.Serialization;

namespace Synthesizer.Core.Infrastructure.Api;

public interface IMessageSerializer
{
    MessageSerializerKey MessageSerializerKey { get; }
    XmlSerializer XmlSerializer { get; }
}