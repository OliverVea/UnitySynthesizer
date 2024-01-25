# nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Synthesizer.Shared.Extensions;
using UnityEngine;

// Because of the limited scope, a service locator pattern is used.
// A DI framework like e.g. Zenject would be a better choice for a larger project.
namespace Demo.Gui
{
    public class ServiceLocator : MonoBehaviour
    {
        private readonly IServiceCollection _serviceCollection = new ServiceCollection();
        private IServiceProvider? _serviceProvider;
        
        private static ServiceLocator? _instance;
        
        private const string MultipleInstancesDetected = "Multiple instances of ServiceLocator tried to register";
        private const string NoInstancesDetected = "Did not register an instance of ServiceLocator yet";
        private const string ServiceProviderIsNull = "Service provider is null";

        public void Awake()
        {
            _serviceCollection.AddClientServices();
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            
            if (_instance is null) _instance = this;
            else throw new ApplicationException(MultipleInstancesDetected);
        }

        public static T GetRequiredService<T>() where T : notnull
        {
            if (_instance is null) throw new NullReferenceException(NoInstancesDetected);
            if (_instance._serviceProvider is null) throw new NullReferenceException(ServiceProviderIsNull);
            
            return _instance._serviceProvider.GetRequiredService<T>();
        }
    }
}