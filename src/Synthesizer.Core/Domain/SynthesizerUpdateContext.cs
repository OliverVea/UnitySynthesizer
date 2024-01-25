namespace Synthesizer.Core.Domain;

public readonly struct SynthesizerUpdateContext
{
    public required double DeltaTime { get; init; }
}