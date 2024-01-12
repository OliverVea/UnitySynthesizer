using System;
using System.Threading.Tasks;

namespace Synthesizer.Shared.Domain
{
    internal interface IMessageHandlerService
    {
        void RegisterMessageHandler<TMessage>(Action<TMessage> handler) where TMessage : Message;
        void RegisterMessageHandler<TMessage>(Func<TMessage, Task> handler) where TMessage : Message;
        Task HandleMessageAsync(Message message);
    }
}