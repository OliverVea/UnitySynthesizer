using Synthesizer.Core.SignalProcessing;

namespace Synthesizer.Core.Serialization;

public class SynthesizerSerializer
{
    public void Serialize(ModularSynthesizer modularSynthesizer, SignalBus signalBus, FileInfo fileInfo)
    {
        var serializer = new JsonSerializer();
        using var streamWriter = fileInfo.CreateText();
        using var jsonWriter = new JsonTextWriter(streamWriter);
        serializer.Serialize(jsonWriter, new SynthesizerState(modularSynthesizer, signalBus));
    }
}