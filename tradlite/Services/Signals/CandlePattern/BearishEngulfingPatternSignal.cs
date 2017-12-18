using System.Collections.Generic;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class BearishEngulfingPatternSignal : CandlePatternBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var upTrendPeriodCount = parameters.ParseJsonParam("upTrendPeriodCount", 3);
            return ExecuteRuleNullableBool<BearishEngulfingPattern>(candles, upTrendPeriodCount);
        }
    }
}
