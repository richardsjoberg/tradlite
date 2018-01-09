using System.Collections.Generic;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Management
{
    public class CurrentHighStopLoss : IStopLossManagement
    {
        public decimal? StopLoss(IReadOnlyList<IOhlcv> candles, int signalIndex, string ticker, string parameters = null)
        {
            return candles[signalIndex].High;
        }
    }
}
