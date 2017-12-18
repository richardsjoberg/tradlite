using System.Collections.Generic;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class UpsideTasukiGapSignal : CandlePatternBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseTasukiGapParams(parameters);
            return ExecuteRuleNullableBool<UpsideTasukiGap>(candles, @params.trendPeriodCount, @params.sizeThreshold);
        }
    }
}
