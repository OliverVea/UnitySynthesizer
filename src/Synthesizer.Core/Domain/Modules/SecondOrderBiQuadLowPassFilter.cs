namespace Synthesizer.Core.Domain.Modules;

public class SecondOrderBiQuadLowPassFilter(SignalBus signalBus, SamplingConfiguration samplingConfiguration) : IIR(Order, signalBus)
{
    private const int Order = 2;
    private const double Tolerance = 0.01;
    private const double QMin = 0.1;
    private const double QMax = 20;

    private readonly SignalBus _signalBus = signalBus;
    
    public SignalId? CutoffFrequencyInput { get; set; }
    public SignalId? ResonanceInput { get; set; }
    
    private double _resonance, _cutoffFrequency;
    
    protected override bool ShouldUpdateCoefficients()
    {
        var shouldUpdate = false;
        
        var resonance = _signalBus.ReadSignalValue(ResonanceInput);
        shouldUpdate |= Math.Abs(resonance - _resonance) > Tolerance;
        _resonance = resonance;
        
        var cutoffFrequency = _signalBus.ReadSignalValue(CutoffFrequencyInput);
        shouldUpdate |= Math.Abs(cutoffFrequency - _cutoffFrequency) > Tolerance;
        _cutoffFrequency = cutoffFrequency;

        return shouldUpdate;
    }

    protected override void UpdateCoefficients(double[] a, double[] b)
    {
        var q = _resonance * (QMax - QMin) + QMin;
        q = Math.Clamp(q, QMin, QMax);
        
        var oc = 2 * Math.PI * _cutoffFrequency / samplingConfiguration.SampleRate;
        var k = Math.Tan(oc / 2);
        var w = k * k;
        var alpha = 1 + k / q + w;

        a[0] = 1;
        a[1] = 2 * (w - 1) / alpha;
        a[2] = (1 - k / q + w) / alpha;
        b[0] = w / alpha;
        b[1] = 2 * w / alpha;
        b[2] = b[0];
    }
}