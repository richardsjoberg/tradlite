using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Management
{
    public class AverageTrueRangeLongLimit : ILimitManagement
    {
        public decimal? Limit(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters)
        {
            var period = parameters.ParseJsonParam("period", 14);
            var limitMultiplier = parameters.ParseJsonParam("limitMultiplier", 5m);
            var atr = candles.Atr(period);
            if (!atr[signalIndex].Tick.HasValue)
                return null;

            return candles[signalIndex].Close + atr[signalIndex].Tick.Value * limitMultiplier;
        }
    }
}
