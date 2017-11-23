using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;
using Trady.Analysis.Indicator;

namespace Tradlite.Services.Signals
{
    public interface IRsiService
    {
        int[] Overbought(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] Oversold(IReadOnlyList<IOhlcv> candles, string parameters);
    }
    public class RsiService : SignalBase, IRsiService
    {
        public int[] Overbought(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters);
            var signal = Rule.Create(c => c.Get<RelativeStrengthIndex>(@params.rsiPeriod)[c.Index].Tick.IsTrue(t => t > @params.rsiTreshold));

            return ExecuteRule(candles, signal);
        }

        public int[] Oversold(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters);
            var signal = Rule.Create(c => c.Get<RelativeStrengthIndex>(@params.rsiPeriod)[c.Index].Tick.IsTrue(t => t < @params.rsiTreshold));

            return ExecuteRule(candles, signal);
        }

        private (int rsiPeriod, int rsiTreshold) ParseParams(string @params)
        {
            return (@params.ParseJsonParam("rsiPeriod", 14), @params.ParseJsonParam("rsiTreshold", 70));
        }
    }
}
