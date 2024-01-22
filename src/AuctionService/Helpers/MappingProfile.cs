using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
            CreateMap<Item, AuctionDto>();

            //because we have car props in CreateAuctionDto and have to map it to item from Auction so that we should use ForMember
            CreateMap<CreateAuctionDto, Auction>().ForMember(d => d.Item, o => o.MapFrom(s => s)); //d id for destination & o is for option & s is for source
            CreateMap<CreateAuctionDto,Item>();
            CreateMap<AuctionDto,AuctionCreated>();
            CreateMap<AuctionDto,AuctionUpdated>();
        }
    }
}
