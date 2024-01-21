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
    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app, ILoggerFactory logger)
        {
            await DB.InitAsync("SearchDb",
            MongoClientSettings.FromConnectionString(
                app.Configuration.GetConnectionString("MongoDbConnection")
            ));

            await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .CreateAsync();

            
            //Fetch Data from Auction Service
            using var scope = app.Services.CreateScope();
            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
            var items = await httpClient.GetItemsForSearchDb();
            logger.CreateLogger<DbInitializer>()
            .LogInformation(items.Count() + " returned from Auction Service");
            if (items.Any())
            {
                await DB.SaveAsync(items);//Seed Data
            }
        }
    }
}