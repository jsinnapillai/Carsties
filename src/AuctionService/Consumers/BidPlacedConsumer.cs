using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
    public class BidPlacedConsumer : IConsumer<BidPlaced>
    {
        private readonly AuctionDBContext _dBContext;

        public BidPlacedConsumer(AuctionDBContext dBContext)
        {
            _dBContext = dBContext;
        }
        public async Task Consume(ConsumeContext<BidPlaced> context)
        {
            Console.WriteLine("---> Consuming bid placed");
             var auction = await _dBContext.Auctions.FindAsync(context.Message.AuctionId);

             if((auction.CurrentHighBid == null 
            //  || context.Message.BidStatus,Contains("Accepted") 
             )
             &&  context.Message.Amount > auction.CurrentHighBid )
             {
                    auction.CurrentHighBid = context.Message.Amount;
                    await _dBContext.SaveChangesAsync();
             }

        }
    }
}