using SmartQuant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Data
{
    public class CsvDataImport
    {
        public BarSeries Bars;
        public TickSeries Trades;
        public TickSeries Asks;
        public TickSeries Bids;

        public void ReadLine(string[] arr, int instrumentId, out Trade trade, out Bid bid, out Ask ask)
        {
            // 注意：instrumentId必填，否则导入后可以在视图中看到数据，但运行时不会触发事件
            // 其它内容请按自己的实际情况进行修改

            trade = null;
            bid = null;
            ask = null;

            DateTime dt = DateTime.Parse(arr[0]);

            {
                trade = new Trade();
                trade.InstrumentId = instrumentId;
                trade.DateTime = dt;
                trade.Price = double.Parse(arr[1]);
                trade.Size = int.Parse(arr[2]);
            }

            {
                bid = new Bid();
                bid.InstrumentId = instrumentId;
                bid.DateTime = dt;
                bid.Price = double.Parse(arr[1]);
                bid.Size = int.Parse(arr[2]);
            }

            {
                ask = new Ask();
                ask.InstrumentId = instrumentId;
                ask.DateTime = dt;
                ask.Price = double.Parse(arr[1]);
                ask.Size = int.Parse(arr[2]);
            }
        }

        public void ReadLine(string[] arr, int instrumentId, SmartQuant.BarType barType, long size, out Bar bar)
        {
            // 注意：instrumentId必填，否则导入后可以在视图中看到数据，但运行时不会触发事件
            // 其它内容请按自己的实际情况进行修改
            bar = null;

            // 由2014导出的时间是CloseDateTime，由3.x导出的时间是OpenDateTime
            DateTime dt = DateTime.Parse(arr[0]);
            DateTime openDateTime = dt.AddSeconds(-size);
            DateTime closeDateTime = dt;

            {
                bar = new Bar(openDateTime,closeDateTime,instrumentId,barType,size);
                
                bar.Open = double.Parse(arr[1]);
                bar.High = double.Parse(arr[2]);
                bar.Low = double.Parse(arr[3]);
                bar.Close = double.Parse(arr[4]);
                bar.Volume = long.Parse(arr[5]);
                bar.OpenInt = long.Parse(arr[6]);
            }
        }

        static void _Main(string[] args)
        {
            CsvDataImport cdi = new CsvDataImport();
            string symbol = "AAPL2";
            Framework framework = Framework.Current;
            Instrument instrument = framework.InstrumentManager.Get(symbol);
            if (instrument == null)
            {
                instrument = new Instrument(SmartQuant.InstrumentType.Stock, symbol, "", CurrencyId.CNY);
                framework.InstrumentManager.Add(instrument);
            }

            TickSeries trades = new TickSeries();
            TickSeries bids = new TickSeries();
            TickSeries asks = new TickSeries();
            BarSeries bars = new BarSeries();

            //using (StreamReader file = File.OpenText(@"‪d:\wukan\Desktop\AAPL.Bar Time 86400.csv"))
            FileInfo fi = new FileInfo(@"D:\AAPL.BarTime86400.csv");
            using (StreamReader file = new StreamReader(fi.OpenRead()))
            {
                int i = 0;
                string str = file.ReadLine();
                do
                {
                    ++i;
                    str = file.ReadLine();
                    if (str == null)
                        break;

                    string[] arr = str.Split(',');
                    
                    Trade trade = null;
                    Bid bid = null;
                    Ask ask = null;
                    Bar bar = null;

                    //cdi.ReadLine(arr, instrument.Id, out trade, out bid, out ask);
                    cdi.ReadLine(arr, instrument.Id, SmartQuant.BarType.Time, 86400, out bar);

                    if (trade != null)
                        trades.Add(trade);

                    if (bid != null)
                        bids.Add(bid);

                    if (ask != null)
                        asks.Add(ask);

                    if (bar != null)
                        bars.Add(bar);

                } while (str != null);
                file.Close();
            }

            if (trades.Count > 0)
                framework.DataManager.Save(trades);

            if (bids.Count > 0)
                framework.DataManager.Save(bids);

            if (asks.Count > 0)
                framework.DataManager.Save(asks);

            if (bars.Count > 0)
                framework.DataManager.Save(bars);

            framework.Dispose();
        }
    }
}
