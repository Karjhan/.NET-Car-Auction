using GatewayService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.MapReverseProxy();

app.UseAuthentication();

app.UseAuthorization();

app.Run();