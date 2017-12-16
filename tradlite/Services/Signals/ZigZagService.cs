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
        int[] Maximas(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] Minimas(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] Support(IReadOnlyList<IOhlcv> candles, string parameters);
        int[] Resistance(IReadOnlyList<IOhlcv> candles, string parameters);
    }
    public class ZigZagService : SignalBase, IZigZagService
    {
        public int[] Maximas(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters);
            var signal = Rule.Create(c => c.Get<ZigZagMaxima>(@params.zigZagThreshold)[c.Index].Tick != null);

            return ExecuteRule(candles, signal);
        }

        public int[] Minimas(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters);
            var signal = Rule.Create(c => c.Get<ZigZagMinima>(@params.zigZagThreshold)[c.Index].Tick != null);

            return ExecuteRule(candles, signal);
        }

        public int[] Resistance(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters);
            var signal = Rule.Create(c => c.Get<ZigZagResistance>(@params.zigZagThreshold, @params.turningPointMargin, @params.requiredNumberOfTurningPoints)[c.Index].Tick);

            return ExecuteRule(candles, signal);
        }

        public int[] Support(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var @params = ParseParams(parameters);
            var signal = Rule.Create(c => c.Get<ZigZagSupport>(@params.zigZagThreshold, @params.turningPointMargin, @params.requiredNumberOfTurningPoints)[c.Index].Tick);

            return ExecuteRule(candles, signal);
        }

        private (decimal zigZagThreshold, decimal turningPointMargin, int requiredNumberOfTurningPoints) ParseParams(string @params)
        {
            return (@params.ParseJsonParam("zigZagThreshold", 0.03m), 
                @params.ParseJsonParam("turningPointMargin", 0.007m), 
                @params.ParseJsonParam("requiredNumberOfTurningPoints", 2));
            
        }

        
    }
}
