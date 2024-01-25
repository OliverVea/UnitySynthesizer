#nullable enable
using UnityEngine;

namespace Demo.Gui
{
    public abstract class Singleton<T> : MonoBehaviour where T : class
    {
        private const string MultipleInstancesDetected = "Multiple instances of singleton tried to register";
        private const string NoInstancesDetected = "Did not register an instance of singleton yet";

        private static T? _instance;
        public static T Instance => _instance ?? throw new System.NullReferenceException(NoInstancesDetected);

        protected abstract T GetInstance();
    
        private void Awake()
        {
            if (_instance is null) _instance = GetInstance();
            else throw new System.ApplicationException(MultipleInstancesDetected);
        }
    }
}