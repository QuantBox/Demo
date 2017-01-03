using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Statistics
{
    /// <summary>
    /// 日内累计利益次数
    /// </summary>
    public class DailyNumOfWinTrades : DailyStatisticsItem
    {
        public DailyNumOfWinTrades()
            : base(SmartQuant.PortfolioStatisticsType.NumOfWinTrades)
        {
        }

        public override int Type
        {
            get { return PortfolioStatisticsType.DailyNumOfWinTrades; }
        }

        public override string Name
        {
            get { return "Daily Num of Winning Trades"; }
        }

        public override bool Show
        {
            get
            {
                return true;
            }
        }

        public override string Category
        {
            get { return PortfolioStatisticsCategory.Trades; }
        }
    }
}
