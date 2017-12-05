using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Models.Backtesting
{
    public class BacktestResult
    {
        public BacktestResult(List<Transaction> transactions, decimal initialCapital)
        {
            Transactions = transactions;
            decimal wins = 0;
            decimal losses = 0;
            foreach(var transaction in transactions)
            {
                var transactionGain = transaction.Direction == "Long" ?
                    (transaction.ExitLevel - transaction.EntryLevel) * transaction.Size * transaction.ExchangeRate :
                    (transaction.EntryLevel - transaction.ExitLevel) * transaction.Size * transaction.ExchangeRate;
                
                Gain += transactionGain;

                if (transactionGain > 0)
                    wins++;
                else
                    losses++;
                transaction.Gain = transactionGain;
                transaction.Reward = transaction.Reward * transaction.ExchangeRate;
                transaction.Risk = transaction.Risk * transaction.ExchangeRate;

            }

            if(transactions.Any())
            {
                WinPercentage = wins / (wins + losses);
            }

            NumberOfTransactions = transactions.Count;
            InitialCapital = initialCapital;
            ReturnRate = Gain / initialCapital;
            Capital = initialCapital + Gain;
            MaxRisk = transactions.Max(t => t.Risk);
            AverageRisk = transactions.Select(t => t.Risk).Average();
            MaxReward = transactions.Max(t => t.Reward);
            AverageReward = transactions.Select(t => t.Reward).Average();
            AverageRiskReward = AverageReward / AverageRisk;
        }

        public decimal Gain { get; set; }
        public decimal WinPercentage { get; set; }
        public int NumberOfTransactions { get; set; }
        public decimal ReturnRate { get; set; }
        public decimal InitialCapital { get; set; }
        public decimal Capital { get; set; }
        public decimal MaxRisk { get; set; }
        public decimal AverageRisk { get; set; }
        public decimal MaxReward { get; set; }
        public decimal AverageReward { get; set; }
        public decimal AverageRiskReward { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
