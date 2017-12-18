using System.Collections.Generic;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class BullishAbandonedBabySignal : CandlePatternBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseAbandonedBabyParams(parameters);
            return ExecuteRuleNullableBool<BullishAbandonedBaby>(candles, @params.trendPeriodCount, @params.periodCount, @params.longThreshold, @params.dojiThreshold);
        }
    }
}
