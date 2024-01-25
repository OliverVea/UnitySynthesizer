using ProtoBuf;
using Synthesizer.Shared;

namespace Demo.Messages
{
    [ProtoContract]
    public class SetSignalValueMessage : Message
    {
        [ProtoMember(1)]
        public Signal Signal { get; set; }
        
        [ProtoMember(2)]
        public double Value { get; set; }
    }
}