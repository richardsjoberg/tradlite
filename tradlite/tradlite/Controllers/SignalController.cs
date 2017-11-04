using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        [Route("api/buysignalindicies")]
        public async Task<int[]> BuySignalIndicies([FromQuery]string ticker, [FromQuery]DateTime? fromDate, [FromQuery]DateTime? toDate, [FromQuery]string importer = "Yahoo")
        {
            var candles = await _candleService.GetCandles(ticker, fromDate, toDate, importer);
            var bullishHarami = new BullishHarami(candles, false, 3);
            var signal = Rule.Create(c => bullishHarami[c.Index].Tick.HasValue && bullishHarami[c.Index].Tick.Value);
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        [Route("api/sellsignalindicies")]
        public async Task<int[]> SellSignalIndicies([FromQuery]string ticker, [FromQuery]DateTime? fromDate, [FromQuery]DateTime? toDate, [FromQuery]string importer = "Yahoo")
        {
            var candles = await _candleService.GetCandles(ticker, fromDate, toDate, importer);
            var bearishHarami = new BearishHarami(candles, false, 3);
            var signal = Rule.Create(c => bearishHarami[c.Index].Tick.HasValue && bearishHarami[c.Index].Tick.Value);
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }
    }
}
