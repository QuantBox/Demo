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
    public class TVWindow : SmartQuant.Indicator
    {
        // Fields
        protected double initialLength;
        protected int floorAmt;
        protected int ceilingAmt;

        // Methods
        public TVWindow(ISeries input, int initialLength, int floorAmt, int ceilingAmt)
            : base(input)
        {
            this.initialLength = initialLength;
            this.floorAmt = floorAmt;
            this.ceilingAmt = ceilingAmt;
            this.Init();
        }

        public override void Calculate(int index)
        {
            double d = Value(base.input, index, this.initialLength, this.floorAmt, this.ceilingAmt);

            if (!double.IsNaN(d))
            {
                base.Add(base.input.GetDateTime(index), d);
            }
        }

        protected override void Init()
        {
            base.name = "TVWindow";
            base.description = "Time-variant Window";
            base.Clear();
            base.calculate = true;
        }

        public static double Value(ISeries input, int index, double length, int floorAmt, int ceilingAmt)
        {
            if (index - 1 < 0)
            {
                return double.NaN;
            }

            double T1 = input[index];
            double T0 = input[index - 1];
            double deltaT = (T1 - T0) / T1;

            double num = length * (1.0 + deltaT);
            num = Math.Min(num, ceilingAmt);
            num = Math.Max(num, floorAmt);

            return Math.Round(num, 0);
        }

        // Properties
        [Description(""), Category("Parameters")]
        public double InitialLength
        {
            get
            {
                return this.initialLength;
            }
            set
            {
                this.initialLength = value;
                this.Init();
            }
        }

        [Description(""), Category("Parameters")]
        public int CeilingAmt
        {
            get
            {
                return this.ceilingAmt;
            }
            set
            {
                this.ceilingAmt = value;
                this.Init();
            }
        }

        [Description(""), Category("Parameters")]
        public int FloorAmt
        {
            get
            {
                return this.floorAmt;
            }
            set
            {
                this.floorAmt = value;
                this.Init();
            }
        }
    }
}
