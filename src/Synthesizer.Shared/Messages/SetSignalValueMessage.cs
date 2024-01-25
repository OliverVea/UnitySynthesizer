using ProtoBuf;
using Synthesizer.Shared;

namespace Synthesizer.Messages
{
    [ProtoContract]
    public class SetSignalValueMessage : Message
    {
        [ProtoMember(1)]
        public int SignalId { get; set; }
        
        [ProtoMember(2)]
        public double Value { get; set; }
    }
}