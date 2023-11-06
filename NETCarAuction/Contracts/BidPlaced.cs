namespace Contracts;

public class BidPlaced : BaseEvent
{
    public string AuctionId { get; set; }

    public string Bidder { get; set; }

    public DateTime BidTime { get; set; }

    public int Amount { get; set; }

    public string BidStatus { get; set; }
}