using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Models.Backtesting
{
    public class Order
    {
        public decimal EntryLevel { get; set; }
        public decimal Size { get; set; }
        public OrderDirection Direction { get; set; }
        public OrderType Type { get; set; }
        public decimal? Stop { get; set; }
        public decimal? Limit { get; set; }
        public DateTime Created { get; set; }
    }

    public enum OrderDirection
    {
        Long,
        Short
    }

    public enum OrderType
    {
        Stop,
        Limit,
        Market
    }
}
