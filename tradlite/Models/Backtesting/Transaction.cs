using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Models.Backtesting
{
    public class Transaction
    {
        public decimal EntryLevel { get; set; }
        public decimal ExitLevel { get; set; }
        public string Direction { get; set; }
        public decimal Size { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime ExitDate { get; set; }
        public decimal Risk { get; set; }
        public decimal Reward { get; set; }
        public decimal Gain { get; set; }
        public string Ticker { get; set; }
    }
}
