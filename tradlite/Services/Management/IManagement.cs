using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Management
{
    public interface IManagement
    {
        decimal? SellStop(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters);
        decimal? SellLimit(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters);
        decimal? BuyStop(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters);
        decimal? BuyLimit(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters);
    }
}
