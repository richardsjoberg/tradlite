using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Management
{
    public class CurrentCloseVwapShortEntry : IEntryManagement
    {
        IReadOnlyList<Trady.Analysis.AnalyzableTick<decimal?>> _vwap;

        public decimal? Entry(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters)
        {
            var candle = candles[signalIndex];
            var vwap = candles.Vwap();
            
            if (!vwap[signalIndex].Tick.HasValue || candle.Close < vwap[signalIndex].Tick.Value)
            {
                return null;
            }

            return candle.Close;
        }
    }
}
