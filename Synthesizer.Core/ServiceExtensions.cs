using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using Synthesizer.Core.Abstractions;
using Synthesizer.Core.Infrastructure;
using Synthesizer.Core.SignalProcessing;

namespace Synthesizer.Core;

public static class ServiceExtensions
{
    public static IServiceCollection AddSynthesizerCore(this IServiceCollection services, SamplingConfiguration configuration)
    {
        var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(configuration.SampleRate, configuration.Channels);
        
        services.AddSingleton(configuration);
        services.AddSingleton(waveFormat);
        services.AddSingleton<SignalBus>();
        services.AddSingleton<ModularSynthesizer>();
        services.AddSingleton<ISampleProvider, SampleProvider>();
        services.AddSingleton<PlaybackController>();
        
        var device = DirectSoundOut.DSDEVID_DefaultPlayback;
        var outputDevice = new DirectSoundOut(device, configuration.Latency);
        
        services.AddTransient<DirectSoundOut>(_ => outputDevice);
        
        return services;
    }
}