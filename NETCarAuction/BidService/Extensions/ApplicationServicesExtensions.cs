using BidService.Consumers;
using BidService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BidService.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add Background task service
        services.AddHostedService<CheckAuctionFinished>();
        // Add AutoMapper for object mapping
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        // Add MassTransit service for RabbitMQ message broker
        services.AddMassTransit(config =>
        {
            config.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
            config.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));
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
        // Add authentication with jwt token
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.Authority = configuration["IdentityServiceURL"];
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.NameClaimType = "username";
        });
        // Add GRPC service for this is client 
        services.AddScoped<GrpcAuctionClient>();
        
        return services;
    }
}