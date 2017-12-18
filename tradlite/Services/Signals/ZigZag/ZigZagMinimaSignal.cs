using System.Collections.Generic;
using Trady.Core.Infrastructure;
using Trady.Analysis;
using Trady.Analysis.Indicator;

namespace Tradlite.Services.Signals.ZigZag
{
    public class ZigZagMinimaSignal : ZigZagBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters, candles);
            var signal = Rule.Create(c => c.Get<ZigZagMinima>(@params.zigZagThreshold)[c.Index].Tick != null);

            return ExecuteRule(candles, signal);
        }
    }
}
