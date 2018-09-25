using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Trady.Core;
using Trady.Core.Infrastructure;
using System.Linq;

namespace Tradlite.IgPocBot
{
    public class TradeEngine
    {
        private readonly Timer _timer;
        private bool _executing;
        public EventHandler<CandleUpdatedEventArgs> _candlesUpdated;
        private readonly string _ticker;
        private IList<IOhlcv> _candles;
        private CandleUpdatedEventArgs _candleEventArgs;
        private bool _updateCandles; 

        public TradeEngine(CandleUpdater candleUpdater, IList<IOhlcv> candles, string ticker)
        {
            //candleUpdater._candlesUpdated += (sender, eventArgs) => UpdateCandles(eventArgs);
            candleUpdater._candlesUpdated += (sender, eventArgs) => UpdateCandles2(eventArgs);
            _timer = new Timer(1000) { AutoReset = true };
            _timer.Elapsed += (sender, eventArgs) => 
            {
                UpdateCandles();
                Execute();
            };
            _ticker = ticker;
        }
        
        public void Start()
        {
            _timer.Start();
        }
        public void Stop()
        {
            _timer.Stop();
            //ignore candle updates
        }
        public void Execute()
        {
            if (_executing)
                return;
            _executing = true;

            Console.WriteLine("executing");
            
            if (_updateCandles)
            {
                UpdateCandles();
                Execute();
            }
            _executing = false;
        }

        //public void UpdateCandles(CandleUpdatedEventArgs candleEventArgs)
        //{
        //    Console.WriteLine("Ho ho ho");
        //    _candleEventArgs = candleEventArgs;
        //    if (_executing)
        //    {
        //        _updateCandles = true;
        //    }
        //    else
        //    {
        //        //UpdateCandles();
        //        Execute();
        //    }
        //}
        public void UpdateCandles2(CandleUpdatedEventArgs candleEventArgs)
        {
            Console.WriteLine("2222");
            _candleEventArgs = candleEventArgs;
            if (_executing)
            {
                _updateCandles = true;
            }
            else
            {
                //UpdateCandles();
                Execute();
            }
        }

        private void UpdateCandles()
        {
            if (_candleEventArgs.NewCandle)
            {
                _candles.Add(_candleEventArgs.Candle);
            }
            else
            {
                var _lastCandle = _candles.Last();
                _lastCandle = _candleEventArgs.Candle;
            }
        }
    }
}
