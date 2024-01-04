using NAudio.Wave;
using Synthesizer.Core.Abstractions;

namespace Synthesizer.Core;

public class PlaybackController(SamplingConfiguration configuration, ISampleProvider sampleProvider)
{
    private DirectSoundOut? _outputDevice;

    public void Start()
    {
        if (_outputDevice != null) return;
        
        var device = DirectSoundOut.DSDEVID_DefaultPlayback;
        _outputDevice = new DirectSoundOut(device, configuration.Latency);
        _outputDevice.Init(sampleProvider);
        _outputDevice.Play();
    }

    public void Stop()
    {
        if (_outputDevice == null) return;
        _outputDevice.Stop();
        _outputDevice.Dispose();
        _outputDevice = null;
    }
}