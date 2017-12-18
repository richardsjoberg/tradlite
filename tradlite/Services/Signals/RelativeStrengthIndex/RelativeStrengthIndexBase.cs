using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Services.Signals.RelativeStrengthIndex
{
    public class RelativeStrengthIndexBase : SignalBase
    {
        public (int rsiPeriod, int rsiThreshold) ParseParams(string @params)
        {
            return (@params.ParseJsonParam("rsiPeriod", 14), @params.ParseJsonParam("rsiThreshold", 70));
        }
    }
}
