using System.Collections.Generic;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class HaramiSignal : CandlePatternBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var containedShadows = parameters.ParseJsonParam("containedShadows", false);
            return ExecuteRuleNullableBool<Harami>(candles, containedShadows);
        }
    }
}
