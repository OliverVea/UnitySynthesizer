using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Synthesizer.Shared;
using Synthesizer.Shared.Extensions;
using Synthesizer.Shared.Messages;

namespace Synthesizer.WPF;

public partial class App : Application
{
    private IServiceProvider? _services;
    public IServiceProvider Services => _services ?? throw new InvalidOperationException("Services not initialized");
    
    private IConfiguration? _configuration;
    public IConfiguration Configuration => _configuration ?? throw new InvalidOperationException("Configuration not initialized");
    
    private static readonly (Key, double)[] KeyMap = [
        (Key.A, 440),
        (Key.W, 466.16),
        (Key.S, 493.88),
        (Key.E, 523.25),
        (Key.D, 554.37),
        (Key.F, 587.33),
        (Key.T, 622.25),
        (Key.G, 659.25),
        (Key.Y, 698.46),
        (Key.H, 739.99),
        (Key.U, 783.99),
        (Key.J, 830.61),
        (Key.K, 880),
        (Key.O, 932.33),
        (Key.L, 987.77),
        (Key.P, 1046.5),
    ];
    
    protected override void OnStartup(StartupEventArgs startupEventArgs)
    {
        base.OnStartup(startupEventArgs);
        
        _services = new ServiceCollection()
            .AddLogging(c => c.AddConsole())
            .AddSingleton<MainWindow>()
            .AddSingleton<KeyboardHandler>()
            .AddClientServices()
            .BuildServiceProvider();
        
        var service = _services.GetRequiredService<IMessageSender>();
        var connectionManager = _services.GetRequiredService<IConnectionManager>();
        var logger = _services.GetRequiredService<ILogger<App>>();
        var window = _services.GetRequiredService<MainWindow>();

        connectionManager.ConnectAsync(CancellationToken.None).Wait();
        
        logger.LogInformation("Connected to server");
        
        var keyboardHandler = _services.GetRequiredService<KeyboardHandler>();
        keyboardHandler.Initialize();

        keyboardHandler.AddDown(Key.Q, () =>
        {
            service.Send(new QuitMessage());
            window.Close();
        });
        
        foreach (var (key, frequency) in KeyMap)
        {
            keyboardHandler.AddDown(key, () =>
            {
                service.Send(new SetSignalValueMessage
                {
                    SignalId = 0,
                    Value = frequency
                });
            });
            
            keyboardHandler.AddDown(key, () =>
            {
                service.Send(new SetSignalValueMessage
                {
                    SignalId = 1,
                    Value = 1
                });
            });
            
            keyboardHandler.AddUp(key, () =>
            {
                service.Send(new SetSignalValueMessage
                {
                    SignalId = 1,
                    Value = 0
                });
            });
        }

        window.Show();
    }
}