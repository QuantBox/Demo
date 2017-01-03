using Newtonsoft.Json;
using SmartQuant;
using SmartQuant.Optimization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuantBox.Demo.Helper
{
    /// <summary>
    /// 将Strategy上的Parameter全部读取出来，存盘，设置回去
    /// </summary>
    public class ParameterHelper
    {
        private static JsonSerializerSettings jSetting = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
        };

        public void SetParameters(object obj,OptimizationParameterSet set)
        {
            foreach (OptimizationParameter parameter in set)
            {
                FieldInfo info = obj.GetType().GetField(parameter.Name,BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if(info != null)
                {
                    var v = Convert.ChangeType(parameter.Value, info.FieldType);
                    info.SetValue(obj, v);
                }
            }
        }

        public OptimizationParameterSet GetParameters(object obj)
        {
            OptimizationParameterSet set = new OptimizationParameterSet();
            foreach (FieldInfo info in obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (info.GetCustomAttributes(typeof(ParameterAttribute), true).Length > 0)
                {
                    set.Add(info.Name, info.GetValue(obj));
                }
            }
            return set;
        }

        public string SaveParameters(OptimizationParameterSet set,string path)
        {
            string str = JsonConvert.SerializeObject(set, set.GetType(), jSetting);
            using (TextWriter writer = new StreamWriter(path))
            {
                writer.Write("{0}", str);
                writer.Close();
            }
            return str;
        }

        public OptimizationParameterSet LoadParameters(string path)
        {
            List<OptimizationParameter> list = new List<OptimizationParameter>();
            try
            {
                using (TextReader reader = new StreamReader(path))
                {
                    list = (List<OptimizationParameter>)JsonConvert.DeserializeObject(reader.ReadToEnd(), list.GetType());
                    reader.Close();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            OptimizationParameterSet set = new OptimizationParameterSet();
            foreach(var l in list)
            {
                set.Add(l);
            }
            return set;
        }
    }
}
