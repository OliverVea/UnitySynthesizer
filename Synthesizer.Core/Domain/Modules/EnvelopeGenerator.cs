namespace Synthesizer.Core.Domain.Modules;

public class EnvelopeGenerator(SignalBus signalBus) : SynthesizerModule
{
    public const double AttackTime = 0.01;
    public const double DecayTime = 0.1;
    public const double SustainLevel = 0.8;
    public const double ReleaseTime = 0.5;
    
    public SignalId? GateInput { get; set; }
    public SignalId? SignalInput { get; set; }
    public SignalId? SignalOutput { get; set; }

    private EnvelopeState _envelopeState = EnvelopeState.None;
    private double _stateTime;
    private double _lastEnvelopeValue;
    private double _stateEnteredEnvelopeValue;
    
    public override void Update(SynthesizerUpdateContext context)
    {
        UpdateGate(context);

        var input = signalBus.ReadSignalValue(SignalInput);
        var envelope = GetEnvelopeValue();
        var output = input * envelope;
        
        signalBus.WriteSignalValue(SignalOutput, output);
        
        _lastEnvelopeValue = envelope;
    }

    private void UpdateGate(SynthesizerUpdateContext context)
    {
        _stateTime += context.DeltaTime;
        
        var gate = signalBus.ReadSignalValue(GateInput) > 0;

        SetState(EnvelopeStateResolvingHelper.ResolveState(_envelopeState, gate, _stateTime));
    }

    private void SetState(EnvelopeState state)
    {
        if (_envelopeState == state) return;
        
        _envelopeState = state;
        _stateTime = 0;
        
        _stateEnteredEnvelopeValue = _lastEnvelopeValue;
    }

    private double GetEnvelopeValue()
    {
        return _envelopeState switch
        {
            EnvelopeState.None => 0,
            EnvelopeState.Attack => AttackValue(),
            EnvelopeState.Decay => DecayValue(),
            EnvelopeState.Sustain => SustainLevel,
            EnvelopeState.Release => ReleaseValue(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private double AttackValue()
    {
        var progress = 1 - (AttackTime - _stateTime) / AttackTime;
        return _lastEnvelopeValue + progress * (1 - _lastEnvelopeValue);
    }

    private double DecayValue()
    {
        var progress = 1 - (DecayTime - _stateTime) / DecayTime;
        return 1 - progress * (1 - SustainLevel);
    }

    private double ReleaseValue()
    {
        var progress = 1 - (ReleaseTime - _stateTime) / ReleaseTime;
        return _stateEnteredEnvelopeValue - progress * _stateEnteredEnvelopeValue;
    }


}