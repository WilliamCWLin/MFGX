using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Script
{
    internal class Script_DUT_Related : ScriptDUTBase
    {
        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;   
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess(string Actionitem, string strParamInput)
        {
            //設定要執行的ITEM及初始化一些參數
            strActItem = Actionitem;
            strParam = strParamInput;
            return true;
        }
        public override bool Process(DUT_BASE DUTDevice)
        {
            //調用DUT中的測試項目
            return DUTDevice.StartAction(strActItem, strParam,ref strOutData);
        }
        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            //標準化輸出資料JSON
            string result = ProcessData(strOutData, strCheckSpec);
            strDataout = strOutData;

            if (result == "PASS" || strCheckSpec == "")
                return true;
            else
                return false;

        }
        public class SpecParam
        {
            public string Name { get; set; }
            public SpecType SpecType { get; set; }
            public string SpecValue { get; set; }
            public double MaxLimit { get; set; }
            public double MinLimit { get; set; }
        }

        public class SpecParamsContainer
        {
            public List<SpecParam> specParams { get; set; }
        }

        public string ProcessData(string dataStr, dynamic specParams)
        {
            try
            {
                SpecParamsContainer specParams2 = JsonConvert.DeserializeObject<SpecParamsContainer>(specParams);
                if (string.IsNullOrEmpty(dataStr))
                {                  
                    return "FAIL";
                }
                JObject data = JObject.Parse(dataStr);

                foreach (var param in specParams2.specParams)
                {
                    if (!data.ContainsKey(param.Name))
                    {
                        continue;
                    }

                    switch (param.SpecType)
                    {
                        case SpecType.Range:
                            double value = (double)data[param.Name];
                            if (!(value >= param.MinLimit && value <= param.MaxLimit))
                            {
                                return $"{param.Name} 超出了允許的範圍 ({param.MinLimit} 到 {param.MaxLimit})";
                            }
                            break;

                        case SpecType.Equal:
                            string str = (string)data[param.Name];
                            if (!str.Equals(param.SpecValue))
                            {
                                return $"{param.Name} 值不符合要求，應該等於 {param.SpecValue}";
                            }
                            break;

                        default:
                            // 如果 SpecType 不是 Range 或 Equal，可以處理相應的情況
                            break;
                    }
                }


                return "PASS";
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                return "無法解析輸入數據為 JSON 格式";
            }
            catch (Exception ex)
            {
                return $"處理數據時出現錯誤: {ex.Message}";
            }
        }

        public enum SpecType
        {
            Range,  // 上下限
            Equal   // 字符相等
        }
    }
}
