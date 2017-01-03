using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartQuant;
using QuantBox.Demo.Statistics;

namespace QuantBox.Demo.Helper
{
    public class StatisticsHelper
    {
        private Framework framework;
        private SmartQuant.Strategy strategy;

        // Statistics
        public TrailingPrice TrailingPrice;
        public HighLowPrice HighLowPrice;

        public StatisticsHelper(Framework framework, SmartQuant.Strategy strategy)
        {
            this.framework = framework;
            this.strategy = strategy;
            this.TrailingPrice = new TrailingPrice(framework, strategy);
            this.HighLowPrice = new HighLowPrice(framework, strategy);
        }

        public void ChangeTradingDay()
        {
            int[] StatisticsTypes = new int[] {
                QuantBox.Demo.Statistics.PortfolioStatisticsType.DailyNumOfLossTrades,
                QuantBox.Demo.Statistics.PortfolioStatisticsType.DailyNumOfWinTrades,
                QuantBox.Demo.Statistics.PortfolioStatisticsType.DailyConsecutiveLossTrades,
            };

            foreach (int st in StatisticsTypes)
            {
                DailyStatisticsItem d = strategy.Portfolio.Statistics.Get(st) as DailyStatisticsItem;
                if (d != null)
                {
                    d.ChangeTradingDay();
                }
            }

            
        }

        
        public void Update(Instrument instrument, double price)
        {
            // Trailing Price
            if (strategy.HasPosition(instrument))
            {
                TrailingPrice.Update(price);
            }
            else
            {
                TrailingPrice.Reset();
            }
        }
    }
}