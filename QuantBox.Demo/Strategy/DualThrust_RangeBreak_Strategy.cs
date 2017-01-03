using QuantBox.Demo.Extension;
using QuantBox.Demo.Helper;
using QuantBox.Demo.Indicator;
using QuantBox.Demo.Position;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantBox.APIProvider;
using QuantBox.Demo.Statistics;

namespace QuantBox.Demo.Strategy
{
    public class DualThrust_RangeBreak_Strategy : InstrumentStrategy
    {
        public enum StrategyType
        {
            DualThrust,
            RangeBreak,
        }

        [Parameter]
        //public StrategyType strategyType = StrategyType.DualThrust;
        public StrategyType strategyType = StrategyType.RangeBreak;
        [Description("K1，上轨的参数")]
        [Parameter]
        public double K1 = 0.3;
        [Description("K2，下轨的参数")]
        [Parameter]
        public double K2 = 0.3;
        [Parameter]
        public int N = 3;
        [Parameter]
        public double Qty = 1;
        [Parameter]
        public long MaxBarSize = 86400;
        [Description("最大失败次数")]
        [Parameter]
        public int MaxLoss = 10;
        [Description("最大连续失败次数")]
        [Parameter]
        public int MaxConsecutiveLoss = 3;
        [Description("K3，接近涨跌停价的参数")]
        [Parameter]
        public double K3 = 0.01;

        Group barsGroup;
        Group hhGroup;
        Group hcGroup;
        Group lcGroup;
        Group llGroup;
        Group rangeGroup;
        Group fillGroup;
        Group equityGroup;
        Group range2Group;
        Group range3Group;

        BarSeries bars86400;

        PriceChannel HH;
        PriceChannel HC;
        PriceChannel LC;
        PriceChannel LL;

        double UpperBand = double.NaN;
        double LowerBand = double.NaN;
        double UpperLimitRangeLongEntry = double.NaN;
        double LowerLimitRangeShortEntry = double.NaN;

        public double UpperLimitPrice = 2400.0;
        public double LowerLimitPrice = 2000.0;
        
        StrategyHelper StrategyHelper;

        bool flgLongStopped = false;
        bool flgShortStopped = false;

        private PortfolioStatisticsItem dailyNumOfLossTrades;
        private PortfolioStatisticsItem dailyConsecutiveLossTrades;

        private TrailingPrice TrailingPrice;

        public DualThrust_RangeBreak_Strategy(Framework framework, string name)
            : base(framework, name)
        {
        }

        protected override void OnStrategyInit()
        {
            bars86400 = new BarSeries("Bars86400");

            int n = N;
            if (StrategyType.RangeBreak == strategyType)
            {
                n = 1;
            }

            HH = new PriceChannel(bars86400, n, PriceChannel.CalcType.Max, PriceChannel.IncludeLast.Yes, BarData.High);
            HC = new PriceChannel(bars86400, n, PriceChannel.CalcType.Max, PriceChannel.IncludeLast.Yes, BarData.Close);
            LC = new PriceChannel(bars86400, n, PriceChannel.CalcType.Min, PriceChannel.IncludeLast.Yes, BarData.Close);
            LL = new PriceChannel(bars86400, n, PriceChannel.CalcType.Min, PriceChannel.IncludeLast.Yes, BarData.Low);

            dailyNumOfLossTrades = Portfolio.Statistics.Get(QuantBox.Demo.Statistics.PortfolioStatisticsType.DailyNumOfLossTrades);
            dailyConsecutiveLossTrades = Portfolio.Statistics.Get(QuantBox.Demo.Statistics.PortfolioStatisticsType.DailyConsecutiveLossTrades);

            AddGroups();
        }
        
        protected override void OnStrategyStart()
        {
            StrategyHelper = new StrategyHelper(framework, this);

            StrategyHelper.OpenCloseHelper.SeparateOrder = SeparateOrder.SeparateCloseOpen;
            StrategyHelper.OpenCloseHelper.DefaultClose = DefaultClose.CloseToday;
            StrategyHelper.OpenCloseHelper.SendStyle = SendStyle.OneByOne;

            if (Instrument != null)
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

            TrailingPrice = new TrailingPrice(framework, this);
        }

