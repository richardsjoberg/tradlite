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
        int[] Overbought(IReadOnlyList<IOhlcv> candles, string extraParams);
        int[] Oversold(IReadOnlyList<IOhlcv> candles, string extraParams);
    }
    public class RsiService : IRsiService
    {
        public int[] Overbought(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseParams(extraParams);
            var signal = Rule.Create(c => c.Get<RelativeStrengthIndex>(@params.rsiPeriod)[c.Index].Tick.IsTrue(t => t > @params.rsiTreshold));
                
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        public int[] Oversold(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseParams(extraParams);
            var signal = Rule.Create(c => c.Get<RelativeStrengthIndex>(@params.rsiPeriod)[c.Index].Tick.IsTrue(t => t < @params.rsiTreshold));

            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        private (int rsiPeriod, int rsiTreshold) ParseParams(string @params)
        {
            var parsedParams = (rsiPeriod: 14, rsiTreshold: 70);
            if (!string.IsNullOrEmpty(@params))
            {
                var extraParam = JObject.Parse(@params);
                if (extraParam["rsiPeriod"] != null)
                    int.TryParse(extraParam["rsiPeriod"].ToString(), out parsedParams.rsiPeriod);
                if(extraParam["rsiTreshold"] != null)
                    int.TryParse(extraParam["rsiTreshold"].ToString(), out parsedParams.rsiTreshold);
            }
            return parsedParams;
        }
    }
}
