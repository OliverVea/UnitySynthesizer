using System;

namespace Synthesizer.Shared.Infrastructure
{
    internal interface IMessageSerializer
    {
        ReadOnlyMemory<byte> Serialize<T>(T message) where T : Message;
        Message Deserialize(ReadOnlyMemory<byte> serializedMessage);
    }
}