namespace Synthesizer.Core.Domain.Modules;

public class Delay : SynthesizerModule
{
    private readonly Queue<double> _queue;
    
    public SignalId? SignalInput { get; set; }
    public SignalId? MixInput { get; set; }
    public SignalId? FeedbackInput { get; set; }
    public SignalId? DelayTimeInput { get; set; }
    public SignalId? SignalOutput { get; set; }
    
    private double DelayTime => _signalBus.ReadSignalValue(DelayTimeInput);
    private int DelaySamples => Math.Max((int)(DelayTime * _configuration.SampleRate), 1);

    private int _delaySamples = 1;
    private readonly SignalBus _signalBus;
    private readonly SamplingConfiguration _configuration;

    public Delay(SignalBus signalBus, SamplingConfiguration configuration)
    {
        _signalBus = signalBus;
        _configuration = configuration;
        
        _queue = new Queue<double>();
        _queue.Enqueue(0);
    }

    public override void Update(SynthesizerUpdateContext context)
    {
        var signal = _signalBus.ReadSignalValue(SignalInput);
        
        UpdateQueueLength(signal);
        
        var delayedSignal = _queue.Dequeue();
        
        var mix = _signalBus.ReadSignalValue(MixInput);
        var output = signal + mix * delayedSignal;
        
        _signalBus.WriteSignalValue(SignalOutput, output);

        var feedback = _signalBus.ReadSignalValue(FeedbackInput);
        var queueSignal = (1 - feedback) * signal + feedback * output;
        
        _queue.Enqueue(queueSignal);
    }

    private void UpdateQueueLength(double signal)
    {
        var newDelaySamples = DelaySamples;
        if (newDelaySamples == _delaySamples) return;
        
        ResampleQueueWithLength(newDelaySamples);
    }
    
    private void ResampleQueueWithLength(int newDelaySamples)
    {
        // Just throw the last elements of the queue.
        while (_queue.Count > newDelaySamples) _queue.Dequeue();
        
        // Add zeroes to the queue.
        while (_queue.Count < newDelaySamples) _queue.Enqueue(0);
        
        _delaySamples = newDelaySamples;
    }
}