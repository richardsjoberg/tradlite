using System.Collections.Generic;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Management
{
    public class RsiOverboughtEntryFilter : IEntryFilterManagement
    {
        public bool Entry(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters = null)
        {
            var rsi = candles.Rsi(5)[signalIndex].Tick;
            if (rsi.HasValue)
            {
                return rsi.Value < 75;
            }
            return false;
        }
    }
}
