using System.Collections.Generic;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class DojiSignal : CandlePatternBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var dojiThreshold = parameters.ParseJsonParam("dojiThreshold", 0.1m);
            return ExecuteRuleBool<Doji>(candles, dojiThreshold);
        }
    }
}
