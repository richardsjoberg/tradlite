using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Models.Backtesting
{
    public class Position
    {
        public decimal EntryLevel { get; set; }
        public OrderDirection Direction { get; set; }
        public decimal Size { get; set; }
        public decimal Stop { get; set; }
        public decimal Limit { get; set; }
        public DateTime Created { get; set; }
    }
}
