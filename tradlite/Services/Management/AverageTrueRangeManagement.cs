using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Management
{
    public class AverageTrueRangeManagement : IManagement
    {
        public decimal? BuyLimit(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters)
        {
            var period = parameters.ParseJsonParam("period", 14);
            var limitMultiplier = parameters.ParseJsonParam("limitMultiplier", 2m);
            var atr = candles.Atr(period);
            if (!atr[signalIndex].Tick.HasValue)
                return null;

            return candles[signalIndex].Close + atr[signalIndex].Tick.Value * limitMultiplier;
        }

        public decimal? BuyStop(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters)
        {
            var period = parameters.ParseJsonParam("period", 14);
            var stopMultiplier = parameters.ParseJsonParam("stopMultiplier", 0.5m);
            var atr = candles.Atr(period);
            if (!atr[signalIndex].Tick.HasValue)
                return null;

            return candles[signalIndex].Close - atr[signalIndex].Tick.Value * stopMultiplier;
        }

        public decimal? SellLimit(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters)
        {
            var period = parameters.ParseJsonParam("period", 14);
            var limitMultiplier = parameters.ParseJsonParam("limitMultiplier", 2m);
            var atr = candles.Atr(period);
            if (!atr[signalIndex].Tick.HasValue)
                return null;

            return candles[signalIndex].Close - atr[signalIndex].Tick.Value * limitMultiplier;
        }

        public decimal? SellStop(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters)
        {
            var period = parameters.ParseJsonParam("period", 14);
            var stopMultiplier = parameters.ParseJsonParam("stopMultiplier", 0.5m);
            var atr = candles.Atr(period);
            if (!atr[signalIndex].Tick.HasValue)
                return null;

            return candles[signalIndex].Close + atr[signalIndex].Tick.Value * stopMultiplier;
        }
    }
}
