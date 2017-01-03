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
    public class PbTickDataImport
    {
        public BarSeries Bars;
        public TickSeries Trades;
        public TickSeries Asks;
        public TickSeries Bids;

        public void ReadFile(int instrumentId, string path)
        {
            Bars = new BarSeries();
            Trades = new TickSeries();
            Asks = new TickSeries();
            Bids = new TickSeries();

            PbTickSerializer pts = new PbTickSerializer();

            PbTick restore = null;

            using (Stream stream = File.OpenRead(path))
            {
                while (true)
                {
                    restore = pts.ReadOne(stream);
                    if (restore == null)
                    {
                        break;
                    }
                    
                    Trade t = new Trade();
                    t.InstrumentId = instrumentId;
                    t.DateTime = pts.Codec.GetActionDayDateTime(restore);
                    t.Price = pts.Codec.GetLastPrice(restore);
                    t.Size = (int)pts.Codec.GetVolume(restore);

                    Trades.Add(t);

                    Bid b = new Bid();
                    b.InstrumentId = instrumentId;
                    b.DateTime = t.DateTime;
                    b.Price = pts.Codec.GetBidPrice(restore, 1);
                    b.Size = pts.Codec.GetBidSize(restore, 1);

                    Bids.Add(b);

                    Ask a = new Ask();
                    a.InstrumentId = instrumentId;
                    a.DateTime = t.DateTime;
                    a.Price = pts.Codec.GetAskPrice(restore, 1);
                    a.Size = pts.Codec.GetAskSize(restore, 1);

                    Asks.Add(a);
                }
                stream.Close();
            }
        }

        static void _Main(string[] args)
        {
            PbTickDataImport ptdi = new PbTickDataImport();

            string symbol = "AAPL";
            Framework framework = Framework.Current;
            Instrument instrument = framework.InstrumentManager.Get(symbol);
            ptdi.ReadFile(instrument.Id, @"D:\1.data");

            //ptdi.Trades;
        }
    }
}
