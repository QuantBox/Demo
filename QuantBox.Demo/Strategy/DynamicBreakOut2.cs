using QuantBox.Demo.Extension;
using QuantBox.Demo.Indicator;
using SmartQuant;
using SmartQuant.Indicators;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using QuantBox.Demo.Helper;
using QuantBox.Demo.Position;

namespace QuantBox.Demo.Strategy
{
    public class DynamicBreakOut2 : InstrumentStrategy
    {
        // 指标窗口长度（LookBackDays）的上限
        [Parameter]
        int ceilingAmt = 60;

        // 指标窗口长度（LookBackDays）的下限
        [Parameter]
        int floorAmt = 20;

        // 布林线参数
        [Parameter]
        double bolBandTrig = 2;

        // 合约数目
        [Parameter]
        double Qty = 1;

        [Parameter]
        int length = 30;

        [Parameter]
        public long MaxBarSize = 0;

        [Parameter]
        public double AllocationPerInstrument = 100000;

        // 指标窗口长度（LookBackDays）的初始值
        int lookBackDays = 20;

        int nBegin, nEnd;
        double buyPoint, sellPoint;
        double upBand, dnBand;
        double longLiqPoint, shortLiqPoint;
        
        SMD smd30;
        LookBackDays lbd;

        BarSeries bars60;
        BarSeries bars86400;
        
        Group bars60Group, bars86400Group;
        Group smd30Group;
        Group lbdGroup;
        Group buyPointGroup, sellPointGroup;
        Group longLiqPointGroup, shortLiqPointGroup;
        Group upBandGroup, dnBandGroup;

        StrategyHelper StrategyHelper;

        public DynamicBreakOut2(Framework framework, string name)
            : base(framework, name)
        {
        }

        protected override void OnStrategyInit()
        {
            Portfolio.Account.Deposit(AllocationPerInstrument, CurrencyId.USD, "Initial allocation");

            bars60 = new BarSeries("Bars60");
            bars86400 = new BarSeries("Bars86400");
            
            smd30 = new SMD(bars86400, length);
            lbd = new LookBackDays(smd30, lookBackDays, floorAmt, ceilingAmt);
            
            AddGroups();
        }

        protected override void OnStrategyStart()
        {
            StrategyHelper = new StrategyHelper(framework, this);
            StrategyHelper.OpenCloseHelper = new OpenCloseHelper(framework, this);
            //StrategyHelper.PriceHelper = new PriceHelper(framework, 0.001);
            //StrategyHelper.TimeHelper = new TimeHelper(Instrument.Symbol);

            //StrategyHelper.MarketOrderType = SmartQuant.OrderType.Limit;
            //StrategyHelper.Tick = 10;

            StrategyHelper.OpenCloseHelper.SeparateOrder = SeparateOrder.SeparateCloseOpen;
            StrategyHelper.OpenCloseHelper.DefaultClose = DefaultClose.CloseToday;
            StrategyHelper.OpenCloseHelper.SendStyle = SendStyle.OneByOne;

            //StrategyHelper.EnableTrading = true;
            //StrategyHelper.EnableLongEntry = true;
            //StrategyHelper.EnableShortEntry = true;
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            if (MaxBarSize == bar.Size)
            {
                bars86400.Add(bar);
                //Log(bar, bars86400Group);
                Update();                
                return;
            }
            else
            {
                bars60.Add(bar);
                Log(bar, bars60Group);
            }

            if (lbd.Count < 1)
            {
                return;
            }

            if (double.IsNaN(longLiqPoint))
            {
                return;
            }

            Log(lbd.Last, lbdGroup);
            Log(smd30.Last, smd30Group);
            Log(buyPoint, buyPointGroup);
            Log(sellPoint, sellPointGroup);
            Log(longLiqPoint, longLiqPointGroup);
            Log(shortLiqPoint, shortLiqPointGroup);
            Log(upBand, upBandGroup);
            Log(dnBand, dnBandGroup);
            
            // 平仓条件
            // 尾盘平仓，并设置不能开新仓
            if (TimeHelper.GetTime(Clock.DateTime) > 1500 && TimeHelper.GetTime(Clock.DateTime) < 2055)
            {
                //StrategyHelper.TargetPosition = 0;
                return;
            }
            // 超过出清价位，平仓
            if (bar.Close <= longLiqPoint)
            {
                if (HasLongPosition(instrument))
                {
                    //StrategyHelper.TargetPosition = 0;
                    return;
                }
            }
            if (bar.Close >= shortLiqPoint)
            {
                if (HasShortPosition(instrument))
                {
                    //StrategyHelper.TargetPosition = 0;
                    return;
                }
            }
            
            double closeYesterday = bars86400.Last.Close;

            // 开仓条件
            // 开多仓
            //if (closeYesterday > upBand)
            //{
            //    if (bar.Close >= buyPoint && !HasPosition(instrument))
            //    {
            //        Order order = StrategyHelper.BuyOrder(instrument, Qty, "Long Entry");
            //        order = StrategyHelper.RebuildOrder(order);
            //        StrategyHelper.Send(order);
            //        return;
            //    }
            //}
            //// 开空仓
            //if (closeYesterday < dnBand)
            //{
            //    if (bar.Close <= sellPoint && !HasPosition(instrument))
            //    {
            //        Order order = StrategyHelper.SellOrder(instrument, Qty, "Short Entry");
            //        order = StrategyHelper.RebuildOrder(order);
            //        StrategyHelper.Send(order);
            //        return;
            //    }
            //}
        }

