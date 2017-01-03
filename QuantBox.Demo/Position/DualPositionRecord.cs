using Newtonsoft.Json;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XAPI;

namespace QuantBox.Demo.Position
{
    public class DualPositionRecord
    {
        [JsonIgnore]
        public Instrument Instrument;
        // 由于保存时用的key是id数字，导致无法区分是哪个合约，所以用Symbol来区分
        public string Symbol { get { return Instrument.Symbol; } }
        [JsonIgnore]
        public string Text;
        public MonoPositionRecord Long { get; set; }
        public MonoPositionRecord Short { get; set; }
        [JsonIgnore]
        public double TargetPosition { get; set; }
        [JsonIgnore]
        public bool IsDone = true;

        public DualPositionRecord()
        {
            Long = new MonoPositionRecord();
            Short = new MonoPositionRecord();
        }
        [JsonIgnore]
        public double LongQty
        {
            get { return Long.Qty; }
        }
        [JsonIgnore]
        public double ShortQty
        {
            get { return Short.Qty; }
        }
        [JsonIgnore]
        public double CurrentPosition
        {
            get { return Long.Qty - Short.Qty; }
        }

        public void ChangeTradingDay()
        {
            Long.ChangeTradingDay();
            Short.ChangeTradingDay();
        }

        public void FilterCloseOnly()
        {
            if (TargetPosition * CurrentPosition <= 0)
            {
                // <0表示反手
                // =0表示从0开新仓或平仓后为0
                TargetPosition = 0;
            }
            else if (TargetPosition > 0)
            {
                //多头
                TargetPosition = Math.Min(TargetPosition, CurrentPosition);
            }
            else
            {
                //空头
                TargetPosition = Math.Max(TargetPosition, CurrentPosition);
            }
        }

        public MonoPositionRecord GetPositionRecord(SmartQuant.OrderSide Side, OpenCloseType OpenClose)
        {
            switch (OpenClose)
            {
                case OpenCloseType.Open:
                    return Side == SmartQuant.OrderSide.Buy ? Long : Short;
                case OpenCloseType.Close:
                case OpenCloseType.CloseToday:
                    return Side == SmartQuant.OrderSide.Buy ? Short : Long;
                default:
                    MessageBox.Show("GetPositionRecord");
                    break;
            }
            return null;
        }

        public void OnExecutionReport(ExecutionReport report)
        {
            // 需要把手上的单子全跑完才能标记为IsDone
            switch (report.ExecType)
            {
                case SmartQuant.ExecType.ExecRejected:
                    OnOrderRejected(report.Order);
                    break;
                case SmartQuant.ExecType.ExecCancelReject:
                    OnOrderCancelReject(report.Order);
                    break;
                case SmartQuant.ExecType.ExecCancelled:
                    OnOrderCancelled(report.Order);
                    break;
                case SmartQuant.ExecType.ExecNew:
                    OnPendingNewOrder(report.Order);
                    break;
                case SmartQuant.ExecType.ExecTrade:
                    OnOrderFilled(report.Order);
                    break;
            }
            IsDone = report.Order.IsDone;
        }

        #region 触发OnOrderRejected\OnOrderCancelReject事件
        private OpenCloseType OnOrderRejected(Order order)
        {
            OpenCloseType OpenClose = OpenCloseHelper.GetOpenClose(order);
            MonoPositionRecord record = GetPositionRecord(order.Side, OpenClose);

            double LeavesQty = order.LeavesQty;

            switch (OpenClose)
            {
                case OpenCloseType.Open:
                    record.OrderRejectedOpen(LeavesQty);
                    break;
                case OpenCloseType.Close:
                    record.OrderRejectedClose(LeavesQty);
                    break;
                case OpenCloseType.CloseToday:
                    record.OrderRejectedCloseToday(LeavesQty);
                    break;
                default:
                    MessageBox.Show("OrderRejected");
                    break;
            }

            return OpenClose;
        }

        private void OnOrderCancelReject(Order order)
        {
            // 由于不改变Order的实际状态，所以可以不处理
        }
        #endregion

        private OpenCloseType OnOrderCancelled(Order order)
        {
            OpenCloseType OpenClose = OpenCloseHelper.GetOpenClose(order);
            MonoPositionRecord record = GetPositionRecord(order.Side, OpenClose);
            ++record.CumCancelCnt;

            OnOrderRejected(order);

            return OpenClose;
        }

        private void OnPendingNewOrder(Order order)
        {
            double Qty = order.Qty;

            OpenCloseType OpenClose = OpenCloseHelper.GetOpenClose(order);
            MonoPositionRecord record = GetPositionRecord(order.Side, OpenClose);

            switch (OpenClose)
            {
                case OpenCloseType.Open:
                    record.NewOrderOpen(Qty);
                    break;
                case OpenCloseType.Close:
                    record.NewOrderClose(Qty);
                    break;
                case OpenCloseType.CloseToday:
                    record.NewOrderCloseToday(Qty);
                    break;
                default:
                    MessageBox.Show("PendingNewOrder");
                    break;
            }
        }

        private void OnOrderFilled(SmartQuant.Order order)
        {
            int index = order.Reports.Count - 1;
            double LastQty = order.Reports[index].LastQty;
            double LastPrice = order.Reports[index].LastPx;
            bool IsDone = order.IsDone;

            OpenCloseType OpenClose = OpenCloseHelper.GetOpenClose(order);
            MonoPositionRecord record = GetPositionRecord(order.Side, OpenClose);

            switch (OpenClose)
            {
                case OpenCloseType.Open:
                    record.FilledOpen(LastQty, LastPrice);
                    break;
                case OpenCloseType.Close:
                    record.FilledClose(LastQty, LastPrice);
                    break;
                case OpenCloseType.CloseToday:
                    record.FilledCloseToday(LastQty, LastPrice);
                    break;
                default:
                    MessageBox.Show("Filled");
                    break;
            }
        }
    }
}
