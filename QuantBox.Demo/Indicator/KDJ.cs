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
        // Fields
        protected int length;
        protected KDJ.K kdj_k;
        protected KDJ.D kdj_d;
        
        // Methods
        public KDJ(ISeries input, int length)
            : base(input)
        {
            this.length = length;
            this.Init();
        }

        public override void Calculate(int index)
        {
            int i = index - length + 1;
            if (i<0)
            {
                return;
            }

            double d = 3.0 * kdj_k[i] - 2.0 * kdj_d[i];
            base.Add(base.input.GetDateTime(index), d);
        }

        protected override void Init()
        {
            base.name = "KDJ(" + length + ")";
            base.description = "Stochastic Oscillator";
            base.Clear();
            base.calculate = true;
            base.Detach();
            if (this.kdj_k != null)
            {
                this.kdj_k.Detach();
            }
            if (this.kdj_d != null)
            {
                this.kdj_d.Detach();
            }
            this.kdj_k = new KDJ.K(base.input, this.length);
            this.kdj_d = new KDJ.D(base.input, this.length);
            base.Attach();
        }

        public static double Value(ISeries input, int index, int length)
        {
            if (index < length - 1)
            {
                return double.NaN;
            }

            double num1 = KDJ.K.Value(input, index, length);
            double num2 = KDJ.D.Value(input, index, length);
            return 3.0 * num1 - 2.0 * num2;
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

        public KDJ.D DSeries
        {
            get { return kdj_d; }
        }
    }
}
