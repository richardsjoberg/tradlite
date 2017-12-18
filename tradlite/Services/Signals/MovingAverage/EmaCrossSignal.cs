using System.Collections.Generic;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.MovingAverage
{
    public class EmaCrossSignal : MovingAverageBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseMaCrossParams(parameters);
            return IndicatorCross<ExponentialMovingAverage, ExponentialMovingAverage>(candles, new object[] { @params.period1 }, new object[] { @params.period2 });
        }
    }
}
