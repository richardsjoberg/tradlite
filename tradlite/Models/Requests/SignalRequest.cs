using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Models.Requests
{
    public class SignalRequest : CandleRequest
    {
        public string Parameters { get; set; }
    }
}
