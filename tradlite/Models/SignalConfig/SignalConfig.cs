using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Models
{
    public class SignalConfig
    {
        public int Id { get; set; }
        public string Endpoint { get; set; }
        public string ExtraParams { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }
}
