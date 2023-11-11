using AutoMapper;
using BidService.DTO;
using BidService.Models;
using Contracts;

namespace BidService.Helpers;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Bid, BidDTO>();
        CreateMap<Bid, BidPlaced>();
    }
}