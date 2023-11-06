using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace GatewayService.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add reverse proxy YARP
        services.AddReverseProxy().LoadFromConfig(configuration.GetSection("ReverseProxy"));
        // Add authentication with jwt token
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.Authority = configuration["IdentityServiceURL"];
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.NameClaimType = "username";
        });
        
        return services;
    }
}