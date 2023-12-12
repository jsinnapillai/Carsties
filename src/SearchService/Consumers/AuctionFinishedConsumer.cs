using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
    {
        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
             Console.WriteLine("---> Consuming AuctionFinished");
            var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

            if(context.Message.ItemSold)
            {
                auction.Winner = context.Message.Winner;
                auction.SolAmount = (int)context.Message.Amount;
            }

            auction.Status = "Finished";
            await auction.SaveAsync();
        }
    }
}