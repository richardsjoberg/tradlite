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
        int[] Trend(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] MdiPdiCrossAndTrendingAdx(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] NewTrend(IReadOnlyList<IOhlcv> candles, string parameters);

    }
    public class DirectionalMovementService : SignalBase, IDirectionalMovementService
    {
        public int[] Trend(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters);
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
                        UpTrend((pdiTick, mdiTick, adxTick, @params.AdxThreshold, previousAdxTick)) :
                        DownTrend((pdiTick, mdiTick, adxTick, @params.AdxThreshold, previousAdxTick));
                }

                return false;
            });
            return ExecuteRule(candles, signal);
        }
        
        public int[] NewTrend(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters);
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
                        UpTrend((pdiTick, mdiTick, adxTick, @params.AdxThreshold, previousAdxTick)) &&
                        !UpTrend((previousPdiTick, previousMdiTick, previousAdxTick, @params.AdxThreshold, previousPreviousAdxTick)) :
                        DownTrend((pdiTick, mdiTick, adxTick, @params.AdxThreshold, previousAdxTick)) &&
                        !DownTrend((previousPdiTick, previousMdiTick, previousAdxTick, @params.AdxThreshold, previousPreviousAdxTick));
                }

                return false;
            });

            return ExecuteRule(candles, signal);
        }

        public int[] MdiPdiCrossAndTrendingAdx(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters);
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
                        MdiPdiBullishCross((pdiTick, mdiTick, adxTick, previousPdiTick, previousMdiTick, previousAdxTick, @params.AdxThreshold)) :
                        MdiPdiBearishCross((pdiTick, mdiTick, adxTick, previousPdiTick, previousMdiTick, previousAdxTick, @params.AdxThreshold));
                }

                return false;
            });

            return ExecuteRule(candles, signal);
        }

        
        
        private Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, decimal? previousPdiTick, decimal? previousMdiTick, decimal? previousAdxTick, int adxThreshold), bool>
            MdiPdiBullishCross = (@params) =>
            {
                return @params.pdiTick.Value > @params.mdiTick.Value &&
                            @params.previousPdiTick.Value <= @params.previousMdiTick.Value &&
                            @params.adxTick.Value > @params.adxThreshold &&
                            @params.adxTick.Value > @params.previousAdxTick.Value;
            };
        private Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, decimal? previousPdiTick, decimal? previousMdiTick, decimal? previousAdxTick, int adxThreshold), bool>
            MdiPdiBearishCross = (@params) =>
            {
                return @params.pdiTick.Value < @params.mdiTick.Value &&
                            @params.previousPdiTick.Value >= @params.previousMdiTick.Value &&
                            @params.adxTick.Value > @params.adxThreshold &&
                            @params.adxTick.Value > @params.previousAdxTick.Value;
            };

        private Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, int adxThreshold, decimal? previousAdxTick), bool>
            UpTrend = (@params) =>
            {
                return @params.pdiTick.Value > @params.mdiTick.Value &&
                            @params.adxTick.Value > @params.adxThreshold &&
                            @params.adxTick.Value > @params.previousAdxTick.Value;
            };
        private Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, int adxThreshold, decimal? previousAdxTick), bool>
            DownTrend = (@params) =>
            {
                return @params.pdiTick.Value < @params.mdiTick.Value &&
                            @params.adxTick.Value > @params.adxThreshold &&
                            @params.adxTick.Value > @params.previousAdxTick.Value;
            };


        private (int AdxPeriod, int MdiPeriod, int PdiPeriod, int AdxThreshold, bool Bullish) ParseParams(string @params)
        {
            return (@params.ParseJsonParam("adxPeriod", 8), 
                @params.ParseJsonParam("mdiPeriod", 8), 
                @params.ParseJsonParam("pdiPeriod", 8), 
                @params.ParseJsonParam("adxThreshold", 20), 
                @params.ParseJsonParam("bullish", true));
        }

    }
}
