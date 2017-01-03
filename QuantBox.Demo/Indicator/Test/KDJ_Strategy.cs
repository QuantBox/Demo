using SmartQuant;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Indicator.Test
{
    public class KDJ_Strategy : InstrumentStrategy
    {
        KDJ kdj;

        Group barsGroup;

        Group kGroup;
        Group dGroup;
        Group jGroup;

        Group k2Group;
        Group d2Group;
        Group j2Group;

        public KDJ_Strategy(Framework framework, string name)
            : base(framework, name)
        {
        }

        protected override void OnStrategyStart()
        {
            kdj = new KDJ(Bars, 10);

            AddGroups();
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            Bars.Add(bar);

            Log(bar, barsGroup);

            if (kdj.Count == 0)
            {
                return;
            }

            
            Log(kdj.KSeries.Last, kGroup);
            Log(kdj.DSeries.Last, dGroup);
            Log(kdj.Last, jGroup);
            
            Log(KDJ.K.Value(Bars, Bars.Count - 1, 10), k2Group);
            Log(KDJ.D.Value(Bars, Bars.Count - 1, 10), d2Group);
            Log(KDJ.Value(Bars, Bars.Count - 1, 10), j2Group);

            Console.WriteLine("{0}, {1}", kdj.LastDateTime, kdj.KSeries.Last);
        }

        private void AddGroups()
        {
            barsGroup = new Group("Bars");
            barsGroup.Add("Pad", DataObjectType.String, 0);
            barsGroup.Add("SelectorKey", Instrument.Symbol);

            kGroup = new Group("K");
            kGroup.Add("Pad", 1);
            kGroup.Add("SelectorKey", Instrument.Symbol);
            kGroup.Add("Color", Color.White);

            k2Group = new Group("K2");
            k2Group.Add("Pad", 1);
            k2Group.Add("SelectorKey", Instrument.Symbol);
            k2Group.Add("Color", Color.Red);

            dGroup = new Group("D");
            dGroup.Add("Pad", 2);
            dGroup.Add("SelectorKey", Instrument.Symbol);
            dGroup.Add("Color", Color.Yellow);

            d2Group = new Group("D2");
            d2Group.Add("Pad", 2);
            d2Group.Add("SelectorKey", Instrument.Symbol);
            d2Group.Add("Color", Color.Yellow);

            jGroup = new Group("J");
            jGroup.Add("Pad", 3);
            jGroup.Add("SelectorKey", Instrument.Symbol);
            jGroup.Add("Color", Color.Yellow);

            j2Group = new Group("J2");
            j2Group.Add("Pad", 3);
            j2Group.Add("SelectorKey", Instrument.Symbol);
            j2Group.Add("Color", Color.Red);

            GroupManager.Add(barsGroup);
            GroupManager.Add(kGroup);
            GroupManager.Add(dGroup);
            GroupManager.Add(jGroup);

            GroupManager.Add(k2Group);
            GroupManager.Add(d2Group);
            GroupManager.Add(j2Group);
        }
    }
}
