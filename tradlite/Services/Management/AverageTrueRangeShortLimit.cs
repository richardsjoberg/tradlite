using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Management
{
    public class AverageTrueRangeShortLimit : ILimitManagement
    {
        private Dictionary<string, IReadOnlyList<IAnalyzableTick<decimal?>>> _atr = new Dictionary<string, IReadOnlyList<IAnalyzableTick<decimal?>>>();

        public decimal? Limit(IReadOnlyList<IOhlcv> candles, int signalIndex, string ticker, string parameters)
        {
            var period = parameters.ParseJsonParam("period", 14);
            var limitMultiplier = parameters.ParseJsonParam("limitMultiplier", 3m);
            var cacheKey = ticker + candles.First().DateTime.ToString() + candles.Last().DateTime.ToString();
            if (!_atr.ContainsKey(cacheKey))
            {
                _atr.Add(cacheKey, candles.Atr(period));
            }

            var atr = _atr[cacheKey];
            if (!atr[signalIndex].Tick.HasValue)
                return null;

            return candles[signalIndex].Close - atr[signalIndex].Tick.Value * limitMultiplier;
        }
    }
}
