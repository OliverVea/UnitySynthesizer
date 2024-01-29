## Introduction

This documentation provides an overview and setup instructions for the core components of the Vynth synthesizer framework.
It includes detailed guidelines on how to build, run, and integrate synthesizer functionalities into .NET applications.

- [Introduction](#Introduction)
- [Projects](#Projects)
- [Synthesizer.Core](#SynthesizerCore)
  - [Building and running the synthesizer](#Building-and-running-the-synthesizer)
  - [Signals](#Signals)
  - [Modules](#Modules)
  - [Limitations](#Limitations)
- [Synthesizer.Shared](#SynthesizerShared)
  - [Initialization](#Initialization)
  - [Sending messages](#Sending-messages)
  - [Receiving messages](#Receiving-messages)

## Projects

The framework is divided into two main projects:
- **Synthesizer.Core**: Implements the essential synthesizing logic, enabling the creation and manipulation of audio signals.
- **Synthesizer.Shared**: Provides shared resources, including gRPC contracts and implementations, facilitating communication and data exchange within the framework.

## Synthesizer.Core

**Synthesizer.Core** encompasses the framework's primary logic for audio synthesis. Key components include:
- `ModularSynthesizer`: Manages audio modules and controls playback.
- `SignalBus`: Handles signal operations, including creation and manipulation.
- `SynthesizerModule`: Acts as a base for modules to generate or process audio signals.
- `PlaybackController`: Controls the playback state of the synthesizer.

### Building and running the synthesizer

The synthesizer integrates seamlessly with Microsoft's Dependency Injection (DI) framework, facilitating easy setup and configuration in .NET applications.
The following code snippet shows how to initialize the synthesizer in a .NET application. It is implemented as a [Test](../tests/Synthesizer.Tests/DocumentationExample.cs).

```csharp
var samplingConfiguration = new SamplingConfiguration
{
    SampleRate = 44100,
    Latency = 50,
    Channels = 1
};

var serviceCollection = new ServiceCollection()
    .AddSynthesizerServerServices(samplingConfiguration);
```

The `SamplingConfiguration` object is used to configure the sampling rate, latency and number of channels.
The `AddSynthesizerServices` method adds the synthesizer services to the dependency injection framework.

To configure the synthesizer, the `SignalBus` must be retrieved.

```csharp
var serviceProvider = serviceCollection.BuildServiceProvider();

var signalBus = serviceProvider.GetRequiredService<SignalBus>();
```

The `SignalBus` contains all the signals that are used by the synthesizer.
It has both signal values that are updated for each sample and e.g. configurations that might only be set once.

The next step is to register modules to the synthesizer.

```csharp
var synthesizer = serviceProvider.GetRequiredService<ModularSynthesizer>();

var oscillatorModule = synthesizer.CreateModule<Oscillator>();
oscillatorModule.Waveform = Waveform.Square;
oscillatorModule.FrequencyInput = signalBus.CreateSignal(440);
oscillatorModule.AmplitudeInput = signalBus.CreateSignal(0.5);
oscillatorModule.SignalOutput = signalBus.CreateSignal();
```

Here, an `Oscillator` is registered to the synthesizer.
The `Oscillator` acts as an Oscillator, which generates a periodic signal.
The `Waveform` property is used to set the waveform of the oscillator.
The `FrequencyInput` and `AmplitudeInput` properties are used to set the frequency and amplitude of the oscillator.

The `SignalOutput` property is used to set the output of the oscillator. Here, we just create a new signal, which can be used by other modules.

To add some character to the note, an `EnvelopeGenerator` is added.

```csharp
var envelopeModule = synthesizer.CreateModule<EnvelopeGenerator>();
envelopeModule.GateInput = signalBus.CreateSignal();
envelopeModule.SignalInput = oscillatorModule.SignalOutput;
envelopeModule.SignalOutput = signalBus.CreateSignal();
```

The `EnvelopeGenerator` is used to shape the signal of the oscillator.
The `GateInput` property is used to set the gate of the envelope, opening and closing the envelope. The gate is activated if `GateInput > 0`.
The `SignalInput` property is used to set the input of the envelope.
The `SignalOutput` property is used to set the output of the envelope.

Lastly, we connect the output of the `EnvelopeGenerator` to the output of the synthesizer.
To play a note, the gate of the envelope must be activated.

```csharp
synthesizer.SetChannelOutputSignal(0, envelopeModule.SignalOutput.Value);

var playbackController = serviceProvider.GetRequiredService<PlaybackController>();
playbackController.Start();

signalBus.SetSignalValue(envelopeModule.GateInput, 1);
await Task.Delay(TimeSpan.FromSeconds(0.5));
signalBus.SetSignalValue(envelopeModule.GateInput, 0);
await Task.Delay(TimeSpan.FromSeconds(0.5));

playbackController.Stop();
```

The `SetChannelOutputSignal` method is used to set the output signal of the synthesizer.
The `PlaybackController` is used to start and stop the playback of the synthesizer.
The `SetSignalValue` method is used to set the value of the gate of the envelope, activating the envelope.
The `Task.Delay` method is used to wait for half a second.
Then the gate is deactivated by setting the value to 0.
The second `Task.Delay` waits for another half a second and then the playback is stopped.

### Signals

A **Signal** represents an audio sample's value, capable of being updated with each new sample processed by the synthesizer.
The `SignalBus` is used to create, set, read and write signals.
In order to support the sampling of a signal, the `SignalBus` has a read and a write line that is flipped for each sample.

```csharp
var signalBus = serviceProvider.GetRequiredService<SignalBus>();

var signal = signalBus.CreateSignal();

signalBus.SetSignalValue(signal, 1); // Sets both the read and write line to 1

signalBus.WriteSignalValue(signal, 2); // Sets the write line to 2

var value = signalBus.ReadSignalValue(signal); // Reads the value of the read line, which is 1

signalBus.SwapBusses(); // Swaps the read and write line

value = signalBus.ReadSignalValue(signal); // Reads the value of the read line, which is 2
```

### Modules

Modules are specialized classes derived from `SynthesizerModule`, designed for specific audio processing tasks such as signal generation, modulation, or effects application.
It can be used to generate, process or output signals.

```csharp
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
```

The `Update` method is called for each sample.
The `SynthesizerUpdateContext` contains information about the current sample.
The `SignalBus` is used to read and write signals.
The `SignalInput` and `SignalOutput` properties are used to set the input and output of the module.

To register the module to the synthesizer, the `CreateModule` method of the `ModularSynthesizer` is used.

```csharp
var delayModule = synthesizer.CreateModule<DelayOneSampleModule>();
delayModule.SignalInput = signalBus.CreateSignal();
delayModule.SignalOutput = signalBus.CreateSignal();
```

### Limitations

Inherent to the signal processing, a minimal delay of one sample is unavoidable between a module's output and the subsequent module's input.
This constraint is essential for real-time audio effects and feedback loops, necessitating careful design considerations.
For any effects that require, e.g., immediate feedback, this has to be implemented as one single module.

## Synthesizer.Shared

The `Synthesizer.Shared` project contains shared gRPC contracts and implementations for the synthesizer framework.

### Initialization

There are two methods for registering services for the IPC communication.
`AddClientServices` is used to register services for clients and `AddServerServices` is used to register services for servers.

```csharp
var serviceCollection = new ServiceCollection();

serviceCollection.AddClientServices(); // For clients
serviceCollection.AddServerServices(); // For servers

var serviceProvider = serviceCollection.BuildServiceProvider();
```

After the services are registered, the `IConnectionManager` can be retrieved and the connection can be established.

````csharp
var connectionManager = serviceProvider.GetRequiredService<IConnectionManager>();

await connectionManager.ConnectAsync(CancellationToken.None);
````

### Sending messages

Messages are used to send data between the client and the server.
To send a message, a class must be created that inherits from the `Message` class.

The message must be a protobuf message and must be decorated with the `ProtoContract` attribute.
The properties of the message must be decorated with the `ProtoMember` attribute.

```csharp
[ProtoContract]
public class EchoMessage : Message
{
    [ProtoMember(1)]
    public required string Text { get; init; }
}
```

To send messages, the `IMessageSender` can be retrieved from the service provider.

```csharp
var messageSender = serviceProvider.GetRequiredService<IMessageSender>()
    
var message = new EchoMessage
{
  Text = "Hello World"
};

messageSender.Send(message); // Synchronous
await messageSender.SendAsync(message, CancellationToken.None); // Asynchronous
```

Messages can be sent synchronously or asynchronously.
The `CancellationToken` can be used to cancel the sending of the message.

### Receiving messages

To receive messages, a message handler must be registered.

```csharp
serviceProvider.RegisterMessageHandler<EchoMessage>(message =>
{
    Console.WriteLine(message.Text);
});
```

The message handler is called for each message that is received of that type.
The message handler can also be asynchronous.

```csharp

serviceProvider.RegisterMessageHandler<EchoMessage>(async message =>
{
    await Task.Delay(TimeSpan.FromSeconds(1));
    Console.WriteLine(message.Text);
});
```