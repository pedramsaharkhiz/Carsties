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
    public class AuctionCreatedConsumer(IMapper mapper) : IConsumer<AuctionCreated>
    {
        private readonly IMapper _mapper = mapper;

        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            Console.WriteLine("--> Consuming Auction Created : " + context.Message.Id);
            var item = _mapper.Map<Item>(context.Message);
            if (context.Message.Model == "Foo") throw new ArgumentException("You cannot Sell Cars name of Foo");
            await item.SaveAsync();
        }
    }
}