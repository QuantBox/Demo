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
    public partial class KaufmanAMA : SmartQuant.Indicator
    {
        public class ER : SmartQuant.Indicator
        {
            // Fields
            protected int n;
            protected BarData barData;

            // Methods
            public ER(ISeries input, int n, BarData barData = BarData.Close)
                : base(input)
            {
                this.n = n;
                this.barData = barData;
                this.Init();
            }

            public override void Calculate(int index)
            {
                double d = Value(base.input, index, n);

                if (!double.IsNaN(d))
                {
                    base.Add(base.input.GetDateTime(index), d);
                }
            }

            protected override void Init()
            {
                base.name = "ER";
                base.description = "KaufmanAMA.ER";
                base.Clear();
                base.calculate = true;
            }

            public static double Value(ISeries input, int index, int n,BarData barData = BarData.Close)
            {
                int j = index - n;

                if (j < 0)
                {
                    return double.NaN;
                }

                double DIRECTION = Math.Abs(input[index, barData] - input[j, barData]);

                double VOLATILITY = 0;
                for (int i = j; i < index; ++i)
                {
                    VOLATILITY += Math.Abs(input[i + 1, barData] - input[i, barData]);
                }

                double _ER = DIRECTION / VOLATILITY; //{EFFICIENCY RATIO是AMA系统中最重要的指标，比值越大，趋势越明显}
                if (VOLATILITY == 0)
                {
                    _ER = 1;
                }

                return _ER;
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
            public int N
            {
                get
                {
                    return this.n;
                }
                set
                {
                    this.n = value;
                    this.Init();
                }
            }
        }
    }
}