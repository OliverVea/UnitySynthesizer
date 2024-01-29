using Microsoft.Extensions.DependencyInjection;
using ProtoBuf;
using Synthesizer.Core.Domain;
using Synthesizer.Core.Domain.Modules;
using Synthesizer.Core.Extensions;
using Synthesizer.Core.Infrastructure.NAudio;
using Synthesizer.Shared;
using Synthesizer.Shared.Extensions;

namespace Tests.Shared;

public class DocumentationExample
{
    [Test]
    public async Task Test()
    {
        var samplingConfiguration = new SamplingConfiguration
        {
            SampleRate = 44100,
            Latency = 50,
            Channels = 1
        };

        var serviceCollection = new ServiceCollection()
            .AddSynthesizerServerServices(samplingConfiguration);
        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var signalBus = serviceProvider.GetRequiredService<SignalBus>();
        var synthesizer = serviceProvider.GetRequiredService<ModularSynthesizer>();

        var oscillatorModule = synthesizer.CreateModule<Oscillator>();
        oscillatorModule.Waveform = Waveform.Square;
        oscillatorModule.FrequencyInput = signalBus.CreateSignal(440);
        oscillatorModule.AmplitudeInput = signalBus.CreateSignal(0.5);
        oscillatorModule.SignalOutput = signalBus.CreateSignal();
        
        var envelopeModule = synthesizer.CreateModule<EnvelopeGenerator>();
        envelopeModule.GateInput = signalBus.CreateSignal();
        envelopeModule.SignalInput = oscillatorModule.SignalOutput;
        envelopeModule.SignalOutput = signalBus.CreateSignal();
        
        synthesizer.SetChannelOutputSignal(0, envelopeModule.SignalOutput.Value);
        
        var playbackController = serviceProvider.GetRequiredService<PlaybackController>();
        playbackController.Start();
        
        signalBus.SetSignalValue(envelopeModule.GateInput, 1);
        await Task.Delay(TimeSpan.FromSeconds(0.5));
        signalBus.SetSignalValue(envelopeModule.GateInput, 0);
        await Task.Delay(TimeSpan.FromSeconds(0.5));
        
        playbackController.Stop();
    }
    
    public class DelayOneSampleModule(SignalBus signalBus) : SynthesizerModule
    {
        public SignalId? SignalInput { get; set; }
        public SignalId? SignalOutput { get; set; }

        private double _previousValue;
    
        public override void Update(SynthesizerUpdateContext context)
        {
            signalBus.WriteSignalValue(SignalOutput, _previousValue);
            _previousValue = signalBus.ReadSignalValue(SignalInput);
        }
    }


    [Test]
    public void Test2()
    {
        var samplingConfiguration = new SamplingConfiguration
        {
            SampleRate = 44100,
            Latency = 50,
            Channels = 1
        };

        var serviceCollection = new ServiceCollection()
            .AddSynthesizerServerServices(samplingConfiguration);
        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var signalBus = serviceProvider.GetRequiredService<SignalBus>();
        var synthesizer = serviceProvider.GetRequiredService<ModularSynthesizer>();
        
        var delayModule = synthesizer.CreateModule<DelayOneSampleModule>();
        delayModule.SignalInput = signalBus.CreateSignal();
        delayModule.SignalOutput = signalBus.CreateSignal();

        var signal = signalBus.CreateSignal();

        signalBus.SetSignalValue(signal, 1); // Sets both the read and write line to 1

        signalBus.WriteSignalValue(signal, 2);

        var value = signalBus.ReadSignalValue(signal);
        
        signalBus.SwapBusses();
    }

    [ProtoContract]
    public class EchoMessage : Message
    {
        [ProtoMember(1)]
        public required string Text { get; init; }
    }
    
    [Test]
    public async Task Test3()
    { 
        var serviceCollection = new ServiceCollection();
            
        serviceCollection.AddClientServices();
        serviceCollection.AddServerServices();
        
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var connectionManager = serviceProvider.GetRequiredService<IConnectionManager>();
        
        await connectionManager.ConnectAsync(CancellationToken.None);
        
        var messageSender = serviceProvider.GetRequiredService<IMessageSender>();
        
        var message = new EchoMessage
        {
            Text = "Hello World"
        };
        
        messageSender.Send(message);
        await messageSender.SendAsync(message, CancellationToken.None);
        
        serviceProvider.RegisterMessageHandler<EchoMessage>(message =>
        {
            Console.WriteLine(message.Text);
        });
        
        serviceProvider.RegisterMessageHandler<EchoMessage>(async message =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            Console.WriteLine(message.Text);
        });
    }
}