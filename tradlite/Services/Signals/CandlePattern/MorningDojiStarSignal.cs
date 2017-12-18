using System.Collections.Generic;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class MorningDojiStarSignal : CandlePatternBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseDojiStarParams(parameters);
            return ExecuteRuleNullableBool<MorningDojiStar>(candles, @params.trendPeriodCount, @params.periodCount, @params.longThreshold, @params.dojiThreshold, @params.threshold);
        }
    }
}
