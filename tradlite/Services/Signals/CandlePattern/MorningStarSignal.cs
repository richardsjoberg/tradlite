using System.Collections.Generic;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class MorningStarSignal : CandlePatternBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseStarParams(parameters);
            return ExecuteRuleNullableBool<MorningStar>(candles, @params.trendPeriodCount, @params.periodCount, @params.shortThreshold, @params.longThreshold, @params.threshold);
        }
    }
}
