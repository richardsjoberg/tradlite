using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tradlite.Models;
using Microsoft.AspNetCore.Routing;
using Trady.Core;
using Trady.Importer;
using Trady.Analysis.Candlestick;
using Trady.Analysis;
using Tradlite.Services.Candle.CandleService;
using Tradlite.Models.Requests;
using Trady.Core.Infrastructure;

namespace Tradlite.Controllers
{
    
    public class CandlesController : Controller
    {
        private readonly ICandleService _candleService;

        public CandlesController(ICandleService candleService)
        {
            _candleService = candleService;
        }

        [Route("api/candles")]
        public async Task<IReadOnlyList<IOhlcvData>> Candles([FromQuery]CandleRequest request)
        {
            var candles = await _candleService.GetCandles(request.Ticker, request.FromDate, request.ToDate, request.Importer, request.Interval);
            return candles;
        }
        
    }
}
