using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Signals.ZigZag
{
    public class ZigZagBase : SignalBase
    {
        public (decimal zigZagThreshold, decimal turningPointMargin, int requiredNumberOfTurningPoints) ParseParams(string @params, IReadOnlyList<IOhlcv> candles)
        {
            var atr = candles.Atr(candles.Count - 1);
            var atrThreshold = atr[candles.Count - 1].Tick.Value / candles.Average(c => c.Close);

            return (@params.ParseJsonParam("zigZagThreshold", atrThreshold),
                @params.ParseJsonParam("turningPointMargin", atrThreshold / 4.2m),
                @params.ParseJsonParam("requiredNumberOfTurningPoints", 1));
        }
    }
}
