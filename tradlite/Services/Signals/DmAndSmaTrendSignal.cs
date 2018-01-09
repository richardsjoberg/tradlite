using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Signals
{
    public class DmAndSmaTrendSignal : SignalBase, ISignalService
    {
        private readonly Func<string, ISignalService> _signalServiceAccessor;

        public DmAndSmaTrendSignal(Func<string, ISignalService> signalServiceAccessor)
        {
            _signalServiceAccessor = signalServiceAccessor;
        }
        public int[] GetSignals(IReadOnlyList<IOhlcv> candles, string parameters)
        {
            var bullish = parameters.ParseJsonParam("bullish", true);
            var trendSignals = _signalServiceAccessor("TrendSignal").GetSignals(candles, parameters);
            var smaService = bullish ? _signalServiceAccessor("PositiveSmaSignal") : _signalServiceAccessor("NegativeSmaSignal");
            var smaSignals = smaService.GetSignals(candles, parameters);
            var intersection = trendSignals.Intersect(smaSignals).ToArray();
            return intersection;
        }
    }
}
