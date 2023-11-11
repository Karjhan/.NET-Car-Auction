using AuctionService.Data.Contexts;
using Grpc.Core;

namespace AuctionService.Services;

public class GRPCAuctionService : GrpcAuction.GrpcAuctionBase
{
    private readonly AuctionDBContext _dbContext;

    public GRPCAuctionService(AuctionDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, ServerCallContext context)
    {
        Console.WriteLine("==> Received GRPC request for auction");

        var auction = await _dbContext.Auctions.FindAsync(Guid.Parse(request.Id));
        if (auction is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Not found!"));
        }
        var response = new GrpcAuctionResponse
        {
            Auction = new GrpcAuctionModel
            {
                AuctionEnd = auction.AuctionEnd.ToString(),
                Id = auction.Id.ToString(),
                ReservePrice = auction.ReservePrice,
                Seller = auction.Seller
            }
        };
        return response;
    }
}