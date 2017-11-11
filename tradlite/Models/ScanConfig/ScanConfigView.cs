using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Models.ScanConfig
{
    public class ScanConfigView : ScanConfig
    {
        public string SignalName { get; set; }
        public string TickerListName { get; set; }
    }
}
