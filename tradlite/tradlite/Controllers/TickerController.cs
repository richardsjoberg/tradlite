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

namespace Tradlite.Controllers
{
    public class TickerController : Controller
    {
        private readonly IDbConnection _dbConnection;
        private readonly IHttpService _httpService;

        public TickerController(IDbConnection dbConnection, IHttpService httpService)
        {
            _dbConnection = dbConnection;
            _httpService = httpService;
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
                "inner join tickerListTicker tlt on tlt.tickerId = t.id " +
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
                "insert into tickerListTicker(tickerListId, tickerId) " +
                "values (@tickerListId, @tickerId)";
            await _dbConnection.ExecuteAsync(sql, new { tickerListId, tickerId });
        }

        [HttpDelete]
        [Route("api/tickerlist/removeticker/{tickerListId}/{tickerId}")]
        public async Task RemoveTickerFromTickerList(int tickerListId, int tickerId)
        {
            var sql =
                "delete from tickerListTicker " +
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
                "insert into tickerListTicker(tickerListId, tickerId) " +
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
    }
}
