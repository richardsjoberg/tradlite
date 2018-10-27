using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models;
using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using HtmlAgilityPack;
using System.Net;
using Tradlite.Services;
using System.Data.SqlClient;
using Tradlite.Services.Ig;

namespace Tradlite.Controllers
{
    public class TickerController : Controller
    {
        private readonly IDbConnection _dbConnection;
        private readonly IHttpService _httpService;
        private readonly IIgService _igService;

        public TickerController(IDbConnection dbConnection, IHttpService httpService, IIgService igService)
        {
            _dbConnection = dbConnection;
            _httpService = httpService;
            _igService = igService;
        }

        [HttpGet]
        [Route("api/tickerlist")]
        public async Task<IReadOnlyList<TickerList>> GetTickerLists()
        {
            var tickerLists = await _dbConnection.GetAllAsync<TickerList>();
            return tickerLists.ToList();
        }

        [HttpGet]
        [Route("api/ticker")]
        public async Task<IReadOnlyList<Ticker>> GetTickers()
        {
            var tickers = await _dbConnection.GetAllAsync<Ticker>();
            return tickers.ToList();
        }

        [HttpGet]
        [Route("api/ticker/{tickerListId}")]
        public async Task<IReadOnlyList<Ticker>> GetTickersByTickerListId(int tickerListId)
        {
            var sql =
                "select * from tickers t " +
                "inner join tickerLists_Tickers tlt on tlt.tickerId = t.id " +
                "where tlt.tickerListId = @tickerListId";
            var tickers = await _dbConnection.QueryAsync<Ticker>(sql, new { tickerListId });
            return tickers.ToList();
        }

        [HttpPost]
        [Route("api/ticker")]
        public async Task CreateTicker([FromBody]Ticker ticker)
        {
            await _dbConnection.InsertAsync(ticker);
        }

        [Route("api/ticker")]
        [HttpPut]
        public async Task UpdateTicker([FromBody]Ticker signalConfig)
        {
            await _dbConnection.UpdateAsync(signalConfig);
        }

        [Route("api/ticker/{id}")]
        [HttpDelete]
        public async Task DeleteTicker(int id)
        {
            await _dbConnection.DeleteAsync(new Ticker { Id = id });
        }

        [HttpPost]
        [Route("api/tickerList")]
        public async Task CreateTickerList([FromBody]TickerList tickerList)
        {
            await _dbConnection.InsertAsync(tickerList);
        }

        [Route("api/tickerList")]
        [HttpPut]
        public async Task UpdateTickerList([FromBody]TickerList tickerList)
        {
            await _dbConnection.UpdateAsync(tickerList);
        }

        [Route("api/tickerList/{id}")]
        [HttpDelete]
        public async Task DeleteTickerList(int id)
        {
            await _dbConnection.DeleteAsync(new TickerList { Id = id });
        }

        [HttpPost]
        [Route("api/tickerlist/addticker/{tickerListId}/{tickerId}")]
        public async Task AddTickerToTickerList(int tickerListId, int tickerId)
        {
            var sql =
                "insert into tickerLists_tickers(tickerListId, tickerId) " +
                "values (@tickerListId, @tickerId)";
            await _dbConnection.ExecuteAsync(sql, new { tickerListId, tickerId });
        }

        [HttpDelete]
        [Route("api/tickerlist/removeticker/{tickerListId}/{tickerId}")]
        public async Task RemoveTickerFromTickerList(int tickerListId, int tickerId)
        {
            var sql =
                "delete from tickerLists_Tickers " +
                "where tickerListId = @tickerListId and tickerId = @tickerId";
            await _dbConnection.ExecuteAsync(sql, new { tickerListId, tickerId });
        }
        
