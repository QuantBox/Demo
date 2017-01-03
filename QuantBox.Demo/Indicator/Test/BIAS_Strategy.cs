using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace QuantBox.Demo.Indicator.Test
{
    public class BIAS_Strategy : InstrumentStrategy
    {
        Group barsGroup;
        Group biasGroup;
        Group bias2Group;
        BIAS bias;

        public BIAS_Strategy(Framework framework, string name)
			: base(framework, name)
		{
		}

        protected override void OnStrategyStart()
        {
            bias = new BIAS(Bars, 10, BarData.Close);
            AddGroups();
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            Bars.Add(bar);

            Log(bar, barsGroup);


            if (bias.Count == 0)
                return;
            
            Log(bias.Last, biasGroup);
            Log(BIAS.Value(Bars,Bars.Count - 1,10,BarData.Close), bias2Group);

            Console.WriteLine("{0}, {1}", bias.LastDateTime, bias.Last);
        }

        private void AddGroups()
        {
            barsGroup = new Group("Bars");
            barsGroup.Add("Pad", DataObjectType.String, 0);
            barsGroup.Add("SelectorKey", Instrument.Symbol);

            biasGroup = new Group("BIAS");
            biasGroup.Add("Pad", 1);
            biasGroup.Add("SelectorKey", Instrument.Symbol);
            biasGroup.Add("Color", Color.Red);

            bias2Group = new Group("BIAS2");
            bias2Group.Add("Pad", 2);
            bias2Group.Add("SelectorKey", Instrument.Symbol);
            bias2Group.Add("Color", Color.White);

            GroupManager.Add(barsGroup);
            GroupManager.Add(biasGroup);
            GroupManager.Add(bias2Group);
        }
    }
}
