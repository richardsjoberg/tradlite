using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Threading.Tasks;
using Tradlite.Models.Backtesting;
using Tradlite.Models.Requests;
using Tradlite.Services.Candle.CandleService;
using Tradlite.Services.Ig;
using Dapper.Contrib.Extensions;
using Tradlite.Services.Backtest;

namespace Tradlite.Controllers
{
    public class BacktestController : Controller
    {
        private readonly ICandleService _candleService;
        private readonly IIgService _igService;
        private readonly IDbConnection _dbConnection;
        private readonly IBacktestService _backtestService;

        public BacktestController(ICandleService candleService, 
            IIgService igService,
            IDbConnection dbConnection,
            IBacktestService backtestService)
        {
            _candleService = candleService;
            _igService = igService;
            _dbConnection = dbConnection;
            _backtestService = backtestService;
        }

        [Route("api/backtest/run")]
        public async Task<BacktestResult> RunBacktest([FromQuery]BacktestRequest backtestRequest)
        {
            var candles = await _candleService.GetCandles(backtestRequest);
            var backtestConfig = _dbConnection.Get<BacktestConfig>(backtestRequest.BacktestConfigId);
            
            var marketDetails = await _igService.GetMarketDetails(backtestRequest.Ticker);
            var size = marketDetails.snapshot.scalingFactor * marketDetails.instrument.lotSize.Value;
            var exchangeRate = 1.0m / marketDetails.instrument.currencies[0].baseExchangeRate.Value;

            var transactions = await _backtestService.Run(candles, backtestConfig, size, backtestRequest.Ticker);

            return new BacktestResult(transactions, backtestConfig.InitialCapital, exchangeRate);
        }
    }
}
