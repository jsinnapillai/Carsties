using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data
{
    public class AuctionDBContext : DbContext
    {
        public AuctionDBContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Auction> Auctions { get; set; }
        // public DbSet<Item> Items { get; set; }
    }
}