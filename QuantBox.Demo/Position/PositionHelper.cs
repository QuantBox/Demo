using QuantBox.Demo.Position;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QuantBox.Extensions;
using XAPI;
using System.IO;

namespace QuantBox.Demo.Helper
{
    public class PositionHelper
    {
        public Framework framework;

        public PositionHelper(Framework framework)
        {
            this.framework = framework;
        }

        public void SyncPosition(byte providerId, byte route, Portfolio Long, Portfolio Short, DualPositionContainer container)
        {
            var accountDataEntry = framework.AccountDataManager.GetSnapshot(providerId, route).Entries[0];
            AccountPositionToPortfolio(accountDataEntry.Positions, true, Long, Short);
            AccountPositionToDualPosition(accountDataEntry.Positions, true, container);
        }

        public void QtyToLongShort(Instrument instrument, double LongQty, double ShortQty, Portfolio Long, Portfolio Short)
        {
            {
                double Qty = 0;
                SmartQuant.Position p = Long.GetPosition(instrument);
                if (p != null)
                    Qty = p.Qty;
                double diff = LongQty - Qty;
                if (diff > 0)
                    Long.Add(new Fill(framework.Clock.DateTime, null, instrument, CurrencyId.CNY, SmartQuant.OrderSide.Buy, diff, 0, "Initial Long Position"));
                else if (diff < 0)
                    Long.Add(new Fill(framework.Clock.DateTime, null, instrument, CurrencyId.CNY, SmartQuant.OrderSide.Sell, -diff, 0, "Initial Long Position"));
            }

            {
                double Qty = 0;
                SmartQuant.Position p = Short.GetPosition(instrument);
                if (p != null)
                    Qty = p.Qty;
                double diff = ShortQty - Qty;
                if (diff > 0)
                    Short.Add(new Fill(framework.Clock.DateTime, null, instrument, CurrencyId.CNY, SmartQuant.OrderSide.Sell, diff, 0, "Initial Short Position"));
                else if (diff < 0)
                    Short.Add(new Fill(framework.Clock.DateTime, null, instrument, CurrencyId.CNY, SmartQuant.OrderSide.Buy, -diff, 0, "Initial Short Position"));
            }
        }

        public void QtyToNet(Instrument instrument, double NetQty, Portfolio Net)
        {
            double Qty = 0;
            SmartQuant.Position p = Net.GetPosition(instrument);
            if (p != null)
                Qty = p.Qty;
            double diff = NetQty - Qty;
            if (diff > 0)
                Net.Add(new Fill(framework.Clock.DateTime, null, instrument, CurrencyId.CNY, SmartQuant.OrderSide.Buy, diff, 0, "Initial Position"));
            else if (diff < 0)
                Net.Add(new Fill(framework.Clock.DateTime, null, instrument, CurrencyId.CNY, SmartQuant.OrderSide.Sell, -diff, 0, "Initial Position"));
        }

        public void AccountPositionToPortfolio(AccountData[] Positions, bool bOnlyToday, Portfolio Long, Portfolio Short)
        {
            foreach (var pos in Positions)
            {
                string symbol = (string)pos.Fields[AccountDataField.SYMBOL];
                Instrument instrument = framework.InstrumentManager.Get(symbol);
                if(instrument == null)
                    continue;

                int date = (int)pos.Fields[AccountDataFieldEx.DATE];
                if (bOnlyToday)
                {
                    if (date != TimeHelper.GetDate(DateTime.Today))
                        continue;
                }

                QtyToLongShort(instrument,
                    (double)pos.Fields[AccountDataField.LONG_QTY], (double)pos.Fields[AccountDataField.SHORT_QTY],
                    Long, Short);
            }
        }



        public void AccountPositionToPortfolio(AccountData[] Positions, bool bOnlyToday, Portfolio Net)
        {
            foreach (var pos in Positions)
            {
                string symbol = (string)pos.Fields[AccountDataField.SYMBOL];
                Instrument instrument = framework.InstrumentManager.Get(symbol);
                if (instrument == null)
                    continue;

                int date = (int)pos.Fields[AccountDataFieldEx.DATE];
                if (bOnlyToday)
                {
                    if (date != TimeHelper.GetDate(DateTime.Today))
                        continue;
                }

                QtyToNet(instrument, (double)pos.Fields[AccountDataField.QTY], Net);
            }
        }

        public void AccountPositionToDualPosition(AccountData[] Positions, bool bOnlyToday, DualPositionContainer container)
        {
            foreach (var pos in Positions)
            {
                string symbol = (string)pos.Fields[AccountDataField.SYMBOL];
                Instrument instrument = framework.InstrumentManager.Get(symbol);
                if (instrument == null)
                    continue;

                int date = (int)pos.Fields[AccountDataFieldEx.DATE];
                if(bOnlyToday)
                {
                    if (date != TimeHelper.GetDate(DateTime.Today))
                        continue;
                }

                PositionFieldEx pfe = (PositionFieldEx)pos.Fields[AccountDataFieldEx.USER_DATA];

                var record = container.GetPositionRecord(instrument);
                record.Long.Qty = pfe.Long.Position;
                record.Long.QtyToday = pfe.Long.TodayPosition;
                record.Short.Qty = pfe.Short.Position;
                record.Short.QtyToday = pfe.Short.TodayPosition;
            }
        }

        public void DualPositionToPortfolio(DualPositionContainer dualPosition,Portfolio Long, Portfolio Short)
        {
            foreach(var kv in dualPosition.Positions)
            {
                Instrument instrument = framework.InstrumentManager.GetById(kv.Key);
                if (instrument == null)
                    continue;
                // 从文件中还原出来，可能insrument丢失
                kv.Value.Instrument = instrument;

                QtyToLongShort(instrument,
                    kv.Value.Long.Qty, kv.Value.Short.Qty,
                    Long, Short);
            }
        }

        public void DualPositionToPortfolio(DualPositionContainer dualPosition, Portfolio Net)
        {
            foreach (var kv in dualPosition.Positions)
            {
                Instrument instrument = framework.InstrumentManager.GetById(kv.Key);
                if (instrument == null)
                    continue;
                kv.Value.Instrument = instrument;

                QtyToNet(instrument,
                    kv.Value.Long.Qty - kv.Value.Short.Qty,
                    Net);
            }
        }

        public void ReadCsv(string path, DualPositionContainer container)
        {
            using (TextReader file = new StreamReader(File.Open(path,FileMode.Open,FileAccess.Read),Encoding.GetEncoding("GBK")))
            {
                int i = 0;
                // 第一行丢弃
                string str = file.ReadLine();
                do
                {
                    ++i;
                    str = file.ReadLine();
                    if (str == null)
                        break;

                    string[] arr = str.Split(',');

                    Instrument instrument = framework.InstrumentManager.Get(arr[0]);
                    if (instrument == null)
                        continue;

                    var record = container.GetPositionRecord(instrument);

                    MonoPositionRecord mpr = null;
                    if (arr[1].StartsWith("买"))
                    {
                        mpr = record.Long;
                    }
                    else
                    {
                        mpr = record.Short;
                    }

                    mpr.Qty = double.Parse(arr[2]);
                    mpr.QtyToday = double.Parse(arr[3]);
                    
                } while (str != null);
                file.Close();
            }
        }
    }
}
