using System.Threading;
using System.Threading.Tasks;

namespace Synthesizer.Shared
{
    public interface IMessageReadingJob
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}