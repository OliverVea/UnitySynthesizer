using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using Synthesizer;
using Synthesizer.SignalProcessing;
using Synthesizer.SignalProcessing.Modules;

var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var config = configurationBuilder.Build();


if (samplingConfiguration is null) throw new InvalidOperationException("Sampling configuration is required.");

var services = new ServiceCollection().AddServices(samplingConfiguration);

var provider = services.BuildServiceProvider();

var sampleController = provider.GetRequiredService<ModularSynthesizer>();
var signalBus = provider.GetRequiredService<SignalBus>();



var keyboard = sampleController.CreateModule<KeyboardGenerator>();

keyboard.AddKeymapEntry(keyInfo => keyInfo.Key == ConsoleKey.A, signalBus.CreateSignal(), 440);
keyboard.AddKeymapEntry(keyInfo => keyInfo.Key == ConsoleKey.S, signalBus.CreateSignal(), 493.88);
keyboard.AddKeymapEntry(keyInfo => keyInfo.Key == ConsoleKey.D, signalBus.CreateSignal(), 523.25);
keyboard.AddKeymapEntry(keyInfo => keyInfo.Key == ConsoleKey.F, signalBus.CreateSignal(), 587.33);


var outputSignal = signalBus.CreateSignal();

var signal = sampleController.CreateModule<FunctionGenerator>();

signal.Waveform = Waveform.Triangle;
signal.FrequencyInput = signalBus.CreateSignal();
signal.AmplitudeInput = signalBus.CreateSignal(0.4f);
signal.SignalOutput = signalBus.CreateSignal();

var envelope = sampleController.CreateModule<EnvelopeGenerator>();

envelope.GateInput = signalBus.CreateSignal();
envelope.SignalInput = signal.SignalOutput;
envelope.SignalOutput = outputSignal;

sampleController.SetChannelOutputSignal(0, envelope.SignalOutput.Value);


var sampleProvider = provider.GetRequiredService<ISampleProvider>();

var device = DirectSoundOut.DSDEVID_DefaultPlayback;

using var outputDevice = new DirectSoundOut(device, samplingConfiguration.Latency);

outputDevice.Init(sampleProvider);
outputDevice.Play();

while (outputDevice.PlaybackState != PlaybackState.Stopped)
{
    Thread.Sleep(100);
}
