using System.IO.Pipes;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using Synthesizer.Core.Domain;
using Synthesizer.Core.Infrastructure.Api;
using Synthesizer.Core.Infrastructure.NAudio;

namespace Synthesizer.Core.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSynthesizerClientServices(this IServiceCollection services)
    {
        AddIPCServices(services);

        var namedPipeClientStream = new NamedPipeClientStream(".", ModularSynthesizerServer.ServerPipeName, PipeDirection.Out);
        services.AddSingleton(namedPipeClientStream);
        services.AddSingleton<IMessageSender, ClientMessageSender>();

        return services;
    }

    public static IServiceCollection AddSynthesizerServerServices(this IServiceCollection services, SamplingConfiguration configuration)
    {
        AddIPCServices(services);

        var namedPipeServerStream = new NamedPipeServerStream(ModularSynthesizerServer.ServerPipeName, PipeDirection.In);
        services.AddSingleton(namedPipeServerStream);
        services.AddSingleton<IMessageSender, ServerMessageSender>();
        
        var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(configuration.SampleRate, configuration.Channels);
        
        services.AddSingleton(configuration);
        services.AddSingleton(waveFormat);
        services.AddSingleton<SignalBus>();
        services.AddSingleton<ModularSynthesizer>();
        services.AddSingleton<ISampleProvider, SampleProvider>();
        services.AddSingleton<PlaybackController>();
        services.AddSingleton<ModularSynthesizerServer>();
        
        return services;
    }

    private static IServiceCollection AddIPCServices(this IServiceCollection services)
    {
        services.AddSingleton<IMessageSerializer, MessageSerializer<EchoMessage>>();
        services.AddSingleton<MessageSerializerResolver>();
        services.AddSingleton<MessageService>();

        return services;
    }
}