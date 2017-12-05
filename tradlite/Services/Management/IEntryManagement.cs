using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Management
{
    public interface IEntryManagement
    {
        decimal? Entry(IReadOnlyList<IOhlcv> candles, int signalIndex, string parameters = null);
    }
}
