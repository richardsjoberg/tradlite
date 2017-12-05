using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Management
{
    public class CurrentLowStopLoss : IStopLossManagement
    {
        public decimal? StopLoss(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters = null)
        {
            return candles[signalIndex].Low;
        }
    }
}
