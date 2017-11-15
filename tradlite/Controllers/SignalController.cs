using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models.Requests;
using Tradlite.Services.Candle.CandleService;
using Trady.Analysis;
using Trady.Analysis.Candlestick;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;
using Tradlite.Services.Signals;

namespace Tradlite.Controllers
{
    public class SignalController : Controller
    {
        private readonly ICandleService _candleService;
        private readonly IMdiPdiService _mdiPdiService;
        private readonly IRsiService _rsiService;
        private readonly IZigZagService _zigZagService;

        public SignalController(ICandleService candleService, IMdiPdiService mdiPdiService, IRsiService rsiService, IZigZagService zigZagService)
        {
            _candleService = candleService;
            _mdiPdiService = mdiPdiService;
            _rsiService = rsiService;
            _zigZagService = zigZagService;
        }
        [Route("api/signal/bullishharami")]
        public async Task<int[]> BullishHarami([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var bullishHarami = new BullishHarami(candles, false, 3);
            var signal = Rule.Create(c => bullishHarami[c.Index].Tick.HasValue && bullishHarami[c.Index].Tick.Value);
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        [Route("api/signal/bearishharami")]
        public async Task<int[]> BearishHarami([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var bearishHarami = new BearishHarami(candles, false, 3);
            var signal = Rule.Create(c => bearishHarami[c.Index].Tick.HasValue && bearishHarami[c.Index].Tick.Value);
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        [Route("api/signal/rsioverbought")]
        public async Task<int[]> RsiOverbought([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _rsiService.Overbought(candles, request.ExtraParams);
            return indicies;
        }

        [Route("api/signal/rsioversold")]
        public async Task<int[]> RsiOversold([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _rsiService.Oversold(candles, request.ExtraParams);
            return indicies;
        }

        [Route("api/signal/mdipdicrosswithtrendingadx")]
        public async Task<int[]> MdiDdiCrossAndTrendingAdx([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _mdiPdiService.MdiPdiCrossAndTrendingAdx(candles, request.ExtraParams);
            return indicies;
        }

        [Route("api/signal/mdipditrend")]
        public async Task<int[]> MdiDdiTrend([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _mdiPdiService.Trend(candles, request.ExtraParams);
            return indicies;
        }

        [Route("api/signal/zigzagmaxima")]
        public async Task<int[]> ZigZagMaxima([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _zigZagService.Maximas(candles, request.ExtraParams);
            return indicies;
        }

        [Route("api/signal/zigzagminima")]
        public async Task<int[]> ZigZagMinima([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _zigZagService.Minimas(candles, request.ExtraParams);
            return indicies;
        }

        [Route("api/signal/zigzagresistance")]
        public async Task<int[]> ZigZagResistance([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _zigZagService.Resistance(candles, request.ExtraParams);
            return indicies;
        }

        [Route("api/signal/zigzagsupport")]
        public async Task<int[]> ZigZagSupport([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _zigZagService.Support(candles, request.ExtraParams);
            return indicies;
        }
    }
}
