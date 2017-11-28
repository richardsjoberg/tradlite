using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Models.Backtesting
{
    public class BacktestResult
    {
        public BacktestResult(List<Transaction> transactions)
        {
            Transactions = transactions;
            foreach(var transaction in transactions)
            {
                Gain += transaction.Direction == "Buy" ?
                    transaction.ExitLevel - transaction.EntryLevel :
                    transaction.EntryLevel - transaction.ExitLevel;
            }
        }

        public List<Transaction> Transactions { get; set; }
        public decimal Gain { get; set; }
    }
}
