using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Core.Infrastructure;
using Trady.Analysis.Extension;
using System.ComponentModel;
using Newtonsoft.Json.Linq;

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
            return ExecuteRule(candles, signal);
        }

        public int[] CloseAboveIndicator<T>(IReadOnlyList<IOhlcv> candles, params object[] @params) where T : IAnalyzable
        {
            Predicate<IIndexedOhlcv> signal = ic =>
            {
                var tick = ((AnalyzableTick<decimal?>)ic.Get<T>(@params)[ic.Index]).Tick;
                return tick.HasValue
                    && tick.Value < ic.Close;
            };

            return ExecuteRule(candles, signal);
        }

        public int[] CloseBelowIndicator<T>(IReadOnlyList<IOhlcv> candles, params object[] @params) where T : IAnalyzable
        {
            Predicate<IIndexedOhlcv> signal = ic =>
            {
                var tick = ((AnalyzableTick<decimal?>)ic.Get<T>(@params)[ic.Index]).Tick;
                return tick.HasValue
                    && tick.Value > ic.Close;
            };
            return ExecuteRule(candles, signal);
        }

        public int[] IndicatorCross<T1,T2>(IReadOnlyList<IOhlcv> candles, object[] indicator1Params, object[] indicator2Params) where T1 : IAnalyzable where T2 : IAnalyzable
        {
            Predicate<IIndexedOhlcv> signal = ic =>
            {
                if (ic.Index == 0)
                    return false;

                var tick1 = ((AnalyzableTick<decimal?>)ic.Get<T1>(indicator1Params)[ic.Index]).Tick;
                var tick2 = ((AnalyzableTick<decimal?>)ic.Get<T2>(indicator2Params)[ic.Index]).Tick;
                var previousTick1 = ((AnalyzableTick<decimal?>)ic.Get<T1>(indicator1Params)[ic.Index-1]).Tick;
                var previousTick2 = ((AnalyzableTick<decimal?>)ic.Get<T2>(indicator2Params)[ic.Index-1]).Tick;
                return tick1.HasValue
                    && tick2.HasValue
                    && previousTick1.HasValue
                    && previousTick2.HasValue
                    && previousTick1.Value <= previousTick2.Value
                    && tick1.Value > tick2.Value;
            };

            return ExecuteRule(candles, signal);
        }
    }
}
