using Microsoft.Extensions.DependencyInjection;
using Synthesizer.Core;
using Synthesizer.Core.Domain;
using Synthesizer.Core.Domain.Modules;
using Synthesizer.Core.Extensions;

var configuration = new SamplingConfiguration()
{
    Channels = 1,
    Latency = 50,
    SampleRate = 44100
};

var serviceCollection = new ServiceCollection().AddSynthesizerServerServices(configuration);

var serviceProvider = serviceCollection.BuildServiceProvider();

BuildSynthesizer(serviceProvider);

var server = serviceProvider.GetRequiredService<ModularSynthesizerServer>();

await server.RunAsync(CancellationToken.None);


void BuildSynthesizer(IServiceProvider services)
{
    var signalBus = services.GetRequiredService<SignalBus>();
    var synthesizer = services.GetRequiredService<ModularSynthesizer>();
    
    var externalSignal = signalBus.CreateSignal();
    var synthesizerOutputSignal = signalBus.CreateSignal();
    
    synthesizer.SetChannelOutputSignal(0, synthesizerOutputSignal);
    
    var oscillator = synthesizer.CreateModule<Oscillator>();
    oscillator.Waveform = Waveform.Sawtooth;
    oscillator.FrequencyInput = externalSignal;
    oscillator.AmplitudeInput = signalBus.CreateSignal(0.5);
    oscillator.SignalOutput = signalBus.CreateSignal();

    var envelopeImpulse = synthesizer.CreateModule<ImpulseGenerator>();
    envelopeImpulse.SignalInput = externalSignal;
    envelopeImpulse.SignalOutput = signalBus.CreateSignal();
    
    var envelopeGenerator = synthesizer.CreateModule<EnvelopeGenerator>();
    envelopeGenerator.GateInput = envelopeImpulse.SignalOutput;
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