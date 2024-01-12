using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Synthesizer.Shared.Extensions;

namespace Synthesizer.WPF;

public class KeyboardHandler(ILogger<KeyboardHandler> logger, MainWindow mainWindow)
{
    private readonly Dictionary<Key, List<Action>> _downKeymap = new();
    private readonly Dictionary<Key, List<Func<Task>>> _asyncDownKeymap = new();
    
    private readonly Dictionary<Key, List<Action>> _upKeymap = new();
    private readonly Dictionary<Key, List<Func<Task>>> _asyncUpKeymap = new();
    
    
    public void Initialize()
    {
        Keyboard.AddKeyDownHandler(mainWindow, OnKeyDown);
        Keyboard.AddKeyUpHandler(mainWindow, OnKeyUp);
    }
    
    public void AddDown(Key key, Action downAction) => _downKeymap.SetDefault(key, []).Add(downAction);
    public void AddDown(Key key, Func<Task> downAction) => _asyncDownKeymap.SetDefault(key, []).Add(downAction);
    public void AddUp(Key key, Action upAction) => _upKeymap.SetDefault(key, []).Add(upAction);
    public void AddUp(Key key, Func<Task> upAction) => _asyncUpKeymap.SetDefault(key, []).Add(upAction);

    private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
    {
        if (_downKeymap.TryGetValue(keyEventArgs.Key, out var actions))
            foreach (var action in actions)
                action();
        
        if (_asyncDownKeymap.TryGetValue(keyEventArgs.Key, out var asyncActions))
            foreach (var asyncAction in asyncActions)
                asyncAction().Wait();
    }

    private void OnKeyUp(object sender, KeyEventArgs keyEventArgs)
    {
        if (_upKeymap.TryGetValue(keyEventArgs.Key, out var actions))
            foreach (var action in actions)
                action();
        
        if (_asyncUpKeymap.TryGetValue(keyEventArgs.Key, out var asyncActions))
            foreach (var asyncAction in asyncActions)
                asyncAction().Wait();
        
        logger.LogInformation($"Key up: {keyEventArgs.Key}");
    }
}