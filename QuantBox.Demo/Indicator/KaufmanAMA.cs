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
        // Fields
        protected int n, p, q;
        protected KaufmanAMA.ER er;
        protected BarData barData;

        // Methods
        public KaufmanAMA(ISeries input, int n, int p, int q, BarData barData = BarData.Close)
            : base(input)
        {
            this.n = n;
            this.p = p;
            this.q = q;
            this.barData = barData;
            this.Init();
        }

        public override void Calculate(int index)
        {
            int j = index - n;
            if (j < 0)
            {
                return;
            }

            double FSC = 2.0 / (1.0 + p); // {快速平滑常数}
            double SSC = 2.0 / (1.0 + q); // {慢速平滑常数}
            double SC = er[j] * (FSC - SSC) + SSC; //{等价于SC=ER*FSC+(1-ER)*SSC,指数平滑序列}
            double SCSQ = SC * SC;

            double db = input[index, barData];

            if (j == 0)
            {
            }
            else
            {
                db = SCSQ * db + (1 - SCSQ) * this[j - 1];
            }

            base.Add(base.input.GetDateTime(index), db);
        }

        protected override void Init()
        {
            base.name = "KaufmanAMA(" + this.n + ", " + this.p + ", " + this.q + ")";
            base.description = "KaufmanAMA";
            base.Clear();
            base.calculate = true;
            base.Detach();
            if (this.er != null)
            {
                this.er.Detach();
            }
            this.er = new ER(base.input, this.n, barData);
            base.Attach();
        }

        public static double Value(ISeries input, int index, int n, int p, int q,BarData barData = BarData.Close)
        {
            int j = index - n;
            if (j < 0)
            {
                return double.NaN;
            }

            double FSC = 2.0 / (1.0 + p); // {快速平滑常数}
            double SSC = 2.0 / (1.0 + q); // {慢速平滑常数}
            double SC = KaufmanAMA.ER.Value(input, index, n) * (FSC - SSC) + SSC; //{等价于SC=ER*FSC+(1-ER)*SSC,指数平滑序列}
            double SCSQ = SC * SC;

            double db = input[index, barData];

            if (j == 0)
            {
            }
            else
            {
                db = SCSQ * db + (1 - SCSQ) * KaufmanAMA.Value(input, index - 1, n, p, q);
            }

            return db;
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

        [Description(""), Category("Parameters")]
        public int P
        {
            get
            {
                return this.p;
            }
            set
            {
                this.p = value;
                this.Init();
            }
        }

        [Description(""), Category("Parameters")]
        public int Q
        {
            get
            {
                return this.q;
            }
            set
            {
                this.q = value;
                this.Init();
            }
        }

        public KaufmanAMA.ER ERSeries
        {
            get { return er; }
        }
    }
}