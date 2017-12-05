using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals
{
    public interface ISignalService
    {
        int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters);
    }
}
