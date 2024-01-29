using System.Collections.Generic;
using System.Linq;
using Demo.Messages;
using Synthesizer.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Demo.Gui
{
    
    public class Gain : MonoBehaviour
    {
        private static IMessageSender MessageSender => ServiceLocator.GetRequiredService<IMessageSender>();

        private readonly Dictionary<Signal, float> _newValues = new(); 
        
        private void Awake()
        {
            var slidersByObjectName = GetComponentsInChildren<Slider>().ToDictionary(x => x.name);

            var gainSlider = slidersByObjectName["Gain"];
            gainSlider.onValueChanged.AddListener(OnGainChanged);
            OnGainChanged(gainSlider.value);
        }
        
        private void OnGainChanged(float gain) => _newValues[Signal.Gain] = gain;
        
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