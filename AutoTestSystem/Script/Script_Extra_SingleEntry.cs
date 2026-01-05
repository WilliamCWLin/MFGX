using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using static AutoTestSystem.BLL.Bd;
using System.ComponentModel;
using Manufacture;
using System.Drawing.Design;

namespace AutoTestSystem.Script
{
    
    internal class Script_Extra_SingleEntry : Script_Extra_Base
    {
        [Category("Common Parameters"), Description("自訂顯示名稱"), Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        public string ParamIN { get; set; }

        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strJsonResult = string.Empty;
        SingleEntry singleEntry = null;
        private static string strDllpath = string.Empty;
        private static string strFunctionName = string.Empty;
        private static string strPin = string.Empty;
        StringBuilder strbuilderResult;
        [DllImport("SE_IVS.dll", EntryPoint = "StartAction", CallingConvention = CallingConvention.StdCall)]
        public static extern void API_Entry(string parm, StringBuilder result);
        public override bool PreProcess(string ActionItem, string Paraminput)
        {
            // 解json
            strActItem = ActionItem;
            strParam = Paraminput;
            try 
            {
                singleEntry = JsonConvert.DeserializeObject<SingleEntry>(strParam);
            }
            catch (Exception ex)
            {
                Logger.Info($"解析SingleEntry控件輸入異常: {ex.Message}");
            }
            // 初始化變數
            strDllpath = singleEntry.DllPath;
            strFunctionName = singleEntry.Api;
            strPin = singleEntry.Pin;
            strbuilderResult = new StringBuilder(4096);
            return true;
        }
        public override bool Process()
        {
            // 執行
            //try
            //{
            //    API_Entry(strPin, strbuilderResult);
            //}
            //catch (Exception ex)
            //{
            //    Logger.Info($"API呼叫異常: {ex.Message}");
            //    return false;
            //}
            //ResultToJson();
            LogMessage(ParamIN);
            return true;
        }
        public override bool PostProcess()
        {
            // Check Ruler
            try
            {
                string s = Spec;
            }
            catch (Exception ex)
            {
                Logger.Info($"CheckRuler異常: {ex.Message}");
                return false;
            }
            return true;

        }
        public class SingleEntry
        {
            public string DllPath { get; set; }
            public string Pin { get; set; }
            public string Api { get; set; }
        }
        public bool ResultToJson()
        {
            try 
            {
                string[] strResult = null;
                try
                {
                    strResult = strbuilderResult.ToString().Split('\n');
                }
                catch (Exception ex)
                {
                    Logger.Info($"Result切割異常: {ex.Message}");
                }
                ParsingStatus status = ParsingStatus.None;
                Dictionary<string, string> Json = new Dictionary<string, string>();
                string NameInSuareBreakets = string.Empty;
                foreach (string str in strResult)
                {
                    switch (status)
                    {
                        case ParsingStatus.None:
                            if (str.Length > 0 && str[0] == '[' && str[str.Length - 1] == ']')
                            {   // 找到第一組[]
                                try
                                {
                                    NameInSuareBreakets = str.Substring(1, str.Length - 2);
                                    status = ParsingStatus.FindSuareBrackets;
                                }
                                catch (Exception ex)
                                {
                                    Logger.Info($"None:擷取[]中資訊異常: {ex.Message}");
                                }
                            }
                            break;
                        case ParsingStatus.FindSuareBrackets:
                            // try catch
                            if (str.Length > 0 && str[0] == '[' && str[str.Length - 1] == ']')
                            {   // 找到下一組[]
                                try
                                {
                                    NameInSuareBreakets = str.Substring(1, str.Length - 2);
                                    status = ParsingStatus.FindSuareBrackets;
                                }
                                catch (Exception ex)
                                {
                                    Logger.Info($"FindSuareBrackets:擷取[]中資訊異常: {ex.Message}");
                                }
                            }
                            else if (str.Contains("="))
                            {
                                try
                                {
                                    // 找到第一組的key & value
                                    string key = str.Split('=')[0].Trim();
                                    string value = str.Split('=')[1].Trim();
                                    string object_key = NameInSuareBreakets+ "-" + key;
                                    Json.Add(object_key, value);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Info($"FindSuareBrackets:分割Key&Value異常: {ex.Message}");
                                }
                            }
                            break;
                    }
                }
                string v = JsonConvert.SerializeObject(Json, Formatting.Indented);
                strJsonResult = v;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Info($"ResultToJson function 異常: {ex.Message}");
                return false;
            }
        }

        //public bool ResultToJson()  // 輸出為多層json格式版本
        //{
        //    try
        //    {
        //        string[] strResult = null;
        //        try
        //        {
        //            strResult = strbuilderResult.ToString().Split('\n');
        //        }
        //        catch (Exception ex)
        //        {
        //            // 詳細原因寫上去
        //            Logger.Info($"Result切割異常: {ex.Message}");
        //        }
        //        ParsingStatus status = ParsingStatus.None;
        //        Dictionary<string, object> Json = new Dictionary<string, object>();
        //        Dictionary<string, string> subJson = new Dictionary<string, string>();
        //        string NameInSuareBreakets = string.Empty;
        //        foreach (string str in strResult)
        //        {
        //            switch (status)
        //            {
        //                case ParsingStatus.None:
        //                    if (str.Length > 0 && str[0] == '[' && str[str.Length - 1] == ']')
        //                    {   // 找到第一組[]
        //                        try
        //                        {
        //                            NameInSuareBreakets = str.Substring(1, str.Length - 2);
        //                            status = ParsingStatus.FindSuareBrackets;
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            // 詳細原因寫上去
        //                            Logger.Info($"None:擷取[]中資訊異常: {ex.Message}");
        //                        }
        //                    }
        //                    break;
        //                case ParsingStatus.FindSuareBrackets:
        //                    // try catch
        //                    if (str.Length > 0 && str[0] == '[' && str[str.Length - 1] == ']')
        //                    {   // 找到下一組[]
        //                        try
        //                        {
        //                            // 插入第一組
        //                            Json.Add(NameInSuareBreakets, new Dictionary<string, string>(subJson));
        //                            subJson.Clear();
        //                            NameInSuareBreakets = str.Substring(1, str.Length - 2);
        //                            status = ParsingStatus.FindSuareBrackets;
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            Logger.Info($"FindSuareBrackets:擷取[]中資訊異常: {ex.Message}");
        //                        }
        //                    }
        //                    else if (str.Contains("="))
        //                    {
        //                        try
        //                        {
        //                            // 找到第一組的key & value
        //                            string key = str.Split('=')[0].Trim();
        //                            string value = str.Split('=')[1].Trim();
        //                            subJson.Add(key, value);
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            Logger.Info($"FindSuareBrackets:分割Key&Value異常: {ex.Message}");
        //                        }
        //                    }
        //                    break;

        //            }
        //        }
        //        string v = JsonConvert.SerializeObject(Json, Formatting.Indented);
        //        strJsonResult = v;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Info($"ResultToJson function 異常: {ex.Message}");
        //        return false;
        //    }
        //}
        enum ParsingStatus : int
        {
            None = 0,
            FindSuareBrackets = 1,
            FindKeyAndValue = 2,
        }
    }
}
