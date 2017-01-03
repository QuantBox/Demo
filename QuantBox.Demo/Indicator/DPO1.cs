using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartQuant;
using SmartQuant.Indicators;
using System.ComponentModel;

namespace QuantBox.Demo.Indicator
{
    /// <summary>
    /// 修正DPO指标
    /// </summary>
    public class DPO1 : SmartQuant.Indicator
    {
        // Fields
        protected BarData barData;
        protected int length;

        // Methods
        public DPO1(ISeries input, int length, BarData barData = BarData.Close)
            : base(input)
        {
            this.length = length;
            this.barData = barData;
            this.Init();
        }
        
        public override void Calculate(int index)
        {
            double d = Value(base.input, index, this.length, this.barData);

            if (!double.IsNaN(d))
            {
                base.Add(base.input.GetDateTime(index), d);
            }
        }

        protected override void Init()
        {
            if (base.input is BarSeries)
            {
                base.name = string.Concat(new object[] { "DPO (", this.length, ",", this.barData, ")" });
            }
            else
            {
                base.name = "DPO (" + this.length + ")";
            }

            base.description = "Detrended Price Oscillator";
            base.Clear();
            base.calculate = true;
        }

        public static double Value(ISeries input, int index, int length, BarData barData = BarData.Close)
        {
            if (index <= length / 2 + length - 1)
            {
                return double.NaN;
            }

            double num1 = input[index, barData];
            double num2 = 0.0;

            //for(int i = index - length / 2; i >index - length - length / 2; i--)
            for (int i = index - length / 2 - 1; i >= index - length - length / 2; i--)
            {
                num2 += input[i, barData];
            }

            num2 /= (double)length;

            return num1 - num2;
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
