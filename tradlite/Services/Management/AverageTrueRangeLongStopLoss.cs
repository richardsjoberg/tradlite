using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Management
{
    public class AverageTrueRangeLongStopLoss : IStopLossManagement
    {
        public decimal? StopLoss(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters)
        {
            var period = parameters.ParseJsonParam("period", 14);
            var stopMultiplier = parameters.ParseJsonParam("stopMultiplier", 20m);
            var atr = candles.Atr(period);
            if (!atr[signalIndex].Tick.HasValue)
                return null;

            return candles[signalIndex].Close - atr[signalIndex].Tick.Value * stopMultiplier;
        }
    }
}
