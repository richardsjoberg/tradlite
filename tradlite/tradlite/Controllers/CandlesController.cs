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
using Tradlite.Services.CandleService;

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
        public async Task<IReadOnlyList<Candle>> Candles([FromQuery]string ticker, [FromQuery]DateTime? fromDate, [FromQuery]DateTime? toDate, [FromQuery]string importer = "Yahoo")
        {
            var candles = await _candleService.GetCandles(ticker, fromDate, toDate, importer);
            return candles;
        }
        
    }
}
