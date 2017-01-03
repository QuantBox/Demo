using QuantBox.Demo.Position;
using QuantBox.Extensions;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XAPI;

namespace QuantBox.Demo.Helper
{
    /// <summary>
    /// 维护一些Helper信息
    /// 维护双向持仓信息
    /// 因为涉及到多合约，但每个合约的时间不同，TickSize不同，必须要每个合约单独设置这些信息
    /// 
    /// 在InstrumentStrategy中，实际上StrategyHelper会被建立多个，每个中只有InstrumentStrategyHelper
    /// 在Strategy中，StrategyHelper只有一个，每个中有多个InstrumentStrategyHelper
    /// </summary>
    public class StrategyHelper
    {
        public Framework framework;
        public SmartQuant.Strategy strategy;


        // 仓位同步工作，一个即可
        public PositionHelper PositionHelper;

        public StatisticsHelper StatisticsHelper;
        

        public int OrderCheckInterval = 5;
        public int MaxResendOrderCount = 3;

        public Dictionary<int, InstrumentStrategyHelper> Strategies { get; private set; }

        // 开平仓工具,只创建一个即可
        public OpenCloseHelper OpenCloseHelper;
        // 用于界面显示
        public Portfolio Long;
        public Portfolio Short;
        // 双向持仓容器
        public DualPositionContainer DualPositionContainer { get; private set; }

        public StrategyHelper(Framework framework, SmartQuant.Strategy strategy)
        {
            this.framework = framework;
            this.strategy = strategy;

            this.DualPositionContainer = new DualPositionContainer(framework);
            this.OpenCloseHelper = new OpenCloseHelper(framework,strategy);
            this.PositionHelper = new PositionHelper(framework);

            this.Strategies = new Dictionary<int, InstrumentStrategyHelper>();

            {
                // 创建多空两套持仓,只是显示用，实际上不以此为依据
                Long = new Portfolio(framework, strategy.Name + "_Long");
                framework.PortfolioManager.Add(Long);
                Long.Parent = strategy.Portfolio;

                Short = new Portfolio(framework, strategy.Name + "_Short");
                framework.PortfolioManager.Add(Short);
                Short.Parent = strategy.Portfolio;
            }

            {
                // 在实际下单时需要使用指定所操作的Portfolio
                OpenCloseHelper.Long = Long;
                OpenCloseHelper.Short = Short;
            }
        }

        public DualPositionRecord GetPositionRecord(Instrument instrument)
        {
            return DualPositionContainer.GetPositionRecord(instrument);
        }

        public InstrumentStrategyHelper GetInstrumentStrategyHelper(Instrument instrument,SmartQuant.Strategy strategy)
        {
            InstrumentStrategyHelper value;
            if (!Strategies.TryGetValue(instrument.Id, out value))
            {
                value = new InstrumentStrategyHelper(framework, strategy, instrument);
                value.OpenCloseHelper = OpenCloseHelper;
                Strategies.Add(instrument.Id, value);
            }
            return value;
        }

        public void SyncPosition(byte providerId,byte route)
        {
            PositionHelper.SyncPosition(providerId, route, Long, Short, DualPositionContainer);
        }

        public void SavePosition(string path)
        {
            DualPositionContainer.Save(path);
        }

        public void LoadPosition(string path)
        {
            DualPositionContainer.Load(path);
            PositionHelper.DualPositionToPortfolio(DualPositionContainer, Long, Short);
        }

        public void ReadCsv(string path)
        {
            PositionHelper.ReadCsv(path, DualPositionContainer);
            PositionHelper.DualPositionToPortfolio(DualPositionContainer, Long, Short);
        }

