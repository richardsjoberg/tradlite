using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;

namespace Tradlite.Services.Signals
{
    public class SignalBase
    {
        public int[] ExecuteRule(IReadOnlyList<IOhlcv> candles, Predicate<IIndexedOhlcv> signal)
        {
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }

        public int[] ExecuteRuleBool<T>(IReadOnlyList<IOhlcv> candles, params object[] @params) where T : IAnalyzable
        {
            var signal = Rule.Create(c => ((AnalyzableTick<bool>)c.Get<T>(@params)[c.Index]).Tick);
            return ExecuteRule(candles, signal);
        }

        public int[] ExecuteRuleNullableBool<T>(IReadOnlyList<IOhlcv> candles, params object[] @params) where T : IAnalyzable
        {
            var signal = Rule.Create(c => ((AnalyzableTick<bool?>)c.Get<T>(@params)[c.Index]).Tick.IsTrue(t=>t));
            using (var ctx = new AnalyzeContext(candles))
            {
                var indexedCandles = new SimpleRuleExecutor(ctx, signal).Execute();
                return indexedCandles.Select(ic => ic.Index).ToArray();
            }
        }
    }
}
