using System;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Models
{
    public class Ticker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Importer { get; set; }
    }
}
