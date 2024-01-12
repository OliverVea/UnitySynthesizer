using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Synthesizer.Shared.Domain;

namespace Synthesizer.Shared.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static IServiceProvider RegisterMessageHandler<TMessage>(
            this IServiceProvider serviceProvider,
            Action<TMessage> handler) where TMessage : Message
        {
            var messageHandlerService = serviceProvider.GetRequiredService<IMessageHandlerService>();
            
            messageHandlerService.RegisterMessageHandler(handler);
            
            return serviceProvider;
        }
        
        public static IServiceProvider RegisterMessageHandler<TMessage>(
            this IServiceProvider serviceProvider,
            Func<TMessage, Task> asyncHandler) where TMessage : Message
        {
            var messageHandlerService = serviceProvider.GetRequiredService<IMessageHandlerService>();
            
            messageHandlerService.RegisterMessageHandler(asyncHandler);
            
            return serviceProvider;
        }
        
        
    }
}