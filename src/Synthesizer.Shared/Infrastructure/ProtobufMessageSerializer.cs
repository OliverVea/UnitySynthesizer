using System;
using Synthesizer.Shared.Domain;

namespace Synthesizer.Shared.Infrastructure
{
    internal class ProtobufMessageSerializer : IMessageSerializer
    {
        public ReadOnlyMemory<byte> Serialize<T>(T message) where T : Message
        {
            using var stream = new System.IO.MemoryStream();
            ProtoBuf.Serializer.Serialize(stream, message);
            var data = stream.ToArray();
            
            var internalMessage = new InternalMessage
            {
                Type = typeof(T),
                Data = data
            };
            using var internalStream = new System.IO.MemoryStream();
            ProtoBuf.Serializer.Serialize(internalStream, internalMessage);
            return internalStream.ToArray();
        }

        public Message Deserialize(ReadOnlyMemory<byte> serializedMessage)
        {
            var internalMessage = ProtoBuf.Serializer.Deserialize<InternalMessage>(serializedMessage);
            using var stream = new System.IO.MemoryStream(internalMessage.Data);
            var message = (Message) ProtoBuf.Serializer.Deserialize(internalMessage.Type, stream);
            return message;
        }

        [ProtoBuf.ProtoContract]
        private struct InternalMessage
        {
            [ProtoBuf.ProtoMember(1)]
            private string _typeName;
            
            public Type Type 
            {
                get => Type.GetType(_typeName) ?? throw new InvalidOperationException();
                set => _typeName = value.AssemblyQualifiedName ?? throw new InvalidOperationException();
            }
            
            [ProtoBuf.ProtoMember(2)]
            public byte[] Data { get; set; }
        }
    }
}