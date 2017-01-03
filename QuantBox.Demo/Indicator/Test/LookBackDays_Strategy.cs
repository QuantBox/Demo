using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using SmartQuant;
using SmartQuant.Indicators;

namespace QuantBox.Demo.Indicator.Test
{
    public class LookBackDays_Strategy : InstrumentStrategy
    {
        SMD smd;
        LookBackDays lookBackDays;
        Group barsGroup;        
        Group lookBackDaysGroup;
        Group smdGroup;
        Group lookBackDaysGroup2;
               
        public LookBackDays_Strategy(Framework framework, string name)
            : base(framework, name)
        {
        }
        
        protected override void OnStrategyStart()
        {
            smd = new SMD(Bars, 30);
            lookBackDays = new LookBackDays(smd, 20, 20, 60);
            AddGroups();
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            Bars.Add(bar);

            if (lookBackDays.Count == 0)
            {
                return;
            }
                        
            int length = (int)lookBackDays.Last;
            double a = SMA.Value(Bars, Bars.Count - 1, length, BarData.Close);

            Log(bar, barsGroup);
            Log(smd.Last, smdGroup);
            Log(lookBackDays.Last, lookBackDaysGroup);
            Log(LookBackDays.Value(smd, smd.Count - 1, 20, 20, 60), lookBackDaysGroup2);

            Console.WriteLine("{0}, {1}", smd.LastDateTime, smd.Last);
            Console.WriteLine("{0}, {1}", lookBackDays.LastDateTime, lookBackDays.Last);
        }

        private void AddGroups()
        {
            barsGroup = new Group("Bars");
            barsGroup.Add("Pad", 0);
            barsGroup.Add("SelectorKey", Instrument.Symbol);
            barsGroup.Add("Color", Color.Black);

            smdGroup = new Group("SMD");
            smdGroup.Add("Pad", 1);
            smdGroup.Add("SelectorKey", Instrument.Symbol);
            smdGroup.Add("Color", Color.Green);

            lookBackDaysGroup = new Group("TVWindow");
            lookBackDaysGroup.Add("Pad", 2);
            lookBackDaysGroup.Add("SelectorKey", Instrument.Symbol);
            lookBackDaysGroup.Add("Color", Color.Blue);

            lookBackDaysGroup2 = new Group("TVWindow");
            lookBackDaysGroup2.Add("Pad", 2);
            lookBackDaysGroup2.Add("SelectorKey", Instrument.Symbol);
            lookBackDaysGroup2.Add("Color", Color.Brown);

            GroupManager.Add(barsGroup);
            GroupManager.Add(smdGroup);
            GroupManager.Add(lookBackDaysGroup);
            GroupManager.Add(lookBackDaysGroup2);
        }
    }
}
