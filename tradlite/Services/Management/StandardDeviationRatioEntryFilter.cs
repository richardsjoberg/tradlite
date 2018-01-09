using System.Collections.Generic;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Management
{
    public class StandardDeviationRatioEntryFilter : IEntryFilterManagement
    {
        public bool Entry(IReadOnlyList<IOhlcv> candles, int signalIndex, string ticker, string parameters = null)
        {
            var sd1 = candles.Sd(5)[signalIndex].Tick;
            var sd2 = candles.Sd(10)[signalIndex].Tick;
            var ratio = sd1 / sd2;
            if(ratio.HasValue)
            {
                return ratio < 1;
            }
            return false;
        }
    }
}
