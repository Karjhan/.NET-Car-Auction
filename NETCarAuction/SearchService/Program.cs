using AuctionService.Extensions;
using SearchService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

// Configure HTTP pipeline
app.UseSwaggerDocumentation();

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    // Attempt to create, index and seed data
    try
    {
        await DBInitializer.InitDB(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
});

app.Run();