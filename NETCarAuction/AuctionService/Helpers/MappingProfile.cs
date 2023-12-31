﻿using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.Helpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Auction, AuctionDTO>().IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionDTO>();
        CreateMap<CreateAuctionDTO, Auction>()
            .ForMember(destMember => destMember.Item, options => options.MapFrom(source => source));
        CreateMap<CreateAuctionDTO, Item>();
        CreateMap<AuctionDTO, AuctionCreated>();
        CreateMap<Auction, AuctionUpdated>().IncludeMembers(auction => auction.Item);
        CreateMap<Item, AuctionUpdated>();
    }
}