        protected override void OnExecutionReport(ExecutionReport report)
        {
            //StrategyHelper.OnExecutionReport(report);
        }

        private void AddGroups()
        {
            bars60Group = new Group("Bars60");
            bars60Group.Add("Pad", DataObjectType.String, 0);
            bars60Group.Add("SelectorKey", Instrument.Symbol);

            bars86400Group = new Group("Bars86400");
            bars86400Group.Add("Pad", DataObjectType.String, 0);
            bars86400Group.Add("SelectorKey", Instrument.Symbol);

            smd30Group = new Group("SMD30");
            smd30Group.Add("Pad", 1);
            smd30Group.Add("SelectorKey", Instrument.Symbol);
            smd30Group.Add("Color", Color.Red);

            lbdGroup = new Group("LookBackDays");
            lbdGroup.Add("Pad", 2);
            lbdGroup.Add("SelectorKey", Instrument.Symbol);
            lbdGroup.Add("Color", Color.Green);

            buyPointGroup = new Group("BuyPoint");
            buyPointGroup.Add("Pad", 0);
            buyPointGroup.Add("SelectorKey", Instrument.Symbol);
            buyPointGroup.Add("Color", Color.Blue);

            sellPointGroup = new Group("SellPoint");
            sellPointGroup.Add("Pad", 0);
            sellPointGroup.Add("SelectorKey", Instrument.Symbol);
            sellPointGroup.Add("Color", Color.Yellow);

            longLiqPointGroup = new Group("LongLiqPoint");
            longLiqPointGroup.Add("Pad", 0);
            longLiqPointGroup.Add("SelectorKey", Instrument.Symbol);
            longLiqPointGroup.Add("Color", Color.Purple);

            shortLiqPointGroup = new Group("ShortLiqGroup");
            shortLiqPointGroup.Add("Pad", 0);
            shortLiqPointGroup.Add("SelectorKey", Instrument.Symbol);
            shortLiqPointGroup.Add("Color", Color.Pink);

            upBandGroup = new Group("UpBand(BBU)");
            upBandGroup.Add("Pad", 0);
            upBandGroup.Add("SelectorKey", Instrument.Symbol);
            upBandGroup.Add("Color", Color.Brown);

            dnBandGroup = new Group("DownBand(BBL)");
            dnBandGroup.Add("Pad", 0);
            dnBandGroup.Add("SelectorKey", Instrument.Symbol);
            dnBandGroup.Add("Color", Color.Gray);

            GroupManager.Add(bars60Group);
            GroupManager.Add(bars86400Group);
            GroupManager.Add(smd30Group);
            GroupManager.Add(lbdGroup);
            GroupManager.Add(buyPointGroup);
            GroupManager.Add(sellPointGroup);
            GroupManager.Add(longLiqPointGroup);
            GroupManager.Add(shortLiqPointGroup);
            GroupManager.Add(upBandGroup);
            GroupManager.Add(dnBandGroup);
        }

        private void Update()
        {
            if (lbd.Count < 1)
            {
                return;
            }

            int lookBackDaysInt = (int)lbd.Last;
            longLiqPoint = SMA.Value(bars86400, bars86400.Count - 1, lookBackDaysInt, BarData.Close);

            if (double.IsNaN(longLiqPoint))
            {
                return;
            }

            nEnd = bars86400.Count - 1;
            nBegin = nEnd - lookBackDays + 1;

            buyPoint = bars86400.HighestHigh(nBegin, nEnd);
            sellPoint = bars86400.LowestLow(nBegin, nEnd);
            shortLiqPoint = SMA.Value(bars86400, bars86400.Count - 1, lookBackDaysInt, BarData.Close);
            upBand = BBU.Value(bars86400, bars86400.Count - 1, lookBackDaysInt, bolBandTrig, BarData.Close);
            dnBand = BBL.Value(bars86400, bars86400.Count - 1, lookBackDaysInt, bolBandTrig, BarData.Close);
        }
    }
}
