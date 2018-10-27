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
using Tradlite.Services.Ig;
using Tradlite.Models.VolatilitySpreadRatio;
using System.Net.Http;

namespace Tradlite.Controllers
{
    public class VolatilityController : Controller
    {
        private readonly ICandleService _candleService;
        private readonly IIgService _igService;
        private readonly Func<string, ISignalService> _signalServiceAccessor;

        public VolatilityController(ICandleService candleService, IIgService igService)
        {
            _candleService = candleService;
            _igService = igService;
        }
        
        [Route("api/volatilityspreadratio")]
        public async Task<VolatilitySpreadRatioResponse> GetVolatilitySpreadRatio([FromQuery]CandleRequest request)
        {
            if(request.Importer != "Ig")
            {
                throw new NotSupportedException();
            }

            var candles = await _candleService.GetCandles(request);
            if (!candles.Any())
            {
                HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                return new VolatilitySpreadRatioResponse();
            }

            var atrs = candles.Atr(candles.Count - 1);
            if(!atrs.Any() || !atrs.Last().Tick.HasValue)
            {
                HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                return new VolatilitySpreadRatioResponse();
            }

            var atr = atrs.Last();
            var igClient = await _igService.GetIgClient();
            var marketDetails = await igClient.marketDetailsV2(request.Ticker);
            if(marketDetails.StatusCode != System.Net.HttpStatusCode.OK)
            {
                HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                return new VolatilitySpreadRatioResponse();
            }
            var marketStatus = marketDetails.Response.snapshot.marketStatus;
            var spread = marketDetails.Response.snapshot.offer - marketDetails.Response.snapshot.bid;
            
            var response = new VolatilitySpreadRatioResponse
            {
                Atr = atr.Tick.GetValueOrDefault(),
                Spread = spread.GetValueOrDefault(),
                Ratio = atr.Tick.HasValue && spread.HasValue && spread.Value != 0 ? atr.Tick.Value / spread.Value : 0,
                MarketStatus = marketStatus
            };
            return response;
        }
    }
}
