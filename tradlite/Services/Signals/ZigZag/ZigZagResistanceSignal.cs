﻿using System.Collections.Generic;
using Trady.Core.Infrastructure;
using Trady.Analysis;
using Trady.Analysis.Indicator;

namespace Tradlite.Services.Signals.ZigZag
{
    public class ZigZagResistanceSignal : ZigZagBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters, candles);
            var signal = Rule.Create(c => c.Get<ZigZagResistance>(@params.zigZagThreshold, @params.turningPointMargin, @params.requiredNumberOfTurningPoints)[c.Index].Tick);

            return ExecuteRule(candles, signal);
        }
    }
}
