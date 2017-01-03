using QuantBox.Data.Serializer.V1;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Data
{
    public class PbTickDataExport
    {
        public static void _Main(string[] args)
        {
            string symbol = "AAPL";
            Framework framework = Framework.Current;
            Instrument instrument = framework.InstrumentManager.Get(symbol);
            DataSeries ds = framework.DataManager.GetDataSeries(instrument, DataObjectType.Trade);
            
            PbTickSerializer pts = new PbTickSerializer();
            // 最关键的部分，需要提前设置
            pts.Codec.Config.SetTickSize(0.01);

            PbTick last = new PbTick();

            using (Stream stream = File.OpenWrite(@"D:\1.data"))
            {
                for (int i = 0; i < ds.Count; ++i)
                {
                    var d = ds[i];
                    Trade t = d as Trade;
                    PbTick tick = new PbTick();

                    // 必须设置数据中的TickSize
                    tick.Config = pts.Codec.Config;

                    pts.Codec.SetLastPrice(tick,t.Price);
                    pts.Codec.SetVolume(tick, t.Size);
                    pts.Codec.SetSymbol(tick, symbol);

                    // 时间设置也很关键
                    pts.Codec.SetActionDay(tick, t.DateTime.Date);
                    pts.Codec.SetUpdateTime(tick, t.DateTime - t.DateTime.Date);
                    
                    // 写入流
                    pts.Write(tick, new Stream[] { stream });
                }
                stream.Close();
            }
        }
    }
}
