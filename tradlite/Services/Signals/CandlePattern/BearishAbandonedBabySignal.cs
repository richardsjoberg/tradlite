using System.Collections.Generic;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class BearishAbandonedBabySignal : CandlePatternBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseAbandonedBabyParams(parameters);
            return ExecuteRuleNullableBool<BearishAbandonedBaby>(candles, @params.trendPeriodCount, @params.periodCount, @params.longThreshold, @params.dojiThreshold);
        }
    }
}
