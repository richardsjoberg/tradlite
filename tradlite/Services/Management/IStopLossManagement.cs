using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Management
{
    public interface IStopLossManagement
    {
        decimal? StopLoss(IReadOnlyList<IOhlcv> candles, int signalIndex, string ticker, string parameters);
    }
}
