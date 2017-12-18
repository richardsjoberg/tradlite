using System.Collections.Generic;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class BullishEngulfingPatternSignal : CandlePatternBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var downTrendPeriodCount = parameters.ParseJsonParam("downTrendPeriodCount", 3);
            return ExecuteRuleNullableBool<BullishEngulfingPattern>(candles, downTrendPeriodCount);
        }
    }
}
