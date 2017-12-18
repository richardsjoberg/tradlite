using System.Collections.Generic;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class BullishHaramiSignal : CandlePatternBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseHaramiParams(parameters);
            return ExecuteRuleNullableBool<BullishHarami>(candles, @params.containedShadows, @params.uptrendPeriodCount);
        }
    }
}
