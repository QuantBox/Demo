using SmartQuant;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Indicator
{
    public class PriceChannel:SmartQuant.Indicator
    {
        public enum CalcType
        {
            Max,
            Min,
        }

        public enum IncludeLast
        {
            Yes,
            No,
        }

        protected BarData barData;
        protected int length;
        protected CalcType calcType;
        protected IncludeLast includeLast;

        public PriceChannel(ISeries input, int length,CalcType calcType,IncludeLast includeLast, BarData barData = BarData.Close)
            : base(input)
        {
            this.length = length;
            this.barData = barData;
            this.calcType = calcType;
            this.includeLast = includeLast;
            this.Init();
        }

        protected override void Init()
        {
            if (base.input is BarSeries)
            {
                base.name = string.Concat(new object[] { "PriceChannel(", this.length, ",", this.barData, ")" });
            }
            else
            {
                base.name = "PriceChannel(" + this.length + ")";
            }

            base.description = "PriceChannel";
            base.Clear();
            base.calculate = true;
        }

        public static double Value(ISeries input, int index, int length, CalcType calcType, IncludeLast includeLast,BarData barData = BarData.Close)
        {
            int _index = index - length + 1;
            if (_index < 0)
            {
                return double.NaN;
            }

            if (calcType == CalcType.Max)
            {
                double max = double.MinValue;
                for (int i = _index; i < index; ++i)
                {
                    max = Math.Max(max, input[i, barData]);
                }
                if (IncludeLast.Yes == includeLast)
                {
                    max = Math.Max(max, input[index, barData]);
                }
                if (max == double.MinValue)
                    return double.NaN;
                return max;
            }
            else
            {
                double min = double.MaxValue;
                for (int i = _index; i < index; ++i)
                {
                    min = Math.Min(min, input[i, barData]);
                }
                if (IncludeLast.Yes == includeLast)
                {
                    min = Math.Min(min, input[index, barData]);
                }
                if (min == double.MaxValue)
                    return double.NaN;
                return min;
            }
        }

        public override void Calculate(int index)
        {
            double d = Value(base.input, index, length,calcType, includeLast, barData);

            if (!double.IsNaN(d))
            {
                base.Add(base.input.GetDateTime(index), d);
            }
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
