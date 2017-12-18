using System.Collections.Generic;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class DragonflyDojiSignal : CandlePatternBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseDojiParams(parameters);
            return ExecuteRuleBool<DragonifyDoji>(candles, @params.dojiThreshold, @params.shadowThreshold);
        }
    }
}
