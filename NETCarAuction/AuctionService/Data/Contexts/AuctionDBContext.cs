using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data.Contexts;

public class AuctionDBContext : DbContext
{
    public DbSet<Auction> Auctions { get; set; }
    
    public AuctionDBContext(DbContextOptions options) : base(options)
    {
        
    }
}