using AuctionService.Data;
using AuctionService.Extensions;
using AuctionService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

// Configure HTTP pipeline
app.UseSwaggerDocumentation();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<GRPCAuctionService>();

try
{
    DBInitializer.InitDB(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();

public partial class Program{}