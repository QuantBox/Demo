using QuantBox.Demo.Indicator.Test;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Scenario
{
    public class IndicatorTest : SmartQuant.Scenario
    {
        // bar size in seconds
        private long barSize = 60;

        public IndicatorTest(Framework framework)
            : base(framework)
        {

        }

        public override void Run()
        {
            //Instrument instrument1 = InstrumentManager.Instruments["AAPL"];
            //Instrument instrument2 = InstrumentManager.Instruments["MSFT"];
            //Instrument instrument3 = InstrumentManager.Instruments["IF1412_TB"];
            Instrument instrument4 = InstrumentManager.Instruments["IF999"];

            // Create SMA Crossover strategy
            strategy = new BIAS_Strategy(framework, "BIAS");
            //strategy = new KaufmanAMA_Strategy(framework, "KaufmanAMA");
            //strategy = new KDJ_Strategy(framework, "KDJ");
            //strategy = new LookBackDays_Strategy(framework, "LookBackDays");
            //strategy = new DPO1_Strategy(framework, "DPO1");
            //strategy = new PC_Strategy(framework, "PC");

            // Add instruments
            //strategy.AddInstrument(instrument1);
            //strategy.AddInstrument(instrument2);
            //strategy.AddInstrument(instrument3);
            strategy.AddInstrument(instrument4);

            // Set simulation interval
            //DataSimulator.DateTime1 = new DateTime(2013, 01, 01);
            //DataSimulator.DateTime1 = new DateTime(2014, 11, 27);
            DataSimulator.DateTime1 = new DateTime(2015, 04, 16);
            //DataSimulator.DateTime2 = new DateTime(2013, 12, 16, 9, 36, 0);
            //DataSimulator.DateTime2 = new DateTime(2013, 12, 31);
            //DataSimulator.DateTime2 = new DateTime(2014, 11, 28);
            DataSimulator.DateTime2 = new DateTime(2016, 11, 02);

            // Add 1 minute bars
            //BarFactory.Add(instrument1, BarType.Time, barSize);
            //BarFactory.Add(instrument2, BarType.Time, barSize);
            //BarFactory.Add(instrument3, BarType.Time, barSize);
            BarFactory.Add(instrument4, SmartQuant.BarType.Time, barSize);
            
            // Run the strategy
            StartStrategy();
        }
    }
}


