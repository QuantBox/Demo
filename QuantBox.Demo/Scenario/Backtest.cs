using QuantBox.Demo.Data;
using QuantBox.Demo.Strategy;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Scenario
{
    public class Backtest : SmartQuant.Scenario
    {

        // bar size in seconds
        private long barSize60 = 60;
        private long barSize86400 = 86400;

        public Backtest(Framework framework)
            : base(framework)
        {
        }

        public override void Run()
        {
            Instrument instrument1 = InstrumentManager.Instruments["AAPL"];
            //Instrument instrument2 = InstrumentManager.Instruments["MSFT"];
            //Instrument instrument3 = InstrumentManager.Instruments["CSCO_2001"];
            //Instrument instrument4 = InstrumentManager.Instruments["IF1409"];
            //Instrument instrument5 = InstrumentManager.Instruments["IF999"];

            // Create SMA Crossover strategy
            strategy = new SMACrossoverLoadOnStart_Strategy(framework, "SMACrossover");
            //strategy = new DynamicBreakOut2(framework, "DynamicBreakOut2");
            //strategy = new DualThrust_RangeBreak_Strategy(framework, "DualThrust_RangeBreak_Strategy");

            // Add instruments
            strategy.AddInstrument(instrument1);
            //strategy.AddInstrument(instrument2);
            //strategy.AddInstrument(instrument3);
            //strategy.AddInstrument(instrument4);
            //strategy.AddInstrument(instrument5);

            PbTickDataImport ptdi = new PbTickDataImport();

            ptdi.ReadFile(instrument1.Id, @"D:\1.data");

            DataSimulator.Series.Add(ptdi.Trades);

            DataSimulator.SubscribeAll = false;

            ExecutionSimulator.FillOnBar = true;

            // Set simulation interval
            //DataSimulator.DateTime1 = new DateTime(2015, 04, 16);
            //DataSimulator.DateTime1 = new DateTime(2001, 01, 01);
            //DataSimulator.DateTime1 = new DateTime(2014, 06, 01);
            DataSimulator.DateTime1 = new DateTime(2013, 12, 16, 9, 30, 01);
            DataSimulator.DateTime2 = new DateTime(2013, 12, 20, 16, 00, 0);
            //DataSimulator.DateTime2 = new DateTime(2013, 12, 31);
            //DataSimulator.DateTime2 = new DateTime(2001, 12, 31);
            //DataSimulator.DateTime2 = new DateTime(2016, 11, 02);

            // Add 1 minute bars
            BarFactory.Add(instrument1, BarType.Time, barSize60);
            BarFactory.Add(instrument1, BarType.Time, barSize86400);
            //BarFactory.Add(instrument2, BarType.Time, barSize);
            //BarFactory.Add(instrument3, BarType.Time, barSize);
            //BarFactory.Add(instrument4, BarType.Time, barSize60);
            //BarFactory.Add(instrument4, BarType.Time, barSize86400);
            //BarFactory.Add(instrument5, SmartQuant.BarType.Time, barSize60);
            //BarFactory.Add(instrument5, SmartQuant.BarType.Time, barSize86400);

            // Run the strategy
            StartStrategy();
        }
    }
}



