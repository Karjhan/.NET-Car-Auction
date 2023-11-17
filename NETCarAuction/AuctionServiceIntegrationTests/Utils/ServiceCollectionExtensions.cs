using AuctionService.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionServiceIntegrationTests.Utils;

public static class ServiceCollectionExtensions
{
    public static void RemoveDBContext<T>(this IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AuctionDBContext>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }

    public static void EnsureCreated<T>(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<AuctionDBContext>();
        db.Database.Migrate(); 
        DBHelper.InitDBForTests(db);
    }
}