﻿using MongoDB.Entities;

namespace BidService.Models;

public class Auction : Entity
{
    public DateTime AuctionEnd { get; set; }

    public string Seller { get; set; }

    public int ReservePrice { get; set; }

    public bool Finished { get; set; }
}