namespace Synthesizer.Core.Domain.Modules;

[Serializable]
public class Oscillator(SignalBus signalBus) : SynthesizerModule
{
    public Waveform Waveform { get; set; }
    public SignalId? FrequencyInput { get; set; }
    public SignalId? AmplitudeInput { get; set; }
    public SignalId? OffsetInput { get; set; }
    public SignalId? SignalOutput { get; set; }

    [NonSerialized]
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
            Waveform.Sine => Math.Sin(_normalizedPhase * 2 * Math.PI),
            Waveform.Square => Math.Round(_normalizedPhase) * 2 - 1,
            Waveform.Sawtooth => _normalizedPhase * 2 - 1,
            Waveform.Triangle => Math.Abs(_normalizedPhase * 4 - 2) - 1,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}