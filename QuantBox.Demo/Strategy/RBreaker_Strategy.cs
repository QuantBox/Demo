using QuantBox.Demo.Extension;
using QuantBox.Demo.Helper;
using QuantBox.Demo.Position;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Strategy
{
    public class RBreaker_Strategy: InstrumentStrategy
    {
        [Parameter]
        double f1 = 0.35;

        [Parameter]
        double f2 = 0.07;

        [Parameter]
        double f3 = 0.12;

        [Parameter]
        public double Qty = 1;

        [Parameter]
        public long MaxBarSize = 0;

        [Parameter]
        public double AllocationPerInstrument = 100000;

        double reverse = 0.5;
        double rangemin = 0.2;
        double xdiv = 3;

        double div = 3;
        double i_reverse = 1;
        double i_rangemin = 1;
        bool rfilter;

        Group barsGroup;
        Group fillGroup;
        Group equityGroup;
        Group ssetupGroup;
        Group bsetupGroup;
        Group bbreakGroup;
        Group sbreakGroup;
        Group senterGroup;
        Group benterGroup;

        BarSeries bars86400;

        double UpLine = double.NaN;
        double DownLine = double.NaN;

        StrategyHelper StrategyHelper;

        public RBreaker_Strategy(Framework framework, string name)
            : base(framework, name)
        {
        }

        protected override void OnStrategyInit()
        {
            Portfolio.Account.Deposit(AllocationPerInstrument, CurrencyId.USD, "Initial allocation");
            bars86400 = new BarSeries("Bars86400");
            AddGroups();
        }
        
        protected override void OnStrategyStart()
        {
            StrategyHelper = new StrategyHelper(framework, this);
            StrategyHelper.OpenCloseHelper = new OpenCloseHelper(framework, this);
            //StrategyHelper.PriceHelper = new PriceHelper(framework, 0.001);
            //StrategyHelper.TimeHelper = new TimeHelper(Instrument.Symbol);
            //StrategyHelper.StatisticsHelper = new StatisticsHelper(framework, this);

            //StrategyHelper.MarketOrderType = SmartQuant.OrderType.Limit;
            //StrategyHelper.Tick = 10;

            StrategyHelper.OpenCloseHelper.SeparateOrder = SeparateOrder.SeparateCloseOpen;
            StrategyHelper.OpenCloseHelper.DefaultClose = DefaultClose.CloseToday;
            StrategyHelper.OpenCloseHelper.SendStyle = SendStyle.OneByOne;

            //StrategyHelper.EnableTrading = true;
            //StrategyHelper.EnableLongEntry = true;
            //StrategyHelper.EnableShortEntry = true;
        }

        protected override void OnBarOpen(Instrument instrument, Bar bar)
        {
            if (MaxBarSize == bar.Size)
            {
                i_reverse = reverse * (bar.Open / 100.0);
                i_rangemin = rangemin * (bar.Open / 100.0);
                div = Math.Max(xdiv, 1);

                //StrategyHelper.EnableTrading = true;
                //StrategyHelper.EnableLongEntry = true;
                //StrategyHelper.EnableShortEntry = true;

                // 1.自动切换
                StrategyHelper.ChangeTradingDay();
                // 2.同步持仓
                //StrategyHelper.SyncPosition();
            }
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            if (MaxBarSize == bar.Size)
            {
                bars86400.Add(bar);

                // 更新RBreaker各项数据
                UpdateDaily_RBreaker(bar);

                // 重置日内的最高最低
                StrategyHelper.StatisticsHelper.HighLowPrice.Reset();

                rfilter = (bar.High - bar.Low) >= i_rangemin;
                return;
            }
            else
            {
                // 更新日内的最高最低
                StrategyHelper.StatisticsHelper.HighLowPrice.Update(bar);
                
                Log(bar, barsGroup);
                DrawLines();
            }

            // Update performance.
            Portfolio.Performance.Update();

            // Log equity.
            Log(Portfolio.Value, equityGroup);

            //StrategyHelper.EnableTrading = true;
            //StrategyHelper.EnableLongEntry = true;
            //StrategyHelper.EnableShortEntry = true;

            // 平仓条件
            // 尾盘平仓，并设置不能开新仓
            if (TimeHelper.GetTime(Clock.DateTime) > 1500 && TimeHelper.GetTime(Clock.DateTime) < 2055)
            {
                //StrategyHelper.ClosePosition(Position, "尾盘平仓");

                //StrategyHelper.EnableLongEntry = false;
                //StrategyHelper.EnableShortEntry = false;
                return;
            }

            // 开仓条件
            double HighToday = StrategyHelper.StatisticsHelper.HighLowPrice.High;
            double LowToday = StrategyHelper.StatisticsHelper.HighLowPrice.Low;

            double _S1 = _senter + (HighToday - _ssetup) / div;
            double _B1 = _benter - (_bsetup - LowToday) / div;

            //if (HasLongPosition(instrument))
            //{
            //    if ((HighToday > _ssetup && bar.Close < _S1) || bar.Close < _sbreak)
            //    {
            //        Order order = StrategyHelper.SellOrder(instrument, Position.Qty + Qty, "AA");
            //        order = StrategyHelper.RebuildOrder(order);
            //        StrategyHelper.Send(order);
            //    }
            //}
            //else if (HasShortPosition(instrument))
            //{
            //    if ((LowToday < _bsetup && bar.Close > _B1) || bar.Close > _bbreak)
            //    {
            //        Order order = StrategyHelper.BuyOrder(instrument, Position.Qty + Qty, "BB");
            //        order = StrategyHelper.RebuildOrder(order);
            //        StrategyHelper.Send(order);
            //    }
            //}
            //else
            //{
            //    if (bar.Close > _bbreak)
            //    {
            //        Order order = StrategyHelper.BuyOrder(instrument, Position.Qty + Qty, "CC");
            //        order = StrategyHelper.RebuildOrder(order);
            //        StrategyHelper.Send(order);
            //    }
            //    if (bar.Close < _sbreak)
            //    {
            //        Order order = StrategyHelper.SellOrder(instrument, Position.Qty + Qty, "DD");
            //        order = StrategyHelper.RebuildOrder(order);
            //        StrategyHelper.Send(order);
            //    }
            //}
        }

        protected override void OnTrade(Instrument instrument, Trade trade)
        {
            StrategyHelper.StatisticsHelper.HighLowPrice.Update(trade.Price);
        }

        protected override void OnPositionOpened(SmartQuant.Position position)
        {
            // 止损条件
            // 1、亏损固定点数
            // 2、亏损当前价格的百分比
            StopEx stop = new StopEx(this, position, 0.01, StopType.Trailing, StopMode.Percent, StopIndicator.Value);
            stop.TraceOnBar = false;
            stop.TraceOnTrade = true;
            AddStop(stop);

            //flgLongStopped = false;
            //flgShortStopped = false;
        }

        protected override void OnStopExecuted(Stop stop)
        {
            // 再进场原则，止损后条件还是满足开仓条件
            // 1.必须突破前期高低点/低点
            // 2.必须待了足够长时间

            // 止损出场
            //StrategyHelper.ClosePosition(Position, "止损");
        }

        protected override void OnExecutionReport(ExecutionReport report)
        {
            //StrategyHelper.OnExecutionReport(report);
        }

        protected override void OnFill(Fill fill)
        {
            // Add fill to group.
            Log(fill, fillGroup);
        }

        private void AddGroups()
        {
            barsGroup = new Group("Bars");
            barsGroup.Add("Pad", 0);
            barsGroup.Add("SelectorKey", Instrument.Symbol);
            barsGroup.Add("Color", Color.Black);

            //// Create fills group.
            fillGroup = new Group("Fills");
            fillGroup.Add("Pad", 0);
            fillGroup.Add("SelectorKey", Instrument.Symbol);

            // Create equity group.
            equityGroup = new Group("Equity");
            equityGroup.Add("Pad", 1);
            equityGroup.Add("SelectorKey", Instrument.Symbol);

            ssetupGroup = new Group("观察卖出价");
            ssetupGroup.Add("Pad", 0);
            ssetupGroup.Add("SelectorKey", Instrument.Symbol);
            ssetupGroup.Add("Color", Color.Green);

            bsetupGroup = new Group("观察买入价");
            bsetupGroup.Add("Pad", 0);
            bsetupGroup.Add("SelectorKey", Instrument.Symbol);
            bsetupGroup.Add("Color", Color.Green);

            bbreakGroup = new Group("突破买入价");
            bbreakGroup.Add("Pad", 0);
            bbreakGroup.Add("SelectorKey", Instrument.Symbol);
            bbreakGroup.Add("Color", Color.Red);

            sbreakGroup = new Group("突破卖出价");
            sbreakGroup.Add("Pad", 0);
            sbreakGroup.Add("SelectorKey", Instrument.Symbol);
            sbreakGroup.Add("Color", Color.Red);

            senterGroup = new Group("反转卖出价");
            senterGroup.Add("Pad", 0);
            senterGroup.Add("SelectorKey", Instrument.Symbol);
            senterGroup.Add("Color", Color.Black);

            benterGroup = new Group("反转买入价");
            benterGroup.Add("Pad", 0);
            benterGroup.Add("SelectorKey", Instrument.Symbol);
            benterGroup.Add("Color", Color.Black);


            GroupManager.Add(barsGroup);
            GroupManager.Add(fillGroup);
            GroupManager.Add(equityGroup);
            GroupManager.Add(ssetupGroup);
            GroupManager.Add(bsetupGroup);
            GroupManager.Add(bbreakGroup);
            GroupManager.Add(sbreakGroup);
            GroupManager.Add(senterGroup);
            GroupManager.Add(benterGroup);
        }

        #region R-Breaker
        double _ssetup = double.NaN;
        double _bsetup = double.NaN;
        double _senter = double.NaN;
        double _benter = double.NaN;
        double _bbreak = double.NaN;
        double _sbreak = double.NaN;

        void UpdateDaily_RBreaker(Bar bar)
        {
            double preDayHigh = bar.High;
            double preDayLow = bar.Low;
            double preDayClose = bar.Close;

            //先计算，时候到了再放到序列中
            _ssetup = preDayHigh + f1 * (preDayClose - preDayLow);
            _bsetup = preDayLow - f1 * (preDayHigh - preDayClose);
            
            // 01.21 修改了计算公式
            _senter = (1 + f2) / 2.0 * (preDayHigh + preDayLow) - f2 * preDayLow;
            _benter = (1 + f2) / 2.0 * (preDayHigh + preDayLow) - f2 * preDayHigh;

            _bbreak = _ssetup + f3 * (_ssetup - _bsetup);
            _sbreak = _bsetup - f3 * (_ssetup - _bsetup);
        }

        private void DrawLines()
        {
            if (double.IsNaN(_ssetup))
            {
                return;
            }

            Log(_ssetup, ssetupGroup);
            Log(_bsetup, bsetupGroup);
            Log(_bbreak, bbreakGroup);
            Log(_sbreak, sbreakGroup);
            Log(_senter, senterGroup);
            Log(_benter, benterGroup);
        }
        #endregion
    }
}
