using System.Collections.Generic;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class BearishHaramiSignal : CandlePatternBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseHaramiParams(parameters);
            return ExecuteRuleNullableBool<BearishHarami>(candles, @params.containedShadows, @params.uptrendPeriodCount);
        }
    }
}
