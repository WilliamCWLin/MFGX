
using AutoTestSystem.Base;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_SimuTest : Script_Extra_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        [Category("Params"), Description("自訂顯示名稱"), TypeConverter(typeof(CheckBoxType))]
        public bool PassorFail { get; set; }
        [Category("Params"), Description("自訂顯示名稱")]
        public int SimuDelayTime { get; set; }
        [Category("Params"), Description("自訂顯示名稱"), Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        public string SimuData { get; set; }
        Delay delay_param = null;

        public Script_SimuTest()
        {
            PassorFail = true;
        }
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess(string ActionItem, string Paraminput)
        {

            strActItem = ActionItem;
            strParam = Paraminput;

            delay_param = JsonConvert.DeserializeObject<Delay>(strParam);

            return true;
        }
        //public static int xx = 0;
        public override bool Action(object o)
        {
            
            Thread.Sleep(SimuDelayTime);
            Logger.Debug($"Data -> {SimuData}");
            string result = CheckRule(SimuData, Spec, RowDataItem);
            Logger.Debug($"Spec -> {Spec}");
            if (result == "PASS" || Spec == "")
            {
                RowDataItem.TestResult = "PASS";

                //if (PassorFail == false)
                //    return false;
                return true;
            }
                
            else
            {
                RowDataItem.TestResult = "FAIL";
                //if (PassorFail == true)
                //    return true;
                return false;
            }              
            
            //return PassorFail;
        }

        public string CheckRule(string JsonDataInput, dynamic specParams, DataItem item)
        {
            try
            {
                SpecParamsContainer specParams2 = JsonConvert.DeserializeObject<SpecParamsContainer>(specParams);

                if (string.IsNullOrEmpty(JsonDataInput))
                {
                    return "FAIL";
                }
                JObject data = JObject.Parse(JsonDataInput);
                string ItemData = string.Empty;
                foreach (var param in specParams2.specParams)
                {
                    if (param.Name != null)
                    {
                        if (!data.ContainsKey(param.Name))
                        {
                            Logger.Warn($"{data} 不等於 {param.Name} 而未卡控");
                            continue;
                        }
                    }

                    Random random = new Random();

                    switch (param.SpecType)
                    {
                        case SpecType.Range:
                            double randomDouble = random.NextDouble();
                            // 將亂數映射到特定範圍，例如 0 到 10 之間的雙精度亂數
                            double minValue = param.MinLimit/*-0.05*/;
                            double maxValue = param.MaxLimit/*+0.05*/;
                            double scaledRandomDouble = minValue + (randomDouble * (maxValue - minValue));
                            // 只保留小數點第二位
                            double roundedRandomDouble = Math.Round(scaledRandomDouble, 2);
                            double value = (double)data[param.Name];
                            //double value = roundedRandomDouble;
                            ItemData += $"{param.Name}:{value}\n";
                            if (!(value >= param.MinLimit && value <= param.MaxLimit))
                            {
                                item.Value = ItemData.TrimEnd('\n');
                                return $"Check {param.Name} 超出了允許的範圍 ({param.MinLimit} 到 {param.MaxLimit}) .Result->Fail";
                            }
                            break;

                        case SpecType.Equal:
                            string str = (string)data[param.Name];
                            ItemData += $"{param.Name}:{str}\n";
                            if (!str.Equals(param.SpecValue))
                            {
                                item.Value = ItemData.TrimEnd('\n');
                                return $"{param.Name} 值不符合要求，應該等於 {param.SpecValue}.Result->Fail";
                            }
                            break;
                        case SpecType.GreaterThan:
                            double valueA = (double)data[param.Name];
                            ItemData += $"{param.Name}:{valueA}\n";
                            double valueB;
                            try
                            {
                                if (!double.TryParse(param.SpecValue, out valueB))
                                {
                                    valueB = (double)data[param.SpecValue];
                                }
                            }
                            catch (Exception e)
                            {
                                item.Value = ItemData.TrimEnd('\n');
                                return $"{param.SpecValue} Parse Date Error {e.Message}";
                            }

                            if (!(valueA > valueB))
                            {
                                item.Value = ItemData.TrimEnd('\n');
                                return $"Check {param.Name}({valueA}) > SpecValue({valueB}) .Result->Fail";
                            }
                            break;
                        case SpecType.LessThan:
                            double A = (double)data[param.Name];
                            ItemData += $"{param.Name}:{A}\n";
                            double B;
                            try
                            {
                                if (!double.TryParse(param.SpecValue, out B))
                                {
                                    B = (double)data[param.SpecValue];
                                }
                            }
                            catch (Exception e)
                            {
                                item.Value = ItemData.TrimEnd('\n');
                                return $"{param.SpecValue} Parse Date Error {e.Message}";
                            }

                            if (!(A < B))
                            {
                                item.Value = ItemData.TrimEnd('\n');
                                return $"Check {param.Name}({A}) < SpecValue({B}) .Result->Fail";
                            }
                            break;
                        default:
                            // 如果 SpecType 不是 Range 或 Equal，可以處理相應的情況
                            break;
                    }
                }
                item.Value = ItemData.TrimEnd('\n');
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

        public override bool Process()
        {
            Sleep(delay_param.DelayTime);
            
            return true;
        }
        public override bool PostProcess()
        {
            
            return true;

        }




        public class Delay
        {
           
            public int DelayTime { get; set; }
            

        }
        public class PASSFAIL_CHECKBOX : TypeConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[] { true, false });
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                {
                    return true;
                }
                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string strValue)
                {
                    if (bool.TryParse(strValue, out bool result))
                    {
                        return result;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid value. Please enter 'true' or 'false'.");
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}
