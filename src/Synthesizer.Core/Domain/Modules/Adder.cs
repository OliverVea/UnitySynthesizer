namespace Synthesizer.Core.Domain.Modules;

public class Adder(SignalBus signalBus) : SynthesizerModule
{
    public List<SignalId?> SignalInputs { get; } = [];
    public SignalId? GainInput { get; set; }
    public SignalId? SignalOutput { get; set; }
    
    public override void Update(SynthesizerUpdateContext context)
    {
        if (SignalOutput is not {} outputSignal) return;
        
        var gain = signalBus.ReadSignalValue(GainInput);
        var inputSum = SignalInputs.Select(s => signalBus.ReadSignalValue(s)).Sum();
        var output = inputSum * gain;
        
        signalBus.WriteSignalValue(outputSignal, output);
    }
}