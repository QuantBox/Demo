using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Helper
{
    public enum EnumTradingTime
    {
        TradingTime_0915_1515,  // 金融
        TradingTime_0900_1515,  // 金融
        TradingTime_0900_1500,  // 商品
        TradingTime_2300,
        TradingTime_2330,
        TradingTime_0100,
        TradingTime_0230, // 黄金、白银
    }

    public class TimeHelper
    {
        public int[] WorkingTime;
        public int EndOfDay { get; private set; }
        public int BeginOfDay { get; private set; }

        public int[] WorkingTime_0915_1515 = { 915, 1130, 1300, 1515 }; //IF,TF,IO
        public int[] WorkingTime_0900_1515 = { 900, 1130, 1300, 1515 }; //EF,AF
        public int[] WorkingTime_0900_1500 = { 900, 1015, 1030, 1130, 1330, 1500 }; //商品
        public int[] WorkingTime_2300 = { 900, 1015, 1030, 1130, 1330, 1500, 2100, 2300 };//天然橡胶
        public int[] WorkingTime_2330 = { 900, 1015, 1030, 1130, 1330, 1500, 2100, 2330 };//白糖、棉花、菜粕、甲醇、PTA
        public int[] WorkingTime_0100 = { 0, 100, 900, 1015, 1030, 1130, 1330, 1500, 2100, 2400 };//铜、铝、铅、锌
        public int[] WorkingTime_0230 = { 0, 230, 900, 1015, 1030, 1130, 1330, 1500, 2100, 2400 };//au,ag,p,j
        
        

        private int EndOfDay_1515 = 1515; //IF
        private int EndOfDay_1500 = 1500; //商品

        private int BeginOfDay_0900 = 900;
        private int BeginOfDay_0915 = 915;
        private int BeginOfDay_2100 = 2100;

        public TimeHelper(EnumTradingTime tt)
        {
            switch (tt)
            {
                case EnumTradingTime.TradingTime_0915_1515:
                    WorkingTime = WorkingTime_0915_1515;
                    BeginOfDay = BeginOfDay_0915;
                    EndOfDay = EndOfDay_1515;                    
                    break;
                case EnumTradingTime.TradingTime_0900_1515:
                    WorkingTime = WorkingTime_0900_1515;
                    BeginOfDay = BeginOfDay_0900;
                    EndOfDay = EndOfDay_1515;                    
                    break;
                case EnumTradingTime.TradingTime_0900_1500:
                    WorkingTime = WorkingTime_0900_1500;
                    BeginOfDay = BeginOfDay_0900;
                    EndOfDay = EndOfDay_1515;
                    break;
                case EnumTradingTime.TradingTime_2300:
                    WorkingTime = WorkingTime_2300;
                    BeginOfDay = BeginOfDay_2100;
                    EndOfDay = EndOfDay_1500;
                    break;
                case EnumTradingTime.TradingTime_2330:
                    WorkingTime = WorkingTime_2330;
                    BeginOfDay = BeginOfDay_2100;
                    EndOfDay = EndOfDay_1500;
                    break;
                case EnumTradingTime.TradingTime_0100:
                    WorkingTime = WorkingTime_0100;
                    BeginOfDay = BeginOfDay_2100;
                    EndOfDay = EndOfDay_1500;
                    break;
                case EnumTradingTime.TradingTime_0230:
                    WorkingTime = WorkingTime_0230;
                    BeginOfDay = BeginOfDay_2100;
                    EndOfDay = EndOfDay_1500;
                    break;
            }
        }

        public TimeHelper(string instrument)
            : this(GetTradingTime(instrument))
        {
        }

        public TimeHelper(int[] workingTime, int beginOfDay, int ennOfDay)
        {
            WorkingTime = workingTime;
            BeginOfDay = beginOfDay;
            EndOfDay = ennOfDay;
        }

        public override string ToString()
        {
            return string.Format("{0}", WorkingTime);
        }

        public static EnumTradingTime GetTradingTime(string instrument)
        {
            string prefix = instrument.Substring(0, 2);
            switch (prefix)
            {
                case "IF":// 中金所 沪深300股指期货
                case "TF":// 中金所 国债期货
                case "IO":// 中金所 沪深300股指期权
                case "IH":// 中金所 上证50股指期货
                case "IC":// 中金所 中证500股指期货
                case "HO":// 中金所 上证50股指期权
                    return EnumTradingTime.TradingTime_0915_1515;
                case "EF":// 中金所 欧元兑美元期货
                case "AF":// 中金所 澳元兑美元期货
                    return EnumTradingTime.TradingTime_0900_1515;
                case "ru":// 上期所 天然橡胶
                    return EnumTradingTime.TradingTime_2300;
                case "SR":// 郑商所 白糖
                case "CF":// 郑商所 棉花
                case "RM":// 郑商所 菜粕
                case "ME":// 郑商所 甲醇 50吨每手
                case "MA":// 郑商所 甲醇 10吨每手 1506开始
                case "TA":// 郑商所 PTA
                    return EnumTradingTime.TradingTime_2330;
                case "cu":// 上期所 铜
                case "al":// 上期所 铝
                case "pb":// 上期所 铅
                case "zn":// 上期所 锌

                case "rb":// 上期所 螺纹纲
                case "hc":// 上期所 热轧卷板
                case "bu":// 上期所 石油沥青
                    return EnumTradingTime.TradingTime_0100;
                case "au":// 上期所 黄金
                case "ag":// 上期所 白银

                case "jm":// 大商所 焦煤
                    return EnumTradingTime.TradingTime_0230;
                default:
                    prefix = instrument.Substring(0, 1);
                    switch (prefix)
                    {
                        case "p":// 大商所 棕榈油
                        case "j":// 大商所 焦炭

                        case "a":// 大商所 黄大豆一号
                        case "b":// 大商所 黄大豆二号
                        case "m":// 大商所 豆粕
                        case "y":// 大商所 豆油
                        case "i":// 大商所 铁矿石
                            return EnumTradingTime.TradingTime_0230;
                        default:
                            return EnumTradingTime.TradingTime_0900_1500;
                    }
                    
            }
        }

        public bool IsTradingTime(int time)
        {
            int index = -1;
            for (int i = 0; i < WorkingTime.Length; ++i)
            {
                if (time < WorkingTime[i])
                {
                    break;
                }
                else
                {
                    index = i;
                }
            }

            if (index % 2 == 0)
            {
                // 交易时段
                return true;
            }

            // 非交易时段
            return false;
        }

        public int GetNextTradingTime(int time)
        {
            int index = -1;
            for (int i = 0; i < WorkingTime.Length; ++i)
            {
                if (time < WorkingTime[i])
                {
                    break;
                }
                else
                {
                    index = i;
                }
            }

            if (index % 2 == 0)
            {
                // 交易时段
                return time;
            }

            if (index+1 >= WorkingTime.Length)
                return WorkingTime[0];

            // 非交易时段
            return WorkingTime[index+1];
        }

        public DateTime GetNextTradingTime(DateTime dt)
        {
            int time1 = GetTime(dt);
            int time2 = GetNextTradingTime(time1);
            if(time1 == time2)
            {
                return dt;
            }
            else if(time2>time1)
            {
                return dt.Date.AddHours((int)(time2 / 100)).AddMinutes(time2 % 100);
            }
            else
            {
                return dt.Date.AddDays(1).AddHours((int)(time2 / 100)).AddMinutes(time2 % 100);
            }
        }

        public static int GetTime(DateTime dt)
        {
            return dt.Hour * 100 + dt.Minute;
        }

        public static int GetDate(DateTime dt)
        {
            return dt.Year * 10000 + dt.Month * 100 + dt.Day;
        }

        public bool IsTradingTime(DateTime dt)
        {
            return IsTradingTime(GetTime(dt));
        }
    }
}
