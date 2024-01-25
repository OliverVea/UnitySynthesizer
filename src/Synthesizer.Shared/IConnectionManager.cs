using System.Threading;
using System.Threading.Tasks;
using Synthesizer.Shared.Infrastructure;

namespace Synthesizer.Shared
{
    public interface IConnectionManager
    {
        Task ConnectAsync(CancellationToken cancellationToken);
        Task DisconnectAsync();
        
        internal INamedPipeStream NamedPipeStream { get; }
    }
}