using QuantBox.Demo.Strategy;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuantBox.Demo.Scenario
{
    public class RealtimeLoadOnStart : SmartQuant.Scenario
    {
        private long barSize;

        public RealtimeLoadOnStart(Framework framework)
            : base(framework)
        {
            // Set bar size in seconds. 300 seconds is 5 minute.
            barSize = 15;
        }

        public override void Run()
        {
            // Prepare running.
            Console.WriteLine("Prepare running in {0} mode...", framework.StrategyManager.Mode);

            // Get trading instruments.
            Instrument ins1 = InstrumentManager.Instruments["IF1612"];

            // Create SMA Crossover with Loading data on start strategy.
            // and add trading instruments.
            DoubleMA_Crossover smaCrossoverLOS = new DoubleMA_Crossover(framework, "SMACrossoverLOS");
            smaCrossoverLOS.Instruments.Add(ins1);

            // Set strategy as main.
            strategy = smaCrossoverLOS;

            Console.WriteLine("Prepare running in {0} mode...", framework.StrategyManager.Mode);

            // 开始时间是前一个交易日，这个地方要按自己策略的实际情况进行调整
            DateTime startDate = DateTime.Now.DayOfWeek == DayOfWeek.Monday ? DateTime.Now.AddDays(-3).Date : DateTime.Now.AddDays(-1).Date;
            DateTime historicalData1EndTime = startDate;

            // 取本地的数据的最后时间
            DataSeries ins1DataSeries = framework.DataManager.GetDataSeries(ins1, DataObjectType.Trade);

            if (ins1DataSeries != null && ins1DataSeries.Count > 0)
                historicalData1EndTime = ins1DataSeries.DateTime2;

            // 以两个时间的最大值为起点
            historicalData1EndTime = new DateTime(Math.Max(historicalData1EndTime.Ticks, startDate.Ticks));
            

            // Load and save historical trades from QuantBase provider.
            IHistoricalDataProvider quantBase = framework.ProviderManager.GetHistoricalDataProvider(94);

            if (quantBase.Status == ProviderStatus.Disconnected)
                quantBase.Connect();
            // 等待连接成功，订阅太快了不行
            while (!quantBase.IsConnected)
                Thread.Sleep(1000);


            // Load historical trades.
            Console.WriteLine("Load historical data.");
            TickSeries ins1TickSeries = framework.DataManager.GetHistoricalTrades(quantBase, ins1, historicalData1EndTime, DateTime.Now);

            Console.WriteLine("Save historical data.");
            // Save historical trades.
            foreach (Trade trade in ins1TickSeries)
                framework.DataManager.Save(ins1, trade);

            // Set DataSimulator's dates.
            DataSimulator.DateTime1 = startDate;
            DataSimulator.DateTime2 = DateTime.Now;

            // Set null for event filter.
            framework.EventManager.Filter = null;

            // Set property for suspend trading during simulation.
            DoubleMA_Crossover.SuspendTrading = true;

            // Add 5 minute bars (300 seconds) for trading instruments.
            BarFactory.Add(ins1, SmartQuant.BarType.Time, barSize);

            // Run in simulation.
            Console.WriteLine("Run in Backtest mode.");

            // Save current strategy mode.
            StrategyMode mode = framework.StrategyManager.Mode;

            // Set backtest mode.
            framework.StrategyManager.Mode = StrategyMode.Backtest;

            StartStrategy(StrategyMode.Backtest);

            // Run.
            Console.WriteLine("Run in {0} mode.", framework.StrategyManager.Mode);



            // Restore strategy mode.
            framework.StrategyManager.Mode = mode;

            // Get provider for realtime.
            Provider quantRouter = framework.ProviderManager.GetProvider(99) as Provider;

            if (quantRouter.Status == ProviderStatus.Disconnected)
                quantRouter.Connect();
            while (!quantRouter.IsConnected)
                Thread.Sleep(1000);

            // Set property for trading.
            DoubleMA_Crossover.SuspendTrading = false;

            if (framework.StrategyManager.Mode == StrategyMode.Paper)
            {
                // Set QuantRouter as data provider.
                strategy.DataProvider = quantRouter as IDataProvider;
            }
            else if (framework.StrategyManager.Mode == StrategyMode.Live)
            {
                // Set QuantRouter as data and execution provider.
                strategy.DataProvider = quantRouter as IDataProvider;
                strategy.ExecutionProvider = quantRouter as IExecutionProvider;
            }
            

            StartStrategy(framework.StrategyManager.Mode);
        }
    }
}

