using ProtoBuf;
using Synthesizer.Shared;

namespace Demo.Messages
{
    [ProtoContract]
    public class SetOscillatorWaveform : Message
    {
        [ProtoMember(1)]
        public Waveform Waveform { get; set; }
    }
}