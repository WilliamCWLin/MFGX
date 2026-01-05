using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoTestSystem.Base.ScriptBase;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Base
{
    public class DataCollection
    {
        private Dictionary<string, string> Data = new Dictionary<string, string>();
        private Dictionary<string, string> SpecData = new Dictionary<string, string>();
        private Dictionary<string, string> CSVItem = new Dictionary<string, string>();
        private Dictionary<string, string> CSVHead = new Dictionary<string, string>();
        private Dictionary<string, DataItem> ItemData = new Dictionary<string, DataItem>();
        OrderedDictionary csvHead = new OrderedDictionary();
        List<string> CSVHeadList = new List<string>(); // 定义一个动态列表

        Stack<string> stack = new Stack<string>();       
        string MESDataFinal = string.Empty;

        private Dictionary<string, string> MESData = new Dictionary<string,string>();
        //public event EventHandler DataChanged;
        public bool SetMesData(string meskey,Tuple<string,string,string>mesdata)
        {
            string Mesdata = string.Empty;
            try
            {
                Mesdata = $"{mesdata.Item1}@{mesdata.Item2}@{mesdata.Item3}";              
                if (MESData.ContainsKey(meskey))
                    MESData[meskey] = Mesdata;
                else
                    MESData.Add(meskey, Mesdata);                   
             return true;
               
            }catch(Exception e3)
            {
                Logger.Error($"MESData Push Error=>{e3.Message}");
                return false;
            }

        }
        //protected virtual void OnDictionaryChanged()
        //{
        //    // 检查是否有订阅者，如果有则触发事件
        //    DataChanged?.Invoke(this, EventArgs.Empty);
        //}
        public string GetMesData(string meskey)
        {          
            // 檢查 key 是否存在並且值不為空
            if (MESData.TryGetValue(meskey, out var mesvalue) && mesvalue != null)
            {
                return mesvalue.ToString(); // 如果值不為空，返回值的字符串表示形式
            }
            else
            {
                // 如果 key 不存在或者值為空，返回一個適當的預設值，或者拋出異常，視情況而定
                return $"NULL";
            }
        }      

        public string GetALLMESData()
        {
            foreach(string Allmeskey in MESData.Keys)
            {
                MESDataFinal += GetMesData(Allmeskey)+";";
            }
            MESDataFinal = MESDataFinal.TrimEnd(';');//去除字串尾端;
            //MesInfoLogger(MESDataFinal);
            return MESDataFinal;
        }

        public void MesInfoLogger(string inputdata)
        {
            string output_data = string.Empty;
            output_data = inputdata.Replace(';','\n');
            Logger.Debug($"\r\nALLMESData-Start=>\r\n{output_data}\r\nALLMESData-END=>");
        }
        public bool SetMoreProp(string key, string data)
        {
            if (Data.ContainsKey(key))
            {
                Data[key] = data;
            }
            else
            {
                Data.Add(key, data);             
            }

            //OnDictionaryChanged();

            return true;
        }
        public bool SetSpecProp(string key, string data)
        {
            if (SpecData.ContainsKey(key))
            {
                SpecData[key] = data;
            }
            else
            {
                SpecData.Add(key, data);
            }

            //OnDictionaryChanged();

            return true;
        }
        public string GetMoreProp(string key)
        {
            // 檢查 key 是否存在並且值不為空
            if (Data.TryGetValue(key, out var value) && value != null)
            {
                return value.ToString(); // 如果值不為空，返回值的字符串表示形式
            }
            else
            {
                // 如果 key 不存在或者值為空，返回一個適當的預設值，或者拋出異常，視情況而定
                return "";
            }
        }

        public bool SetCSVHeader(string headerkey)
        {
           if (!CSVHeadList.Contains("ProductSN"))
            {
                CSVHeadList.Add("ProductSN");
                CSVHeadList.Add("Result");
                CSVHeadList.Add("Failitem");
                CSVHeadList.Add("StartTime");
                CSVHeadList.Add("EndTime");
                CSVHeadList.Add("EndTotalTime");
            }

            CSVHeadList.Add(headerkey);           
            //CSVHeadList.Remove("Result");
            //CSVHeadList.Add("Result");

            // 循环遍历 dataList 列表
            foreach (string item in CSVHeadList)
            {
                // 将元素添加到 dataDict 中
                if (CSVItem.ContainsKey(item))
                    CSVItem[item] = item;
                else
                    CSVItem.Add(item ,item);           
            }

            SaveCSVKeyFile();

            return true;
        }

        public string GetCSVHeader(string item)
        {
            if (CSVItem.TryGetValue(item, out var item_name) && item_name != null)
            {
               if (CSVHead.ContainsKey(item_name))
                   CSVHead[item_name] = "";
               else
                  CSVHead.Add(item_name, "");
                //SaveCSVKeyFile(true);
                return item_name.ToString(); // 如果值不為空，返回值的字符串表示形式
            }
            else
            {
                // 如果 key 不存在或者值為空，返回一個適當的預設值，或者拋出異常，視情況而定
                return "";
            }
           
        }
        static void AddKeyValuePair(OrderedDictionary dictionary,object key ,object value)
        {
            if(dictionary.Contains(key))
            {
                dictionary.Remove(key);
            }

            dictionary.Add(key, value);
        }
        public bool Clear()
        {
            try
            {
                MESDataFinal = string.Empty;            
                Data.Clear();
                SpecData.Clear();
                CSVHeadList.Clear();
                CSVItem.Clear();
                MESData.Clear();
                ItemData.Clear();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public void SaveCSVKeyFile()
        {
            string jsonStr = string.Empty;
            string jsonStr_key = string.Empty;
            // Convert the dictionary to a JSON string           
            jsonStr = JsonConvert.SerializeObject(new { headers = CSVItem.Keys });
            // Write the JSON string to a file
            File.WriteAllText("Config\\Header.json", jsonStr);
                       
        }

        public Dictionary<string, string> GetData()
        {
            return Data;
        }

        public Dictionary<string, string> GetMESData()
        {
            return MESData;
        }
        public Dictionary<string, string> GetSpecData()
        {
            return SpecData;
        }
        public Dictionary<string, DataItem> GetItemData()
        {
            return ItemData;
        }
    }
}
