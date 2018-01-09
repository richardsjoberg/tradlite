using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;
using Trady.Analysis;

namespace Tradlite.Services.Signals.MovingAverage
{
    public class PositiveSmaSignal : MovingAverageBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var period = ParsePeriodCountParam(parameters);
            var smaDiff = candles.Sma(period).Select(t=>t.Tick).Diff();
            var signal = Rule.Create(ic => smaDiff[ic.Index].HasValue && smaDiff[ic.Index].Value > 0);
            return ExecuteRule(candles, signal);
        }
    }
}
