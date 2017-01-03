using Newtonsoft.Json;
using SmartQuant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XAPI;

namespace QuantBox.Demo.Position
{
    // 支持多个合约
    public class DualPositionContainer
    {
        Framework framework;

        // 同时记录了挂单信息，还区分今昨
        public Dictionary<int, DualPositionRecord> Positions { get; private set; }

        /// <summary>
        /// 指定保存格式
        /// </summary>
        private static JsonSerializerSettings jSetting = new JsonSerializerSettings()
        {
            //NullValueHandling = NullValueHandling.Ignore,
            //DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.Indented,
        };

        public DualPositionContainer(Framework framework)
        {
            this.framework = framework;
            Positions = new Dictionary<int, DualPositionRecord>();
        }

        public void Reset()
        {
            Positions.Clear();
        }

        public void ChangeTradingDay()
        {
            foreach (var kv in Positions)
            {
                kv.Value.ChangeTradingDay();
            }
        }

        public void Load(string path)
        {
            object ret;
            using (TextReader reader = new StreamReader(path))
            {
                ret = JsonConvert.DeserializeObject(reader.ReadToEnd(), Positions.GetType());
                reader.Close();
            }
            Positions = ret as Dictionary<int, DualPositionRecord>;
        }

        public void Save(string path)
        {
            using (TextWriter writer = new StreamWriter(path))
            {
                writer.Write("{0}", JsonConvert.SerializeObject(Positions, Positions.GetType(), jSetting));
                writer.Close();
            }
        }

        #region 基本函数
        public MonoPositionRecord GetPositionRecord(DualPositionRecord record, SmartQuant.OrderSide Side, OpenCloseType OpenClose)
        {
            return record.GetPositionRecord(Side, OpenClose);
        }

        public DualPositionRecord GetPositionRecord(Instrument instrument)
        {
            DualPositionRecord value;
            if (!Positions.TryGetValue(instrument.Id, out value))
            {
                value = new DualPositionRecord();
                value.Instrument = instrument;
                Positions.Add(instrument.Id, value);
            }
            return value;
        }
        #endregion
    }
}
