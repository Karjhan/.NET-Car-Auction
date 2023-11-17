using AuctionService.Entities;

namespace AuctionServiceTests;

public class AuctionEntityTests
{
    [Fact]
    public void Auction_HasReservedPrice_ReservePriceGreaterThanZero_ReturnsTrue()
    {
        //Arrange
        var testAuction = new Auction()
        {
            ReservePrice = 10
        };
        //Act
        var result = testAuction.HasReservePrice();
        //Assert
        Assert.True(result);
    }
    
    [Fact]
    public void Auction_HasReservedPrice_ReservePriceLowerThanZero_ReturnsFalse()
    {
        //Arrange
        var testAuction = new Auction()
        {
            ReservePrice = 0
        };
        //Act
        var result = testAuction.HasReservePrice();
        //Assert
        Assert.False(result);
    }
}