namespace Synthesizer.Core.Domain;

public abstract class SynthesizerModule
{
    public virtual void Initialize() { }
    public abstract void Update(SynthesizerUpdateContext context);
}