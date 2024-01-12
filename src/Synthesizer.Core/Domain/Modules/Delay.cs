namespace Synthesizer.Core.Domain.Modules;

public class Delay(SignalBus signalBus, SamplingConfiguration configuration) : SynthesizerModule
{
    private readonly Queue<double> _queue = new();
    
    public SignalId? SignalInput { get; set; }
    public SignalId? MixInput { get; set; }
    public SignalId? FeedbackInput { get; set; }
    public SignalId? DelayTimeInput { get; set; }
    public SignalId? SignalOutput { get; set; }
    
    private double DelayTime => signalBus.ReadSignalValue(DelayTimeInput);
    
    private int DelaySamples => Math.Max((int)(DelayTime * configuration.SampleRate), 1);
    private int _delaySamples;
    
    public override void Update(SynthesizerUpdateContext context)
    {
        var signal = signalBus.ReadSignalValue(SignalInput);
        
        UpdateQueueLength(signal);
        
        var delayedSignal = _queue.Dequeue();
        
        var mix = signalBus.ReadSignalValue(MixInput);
        var output = signal + mix * delayedSignal;
        
        signalBus.WriteSignalValue(SignalOutput, output);

        var feedback = signalBus.ReadSignalValue(FeedbackInput);
        var queueSignal = (1 - feedback) * signal + feedback * output;
        
        _queue.Enqueue(queueSignal);
    }

    private void UpdateQueueLength(double signal)
    {
        var newDelaySamples = DelaySamples;
        if (newDelaySamples == _delaySamples) return;

        var delta = newDelaySamples - _queue.Count;
        if (delta > 0) for (var _ = 0; _ < delta; _++) _queue.Enqueue(signal);
        if (delta < 0) for (var _ = 0; _ > -delta; _--) _queue.Dequeue();

        _delaySamples = newDelaySamples;
    }
}