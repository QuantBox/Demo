using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using SmartQuant;
using SmartQuant.Indicators;

namespace QuantBox.Demo.Indicator
{
    /// <summary>
    /// 乖离率
    /// </summary>
    public class BIAS : SmartQuant.Indicator
    {
        // Fields
        protected SMA sma;
        protected int length;
        protected BarData barData;

        // Methods
        public BIAS(ISeries input, int length, BarData barData = BarData.Close)
            : base(input)
        {
            this.length = length;
            this.barData = barData;
            this.Init();
        }

        public override void Calculate(int index)
        {
            int i = index - length + 1;
            if (i < 0)
                return;

            double num1 = input[index, barData];
            double num2 = this.sma[i];
            base.Add(base.input.GetDateTime(index), (num1 - num2) * 100.0 / num2);
        }

        protected override void Init()
        {
            if (base.input is BarSeries)
            {
                base.name = string.Concat(new object[] { "BIAS(", this.length, ",", this.barData, ")" });
            }
            else
            {
                base.name = "BIAS(" + this.length + ")";
            }

            base.description = "Bias Ratio";
            base.Clear();
            base.calculate = true;
            base.Detach();
            if(this.sma != null)
            {
                this.sma.Detach();
            }
            this.sma = new SMA(base.input, this.length, barData);
            base.Attach();
        }

        public static double Value(ISeries input, int index, int length, BarData barData = BarData.Close)
        {
            if (index >= (length - 1))
            {
                double num1 = input[index, barData];
                double num2 = SMA.Value(input, index, length, barData);
                return (num1 - num2) * 100.0 / num2;
            }
                       
            return double.NaN;
        }

        // Properties
        [Description(""), Category("Parameters")]
        public BarData BarData
        {
            get
            {
                return this.barData;
            }
            set
            {
                this.barData = value;
                this.Init();
            }
        }

        [Description(""), Category("Parameters")]
        public int Length
        {
            get
            {
                return this.length;
            }
            set
            {
                this.length = value;
                this.Init();
            }
        }
    }
}
