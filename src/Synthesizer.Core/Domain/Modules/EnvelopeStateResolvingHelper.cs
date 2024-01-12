namespace Synthesizer.Core.Domain.Modules;

public static class EnvelopeStateResolvingHelper
{
    public static EnvelopeState ResolveState(EnvelopeState currentState, bool gate, double stateTime)
    {
        return currentState switch
        {
            EnvelopeState.None => ResolveForNone(gate),
            EnvelopeState.Attack => ResolveForAttack(gate, stateTime),
            EnvelopeState.Decay => ResolveForDecay(gate, stateTime),
            EnvelopeState.Sustain => ResolveForSustain(gate),
            EnvelopeState.Release => ResolveForRelease(gate, stateTime),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static EnvelopeState ResolveForNone(bool gate)
    {
        return gate ? EnvelopeState.Attack : EnvelopeState.None;
    }
    
    private static EnvelopeState ResolveForAttack(bool gate, double stateTime)
    {
        if (!gate) return EnvelopeState.Release;
        return stateTime > EnvelopeGenerator.AttackTime ? EnvelopeState.Decay : EnvelopeState.Attack;
    }
    
    private static EnvelopeState ResolveForDecay(bool gate, double stateTime)
    {
        if (!gate) return EnvelopeState.Release;
        if (stateTime > EnvelopeGenerator.DecayTime) return EnvelopeState.Sustain;
        return EnvelopeState.Decay;
    }
    
    private static EnvelopeState ResolveForSustain(bool gate)
    {
        if (!gate) return EnvelopeState.Release;
        return EnvelopeState.Sustain;
    }
    
    private static EnvelopeState ResolveForRelease(bool gate, double stateTime)
    {
        if (gate) return EnvelopeState.Attack;
        if (stateTime > EnvelopeGenerator.ReleaseTime) return EnvelopeState.None;
        return EnvelopeState.Release;
    }
}