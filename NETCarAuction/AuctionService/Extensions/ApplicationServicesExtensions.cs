using AuctionService.Consumers;
using AuctionService.Data.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
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
                cfg.ConfigureEndpoints(context);
            });
        });
        // Add entity dbContext for app, add postgreSQL connection for dbContext
        services.AddDbContext<AuctionDBContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        
        return services;
    }
}