using MassTransit;
using NotificationService.Consumers;

namespace NotificationService.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add SignalR service
        services.AddSignalR();
        // Add MassTransit service for RabbitMQ message broker
        services.AddMassTransit(config =>
        {
            config.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
            config.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("nt", false));
            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], "/", host =>
                {
                    host.Username(configuration.GetValue("RabbitMQ:Username", "guest"));
                    host.Password(configuration.GetValue("RabbitMQ:Password", "guest"));
                });
                cfg.ConfigureEndpoints(context);
            });
        });
        
        return services;
    }
}