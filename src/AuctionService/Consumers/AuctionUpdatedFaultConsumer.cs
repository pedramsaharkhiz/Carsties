using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
    public class AuctionUpdatedFaultConsumer : IConsumer<Fault<AuctionUpdated>>
    {
        public async Task Consume(ConsumeContext<Fault<AuctionUpdated>> context)
        {
            Console.WriteLine("--> Consuming faulty updatation");
            var exception = context.Message.Exceptions.First();
            if (exception.ExceptionType == "System.ArgumentException" &&
            context.Message.Message.Year < 1960)
            {
                context.Message.Message.Year = 1960;
                await context.Publish(context.Message.Message);
            }
            else
            {
                Console.WriteLine("Not an Argument Exception - update error dashboard somewhere");
            }
        }
    }
}