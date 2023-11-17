using System.Net;
using System.Net.Http.Json;
using AuctionService.Data.Contexts;
using AuctionService.DTO;
using AuctionServiceIntegrationTests.Fixtures;
using AuctionServiceIntegrationTests.Utils;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionServiceIntegrationTests;

[Collection("Shared collection")]
public class AuctionBusTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private ITestHarness _testHarness;

    public AuctionBusTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
        _testHarness = factory.Services.GetTestHarness();
    }

    [Fact]
    public async Task AuctionController_CreateAuction_WithValidObject_ShouldPublishAuctionCreated()
    {
        //Arrange
        var auction = GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        //Act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);
        //Assert
        response.EnsureSuccessStatusCode();
        Assert.True(await _testHarness.Published.Any<AuctionCreated>());
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