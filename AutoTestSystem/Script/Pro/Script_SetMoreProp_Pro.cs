
using AutoTestSystem.Base;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_SetMoreProp_Pro : Script_Extra_Base
    {       
        string jsonStr = string.Empty;

        [Category("Params"), Description("Type_Mode(0_Write_Memory/Calculation)")]
        public int Mode { get; set; } = 0;

        [Category("Params"), Description("Command Window(支援%%變數ex:Test=%Test1%，若添加多變數需要 \",\" 區隔 ex:Test=%Test1%,Test2=%Test2%,.....))"), Editor(typeof(CommandEditor), typeof(UITypeEditor))]
        public string SetComment { get; set; } = "Test = 123";

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            jsonStr = string.Empty;
            return true;
        }
        public override bool Process(ref string strDataOut)       
        {
            bool result = true;
            if (Mode == 0) //0_Write_Memory  and  Calculation
            {
                result = replaceWithProp(SetComment, ref strDataOut);
                jsonStr = strDataOut;
            }
            LogMessage($"SetParamData: {jsonStr}", MessageLevel.Info);

            return result;
        }
        public override bool PostProcess()
        {
            if (Spec != string.Empty && Spec != null)
            {
                string ret = string.Empty;
                ret = CheckRule(jsonStr, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Info);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;
        }

        public bool replaceWithProp(string input_string, ref string output_string)
        {
            Dictionary<string, string> outputdata = new Dictionary<string, string>();
            bool pass_fail = true;
            double output_value = 0;
            string originInput = input_string;
            string[] param_1 = null;
            string[] param_2 = null;
            string NGStatus = string.Empty;
            outputdata.Add("Input_String", originInput);

            // 正規表達式來匹配 %任意文字%
            Regex regex = new Regex(@"%([^%]+)%");

            // 尋找匹配的 %%
            MatchCollection matches = regex.Matches(input_string);

            // 迭代每個匹配
            foreach (Match match in matches)
            {
                // 取得匹配的 key
                string key = match.Groups[1].Value;

                if (key != null)
                {
                    // 使用 GetMoreProp 方法取得對應的 value 並進行替換
                    string value = PopMoreData(key);
                    input_string = input_string.Replace(match.Value, value);
                }
                else
                {
                    // 如果沒有匹配的 key，則移除佔位符
                    input_string = input_string.Replace(match.Value, "");
                }
            }

            if (input_string.Contains("\r\n") || input_string.Contains("\n") || input_string.Contains("\r"))
                input_string = input_string.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
            if (input_string.Contains(" "))
                input_string = input_string.Replace(" ", "");

            outputdata.Add("Replace_Input_String", input_string);
            if (input_string.Contains(","))
            {
                if (input_string.EndsWith(","))
                    input_string = input_string.TrimEnd(',');
                param_1 = input_string.Split(',');
                int key_comma = param_1.Count();
                for (int idex_comma = 0; idex_comma < key_comma; idex_comma++)
                {
                    if (param_1[idex_comma].Contains("="))
                    {
                        param_2 = param_1[idex_comma].Split('=');
                        if (!ContainsMathExpression(param_2[1]))
                            PushMoreData(param_2[0], param_2[1]);
                        else
                        {
                            if (IsValidExpression(param_2[1]))
                            {
                                try
                                {
                                    output_value = EvaluateExpression(param_2[1]);
                                    PushMoreData(param_2[0], output_value.ToString());
                                    if (double.IsInfinity(output_value) || double.IsNaN(output_value))
                                    {
                                        NGStatus = $"invalid value[Infinity] or [IsNaN]";
                                        pass_fail = false;
                                        outputdata.Add($"{param_2[0]}", PopMoreData(param_2[0]));
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    NGStatus = $"invalid value[{ex.Message}]";
                                    pass_fail = false;
                                    outputdata.Add($"{param_2[0]}", $"${ex.Message}");
                                    break;
                                }
                            }
                            else
                            {
                                //MessageBox.Show($"Valid value:{param_2[0]} value is {param_2[1]}", "Valid Expression", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Logger.Error($"invalid value:{param_2[0]} value is {param_2[1]}");
                                outputdata.Add($"{param_2[0]}", $"{ param_2[1]}");
                                NGStatus = $"invalid value";
                                pass_fail = false;
                                break;
                            }
                        }
                        outputdata.Add($"{param_2[0]}", PopMoreData(param_2[0]));
                    }
                }
            }
            else
            {
                if (input_string.Contains("="))
                {
                    param_2 = input_string.Split('=');
                    if (!ContainsMathExpression(param_2[1]))
                        PushMoreData(param_2[0], param_2[1]);
                    else
                    {
                        if (IsValidExpression(param_2[1]))
                        {
                            try
                            {
                                output_value = EvaluateExpression(param_2[1]);
                                PushMoreData(param_2[0], output_value.ToString());
                                outputdata.Add($"{param_2[0]}", PopMoreData(param_2[0]));
                                if (double.IsInfinity(output_value) || double.IsNaN(output_value))
                                {
                                    NGStatus = $"invalid value[Infinity] or [IsNaN]";
                                    outputdata.Add($"{param_2[0]}", PopMoreData(param_2[0]));
                                    pass_fail = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                NGStatus = $"invalid value[{ex.Message}]";
                                pass_fail = false;
                                outputdata.Add($"{param_2[0]}", $"${ex.Message}");
                            }
                        }
                        else
                        {
                            //MessageBox.Show($"Valid value:{param_2[0]} value is {param_2[1]}", "Valid Expression", MessageBoxButtons.OK,MessageBoxIcon.Error);
                            Logger.Error($"invalid value:{param_2[0]} value is {param_2[1]}");
                            outputdata.Add($"{param_2[0]}", $"{ param_2[1]}");
                            NGStatus = $"invalid value";
                            pass_fail = false;
                        }
                    }
                }
                else
                {
                    Logger.Error($"input_string Type is not Defined");
                    NGStatus = $"input_string Type is not Defined";
                    pass_fail = false;
                }
            }

            if (pass_fail)
                outputdata.Add("SetParam", "OK");
            else
                outputdata.Add("SetParam", $"Fail[{NGStatus}]");
            output_string = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
            return pass_fail;
        }

        static bool ContainsMathExpression(string input)
        {
            // 正則表達式匹配簡單的運算式子
            string pattern = @"[\d\s]*[\+\-\*\/][\d\s]*";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }

        static bool IsValidExpression(string input)
        { // 正則表達式匹配有效的運算方程式 
            string pattern = @"^\s*[\+\-]?\s*\d+(\.\d+)?\s*([\+\-\*/\^]\s*[\+\-]?\s*\d+(\.\d+)?\s*)*$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }
        static double EvaluateExpression(string expression)
        {
            // 使用正則表達式處理次方運算，先進行次方運算再用DataTable計算
            string processedExpression = ProcessExponentiation(expression);
            // 使用 DataTable 計算簡單的數學表達式
            var table = new DataTable();
            var result = table.Compute(processedExpression, string.Empty);
            return Convert.ToDouble(result);
        }

        static string ProcessExponentiation(string expression)
        {
            // 正則表達式查找次方運算
            Regex regex = new Regex(@"\d+(\.\d+)?\s*\^\s*\d+(\.\d+)?");
            Match match;
            // 不斷查找並替換次方運算
            while ((match = regex.Match(expression)).Success)
            {
                // 提取匹配到的次方運算子
                string exp = match.Value;
                // 分割次方運算式
                string[] numbers = exp.Split('^');
                double baseNumber = Convert.ToDouble(numbers[0]);
                double exponent = Convert.ToDouble(numbers[1]);
                // 計算次方
                double powerResult = Math.Pow(baseNumber, exponent);
                // 用次方運算結果替換原始表達式
                expression = expression.Replace(exp, powerResult.ToString());
            }
            return expression;
        }

    }
}
