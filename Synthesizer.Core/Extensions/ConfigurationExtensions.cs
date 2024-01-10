using Microsoft.Extensions.Configuration;
using Synthesizer.Core.Domain;

namespace Synthesizer.Core.Extensions;

public static class ConfigurationExtensions
{
    public static SamplingConfiguration GetSamplingConfiguration(this IConfiguration configuration, string sectionName = "Sampling")
    {
        return configuration.GetRequiredSection(sectionName).Get<SamplingConfiguration>() ?? throw new InvalidOperationException($"Could not find configuration section '{sectionName}'");
    }
}