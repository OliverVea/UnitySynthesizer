using System;
using System.Threading;
using Synthesizer.Shared;
using UnityEngine;

namespace Demo.Gui
{
    public class ServerConnectionManager : MonoBehaviour
    {
        private readonly Lazy<IConnectionManager> _connectionManager = new(ServiceLocator.GetRequiredService<IConnectionManager>);
        private IConnectionManager ConnectionManager => _connectionManager.Value;

        private void Awake()
        {
            ConnectionManager.ConnectAsync(CancellationToken.None).Wait();
        }
        
        private void OnDestroy()
        {
            ConnectionManager.DisconnectAsync().Wait();
        }
    }
}