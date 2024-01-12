using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using Synthesizer.Core.Domain;
using Synthesizer.Core.Infrastructure.NAudio;
using Synthesizer.Shared;
using Synthesizer.Shared.Extensions;

namespace Synthesizer.Core.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSynthesizerServerServices(this IServiceCollection services, SamplingConfiguration configuration)
    {
        services.AddServerServices();

        var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(configuration.SampleRate, configuration.Channels);

        services.AddSingleton(configuration);
        services.AddSingleton(waveFormat);
        services.AddSingleton<SignalBus>();
        services.AddSingleton<ModularSynthesizer>();
        services.AddSingleton<ISampleProvider, SampleProvider>();
        services.AddSingleton<PlaybackController>();

        return services;
    }
}