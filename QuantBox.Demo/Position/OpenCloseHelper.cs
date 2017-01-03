using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XAPI;
using QuantBox.Extensions;

namespace QuantBox.Demo.Position
{
    public enum SeparateOrder
    {
        /// <summary>
        /// 将单子拆成平今平昨开仓
        /// </summary>
        SeparateCloseOpen,
        /// <summary>
        /// 如果可平数不够，全部开仓
        /// </summary>
        AllOpen,
    }

    public enum DefaultClose
    {
        /// <summary>
        /// 对于非上海的合约，是用平今指令来下
        /// </summary>
        CloseToday,
        /// <summary>
        /// 对于非上海的合约，用平仓指令
        /// </summary>
        Close,
    }

    public enum SendStyle
    {
        /// <summary>
        /// 一起
        /// </summary>
        Together,
        /// <summary>
        /// 一个接一个
        /// </summary>
        OneByOne,
    }

    /// <summary>
    /// 在一个BuySide策略中，直接发出的单子就带开平标记，不用接SellSide
    /// 
    /// 它只做持仓的创建
    /// </summary>
    public class OpenCloseHelper
    {
        public Framework framework;
        public SmartQuant.Strategy strategy;

        public Portfolio Long;
        public Portfolio Short;
        
        public SeparateOrder SeparateOrder = SeparateOrder.AllOpen;
        public DefaultClose DefaultClose = DefaultClose.CloseToday;
        public SendStyle SendStyle = SendStyle.OneByOne;

        public OpenCloseHelper(Framework framework,SmartQuant.Strategy strategy)
        {
            this.framework = framework;
            this.strategy = strategy;
        }

        public static OpenCloseType GetOpenClose(Order order)
        {
            object obj = order.GetOpenClose();
            if (obj == null)
                return OpenCloseType.Close;
            OpenCloseType OpenClose = (OpenCloseType)obj;
            return OpenClose;
        }

        public Order RebuildOrder(DualPositionRecord dualRecord,Order order)
        {
            if (order == null)
                return null;

            List<Order> orders = new List<Order>();

            MonoPositionRecord record = dualRecord.GetPositionRecord(order.Side, OpenCloseType.Close);

            // 先分析是否有仓要平
            double QtyToday = 0;
            double QtyYesterday = 0;
            record.GetCanCloseQty(out QtyToday, out QtyYesterday);

            // 非上海要处理
            if (order.Instrument.Exchange != "SHFE")
            {
                // 如果是上海就不动，非上海有两方案，全转今仓或全转昨仓
                if (DefaultClose == DefaultClose.CloseToday)
                {
                    // 全转今仓
                    QtyToday += QtyYesterday;
                    QtyYesterday = 0;
                }
                else
                {
                    // 全转昨仓
                    QtyYesterday += QtyToday;
                    QtyToday = 0;
                }
            }

            // 1.拆单
            // 3.全开
            double leave = order.Qty;

            if (SeparateOrder == SeparateOrder.SeparateCloseOpen)
            {
                // 拆单，先平今，再平昨，最后开仓
                if (QtyToday > 0 && leave > 0)
                {
                    double Qty = Math.Min(QtyToday, leave);
                    leave -= Qty;

                    Order _order = strategy.Order(order.Instrument, order.Type, order.Side, order.Qty, order.StopPx, order.Price, order.Text);
                    order.Fields.CopyTo(_order.Fields);
                    AddOpenClose(_order, Qty, OpenCloseType.CloseToday);
                    orders.Add(_order);
                }

                if (QtyYesterday > 0 && leave > 0)
                {
                    double Qty = Math.Min(QtyYesterday, leave);
                    leave -= Qty;

                    Order _order = strategy.Order(order.Instrument, order.Type, order.Side, order.Qty, order.StopPx, order.Price, order.Text);
                    order.Fields.CopyTo(_order.Fields);
                    AddOpenClose(_order, Qty, OpenCloseType.Close);
                    orders.Add(_order);
                }

                if (leave > 0)
                {
                    double Qty = leave;
                    leave -= Qty;

                    AddOpenClose(order, Qty, OpenCloseType.Open);
                    orders.Add(order);
                }
            }
            else
            {
                // 发现可平数不够，直接开仓
                if (leave <= QtyToday && leave > 0)
                {
                    double Qty = leave;
                    leave -= Qty;
                    AddOpenClose(order, Qty, OpenCloseType.CloseToday);
                    orders.Add(order);
                }
                else if (leave <= QtyYesterday && leave > 0)
                {
                    double Qty = leave;
                    leave -= Qty;
                    AddOpenClose(order, Qty, OpenCloseType.Close);
                    orders.Add(order);
                }
                else if (leave > 0)
                {
                    double Qty = leave;
                    leave -= Qty;

                    AddOpenClose(order, Qty, OpenCloseType.Open);
                    orders.Add(order);
                }
            }


            // 让每个Order串起来，这样可以自己查找到下一个Order
            for (int i = orders.Count - 1; i > 0; --i)
            {
                orders[i - 1].SetNextTimeOrder(orders[i]);
            }

            if (orders.Count > 0)
                return orders[0];

            return null;
        }

        public void Send(Order order)
        {
            if (order == null)
                return;

            if (SendStyle == SendStyle.OneByOne)
            {
                strategy.Send(order);
            }
            else
            {
                var orders = order.GetNextTimeOrderList();
                foreach(var o in orders)
                {
                    strategy.Send(o);
                }
            }
        }

        /// <summary>
        /// 找到下一个Order并发送
        /// </summary>
        /// <param name="lastOrder"></param>
        public bool SendNext(Order lastOrder)
        {
            if (SendStyle != SendStyle.OneByOne)
                return false;

            if(lastOrder.IsDone)
            {
                Order o = lastOrder.GetNextTimeOrder();
                if(o != null && !o.IsDone)
                {
                    o.Text = string.Format("N:{0}", o.Text);
                    strategy.Send(o);
                    return true;
                }
            }
            return false;
        }

        private void AddOpenClose(Order order, double Qty, OpenCloseType OpenClose)
        {
            order.Qty = Qty;

            switch (OpenClose)
            {
                case OpenCloseType.Open:
                    order.Open();
                    order.Portfolio = order.Side == SmartQuant.OrderSide.Buy ? Long : Short;
                    order.Text = "O:" + order.Text;//加了标记，这样在OrderManger中比较直观
                    break;
                case OpenCloseType.Close:
                    order.Close();
                    order.Portfolio = order.Side == SmartQuant.OrderSide.Buy ? Short : Long;
                    order.Text = "C:" + order.Text;
                    break;
                case OpenCloseType.CloseToday:
                    order.CloseToday();
                    order.Portfolio = order.Side == SmartQuant.OrderSide.Buy ? Short : Long;
                    order.Text = "T:" + order.Text;
                    break;
                default:
                    break;
            }
        }
    }
}
