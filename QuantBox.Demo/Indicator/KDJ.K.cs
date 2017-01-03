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
        public class K : SmartQuant.Indicator
        {
            // Fields
            protected int length;

            protected K_Fast k_fast;

            // Methods
            public K(ISeries input, int length)
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
                    num = (2 * num + K_Fast.Value(input, index, length)) / 3.0;
                }
                else
                {
                    num = (2 * this[i - 1] + K_Fast.Value(input, index, length)) / 3.0;
                }

                base.Add(base.input.GetDateTime(index), num);
            }

            protected override void Init()
            {
                base.name = "K";
                base.description = "KDJ.K";
                base.Clear();
                base.calculate = true;
                base.Detach();
                if(this.k_fast != null)
                {
                    this.k_fast.Detach();
                }
                this.k_fast = new K_Fast(base.input, this.length);
                base.Attach();
            }

            public static double Value(ISeries input, int index, int length)
            {
                int i = index - length + 1;
                if (i < 0)
                    return double.NaN;

                double num = 50;
                for (int j = length - 1; j <= index; ++j)
                {
                    num = (2 * num + K_Fast.Value(input, j, length)) / 3.0;
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
        }
    }
}
