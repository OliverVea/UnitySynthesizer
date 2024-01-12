using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Synthesizer.Shared.Domain
{
    internal class MessageHandlerService : IMessageHandlerService
    {
        private readonly Dictionary<Type, List<Action<Message>>> _handlers = new Dictionary<Type, List<Action<Message>>>();
        private readonly Dictionary<Type, List<Func<Message, Task>>> _asyncHandlers = new Dictionary<Type, List<Func<Message, Task>>>();

        public void RegisterMessageHandler<TMessage>(Action<TMessage> handler) where TMessage : Message
        {
            var messageType = typeof(TMessage);
            if (!_handlers.ContainsKey(messageType))
            {
                _handlers.Add(messageType, new List<Action<Message>>());
            }
            _handlers[messageType].Add(message => handler((TMessage)message));
        }

        public void RegisterMessageHandler<TMessage>(Func<TMessage, Task> handler) where TMessage : Message
        {
            var messageType = typeof(TMessage);
            if (!_asyncHandlers.ContainsKey(messageType))
            {
                _asyncHandlers.Add(messageType, new List<Func<Message, Task>>());
            }
            _asyncHandlers[messageType].Add(message => handler((TMessage)message));
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
            
            foreach (var handler in handlers)
            {
                handler(message);
            }
        }
        
        private async Task InternalHandleMessageAsync(Message message)
        {
            var messageType = message.GetType();
            if (!_asyncHandlers.TryGetValue(messageType, out var handlers)) return;
            
            foreach (var handler in handlers)
            {
                await handler(message);
            }
        }
    }
}