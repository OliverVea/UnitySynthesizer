using Microsoft.Extensions.DependencyInjection;
using Synthesizer.Core.Abstractions;

namespace Synthesizer.Core.SignalProcessing;

public class ModularSynthesizer(
    SamplingConfiguration samplingConfiguration,
    SignalBus signalBus,
    IServiceProvider serviceProvider)
{
    private readonly SignalId[] _outputChannels = new SignalId[samplingConfiguration.Channels];
    
    private readonly List<SynthesizerModule> _modules = new();
    
    private readonly SynthesizerUpdateContext _updateContext = new()
    {
        DeltaTime = samplingConfiguration.SamplePeriod
    };
    
    public T CreateModule<T>() where T : SynthesizerModule
    {
        var module = ActivatorUtilities.CreateInstance<T>(serviceProvider);
        module.Initialize();
        _modules.Add(module);
        return module;
    }

    public void SampleOutput(Span<float> channelSamples)
    {
        if (channelSamples.Length != _outputChannels.Length) throw new ArgumentException("Buffer length must match output channel count.");
        
        UpdateModules();
        
        for (var channel = 0; channel < _outputChannels.Length; channel++)
        {
            channelSamples[channel] = (float)signalBus.ReadSignalValue(_outputChannels[channel]);
        }

        signalBus.SwapBusses();
    }
    
    private void UpdateModules() => _modules.ForEach(m => m.Update(_updateContext));

    public void SetChannelOutputSignal(int channel, SignalId signalId) => _outputChannels[channel] = signalId;
}