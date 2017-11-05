using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models;
using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Tradlite.Controllers
{
    public class TickerController : Controller
    {
        private readonly IDbConnection _dbConnection;

        public TickerController(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
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

        [Route("api/tickerlist/addticker/{tickerListId}/{tickerId}")]
        public async Task AddTickerToTickerList(int tickerListId, int tickerId)
        {
            var sql =
                "insert into tickerListTicker(tickerListId, tickerId) " +
                "values (@tickerListId, @tickerId)";
            await _dbConnection.ExecuteAsync(sql, new { tickerListId, tickerId });
        }

        [Route("api/tickerlist/removeticker/{tickerListId}/{tickerId}")]
        public async Task RemoveTickerFromTickerList(int tickerListId, int tickerId)
        {
            var sql =
                "delete from tickerListTicker " +
                "where tickerListId = @tickerListId and tickerId = @tickerId";
            await _dbConnection.ExecuteAsync(sql, new { tickerListId, tickerId });
        }
    }
}
