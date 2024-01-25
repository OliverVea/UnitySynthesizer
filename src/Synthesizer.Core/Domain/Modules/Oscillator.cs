namespace Synthesizer.Core.Domain.Modules;

[Serializable]
public class Oscillator(SignalBus signalBus) : SynthesizerModule
{
    public Waveform Waveform { get; set; }
    public SignalId? FrequencyInput { get; set; }
    public SignalId? AmplitudeInput { get; set; }
    public SignalId? OffsetInput { get; set; }
    public SignalId? SignalOutput { get; set; }

    private double _normalizedPhase;
    
    private double Frequency => signalBus.ReadSignalValue(FrequencyInput);
    private double Amplitude => signalBus.ReadSignalValue(AmplitudeInput);
    private double Offset => signalBus.ReadSignalValue(OffsetInput);
    
    public override void Update(SynthesizerUpdateContext context)
    {
        var sample = Sample();

        signalBus.WriteSignalValue(SignalOutput, sample);
        
        _normalizedPhase = (context.DeltaTime * Frequency + _normalizedPhase) % 1;
    }

    private double Sample()
    {
        var sample = SampleFunction();
        return sample * Amplitude + Offset;
    }

    private double SampleFunction()
    {
        return Waveform switch
        {
            Waveform.None => 0,
            Waveform.Sine => WaveformHelper.SampleSine(_normalizedPhase),
            Waveform.Square => WaveformHelper.SampleSquare(_normalizedPhase),
            Waveform.Sawtooth => WaveformHelper.SampleSawtooth(_normalizedPhase),
            Waveform.Triangle => WaveformHelper.SampleTriangle(_normalizedPhase),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}