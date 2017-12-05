using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Models.Requests
{
    public class BacktestTickerListRequest
    {
        public int TickerListId { get; set; }
        public int? LongBacktestConfigId { get; set; }
        public int? ShortBacktestConfigId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Interval { get; set; }
    }
}
