using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Statistics
{
    public class DailyConsecutiveLossTrades : DailyStatisticsItem
    {
        protected double longLossTrades;
        protected double longWinTrades;
        protected double shortLossTrades;
        protected double shortWinTrades;
        protected double totalLossTrades;
        protected double totalWinTrades;

        protected override void OnInit()
        {
            base.Subscribe(PortfolioStatisticsType.DailyNumOfLossTrades);
            base.Subscribe(PortfolioStatisticsType.DailyNumOfWinTrades);
        }

        protected override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            if (statistics.Type == PortfolioStatisticsType.DailyNumOfLossTrades)
            {
                bool isEmit = false;
                if (statistics.LongValue != this.longLossTrades)
                {
                    this.longLossTrades = statistics.LongValue;
                    base.longValues.Add(base.Clock.DateTime, ++base.longValue);
                    isEmit = true;
                }
                if (statistics.ShortValue != this.shortLossTrades)
                {
                    this.shortLossTrades = statistics.ShortValue;
                    base.shortValues.Add(base.Clock.DateTime, ++base.shortValue);
                    isEmit = true;
                }
                if (statistics.TotalValue != this.totalLossTrades)
                {
                    this.totalLossTrades = statistics.TotalValue;
                    base.totalValues.Add(base.Clock.DateTime, ++base.totalValue);
                    isEmit = true;
                }
                if (isEmit)
                {
                    base.Emit();
                }
            }
            if (statistics.Type == PortfolioStatisticsType.DailyNumOfWinTrades)
            {
                bool isEmit = false;
                if (statistics.LongValue != this.longWinTrades)
                {
                    this.longWinTrades = statistics.LongValue;
                    base.longValue = 0.0;
                    base.longValues.Add(base.Clock.DateTime, base.longValue);
                    isEmit = true;
                }
                if (statistics.ShortValue != this.shortWinTrades)
                {
                    this.shortWinTrades = statistics.ShortValue;
                    base.shortValue = 0.0;
                    base.shortValues.Add(base.Clock.DateTime, base.shortValue);
                    isEmit = true;
                }
                if (statistics.TotalValue != this.totalWinTrades)
                {
                    this.totalWinTrades = statistics.TotalValue;
                    base.totalValue = 0.0;
                    base.totalValues.Add(base.Clock.DateTime, base.totalValue);
                    isEmit = true;
                }
                if (isEmit)
                {
                    base.Emit();
                }
            }
        }

        public override string Category
        {
            get
            {
                return PortfolioStatisticsCategory.Trades;
            }
        }

        public override string Format
        {
            get
            {
                return "F0";
            }
        }

        public override string Name
        {
            get
            {
                return "Daily Consecutive Losing Trades";
            }
        }

        public override bool Show
        {
            get
            {
                return true;
            }
        }

        public override int Type
        {
            get
            {
                return PortfolioStatisticsType.DailyConsecutiveLossTrades;
            }
        }
    }
}
