using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.DirectionalMovement
{
    public class TrendSignal : DirectionalMovementBase, ISignalService 
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
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
    }
}
