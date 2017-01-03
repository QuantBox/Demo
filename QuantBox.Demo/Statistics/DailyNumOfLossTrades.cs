using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Statistics
{
    /// <summary>
    /// 日内累计亏损次数
    /// </summary>
    public class DailyNumOfLossTrades : DailyStatisticsItem
    {
        public DailyNumOfLossTrades()
            : base(SmartQuant.PortfolioStatisticsType.NumOfLossTrades)
        {
        }

        public override int Type
        {
            get { return PortfolioStatisticsType.DailyNumOfLossTrades; }
        }

        public override string Name
        {
            get { return "Daily Num of Losing Trades"; }
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
