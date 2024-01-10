namespace Synthesizer.Core.Domain.Modules;

public class ImpulseGenerator(SignalBus signalBus) : SynthesizerModule
{
    public SignalId? SignalInput { get; set; }
    public SignalId? SignalOutput { get; set; }

    private bool _previousWasNonZero;
    
    public override void Update(SynthesizerUpdateContext context)
    {
        if (_previousWasNonZero)
        {
            signalBus.WriteSignalValue(SignalOutput, 0);
            _previousWasNonZero = false;
        }

        var input = signalBus.ReadSignalValue(SignalInput);
        var output = Math.Round(Math.Clamp(input, 0, 1));
        
        signalBus.WriteSignalValue(SignalOutput, output);
        _previousWasNonZero = output > 0;
    }
}