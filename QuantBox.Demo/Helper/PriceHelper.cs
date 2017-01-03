using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Helper
{
    /// <summary>
    /// 价格调整工具
    /// 
    /// 以后得处理港股一类的TickSize变化的情况
    /// </summary>
    public class PriceHelper
    {
        public Framework framework;
        public double UpperLimitPrice { get; private set; }
        public double LowerLimitPrice { get; private set; }
        public double TickSize { get; private set; }

        public PriceHelper(Framework framework, double TickSize)
        {
            this.framework = framework;
            this.UpperLimitPrice = double.MaxValue;
            this.LowerLimitPrice = double.MinValue;
            this.TickSize = TickSize;
            // 可能出现TickSize=0的情况
            this.TickSize = Math.Max(0.0001, this.TickSize);
        }

        public PriceHelper(Framework framework, double UpperLimitPrice,
            double LowerLimitPrice,
            double TickSize)
        {
            this.framework = framework;
            this.UpperLimitPrice = UpperLimitPrice;
            this.LowerLimitPrice = LowerLimitPrice;
            this.TickSize = TickSize;
            this.TickSize = Math.Max(0.0001, this.TickSize);
        }

        public override string ToString()
        {
            return string.Format("TickSize:{0},LowerLimitPrice:{1},UpperLimitPrice:{2}",TickSize,LowerLimitPrice,UpperLimitPrice);
        }

        public int GetLevelByPrice(double price, SmartQuant.OrderSide Side)
        {
            price = Math.Min(price, UpperLimitPrice);
            price = Math.Max(price, LowerLimitPrice);

            int index = (int)((Side == SmartQuant.OrderSide.Buy) ? Math.Ceiling(price / TickSize) : Math.Floor(price / TickSize));
            return index;
        }

        public double GetPriceByLevel(int level)
        {
            return Math.Round(level * TickSize, 8);
        }

        public double FixPrice(double price, SmartQuant.OrderSide Side)
        {
            return GetPriceByLevel(GetLevelByPrice(price, Side));
        }

        public double GetMatchPrice(Instrument instrument, SmartQuant.OrderSide side)
        {
            if (side == SmartQuant.OrderSide.Sell)
            {
                Bid bid = framework.DataManager.GetBid(instrument);

                if (bid != null)
                    return bid.Price;
            }

            if (side == SmartQuant.OrderSide.Buy)
            {
                Ask ask = framework.DataManager.GetAsk(instrument);

                if (ask != null)
                    return ask.Price;
            }

            Trade trade = framework.DataManager.GetTrade(instrument);

            if (trade != null)
                return trade.Price;

            Bar bar = framework.DataManager.GetBar(instrument);

            if (bar != null)
                return bar.Close;

            return 0;
        }

        // 在对手价上加一定跳数
        public double GetMatchPrice(Instrument instrument, SmartQuant.OrderSide side, double jump)
        {
            double price = GetMatchPrice(instrument, side);
            if (side == SmartQuant.OrderSide.Buy)
            {
                price += jump * TickSize;
            }
            else
            {
                price -= jump * TickSize;
            }

            // 修正一下价格
            price = FixPrice(price, side);

            return price;
        }
    }
}
