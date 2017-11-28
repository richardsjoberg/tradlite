using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals
{
    public interface IMovingAverageService
    {
        int[] CloseAboveSma(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] CloseBelowSma(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] CloseAboveEma(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] CloseBelowEma(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] SmaCross(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] EmaCross(IReadOnlyList<IOhlcv> candles, string parameters);
    }
    public class MovingAverageService : SignalBase, IMovingAverageService
    {
        public int[] CloseAboveEma(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var period = ParsePeriodCountParam(parameters);
            return CloseAboveIndicator<ExponentialMovingAverage>(candles, period);
        }

        public int[] CloseAboveSma(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var period = ParsePeriodCountParam(parameters);
            return CloseAboveIndicator<SimpleMovingAverage>(candles, period);
        }

        public int[] CloseBelowEma(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var period = ParsePeriodCountParam(parameters);
            return CloseBelowIndicator<ExponentialMovingAverage>(candles, period);
        }

        public int[] CloseBelowSma(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var period = ParsePeriodCountParam(parameters);
            return CloseBelowIndicator<SimpleMovingAverage>(candles, period);
        }

        private int ParsePeriodCountParam(string @params)
        {
            return @params.ParseJsonParam("period", 14);
        }
        
        public int[] SmaCross(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseMaCrossParams(parameters);
            return IndicatorCross<SimpleMovingAverage, SimpleMovingAverage>(candles, new object[] { @params.period1 }, new object[] { @params.period2 });
        }

        public int[] EmaCross(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseMaCrossParams(parameters);
            return IndicatorCross<ExponentialMovingAverage, ExponentialMovingAverage>(candles, new object[] { @params.period1 }, new object[] { @params.period2 });
        }

        private (int period1, int period2) ParseMaCrossParams(string @params)
        {
            return (@params.ParseJsonParam("period1", 8), @params.ParseJsonParam("period2", 14));
        }
    }
}
