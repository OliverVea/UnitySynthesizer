namespace Synthesizer.Core.Domain.Modules;

public class ImpulseGenerator(SignalBus signalBus) : SynthesizerModule
{
    public SignalId? SignalInput { get; set; }
    public SignalId? SignalOutput { get; set; }

    private double _previousInput;
    
    public override void Update(SynthesizerUpdateContext context)
    {
        var input = signalBus.ReadSignalValue(SignalInput);
        
        if (input > 0 && _previousInput <= 0)
        {
            signalBus.SetSignalValue(SignalOutput, 1);
        }
        else
        {
            signalBus.SetSignalValue(SignalOutput, 0);
        }

        _previousInput = input;
    }
}