using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class AuctionUpdatedConsumer(IMapper mapper) : IConsumer<AuctionUpdated>
    {
        private readonly IMapper _mapper = mapper;

        public async Task Consume(ConsumeContext<AuctionUpdated> context)
        {
            Console.WriteLine("--> Consuming Auction Updated : " + context.Message.Id);
            var itemToUpdate = _mapper.Map<Item>(context.Message);
            if (context.Message.Year < 1960) throw new ArgumentException("You cannot Sell Cars Year smaller than 1960");
            var result = await DB.Update<Item>()
                    .MatchID(context.Message.Id)
                    .ModifyOnly(a => new { a.Make, a.Model, a.Year, a.Color, a.Mileage }, itemToUpdate)
                    .ExecuteAsync();
            if (!result.IsAcknowledged)
                throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb");
        }
    }
}