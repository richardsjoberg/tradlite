using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Analysis.Infrastructure;
using Trady.Core;
using Trady.Core.Infrastructure;

namespace Tradlite.Controllers
{
    public class CandlePatternController : Controller
    {
        [Route("api/candlepatterns")]
        public dynamic[] AvailableCandlePatterns()
        {
            var type1 = typeof(AnalyzableBase<Candle, (decimal Open, decimal Close), bool, AnalyzableTick<bool>>);
            var type2 = typeof(AnalyzableBase<Candle, (decimal Open, decimal Close), bool?, AnalyzableTick<bool?>>);
            var type3 = typeof(AnalyzableBase<Candle, (decimal Open, decimal High, decimal Low, decimal Close), bool, AnalyzableTick<bool>>);
            var type4 = typeof(AnalyzableBase<Candle, (decimal Open, decimal High, decimal Low, decimal Close), bool?, AnalyzableTick<bool?>>);
            var types = new List<CandlePattern>();
            types.AddRange(GetSubTypes(type1));
            types.AddRange(GetSubTypes(type2));
            types.AddRange(GetSubTypes(type3));
            types.AddRange(GetSubTypes(type4));
            return types.ToArray();
        }

        private List<CandlePattern> GetSubTypes(Type type)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => p.IsSubclassOf(type))
                .Select(t => new CandlePattern
                {
                    Name = t.ToString().Split('.').Last(),
                    Type = t.ToString()
                }).ToList();
        }

        public class CandlePattern
        {
            public string Type { get; set; }
            public string Name { get; set; }
        }
    }
}
