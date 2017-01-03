using SmartQuant;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Statistics
{
    public class HighLowPrice
    {
        private Framework framework;
        private SmartQuant.Strategy strategy;

        public double High;
        public double Low;

        public HighLowPrice()
        {
        }

        public HighLowPrice(Framework framework, SmartQuant.Strategy strategy)
        {
            this.framework = framework;
            this.strategy = strategy;
        }

        public HighLowPrice(HighLowPrice source)
        {
            High = source.High;
            Low = source.Low;
        }

        public void Reset()
        {
            High = double.MinValue;
            Low = double.MaxValue;
        }

        public void Update(double price)
        {
            High = Math.Max(High, price);
            Low = Math.Min(Low, price);
        }

        public void Update(Bar bar)
        {
            High = Math.Max(High, bar.High);
            Low = Math.Min(Low, bar.Low);
        }
    }
}
