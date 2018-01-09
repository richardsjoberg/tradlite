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
            if (!_atr.ContainsKey(ticker))
            {
                _atr.Add(ticker, candles.Atr(period));
            }

            var atr = _atr[ticker];
            if (!atr[signalIndex].Tick.HasValue)
                return null;

            return candles[signalIndex].Close - atr[signalIndex].Tick.Value * limitMultiplier;
        }
    }
}