        [Route("api/tickerlist/{tickertag}")]
        public async Task<string> CreateTickerListFromTickerTag(string tickerTag)
        {
            int tickerListId = await _dbConnection.InsertAsync(new TickerList { Name = tickerTag });
            var sql = "select * from tickers where tags = @tickerTag";
            var tickers = await _dbConnection.QueryAsync<Ticker>(sql, new { tickerTag });
            var insertSql = 
                "insert into tickerLists_Tickers(tickerListId, tickerId) " +
                "values (@tickerListId, @tickerId)";

            await _dbConnection.ExecuteAsync(insertSql, tickers.Select(t => new { tickerListId, tickerId = t.Id }).ToList());

            return $"tickerlist {tickerTag} created";
        }

        [Route("api/ticker/import/cnbc/{index}")]
        public async Task<string> ImportCnbcTickers(string index) // index = dow-components or nasdaq-100 or iq-100-real-time-quotes
        {
            var web = new HtmlWeb();
            var doc = web.Load($"https://www.cnbc.com/{index}/");
            var symbols = doc.DocumentNode.Descendants("td").Where(td => td.Attributes["data-field"].Value == "symbol").Select(td => td.ChildNodes.First().InnerHtml).ToList();
            var symbolString = string.Join('|', symbols);
            var url = $"https://quote.cnbc.com/quote-html-webservice/quote.htm?output=json&symbols={symbolString}";
            var response = await _httpService.Get(url);
            var quotes = response["QuickQuoteResult"]["QuickQuote"];
            foreach(var quote in quotes)
            {
                var ticker = new Ticker
                {
                    Symbol = quote["symbol"].ToString(),
                    Name = quote["name"]?.ToString() ?? quote["symbol"].ToString(),
                    Importer = "Yahoo",
                    Tags = index
                };

                _dbConnection.Insert(ticker);
            }

            return $"{symbols.Count} symbols imported";
        }

        [Route("api/ticker/import/ig/watchlist/{watchlist}")] //whitespace %20
        public async Task<string> ImportIgTickersFromWatchList(string watchlist)
        {
            var client = await _igService.GetIgClient();
            var watchlistsResponse = await client.listOfWatchlists();
            var list = watchlistsResponse.Response.watchlists.First(wl => wl.name.ToLower() == watchlist.ToLower());
            var instrumentResponse = await client.instrumentsForWatchlist(list.id);
            foreach (var market in instrumentResponse.Response.markets)
            {
                var ticker = new Ticker
                {
                    Symbol = market.epic,
                    Name = market.instrumentName,
                    Importer = "Ig",
                    Tags = watchlist.Replace(" ", "")
                };

                _dbConnection.Insert(ticker);
            }
            return $"{instrumentResponse.Response.markets.Count} tickers imported";
        }

        [Route("api/ticker/import/ig/node/{nodeId}/{tickerListName}")]
        public async Task<string> ImportIgTickersFromNodeId(string nodeId, string tickerListName)
        {
            var client = await _igService.GetIgClient();
            var importedTickerCount = ImportIgTickers(client, nodeId, tickerListName);

            return $"{importedTickerCount} tickers imported";
        }

        [Route("api/ticker/import/ig/nodes/{nodeIds}/{tickerListName}")]
        public async Task<string> ImportIgTickersFromNodeIds(string nodeIds, string tickerListName)
        {
            var addedTickerCount = 0;
            var nodeIdsArray = nodeIds.Split('_');
            var client = await _igService.GetIgClient();
            foreach (var nodeId in nodeIdsArray)
            {
                var importedTickerCount = await ImportIgTickers(client, nodeId, tickerListName);
                addedTickerCount += importedTickerCount;
            }
            
            return $"{addedTickerCount} tickers imported";
        }

        private async Task<int> ImportIgTickers(IGWebApiClient.IgRestApiClient client, string nodeId, string tickerListName)
        {
            var response = await client.browse(nodeId);
            foreach (var market in response.Response.markets)
            {
                var ticker = new Ticker
                {
                    Symbol = market.epic,
                    Name = market.instrumentName,
                    Importer = "Ig",
                    Tags = tickerListName
                };

                _dbConnection.Insert(ticker);
            }
            return response.Response.markets.Count;
        }
    }
}
