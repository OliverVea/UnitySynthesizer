using Microsoft.Extensions.DependencyInjection;
using Synthesizer.Shared.Domain;
using Synthesizer.Shared.Infrastructure;

namespace Synthesizer.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServerServices(this IServiceCollection services)
        {
            services.AddSharedServices();
            services.AddSingleton<INamedPipeStream, ServerNamedPipeStream>();
            
            return services;
        }
        
        public static IServiceCollection AddClientServices(this IServiceCollection services)
        {
            services.AddSharedServices();
            services.AddSingleton<INamedPipeStream, ClientNamedPipeStream>();
            
            return services;
        }
        
        private static void AddSharedServices(this IServiceCollection services)
        {
            services.AddSingleton<IMessageHandlerService, MessageHandlerService>();
            services.AddSingleton<IMessageReadingJob, MessageReadingJob>();
            services.AddSingleton<IMessageRepository, MessageRepository>();
            services.AddSingleton<IMessageSerializer, ProtobufMessageSerializer>();
            services.AddSingleton<IMessageSender, MessageSendingService>();
            services.AddSingleton<IConnectionManager, ConnectionManagementService>();
        }
    }
}