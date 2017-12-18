using System;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Services.Signals.CandlePattern
{
    public class CandlePatternBase : SignalBase
    {
        public (bool containedShadows, int uptrendPeriodCount) ParseHaramiParams(string parameters)
        {
            return (parameters.ParseJsonParam("containedShadows", false), parameters.ParseJsonParam("uptrendPeriodCount", 3));
        }

        public (decimal dojiThreshold, decimal shadowThreshold) ParseDojiParams(string parameters)
        {
            return (parameters.ParseJsonParam("dojiThreshold", 0.1m), parameters.ParseJsonParam("shadowThreshold", 0.1m));
        }

        public (int trendPeriodCount, int periodCount, decimal longThreshold, decimal dojiThreshold, decimal threshold) ParseDojiStarParams(string parameters)
        {
            return (parameters.ParseJsonParam("trendPeriodCount", 3),
                parameters.ParseJsonParam("periodCount", 20),
                parameters.ParseJsonParam("longThreshold", 0.75m),
                parameters.ParseJsonParam("dojiThreshold", 0.25m),
                parameters.ParseJsonParam("threshold", 0.1m));
        }

        public (int trendPeriodCount, int periodCount, decimal longThreshold, decimal dojiThreshold) ParseAbandonedBabyParams(string parameters)
        {
            return (parameters.ParseJsonParam("trendPeriodCount", 3),
                parameters.ParseJsonParam("periodCount", 20),
                parameters.ParseJsonParam("longThreshold", 0.75m),
                parameters.ParseJsonParam("dojiThreshold", 0.25m));
        }

        public (int trendPeriodCount, decimal sizeThreshold) ParseTasukiGapParams(string parameters)
        {
            return (parameters.ParseJsonParam("upTrendPeriodCount", 3), parameters.ParseJsonParam("sizeThreshold", 0.1m));
        }

        public (int trendPeriodCount, int periodCount, decimal shortThreshold, decimal longThreshold, decimal threshold) ParseStarParams(string parameters)
        {
            return (parameters.ParseJsonParam("trendPeriodCount", 3),
                parameters.ParseJsonParam("periodCount", 20),
                parameters.ParseJsonParam("shortThreshold", 0.75m),
                parameters.ParseJsonParam("longThreshold", 0.25m),
                parameters.ParseJsonParam("threshold", 0.1m));
        }

        public (int trendPeriodCount, int periodCount, decimal shortThreshold, decimal longThreshold) ParseThreeParams(string parameters)
        {
            return (parameters.ParseJsonParam("trendPeriodCount", 20),
                parameters.ParseJsonParam("periodCount", 20),
                parameters.ParseJsonParam("shortThreshold", 0.25m),
                parameters.ParseJsonParam("longThreshold", 0.75m));
        }
    }
}
