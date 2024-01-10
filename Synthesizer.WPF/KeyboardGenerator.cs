using System.Windows.Input;
using Synthesizer.Core.Domain;
using Synthesizer.Core.Extensions;

namespace Synthesizer.WPF;

public class KeyboardGenerator(SignalBus signalBus, MainWindow mainWindow) : SynthesizerModule
{
    private readonly Dictionary<Key, List<(SignalId signalId, double value)>> _downKeymap = new();
    private readonly Dictionary<Key, List<(SignalId signalId, double value)>> _upKeymap = new();
    private readonly List<(SignalId signalId, double value)> _pressedKeys = new();
    
    public override void Initialize()
    {
        base.Initialize();
        
        Keyboard.AddKeyDownHandler(mainWindow, OnKeyDown);
        Keyboard.AddKeyUpHandler(mainWindow, OnKeyUp);
    }
    
    public void AddKeymapEntry(Key key, SignalId signalId, double? downValue, double? upValue = null)
    {
        if (downValue != null) _downKeymap.SetDefault(key, []).Add((signalId, downValue.Value));
        if (upValue != null) _upKeymap.SetDefault(key, []).Add((signalId, upValue.Value));
    }

    private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
    {
        if (!_downKeymap.TryGetValue(keyEventArgs.Key, out var entries)) return;
        
        foreach (var entry in entries) _pressedKeys.Add(entry);
    }

    private void OnKeyUp(object sender, KeyEventArgs keyEventArgs)
    {
        if (!_upKeymap.TryGetValue(keyEventArgs.Key, out var entries)) return;
        
        foreach (var entry in entries) _pressedKeys.Add(entry);
    }

    public override void Update(SynthesizerUpdateContext context)
    {
        if (_pressedKeys.Count == 0) return;
        
        foreach (var (signalId, value) in _pressedKeys)
        {
            signalBus.SetSignalValue(signalId, value);
        }
        
        _pressedKeys.Clear();
    }
}