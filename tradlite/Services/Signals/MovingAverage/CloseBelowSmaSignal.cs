using System.Collections.Generic;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.MovingAverage
{
    public class CloseBelowSmaSignal : MovingAverageBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var period = ParsePeriodCountParam(parameters);
            return CloseBelowIndicator<SimpleMovingAverage>(candles, period);
        }
    }
}
