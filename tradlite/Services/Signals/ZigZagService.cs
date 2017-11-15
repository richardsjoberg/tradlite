using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;
using Newtonsoft.Json.Linq;

namespace Tradlite.Services.Signals
{
    public interface IZigZagService
    {
        int[] Maximas(IReadOnlyList<IOhlcv> candles, string extraParams);
        int[] Minimas(IReadOnlyList<IOhlcv> candles, string extraParams);
        int[] Support(IReadOnlyList<IOhlcv> candles, string extraParams);
        int[] Resistance(IReadOnlyList<IOhlcv> candles, string extraParams);
    }
    public class ZigZagService : IZigZagService
    {
        public int[] Maximas(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseParams(extraParams);
            var signal = Rule.Create(c => c.Get<ZigZagMaxima>(@params.zigZagTreshold)[c.Index].Tick != null);

            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        public int[] Minimas(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseParams(extraParams);
            var signal = Rule.Create(c => c.Get<ZigZagMinima>(@params.zigZagTreshold)[c.Index].Tick != null);

            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        public int[] Resistance(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseParams(extraParams);
            var signal = Rule.Create(c => c.Get<ZigZagResistance>(@params.zigZagTreshold, @params.turningPointMargin, @params.requiredNumberOfTurningPoints)[c.Index].Tick);

            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        public int[] Support(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseParams(extraParams);
            var signal = Rule.Create(c => c.Get<ZigZagSupport>(@params.zigZagTreshold, @params.turningPointMargin, @params.requiredNumberOfTurningPoints)[c.Index].Tick);

            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        private (decimal zigZagTreshold, decimal turningPointMargin, int requiredNumberOfTurningPoints) ParseParams(string @params)
        {
            var parsedParams = (zigZagTreshold: 0.03m, turningPointMargin: 0.007m, requiredNumberOfTurningPoints: 2);
            if (!string.IsNullOrEmpty(@params))
            {
                var extraParam = JObject.Parse(@params);
                if (extraParam["zigZagTreshold"] != null)
                    decimal.TryParse(extraParam["zigZagTreshold"].ToString(), out parsedParams.zigZagTreshold);
                if (extraParam["turningPointMargin"] != null)
                    decimal.TryParse(extraParam["turningPointMargin"].ToString(), out parsedParams.turningPointMargin);
                if (extraParam["requiredNumberOfTurningPoints"] != null)
                    int.TryParse(extraParam["requiredNumberOfTurningPoints"].ToString(), out parsedParams.requiredNumberOfTurningPoints);
            }
            return parsedParams;
        }

        
    }
}
