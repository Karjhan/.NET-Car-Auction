using AuctionService.Data.Contexts;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly AuctionDBContext _context;

    public BidPlacedConsumer(AuctionDBContext context)
    {
        _context = context;
    }
    
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> Consuming bid placed!");
        var auction = await _context.Auctions.FindAsync(context.Message.AuctionId);
        if (auction.CurrentHighBid == null || (context.Message.BidStatus.Contains("Accepted") &&
                                               context.Message.Amount > auction.CurrentHighBid))
        {
            auction.CurrentHighBid = context.Message.Amount;
            await _context.SaveChangesAsync();
        }
    }
}