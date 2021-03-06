﻿using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Threading.Tasks;
using Tradlite.Models.Backtesting;
using Tradlite.Models.Requests;
using Tradlite.Services.Candle.CandleService;
using Tradlite.Services.Ig;
using Dapper.Contrib.Extensions;
using Tradlite.Services.Backtest;
using System;
using Dapper;
using Tradlite.Models;
using System.Linq;
using System.Collections.Generic;
using Tradlite.Services.SqlConnectionFactory;

namespace Tradlite.Controllers
{
    public class BacktestController : Controller
    {
        private readonly ICandleService _candleService;
        private readonly IIgService _igService;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly IBacktestService _backtestService;

        public BacktestController(ICandleService candleService,
            IIgService igService,
            ISqlConnectionFactory sqlConnectionFactory,
            IBacktestService backtestService)
        {
            _candleService = candleService;
            _igService = igService;
            _sqlConnectionFactory = sqlConnectionFactory;
            _backtestService = backtestService;
        }

        [Route("api/backtest/ticker")]
        public async Task<BacktestResult> BacktestTicker([FromQuery]BacktestTickerRequest request)
        {
            using (var _dbConnection = _sqlConnectionFactory.CreateConnection())
            {
                var candles = await _candleService.GetCandles(request);
                var backtestConfig = _dbConnection.Get<BacktestConfig>(request.BacktestConfigId);

                var marketDetails = await _igService.GetMarketDetails(request.Ticker);
                var minSize = marketDetails.snapshot.scalingFactor * marketDetails.instrument.lotSize.Value * marketDetails.dealingRules.minDealSize.value.Value;
                var exchangeRate = 1.0m / marketDetails.instrument.currencies[0].baseExchangeRate.Value;

                var transactions = await _backtestService.Run(candles, backtestConfig, minSize, request.Ticker, exchangeRate, request.Risk);

                return new BacktestResult(transactions, request.CurrentCapital.HasValue ? request.CurrentCapital.Value : 100000);
            }
        }

        [Route("api/backtest/tickerlist")]
        public async Task<BacktestResult> BacktestTickerlist([FromQuery]BacktestTickerListRequest request)
        {
            using (var _dbConnection = _sqlConnectionFactory.CreateConnection())
            {
                if (!request.LongBacktestConfigId.HasValue && !request.ShortBacktestConfigId.HasValue)
                {
                    throw new ArgumentException("at least one backtest config id is required");
                }

                var sql =
                    "select * from tickers t " +
                    "inner join tickerLists_Tickers tlt on tlt.tickerId = t.id " +
                    "where tlt.tickerListId = @tickerListId";
                var tickers = (await _dbConnection.QueryAsync<Ticker>(sql, new { request.TickerListId })).ToList();
                BacktestConfig longBacktestConfig = null;
                if (request.LongBacktestConfigId.HasValue)
                {
                    longBacktestConfig = _dbConnection.Get<BacktestConfig>(request.LongBacktestConfigId);
                }

                BacktestConfig shortBacktestConfig = null;
                if (request.ShortBacktestConfigId.HasValue)
                {
                    shortBacktestConfig = _dbConnection.Get<BacktestConfig>(request.ShortBacktestConfigId);
                }

                var tasks = new List<Task<List<Transaction>>>();
                foreach (var ticker in tickers)
                {
                    var marketDetails = await _igService.GetMarketDetails(ticker.Symbol);
                    var size = marketDetails.snapshot.scalingFactor * marketDetails.instrument.lotSize.Value;
                    var exchangeRate = 1.0m / marketDetails.instrument.currencies[0].baseExchangeRate.Value;
                    var candles = await _candleService.GetCandles(new CandleRequest
                    {
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        Importer = ticker.Importer,
                        Interval = request.Interval,
                        Ticker = ticker.Symbol
                    });
                    if (longBacktestConfig != null)
                    {
                        tasks.Add(_backtestService.Run(candles, longBacktestConfig, size, ticker.Symbol, exchangeRate, request.Risk));
                        //var backtestTransactions = await _backtestService.Run(candles, longBacktestConfig, size, ticker.Symbol, exchangeRate, request.Risk);
                        //transactions.AddRange(backtestTransactions);
                    }

                    if (shortBacktestConfig != null)
                    {
                        tasks.Add(_backtestService.Run(candles, shortBacktestConfig, size, ticker.Symbol, exchangeRate, request.Risk));
                        //var backtestTransactions = await _backtestService.Run(candles, shortBacktestConfig, size, ticker.Symbol, exchangeRate, request.Risk);
                        //transactions.AddRange(backtestTransactions);
                    }
                }
                var transactions = new List<Transaction>();
                var results = await Task.WhenAll(tasks);
                foreach (var result in results)
                {
                    transactions.AddRange(result);
                }
                return new BacktestResult(transactions, 100000);
            }
        }

        [Route("api/backtestswedish/tickerlist")]
        public async Task<BacktestResult> BacktestTickerlist2([FromQuery]BacktestTickerListRequest request)
        {
            using (var _dbConnection = _sqlConnectionFactory.CreateConnection())
            {
                if (!request.LongBacktestConfigId.HasValue && !request.ShortBacktestConfigId.HasValue)
                {
                    throw new ArgumentException("at least one backtest config id is required");
                }

                var sql =
                    "select * from tickers t " +
                    "inner join tickerLists_Tickers tlt on tlt.tickerId = t.id " +
                    "where tlt.tickerListId = @tickerListId";
                var tickers = (await _dbConnection.QueryAsync<Ticker>(sql, new { request.TickerListId })).ToList();
                BacktestConfig longBacktestConfig = null;
                if (request.LongBacktestConfigId.HasValue)
                {
                    longBacktestConfig = _dbConnection.Get<BacktestConfig>(request.LongBacktestConfigId);
                }

                BacktestConfig shortBacktestConfig = null;
                if (request.ShortBacktestConfigId.HasValue)
                {
                    shortBacktestConfig = _dbConnection.Get<BacktestConfig>(request.ShortBacktestConfigId);
                }

                var transactions = new List<Transaction>();
                foreach (var ticker in tickers)
                {
                    var candles = await _candleService.GetCandles(new CandleRequest
                    {
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        Importer = ticker.Importer,
                        Interval = request.Interval,
                        Ticker = ticker.Symbol
                    });
                    if (longBacktestConfig != null)
                    {

                        var backtestTransactions = await _backtestService.Run(candles, longBacktestConfig, 1, ticker.Symbol, 1, request.Risk);
                        transactions.AddRange(backtestTransactions);
                    }

                    if (shortBacktestConfig != null)
                    {
                        var backtestTransactions = await _backtestService.Run(candles, shortBacktestConfig, 1, ticker.Symbol, 1, request.Risk);
                        transactions.AddRange(backtestTransactions);
                    }

                }
                return new BacktestResult(transactions, 100000);
            }
        }
    }
}
