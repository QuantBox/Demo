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
    public class PriceChannel_Strategy : InstrumentStrategy
    {
        PriceChannel pc;

        Group pcGroup;
        Group pcValueGroup;
        Group barsGroup;

        int length = 10;
        PriceChannel.CalcType calcType = PriceChannel.CalcType.Max;
        PriceChannel.IncludeLast useLast = PriceChannel.IncludeLast.No;
        BarData barData = BarData.Close;

        public PriceChannel_Strategy(Framework framework, string name)
            : base(framework, name)
        {
        }

        protected override void OnStrategyStart()
        {
            pc = new PriceChannel(Bars, length, calcType, useLast, barData);

            AddGroups();
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            Bars.Add(bar);

            Log(bar, barsGroup);

            if (pc.Count == 0)
            {
                return;
            }

            Log(pc.Last, pcGroup);
            Log(PriceChannel.Value(Bars, Bars.Count - 1, length, calcType, useLast, barData), pcValueGroup);

            Console.WriteLine("{0}, {1}", pc.LastDateTime, pc.Last);
        }

        private void AddGroups()
        {
            barsGroup = new Group("Bars");
            barsGroup.Add("Pad", DataObjectType.String, 0);
            barsGroup.Add("SelectorKey", Instrument.Symbol);

            pcGroup = new Group("PC");
            pcGroup.Add("Pad", 1);
            pcGroup.Add("SelectorKey", Instrument.Symbol);
            pcGroup.Add("Color", Color.Red);

            pcValueGroup = new Group("PC.Value");
            pcValueGroup.Add("Pad", 1);
            pcValueGroup.Add("SelectorKey", Instrument.Symbol);
            pcValueGroup.Add("Color", Color.Blue);

            GroupManager.Add(barsGroup);
            GroupManager.Add(pcGroup);
            GroupManager.Add(pcValueGroup);
        }
    }
}
