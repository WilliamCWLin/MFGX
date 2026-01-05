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
using DocumentFormat.OpenXml.Drawing.Diagrams;
using System.IO.Ports;
using MvCamCtrl.NET;
using System.Web.UI.WebControls;
using System.Windows.Interop;
using NAudio.CoreAudioApi;

namespace AutoTestSystem.Script
{

    internal class Script_Extra_Generic_Command : Script_Extra_Base
    {
        private string SendStrCmd;
        public string SendorgCmd;
        public enum CMD_ACTION
        {
            SEND,
            READ,
            SENDREAD,
            READSEND,
            SEND_HEX,
            CLEAR
        }

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(Extra_DevList))]
        public string DeviceSel { get; set; } = "";

        string strOutData = string.Empty;

        [Category("Command"), Description("READ支援SerialPort\r\nREADSEND支援SerialPort\r\nSEND支援SerialPort、OneTimeExe\r\nSENDREAD支援SerialPort、OneTimeFile、Echo、OneTimeExe")]
        public CMD_ACTION P0_Mode { get; set; } = CMD_ACTION.SENDREAD;

        [Category("Command"), Description("支援用%%方式做變數值取代"), TypeConverter(typeof(CommandConverter))]
        //public string P1_Send { get; set; } = string.Empty;
        public string P1_Send {
            get
            {
                if (SendStrCmd == null)
                    return string.Empty;
                else
                    return SendStrCmd;
            }
            set
            {
                SendStrCmd = ExtractASCII_Key(value);
                SendorgCmd = SendStrCmd;
            }
        }

        [Category("Command"), Description("自訂顯示名稱"), Editor(typeof(RegexTestPatterns), typeof(UITypeEditor))]
        public string P2_ReadCheck { get; set; } = string.Empty;

        [Category("Command"), Description("自訂顯示名稱")]
        public int P3_TimeOut { get; set; } = 10000;
        [Category("Command"), Description("自訂顯示名稱")]
        public string P4_NewLine { get; set; } = "";
        //[Category("Command"), Description("自訂顯示名稱")]
        //public bool P4_BypassTimeout { get; set; } = false;

        public override void Dispose()
        {

        }

        public override bool PreProcess()
        {
            strOutData = string.Empty;
            switch (DeviceSel)
            {
                case "Echo":
                case "OneTimeFile":
                    if (P0_Mode != CMD_ACTION.SENDREAD)
                    {
                        LogMessage("Echo and OneTimeFile , only support SENDREAD mode.");
                        return false;
                    }

                    break;
                case "OneTimeExe":
                    if (P0_Mode != CMD_ACTION.SENDREAD && P0_Mode != CMD_ACTION.SEND)
                    {
                        LogMessage("OneTimeExe only support SENDREAD and SEND mode.");
                        return false;
                    }

                    break;
            }
            return true;
        }

        public override bool Process(ref string output)
        {
            string ReadOutput = string.Empty;
            string SendCmd = ReplaceProp(P1_Send);

            dynamic Device = null;
            bool res = true;

            switch (DeviceSel)
            {
                case "Echo":
                    ReadOutput = SendCmd;
                    LogMessage("Enter Echo Mode");

                    break;
                case "OneTimeFile":
                    Device = new FILE();
                    LogMessage("Enter OneTimeFile Mode");

                    break;
                case "OneTimeExe":
                    Device = new ExeProcess();
                    LogMessage("Enter OneTimeExe Mode");

                    break;

                default:
                    if (GlobalNew.Devices.ContainsKey(DeviceSel) == false)
                    {
                        string ref_Dev = FindMultiDeviceName(DeviceSel);

                        if (GlobalNew.Devices.ContainsKey(ref_Dev))
                        {
                            LogMessage($"FindMultiDevice({ref_Dev})");
                            Device = GlobalNew.Devices[ref_Dev];
                        }
                        else
                        {
                            LogMessage($"{ref_Dev} not found multidevice");
                            return false;
                        }
                    }
                    else
                        Device = GlobalNew.Devices[DeviceSel];

                    LogMessage("Enter ByDeviceSel Mode");
                    break;
            }

            try
            {
                Device?.SetTimeout(P3_TimeOut);
                switch (P0_Mode)
                {
                    case CMD_ACTION.SEND:
                        Device?.Clear();                       
                        return SEND_COMMAND(Device, SendCmd);

                    case CMD_ACTION.READSEND:
                        res = READ_CHECK_PARSE(Device, ref ReadOutput);
                        SEND_COMMAND(Device, SendCmd);

                        break;
                    case CMD_ACTION.SENDREAD:
                        Device?.Clear();
                        SEND_COMMAND(Device, SendCmd);
                        res = READ_CHECK_PARSE(Device, ref ReadOutput);

                        break;
                    case CMD_ACTION.READ:
                        res = READ_CHECK_PARSE(Device, ref ReadOutput);

                        break;

                    case CMD_ACTION.SEND_HEX:
                        Device?.Clear();
                        SEND_HEX(Device, SendCmd);
                        res = READ_CHECK_PARSE(Device, ref ReadOutput);

                        break;
                    case CMD_ACTION.CLEAR:
                        Device?.Clear();
                        LogMessage("Device Clear");
                        return true;
                }

                //Device?.Clear();

                switch (DeviceSel)
                {
                    case "OneTimeFile":
                    case "OneTimeExe":
                        if (P0_Mode == CMD_ACTION.SENDREAD)
                        {
                            
                            Device?.Dispose();
                            LogMessage("Device Dispose");
                        }
                        break;
                }
                output = ReadOutput;
                strOutData = output;
                LogMessage("\n==============Extract Data==============\n" + CleanMessage(strOutData));
                if (!string.IsNullOrEmpty(P2_ReadCheck))
                {
                    if (!res)
                    {
                        LogMessage("Timeout or Read Fail.return false");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Device?.Clear();
                switch (DeviceSel)
                {
                    case "OneTimeFile":
                    case "OneTimeExe":
                        if (P0_Mode == CMD_ACTION.SENDREAD)
                        {
                            Device?.Dispose();
                            LogMessage("Device Dispose");
                        }

                        break;
                }
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

            if (P4_NewLine == "CRLF")
                return DUTDevice.SEND(cmd + "\r\n");
            else if (P4_NewLine == "CR")
                return DUTDevice.SEND(cmd + "\r");
            else if (P4_NewLine == "LF")
                return DUTDevice.SEND(cmd + "\n");
            else
                return DUTDevice.SEND(cmd);
        }
        public bool SEND_HEX(dynamic DUTDevice, string cmd)
        {
            return DUTDevice.SEND(ConvertHexStringToByteArray(cmd));
        }
        // 將十六進位字串轉換為byte[]
        private byte[] ConvertHexStringToByteArray(string hexString)
        {
            // 清除字串中的空格和0x
            hexString = hexString.Replace("0x", "").Replace(" ", "").ToUpper();

            // 檢查字串長度是否是偶數
            if (hexString.Length % 2 != 0)
            {
                LogMessage("Invalid hex string length.");
                return null;
            }

            try
            {
                // 將十六進位字串轉換為byte[]
                byte[] byteArray = Enumerable.Range(0, hexString.Length)
                                             .Where(x => x % 2 == 0)
                                             .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                                             .ToArray();

                return byteArray;
            }
            catch (FormatException)
            {
                LogMessage("Invalid hex string format.");
                return null;
            }
        }
        public bool READ_CHECK_PARSE(dynamic dutDevice, ref string outputString)
        {
            string ReplaceReadCheck = P2_ReadCheck;
            //echo mode
            if (dutDevice == null && DeviceSel == "Echo")
            {
                DataStoreKey = outputString;
                LogMessage(DataStoreKey);
                if (!string.IsNullOrEmpty(ReplaceReadCheck)) // 如果讀取成功並且有指定匹配模式，繼續執行
                {
                    var result = ProcessInputString(outputString, ReplaceReadCheck);

                    if (result.Count > 0) // 若無法匹配模式，則認定失敗
                    {
                        outputString = JsonConvert.SerializeObject(result, Formatting.Indented);
                    }
                    else
                    {
                        outputString = string.Empty;
                    }
                }
                else
                {
                    outputString = string.Empty;
                }
                LogMessage("\n==============PopQueue Data==============\n" + CleanMessage(DataStoreKey));
                return true;
            }

            // 用於儲存輸出內容
            StringBuilder outputBuilder = new StringBuilder();
            string sectionOutput = string.Empty;
            bool isCheckSuccessful = false;

            // 設置計時器
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string sectiontemp = string.Empty;

            string CheckKey = string.Empty;
            bool isJsonCheck = IsValidJson(ReplaceReadCheck);
            if (isJsonCheck)
            {
                CheckKey = ReplaceReadCheck;
            }
            else
                CheckKey = ReplaceProp(ReplaceReadCheck);

            while (stopwatch.ElapsedMilliseconds <= P3_TimeOut)
            {
                string tempBuffer = string.Empty;

                // 嘗試從設備中讀取數據
                dutDevice?.READ(ref tempBuffer);

                if (!string.IsNullOrEmpty(tempBuffer))
                {

                    sectiontemp += tempBuffer;
                    outputBuilder.Append(tempBuffer); // 將新數據添加到輸出
                    sectionOutput = outputBuilder.ToString();
                    //if (sectiontemp.Length > 500)
                    //{
                    //    //logBuffer.Add(sectiontemp); // 將新數據添加到緩衝區
                    //    LogMessage(sectiontemp);
                    //    sectiontemp = string.Empty;
                    //}

                    // 當緩衝區達到指定大小時，寫入日誌並清空緩衝區
                    //if (logBuffer.Count >= logBufferSize)
                    //{
                    //    LogMessage(string.Join(Environment.NewLine, logBuffer));
                    //    logBuffer.Clear();

                    //}

                }
                //if (InputRegexMatchCheck(sectionOutput, CheckKey))check到直接Break
                //{
                //    isCheckSuccessful = true;
                //    break;
                //}
                if (isJsonCheck)
                {
                    if (InputRegexMatchCheck(sectionOutput, CheckKey))
                    {
                        isCheckSuccessful = true;
                        break;
                    }
                }
                else
                {
                    if (CheckKey != string.Empty)
                    {
                        if (sectionOutput.Contains(CheckKey))
                        {
                            isCheckSuccessful = true;
                            if (P0_Mode != CMD_ACTION.READSEND || P0_Mode != CMD_ACTION.READ)//這段單純為了補抓可能的JSON格式所寫
                            {
                                if (sectionOutput.Contains("{"))
                                {
                                    // **使用 sectionOutput 來計算 `{` 和 `}` 數量**
                                    int leftBraceCount = sectionOutput.Count(c => c == '{');
                                    int rightBraceCount = sectionOutput.Count(c => c == '}');

                                    // **當 `{` 和 `}` 數量相等時才跳出**
                                    if (leftBraceCount > 0 && leftBraceCount == rightBraceCount)
                                    {
                                        sectionOutput = ExtractJsonWithCheckKey(sectionOutput, CheckKey);
                                        LogMessage($"Check Contains [{CheckKey}] Success.");

                                        break;
                                    }
                                }
                                else
                                {
                                    LogMessage($"Check Contains [{CheckKey}] Success.");
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                }
                //偵測EXE是否已經結束不做多餘的Timeout等待
                if (dutDevice is ExeProcess)
                {
                    if (dutDevice.CheckParam())//用來判定是否結束
                    {
                        if (CheckKey == string.Empty)
                            isCheckSuccessful = true;
                        break;
                    }
                }
                if (dutDevice is FILE)//File採用一次讀全部Read回來有數據就已經檢測結束
                {
                    if (sectionOutput != string.Empty)
                    {
                        if (CheckKey == string.Empty)
                            isCheckSuccessful = true;
                        break;
                    }


                }
                //Thread.Sleep(1);

            }

            DataStoreKey = outputBuilder.ToString();

            LogMessage("\n==============PopQueue Data==============\n" + CleanMessage(DataStoreKey));

            if (!string.IsNullOrEmpty(ReplaceReadCheck)) // 如果讀取成功並且有指定匹配模式，繼續執行
            {
                var result = ProcessInputString(sectionOutput, ReplaceReadCheck);

                if (result.Count > 0) // 若無法匹配模式，則認定失敗
                {
                    outputString = JsonConvert.SerializeObject(result, Formatting.Indented);
                }
                else
                {
                    if (!isJsonCheck)
                        outputString = sectionOutput;
                }
            }

            // 若超時，記錄超時訊息
            if (!isCheckSuccessful)
            {
                LogMessage($"Timeout after {P3_TimeOut} ms");
            }

            // 停止計時器並釋放資源
            stopwatch.Stop();
            outputBuilder.Clear();

            return isCheckSuccessful;
        }
        string CleanMessage(string msg)
        {
            return new string(msg.Where(c => c >= 0x20 || c == '\n' || c == '\r' || c == '\t').ToArray());
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

       public string ExtractASCII_Key(string cmd)
        {
            if (P0_Mode == CMD_ACTION.SEND_HEX) {
                string MatchRet = string.Empty;
                if (!string.IsNullOrEmpty(cmd))
                {
                    string[] parts = cmd.Split(new string[] { "||" }, StringSplitOptions.None);
                    foreach (string part in parts)
                    {
                        if (part.StartsWith("Hex:"))
                        {
                            MatchRet = $"{part.Substring(4)}";
                            cmd = $"{SendorgCmd}{MatchRet}";
                        }
                    }
                }
            }
            return cmd;
        }
    }

    public class RegexTestPatterns : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            dynamic currentObject = context.Instance;

            string Sample = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(currentObject.DataStoreKey))
                {
                    Sample = currentObject.DataStoreKey;
                }
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
                return value;
            }

            using (RegexTesterForm Regexform = new RegexTesterForm(value?.ToString(), Sample))
            {
                var result = Regexform.ShowDialog();

                if (result == DialogResult.OK)
                {

                    return Regexform.JsonData;
                }
                else
                {
                    return value;
                }
            }
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
    public class RegexCheckPatterns : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            dynamic currentObject = context.Instance;

            string Sample = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(currentObject.DataStoreKey))
                {
                    Sample = currentObject.DataStoreKey;
                }
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
                return value;
            }

            using (RegexTesterForm Regexform = new RegexTesterForm(value?.ToString(), Sample, 1))
            {
                var result = Regexform.ShowDialog();

                if (result == DialogResult.OK)
                {

                    return Regexform.JsonData;
                }
                else
                {
                    return value;
                }
            }
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }

    public class CommandConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (value is string)
            {
                return value;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return value.ToString();
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            dynamic currentObject = context.Instance;

            string Sample = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(currentObject.DeviceSel))
                {                  
                    return new StandardValuesCollection(new string[] { });
                }


                if (GlobalNew.Devices.ContainsKey(currentObject.DeviceSel) == false)
                {
                    if (currentObject.DeviceSel == "OneTimeFile")
                    {
                        var FileCmdList = JsonConvert.DeserializeObject<Dictionary<string, string>>("{\"ReadAllText example.txt\":\"\",\"Move src.txt dst.txt\":\"\",\"Copy src.txt dst.txt\":\"\",\"Delete src.txt\":\"\",\"Exists src.txt\":\"\"}\r\n");

                        var FilekeysArray = FileCmdList.Keys.ToArray();

                        return new StandardValuesCollection(FilekeysArray);
                    }
                    else
                    {
                        string multiDeviceTable = string.Empty;
                        foreach (var value in GlobalNew.Devices.Values)
                        {

                            if (value is DUT_BASE)
                            {
                                if (((DUT_BASE)(value)).Enable)
                                {
                                    multiDeviceTable = ((DUT_BASE)(value)).MultiDeviceTable;
                                    break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(multiDeviceTable))
                        {
                            // 解析 JSON 字符串
                            JArray data = JArray.Parse(multiDeviceTable);

                            // 找到 DeviceObject 欄中的值是否在 GlobalNew.Devices 中，並將對應的 SharedName 值列到 hwListKeys 中
                            foreach (var item in data)
                            {
                                string deviceObject = (string)item["DeviceObject"];
                                if (GlobalNew.Devices.ContainsKey(deviceObject))
                                {
                                    if (GlobalNew.Devices[deviceObject] is DUT_BASE)
                                    {

                                        dynamic DUT_A = (dynamic)GlobalNew.Devices[deviceObject];

                                        Sample = DUT_A.CommandTable;
                                        if (string.IsNullOrEmpty(Sample))
                                            return new StandardValuesCollection(new string[] { });


                                        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Sample);

                                        var keysArray = dictionary.Keys.ToArray();

                                        return new StandardValuesCollection(keysArray);
                                    }
                                }
                            }
                        }
                        else
                        {
                            return new StandardValuesCollection(new string[] { });
                        }                       
                    }
                        //return new StandardValuesCollection(new string[] { });
                }

                dynamic DUT = (dynamic)GlobalNew.Devices[currentObject.DeviceSel];

                if (currentObject.P0_Mode == AutoTestSystem.Script.Script_Extra_Generic_Command.CMD_ACTION.SEND_HEX)
                {
                    List<AsciiEntry> CreatTable = new List<AsciiEntry>();
                    List<string> AsciiTable = new List<string>();
                    for (int i = 0; i < 256; i++)
                    {
                        //char character = (char)i;
                        //string displayChar = char.IsControl(character) ? "NA" : character.ToString();
                        CreatTable.Add(new AsciiEntry
                        {
                            Dec = i,
                            Hex = i.ToString("X2"),
                            Char = (char)i
                        });
                    }
                    foreach (var entry in CreatTable)
                    {
                        AsciiTable.Add($"Dec:KEY_{entry.Dec}||Hex:{entry.Hex}||Char:{entry.Char}");
                    }

                    return new StandardValuesCollection(AsciiTable.ToArray());
                }
                else
                {
                    Sample = DUT.CommandTable;

                    if (string.IsNullOrEmpty(Sample))
                        return new StandardValuesCollection(new string[] { });


                    var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Sample);

                    var keysArray = dictionary.Keys.ToArray();

                    return new StandardValuesCollection(keysArray);
                }
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
                return new StandardValuesCollection(new string[] { });
            }
            catch (Exception)
            {
                return new StandardValuesCollection(new string[] { });
            }
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
    class AsciiEntry
    {
        public int Dec { get; set; }
        public string Hex { get; set; }
        public char Char { get; set; }
    }
}
