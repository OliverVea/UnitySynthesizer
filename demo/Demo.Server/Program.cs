using Demo.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Synthesizer.Core.Domain;
using Synthesizer.Core.Domain.Modules;
using Synthesizer.Core.Extensions;
using Synthesizer.Core.Infrastructure.NAudio;
using Synthesizer.Shared;
using Synthesizer.Shared.Extensions;
using MessagesWaveform = Demo.Messages.Waveform;
using DomainWaveform = Synthesizer.Core.Domain.Waveform;

var samplingConfiguration = new SamplingConfiguration
{
    Channels = 1,
    Latency = 50,
    SampleRate = 44100
};

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var serviceCollection = new ServiceCollection()
    .AddLogging(c =>
    {
        c.ClearProviders();
        c.AddConsole();
        c.AddConfiguration(configuration);
    })
    .AddSynthesizerServerServices(samplingConfiguration);

var serviceProvider = serviceCollection.BuildServiceProvider();

var signalBus = serviceProvider.GetRequiredService<SignalBus>();
var playbackController = serviceProvider.GetRequiredService<PlaybackController>();
var connectionManager = serviceProvider.GetRequiredService<IConnectionManager>();
var messageReadingJob = serviceProvider.GetRequiredService<IMessageReadingJob>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var synthesizer = serviceProvider.GetRequiredService<ModularSynthesizer>();
    
var voice1Frequency = signalBus.CreateSignal();
var voice2Frequency = signalBus.CreateSignal();
var voice3Frequency = signalBus.CreateSignal();
var voice1Gate = signalBus.CreateSignal();
var voice2Gate = signalBus.CreateSignal();
var voice3Gate = signalBus.CreateSignal();
var synthesizerOutputSignal = signalBus.CreateSignal();
    
synthesizer.SetChannelOutputSignal(0, synthesizerOutputSignal);
    
var voice1Oscillator = synthesizer.CreateModule<Oscillator>();
voice1Oscillator.Waveform = Synthesizer.Core.Domain.Waveform.Sine;
voice1Oscillator.FrequencyInput = voice1Frequency;
voice1Oscillator.AmplitudeInput = signalBus.CreateSignal(0.5);
voice1Oscillator.SignalOutput = signalBus.CreateSignal();
    
var voice2Oscillator = synthesizer.CreateModule<Oscillator>();
voice2Oscillator.Waveform = Synthesizer.Core.Domain.Waveform.Sine;
voice2Oscillator.FrequencyInput = voice2Frequency;
voice2Oscillator.AmplitudeInput = signalBus.CreateSignal(0.5);
voice2Oscillator.SignalOutput = signalBus.CreateSignal();
    
var voice3Oscillator = synthesizer.CreateModule<Oscillator>();
voice3Oscillator.Waveform = Synthesizer.Core.Domain.Waveform.Sine;
voice3Oscillator.FrequencyInput = voice3Frequency;
voice3Oscillator.AmplitudeInput = signalBus.CreateSignal(0.5);
voice3Oscillator.SignalOutput = signalBus.CreateSignal();
    
Oscillator[] oscillators = [voice1Oscillator, voice2Oscillator, voice3Oscillator];
    
var voice1Envelope = synthesizer.CreateModule<EnvelopeGenerator>();
voice1Envelope.GateInput = voice1Gate;
voice1Envelope.SignalInput = voice1Oscillator.SignalOutput;
voice1Envelope.SignalOutput = signalBus.CreateSignal();
    
var voice2Envelope = synthesizer.CreateModule<EnvelopeGenerator>();
voice2Envelope.GateInput = voice2Gate;
voice2Envelope.SignalInput = voice2Oscillator.SignalOutput;
voice2Envelope.SignalOutput = signalBus.CreateSignal();
    
var voice3Envelope = synthesizer.CreateModule<EnvelopeGenerator>();
voice3Envelope.GateInput = voice3Gate;
voice3Envelope.SignalInput = voice3Oscillator.SignalOutput;
voice3Envelope.SignalOutput = signalBus.CreateSignal();

var adder = synthesizer.CreateModule<Adder>();
adder.GainInput = signalBus.CreateSignal(1);
adder.SignalInputs.Add(voice1Envelope.SignalOutput);
adder.SignalInputs.Add(voice2Envelope.SignalOutput);
adder.SignalInputs.Add(voice3Envelope.SignalOutput);
adder.SignalOutput = signalBus.CreateSignal();
    
var lowPassFilter = synthesizer.CreateModule<ButterworthLowpass>();
lowPassFilter.CutoffFrequencyInput = signalBus.CreateSignal(1500);
lowPassFilter.SignalInput = adder.SignalOutput;
lowPassFilter.SignalOutput = signalBus.CreateSignal();

var delay = synthesizer.CreateModule<Delay>();
delay.SignalInput = lowPassFilter.SignalOutput;
delay.DelayTimeInput = signalBus.CreateSignal(0.3);
delay.MixInput = signalBus.CreateSignal(0.5);
delay.FeedbackInput = signalBus.CreateSignal(0.999);
delay.SignalOutput = synthesizerOutputSignal;

var cancellationTokenSource = new CancellationTokenSource();

serviceProvider.RegisterMessageHandler<SetSignalValueMessage>(m =>
{
    logger.LogInformation("Received set signal value message {Signal} {Value}", m.Signal, m.Value);
    var signalId = m.Signal switch
    {
        Signal.Voice1Frequency => voice1Frequency,
        Signal.Voice2Frequency => voice2Frequency,
        Signal.Voice3Frequency => voice3Frequency,
        Signal.Voice1Gate => voice1Gate,
        Signal.Voice2Gate => voice2Gate,
        Signal.Voice3Gate => voice3Gate,
        Signal.DelayTime => delay.DelayTimeInput,
        Signal.DelayMix => delay.MixInput,
        Signal.DelayFeedback => delay.FeedbackInput,
        Signal.LowPassCutoff => lowPassFilter.CutoffFrequencyInput,
        Signal.LowPassResonance => null,
        Signal.Gain => adder.GainInput,
        _ => throw new ArgumentOutOfRangeException(nameof(m.Signal), m.Signal, "Received invalid message signal")
    };
    signalBus.SetSignalValue(signalId, m.Value);
});

serviceProvider.RegisterMessageHandler<SetOscillatorWaveform>(m =>
{
    logger.LogInformation("Received set oscillator waveform message {Waveform}", m.Waveform);
    foreach (var oscillator in oscillators)
    {
        oscillator.Waveform = m.Waveform switch
        {
            MessagesWaveform.Sine => DomainWaveform.Sine,
            MessagesWaveform.Square => DomainWaveform.Square,
            MessagesWaveform.Triangle => DomainWaveform.Triangle,
            MessagesWaveform.Sawtooth => DomainWaveform.Sawtooth,
            _ => throw new ArgumentOutOfRangeException(nameof(m.Waveform), m.Waveform, "Received invalid message waveform")
        };
    }
});

while (!cancellationTokenSource.IsCancellationRequested)
{
    logger.LogInformation("Waiting for clients to connect");

    await connectionManager.ConnectAsync(cancellationTokenSource.Token);

    logger.LogInformation("Client connected to server");

    playbackController.Start();

    try
    {
        await messageReadingJob.RunAsync(cancellationTokenSource.Token);
    }
    catch (Exception e)
    {
        logger.LogError(e, "Received error while waiting for messages from client");
    }

    playbackController.Stop();

    await connectionManager.DisconnectAsync();
}
