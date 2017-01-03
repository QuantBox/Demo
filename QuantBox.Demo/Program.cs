using QuantBox.Demo.Helper;
using QuantBox.Demo.Scenario;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //Framework.Current.StrategyManager.Mode = StrategyMode.Live;

            //SmartQuant.Scenario scenario = new IndicatorTest(Framework.Current);
            //SmartQuant.Scenario scenario = new Backtest86400(Framework.Current);
            //SmartQuant.Scenario scenario = new Realtime86400(Framework.Current);
            // 在使用Backtest之前，需要先使用 PbTickDataExport._Main 生成 数据文件 D:\1.data 才能测试
            SmartQuant.Scenario scenario = new Backtest(Framework.Current);
            //SmartQuant.Scenario scenario = new RealtimeLoadOnStart(Framework.Current);
            //SmartQuant.Scenario scenario = new BacktestLoadOnStart(Framework.Current);

            scenario.Run();
        }
    }
}





