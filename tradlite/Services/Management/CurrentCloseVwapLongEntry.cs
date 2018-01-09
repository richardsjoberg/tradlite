using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Management
{
    public class CurrentCloseVwapLongEntry : IEntryManagement
    {
        private Dictionary<string, IReadOnlyList<IAnalyzableTick<decimal?>>> _vwap = new Dictionary<string, IReadOnlyList<IAnalyzableTick<decimal?>>>();
        public decimal? Entry(IReadOnlyList<IOhlcv> candles, int signalIndex, string ticker, string parameters)
        {
            var candle = candles[signalIndex];
            var cacheKey = ticker + candles.Min(c => c.DateTime).DateTime + candles.Max(c => c.DateTime).DateTime;
            if (!_vwap.ContainsKey(cacheKey))
            {
                _vwap.Add(cacheKey, candles.Vwap());
            }
            var vwap = _vwap[cacheKey];

            if (!vwap[signalIndex].Tick.HasValue || candle.Close > vwap[signalIndex].Tick.Value)
            {
                return null;
            }
            
            return candle.Close;
        }
    }
}
