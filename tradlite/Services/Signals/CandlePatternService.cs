using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Analysis.Candlestick;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;
using Newtonsoft.Json.Linq;
using Trady.Analysis.Infrastructure;

namespace Tradlite.Services.Signals
{
    public interface ICandlePatternService 
    {
        int[] BearishHarami(IReadOnlyList<IOhlcv> candles, string extraParams);
        int[] BullishhHarami(IReadOnlyList<IOhlcv> candles, string extraParams);
        int[] DragonflyDoji(IReadOnlyList<IOhlcv> candles, string extraParams);
        int[] GravestoneDoji(IReadOnlyList<IOhlcv> candles, string extraParams);
        int[] Doji(IReadOnlyList<IOhlcv> candles, string extraParams);
    }
    public class CandlePatternService : SignalBase, ICandlePatternService
    {
        public int[] BearishHarami(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseHaramiParams(extraParams);
            return ExecuteRuleNullableBool<BearishHarami>(candles, @params.containedShadows, @params.uptrendPeriodCount);
        }

        public int[] BullishhHarami(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseHaramiParams(extraParams);
            return ExecuteRuleNullableBool<BullishHarami>(candles, @params.containedShadows, @params.uptrendPeriodCount);
        }
        
        private (bool containedShadows, int uptrendPeriodCount) ParseHaramiParams(string @params)
        {
            var parsedParams = (containedShadows: false, uptrendPeriodCount: 3);

            if (!string.IsNullOrEmpty(@params))
            {
                var extraParam = JObject.Parse(@params);
                if (extraParam["containedShadows"] != null)
                    bool.TryParse(extraParam["containedShadows"].ToString(), out parsedParams.containedShadows);
                if (extraParam["uptrendPeriodCount"] != null)
                    int.TryParse(extraParam["uptrendPeriodCount"].ToString(), out parsedParams.uptrendPeriodCount);
            }
            return parsedParams;
        }

        public int[] Doji(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseDojiParams(extraParams);
            return ExecuteRuleBool<Doji>(candles, @params.dojiThreshold);
        }

        public int[] DragonflyDoji(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseDojiParams(extraParams);
            return ExecuteRuleBool<DragonifyDoji>(candles, @params.dojiThreshold, @params.shadowThreshold);
        }

        public int[] GravestoneDoji(IReadOnlyList<IOhlcv> candles, string extraParams)
        {
            var @params = ParseDojiParams(extraParams);
            return ExecuteRuleBool<GravestoneDoji>(candles, @params.dojiThreshold, @params.shadowThreshold);
        }
        
        private (decimal dojiThreshold, decimal shadowThreshold) ParseDojiParams(string @params)
        {
            var parsedParams = (dojiThreshold: 0.1m, shadowThreshold: 0.1m);

            if (!string.IsNullOrEmpty(@params))
            {
                var extraParam = JObject.Parse(@params);
                if (extraParam["dojiTreshold"] != null)
                    decimal.TryParse(extraParam["dojiTreshold"].ToString(), out parsedParams.dojiThreshold);
                if (extraParam["shadowThreshold"] != null)
                    decimal.TryParse(extraParam["shadowTreshold"].ToString(), out parsedParams.shadowThreshold);
            }
            return parsedParams;
        }
    }
}
