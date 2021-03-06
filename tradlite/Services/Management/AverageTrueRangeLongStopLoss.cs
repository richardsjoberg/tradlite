﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Management
{
    public class AverageTrueRangeLongStopLoss : IStopLossManagement
    {
        private Dictionary<string, IReadOnlyList<IAnalyzableTick<decimal?>>> _atr = new Dictionary<string, IReadOnlyList<IAnalyzableTick<decimal?>>>();

        public decimal? StopLoss(IReadOnlyList<IOhlcv> candles, int signalIndex, string ticker, string parameters)
        {
            var period = parameters.ParseJsonParam("period", 14);
            var stopMultiplier = parameters.ParseJsonParam("stopMultiplier", 5m);
            var cacheKey = ticker + candles.First().DateTime.ToString() + candles.Last().DateTime.ToString();
            if (!_atr.ContainsKey(cacheKey))
            {
                _atr.Add(cacheKey, candles.Atr(period));
            }

            var atr = _atr[cacheKey];
            if (!atr[signalIndex].Tick.HasValue)
                return null;

            return candles[signalIndex].Low - atr[signalIndex].Tick.Value * stopMultiplier;
        }
    }
}
