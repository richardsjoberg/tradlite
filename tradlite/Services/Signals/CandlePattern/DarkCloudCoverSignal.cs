using System.Collections.Generic;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class DarkCloudCoverSignal : CandlePatternBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var upTrendPeriodCount = parameters.ParseJsonParam("upTrendPeriodCount", 3);
            var downTrendPeriodCount = parameters.ParseJsonParam("downTrendPeriodCount", 3);
            var longPeriodCount = parameters.ParseJsonParam("longPeriodCount", 20);
            var longThreshold = parameters.ParseJsonParam("upTrendPeriodCount", 0.75m);
            return ExecuteRuleNullableBool<DarkCloudCover>(candles, upTrendPeriodCount, downTrendPeriodCount, longPeriodCount, longThreshold);
        }
    }
}
