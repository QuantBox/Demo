using QuantBox.Demo.Extension;
using QuantBox.Demo.Helper;
using QuantBox.Demo.Position;
using SmartQuant;
using SmartQuant.Indicators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QuantBox.APIProvider;
using XAPI;

namespace QuantBox.Demo.Strategy
{
    public class DoubleMA_Crossover : InstrumentStrategy
    {
        SMA fastSMA;
        SMA slowSMA;
        Group bars60Group;
        Group fillGroup;
        Group equityGroup;
        Group fastSmaGroup;
        Group slowSmaGroup;
        BarSeries bars60, bars86400;

        // 涨跌停价
        double UpperLimitPrice = 2400.0;
        double LowerLimitPrice = 2000.0;

        StrategyHelper StrategyHelper;

        public static bool SuspendTrading = false;

        [Parameter]
        public double AllocationPerInstrument = 100000;

        [Parameter]
        public double Qty = 100;

        [Parameter]
        public int FastSMALength = 8;

        [Parameter]
        public int SlowSMALength = 21;

        [Parameter]
        public long MaxBarSize = 86400;

        public DoubleMA_Crossover(Framework framework, string name)
			: base(framework, name)
		{
		}

        protected override void OnStrategyInit()
        {
            Portfolio.Account.Deposit(AllocationPerInstrument, CurrencyId.USD, "Initial allocation");

            bars60 = new BarSeries("Bars60");
            bars86400 = new BarSeries("Bars86400");

            // Set up indicators.
            fastSMA = new SMA(bars60, FastSMALength);
            slowSMA = new SMA(bars60, SlowSMALength);

            AddGroups();
        }

        protected override void OnStrategyStart()
        {
            StrategyHelper = new StrategyHelper(framework, this);

            StrategyHelper.OpenCloseHelper.SeparateOrder = SeparateOrder.SeparateCloseOpen;
            StrategyHelper.OpenCloseHelper.DefaultClose = DefaultClose.CloseToday;
            StrategyHelper.OpenCloseHelper.SendStyle = SendStyle.OneByOne;

            if(Instrument != null)
            {
                InstrumentStrategyHelper InstrumentStrategyHelper = StrategyHelper.GetInstrumentStrategyHelper(Instrument, this);
                InstrumentStrategyHelper.MarketOrderType = SmartQuant.OrderType.Limit;
                InstrumentStrategyHelper.Tick = 10;

                // 看情况是否要修正
                Console.WriteLine(string.Format("{0},{1}", Instrument.Symbol, InstrumentStrategyHelper.PriceHelper));
                // 如果是在模拟测试，需要修正此处理，防止出现需要用到交易所时间的情况下出错
                // 注意时区
                Console.WriteLine(string.Format("{0},{1}", Instrument.Symbol, InstrumentStrategyHelper.TimeHelper));
            }

            Console.WriteLine("Starting strategy in {0} mode.", Mode);
        }

        [StrategyMethod]
        public void SyncPosition()
        {
            if (StrategyHelper == null)
                return;

            StrategyHelper.SyncPosition(this.ExecutionProvider.Id, this.ExecutionProvider.Id);
        }

        [StrategyMethod]
        public void SavePosition()
        {
            if (StrategyHelper == null)
                return;

            StrategyHelper.SavePosition(string.Format(@"D:\{0}.json",this.Name));
        }

        [StrategyMethod]
        public void LoadPosition()
        {
            if (StrategyHelper == null)
                return;

            StrategyHelper.LoadPosition(string.Format(@"D:\{0}.json", this.Name));
        }

        [StrategyMethod]
        public void ReadCsv()
        {
            if (StrategyHelper == null)
                return;

            StrategyHelper.ReadCsv(@"D:\持仓_150311.csv");
        }

