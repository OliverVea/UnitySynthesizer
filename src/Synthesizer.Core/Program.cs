using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Synthesizer.Core.Domain;
using Synthesizer.Core.Domain.Modules;
using Synthesizer.Core.Extensions;
using Synthesizer.Core.Infrastructure.NAudio;
using Synthesizer.Shared;
using Synthesizer.Shared.Extensions;
using Synthesizer.Shared.Messages;

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

BuildSynthesizer(serviceProvider);

var signalBus = serviceProvider.GetRequiredService<SignalBus>();
var playbackController = serviceProvider.GetRequiredService<PlaybackController>();
var connectionManager = serviceProvider.GetRequiredService<IConnectionManager>();
var messageReadingJob = serviceProvider.GetRequiredService<IMessageReadingJob>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

var cancellationTokenSource = new CancellationTokenSource();

serviceProvider.RegisterMessageHandler<SetSignalValueMessage>(m =>
{
    logger.LogInformation("Received set signal value message {SignalId} {Value}", m.SignalId, m.Value);
    signalBus.SetSignalValue(new SignalId(m.SignalId), m.Value);
});

serviceProvider.RegisterMessageHandler<QuitMessage>(m =>
{
    logger.LogInformation("Received quit message");
    cancellationTokenSource.Cancel();
});

logger.LogInformation("Waiting for clients to connect");

await connectionManager.ConnectAsync(cancellationTokenSource.Token);

logger.LogInformation("Connected to server");

playbackController.Start();

await messageReadingJob.RunAsync(cancellationTokenSource.Token);

playbackController.Stop();

await connectionManager.DisconnectAsync();

return;


void BuildSynthesizer(IServiceProvider services)
{
    var signalBus = services.GetRequiredService<SignalBus>();
    var synthesizer = services.GetRequiredService<ModularSynthesizer>();
    
    var externalFrequency = signalBus.CreateSignal();
    var externalGate = signalBus.CreateSignal();
    var synthesizerOutputSignal = signalBus.CreateSignal();
    
    synthesizer.SetChannelOutputSignal(0, synthesizerOutputSignal);
    
    var oscillator = synthesizer.CreateModule<Oscillator>();
    oscillator.Waveform = Waveform.Square;
    oscillator.FrequencyInput = externalFrequency;
    oscillator.AmplitudeInput = signalBus.CreateSignal(0.5);
    oscillator.SignalOutput = signalBus.CreateSignal();
    
    var envelopeGenerator = synthesizer.CreateModule<EnvelopeGenerator>();
    envelopeGenerator.GateInput = externalGate;
    envelopeGenerator.SignalInput = oscillator.SignalOutput;
    envelopeGenerator.SignalOutput = signalBus.CreateSignal();
    
    var lowPassFilter = synthesizer.CreateModule<ButterworthLowpass>();
    lowPassFilter.CutoffFrequencyInput = signalBus.CreateSignal(1500);
    lowPassFilter.SignalInput = envelopeGenerator.SignalOutput;
    lowPassFilter.SignalOutput = signalBus.CreateSignal();

    /*
    var delayTimeOscillator = synthesizer.CreateModule<Oscillator>();
    delayTimeOscillator.AmplitudeInput = signalBus.CreateSignal(0.4);
    delayTimeOscillator.OffsetInput = signalBus.CreateSignal(0.5);
    delayTimeOscillator.FrequencyInput = signalBus.CreateSignal(550);
    delayTimeOscillator.SignalOutput = signalBus.CreateSignal();
    */

    var delay = synthesizer.CreateModule<Delay>();
    delay.SignalInput = lowPassFilter.SignalOutput;
    delay.DelayTimeInput = signalBus.CreateSignal(0.3);
    delay.MixInput = signalBus.CreateSignal(0.5);
    delay.FeedbackInput = signalBus.CreateSignal(0.99);
    delay.SignalOutput = synthesizerOutputSignal;
}