using System;
using System.Collections.Generic;
using System.Text;
using Trady.Core.Infrastructure;

namespace Tradlite.IgPocBot
{
    public class CandleUpdatedEventArgs : EventArgs
    { //test 
        public IOhlcv Candle { get; set; }
        public string Ticker { get; set; }
        public bool NewCandle { get; set; }
    }
}
