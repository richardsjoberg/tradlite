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
        private readonly IDirectionalMovementService _mdiPdiService;
        private readonly IRsiService _rsiService;
        private readonly IZigZagService _zigZagService;
        private readonly ICandlePatternService _candlePatternService;
        private readonly IMovingAverageService _movingAverageService;

        public SignalController(ICandleService candleService, IDirectionalMovementService mdiPdiService, IRsiService rsiService, IZigZagService zigZagService, ICandlePatternService candlePatternService, IMovingAverageService movingAverageService)
        {
            _candleService = candleService;
            _mdiPdiService = mdiPdiService;
            _rsiService = rsiService;
            _zigZagService = zigZagService;
            _candlePatternService = candlePatternService;
            _movingAverageService = movingAverageService;
        }
        [Route("api/signal/bullishharami")]
        public async Task<int[]> BullishHarami([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.BullishHarami(candles, request.Parameters);
        }

        [Route("api/signal/bearishharami")]
        public async Task<int[]> BearishHarami([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.BearishHarami(candles, request.Parameters);
        }

        [Route("api/signal/doji")]
        public async Task<int[]> Doji([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.Doji(candles, request.Parameters);
        }

        [Route("api/signal/dragonflydoji")]
        public async Task<int[]> DragonFlyDoji([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.DragonflyDoji(candles, request.Parameters);
        }

        [Route("api/signal/gravestonedoji")]
        public async Task<int[]> GravestoneDoji([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.GravestoneDoji(candles, request.Parameters);
        }
        
        [Route("api/signal/eveningdojistar")]
        public async Task<int[]> EveningDojiStar([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.EveningDojiStar(candles, request.Parameters);
        }
        [Route("api/signal/morningdojistar")]
        public async Task<int[]> MorningDojiStar([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.MorningDojiStar(candles, request.Parameters);
        }
        [Route("api/signal/bearishabandonedbaby")]
        public async Task<int[]> BearishAbandonedBaby([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.BearishAbandonedBaby(candles, request.Parameters);
        }
        [Route("api/signal/bullishabandonedbaby")]
        public async Task<int[]> BullishAbandonedBaby([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.BullishAbandonedBaby(candles, request.Parameters);
        }
        [Route("api/signal/bearishengulfingpattern")]
        public async Task<int[]> BearishEngulfingPattern([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.BearishEngulfingPattern(candles, request.Parameters);
        }
        [Route("api/signal/bullishengulfingpattern")]
        public async Task<int[]> BullishEngulfingPattern([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.BullishEngulfingPattern(candles, request.Parameters);
        }
        [Route("api/signal/darkcloudcover")]
        public async Task<int[]> DarkCloudCover([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.DarkCloudCover(candles, request.Parameters);
        }
        [Route("api/signal/downsidetasukigap")]
        public async Task<int[]> DownsideTasukiGap([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.DownsideTasukiGap(candles, request.Parameters);
        }
        [Route("api/signal/upsidetasukigap")]
        public async Task<int[]> UpsideTasukiGap([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.UpsideTasukiGap(candles, request.Parameters);
        }
        [Route("api/signal/eveningstar")]
        public async Task<int[]> EveningStar([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.EveningStar(candles, request.Parameters);
        }
        [Route("api/signal/morningstar")]
        public async Task<int[]> MorningStar([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.MorningStar(candles, request.Parameters);
        }
        [Route("api/signal/fallingthree")]
        public async Task<int[]> FallingThree([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.FallingThree(candles, request.Parameters);
        }
        [Route("api/signal/risingthree")]
        public async Task<int[]> RisingThree([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            return _candlePatternService.GravestoneDoji(candles, request.Parameters);
        }
        
        [Route("api/signal/rsioverbought")]
        public async Task<int[]> RsiOverbought([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _rsiService.Overbought(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/rsioversold")]
        public async Task<int[]> RsiOversold([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _rsiService.Oversold(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/mdipdicrosswithtrendingadx")]
        public async Task<int[]> MdiDdiCrossAndTrendingAdx([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _mdiPdiService.MdiPdiCrossAndTrendingAdx(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/mdipditrend")]
        public async Task<int[]> MdiPdiTrend([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _mdiPdiService.Trend(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/mdipdinewtrend")]
        public async Task<int[]> MdiPdiNewTrend([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _mdiPdiService.NewTrend(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/zigzagmaxima")]
        public async Task<int[]> ZigZagMaxima([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _zigZagService.Maximas(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/zigzagminima")]
        public async Task<int[]> ZigZagMinima([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _zigZagService.Minimas(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/zigzagresistance")]
        public async Task<int[]> ZigZagResistance([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _zigZagService.Resistance(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/zigzagsupport")]
        public async Task<int[]> ZigZagSupport([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _zigZagService.Support(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/closeabovesma")]
        public async Task<int[]> CloseAboveSma([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _movingAverageService.CloseAboveSma(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/closebelowsma")]
        public async Task<int[]> CloseBelowSma([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _movingAverageService.CloseBelowSma(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/closeaboveema")]
        public async Task<int[]> CloseAboveEma([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _movingAverageService.CloseAboveEma(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/closebelowema")]
        public async Task<int[]> CloseBelowEma([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _movingAverageService.CloseBelowEma(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/smacross")]
        public async Task<int[]> SmaCross([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _movingAverageService.SmaCross(candles, request.Parameters);
            return indicies;
        }

        [Route("api/signal/emacross")]
        public async Task<int[]> EmaCross([FromQuery]SignalRequest request)
        {
            var candles = await _candleService.GetCandles(request);
            var indicies = _movingAverageService.EmaCross(candles, request.Parameters);
            return indicies;
        }
    }
}
