using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Management
{
    public class CurrentCloseEntry : IEntryManagement
    {
        public decimal? Entry(IReadOnlyList<IOhlcv> candles, int signalIndex, string ticker, string parameters)
        {
            return candles[signalIndex].Close;
        }
    }
}
