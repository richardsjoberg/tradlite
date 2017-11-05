using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models.Requests;
using Tradlite.Services.CandleService;
using Trady.Analysis;
using Trady.Analysis.Candlestick;

namespace Tradlite.Controllers
{
    public class SignalController : Controller
    {
        private readonly ICandleService _candleService;

        public SignalController(ICandleService candleService)
        {
            _candleService = candleService;
        }
        [Route("api/bullishharami")]
        public async Task<int[]> BullishHarami([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request.Ticker, request.FromDate, request.ToDate, request.Importer, request.Interval);
            var bullishHarami = new BullishHarami(candles, false, 3);
            var signal = Rule.Create(c => bullishHarami[c.Index].Tick.HasValue && bullishHarami[c.Index].Tick.Value);
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        [Route("api/bearishharami")]
        public async Task<int[]> BearishHarami([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request.Ticker, request.FromDate, request.ToDate, request.Importer, request.Interval);
            var bearishHarami = new BearishHarami(candles, false, 3);
            var signal = Rule.Create(c => bearishHarami[c.Index].Tick.HasValue && bearishHarami[c.Index].Tick.Value);
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        [Route("api/rsioverboughtanduptrend")]
        public async Task<int[]> Overbought([FromQuery]SignalRequest request)
        {
            int uptrendPeriod = 5;
            int rsiPeriod = 14;
            
            if(!string.IsNullOrEmpty(request.ExtraParams))
            {
                var extraParam = JObject.Parse(request.ExtraParams);
                int.TryParse(extraParam["uptrendPeriod"].ToString(), out uptrendPeriod);
                int.TryParse(extraParam["rsiPeriod"].ToString(), out rsiPeriod);
            }
            
            var candles = await _candleService.GetCandles(request.Ticker, request.FromDate, request.ToDate, request.Importer, request.Interval);
            var uptrend = new UpTrend(candles, uptrendPeriod);
            
            var signal = Rule.Create(c => c.IsRsiOverbought(rsiPeriod) && uptrend[c.Index].Tick.HasValue && uptrend[c.Index].Tick.Value);
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }
    }
}
