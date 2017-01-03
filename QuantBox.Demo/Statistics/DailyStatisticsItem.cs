using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Statistics
{
    /// <summary>
    /// 每天早上一开盘时要取到清0的数据
    /// 有两种方案：
    /// 1.在每次改变交易日时进行重置
    /// 2.在get方法中返回0
    /// 
    /// 测试了多次后，发现2用起来简单，实际写起来太复杂
    /// </summary>
    public abstract class DailyStatisticsItem : PortfolioStatisticsItem
    {
        protected double longTrades_Begin;
        protected double shortTrades_Begin;
        protected double totalTrades_Begin;

        protected double longTrades_Last;
        protected double shortTrades_Last;
        protected double totalTrades_Last;

        protected bool IsNewDay;

        protected int type;

        public DailyStatisticsItem()
        {
            ChangeTradingDay();
        }

        public DailyStatisticsItem(int type)
        {
            this.type = type;

            ChangeTradingDay();
        }

        protected override void OnInit()
        {
            if(this.type>0)
                base.Subscribe(this.type);
        }

        public virtual void ChangeTradingDay()
        {
            base.longValue = 0;
            base.shortValue = 0;
            base.totalValue = 0;

            IsNewDay = true;
        }

        protected override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            if (statistics.Type == this.type)
            {
                if (IsNewDay)
                {
                    // 减去昨天最后的数据
                    base.longValue = statistics.LongValue - longTrades_Last;
                    base.shortValue = statistics.ShortValue - shortTrades_Last;
                    base.totalValue = statistics.TotalValue - totalTrades_Last;

                    // 本天初始化的数量
                    this.longTrades_Begin = statistics.LongValue;
                    this.shortTrades_Begin = statistics.ShortValue;
                    this.totalTrades_Begin = statistics.TotalValue;
                }
                else
                {
                    // 当天第一轮的后几个
                    base.longValue = statistics.LongValue - longTrades_Begin;
                    base.shortValue = statistics.ShortValue - shortTrades_Begin;
                    base.totalValue = statistics.TotalValue - totalTrades_Begin;
                }

                IsNewDay = false;

                // 一直更新，第二天早上要用
                longTrades_Last = statistics.LongValue;
                shortTrades_Last = statistics.ShortValue;
                totalTrades_Last = statistics.TotalValue;

                base.longValues.Add(base.Clock.DateTime, base.longValue);
                base.shortValues.Add(base.Clock.DateTime, base.shortValue);
                base.totalValues.Add(base.Clock.DateTime, base.totalValue);

                base.Emit();
            }
        }
    }
}
