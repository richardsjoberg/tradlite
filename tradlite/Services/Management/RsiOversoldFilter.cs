﻿using System.Collections.Generic;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Management
{
    public class RsiOversoldEntryFilter : IEntryFilterManagement
    {
        public bool Entry(IReadOnlyList<IOhlcv> candles, int signalIndex, string ticker, string parameters = null)
        {
            var rsi = candles.Rsi(5)[signalIndex].Tick;
            if (rsi.HasValue)
            {
                return rsi.Value > 25;
            }
            return false;
        }
    }
}
