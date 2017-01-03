using QuantBox.Demo.Strategy;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Scenario
{
    public class BacktestLoadOnStart : SmartQuant.Scenario
    {
        private long barSize = 30;

        public BacktestLoadOnStart(Framework framework)
			: base(framework)
		{
		}

		public override void Run()
		{
			// Prepare running.
			Console.WriteLine("Prepare running in {0} mode...", framework.StrategyManager.Mode);

			// Get trading instruments.
			Instrument ins1 = InstrumentManager.Instruments["AAPL"];

			// Create SMA Crossover with Loading data on start strategy.
			// and add trading instruments.
            SMACrossoverLoadOnStart_Strategy smaCrossoverLOS = new SMACrossoverLoadOnStart_Strategy(framework, "SMACrossoverLOS");
			smaCrossoverLOS.Instruments.Add(ins1);

			// Set strategy as main.
			strategy = smaCrossoverLOS;

			Console.WriteLine("Prepare running in {0} mode...", framework.StrategyManager.Mode);

			// Set DataSimulator's dates.
			DataSimulator.DateTime1 = new DateTime(2013, 01, 01);
			DataSimulator.DateTime2 = new DateTime(2013, 12, 18);

			// Set property for suspend trading during simulation.
            SMACrossoverLoadOnStart_Strategy.SuspendTrading = true;

			// Add 5 minute bars (300 seconds) for trading instruments.
            BarFactory.Add(ins1, SmartQuant.BarType.Time, barSize);

			StartStrategy(StrategyMode.Backtest);

            DataSimulator.DateTime1 = new DateTime(2013, 12, 18);
			DataSimulator.DateTime2 = new DateTime(2013, 12, 31);


			// Set property for trading.
            SMACrossoverLoadOnStart_Strategy.SuspendTrading = false;

			// Run.
			Console.WriteLine("Run in {0} mode.", framework.StrategyManager.Mode);


            StartStrategy(StrategyMode.Backtest);
			
			return;
		}

    }
}
