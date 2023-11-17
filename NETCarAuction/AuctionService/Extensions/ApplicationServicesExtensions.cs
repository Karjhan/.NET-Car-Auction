using AuctionService.Consumers;
using AuctionService.Data;
using AuctionService.Data.Contexts;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IAuctionRepository, AuctionRepository>();
        // Add AutoMapper for object mapping
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        // Add MassTransit service for RabbitMQ message broker
        services.AddMassTransit(config =>
        {
            config.AddEntityFrameworkOutbox<AuctionDBContext>(options =>
            {
                options.QueryDelay = TimeSpan.FromSeconds(10);
                options.UsePostgres();
                options.UseBusOutbox();
            });
            config.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
            config.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));
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
        // Add entity dbContext for app, add postgreSQL connection for dbContext
        services.AddDbContext<AuctionDBContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        // Add authentication with jwt token
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.Authority = configuration["IdentityServiceURL"];
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.NameClaimType = "username";
        });
        // Add GRPC auction service for service to service communication
        services.AddGrpc();
        
        return services;
    }
}