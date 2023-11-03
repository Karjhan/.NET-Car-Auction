using System.Net;
using Polly;
using Polly.Extensions.Http;
using SearchService.Services;

namespace AuctionService.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add auction service http service
        services.AddHttpClient<AuctionServiceHTTPClient>().AddPolicyHandler(GetPolicy());
        
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