using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Synthesizer.Shared.Extensions;

namespace Synthesizer.Shared.Domain
{
    internal class MessageHandlerService : IMessageHandlerService
    {
        private readonly Dictionary<Type, List<Action<Message>>> _handlers = new Dictionary<Type, List<Action<Message>>>();
        private readonly Dictionary<Type, List<Func<Message, Task>>> _asyncHandlers = new Dictionary<Type, List<Func<Message, Task>>>();

        public void RegisterMessageHandler<TMessage>(Action<TMessage> handler) where TMessage : Message
        {
            _handlers.SetDefault(typeof(TMessage), new List<Action<Message>>())
                .Add(message => handler((TMessage)message));
        }

        public void RegisterMessageHandler<TMessage>(Func<TMessage, Task> handler) where TMessage : Message
        {
            _asyncHandlers.SetDefault(typeof(TMessage), new List<Func<Message, Task>>())
                .Add(message => handler((TMessage)message));
        }

        public Task HandleMessageAsync(Message message)
        {
            InternalHandleMessage(message);
            return InternalHandleMessageAsync(message);
        }
        
        private void InternalHandleMessage(Message message)
        {
            var messageType = message.GetType();
            if (!_handlers.TryGetValue(messageType, out var handlers)) return;
            
            foreach (var handler in handlers) handler(message);
        }
        
        private async Task InternalHandleMessageAsync(Message message)
        {
            var messageType = message.GetType();
            if (!_asyncHandlers.TryGetValue(messageType, out var handlers)) return;
            
            foreach (var handler in handlers) await handler(message);
        }
    }
}