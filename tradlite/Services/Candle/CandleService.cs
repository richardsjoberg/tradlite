using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models;
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
        public async Task<IReadOnlyList<IOhlcvData>> GetCandles(string ticker, DateTime? fromDate, DateTime? toDate, string importer = "Yahoo", string interval = "DAY")
        {
            if(!fromDate.HasValue)
            {
                fromDate = DateTime.Now.AddDays(-365);
            }

            if(!toDate.HasValue)
            {
                toDate = DateTime.Now;
            }
            //return await _serviceAccessor(importer).ImportAsync(ticker, fromDate.Value, toDate.Value, interval.ToTradyPeriod());
            

            var tickerId = (await _dbConnection.QueryFirstAsync<int?>("select t.id from tickers t where t.symbol = @ticker and t.importer = @importer", new { ticker, importer }));
            
            if(!tickerId.HasValue)
            {
                tickerId = await _dbConnection.InsertAsync(new Ticker { Symbol = ticker, Importer = importer, Name = ticker, Tags = "Cache" });
            }

            var sql = @"select c.* from tickers t 
                        inner join candles_tickers ct on t.id = ct.tickerId
                        inner join candles c on ct.candleId = c.id
                        where t.Symbol = @ticker 
                        and t.importer = @importer";

            var cachedCandles = (await _dbConnection.QueryAsync<CachedCandle>(sql, new { ticker, importer })).ToList();
            if(cachedCandles.Any())
            {
                var cachedFrom = cachedCandles.Min(c => c.DateTime);
                var cachedTo = cachedCandles.Max(c => c.DateTime);
                if (cachedTo < fromDate || cachedFrom > toDate)
                {
                    var candles = await _serviceAccessor(importer).ImportAsync(ticker, fromDate.Value, toDate.Value, interval.ToTradyPeriod());
                    await CacheCandles(candles, tickerId.Value, interval);
                    return candles;
                }
                else if (fromDate >= cachedFrom && toDate <= cachedTo)
                {
                    return cachedCandles.Where(c => c.DateTime >= fromDate && c.DateTime <= toDate).ToList();
                }
                else
                {
                    if (cachedTo < toDate)
                    {
                        var newCandles = (await _serviceAccessor(importer).ImportAsync(ticker, cachedTo, toDate, interval.ToTradyPeriod()))
                            .Where(nc => !cachedCandles.Select(cc => new { cc.DateTime, cc.Interval }).Contains(new { nc.DateTime, Interval = interval })).ToList();
                        await CacheCandles(newCandles, tickerId.Value, interval);
                        cachedCandles.AddRange(newCandles.Select(c => new CachedCandle(c, interval)));
                    }
                    if (fromDate < cachedFrom)
                    {
                        var newCandles = (await _serviceAccessor(importer).ImportAsync(ticker, fromDate, cachedFrom, interval.ToTradyPeriod()))
                            .Where(nc => !cachedCandles.Select(cc => new { cc.DateTime, cc.Interval }).Contains(new { nc.DateTime, Interval = interval })).ToList();
                        await CacheCandles(newCandles, tickerId.Value, interval);
                        cachedCandles.AddRange(newCandles.Select(c => new CachedCandle(c, interval)));
                    }
                    return cachedCandles.Distinct().ToList();
                } 
                
            }
            else
            {
                var candles = await _serviceAccessor(importer).ImportAsync(ticker, fromDate.Value, toDate.Value, interval.ToTradyPeriod());
                await CacheCandles(candles, tickerId.Value, interval);
                return candles;
            }
        }

        public async Task CacheCandles(IReadOnlyList<IOhlcvData> candles, int tickerId, string interval)
        {
            var cachedCandles = candles.Select(c => new CachedCandle(c, interval));
            foreach(var cachedCandle in cachedCandles)
            {
                var candleId = await _dbConnection.InsertAsync(cachedCandle);
                await _dbConnection.ExecuteAsync("insert into candles_tickers(tickerId, candleId) values (@tickerId, @candleId)", new { candleId, tickerId });
            }

        }
    }
}
