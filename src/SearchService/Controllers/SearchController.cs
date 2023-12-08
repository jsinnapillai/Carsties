using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Quic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchparam )
        {
            var query = DB.PagedSearch<Item, Item>();

            if (!string.IsNullOrEmpty(searchparam.SearchTerm))
            {
                query.Match(Search.Full, searchparam.SearchTerm).SortByTextScore();
            }

            query = searchparam.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(a => a.Make))
                    .Sort(x => x.Ascending(a => a.Model)),
                "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
                _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
            };

            query = searchparam.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6)
                    && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };

            if (!string.IsNullOrEmpty(searchparam.Seller))
            {
                Console.WriteLine("seller");
                query.Match(x => x.Seller == searchparam.Seller);
            }

            if (!string.IsNullOrEmpty(searchparam.Winner))
            {
                         Console.WriteLine("Winner");
                query.Match(x => x.Winner == searchparam.Winner);
            }

            query.PageNumber(searchparam.PageNumber);
            query.PageSize(searchparam.PageSize);

            var result = await query.ExecuteAsync();

            return Ok(new
            {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            });
        }
    }
}