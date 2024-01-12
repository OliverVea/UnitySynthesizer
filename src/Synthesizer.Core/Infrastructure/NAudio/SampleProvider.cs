using NAudio.Wave;
using Synthesizer.Core.Domain;

namespace Synthesizer.Core.Infrastructure.NAudio;

public class SampleProvider(
    WaveFormat waveFormat, 
    ModularSynthesizer modularSynthesizer) : ISampleProvider
{
    public WaveFormat WaveFormat => waveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        for (var i = 0; i < count / waveFormat.Channels; i++)
        {
            var span = buffer.AsSpan(i * waveFormat.Channels + offset, waveFormat.Channels);
            modularSynthesizer.SampleOutput(span);
        }

        return count;
    }
}