using System.Collections.Generic;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.MovingAverage
{
    public class CloseAboveEmaSignal : MovingAverageBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var period = ParsePeriodCountParam(parameters);
            return CloseAboveIndicator<ExponentialMovingAverage>(candles, period);
        }
    }
}
