using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core;
using Trady.Core.Infrastructure;

namespace Tradlite.Services.CandleService
{
    public interface ICandleService
    {
        Task<IReadOnlyList<Candle>> GetCandles(string ticker, DateTime? fromDate, DateTime? toDate, string importer = "Yahoo", string interval = "DAY");
    }
    public class CandleService : ICandleService
    {
        private readonly Func<string, IImporter> _serviceAccessor;

        public CandleService(Func<string, IImporter> serviceAccessor)
        {
            _serviceAccessor = serviceAccessor;
        }
        public async Task<IReadOnlyList<Candle>> GetCandles(string ticker, DateTime? fromDate, DateTime? toDate, string importer = "Yahoo", string interval = "DAY")
        {
            if(!fromDate.HasValue)
            {
                fromDate = DateTime.Now.AddDays(-365);
            }

            if(!toDate.HasValue)
            {
                toDate = DateTime.Now;
            }

            return await _serviceAccessor(importer).ImportAsync(ticker, fromDate.Value, toDate.Value, interval.ToTradyPeriod());
        }
    }
}
