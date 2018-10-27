using dto.endpoint.browse;
using System.Collections.Generic;

namespace Tradlite.Models.VolatilitySpreadRatio
{
    public class VolatilitySpreadRatioResponse
    {
        public decimal Spread { get; set; }
        public decimal Atr { get; set; }
        public decimal Ratio { get; set; }
        public string MarketStatus { get; set; }
    }
}
