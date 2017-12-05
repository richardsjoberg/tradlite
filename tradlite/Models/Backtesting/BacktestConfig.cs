using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Models.Backtesting
{
    public class BacktestConfig
    {
        public int Id { get; set; }
        public int EntrySignalConfigId { get; set; }
        public string EntrySignalService { get; set; }
        public string StopLossManagement { get; set; }
        public string LimitManagement { get; set; }
        public string EntryManagement { get; set; }
        public string Parameters { get; set; }
        public int AllowedRisk { get; set; }
        public int? ExitSignalConfigId { get; set; }
        public string ExitSignalService { get; set; }
        public string Direction { get; set; }
        public string OrderType { get; set; }
    }
}
