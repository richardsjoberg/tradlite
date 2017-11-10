using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite
{
    public static class Extensions
    {
        public static Trady.Core.Period.PeriodOption ToTradyPeriod(this string period)
        {
            switch(period)
            {
                case "SECOND":
                    return Trady.Core.Period.PeriodOption.PerSecond;
                case "MINUTE":
                    return Trady.Core.Period.PeriodOption.PerMinute;
                case "HOUR":
                    return Trady.Core.Period.PeriodOption.Hourly;
                case "DAY":
                    return Trady.Core.Period.PeriodOption.Daily;
                case "WEEK":
                    return Trady.Core.Period.PeriodOption.Weekly;
                case "MONTH":
                    return Trady.Core.Period.PeriodOption.Monthly;
                default:
                    return Trady.Core.Period.PeriodOption.Daily;
            }
        }
    }
}