        protected override void OnBarOpen(Instrument instrument, Bar bar)
        {
            if (MaxBarSize == bar.Size)
            {
                DualPositionRecord record = StrategyHelper.GetPositionRecord(instrument);
                InstrumentStrategyHelper InstrumentStrategyHelper = StrategyHelper.GetInstrumentStrategyHelper(Instrument, this);

                InstrumentStrategyHelper.ChangeTradingDay(record);
                // 2.同步持仓
                //SyncPosition();
            }
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            DualPositionRecord record = StrategyHelper.GetPositionRecord(instrument);
            InstrumentStrategyHelper InstrumentStrategyHelper = StrategyHelper.GetInstrumentStrategyHelper(Instrument, this);

            // Add bar to bar series.
            if (MaxBarSize == bar.Size)
            {
                bars86400.Add(bar);
                return;
            }
            else
            {
                bars60.Add(bar);
                Log(bar, bars60Group);
            }
            
            //if (!SuspendTrading)
            //{
            //    Console.WriteLine("fastSMA.Count = {0}", fastSMA.Count);
            //    Console.WriteLine("slowSMA.Count = {0}", slowSMA.Count);
            //}

            if (fastSMA.Count <= 0)
            {
                return;
            }
            Log(fastSMA.Last, fastSmaGroup);

            if (slowSMA.Count <= 0)
            {
                return;
            }
            Log(slowSMA.Last, slowSmaGroup);

            // Update performance.
            Portfolio.Performance.Update();

            // Log equity.
            Log(Portfolio.Value, equityGroup);

            if (SuspendTrading)
            {
                return;
            }
            
            do
            {
                // 尾盘平仓
                if (TimeHelper.GetTime(Clock.DateTime) > 1500 && TimeHelper.GetTime(Clock.DateTime) < 2055)
                {
                    record.TargetPosition = 0;
                    break;
                }

                // 开仓条件
                Cross cross = fastSMA.Crosses(slowSMA, bar.DateTime);
                switch (cross)
                {
                    case Cross.Above:
                        record.TargetPosition = 1;
                        record.Text = "上行";
                        break;
                    case Cross.Below:
                        record.TargetPosition = -1;
                        record.Text = "下行";
                        break;
                }

                

            }while(false);

            

            InstrumentStrategyHelper.Process(record, bar.Close);
        }

        protected override void OnPositionOpened(SmartQuant.Position position)
        {
            // 止损条件
            // 1、亏损固定点数
            // 2、亏损当前价格的百分比
            //StopEx stop = new StopEx(this, position, 0.01, StopType.Trailing, StopMode.Percent, StopIndicator.Value);
            //stop.TraceOnBar = false;
            //stop.TraceOnTrade = true;
            //AddStop(stop);
        }

        protected override void OnStopExecuted(Stop stop)
        {
            // 止损出场
            //StrategyHelper.ClosePosition(Position, "止损");
        }

        protected override void OnTrade(Instrument instrument, Trade trade)
        {
            //if (!SuspendTrading)
            //{
            //    Console.WriteLine(trade);
            //}
            //TradeEx t = trade as TradeEx;
            //Console.WriteLine(t.DepthMarketData.AskPrice1);
            //Console.WriteLine(t.DepthMarketData.OpenInterest);
        }

        protected override void OnExecutionReport(ExecutionReport report)
        {
            // 用于更新其中的持仓数量等信息
            // 反手等事件的处理
            StrategyHelper.OnExecutionReport(report);
        }

        protected override void OnFill(Fill fill)
        {
            // Add fill to group.
            Log(fill, fillGroup);
        }

        private void AddGroups()
        {
            // Create bars group.
            bars60Group = new Group("Bars");
            bars60Group.Add("Pad", DataObjectType.String, 0);
            bars60Group.Add("SelectorKey", Instrument.Symbol);

            // Create fills group.
            fillGroup = new Group("Fills");
            fillGroup.Add("Pad", 0);
            fillGroup.Add("SelectorKey", Instrument.Symbol);

            // Create equity group.
            equityGroup = new Group("Equity");
            equityGroup.Add("Pad", 1);
            equityGroup.Add("SelectorKey", Instrument.Symbol);

            // Create fast sma values group.
            fastSmaGroup = new Group("FastSMA");
            fastSmaGroup.Add("Pad", 0);
            fastSmaGroup.Add("SelectorKey", Instrument.Symbol);
            fastSmaGroup.Add("Color", Color.Green);

            // Create slow sma values group.
            slowSmaGroup = new Group("SlowSMA");
            slowSmaGroup.Add("Pad", 0);
            slowSmaGroup.Add("SelectorKey", Instrument.Symbol);
            slowSmaGroup.Add("Color", Color.Red);

            // Add groups to manager.
            GroupManager.Add(bars60Group);
            GroupManager.Add(fillGroup);
            //GroupManager.Add(equityGroup);
            GroupManager.Add(fastSmaGroup);
            GroupManager.Add(slowSmaGroup);
        }
    }
}