        protected override void OnBarOpen(Instrument instrument, Bar bar)
        {
            if (MaxBarSize == bar.Size)
            {
                // 今天的开盘价
                double dbOpen = bar.Open;

                // 得到可以开仓的边界，
                //UpperLimitRangeLongEntry = dbOpen + 10;
                //LowerLimitRangeShortEntry = dbOpen - 10;
                //UpperLimitPrice = dbOpen + 20;
                //LowerLimitPrice = dbOpen - 20;

                UpperLimitRangeLongEntry = UpperLimitPrice - K3 * dbOpen;
                LowerLimitRangeShortEntry = LowerLimitPrice + K3 * dbOpen;

                // 1.自动切换
                StrategyHelper.ChangeTradingDay();
                // 2.同步持仓
                //StrategyHelper.SyncPosition();

                if (HH.Count < 1)
                    return;

                double Range = 0;

                if (StrategyType.RangeBreak == strategyType)
                {
                    Range = HH.Last - LL.Last;
                }
                else
                {
                    Range = Math.Max(HH.Last - LC.Last, HC.Last - LL.Last);
                }

                Log(Range, rangeGroup);

                // V1.如果昨天波动过小，调整一下最小range，此为开盘价的0.2%
                Range = Math.Max(Range, dbOpen * 0.01 * 0.2);

                // 在RangeBreak中K1==K2
                UpperBand = dbOpen + K1 * Range;
                LowerBand = dbOpen - K2 * Range;
            }
        }

        protected override void OnStrategyStop()
        {
            //for (int i = 0; i < dailyNumOfWinTrades.TotalValues.Count - 1; ++i)
            //{
            //    Console.WriteLine("{0},{1},{2},{3}", dailyNumOfLossTrades.TotalValues.GetDateTime(i),dailyNumOfLossTrades.TotalValues[i], dailyNumOfWinTrades.TotalValues[i], dailyConsecutiveLossTrades.TotalValues[i]);
            //}
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            StrategyHelper.StatisticsHelper.Update(instrument, bar.Close);

            DualPositionRecord record = StrategyHelper.GetPositionRecord(instrument);
            InstrumentStrategyHelper InstrumentStrategyHelper = StrategyHelper.GetInstrumentStrategyHelper(Instrument, this);

            if (MaxBarSize == bar.Size)
            {
                bars86400.Add(bar);
                return;
            }
            else
            {
                Log(bar, barsGroup);
            }

            if (HH.Count > 0)
            {
                Log(HH.Last, hhGroup);
                Log(HC.Last, hcGroup);
                Log(LC.Last, lcGroup);
                Log(LL.Last, llGroup);
            }
            
            // Update performance.
            Portfolio.Performance.Update();

            // Log equity.
            Log(Portfolio.Value, equityGroup);


            do
            {
                // 尾盘平仓
                if (TimeHelper.GetTime(Clock.DateTime) > 1500 && TimeHelper.GetTime(Clock.DateTime) < 2100)
                {
                    record.TargetPosition = 0;
                    break;
                }

                double price = bar.Close;
                double NetQty = record.CurrentPosition;
                if (NetQty == 0)
                {
                    if (price >= UpperBand)
                    {
                        record.TargetPosition = 1;
                    }
                    if (price <= LowerBand)
                    {
                        record.TargetPosition = -1;
                    }
                }
                else if (NetQty > 0)
                {
                    if (price <= LowerBand)
                    {
                        record.TargetPosition = -1;
                    }
                }
                else if (NetQty < 0)
                {
                    if (price >= UpperBand)
                    {
                        record.TargetPosition = 1;
                    }
                }

                // 策略改进:14:00以后了只让平仓，不让开仓
                if (TimeHelper.GetTime(Clock.DateTime) > 1400)
                {
                    record.FilterCloseOnly();
                }

                // 策略改进:达到当日亏损次数上限和当日连续亏损次数上限后不让开仓
                if (dailyNumOfLossTrades.TotalValue >= MaxLoss || dailyConsecutiveLossTrades.TotalValue >= MaxConsecutiveLoss)
                {
                    record.FilterCloseOnly();
                }

            } while (false);

            InstrumentStrategyHelper.Process(record, bar.Close);
            
            //// 价格达到涨跌停
            //if (bar.Close >= UpperLimitPrice)
            //{
            //    //// 这个地方方向是否有问题？会不会将我在这个地方的反方向开仓给平了？
            //    //StrategyHelper.ClosePosition(Position, "涨跌停平仓");
            //    //// 由于在这之前已经先达到了

            //    // 01.19 判断一下持仓方向，然后决定是否平仓（如涨停时，持有多仓则不应平仓）
            //    if (HasShortPosition(instrument))
            //    {
            //        StrategyHelper.ClosePosition(Position, "涨停平仓");
            //    }
                
            //    return;
            //}
            //else if (bar.Close <= LowerLimitPrice)
            //{
            //    // 01.19 判断一下持仓方向，然后决定是否平仓（如跌停时，持有空仓则不应平仓）
            //    if (HasLongPosition(instrument))
            //    {
            //        StrategyHelper.ClosePosition(Position, "跌停平仓");
            //    }
            //}

            // 开仓条件
            //if (bar.Close >= UpperBand)
            //{
            //    if (HasShortPosition(instrument))
            //    {
            //        Order order = StrategyHelper.BuyOrder(instrument, Position.Qty + Qty, "AA");
            //        order = StrategyHelper.RebuildOrder(order);
            //        StrategyHelper.Send(order);
            //        return;
            //    }

            //    if (!HasPosition(instrument))
            //    {
            //        if (!flgLongStopped)
            //        {
            //            Order order = StrategyHelper.BuyOrder(instrument, Position.Qty + Qty, "AA");
            //            order = StrategyHelper.RebuildOrder(order);
            //            StrategyHelper.Send(order);
            //            return;
            //        }

            //        if (bar.Close > TrailingPrice.HighestAfterEntry)
            //        {
            //            Order order = StrategyHelper.BuyOrder(instrument, Position.Qty + Qty, "AA");
            //            order = StrategyHelper.RebuildOrder(order);
            //            StrategyHelper.Send(order);
            //            return;
            //        }
            //    }
            //}

            //if (bar.Close <= LowerBand)
            //{
            //    if (HasLongPosition(instrument))
            //    {
            //        Order order = StrategyHelper.SellOrder(instrument, Position.Qty + Qty, "BB");
            //        order = StrategyHelper.RebuildOrder(order);
            //        StrategyHelper.Send(order);
            //        return;
            //    }

            //    if (!HasPosition(instrument))
            //    {
            //        if (!flgShortStopped)
            //        {
            //            Order order = StrategyHelper.SellOrder(instrument, Position.Qty + Qty, "BB");
            //            order = StrategyHelper.RebuildOrder(order);
            //            StrategyHelper.Send(order);
            //            return;
            //        }

            //        if (bar.Close < TrailingPrice.LowestAfterEntry)
            //        {
            //            Order order = StrategyHelper.SellOrder(instrument, Position.Qty + Qty, "BB");
            //            order = StrategyHelper.RebuildOrder(order);
            //            StrategyHelper.Send(order);
            //            return;
            //        }
            //    }
            //}
        }

