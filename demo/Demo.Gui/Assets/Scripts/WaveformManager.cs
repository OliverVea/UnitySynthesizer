#nullable enable

using Demo.Messages;
using Synthesizer.Shared;

namespace Demo.Gui
{
    public class WaveformManager : Singleton<WaveformManager>
    {
        private static IMessageSender MessageSender => ServiceLocator.GetRequiredService<IMessageSender>();
    
        private Waveform _waveform = Waveform.Sine;
    
        private void Start() => UpdateWaveform();
    
        public void SetWaveform(int waveform)
        {
            _waveform = (Waveform)waveform;
            UpdateWaveform();
        }

        private void UpdateWaveform()
        {
            MessageSender.Send(new SetOscillatorWaveform
            {
                Waveform = _waveform
            });
        }

        protected override WaveformManager GetInstance() => this;
    }
}
