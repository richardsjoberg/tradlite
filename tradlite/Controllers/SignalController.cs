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
            var candles = await _candleService.GetCandles(request);
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
            var candles = await _candleService.GetCandles(request);
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

            if (!string.IsNullOrEmpty(request.ExtraParams))
            {
                var extraParam = JObject.Parse(request.ExtraParams);
                if (extraParam["uptrendPeriod"] != null)
                    int.TryParse(extraParam["uptrendPeriod"].ToString(), out uptrendPeriod);
                if (extraParam["rsiPeriod"] != null)
                    int.TryParse(extraParam["rsiPeriod"].ToString(), out rsiPeriod);
            }

            var candles = await _candleService.GetCandles(request);
            
            var uptrend = new UpTrend(candles, uptrendPeriod);

            var signal = Rule.Create(c => c.IsRsiOverbought(rsiPeriod) && uptrend[c.Index].Tick.HasValue && uptrend[c.Index].Tick.Value);
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        [Route("api/mdipdicrosswithtrendingadx")]
        public async Task<int[]> MdiDdiCrossAndTrendingAdx([FromQuery]SignalRequest request)
        {
            var adxPeriod = 8;
            var mdiPeriod = 8;
            var pdiPeriod = 8;
            var adxTreshold = 20;
            var bullish = true;
            if (!string.IsNullOrEmpty(request.ExtraParams))
            {
                var extraParam = JObject.Parse(request.ExtraParams);
                if(extraParam["adxPeriod"] != null)
                    int.TryParse(extraParam["adxPeriod"].ToString(), out adxPeriod);
                if (extraParam["mdiPeriod"] != null)
                    int.TryParse(extraParam["mdiPeriod"]?.ToString(), out mdiPeriod);
                if (extraParam["pdiPeriod"] != null)
                    int.TryParse(extraParam["pdiPeriod"]?.ToString(), out pdiPeriod);
                if (extraParam["adxTreshold"] != null)
                    int.TryParse(extraParam["adxTreshold"]?.ToString(), out adxTreshold);
                if (extraParam["bearish"] != null)
                    bullish = false;
            }

            var candles = await _candleService.GetCandles(request);
            var adx = candles.Adx(adxPeriod);
            var mdi = candles.Mdi(mdiPeriod);
            var pdi = candles.Pdi(pdiPeriod);
            var signal = Rule.Create(c => 
            {
                var adxTick = c.Get<AverageDirectionalIndex>(adxPeriod)[c.Index].Tick;
                var pdiTick = c.Get<PlusDirectionalIndicator>(pdiPeriod)[c.Index].Tick;
                var mdiTick = c.Get<MinusDirectionalIndicator>(mdiPeriod)[c.Index].Tick;
                decimal? previousAdxTick = null;
                decimal? previousPdiTick = null;
                decimal? previousMdiTick = null;
                if (c.Index > 0)
                {
                    previousAdxTick = c.Get<AverageDirectionalIndex>(adxPeriod)[c.Index - 1].Tick;
                    previousPdiTick = c.Get<PlusDirectionalIndicator>(pdiPeriod)[c.Index - 1].Tick;
                    previousMdiTick = c.Get<MinusDirectionalIndicator>(mdiPeriod)[c.Index - 1].Tick;
                }

                if (pdiTick.HasValue && mdiTick.HasValue && adxTick.HasValue && previousAdxTick.HasValue)
                {
                    return bullish ?
                        MdiPdiBullishCross((pdiTick, mdiTick, adxTick, previousPdiTick, previousMdiTick, previousAdxTick, adxTreshold)) :
                        MdiPdiBearishCross((pdiTick, mdiTick, adxTick, previousPdiTick, previousMdiTick, previousAdxTick, adxTreshold));
                }

                return false;
            });
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        //private Func<decimal?, decimal?, decimal?, decimal?, decimal?, decimal?, int, bool> MdiPdiBullishCross =
        //    (pdiTick, mdiTick, adxTick, previousPdiTick, previousMdiTick, previousAdxTick, adxTreshold) =>
        //    {
        //        return pdiTick.Value > mdiTick.Value &&
        //                    previousPdiTick.Value <= previousMdiTick.Value &&
        //                    adxTick.Value > adxTreshold &&
        //                    adxTick.Value > previousAdxTick.Value;
        //    };
        private Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, decimal? previousPdiTick, decimal? previousMdiTick, decimal? previousAdxTick, int adxTreshold), bool> 
            MdiPdiBullishCross = (@params) =>
        {
            return @params.pdiTick.Value > @params.mdiTick.Value &&
                        //@params.previousPdiTick.Value <= @params.previousMdiTick.Value &&
                        @params.adxTick.Value > @params.adxTreshold &&
                        @params.adxTick.Value > @params.previousAdxTick.Value;
        };
        private Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, decimal? previousPdiTick, decimal? previousMdiTick, decimal? previousAdxTick, int adxTreshold),bool> 
            MdiPdiBearishCross = (@params) =>
            {
                return @params.pdiTick.Value < @params.mdiTick.Value &&
                            //@params.previousPdiTick.Value >= @params.previousMdiTick.Value &&
                            @params.adxTick.Value > @params.adxTreshold &&
                            @params.adxTick.Value > @params.previousAdxTick.Value;
            };

        [Route("api/mdipdibearishcrosswithtrendingadx")]
        public async Task<int[]> MdiDdiBearishCrossAndTrendingAdx([FromQuery]SignalRequest request)
        {
            int adxPeriod = 8;
            int mdiPeriod = 8;
            int pdiPeriod = 8;
            int adxTreshold = 20;

            if (!string.IsNullOrEmpty(request.ExtraParams))
            {
                var extraParam = JObject.Parse(request.ExtraParams);
                if (extraParam["adxPeriod"] != null)
                    int.TryParse(extraParam["adxPeriod"].ToString(), out adxPeriod);
                if (extraParam["mdiPeriod"] != null)
                    int.TryParse(extraParam["mdiPeriod"]?.ToString(), out mdiPeriod);
                if (extraParam["pdiPeriod"] != null)
                    int.TryParse(extraParam["pdiPeriod"]?.ToString(), out pdiPeriod);
                if (extraParam["adxTreshold"] != null)
                    int.TryParse(extraParam["adxTreshold"]?.ToString(), out adxTreshold);
            }

            var candles = await _candleService.GetCandles(request);
            var adx = candles.Adx(adxPeriod);
            var mdi = candles.Mdi(mdiPeriod);
            var pdi = candles.Pdi(pdiPeriod);
            var signal = Rule.Create(c =>
            {
                var adxTick = c.Get<AverageDirectionalIndex>(adxPeriod)[c.Index].Tick;
                var pdiTick = c.Get<PlusDirectionalIndicator>(pdiPeriod)[c.Index].Tick;
                var mdiTick = c.Get<MinusDirectionalIndicator>(mdiPeriod)[c.Index].Tick;
                decimal? previousAdxTick = null;
                decimal? previousPdiTick = null;
                decimal? previousMdiTick = null;
                if (c.Index > 0)
                {
                    previousAdxTick = c.Get<AverageDirectionalIndex>(adxPeriod)[c.Index - 1].Tick;
                    previousPdiTick = c.Get<PlusDirectionalIndicator>(pdiPeriod)[c.Index - 1].Tick;
                    previousMdiTick = c.Get<MinusDirectionalIndicator>(mdiPeriod)[c.Index - 1].Tick;
                }

                if (pdiTick.HasValue && mdiTick.HasValue && adxTick.HasValue && previousAdxTick.HasValue)
                {
                    return pdiTick.Value < mdiTick.Value &&
                        previousPdiTick.Value >= previousMdiTick.Value &&
                        adxTick.Value > adxTreshold &&
                        adxTick.Value > previousAdxTick.Value;
                }

                return false;
            });
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }
    }
}
