using System.Threading;
using System.Threading.Tasks;

namespace Synthesizer.Shared
{
    public interface IConnectionManager
    {
        Task ConnectAsync(CancellationToken cancellationToken);
        Task DisconnectAsync();
    }
}