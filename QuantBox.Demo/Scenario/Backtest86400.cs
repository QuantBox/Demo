using QuantBox.Demo.Statistics;
using QuantBox.Demo.Strategy;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Scenario
{
    public class Backtest86400 : SmartQuant.Scenario
    {
        // bar size in seconds
        private long barSize = 60;
        private long MaxBarSize = 86400;
        //private long MaxBarSize = 120;

        public Backtest86400(Framework framework)
            : base(framework)
        {

        }

        public override void Run()
        {
            StatisticsManager.Add(new DailyNumOfLossTrades());
            StatisticsManager.Add(new DailyNumOfWinTrades());
            StatisticsManager.Add(new DailyConsecutiveLossTrades());

            Instrument instrument1 = InstrumentManager.Instruments["IF999"];
            Instrument instrument2 = InstrumentManager.Instruments["IC999"];

            // Create SMA Crossover strategy
            //DualThrust_RangeBreak_Strategy strategy = new DualThrust_RangeBreak_Strategy(framework, "DualThrust & RangeBreak");
            //RBreaker_Strategy strategy = new RBreaker_Strategy(framework, "RBreaker");
            //DynamicBreakOut2 strategy = new DynamicBreakOut2(framework, "DynamicBreakOut2");
            DoubleMA_Crossover strategy = new DoubleMA_Crossover(framework, "DoubleMA Crossover");
            
            strategy.MaxBarSize = MaxBarSize;
            
            // Add instruments
            strategy.AddInstrument(instrument1);
            strategy.AddInstrument(instrument2);

            // Set simulation interval
            DataSimulator.DateTime1 = new DateTime(2015, 04, 16);
            //DataSimulator.DateTime2 = new DateTime(2013, 12, 16, 9, 36, 0);
            DataSimulator.DateTime2 = new DateTime(2016, 11, 02);

            // Add 1 minute bars
            BarFactory.Add(instrument1, SmartQuant.BarType.Time, barSize);
            BarFactory.Add(instrument2, SmartQuant.BarType.Time, barSize);

            BarFactory.Add(instrument1, SmartQuant.BarType.Time, MaxBarSize);
            BarFactory.Add(instrument2, SmartQuant.BarType.Time, MaxBarSize);

            this.strategy = strategy;

            // Run the strategy
            StartStrategy();
        }
    }
}


