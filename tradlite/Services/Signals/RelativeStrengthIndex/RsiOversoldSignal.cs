using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Signals.RelativeStrengthIndex
{
    public class RsiOversoldSignal : RelativeStrengthIndexBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters);
            var signal = Rule.Create(c => c.Get<Trady.Analysis.Indicator.RelativeStrengthIndex>(@params.rsiPeriod)[c.Index].Tick.IsTrue(t => t < @params.rsiThreshold));

            return ExecuteRule(candles, signal);
        }
    }
}
