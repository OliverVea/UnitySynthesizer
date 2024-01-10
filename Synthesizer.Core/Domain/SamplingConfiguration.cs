namespace Synthesizer.Core.Domain;

public record SamplingConfiguration
{
    public int SampleRate { get; init; }
    public required int Channels { get; init; }
    public int Latency { get; init; } = 100;
    
    private readonly Lazy<double> _sampleDuration;
    public double SamplePeriod => _sampleDuration.Value;

    public SamplingConfiguration()
    {
        _sampleDuration = new Lazy<double>(() => 1.0 / SampleRate);
    }
}