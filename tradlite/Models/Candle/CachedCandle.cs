using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;

namespace Tradlite.Models.Candle
{
    public class CachedCandle : IOhlcvData
    {
        public CachedCandle()
        {

        }

        public CachedCandle(IOhlcvData candle, string interval, int cachedCandleKeyId)
        {
            Interval = interval;
            Open = candle.Open;
            High = candle.High;
            Low = candle.Low;
            Close = candle.Close;
            Volume = candle.Volume;
            DateTime = candle.DateTime;
            CachedCandleKeyId = cachedCandleKeyId;
        }

        public int Id { get; set; }
        public string Interval { get; set; }
        public decimal Open { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Close { get; set; }

        public decimal Volume { get; set; }

        public DateTime DateTime { get; set; }
        public int CachedCandleKeyId { get; set; }

    }
}
