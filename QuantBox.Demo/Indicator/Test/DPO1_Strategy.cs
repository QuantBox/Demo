using SmartQuant;
using SmartQuant.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace QuantBox.Demo.Indicator.Test
{
    public class DPO1_Strategy : InstrumentStrategy
    {
        DPO1 dpo1;
        SMA sma;

        int length = 14;

        Group barsGroup;
        Group dpo1Group;
        Group dpo1ValueGroup;

        public DPO1_Strategy(Framework framework, string name)
            : base(framework, name)
        {
        }

        protected override void OnStrategyStart()
        {
            dpo1 = new DPO1(Bars, length, BarData.Close);
            sma = new SMA(Bars, length, BarData.Close);

            AddGroups();
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            Bars.Add(bar);

            Log(bar, barsGroup);

            if (dpo1.Count == 0)
            {
                return;
            }

            if (sma.Count < length)
            {
                return;
            }

            Log(dpo1.Last, dpo1Group);
            Log(DPO1.Value(Bars, Bars.Count - 1, length, BarData.Close), dpo1ValueGroup);
            
            Console.WriteLine("{0}, {1}", dpo1.LastDateTime, dpo1.Last);
        }

        private void AddGroups()
        {
            barsGroup = new Group("Bars");
            barsGroup.Add("Pad", DataObjectType.String, 0);
            barsGroup.Add("SelectorKey", Instrument.Symbol);

            dpo1Group = new Group("DPO1");
            dpo1Group.Add("Pad", 1);
            dpo1Group.Add("SelectorKey", Instrument.Symbol);
            dpo1Group.Add("Color", Color.Red);

            dpo1ValueGroup = new Group("DPO1.Value");
            dpo1ValueGroup.Add("Pad", 1);
            dpo1ValueGroup.Add("SelectorKey", Instrument.Symbol);
            dpo1ValueGroup.Add("Color", Color.Blue);

            GroupManager.Add(barsGroup);
            GroupManager.Add(dpo1Group);
            GroupManager.Add(dpo1ValueGroup);
        }
    }
}
