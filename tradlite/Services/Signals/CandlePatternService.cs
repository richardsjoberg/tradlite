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
        int[] Harami(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] BearishHarami(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] BullishHarami(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] DragonflyDoji(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] GravestoneDoji(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] Doji(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] EveningDojiStar(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] MorningDojiStar(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] BearishAbandonedBaby(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] BullishAbandonedBaby(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] BearishEngulfingPattern(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] BullishEngulfingPattern(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] DarkCloudCover(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] DownsideTasukiGap(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] UpsideTasukiGap(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] EveningStar(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] MorningStar(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] FallingThree(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] RisingThree(IReadOnlyList<IOhlcv> candles, string parameters);

    }
    public class CandlePatternService : SignalBase, ICandlePatternService
    {
        public int[] Harami(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var containedShadows = parameters.ParseJsonParam("containedShadows", false);
            return ExecuteRuleNullableBool<Harami>(candles, containedShadows);
        }

        public int[] BearishHarami(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseHaramiParams(parameters);
            return ExecuteRuleNullableBool<BearishHarami>(candles, @params.containedShadows, @params.uptrendPeriodCount);
        }

        public int[] BullishHarami(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseHaramiParams(parameters);
            return ExecuteRuleNullableBool<BullishHarami>(candles, @params.containedShadows, @params.uptrendPeriodCount);
        }

        private (bool containedShadows, int uptrendPeriodCount) ParseHaramiParams(string parameters)
        {
            return (parameters.ParseJsonParam("containedShadows", false), parameters.ParseJsonParam("uptrendPeriodCount", 3));
        }

        public int[] Doji(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var dojiTreshold = parameters.ParseJsonParam("dojiTreshold", 0.1m);
            return ExecuteRuleBool<Doji>(candles, dojiTreshold);
        }

        public int[] DragonflyDoji(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseDojiParams(parameters);
            return ExecuteRuleBool<DragonifyDoji>(candles, @params.dojiTreshold, @params.shadowTreshold);
        }

        public int[] GravestoneDoji(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseDojiParams(parameters);
            return ExecuteRuleBool<GravestoneDoji>(candles, @params.dojiTreshold, @params.shadowTreshold);
        }
        
        private (decimal dojiTreshold, decimal shadowTreshold) ParseDojiParams(string parameters)
        {
            return (parameters.ParseJsonParam("dojiTreshold", 0.1m), parameters.ParseJsonParam("shadowTreshold", 0.1m));
        }

        public int[] EveningDojiStar(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseDojiStarParams(parameters);
            return ExecuteRuleNullableBool<EveningDojiStar>(candles, @params.trendPeriodCount, @params.periodCount, @params.longTreshold, @params.dojiTreshold, @params.threshold);
        }

        public int[] MorningDojiStar(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseDojiStarParams(parameters);
            return ExecuteRuleNullableBool<MorningDojiStar>(candles, @params.trendPeriodCount, @params.periodCount, @params.longTreshold, @params.dojiTreshold, @params.threshold);
        }

        private (int trendPeriodCount, int periodCount, decimal longTreshold, decimal dojiTreshold, decimal threshold) ParseDojiStarParams(string parameters)
        {
            return (parameters.ParseJsonParam("trendPeriodCount", 3),
                parameters.ParseJsonParam("periodCount", 20),
                parameters.ParseJsonParam("longTreshold", 0.75m),
                parameters.ParseJsonParam("dojiTreshold", 0.25m),
                parameters.ParseJsonParam("threshold", 0.1m));
        }

        public int[] BearishAbandonedBaby(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseAbandonedBabyParams(parameters);
            return ExecuteRuleNullableBool<BearishAbandonedBaby>(candles, @params.trendPeriodCount, @params.periodCount, @params.longThreshold, @params.dojiThreshold);
        }

        public int[] BullishAbandonedBaby(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseAbandonedBabyParams(parameters);
            return ExecuteRuleNullableBool<BullishAbandonedBaby>(candles, @params.trendPeriodCount, @params.periodCount, @params.longThreshold, @params.dojiThreshold);
        }

        private (int trendPeriodCount, int periodCount, decimal longThreshold, decimal dojiThreshold) ParseAbandonedBabyParams(string parameters)
        {
            return (parameters.ParseJsonParam("trendPeriodCount", 3),
                parameters.ParseJsonParam("periodCount", 20),
                parameters.ParseJsonParam("longThreshold", 0.75m),
                parameters.ParseJsonParam("dojiThreshold", 0.25m));
        }

        public int[] BearishEngulfingPattern(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var upTrendPeriodCount = parameters.ParseJsonParam("upTrendPeriodCount", 3);
            return ExecuteRuleNullableBool<BearishEngulfingPattern>(candles, upTrendPeriodCount);
        }

        public int[] BullishEngulfingPattern(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var downTrendPeriodCount = parameters.ParseJsonParam("downTrendPeriodCount", 3);
            return ExecuteRuleNullableBool<BullishEngulfingPattern>(candles, downTrendPeriodCount);
        }
        

        public int[] DarkCloudCover(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var upTrendPeriodCount = parameters.ParseJsonParam("upTrendPeriodCount", 3);
            var downTrendPeriodCount = parameters.ParseJsonParam("downTrendPeriodCount", 3);
            var longPeriodCount = parameters.ParseJsonParam("longPeriodCount", 20);
            var longThreshold = parameters.ParseJsonParam("upTrendPeriodCount", 0.75m);
            return ExecuteRuleNullableBool<DarkCloudCover>(candles, upTrendPeriodCount, downTrendPeriodCount, longPeriodCount, longThreshold);
        }

        public int[] DownsideTasukiGap(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseTasukiGapParams(parameters);
            return ExecuteRuleNullableBool<DownsideTasukiGap>(candles, @params.trendPeriodCount, @params.sizeThreshold);
        }

        public int[] UpsideTasukiGap(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseTasukiGapParams(parameters);
            return ExecuteRuleNullableBool<UpsideTasukiGap>(candles, @params.trendPeriodCount, @params.sizeThreshold);
        }

        private (int trendPeriodCount, decimal sizeThreshold) ParseTasukiGapParams(string parameters)
        {
            return (parameters.ParseJsonParam("upTrendPeriodCount", 3), parameters.ParseJsonParam("sizeThreshold", 0.1m));
        }

        public int[] EveningStar(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseStarParams(parameters);
            return ExecuteRuleNullableBool<EveningStar>(candles, @params.trendPeriodCount, @params.periodCount, @params.shortThreshold, @params.longTreshold, @params.threshold);
        }

        public int[] MorningStar(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseStarParams(parameters);
            return ExecuteRuleNullableBool<MorningStar>(candles, @params.trendPeriodCount, @params.periodCount, @params.shortThreshold, @params.longTreshold, @params.threshold);
        }

        private (int trendPeriodCount, int periodCount, decimal shortThreshold, decimal longTreshold, decimal threshold) ParseStarParams(string parameters)
        {
            return (parameters.ParseJsonParam("trendPeriodCount", 3),
                parameters.ParseJsonParam("periodCount", 20),
                parameters.ParseJsonParam("shortThreshold", 0.75m),
                parameters.ParseJsonParam("longTreshold", 0.25m),
                parameters.ParseJsonParam("threshold", 0.1m));
        }

        public int[] FallingThree(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseThreeParams(parameters);
            return ExecuteRuleNullableBool<FallingThreeMethods>(candles, @params.trendPeriodCount, @params.periodCount, @params.shortThreshold, @params.longTreshold);
        }
        
        public int[] RisingThree(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseThreeParams(parameters);
            return ExecuteRuleNullableBool<RisingThreeMethods>(candles, @params.trendPeriodCount, @params.periodCount, @params.shortThreshold, @params.longTreshold);
        }

        private (int trendPeriodCount, int periodCount, decimal shortThreshold, decimal longTreshold) ParseThreeParams(string parameters)
        {
            return (parameters.ParseJsonParam("trendPeriodCount", 20),
                parameters.ParseJsonParam("periodCount", 20), 
                parameters.ParseJsonParam("shortThreshold", 0.25m), 
                parameters.ParseJsonParam("longThreshold", 0.75m));
        }
    }
}
