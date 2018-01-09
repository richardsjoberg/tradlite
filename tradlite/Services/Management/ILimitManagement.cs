using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Management
{
    public interface ILimitManagement
    {
        decimal? Limit(IReadOnlyList<IOhlcv> candles, int signalIndex, string ticker, string parameters);
    }
}
