using QuantBox.Demo.Position;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Helper
{
    /// <summary>
    /// 按合约名维护一些持仓以及下单时的一些基本信息
    /// </summary>
    public class InstrumentStrategyHelper
    {
        public Framework framework;
        public SmartQuant.Strategy strategy;
        public Instrument instrument;

        public PriceHelper PriceHelper;
        public TimeHelper TimeHelper;

        // 从StrategyHelper中获得，只需要一个即可
        public OpenCloseHelper OpenCloseHelper;

        public SmartQuant.OrderType MarketOrderType = SmartQuant.OrderType.Market;
        public int Tick = 10;

        public InstrumentStrategyHelper(Framework framework, SmartQuant.Strategy strategy,Instrument instrument)
        {
            this.framework = framework;
            this.strategy = strategy;
            this.instrument = instrument;

            this.PriceHelper = new PriceHelper(framework, instrument.TickSize);
            this.TimeHelper = new TimeHelper(instrument.Symbol);
        }

        public Order SellOrder(Instrument instrument, double qty, string text)
        {
            if (MarketOrderType == SmartQuant.OrderType.Market)
            {
                return strategy.SellOrder(instrument, qty, text);
            }
            else
            {
                return strategy.SellLimitOrder(instrument, qty, PriceHelper.GetMatchPrice(instrument, SmartQuant.OrderSide.Sell, Tick), text);
            }
        }

        public Order BuyOrder(Instrument instrument, double qty, string text)
        {
            if (MarketOrderType == SmartQuant.OrderType.Market)
            {
                return strategy.BuyOrder(instrument, qty, text);
            }
            else
            {
                return strategy.BuyLimitOrder(instrument, qty, PriceHelper.GetMatchPrice(instrument, SmartQuant.OrderSide.Buy, Tick), text);
            }
        }

        /// <summary>
        /// 处理核心
        /// </summary>
        public virtual void Process(DualPositionRecord record, double price)
        {
            // 得到仓差
            double diff = record.TargetPosition - record.CurrentPosition;
            if (diff != 0
                && record.IsDone)
            {
                // 先标记成处理中，防止再次进入
                record.IsDone = false;

                if (diff > 0)
                {
                    // 增多仓
                    Order order1 = BuyOrder(record.Instrument, diff, record.Text);
                    Order order2 = OpenCloseHelper.RebuildOrder(record, order1);
                    OpenCloseHelper.Send(order2);
                }
                else
                {
                    // 增空仓
                    Order order1 = SellOrder(record.Instrument, -diff, record.Text);
                    Order order2 = OpenCloseHelper.RebuildOrder(record, order1);
                    OpenCloseHelper.Send(order2);
                }
            }
        }

        public void ChangeTradingDay(DualPositionRecord record)
        {
            if (TimeHelper.BeginOfDay > TimeHelper.EndOfDay)
            {
                // 夜盘
                if (TimeHelper.GetTime(framework.Clock.DateTime) > TimeHelper.EndOfDay
                && TimeHelper.GetTime(framework.Clock.DateTime) < TimeHelper.BeginOfDay)
                {
                    record.ChangeTradingDay();
                }
            }
            else
            {
                if (TimeHelper.GetTime(framework.Clock.DateTime) > TimeHelper.EndOfDay
                || TimeHelper.GetTime(framework.Clock.DateTime) < TimeHelper.BeginOfDay)
                {
                    record.ChangeTradingDay();
                }
            }
        }
    }
}
