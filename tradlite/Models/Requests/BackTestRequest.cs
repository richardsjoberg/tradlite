using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Models.Requests
{
    public class BacktestRequest
    {
        public CandleRequest CandleRequest { get; set; }
        public string BuySignalEndpoint { get; set; }
        public SignalRequest BuySignalRequest { get; set; }
        public string SellSignalEndpoint { get; set; }
        public SignalRequest SellSignalRequest { get; set; }
        public string ManagementService { get; set; }
    }
}
