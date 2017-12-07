using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Models.Requests
{
    public class BacktestTickerRequest : CandleRequest
    {
        public int BacktestConfigId { get; set; }
        public decimal Risk { get; set; }
        public decimal? CurrentCapital { get; set; }
    }
}
