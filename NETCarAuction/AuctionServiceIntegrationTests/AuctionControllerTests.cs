using System.Net;
using System.Net.Http.Json;
using AuctionService.Data.Contexts;
using AuctionService.DTO;
using AuctionServiceIntegrationTests.Fixtures;
using AuctionServiceIntegrationTests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionServiceIntegrationTests;

[Collection("Shared collection")]
public class AuctionControllerTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private const string _gT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

    public AuctionControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task AuctionController_GetAuctions_ShouldReturn3Auctions()
    {
        //Arrange
        //Act
        var response = await _httpClient.GetFromJsonAsync<List<AuctionDTO>>("api/Auctions");
        //Assert
        Assert.Equal(3, response.Count);
    }
    
    [Fact]
    public async Task AuctionController_GetAuctionById_WithValidId_ShouldReturnAuction()
    {
        // arrange? 

        // act
        var response = await _httpClient.GetFromJsonAsync<AuctionDTO>($"api/auctions/{_gT_ID}");

        // assert
        Assert.Equal("GT", response.Model);
    }

    [Fact]
    public async Task AuctionController_GetAuctionById_WithInvalidId_ShouldReturn404()
    {
        // arrange? 

        // act
        var response = await _httpClient.GetAsync($"api/auctions/{Guid.NewGuid()}");

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AuctionController_GetAuctionById_WithInvalidGuid_ShouldReturn400()
    {
        // arrange? 

        // act
        var response = await _httpClient.GetAsync($"api/auctions/notaguid");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AuctionController_CreateAuction_WithNoAuth_ShouldReturn401()
    {
        // arrange? 
        var auction = new CreateAuctionDTO { Make = "test" };

        // act
        var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuctionController_CreateAuction_WithAuth_ShouldReturn201()
    {
        // arrange? 
        var auction = GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // act
        var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

        // assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDTO>();
        Assert.Equal("bob", createdAuction.Seller);
    }

    [Fact]
    public async Task AuctionController_CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
    {
        // arrange? 
        var auction = GetAuctionForCreate();
        auction.Make = null;
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // act
        var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AuctionController_UpdateAuction_WithWrongUpdateDtoOrUser_ShouldReturn400()
    {
        // arrange? 
        var updateAuction = new UpdateAuctionDTO { Make = "Updated" };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // act
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{_gT_ID}", updateAuction);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AuctionController_UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
    {
        // arrange? 
        var updateAuction = new UpdateAuctionDTO { Make = "Updated" };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("notbob"));

        // act
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{_gT_ID}", updateAuction);

        // assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuctionDBContext>();
            DBHelper.ReinitDBForTests(db);
        }
        return Task.CompletedTask;
    }
    
    private static CreateAuctionDTO GetAuctionForCreate()
    {
        return new CreateAuctionDTO
        {
            Make = "test",
            Model = "testModel",
            ImageURL = "test",
            Color = "test",
            Mileage = 10,
            Year = 10,
            ReservePrice = 10
        };
    }
}