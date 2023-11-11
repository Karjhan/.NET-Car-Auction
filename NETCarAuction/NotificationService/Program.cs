using NotificationService.Extensions;
using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.MapHub<NotificationHub>("/notifications");

app.Run();