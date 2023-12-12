using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Consumers
{
    public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
    { 
        private readonly AuctionDBContext _dBContext;

        public AuctionFinishedConsumer(AuctionDBContext dBContext)
        {
       
            _dBContext = dBContext;
        }
        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            Console.WriteLine("---> Consuming Auction finished");
            var auction = await _dBContext.Auctions.FindAsync(context.Message.AuctionId);

            if(context.Message.ItemSold)
            {
                auction.Winner = context.Message.Winner;
                auction.SolAmount = context.Message.Amount;
            }

            auction.Status = auction.SolAmount > auction.ReservePrice
            ? Entities.Status.Finished : Entities.Status.ReserveNotMet;

            await _dBContext.SaveChangesAsync();

        }
    }
}