        // 发送做市商双向报单
        public Order SendQuote(Instrument instrument,
            double askQty, double askPrice, double bidQty, double bidPrice,string quoteReqID)
        {
            //Order askOrder = RebuildOrder(Strategy.SellLimitOrder(instrument, askQty, askPrice));
            //Order bidOrder = RebuildOrder(Strategy.BuyLimitOrder(instrument, bidQty, bidPrice));

            //askOrder.SetMsgType(OrderMsgType.Ignore);
            //bidOrder.SetMsgType(OrderMsgType.Quote);

            //askOrder.SetOrder(bidOrder);
            //bidOrder.SetOrder(askOrder);

            //bidOrder.SetQuoteReqID(quoteReqID);

            //Send(askOrder);
            //Send(bidOrder);

            //// 返回其中任意一个Order，可用来定时撤单
            //return askOrder;
            return null;
        }

        // 做市商双向撤单，只要对其中的一个Order撤单即可
        public void CancelQuote(Order order)
        {
            order.SetMsgType(OrderMsgType.QuoteCancel);

            strategy.Cancel(order);
        }

        public void ChangeTradingDay()
        {
            if(StatisticsHelper != null)
            {
                StatisticsHelper.ChangeTradingDay();
            }
        }

        public void OnExecutionReport(ExecutionReport report)
        {
            DualPositionRecord record = GetPositionRecord(report.Instrument);
            // 更新对应的持仓信息
            record.OnExecutionReport(report);

            // 加入定时器，用于定时跟单功能
            switch (report.ExecType)
            {
                case SmartQuant.ExecType.ExecTrade:
                    OnOrderFilled(report.Order, record);
                    break;
                case SmartQuant.ExecType.ExecRejected:
                    OnOrderRejected(report.Order, record);
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
                
            }
        }

        private void OnOrderFilled(Order order, DualPositionRecord record)
        {
            // 当前单子成交后，立即发接下来的新单
            if (OpenCloseHelper.SendNext(order))
            {
                // 表示还在发单，所以状态必须改成一样的
                record.IsDone = false;
            }
        }

        private void OnOrderRejected(Order order, DualPositionRecord record)
        {
            // 1.非交易时间
            // 2.超出涨跌停
            // 3.报单错误
            OpenCloseType OpenClose = OpenCloseHelper.GetOpenClose(order);
            if(OpenClose == OpenCloseType.Open)
            {
                // 1.资金不足


                // 开仓被拒绝，需要返回原持仓
            }
            else
            {
                // 1.可平手数不足
            }

            // 是重试N次还是立即还原呢？就算是立即还原，会由于行情满足会再次进入

            // 这种方法还原是否可行？
            record.TargetPosition = record.CurrentPosition;
        }

        private void OnPendingNewOrder(Order order)
        {
            // 快到盘中休息的时候，下的单子挂在那，但撤单会被拒绝，定时器要重新设置
            //DateTime dt = TimeHelper.GetNextTradingTime(framework.Clock.DateTime.AddSeconds(OrderCheckInterval));
            //strategy.AddReminder(dt, order);
            
        }

        private void OnOrderCancelReject(Order order)
        {
            // 撤单被拒绝了，又是定时器的主动撤单
            //int cnt = order.GetCancelCount();
            //if (cnt > 0)
            //{
            //    // 如何找到下一个可用时间？
            //    DateTime dt = TimeHelper.GetNextTradingTime(framework.Clock.DateTime.AddSeconds(OrderCheckInterval));
            //    strategy.AddReminder(dt, order);
            //}
        }

        private void OnOrderCancelled(Order order)
        {
            // 这地方应当区分一下是定时撤单还是由策略撤单所导致
            // 如果是策略撤单，应当不重发，如果是定时撤单，得重发
            // FAK/FOK因为没有定时撤单的标记，所以也不会重发

            // 如何处理跟单一次后，策略又想主动撤单
            //int cnt = order.GetCancelCount();
            //order.SetCancelCount(0);//重置此标记
            //if (cnt > 0)
            //{
            //    ResendOrder(order);
            //}
        }

        public void OnReminder(DateTime dateTime, object data)
        {
            //Order order = data as Order;
            //if (order == null || order.IsDone)
            //    return;

            //order.SetCancelCount(1);
            //strategy.Cancel(order);
        }
    }
}
