using SmartQuant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Data
{
    public class TdxDataImport
    {
        public Bar ReadBarDay(BinaryReader br,int instrumentId,long size)
        {
            int date = br.ReadInt32();
            double open = br.ReadInt32() / 100.0;
            double high = br.ReadInt32() / 100.0;
            double low = br.ReadInt32() / 100.0;
            double close = br.ReadInt32() / 100.0;
            double amount = Convert.ToDouble(Convert.ToDecimal(br.ReadSingle()));
            int vol = br.ReadInt32();
            int preclose = br.ReadInt32(); // 奇怪，这个数不知道是什么

            int year = date / 10000;
            int month = date % 10000 / 100;
            int day = date % 100;

            DateTime openDateTime = new DateTime(year, month, day);
            DateTime closeDataTime = openDateTime.AddSeconds(size);

            Bar bar = new Bar(openDateTime, closeDataTime, instrumentId, SmartQuant.BarType.Time, size,
                open,high,low,close,vol,0);

            // 成交额不要了？
            //Bar.AddField("Amount",0);
            //bar["Amount"] = amount;
            
            return bar;
        }

        public Bar ReadBarMin(BinaryReader br, int instrumentId, long size)
        {
            int date = br.ReadUInt16();
            int min = br.ReadUInt16();
            // 股票2位，国债回购3位，300ETF是4位
            double open = Convert.ToDouble(Convert.ToDecimal(br.ReadSingle()));
            double high = Convert.ToDouble(Convert.ToDecimal(br.ReadSingle()));
            double low = Convert.ToDouble(Convert.ToDecimal(br.ReadSingle()));
            double close = Convert.ToDouble(Convert.ToDecimal(br.ReadSingle()));
            double amount = Convert.ToDouble(Convert.ToDecimal(br.ReadSingle())); // 成交额不保存
            int vol = br.ReadInt32();
            int reserve = br.ReadInt32(); // 保留字

            int year = date / 2048 + 2004;
            int month = date % 2048 / 100;
            int day = date % 2048 % 100;

            int HH = min / 60;
            int mm = min % 60;

            DateTime closeDataTime = new DateTime(year, month, day, HH, mm, 0);
            DateTime openDateTime = closeDataTime.AddSeconds(-size);

            Bar bar = new Bar(openDateTime, closeDataTime, instrumentId, SmartQuant.BarType.Time, size,
                open, high, low, close, vol, 0);

            return bar;
        }

        public Bar ReadBar(BinaryReader br,int instrumentId,long size)
        {
            if (size == 86400)
                return ReadBarDay(br, instrumentId, size);
            else if(size == 60 || size == 60*5)
                return ReadBarMin(br, instrumentId, size);
            
            return null;
        }

        public BarSeries ReadFile(Stream fs, long size, int instrumentId, SeekOrigin orgin, int offset)
        {
            BarSeries bs = new BarSeries();

            if (orgin == SeekOrigin.End)
                fs.Position = fs.Length - 32 * offset;
            else
                fs.Position = 32 * offset;

            BinaryReader br = new BinaryReader(fs);
            for (long i = fs.Position; i < fs.Length; i += 32)
            {
                bs.Add(ReadBar(br, instrumentId, size));
            }
            br.Close();
            fs.Close();
            return bs;
        }

        static void _Main(string[] args)
        {
            TdxDataImport tdi = new TdxDataImport();
            string symbol = "000001.SZE";
            Framework framework = Framework.Current;
            Instrument instrument = framework.InstrumentManager.Get(symbol);
            if(instrument == null)
            {
                instrument = new Instrument(SmartQuant.InstrumentType.Stock,symbol,"",CurrencyId.CNY);
                framework.InstrumentManager.Add(instrument);
            }

            //BarSeries bs = tdi.ReadFile(File.Open(@"D:\new_hbzq\vipdoc\sh\lday\sh000001.day", FileMode.Open), 86400,instrument.Id, SeekOrigin.Begin, 0);
            //BarSeries bs = tdi.ReadFile(File.Open(@"D:\new_hbzq\vipdoc\sh\minline\sh000002.lc1", FileMode.Open), 60,instrument.Id, SeekOrigin.Begin, 0);
            BarSeries bs = tdi.ReadFile(File.Open(@"D:\new_hbzq\vipdoc\sh\fzline\sh204001.lc5", FileMode.Open), 5 * 60, instrument.Id, SeekOrigin.End, 1);
            framework.DataManager.Save(bs);

            framework.Dispose();
        }
    }
}
