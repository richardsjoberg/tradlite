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
                case "MINUTE_15":
                    return Trady.Core.Period.PeriodOption.Per15Minute;
                case "MINUTE_30":
                    return Trady.Core.Period.PeriodOption.Per30Minute;
                case "HOUR":
                    return Trady.Core.Period.PeriodOption.Hourly;
                case "HOUR_4":
                    return Trady.Core.Period.PeriodOption.BiHourly;
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
