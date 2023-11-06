using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Services;

namespace AuctionService.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add auction service http service
        services.AddHttpClient<AuctionServiceHTTPClient>().AddPolicyHandler(GetPolicy());
        // Add AutoMapper for object mapping
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        // Add MassTransit service for RabbitMQ message broker
        services.AddMassTransit(config =>
        {
            config.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
            config.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], "/", host =>
                {
                    host.Username(configuration.GetValue("RabbitMQ:Username", "guest"));
                    host.Password(configuration.GetValue("RabbitMQ:Password", "guest"));
                });
                cfg.ReceiveEndpoint("search-auction-created", endpoint =>
                {
                    endpoint.UseMessageRetry(retryConfig => retryConfig.Interval(5,5));
                    endpoint.ConfigureConsumer<AuctionCreatedConsumer>(context);
                });
                cfg.ConfigureEndpoints(context);
            });
        });
        
        return services;
    }
    
    static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(message => message.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
    }
}