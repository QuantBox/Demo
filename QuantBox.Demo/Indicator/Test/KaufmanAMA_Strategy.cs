using SmartQuant;
using SmartQuant.Indicators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Indicator.Test
{
    public class KaufmanAMA_Strategy : InstrumentStrategy
    {
        KaufmanAMA ama;
        SMA sma;
        int N = 8;
        int SL = 2;
        int FS = 20;

        Group barsGroup;
        Group amaGroup;
        Group ama2Group;
        Group erGroup;
        Group smaGroup;

        public KaufmanAMA_Strategy(Framework framework, string name)
            : base(framework, name)
        {
        }

        protected override void OnStrategyStart()
        {
            ama = new KaufmanAMA(Bars, N, SL, FS);
            sma = new SMA(Bars, N);
            AddGroups();
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            Bars.Add(bar);

            Log(bar, barsGroup);

            if (ama.Count == 0)
            {
                return;
            }

            Log(ama.Last, amaGroup);
            Log(ama.ERSeries.Last, erGroup);
            Log(sma.Last, smaGroup);

            Log(KaufmanAMA.Value(Bars, Bars.Count - 1, N, SL, FS), ama2Group);
            
            Console.WriteLine("{0}, {1}", ama.LastDateTime, ama.Last);
            Console.WriteLine("{0}, {1}", sma.LastDateTime, sma.Last);
        }

        private void AddGroups()
        {
            barsGroup = new Group("Bars");
            barsGroup.Add("Pad", DataObjectType.String, 0);
            barsGroup.Add("SelectorKey", Instrument.Symbol);

            amaGroup = new Group("AMA");
            amaGroup.Add("Pad", 0);
            amaGroup.Add("SelectorKey", Instrument.Symbol);
            amaGroup.Add("Color", Color.White);

            ama2Group = new Group("AMA");
            ama2Group.Add("Pad", 0);
            ama2Group.Add("SelectorKey", Instrument.Symbol);
            ama2Group.Add("Color", Color.Red);

            erGroup = new Group("ER");
            erGroup.Add("Pad", 1);
            erGroup.Add("SelectorKey", Instrument.Symbol);
            erGroup.Add("Color", Color.Yellow);

            smaGroup = new Group("SMA");
            smaGroup.Add("Pad", 0);
            smaGroup.Add("SelectorKey", Instrument.Symbol);
            smaGroup.Add("Color", Color.Green);

            GroupManager.Add(barsGroup);
            GroupManager.Add(amaGroup);
            GroupManager.Add(ama2Group);
            GroupManager.Add(erGroup);
            GroupManager.Add(smaGroup);
        }
    }
}