        protected override void OnTrade(Instrument instrument, Trade trade)
        {
            StrategyHelper.StatisticsHelper.Update(instrument, trade.Price);
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

            // 记下前期的高点
            TrailingPrice = new TrailingPrice(StrategyHelper.StatisticsHelper.TrailingPrice);

            StopEx s = stop as StopEx;
            
            //switch (s.Side)
            //{
            //    case PositionSide.Long:
            //        flgLongStopped = true;
            //        break;
            //    case PositionSide.Short:
            //        flgShortStopped = true;
            //        break;
            //    default:
            //        break;
            //}

            // 止损出场
            //StrategyHelper.ClosePosition(Position, "止损");
        }

        protected override void OnReminder(DateTime dateTime, object data)
        {
            StrategyHelper.OnReminder(dateTime, data);
        }

        protected override void OnExecutionReport(ExecutionReport report)
        {
            StrategyHelper.OnExecutionReport(report);
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

            // Create fills group.
            fillGroup = new Group("Fills");
            fillGroup.Add("Pad", 0);
            fillGroup.Add("SelectorKey", Instrument.Symbol);

            // Create equity group.
            equityGroup = new Group("Equity");
            equityGroup.Add("Pad", 1);
            equityGroup.Add("SelectorKey", Instrument.Symbol);

            hhGroup = new Group("HH");
            hhGroup.Add("Pad", 0);
            hhGroup.Add("SelectorKey", Instrument.Symbol);
            hhGroup.Add("Color", Color.Red);

            hcGroup = new Group("HC");
            hcGroup.Add("Pad", 0);
            hcGroup.Add("SelectorKey", Instrument.Symbol);
            hcGroup.Add("Color", Color.Yellow);

            lcGroup = new Group("LC");
            lcGroup.Add("Pad", 0);
            lcGroup.Add("SelectorKey", Instrument.Symbol);
            lcGroup.Add("Color", Color.Yellow);

            llGroup = new Group("LL");
            llGroup.Add("Pad", 0);
            llGroup.Add("SelectorKey", Instrument.Symbol);
            llGroup.Add("Color", Color.Red);

            rangeGroup = new Group("Range");
            rangeGroup.Add("Pad", 2);
            rangeGroup.Add("SelectorKey", Instrument.Symbol);
            rangeGroup.Add("Color", Color.Red);

            range2Group = new Group("Win");
            range2Group.Add("Pad", 3);
            range2Group.Add("SelectorKey", Instrument.Symbol);
            range2Group.Add("Color", Color.Red);

            range3Group = new Group("Loss");
            range3Group.Add("Pad", 3);
            range3Group.Add("SelectorKey", Instrument.Symbol);
            range3Group.Add("Color", Color.Green);

            GroupManager.Add(barsGroup);
            GroupManager.Add(fillGroup);
            GroupManager.Add(equityGroup); 
            GroupManager.Add(hhGroup);
            GroupManager.Add(hcGroup);
            GroupManager.Add(lcGroup);
            GroupManager.Add(llGroup);
            GroupManager.Add(rangeGroup);
            GroupManager.Add(range2Group);
            GroupManager.Add(range3Group);
        }
    }
}