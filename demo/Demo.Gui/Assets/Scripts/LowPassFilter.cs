using System.Collections.Generic;
using System.Linq;
using Demo.Messages;
using Synthesizer.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Demo.Gui
{
    public class LowPassFilter : MonoBehaviour
    {
        private static IMessageSender MessageSender => ServiceLocator.GetRequiredService<IMessageSender>();

        private readonly Dictionary<Signal, float> _newValues = new(); 
        
        private void Awake()
        {
            var slidersByObjectName = GetComponentsInChildren<Slider>().ToDictionary(x => x.name);

            var frequencySlider = slidersByObjectName["Cutoff"];
            frequencySlider.onValueChanged.AddListener(OnFrequencyChanged);
            OnFrequencyChanged(frequencySlider.value);
            
            var resonanceSlider = slidersByObjectName["Resonance"];
            resonanceSlider.onValueChanged.AddListener(OnResonanceChanged);
            OnResonanceChanged(resonanceSlider.value);
        }

        private void OnFrequencyChanged(float cutoffFrequency) => _newValues[Signal.LowPassCutoff] = cutoffFrequency;
        private void OnResonanceChanged(float resonance) => _newValues[Signal.LowPassResonance] = resonance;

        private void Update()
        {
            foreach (var (signal, value) in _newValues.ToArray())
            {
                MessageSender.Send(new SetSignalValueMessage
                {
                    Signal = signal,
                    Value = value
                });

                _newValues.Remove(signal);
            }
        }
    }
}