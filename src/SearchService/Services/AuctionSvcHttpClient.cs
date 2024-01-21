using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
    public class AuctionSvcHttpClient(HttpClient http, IConfiguration config)
    {
        private readonly HttpClient _http = http;
        private readonly IConfiguration _config = config;
        public async Task<IEnumerable<Item>> GetItemsForSearchDb()
        {
            var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(a => a.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())//we need to project the date of last update in string
            .ExecuteFirstAsync();
            return await _http.GetFromJsonAsync<IEnumerable<Item>>(_config["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdated);
        }
    }
}