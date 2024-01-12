using System.Threading;
using System.Threading.Tasks;

namespace Synthesizer.Shared
{
    public interface IMessageSender
    {
        void Send<T>(T message) where T : Message;
        Task SendAsync<T>(T message, CancellationToken cancellationToken) where T : Message;
    }
}