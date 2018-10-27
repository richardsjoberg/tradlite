using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models;
using Tradlite.Models.Candle;
using Tradlite.Models.Requests;
using Trady.Core;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Candle.CandleService
{
    public interface ICandleService
    {
        Task<IReadOnlyList<IOhlcv>> GetCandles(CandleRequest candleRequest);
    }
    public class CandleService : ICandleService
    {
        private readonly Func<string, IImporter> _importerAccessor;
        private readonly IDbConnection _dbConnection;

        public CandleService(Func<string, IImporter> importerAccessor, IDbConnection dbConnection)
        {
            _importerAccessor = importerAccessor;
            _dbConnection = dbConnection;
        }
        // cache                       |-----------|
        // queries:     
        // p1                            |----|
        // p2                        |-----|
        // p3                                   |-----| 
        // p5                       |-----------------|
        // p7                  |--|
        // p8                                           |-----|
        public async Task<IReadOnlyList<IOhlcv>> GetCandles(CandleRequest request)
        {
            
            if(!request.FromDate.HasValue)
            {
                request.FromDate = GetDefaultFromDate(request.Interval);
            }

            if(!request.ToDate.HasValue)
            {
                request.ToDate = DateTime.Now;
            }

            if(request.FromDate.Value > request.ToDate.Value)
            {
                throw new ArgumentException("invalid request dates");
            }
            
            var tickerId = (await _dbConnection.QueryFirstOrDefaultAsync<int?>("select t.id from tickers t where t.symbol = @ticker and t.importer = @importer", new { request.Ticker, request.Importer }));
            
            if(!tickerId.HasValue)
            {
                return await _importerAccessor(request.Importer).ImportAsync(request.Ticker, request.FromDate.Value, request.ToDate.Value, request.Interval.ToTradyPeriod());
            }
            
            var candleKeysSql = @"select * from cachedCandleKeys cck 
                        where cck.TickerId = @tickerId
                        and cck.Interval = @interval";

            var cachedCandleKeys = (await _dbConnection.QueryAsync<CachedCandleKey>(candleKeysSql, new { tickerId, interval = request.Interval })).ToList();

            if(!cachedCandleKeys.Any())
            {
                var candles = await _importerAccessor(request.Importer).ImportAsync(request.Ticker, request.FromDate.Value, request.ToDate.Value, request.Interval.ToTradyPeriod());
                await CacheCandles(candles.ToList(), tickerId.Value, request.Interval);
                return candles;
            }
            
            var getCachedCandlesSql = @"select * from cachedCandles cc 
                            where cc.cachedCandleKeyId = @cachedCandleKeyId
                            order by datetime";
            if (cachedCandleKeys.Any(cck=>cck.FromDate <= request.FromDate && cck.ToDate >= request.ToDate)) //we have all candles cached: p1
            {
                var cachedCandleKeyId = cachedCandleKeys.First(cck => cck.FromDate <= request.FromDate && cck.ToDate >= request.ToDate).Id;
                var cachedCandles = (await _dbConnection.QueryAsync<CachedCandle>(getCachedCandlesSql, new { cachedCandleKeyId })).ToList()
                    .Where(cc => cc.DateTime.DateTime >= request.FromDate && cc.DateTime.DateTime <= request.ToDate).ToList();
                return cachedCandles.ToList();
            }
            else if(cachedCandleKeys.Any(cck => cck.FromDate > request.FromDate && cck.ToDate >= request.ToDate && cck.FromDate > request.ToDate)) // we miss some candles in the beginnig of a cached period (p2)
            {
                var cachedCandleKey = cachedCandleKeys.Single(cck => cck.FromDate > request.FromDate && cck.ToDate >= request.ToDate);

                var cachedCandleKeyId = cachedCandleKey.Id;
                var cachedCandles = (await _dbConnection.QueryAsync<CachedCandle>(getCachedCandlesSql, new { cachedCandleKeyId })).ToList();

                var candles = (await _importerAccessor(request.Importer).ImportAsync(request.Ticker, request.FromDate.Value, cachedCandleKey.FromDate, request.Interval.ToTradyPeriod()))
                    .Where(nc => !cachedCandles.Select(cc => new { cc.DateTime, cc.Interval }).Contains(new { nc.DateTime, Interval = request.Interval })).ToList().ToList();
                cachedCandleKey.FromDate = request.FromDate.Value.Date;
                await CacheCandles(candles, tickerId.Value, request.Interval, cachedCandleKey);
                    
                candles.AddRange(cachedCandles);
                return candles.Where(cc => cc.DateTime.DateTime >= request.FromDate && cc.DateTime.DateTime <= request.ToDate).ToList();
            }
            else if (cachedCandleKeys.Any(cck => cck.FromDate <= request.FromDate && cck.ToDate < request.ToDate && cck.ToDate >= request.FromDate)) // we miss some candles at the end of a cached period (p3)
            {
                var cachedCandleKey = cachedCandleKeys.Single(cck => cck.FromDate <= request.FromDate && cck.ToDate < request.ToDate);

                var cachedCandleKeyId = cachedCandleKey.Id;
                var cachedCandles = (await _dbConnection.QueryAsync<CachedCandle>(getCachedCandlesSql, new { cachedCandleKeyId })).ToList();

                var candles = (await _importerAccessor(request.Importer).ImportAsync(request.Ticker, cachedCandleKey.ToDate, request.ToDate.Value, request.Interval.ToTradyPeriod()))
                    .Where(nc => !cachedCandles.Select(cc => new { cc.DateTime, cc.Interval }).Contains(new { DateTime = nc.DateTime, Interval = request.Interval })).ToList();

                cachedCandleKey.ToDate = request.ToDate.Value.Date;
                await CacheCandles(candles, tickerId.Value, request.Interval, cachedCandleKey);
                
                var cachedCandlesOhlcv = cachedCandles.Cast<IOhlcv>().ToList();
                cachedCandlesOhlcv.AddRange(candles);
                cachedCandlesOhlcv = cachedCandlesOhlcv.Where(cc => cc.DateTime.DateTime >= request.FromDate && cc.DateTime.DateTime <= request.ToDate).ToList();
                return cachedCandlesOhlcv;
            }
            else if(cachedCandleKeys.Any(cck => cck.FromDate <= request.FromDate && cck.ToDate < request.ToDate && cck.ToDate < request.FromDate) || 
                cachedCandleKeys.Any(cck => cck.FromDate > request.FromDate && cck.ToDate >= request.ToDate && cck.FromDate > request.ToDate)) //request is outside of cache, don't cache because of complexity (p7,p8)
            {
                var candles = await _importerAccessor(request.Importer).ImportAsync(request.Ticker, request.FromDate.Value, request.ToDate.Value, request.Interval.ToTradyPeriod());
                return candles;
            }
            else //we wither have no cacheKeys or this is a p5, import all candles and delete old cacheKeys if any
            {
                if(cachedCandleKeys.Any())
                {
                    foreach(var cachedCandleKey in cachedCandleKeys)
                    {
                        await _dbConnection.DeleteAsync(cachedCandleKey);
                    }
                }

                var candles = await _importerAccessor(request.Importer).ImportAsync(request.Ticker, request.FromDate.Value, request.ToDate.Value, request.Interval.ToTradyPeriod());
                await CacheCandles(candles.ToList(), tickerId.Value, request.Interval);
                return candles.Where(cc => cc.DateTime.DateTime >= request.FromDate && cc.DateTime.DateTime <= request.ToDate).ToList();
            }
        }

        private DateTime? GetDefaultFromDate(string interval)
        {
            switch(interval.ToTradyPeriod())
            {
                case Trady.Core.Period.PeriodOption.Monthly:
                    return DateTime.Now.AddMonths(-60).Date;
                case Trady.Core.Period.PeriodOption.Weekly:
                    return DateTime.Now.AddDays(-7 * 260).Date;
                case Trady.Core.Period.PeriodOption.Daily:
                    return DateTime.Now.AddDays(-365).Date;
                case Trady.Core.Period.PeriodOption.Hourly:
                    return DateTime.Now.AddDays(-14);
                case Trady.Core.Period.PeriodOption.BiHourly:
                    return DateTime.Now.AddDays(-56);
                case Trady.Core.Period.PeriodOption.Per30Minute:
                    return DateTime.Now.AddDays(-7);
                case Trady.Core.Period.PeriodOption.Per15Minute:
                    return DateTime.Now.AddDays(-4);
                case Trady.Core.Period.PeriodOption.PerMinute:
                    return DateTime.Now.AddHours(-6);
            }
            throw new ArgumentException("invalid interval");
        }

        public async Task CacheCandles(IReadOnlyList<IOhlcv> candles, int tickerId, string interval, CachedCandleKey cachedCandleKey = null)
        {
            var _candles = candles.ToList(); //hack in order to be able to remove last candle (candles is a reference variable) 
            if (!_candles.Any())
            {
                return;
            }

            var lastCandle = _candles.Last();
            if (lastCandle.DateTime.Date == DateTime.Now.Date)
            {
                _candles.Remove(_candles.Last()); //dont cache last candle in case market is open
            }

            if (!_candles.Any())
            {
                return;
            }

            int cachedCandleKeyId;
            if (cachedCandleKey != null)
            {
                cachedCandleKeyId = cachedCandleKey.Id;
                await _dbConnection.UpdateAsync(cachedCandleKey);
            }
            else
            {
                cachedCandleKeyId = (await _dbConnection.InsertAsync(new CachedCandleKey
                {
                    FromDate = _candles.Min(c => c.DateTime.LocalDateTime),
                    ToDate = _candles.Max(c => c.DateTime.LocalDateTime),
                    TickerId = tickerId,
                    Interval = interval
                }));
            }

            var cachedCandles = _candles.Select(c => new CachedCandle(c, interval, cachedCandleKeyId));
            var candleId = await _dbConnection.InsertAsync(cachedCandles);
        }
    }
}
