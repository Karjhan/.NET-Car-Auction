﻿using AuctionService.Data.Contexts;
using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

public class AuctionsController : BaseAPIController
{
    private readonly AuctionDBContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(AuctionDBContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
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

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDTO>> CreateAuction([FromBody] CreateAuctionDTO auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        
        auction.Seller = User.Identity.Name;
        
        _context.Auctions.Add(auction);
        var newAuction = _mapper.Map<AuctionDTO>(auction);
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));
        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
        {
            return BadRequest("Could not save changes to the database!");
        }
        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, [FromBody] UpdateAuctionDTO updateAuctionDto)
    {
        var auction = await _context.Auctions.Include(auction => auction.Item).FirstOrDefaultAsync(auction => auction.Id == id);
        if (auction is null)
        {
            return NotFound();
        }

        if (auction.Seller != User.Identity.Name)
        {
            return Forbid();
        }

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
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
        
        if (auction.Seller != User.Identity.Name)
        {
            return Forbid();
        }
        
        _context.Auctions.Remove(auction);
        await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });
        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
        {
            return BadRequest("Could not update DB");
        }
        return Ok();
    }
}