using System.Collections.Generic;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.MovingAverage
{
    public class SmaCrossSignal : MovingAverageBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseMaCrossParams(parameters);
            return IndicatorCross<SimpleMovingAverage, SimpleMovingAverage>(candles, new object[] { @params.period1 }, new object[] { @params.period2 });
        }
    }
}
