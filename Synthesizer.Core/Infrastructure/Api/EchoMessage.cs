using System.Xml.Serialization;

namespace Synthesizer.Core.Infrastructure.Api;

[Serializable]
public class EchoMessage : Message
{
    [XmlElement]
    public required string Message { get; init; }
}