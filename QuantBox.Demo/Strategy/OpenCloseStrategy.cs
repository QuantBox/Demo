using QuantBox.Demo.Position;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QuantBox.Extensions;

namespace QuantBox.Demo.Strategy
{
    public class OpenCloseStrategy:InstrumentStrategy
    {
        public OpenCloseHelper OpenCloseHelper;

        public OpenCloseStrategy(Framework framework, string name)
			: base(framework, name)
		{
		}

        protected override void OnStrategyStart()
        {
            OpenCloseHelper = new OpenCloseHelper(framework,this);
            OpenCloseHelper.SeparateOrder = SeparateOrder.SeparateCloseOpen;
            OpenCloseHelper.SendStyle = SendStyle.OneByOne;
        }

        protected override void OnBar(Instrument instrument, Bar bar)
        {
            //if (HasShortPosition())
            //{
            //    var order = BuyOrder(instrument, Position.Qty+1, "S2L");
            //    var orders = OpenCloseHelper.RebuildOrder(order);
            //    OpenCloseHelper.Send(orders);
            //}
            //else if (HasLongPosition())
            //{
            //    var order = SellOrder(instrument, Position.Qty+1, "L2S");
            //    var orders = OpenCloseHelper.RebuildOrder(order);
            //    OpenCloseHelper.Send(orders);
            //}
            //else
            //{
            //    var order = BuyOrder(instrument, 1, "2L");
            //    var orders = OpenCloseHelper.RebuildOrder(order);
            //    OpenCloseHelper.Send(orders);
            //}
        }

        protected override void OnExecutionReport(ExecutionReport report)
        {
            // 极为关键的地方，通过它进行内部的平今与平昨的持仓量更新
            //OpenCloseHelper.OnExecutionReport(report);
        }
    }
}
