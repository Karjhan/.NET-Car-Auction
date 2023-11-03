using AuctionService.Data.Contexts;
using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

public class AuctionsController : BaseAPIController
{
    private readonly AuctionDBContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDBContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions(string? date)
    {
        // Query for auctions for later than a given data
        var query = _context.Auctions.OrderBy(auction => auction.Item.Make).AsQueryable();
        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(auction => auction.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }
        return await query.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider).ToListAsync();
        // var auctions = await _context.Auctions.Include(auction => auction.Item).OrderBy(auction => auction.Item.Make).ToListAsync();
        // return _mapper.Map<List<AuctionDTO>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions.Include(auction => auction.Item).FirstOrDefaultAsync(auction => auction.Id == id);
        if (auction is null)
        {
            return NotFound();
        }
        return _mapper.Map<AuctionDTO>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDTO>> CreateAuction([FromBody] CreateAuctionDTO auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        
        // future: add user as seller
        auction.Seller = "test";
        
        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
        {
            return BadRequest("Could not save changes to the database!");
        }
        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDTO>(auction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, [FromBody] UpdateAuctionDTO updateAuctionDto)
    {
        var auction = await _context.Auctions.Include(auction => auction.Item).FirstOrDefaultAsync(auction => auction.Id == id);
        if (auction is null)
        {
            return NotFound();
        }
        
        // future: check seller == username

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
        var result = await _context.SaveChangesAsync() > 0;
        if (result)
        {
            return Ok();
        }
        return BadRequest("Problem saving changes!");
    } 
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FirstOrDefaultAsync(auction => auction.Id == id);
        if (auction is null)
        {
            return NotFound();
        }
        
        // future: check seller == username
        
        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
        {
            return BadRequest("Could not update DB");
        }
        return Ok();
    }
}