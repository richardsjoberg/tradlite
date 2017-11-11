using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models;
using Tradlite.Models.Candle;
using Trady.Core;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.Candle.CandleService
{
    public interface ICandleService
    {
        Task<IReadOnlyList<IOhlcvData>> GetCandles(string ticker, DateTime? fromDate, DateTime? toDate, string importer = "Yahoo", string interval = "DAY");
    }
    public class CandleService : ICandleService
    {
        private readonly Func<string, IImporter> _serviceAccessor;
        private readonly IDbConnection _dbConnection;

        public CandleService(Func<string, IImporter> serviceAccessor, IDbConnection dbConnection)
        {
            _serviceAccessor = serviceAccessor;
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
        public async Task<IReadOnlyList<IOhlcvData>> GetCandles(string ticker, DateTime? fromDate, DateTime? toDate, string importer = "Yahoo", string interval = "DAY")
        {
            if(!fromDate.HasValue)
            {
                fromDate = DateTime.Now.AddDays(-365).Date;
            }

            if(!toDate.HasValue)
            {
                toDate = DateTime.Now;
            }
            
            var tickerId = (await _dbConnection.QueryFirstAsync<int?>("select t.id from tickers t where t.symbol = @ticker and t.importer = @importer", new { ticker, importer }));
            
            if(!tickerId.HasValue)
            {
                return await _serviceAccessor(importer).ImportAsync(ticker, fromDate.Value, toDate.Value, interval.ToTradyPeriod());
            }

            var candleKeysSql = @"select * from cachedCandleKeys cck 
                        where cck.TickerId = @tickerId";

            var cachedCandleKeys = (await _dbConnection.QueryAsync<CachedCandleKey>(candleKeysSql, new { tickerId })).ToList();

            if(!cachedCandleKeys.Any())
            {
                var candles = await _serviceAccessor(importer).ImportAsync(ticker, fromDate.Value, toDate.Value, interval.ToTradyPeriod());
                await CacheCandles(candles.ToList(), tickerId.Value, interval);
                return candles;
            }
            
            var getCachedCandlesSql = @"select * from cachedCandles cc 
                            where cc.cachedCandleKeyId = @cachedCandleKeyId";
            if (cachedCandleKeys.Any(cck=>cck.FromDate <= fromDate && cck.ToDate >= toDate)) //we have all candles cached: p1
            {
                var cachedCandleKeyId = cachedCandleKeys.First(cck => cck.FromDate <= fromDate && cck.ToDate >= toDate).Id;
                var cachedCandles = (await _dbConnection.QueryAsync<CachedCandle>(getCachedCandlesSql, new { cachedCandleKeyId}))
                    .Where(cc => cc.DateTime >= fromDate && cc.DateTime <= toDate).ToList();
                return cachedCandles.ToList();
            }
            else if(cachedCandleKeys.Any(cck => cck.FromDate > fromDate && cck.ToDate >= toDate)) // we miss some candles in the beginnig of a cached period (p2, p7)
            {
                var cachedCandleKey = cachedCandleKeys.Single(cck => cck.FromDate > fromDate && cck.ToDate >= toDate);

                var cachedCandleKeyId = cachedCandleKey.Id;
                var cachedCandles = (await _dbConnection.QueryAsync<CachedCandle>(getCachedCandlesSql, new { cachedCandleKeyId })).ToList();

                var candles = (await _serviceAccessor(importer).ImportAsync(ticker, fromDate.Value, cachedCandleKey.FromDate, interval.ToTradyPeriod()))
                    .Where(nc => !cachedCandles.Select(cc => new { cc.DateTime, cc.Interval }).Contains(new { nc.DateTime, Interval = interval })).ToList().ToList();
                cachedCandleKey.FromDate = fromDate.Value;
                await CacheCandles(candles, tickerId.Value, interval, cachedCandleKey);
                    
                candles.AddRange(cachedCandles);
                return candles.Where(cc => cc.DateTime >= fromDate && cc.DateTime <= toDate).ToList();
            }
            else if (cachedCandleKeys.Any(cck => cck.FromDate <= fromDate && cck.ToDate < toDate)) // we miss some candles at the end of a cached period (p3, p8)
            {
                var cachedCandleKey = cachedCandleKeys.Single(cck => cck.FromDate <= fromDate && cck.ToDate < toDate);

                var cachedCandleKeyId = cachedCandleKey.Id;
                var cachedCandles = (await _dbConnection.QueryAsync<CachedCandle>(getCachedCandlesSql, new { cachedCandleKeyId }));

                var candles = (await _serviceAccessor(importer).ImportAsync(ticker, cachedCandleKey.ToDate, toDate.Value, interval.ToTradyPeriod()))
                    .Where(nc => !cachedCandles.Select(cc => new { cc.DateTime, cc.Interval }).Contains(new { nc.DateTime, Interval = interval })).ToList();

                cachedCandleKey.ToDate = toDate.Value;
                await CacheCandles(candles, tickerId.Value, interval, cachedCandleKey);
                    
                candles.AddRange(cachedCandles);
                return candles.Where(cc => cc.DateTime >= fromDate && cc.DateTime <= toDate).ToList();
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

                var candles = await _serviceAccessor(importer).ImportAsync(ticker, fromDate.Value, toDate.Value, interval.ToTradyPeriod());
                await CacheCandles(candles.ToList(), tickerId.Value, interval);
                return candles;
            }
        }

        public async Task CacheCandles(List<IOhlcvData> candles, int tickerId, string interval, CachedCandleKey cachedCandleKey = null)
        {
            if(!candles.Any())
            {
                return;
            }
            var lastCandle = candles.Last();
            if (lastCandle.DateTime.Date == DateTime.Now.Date)
            {
                candles.Remove(candles.Last()); //dont cache last candle in case market is open
            }
            int cachedCandleKeyId;
            if(cachedCandleKey != null)
            {
                cachedCandleKeyId = cachedCandleKey.Id;
                await _dbConnection.UpdateAsync(cachedCandleKey);
            } 
            else
            {
                cachedCandleKeyId = (await _dbConnection.InsertAsync(new CachedCandleKey
                {
                    FromDate = candles.Min(c => c.DateTime),
                    ToDate = candles.Max(c => c.DateTime),
                    TickerId = tickerId
                }));
            }
            
            var cachedCandles = candles.Select(c => new CachedCandle(c, interval, cachedCandleKeyId));
            var candleId = await _dbConnection.InsertAsync(cachedCandles);
        }
    }
}
