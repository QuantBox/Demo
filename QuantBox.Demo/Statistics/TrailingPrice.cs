using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Statistics
{
    public class TrailingPrice
    {
        private Framework framework;
        private SmartQuant.Strategy strategy;

        public double HighestAfterEntry;
        public double LowestAfterEntry;
        public DateTime HighestAfterEntryDateTime;
        public DateTime LowestAfterEntryDateTime;

        public TrailingPrice()
        {
        }

        public TrailingPrice(Framework framework, SmartQuant.Strategy strategy)
        {
            this.framework = framework;
            this.strategy = strategy;
        }

        public TrailingPrice(TrailingPrice trailingPrice)
        {
            HighestAfterEntry = trailingPrice.HighestAfterEntry;
            LowestAfterEntry = trailingPrice.LowestAfterEntry;
            HighestAfterEntryDateTime = trailingPrice.HighestAfterEntryDateTime;
            LowestAfterEntryDateTime = trailingPrice.LowestAfterEntryDateTime;
        }

        public void Reset()
        {
            HighestAfterEntry = double.MinValue;
            LowestAfterEntry = double.MaxValue;
        }

        public void Update(double price)
        {
            if (price > HighestAfterEntry)
            {
                HighestAfterEntry = price;
                HighestAfterEntryDateTime = framework.Clock.DateTime;
            }

            if (price < LowestAfterEntry)
            {
                LowestAfterEntry = price;
                LowestAfterEntryDateTime = framework.Clock.DateTime;
            }
        }
    }
}
