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
    public partial class KDJ : SmartQuant.Indicator
    {
        public class D : SmartQuant.Indicator
        {
            // Fields
            protected int length;
            protected KDJ.K kdj_k;

            // Methods
            public D(ISeries input, int length)
                : base(input)
            {
                this.length = length;
                this.Init();
            }

            public override void Calculate(int index)
            {
                int i = index - length + 1;
                if (i < 0)
                    return;

                double num = 50;
                if (i == 0)
                {
                    num = (2 * num + kdj_k[i]) / 3.0;
                }
                else
                {
                    num = (2 * this[i - 1] + kdj_k[i]) / 3.0;
                }

                base.Add(base.input.GetDateTime(index), num);
            }

            protected override void Init()
            {
                base.name = "D";
                base.description = "KDJ.D";
                base.Clear();
                base.calculate = true;
                base.Detach();
                if (this.kdj_k != null)
                {
                    this.kdj_k.Detach();
                }
                this.kdj_k = new KDJ.K(base.input, this.length);
                base.Attach();
            }

            public static double Value(ISeries input, int index, int length)
            {
                if (index < (length - 1))
                {
                    return double.NaN;
                }

                double num = 50.0;
                for (int i = length - 1; i <= index; i++)
                {
                    num = (2 * num + KDJ.K.Value(input, i, length)) / 3.0;
                }
                return num;
            }

            // Properties
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

            public KDJ.K KSeries
            {
                get { return kdj_k; }
            }
        }
    }
}
