using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public class DBInitializer
{
    public static async Task InitDB(WebApplication app)
    {
        // Initialize mongo database
        await DB.InitAsync("SearchDB", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDBConnection")));

        // Create indexes for some items
        await DB.Index<Item>()
            .Key(item => item.Make, KeyType.Text)
            .Key(item => item.Model, KeyType.Text)
            .Key(item => item.Color, KeyType.Text)
            .CreateAsync();

        // Seed data from auction service request
        using (var scope = app.Services.CreateScope())
        {
            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHTTPClient>();
            var items = await httpClient.GetItemsForSearchDB();
            Console.WriteLine(items.Count + " returned from auction service!");
            if (items.Count > 0)
            {
                await DB.SaveAsync(items);
            }
        }
        
        // Seed data from json file
        // var count = await DB.CountAsync<Item>();
        // if (count == 0)
        // {
        //     Console.WriteLine("There is no item data. Attempting to seed...");
        //     var itemData = await File.ReadAllTextAsync("Data/SeedData/auctions.json");
        //     var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        //     var items = JsonSerializer.Deserialize<List<Item>>(itemData, jsonOptions);
        //     await DB.SaveAsync(items);
        // }
    }
}