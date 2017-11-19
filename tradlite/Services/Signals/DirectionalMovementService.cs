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
                
                if (pdiTick.HasValue && mdiTick.HasValue && adxTick.HasValue)
                {
                    return @params.Bullish ?
                        UpTrend((pdiTick, mdiTick, adxTick, @params.AdxTreshold)) :
                        DownTrend((pdiTick, mdiTick, adxTick, @params.AdxTreshold));
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
                if (c.Index > 0)
                {
                    previousAdxTick = c.Get<AverageDirectionalIndex>(@params.AdxPeriod)[c.Index - 1].Tick;
                    previousPdiTick = c.Get<PlusDirectionalIndicator>(@params.PdiPeriod)[c.Index - 1].Tick;
                    previousMdiTick = c.Get<MinusDirectionalIndicator>(@params.MdiPeriod)[c.Index - 1].Tick;
                }
                
                if (pdiTick.HasValue && mdiTick.HasValue && adxTick.HasValue && previousAdxTick.HasValue)
                {
                    return @params.Bullish ?
                        UpTrend((pdiTick, mdiTick, adxTick, @params.AdxTreshold)) &&
                        !UpTrend((previousPdiTick, previousMdiTick, previousAdxTick, @params.AdxTreshold)) :
                        DownTrend((pdiTick, mdiTick, adxTick, @params.AdxTreshold)) &&
                        !DownTrend((previousPdiTick, previousMdiTick, previousAdxTick, @params.AdxTreshold));
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

        private Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, int adxTreshold), bool>
            UpTrend = (@params) =>
            {
                return @params.pdiTick.Value > @params.mdiTick.Value &&
                            @params.adxTick.Value > @params.adxTreshold;
            };
        private Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, int adxTreshold), bool>
            DownTrend = (@params) =>
            {
                return @params.pdiTick.Value < @params.mdiTick.Value &&
                            @params.adxTick.Value > @params.adxTreshold;
            };


        private (int AdxPeriod, int MdiPeriod, int PdiPeriod, int AdxTreshold, bool Bullish) ParseParams(string @params)
        {
            var parsedParams = (AdxPeriod: 8, MdiPeriod: 8, PdiPeriod: 8, AdxTreshold: 20, Bullish: true);

            if (!string.IsNullOrEmpty(@params))
            {
                var extraParam = JObject.Parse(@params);
                if (extraParam["adxPeriod"] != null)
                    int.TryParse(extraParam["adxPeriod"].ToString(), out parsedParams.AdxPeriod);
                if (extraParam["mdiPeriod"] != null)
                    int.TryParse(extraParam["mdiPeriod"].ToString(), out parsedParams.MdiPeriod);
                if (extraParam["pdiPeriod"] != null)
                    int.TryParse(extraParam["pdiPeriod"].ToString(), out parsedParams.PdiPeriod);
                if (extraParam["adxTreshold"] != null)
                    int.TryParse(extraParam["adxTreshold"].ToString(), out parsedParams.AdxTreshold);
                if (extraParam["bearish"] != null)
                    parsedParams.Bullish = false;
            }
            return parsedParams;
        }

    }
}
