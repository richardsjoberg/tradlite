using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Services.Signals.DirectionalMovement
{
    public class DirectionalMovementBase : SignalBase
    {
        public Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, int adxThreshold, decimal? previousAdxTick), bool>
            UpTrend = (@params) =>
            {
                return @params.pdiTick.Value > @params.mdiTick.Value &&
                            @params.adxTick.Value > @params.adxThreshold &&
                            @params.adxTick.Value > @params.previousAdxTick.Value;
            };
        public Func<(decimal? pdiTick, decimal? mdiTick, decimal? adxTick, int adxThreshold, decimal? previousAdxTick), bool>
            DownTrend = (@params) =>
            {
                return @params.pdiTick.Value < @params.mdiTick.Value &&
                            @params.adxTick.Value > @params.adxThreshold &&
                            @params.adxTick.Value > @params.previousAdxTick.Value;
            };

        public (int AdxPeriod, int MdiPeriod, int PdiPeriod, int AdxThreshold, bool Bullish) ParseParams(string @params)
        {
            return (@params.ParseJsonParam("adxPeriod", 8),
                @params.ParseJsonParam("mdiPeriod", 8),
                @params.ParseJsonParam("pdiPeriod", 8),
                @params.ParseJsonParam("adxThreshold", 20),
                @params.ParseJsonParam("bullish", true));
        }
    }
}
