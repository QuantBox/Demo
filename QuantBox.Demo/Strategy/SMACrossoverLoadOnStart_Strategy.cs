using SmartQuant;
using SmartQuant.Indicators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Strategy
{
    public class SMACrossoverLoadOnStart_Strategy : InstrumentStrategy
	{
		private SMA fastSMA;
		private SMA slowSMA;
		private Group barsGroup;
		private Group fillGroup;
		private Group equityGroup;
		private Group fastSmaGroup;
		private Group slowSmaGroup;

		public static bool SuspendTrading = false;

		[Parameter]
		public double AllocationPerInstrument = 100000;

		[Parameter]
		public double Qty = 100;

		[Parameter]
		public int FastSMALength = 8;

		[Parameter]
		public int SlowSMALength = 21;

        public SMACrossoverLoadOnStart_Strategy(Framework framework, string name)
			: base(framework, name)
		{
		}

		protected override void OnStrategyInit()
		{
            //if (SuspendTrading)
            //{
            //    Portfolio.Account.Deposit(AllocationPerInstrument, CurrencyId.USD, "Initial allocation");
            //}
            //else
            //{
            //    Portfolio.Account.Deposit(AllocationPerInstrument - 1000, CurrencyId.USD, "Initial allocation");
            //}

			// Set up indicators.
			fastSMA = new SMA(Bars, FastSMALength);
			slowSMA = new SMA(Bars, SlowSMALength);

			AddGroups();
		}

		protected override void OnStrategyStart()
		{
            if (SuspendTrading)
            {
                Portfolio.Account.Deposit(AllocationPerInstrument, CurrencyId.USD, "Initial allocation");
            }
            else
            {
                Portfolio.Account.Deposit(AllocationPerInstrument - 1000, CurrencyId.USD, "Initial allocation");
            }

			Console.WriteLine("Starting strategy in {0} mode.", Mode);
		}

		protected override void OnBar(Instrument instrument, Bar bar)
		{
            if (!SuspendTrading)
            {
                //Console.WriteLine(bar);
                Console.WriteLine(Bars.Count);
            }
			// Add bar to bar series.
			Bars.Add(bar);

			Log(bar, barsGroup);

			if (fastSMA.Count > 0)
				Log(fastSMA.Last, fastSmaGroup);

			if (slowSMA.Count > 0)
				Log(slowSMA.Last, slowSmaGroup);

			// Calculate performance.
			Portfolio.Performance.Update();

			Log(Portfolio.Value, equityGroup);

			if (!SuspendTrading)
			{
				// Check strategy logic.
				if (fastSMA.Count > 0 && slowSMA.Count > 0)
				{
					Cross cross = fastSMA.Crosses(slowSMA, bar.DateTime);

					if (!HasPosition(instrument))
					{
						// Enter long/short.
						if (cross == Cross.Above)
						{
							Order enterOrder = BuyOrder(Instrument, Qty, "Enter Long");
							Send(enterOrder);
						}
						else if (cross == Cross.Below)
						{
							Order enterOrder = SellOrder(Instrument, Qty, "Enter Short");
							Send(enterOrder);
						}
					}
					else
					{
						// Reverse to long/short.
                        if (Position.Side == SmartQuant.PositionSide.Long && cross == Cross.Below)
						{
							Order reverseOrder = SellOrder(Instrument, Math.Abs(Position.Amount) + Qty, "Reverse to Short");
							Send(reverseOrder);
						}
                        else if (Position.Side == SmartQuant.PositionSide.Short && cross == Cross.Above)
						{
							Order reverseOrder = BuyOrder(Instrument, Math.Abs(Position.Amount) + Qty, "Reverse to Long");
							Send(reverseOrder);
						}
					}
				}
			}
		}

		protected override void OnFill(Fill fill)
		{
			// Add fill to group.
			Log(fill, fillGroup);
		}

		private void AddGroups()
		{
			// Create bars group.
			barsGroup = new Group("Bars");
			barsGroup.Add("Pad", DataObjectType.String, 0);
			barsGroup.Add("SelectorKey", Instrument.Symbol);

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
			GroupManager.Add(barsGroup);
			GroupManager.Add(fillGroup);
			GroupManager.Add(equityGroup);
			GroupManager.Add(fastSmaGroup);
			GroupManager.Add(slowSmaGroup);
		}
	}
}
