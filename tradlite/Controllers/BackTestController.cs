using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models.Backtesting;
using Tradlite.Models.Requests;
using Tradlite.Services.Candle.CandleService;
using Tradlite.Services.Management;
using Tradlite.Services.Signals;
using Trady.Analysis.Extension;

namespace Tradlite.Controllers
{
    public class BacktestController : Controller
    {
        private readonly ICandleService _candleService;
        private readonly IMovingAverageService _movingAverageService;
        private readonly Func<string, IManagement> _managementAccessor;
        public BacktestController(ICandleService candleService, IMovingAverageService movingAverageService, Func<string, IManagement> managementAccessor)
        {
            _candleService = candleService;
            _movingAverageService = movingAverageService;
            _managementAccessor = managementAccessor;
        }

        [Route("api/backtest/tickertest")]
        public async Task<BacktestResult> TickerTest(BacktestRequest request)
        {
            return new BacktestResult(new List<Transaction>());
        }

        [Route("api/backtest/testing")]
        public async Task<BacktestResult> Testing()
        {
            var candles = await _candleService.GetCandles(new Models.Requests.CandleRequest
            {
                Interval = "DAY",
                Importer = "Yahoo",
                Ticker = "TSLA"
            });

            var atrList = candles.Atr(14);
            
            var buyParameters = "{\"period1\":\"8\", \"period2\":\"13\"}";
            var buySignals = _movingAverageService.SmaCross(candles, buyParameters);
            var sellParameters = "{\"period1\":\"13\", \"period2\":\"8\"}";
            var sellSignals = _movingAverageService.SmaCross(candles, sellParameters);

            var positions = new List<Position>();
            var transactions = new List<Transaction>();
            Position currentPosition = null;
            var currentIndex = 0;
            while(currentIndex <= candles.Count-1)
            {
                var candle = candles[currentIndex];
                if(currentPosition != null)
                {
                    if(currentPosition.Direction == OrderDirection.Buy)
                    {
                        if(candle.High >= currentPosition.Limit)
                        {
                            transactions.Add(new Transaction
                            {
                                Direction = OrderDirection.Buy.ToString(),
                                EntryDate = currentPosition.Created,
                                EntryLevel = currentPosition.EntryLevel,
                                ExitLevel = currentPosition.Limit,
                                ExitDate = candle.DateTime.LocalDateTime,
                                Size = currentPosition.Size
                            });
                            currentPosition = null;
                            currentIndex++;
                            continue;
                        }
                        else if(candle.Low <= currentPosition.Stop)
                        {
                            transactions.Add(new Transaction
                            {
                                Direction = OrderDirection.Buy.ToString(),
                                EntryDate = currentPosition.Created,
                                EntryLevel = currentPosition.EntryLevel,
                                ExitLevel = currentPosition.Stop,
                                ExitDate = candle.DateTime.LocalDateTime,
                                Size = currentPosition.Size
                            });
                            currentPosition = null;
                            currentIndex++;
                            continue;
                        }
                    }
                    if (currentPosition.Direction == OrderDirection.Sell)
                    {
                        if (candle.Low <= currentPosition.Limit)
                        {
                            transactions.Add(new Transaction
                            {
                                Direction = OrderDirection.Sell.ToString(),
                                EntryDate = currentPosition.Created,
                                EntryLevel = currentPosition.EntryLevel,
                                ExitLevel = currentPosition.Limit,
                                ExitDate = candle.DateTime.LocalDateTime,
                                Size = currentPosition.Size
                            });
                            currentPosition = null;
                            currentIndex++;
                            continue;
                        }
                        else if (candle.High >= currentPosition.Stop)
                        {
                            transactions.Add(new Transaction
                            {
                                Direction = OrderDirection.Sell.ToString(),
                                EntryDate = currentPosition.Created,
                                EntryLevel = currentPosition.EntryLevel,
                                ExitLevel = currentPosition.Stop,
                                ExitDate = candle.DateTime.LocalDateTime,
                                Size = currentPosition.Size
                            });
                            currentPosition = null;
                            currentIndex++;
                            continue;
                        }
                    }
                    currentIndex++;
                    continue;
                }

                if(buySignals.Any(s => s == currentIndex))
                {
                    var limit = _managementAccessor("AverageTrueRange").BuyLimit(candles, currentIndex, "");
                    var stop = _managementAccessor("AverageTrueRange").BuyStop(candles, currentIndex, "");

                    if (currentIndex + 1 > candles.Count - 1 || !limit.HasValue || !stop.HasValue)
                    {
                        currentIndex++;
                        continue;
                    }
                    var nextCandle = candles[currentIndex + 1];

                    currentPosition = new Position
                    {
                        Direction = OrderDirection.Buy,
                        EntryLevel = nextCandle.Open,
                        Created = nextCandle.DateTime.LocalDateTime,
                        Size = 1,
                        Limit = limit.Value,
                        Stop = stop.Value
                    };
                }

                if (sellSignals.Any(s => s == currentIndex))
                {
                    var limit = _managementAccessor("AverageTrueRange").SellLimit(candles, currentIndex, "");
                    var stop = _managementAccessor("AverageTrueRange").SellStop(candles, currentIndex, "");
                    if (currentIndex + 1 > candles.Count - 1 || !limit.HasValue || !stop.HasValue)
                    {
                        currentIndex++;
                        continue;
                    }
                    var nextCandle = candles[currentIndex + 1];
                    currentPosition = new Position
                    {
                        Direction = OrderDirection.Sell,
                        EntryLevel = nextCandle.Open,
                        Created = nextCandle.DateTime.LocalDateTime,
                        Size = 1,
                        Limit = limit.Value,
                        Stop = stop.Value
                    };
                }

                currentIndex++;
            }
            

            return new BacktestResult(transactions);
        }
    }
}
