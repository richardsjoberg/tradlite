using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models.Requests;
using Tradlite.Services.Candle.CandleService;
using Trady.Analysis;
using Trady.Analysis.Candlestick;
using Trady.Analysis.Indicator;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;
using Tradlite.Services.Signals;

namespace Tradlite.Controllers
{
    public class SignalController : Controller
    {
        private readonly ICandleService _candleService;
        private readonly Func<string, ISignalService> _signalServiceAccessor;

        public SignalController(ICandleService candleService, Func<string, ISignalService> signalServiceAccessor)
        {
            _candleService = candleService;
            _signalServiceAccessor = signalServiceAccessor;
        }
        
        [Route("api/signal/{signalService}")]
        public async Task<int[]> GetSignals([FromQuery]SignalRequest request, string signalService)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _signalServiceAccessor(signalService).GetSignals(candles, request.Parameters);
            return indicies;
        }
    }
}
