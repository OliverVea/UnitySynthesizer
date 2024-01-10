﻿using System.IO.Pipes;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Synthesizer.Core.Domain;
using Synthesizer.Core.Domain.Modules;
using Synthesizer.Core.Extensions;
using Synthesizer.Core.Infrastructure.Api;
using Synthesizer.Core.Infrastructure.NAudio;

namespace Synthesizer.WPF;

public partial class App : Application
{
    private IServiceProvider? _services;
    public IServiceProvider Services => _services ?? throw new InvalidOperationException("Services not initialized");
    
    private IConfiguration? _configuration;
    public IConfiguration Configuration => _configuration ?? throw new InvalidOperationException("Configuration not initialized");
    
    protected override void OnStartup(StartupEventArgs startupEventArgs)
    {
        base.OnStartup(startupEventArgs);
        
        /*
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        var samplingConfiguration = Configuration.GetSamplingConfiguration();
        */
        
        _services = new ServiceCollection()
            .AddSingleton<MainWindow>()
            .AddSynthesizerClientServices()
            .BuildServiceProvider();
        
        var message = new EchoMessage
        {
            Message = "Hello, World!"
        };

        var namedPipeClientStream = _services.GetRequiredService<NamedPipeClientStream>();
        namedPipeClientStream.Connect();
        
        var service = _services.GetRequiredService<MessageService>();
        service.WriteMessageAsync(message, CancellationToken.None).Wait();

        var window = _services.GetRequiredService<MainWindow>();
        window.Show();
    }

    private static void BuildSynthesizer(IServiceProvider services)
    {
        var signalBus = services.GetRequiredService<SignalBus>();
        var synthesizer = services.GetRequiredService<ModularSynthesizer>();
        
        var synthesizerOutputSignal = signalBus.CreateSignal();
        var keyboardFrequency = signalBus.CreateSignal();
        var keyboardGate = signalBus.CreateSignal();
        
        synthesizer.SetChannelOutputSignal(0, synthesizerOutputSignal);
        
        var keyboardGenerator = synthesizer.CreateModule<KeyboardGenerator>();
        keyboardGenerator.AddKeymapEntry(Key.A, keyboardFrequency, 440);
        keyboardGenerator.AddKeymapEntry(Key.A, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.W, keyboardFrequency, 466.16);
        keyboardGenerator.AddKeymapEntry(Key.W, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.S, keyboardFrequency, 493.88);
        keyboardGenerator.AddKeymapEntry(Key.S, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.E, keyboardFrequency, 523.25);
        keyboardGenerator.AddKeymapEntry(Key.E, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.D, keyboardFrequency, 554.37);
        keyboardGenerator.AddKeymapEntry(Key.D, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.F, keyboardFrequency, 587.33);
        keyboardGenerator.AddKeymapEntry(Key.F, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.T, keyboardFrequency, 622.25);
        keyboardGenerator.AddKeymapEntry(Key.T, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.G, keyboardFrequency, 659.25);
        keyboardGenerator.AddKeymapEntry(Key.G, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.Y, keyboardFrequency, 698.46);
        keyboardGenerator.AddKeymapEntry(Key.Y, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.H, keyboardFrequency, 739.99);
        keyboardGenerator.AddKeymapEntry(Key.H, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.U, keyboardFrequency, 783.99);
        keyboardGenerator.AddKeymapEntry(Key.U, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.J, keyboardFrequency, 830.61);
        keyboardGenerator.AddKeymapEntry(Key.J, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.K, keyboardFrequency, 880);
        keyboardGenerator.AddKeymapEntry(Key.K, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.O, keyboardFrequency, 932.33);
        keyboardGenerator.AddKeymapEntry(Key.O, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.L, keyboardFrequency, 987.77);
        keyboardGenerator.AddKeymapEntry(Key.L, keyboardGate, 1, 0);
        keyboardGenerator.AddKeymapEntry(Key.P, keyboardFrequency, 1046.5);
        keyboardGenerator.AddKeymapEntry(Key.P, keyboardGate, 1, 0);
        
        var oscillator = synthesizer.CreateModule<Oscillator>();
        oscillator.Waveform = Waveform.Sawtooth;
        oscillator.FrequencyInput = keyboardFrequency;
        oscillator.AmplitudeInput = signalBus.CreateSignal(0.5);
        oscillator.SignalOutput = signalBus.CreateSignal();
        
        var envelopeGenerator = synthesizer.CreateModule<EnvelopeGenerator>();
        envelopeGenerator.GateInput = keyboardGate;
        envelopeGenerator.SignalInput = oscillator.SignalOutput;
        envelopeGenerator.SignalOutput = signalBus.CreateSignal();
        
        var lowPassFilter = synthesizer.CreateModule<ButterworthLowpass>();
        lowPassFilter.CutoffFrequencyInput = signalBus.CreateSignal(3000);
        lowPassFilter.SignalInput = envelopeGenerator.SignalOutput;
        lowPassFilter.SignalOutput = signalBus.CreateSignal();

        var delayTimeOscillator = synthesizer.CreateModule<Oscillator>();
        delayTimeOscillator.AmplitudeInput = signalBus.CreateSignal(0.4);
        delayTimeOscillator.OffsetInput = signalBus.CreateSignal(0.5);
        delayTimeOscillator.FrequencyInput = signalBus.CreateSignal(333);
        delayTimeOscillator.SignalOutput = signalBus.CreateSignal();

        var delay = synthesizer.CreateModule<Delay>();
        delay.SignalInput = lowPassFilter.SignalOutput;
        delay.DelayTimeInput = delayTimeOscillator.SignalOutput;
        delay.MixInput = signalBus.CreateSignal(0.5);
        delay.FeedbackInput = signalBus.CreateSignal(1);
        delay.SignalOutput = synthesizerOutputSignal;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        
        var playbackController = Services.GetRequiredService<PlaybackController>();
        
        playbackController.Stop();
    }
}