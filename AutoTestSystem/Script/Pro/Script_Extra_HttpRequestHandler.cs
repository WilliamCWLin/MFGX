using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.Diagnostics;
using System.ComponentModel;
using System.Drawing.Design;
using System.Threading;
using System.Windows.Forms;
using AutoTestSystem.Model;
using AutoTestSystem.Equipment.Teach;
using static AutoTestSystem.Equipment.Teach.Teach_IQ_Tuning;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using AutoTestSystem.Equipment.ControlDevice;
using DocumentFormat.OpenXml.Spreadsheet;
using Manufacture;
using System.Net;
using DocumentFormat.OpenXml;
using System.Net.Http;
using static AutoTestSystem.Script.Script_Extra_Generic_Command;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_HttpRequestHandler : Script_Extra_Base
    {
        private readonly Queue<string> dataQueue = new Queue<string>();

        public enum HttpMethodEnum
        {
            GET,
            POST,
        }

        string strOutData = string.Empty;

        [Category("HTTP"), Description("自訂顯示名稱")]
        public string P0_IP_Address { get; set; } = "10.0.0.2";

        [Category("HTTP"), Description("自訂顯示名稱")]
        public string P1_URL_PATH { get; set; } = "/api/v1";

        [Category("HTTP"), Description("自訂顯示名稱")]
        public HttpMethodEnum P2_HttpMethod { get; set; } = HttpMethodEnum.GET;

        [Category("HTTP"), Description("自訂顯示名稱"), Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        public string P3_Body { get; set; }

        [Category("HTTP"), Description("自訂顯示名稱"), Editor(typeof(RegexTestPatterns), typeof(UITypeEditor))]
        public string P4_ReadCheck { get; set; } = string.Empty;

        [Category("HTTP"), Description("自訂顯示名稱")]
        public int P5_TimeOut { get; set; } = 5000;

        public override void Dispose()
        {

        }
        public Script_Extra_HttpRequestHandler()
        {
            Description = "HttpRequest";
        }
        public override bool PreProcess()
        {
            dataQueue.Clear();
            strOutData = string.Empty;

            return true;
        }
        public bool SendCGICommand(string ip, string urlpath, string post_body, ref string output, HttpMethodEnum Httpmethod, int totalTimeOut)
        {
            try
            {
                string CGIURL = "http://" + ip + urlpath;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(CGIURL);
                httpWebRequest.Timeout = totalTimeOut;

                httpWebRequest.ContentType = "application/json";
                if (HttpMethodEnum.GET == Httpmethod)
                    httpWebRequest.Method = "Get";
                else
                    httpWebRequest.Method = "POST";

                if (HttpMethodEnum.GET != Httpmethod)
                {
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        streamWriter.Write(post_body);
                    }
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    output = result;

                    dataQueue.Enqueue(result); // 整筆數據放入隊列
                    
                }

                return true;
            }
            catch (Exception ex)
            {
                output = ex.Message;
                return false;
            }

        }
        public override bool Process(ref string output)
        {
            string ReadOutput = string.Empty;


            bool res = true;
            string data = string.Empty;
            if (!string.IsNullOrEmpty(P3_Body))
                data = ReplaceProp(P3_Body);
            try
            {
                string extractdat = string.Empty;
                if (SendCGICommand(P0_IP_Address, P1_URL_PATH, data, ref ReadOutput, P2_HttpMethod, P5_TimeOut))
                {
                    res = CHECK_PARSE(ReadOutput, ref extractdat);
                }
                    
                output = extractdat;
                strOutData = output;
                LogMessage("\n==============Extract Data==============\n" + strOutData);
                if (!string.IsNullOrEmpty(P4_ReadCheck))
                {
                    if (!res)
                    {
                        LogMessage("ReadCheck Fail");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                strOutData = output;
                LogMessage(ex.Message);
                return false;
            }
        }
        public bool IsValidJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            try
            {
                var obj = JsonConvert.DeserializeObject<List<RegexGroup>>(input);
                return obj != null;
            }
            catch (JsonReaderException)
            {
                return false;
            }
            catch (JsonSerializationException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }
        public bool InputRegexMatchCheck(string inputString, string json)
        {
            if (string.IsNullOrEmpty(json))
                return false;
            // 反序列化 JSON 來建立 RegexGroup 物件
            List<RegexGroup> regexGroups;
            try
            {
                // 嘗試解析 JSON
                regexGroups = JsonConvert.DeserializeObject<List<RegexGroup>>(json);
            }
            catch (Exception)
            {
                return inputString.Contains(json);
            }
            // 反序列化 JSON 來建立 RegexGroup 物件
            //List<RegexGroup> regexGroups = JsonConvert.DeserializeObject<List<RegexGroup>>(json);

            // 遍歷每個 RegexGroup
            foreach (var regexGroup in regexGroups)
            {
                try
                {
                    // 建立 Regex 實例
                    Regex regex = new Regex(regexGroup.Regex, RegexOptions.Multiline);

                    // 匹配輸入字串
                    var matches = regex.Matches(inputString);

                    // 如果這個正則表達式沒有匹配成功，返回 false
                    if (matches.Count == 0)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    // 可以加上適當的例外處理
                    Console.WriteLine($"Regex '{regexGroup.Regex}' 發生錯誤：{ex.Message}");
                    return false; // 任何正則表達式異常，返回 false
                }
            }

            // 如果所有的正則表達式都匹配成功，返回 true
            return true;
        }

        public Dictionary<string, object> ProcessInputString(string inputString, string json)
        {
            var resultDictionary = new Dictionary<string, object>();

            // 反序列化 JSON 來建立 RegexGroup 物件
            List<RegexGroup> regexGroups;
            try
            {
                // 嘗試解析 JSON
                regexGroups = JsonConvert.DeserializeObject<List<RegexGroup>>(json);
            }
            catch (Exception)
            {
                // JSON 解析失敗，直接檢查 inputString 是否包含 json 字串
                return resultDictionary;
            }

            // 遍歷每個 RegexGroup
            foreach (var regexGroup in regexGroups)
            {
                try
                {
                    // 建立 Regex 實例
                    Regex regex = new Regex(regexGroup.Regex, RegexOptions.Multiline);

                    // 匹配輸入字串
                    var matches = regex.Matches(inputString);

                    //foreach (Match match in matches)
                    {
                        foreach (var item in regexGroup.Items)
                        {
                            if (matches.Count > item.MatchIndex) // 確保 matches 內有該索引
                            {
                                Match match = matches[item.MatchIndex];

                                string groupValue = match.Groups.Count > item.GroupIndex
                                ? match.Groups[item.GroupIndex].Value
                                : string.Empty;
                                object result = GlobalNew.ParseAndConvert(groupValue, item.Name);


                                // 使用 Name 作為鍵，匹配結果作為值存入字典
                                if (!resultDictionary.ContainsKey(item.Name))
                                {
                                    string keyName = item.Name;
                                    if (item.Name.Contains("("))
                                    {
                                        // 使用正則表達式來去除格式部分，只保留名稱
                                        keyName = Regex.IsMatch(item.Name, @"^\w+\(([-+]?[0-9]*\.?[0-9]+|[a-zA-Z]+)\)([+\-*/]?\d*)$")
                                                     ? Regex.Match(item.Name, @"^\w+\(([-+]?[0-9]*\.?[0-9]+|[a-zA-Z]+)\)([+\-*/]?\d*)$").Groups[1].Value
                                                     : item.Name;  // 如果匹配，抓取括號內的數字或字母，否則保留原始名稱
                                    }
                                    if (item.Name.Contains("@"))
                                    {
                                        bool ismatch = true;
                                        string originInput = item.Name;
                                        string FinalKey = string.Empty;
                                        // 正規表達式來匹配 %任意文字%
                                        Regex regex2 = new Regex(@"@([^@]+)@");

                                        // 尋找匹配的 %%
                                        MatchCollection matches2 = regex2.Matches(originInput);

                                        if (matches2.Count > 0)
                                        {
                                            foreach (Match mt in matches2)
                                            {
                                                // 取得匹配的 key
                                                string key = mt.Groups[1].Value;

                                                if (resultDictionary.ContainsKey(key))
                                                {
                                                    string value = resultDictionary[key].ToString();
                                                    keyName = keyName.Replace(mt.Value, value);

                                                }
                                                else
                                                {
                                                    ismatch = false;
                                                }
                                            }

                                            if (!ismatch)
                                            {
                                                continue;
                                            }
                                        }


                                    }
                                    if (item.Name.Contains("{") && item.Name.Contains("}"))
                                    {
                                        keyName = FormatWithGroups(item.Name, match.Groups);
                                        if (keyName == "N/A")
                                        {
                                            continue;
                                        }
                                    }

                                    resultDictionary[keyName] = $"{result.ToString()}";

                                }
                            }
                            else
                            {
                                resultDictionary[item.Name] = $"{string.Empty}";
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    // 可以加上適當的例外處理
                    Console.WriteLine($"Regex '{regexGroup.Regex}' 發生錯誤：{ex.Message}");
                }
            }

            return resultDictionary;
        }
        static string FormatWithGroups(string template, GroupCollection groups)
        {
            // 使用正則表達式匹配模板中的佔位符（如 {0}、{1}）
            var regex = new Regex(@"\{(\d+)\}");
            var matches = regex.Matches(template);

            foreach (Match match in matches)
            {
                // 獲取佔位符中的索引（例如 {1} 中的 1）
                if (int.TryParse(match.Groups[1].Value, out int index))
                {
                    // 檢查索引是否在捕獲組的範圍內
                    if (index >= 0 && index < groups.Count)
                    {
                        // 替換佔位符為對應的捕獲組內容
                        template = template.Replace(match.Value, groups[index].Value);
                    }
                    else
                    {
                        // 如果索引超出範圍，替換為空字符串或默認值
                        template = template.Replace(match.Value, "N/A");
                    }
                }
            }

            return template;
        }
        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
            {
                LogMessage("Spec check passed. or Spec is empty");
                return true;
            }
            else
            {
                LogMessage($"{result}", MessageLevel.Error);
                return false;
            }

        }

        public bool SEND_COMMAND(dynamic DUTDevice, string cmd)
        {
            if (DUTDevice == null)
                return false;

            return DUTDevice.SEND(cmd);
        }

        public bool CHECK_PARSE(string input,ref string outputString)
        {
            string Data = input;
            bool isCheckSuccessful = false;
            //echo mode
            try
            {
                string CheckKey = string.Empty;
                bool isJsonCheck = IsValidJson(P4_ReadCheck);
                if (isJsonCheck)
                {
                    CheckKey = P4_ReadCheck;
                }
                else
                    CheckKey = ReplaceProp(P4_ReadCheck);

                

                DataStoreKey = Data;
                LogMessage(DataStoreKey);
                if (InputRegexMatchCheck(Data, CheckKey))
                {
                    isCheckSuccessful = true;
                    if (!isJsonCheck)//這段單純為了補抓可能的JSON格式所寫
                    {
                        if (Data.Contains("{"))
                        {
                            // **使用 sectionOutput 來計算 `{` 和 `}` 數量**
                            int leftBraceCount = Data.Count(c => c == '{');
                            int rightBraceCount = Data.Count(c => c == '}');

                            // **當 `{` 和 `}` 數量相等時才跳出**
                            if (leftBraceCount > 0 && leftBraceCount == rightBraceCount)
                            {
                                Data = ExtractJsonWithCheckKey(Data, CheckKey);
                                LogMessage($"Check Contains [{CheckKey}] Success.");
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(CheckKey)) // 如果讀取成功並且有指定匹配模式，繼續執行
                {
                    var result = ProcessInputString(Data, CheckKey);

                    if (result.Count > 0) // 若無法匹配模式，則認定失敗
                    {
                        outputString = JsonConvert.SerializeObject(result, Formatting.Indented);
                    }
                    else
                    {
                        if (!isJsonCheck)
                            outputString = Data;
                    }
                }
                else
                {
                    outputString = string.Empty;
                }

                return isCheckSuccessful;
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                return false;
            }
            

            //// 用於儲存輸出內容
            //StringBuilder outputBuilder = new StringBuilder();
            //string sectionOutput = string.Empty;
            //bool isCheckSuccessful = false;

            //// 設置計時器
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //string sectiontemp = string.Empty;

            //string CheckKey = string.Empty;
            //bool isJsonCheck = IsValidJson(P2_ReadCheck);
            //if (isJsonCheck)
            //{
            //    CheckKey = P2_ReadCheck;
            //}
            //else
            //    CheckKey = ReplaceProp(P2_ReadCheck);

            //while (stopwatch.ElapsedMilliseconds <= P3_TimeOut)
            //{
            //    string tempBuffer = string.Empty;

            //    // 嘗試從設備中讀取數據
            //    dutDevice?.READ(ref tempBuffer);

            //    if (!string.IsNullOrEmpty(tempBuffer))
            //    {
            //        sectiontemp += tempBuffer;
            //        outputBuilder.Append(tempBuffer); // 將新數據添加到輸出
            //        sectionOutput = outputBuilder.ToString();
            //    }
            //    //if (InputRegexMatchCheck(sectionOutput, CheckKey))check到直接Break
            //    //{
            //    //    isCheckSuccessful = true;
            //    //    break;
            //    //}
            //    if (InputRegexMatchCheck(sectionOutput, CheckKey))
            //    {
            //        isCheckSuccessful = true;
            //        if (!isJsonCheck && P0_Mode != CMD_ACTION.READSEND)//這段單純為了補抓可能的JSON格式所寫
            //        {
            //            if (sectionOutput.Contains("{"))
            //            {
            //                // **使用 sectionOutput 來計算 `{` 和 `}` 數量**
            //                int leftBraceCount = sectionOutput.Count(c => c == '{');
            //                int rightBraceCount = sectionOutput.Count(c => c == '}');

            //                // **當 `{` 和 `}` 數量相等時才跳出**
            //                if (leftBraceCount > 0 && leftBraceCount == rightBraceCount)
            //                {
            //                    sectionOutput = ExtractJsonWithCheckKey(sectionOutput, CheckKey);
            //                    LogMessage($"Check Contains [{CheckKey}] Success.");

            //                    break;
            //                }
            //            }
            //            else
            //            {
            //                LogMessage($"Check Contains [{CheckKey}] Success.");
            //                break;
            //            }

            //        }
            //        else
            //            break;
            //    }
            //    //偵測EXE是否已經結束不做多餘的Timeout等待
            //    if (dutDevice is ExeProcess)
            //    {
            //        if (dutDevice.CheckParam())
            //        {
            //            if (CheckKey == string.Empty)
            //                isCheckSuccessful = true;
            //            break;
            //        }
            //    }

            //    Thread.Sleep(3); // 短暫等待以減少 CPU 資源佔用
            //}

            //DataStoreKey = outputBuilder.ToString();

            //if (!string.IsNullOrEmpty(P2_ReadCheck)) // 如果讀取成功並且有指定匹配模式，繼續執行
            //{
            //    var result = ProcessInputString(sectionOutput, P2_ReadCheck);

            //    if (result.Count > 0) // 若無法匹配模式，則認定失敗
            //    {
            //        outputString = JsonConvert.SerializeObject(result, Formatting.Indented);
            //    }
            //    else
            //    {
            //        if (!isJsonCheck)
            //            outputString = sectionOutput;
            //    }
            //}

            //// 若超時，記錄超時訊息
            //if (!isCheckSuccessful)
            //{
            //    LogMessage($"Timeout after {P3_TimeOut} ms");
            //}

            //// 停止計時器並釋放資源
            //stopwatch.Stop();
            //outputBuilder.Clear();

            //return isCheckSuccessful;
        }

        static string ExtractJsonWithCheckKey(string input, string checkKey)
        {
            try
            {
                // 檢查輸入字串是否為空
                if (string.IsNullOrEmpty(input))
                {
                    throw new ArgumentException("Input string cannot be null or empty.", nameof(input));
                }

                // 檢查 checkKey 是否為空
                if (string.IsNullOrEmpty(checkKey))
                {
                    throw new ArgumentException("CheckKey cannot be null or empty.", nameof(checkKey));
                }

                int leftBraceCount = 0;
                int startIndex = -1;
                string resultJson = string.Empty; // 用來儲存符合條件的 JSON

                // 遍歷字串並檢查 JSON 區塊
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] == '{')
                    {
                        if (leftBraceCount == 0)
                        {
                            startIndex = i; // 記錄第一個 `{` 的索引
                        }
                        leftBraceCount++;
                    }
                    else if (input[i] == '}')
                    {
                        leftBraceCount--;
                        if (leftBraceCount == 0 && startIndex != -1)
                        {
                            // 從 startIndex 到當前 i 位置的子字串即為一個完整的 JSON
                            string json = input.Substring(startIndex, i - startIndex + 1);

                            // 檢查該 JSON 是否包含 CheckKey
                            if (json.Contains(checkKey))
                            {
                                resultJson = json; // 若包含則儲存這個 JSON
                                break; // 找到第一個符合條件的 JSON 即可停止
                            }

                            startIndex = -1; // 重置 startIndex，以便尋找下一個 JSON
                        }
                    }
                }

                // 如果沒有找到符合條件的 JSON，則回傳空字串
                if (string.IsNullOrEmpty(resultJson))
                {
                    throw new Exception($"No JSON containing the key '{checkKey}' was found.");
                }

                return resultJson;
            }
            catch (Exception ex)
            {
                // 捕獲並顯示錯誤
                Console.WriteLine("An error occurred: " + ex.Message);
                return string.Empty; // 發生錯誤時返回空字串
            }
        }

    }
}
