using Synthesizer.Core.Abstractions;

namespace Synthesizer.Core.Domain;

public class SignalBus
{
    private readonly HashSet<SignalId> _releasedSignalIds = new();
    private readonly SwitchArray<double> _busses = new();

    
    public SignalId CreateSignal(double defaultValue = 0)
    {
        if (_releasedSignalIds.Count != 0) return RecycleSignalId(defaultValue);
        return CreateNewSignalId(defaultValue);
    }

    private SignalId CreateNewSignalId(double defaultValue)
    {
        var signalId = new SignalId(_busses.Count);
        
        _busses.Add(defaultValue);
        
        return signalId;
    }

    private SignalId RecycleSignalId(double defaultValue)
    {
        var signalId = _releasedSignalIds.First();
        
        _busses.Set(signalId.Id, defaultValue);
        _releasedSignalIds.Remove(signalId);
        
        return signalId;
    }

    public void ReleaseSignal(SignalId signalId)
    {
        _busses.Set(signalId.Id, 0);
        _releasedSignalIds.Add(signalId);
    }

    public void SwapBusses() => _busses.Swap();
    
    public double ReadSignalValue(SignalId? signalId, double defaultValue = 0)
    {
        return signalId == null ? defaultValue : _busses.Read(signalId.Value.Id);
    }

    public void WriteSignalValue(SignalId? signalId, double value)
    {
        if (signalId != null) _busses.Write(signalId.Value.Id, value);
    }

    public void SetSignalValue(SignalId? signalId, double value)
    {
        if (signalId != null) _busses.Set(signalId.Value.Id, value);
    }
}