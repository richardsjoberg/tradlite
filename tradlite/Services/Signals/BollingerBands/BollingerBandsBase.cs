using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Analysis.Infrastructure;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals.BollingerBands
{
    public class BollingerBandsBase : SignalBase
    {
        public IReadOnlyList<AnalyzableTick<(decimal? LowerBand, decimal? MiddleBand, decimal? UpperBand)>> GetBollingerBands(IEnumerable<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters);
            var bollingerBands = new Trady.Analysis.Indicator.BollingerBands(candles, @params.period, @params.standardDeviation).Compute();
            return bollingerBands;
        }
         
        public (int period, decimal standardDeviation) ParseParams(string parameters)
        {
            return (parameters.ParseJsonParam("period", 14), parameters.ParseJsonParam("standardDeviation", 2));
        }
    }
}
