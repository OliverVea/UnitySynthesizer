using System.Collections.Generic;
using System.Linq;
using Demo.Messages;
using Synthesizer.Shared;

namespace Demo.Gui
{
    public class VoiceManager : Singleton<VoiceManager>
    {
        protected override VoiceManager GetInstance() => this;

        private readonly Voice[] _voices =
        {
            new(Signal.Voice1Frequency, Signal.Voice1Gate),
            new(Signal.Voice2Frequency, Signal.Voice2Gate),
            new(Signal.Voice3Frequency, Signal.Voice3Gate),
        };

        private readonly HashSet<int> _availableVoices = new() { 0, 1, 2 };
        
        private static IMessageSender MessageSender => ServiceLocator.GetRequiredService<IMessageSender>();


        public static int? PlayVoice(double frequency) => Instance.PlayVoiceInternal(frequency);
        private int? PlayVoiceInternal(double frequency)
        {
            if (_availableVoices.Count == 0) return null;

            var voiceIndex = _availableVoices.First();
            _availableVoices.Remove(voiceIndex);

            var voice = _voices[voiceIndex];
            
            MessageSender.Send(new SetSignalValueMessage
            {
                Signal = voice.FrequencySignal,
                Value = frequency
            });
            
            MessageSender.Send(new SetSignalValueMessage
            {
                Signal = voice.GateSignal,
                Value = 1
            });

            return voiceIndex;
        }

        public static void StopVoice(int voiceIndex) => Instance.StopVoiceInternal(voiceIndex);

        public void StopVoiceInternal(int voiceIndex)
        {
            if (!_availableVoices.Add(voiceIndex)) return;
            
            var voice = _voices[voiceIndex];
            
            MessageSender.Send(new SetSignalValueMessage
            {
                Signal = voice.GateSignal,
                Value = 0
            });
        }

        private class Voice
        {
            public Signal FrequencySignal { get; }
            public Signal GateSignal { get; }

            public Voice(Signal frequencySignal, Signal gateSignal)
            {
                FrequencySignal = frequencySignal;
                GateSignal = gateSignal;
            }
        }
    }
}