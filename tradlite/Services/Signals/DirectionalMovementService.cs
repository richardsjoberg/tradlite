using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;
using Trady.Analysis;
using Trady.Analysis.Indicator;

namespace Tradlite.Services.Signals
{
    public interface IDirectionalMovementService
    {
        int[] Trend(IReadOnlyList<IOhlcv> candles, string extraParams);
        int[] MdiPdiCrossAndTrendingAdx(IReadOnlyList<IOhlcv> candles, string extraParams);
        int[] NewTrend(IReadOnlyList<IOhlcv> candles, string extraParams);

    }
    public class DirectionalMovementService : IDirectionalMovementService
    {
        public int[] Trend(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseParams(extraParams);
            var signal = Rule.Create(c =>
            {
                var adxTick = c.Get<AverageDirectionalIndex>(@params.AdxPeriod)[c.Index].Tick;
                var pdiTick = c.Get<PlusDirectionalIndicator>(@params.PdiPeriod)[c.Index].Tick;
                var mdiTick = c.Get<MinusDirectionalIndicator>(@params.MdiPeriod)[c.Index].Tick;
                decimal? previousAdxTick = null;
                if (c.Index > 0)
                {
                    previousAdxTick = c.Get<AverageDirectionalIndex>(@params.AdxPeriod)[c.Index - 1].Tick;
                }
                if (pdiTick.HasValue && mdiTick.HasValue && adxTick.HasValue && previousAdxTick.HasValue)
                {
                    return @params.Bullish ?
                        UpTrend((pdiTick, mdiTick, adxTick, @params.AdxTreshold, previousAdxTick)) :
                        DownTrend((pdiTick, mdiTick, adxTick, @params.AdxTreshold, previousAdxTick));
                }

                return false;
            });
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }
        
        public int[] NewTrend(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseParams(extraParams);
            var signal = Rule.Create(c =>
            {
                var adxTick = c.Get<AverageDirectionalIndex>(@params.AdxPeriod)[c.Index].Tick;
                var pdiTick = c.Get<PlusDirectionalIndicator>(@params.PdiPeriod)[c.Index].Tick;
                var mdiTick = c.Get<MinusDirectionalIndicator>(@params.MdiPeriod)[c.Index].Tick;
                decimal? previousAdxTick = null;
                decimal? previousPdiTick = null;
                decimal? previousMdiTick = null;
                decimal? previousPreviousAdxTick = null;
                if (c.Index > 1)
                {
                    previousAdxTick = c.Get<AverageDirectionalIndex>(@params.AdxPeriod)[c.Index - 1].Tick;
                    previousPdiTick = c.Get<PlusDirectionalIndicator>(@params.PdiPeriod)[c.Index - 1].Tick;
                    previousMdiTick = c.Get<MinusDirectionalIndicator>(@params.MdiPeriod)[c.Index - 1].Tick;
                    previousPreviousAdxTick = c.Get<AverageDirectionalIndex>(@params.AdxPeriod)[c.Index - 2].Tick;
                }
                
                if (pdiTick.HasValue && mdiTick.HasValue && adxTick.HasValue && previousAdxTick.HasValue && previousPreviousAdxTick.HasValue)
                {
                    return @params.Bullish ?
                        UpTrend((pdiTick, mdiTick, adxTick, @params.AdxTreshold, previousAdxTick)) &&
                        !UpTrend((previousPdiTick, previousMdiTick, previousAdxTick, @params.AdxTreshold, previousPreviousAdxTick)) :
                        DownTrend((pdiTick, mdiTick, adxTick, @params.AdxTreshold, previousAdxTick)) &&
                        !DownTrend((previousPdiTick, previousMdiTick, previousAdxTick, @params.AdxTreshold, previousPreviousAdxTick));
                }

                return false;
            });
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        public int[] MdiPdiCrossAndTrendingAdx(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseParams(extraParams);
            var signal = Rule.Create(c =>
            {
                var adxTick = c.Get<AverageDirectionalIndex>(@params.AdxPeriod)[c.Index].Tick;
                var pdiTick = c.Get<PlusDirectionalIndicator>(@params.PdiPeriod)[c.Index].Tick;
                var mdiTick = c.Get<MinusDirectionalIndicator>(@params.MdiPeriod)[c.Index].Tick;
                decimal? previousAdxTick = null;
                decimal? previousPdiTick = null;
                decimal? previousMdiTick = null;
                if (c.Index > 0)
                {
                    previousAdxTick = c.Get<AverageDirectionalIndex>(@params.AdxPeriod)[c.Index - 1].Tick;
                    previousPdiTick = c.Get<PlusDirectionalIndicator>(@params.PdiPeriod)[c.Index - 1].Tick;
                    previousMdiTick = c.Get<MinusDirectionalIndicator>(@params.MdiPeriod)[c.Index - 1].Tick;
                }

                if (pdiTick.HasValue && mdiTick.HasValue && adxTick.HasValue && previousAdxTick.HasValue)
                {
                    return @params.Bullish ?
                        MdiPdiBullishCross((pdiTick, mdiTick, adxTick, previousPdiTick, previousMdiTick, previousAdxTick, @params.AdxTreshold)) :
                        MdiPdiBearishCross((pdiTick, mdiTick, adxTick, previousPdiTick, previousMdiTick, previousAdxTick, @params.AdxTreshold));
                }

                return false;
            });
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        
        
        private Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, decimal? previousPdiTick, decimal? previousMdiTick, decimal? previousAdxTick, int adxTreshold), bool>
            MdiPdiBullishCross = (@params) =>
            {
                return @params.pdiTick.Value > @params.mdiTick.Value &&
                            @params.previousPdiTick.Value <= @params.previousMdiTick.Value &&
                            @params.adxTick.Value > @params.adxTreshold &&
                            @params.adxTick.Value > @params.previousAdxTick.Value;
            };
        private Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, decimal? previousPdiTick, decimal? previousMdiTick, decimal? previousAdxTick, int adxTreshold), bool>
            MdiPdiBearishCross = (@params) =>
            {
                return @params.pdiTick.Value < @params.mdiTick.Value &&
                            @params.previousPdiTick.Value >= @params.previousMdiTick.Value &&
                            @params.adxTick.Value > @params.adxTreshold &&
                            @params.adxTick.Value > @params.previousAdxTick.Value;
            };

        private Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, int adxTreshold, decimal? previousAdxTick), bool>
            UpTrend = (@params) =>
            {
                return @params.pdiTick.Value > @params.mdiTick.Value &&
                            @params.adxTick.Value > @params.adxTreshold &&
                            @params.adxTick.Value > @params.previousAdxTick.Value;
            };
        private Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, int adxTreshold, decimal? previousAdxTick), bool>
            DownTrend = (@params) =>
            {
                return @params.pdiTick.Value < @params.mdiTick.Value &&
                            @params.adxTick.Value > @params.adxTreshold &&
                            @params.adxTick.Value > @params.previousAdxTick.Value;
            };


        private (int AdxPeriod, int MdiPeriod, int PdiPeriod, int AdxTreshold, bool Bullish) ParseParams(string @params)
        {
            return (@params.ParseJsonParam("adxPeriod", 8), 
                @params.ParseJsonParam("mdiPeriod", 8), 
                @params.ParseJsonParam("pdiPeriod", 8), 
                @params.ParseJsonParam("adxTreshold", 20), 
                @params.ParseJsonParam("bullish", true));
        }

    }
}
