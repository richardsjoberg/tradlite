using System;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Services.Signals.MovingAverage
{
    public class MovingAverageBase : SignalBase
    {
        public (int period1, int period2) ParseMaCrossParams(string @params)
        {
            return (@params.ParseJsonParam("period1", 8), @params.ParseJsonParam("period2", 14));
        }

        public int ParsePeriodCountParam(string @params)
        {
            return @params.ParseJsonParam("period", 14);
        }
    }
}
