using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Signals.DirectionalMovement
{
    public class NoTrendSignal : DirectionalMovementBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters);
            var adx = candles.Adx(@params.AdxPeriod);
            var pdi = candles.Pdi(@params.PdiPeriod);
            var mdi = candles.Mdi(@params.MdiPeriod);
            var signal = Rule.Create(c =>
            {
                var adxTick = adx[c.Index].Tick;
                var pdiTick = pdi[c.Index].Tick;
                var mdiTick = mdi[c.Index].Tick;
                decimal? previousAdxTick = null;
                if (c.Index > 0)
                {
                    previousAdxTick = adx[c.Index - 1].Tick;
                }
                if (pdiTick.HasValue && mdiTick.HasValue && adxTick.HasValue && previousAdxTick.HasValue)
                {
                    return @params.Bullish ?
                        !UpTrend((pdiTick, mdiTick, adxTick, @params.AdxThreshold, previousAdxTick)) :
                        !DownTrend((pdiTick, mdiTick, adxTick, @params.AdxThreshold, previousAdxTick));
                }

                return false;
            });
            return ExecuteRule(candles, signal);
        }
    }
}
