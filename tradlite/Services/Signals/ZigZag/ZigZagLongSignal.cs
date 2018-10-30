using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.ZigZag
{
    public class ZigZagLongSignal : ZigZagBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters, candles);
            var maximaSignal = Rule.Create(c => c.Get<ZigZagMaxima>(@params.zigZagThreshold)[c.Index].Tick != null);
            var maximas = ExecuteRule(candles, maximaSignal);
            var minimaSignal = Rule.Create(c => c.Get<ZigZagMinima>(@params.zigZagThreshold)[c.Index].Tick != null);
            var minimas = ExecuteRule(candles, minimaSignal);
            var signals = new List<int>();

            if (maximas.First() < minimas.First())
            {
                minimas = minimas.Skip(1).ToArray();
            }

            bool buy = false;
            for(var index = 0; index < candles.Count; index++)
            {
                if (minimas.Any() && minimas.First() == index)
                {
                    buy = true;
                    minimas = minimas.Skip(1).ToArray();
                }
                    

                if (buy && maximas.Any() && maximas.First() == index)
                {
                    buy = false;
                    maximas = maximas.Skip(1).ToArray();
                }
                    

                if (buy)
                    signals.Add(index);
            }
            return signals.ToArray();
        }
    }
}
