using Synthesizer.Core.Abstractions;

namespace Synthesizer.Core.SignalProcessing.Modules;

public class ButterworthLowpass(SamplingConfiguration samplingConfiguration, SignalBus signalBus) : SynthesizerModule
{
    private const double CutoffFrequencyTolerance = 0.0001;
    
    private double _cutoffFrequency = double.MinValue, _b0, _b1, _a1, _x1, _y1;
    
    public SignalId? SignalInput { get; set; }
    public SignalId? CutoffFrequencyInput { get; set; }
    public SignalId? SignalOutput { get; set; }
    
    public override void Update(SynthesizerUpdateContext context)
    {
        var cutoffFrequency = signalBus.ReadSignalValue(CutoffFrequencyInput);
        if (CutoffFrequencyChanged(cutoffFrequency)) UpdateCoefficients(cutoffFrequency);
        
        var input = signalBus.ReadSignalValue(SignalInput);
        var output = _b0 * input + _b1 * _x1 + _a1 * _y1;
        
        signalBus.WriteSignalValue(SignalOutput, output);
        
        _x1 = input;
        _y1 = output;
    }

    private bool CutoffFrequencyChanged(double cutoffFrequency)
    {
        return Math.Abs(cutoffFrequency - _cutoffFrequency) > CutoffFrequencyTolerance;
    }

    private void UpdateCoefficients(double cutoffFrequency)
    {
        var w0 = 2 * Math.PI * cutoffFrequency;
        var T = samplingConfiguration.SamplePeriod;

        _b0 = 1 / (1 + 2 / (T * w0));
        _b1 = _b0;
        _a1 = -(T * w0 - 2) / (T * w0 + 2);

        _cutoffFrequency = cutoffFrequency;
    }
}