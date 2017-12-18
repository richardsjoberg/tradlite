using System.Collections.Generic;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.MovingAverage
{
    public class CloseAboveSmaSignal : MovingAverageBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var period = ParsePeriodCountParam(parameters);
            return CloseAboveIndicator<SimpleMovingAverage>(candles, period);
        }
    }
}
