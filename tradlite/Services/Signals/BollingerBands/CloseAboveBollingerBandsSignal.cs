using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;
using Trady.Analysis.Indicator;
using Trady.Analysis;

namespace Tradlite.Services.Signals.BollingerBands
{
    public class CloseAboveBollingerBandsSignal : BollingerBandsBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var bollingerBands = GetBollingerBands(candles, parameters);
            var signal = Rule.Create(ic => bollingerBands[ic.Index].Tick.UpperBand.HasValue && ic.Close > bollingerBands[ic.Index].Tick.UpperBand);
            return ExecuteRule(candles, signal);
        }
    }
}
