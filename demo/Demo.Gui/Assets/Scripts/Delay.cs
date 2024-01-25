using System.Collections.Generic;
using System.Linq;
using Demo.Messages;
using Synthesizer.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Demo.Gui
{
    public class Delay : MonoBehaviour
    {
        private static IMessageSender MessageSender => ServiceLocator.GetRequiredService<IMessageSender>();

        private readonly Dictionary<Signal, float> _newValues = new(); 
        
        private void Awake()
        {
            var slidersByObjectName = GetComponentsInChildren<Slider>().ToDictionary(x => x.name);

            var mixSlider = slidersByObjectName["Mix"];
            mixSlider.onValueChanged.AddListener(OnMixChanged);
            OnMixChanged(mixSlider.value);
            
            var timeSlider = slidersByObjectName["Time"];
            timeSlider.onValueChanged.AddListener(OnTimeChanged);
            OnTimeChanged(timeSlider.value);
            
            var feedbackSlider = slidersByObjectName["Feedback"];
            feedbackSlider.onValueChanged.AddListener(OnFeedbackChanged);
            OnFeedbackChanged(feedbackSlider.value);
        }

        private void OnMixChanged(float mix) => _newValues[Signal.DelayMix] = mix;
        private void OnTimeChanged(float time) => _newValues[Signal.DelayTime] = time;
        private void OnFeedbackChanged(float feedback) => _newValues[Signal.DelayFeedback] = feedback;

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
