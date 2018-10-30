using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.ZigZag
{
    public class ZigZagShortSignal : ZigZagBase, ISignalService
    {
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters, candles);
            var maximaSignal = Rule.Create(c => c.Get<ZigZagMaxima>(@params.zigZagThreshold)[c.Index].Tick != null);
            var maximas = ExecuteRule(candles, maximaSignal);
            var minimaSignal = Rule.Create(c => c.Get<ZigZagMinima>(@params.zigZagThreshold)[c.Index].Tick != null);
            var minimas = ExecuteRule(candles, minimaSignal);
            var signals = new List<int>();

            if(minimas.First() < maximas.First())
            {
                minimas = minimas.Skip(1).ToArray();
            }

            bool sell = false;
            for(var index = 0; index < candles.Count; index++)
            {
                if (maximas.Any() && maximas.First() == index)
                {
                    sell = true;
                    maximas = maximas.Skip(1).ToArray();
                }
                    
                if (sell && minimas.Any() && minimas.First() == index)
                {
                    sell = false;
                    minimas = minimas.Skip(1).ToArray();
                }
                    
                if (sell)
                    signals.Add(index);
            }
            return signals.ToArray();
        }
    }
}
