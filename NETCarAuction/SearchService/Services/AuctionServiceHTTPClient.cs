using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionServiceHTTPClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuctionServiceHTTPClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<Item>> GetItemsForSearchDB()
    {
        var dateOfLastUpdated = await DB.Find<Item, string>()
            .Sort(sort => sort.Descending(item => item.UpdatedAt))
            .Project(item => item.UpdatedAt.ToString())
            .ExecuteFirstAsync();
        return await _httpClient.GetFromJsonAsync<List<Item>>(_configuration["AuctionServiceURL"] + "/api/auctions?date=" + dateOfLastUpdated);
    }
}