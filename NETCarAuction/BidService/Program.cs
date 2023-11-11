using BidService.Extensions;
using MongoDB.Driver;
using MongoDB.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwaggerDocumentation();

app.UseAuthorization();

app.MapControllers();

await DB.InitAsync("BidDB", MongoClientSettings.FromConnectionString(builder.Configuration.GetConnectionString("BidDBConnection")));

app.Run();