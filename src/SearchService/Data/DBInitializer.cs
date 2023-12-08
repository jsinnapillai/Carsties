using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data
{
    public class DBInitializer
    {
        public static async Task IntitDb(WebApplication app)
        {
            await DB.InitAsync("SearchDB", MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDBConnection")));

            await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

            var count = await DB.CountAsync<Item>();

            using var scope = app.Services.CreateScope();
            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();

            var items = await httpClient.GetItemsforSearchDB();
            Console.WriteLine(items.Count);

            if(items.Count >0 ) await DB.SaveAsync(items);

            // if(count==0)
            // {
            //     Console.WriteLine("No Data - will attempt to seed");
            //     var itemData = await File.ReadAllBytesAsync("Data/auctions.json");
            //     var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};
            //     var items = JsonSerializer.Deserialize<List<Item>>(itemData,options);
            //     await  DB.SaveAsync(items);
            // }
        }
    }
}