using AutoTestSystem.Model;
using AutoTestSystem.Script;
using Manufacture;
using MvCamCtrl.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using ZXing;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Base
{
    public abstract class ScriptBase : Manufacture.TerminalNode
    {
        private DUT_BASE Dutmp;
        private bool Log_ON_OFF = false;
        //private Dictionary<string, string> Check_NetworkPath = new Dictionary<string, string>();

        private Dictionary<string, string> check_Samename = new Dictionary<string, string>();
        [JsonIgnore]
        public DataItem RowDataItem;

        [Category("Base"), Description("自訂顯示名稱")]
        public string ErrorCode { get; set; } = "Undefined";

        //private string _SpecRule;
        [Category("Base"), Description("自訂顯示名稱"), Editor(typeof(SpecEditor), typeof(UITypeEditor))]
        public string Spec { get; set; }

        //public string Spec
        //{
        //    get { return _SpecRule; }
        //    set { AttributeLevel(value, ref _SpecRule, (int)User_Level.RD); }
        //}


        [Category("Base"), Description("RetryTimes")]
        public int RetryTimes { get; set; }

        [Category("Base"), Description("自訂顯示名稱")]
        public string Prefix { get; set; }

        [JsonIgnore]
        [Browsable(false)]
        [Category("Base"), Description("暫存測項輸出資料，設定KeyName")]
        public string DataStoreKey { get; set; }

        [JsonIgnore]
        [Browsable(false)]
        public DUT_BASE HandleDevice
        {
            get { return Dutmp; }
            set { Dutmp = value; }
        }

        public ScriptBase()
        {
            RowDataItem = new DataItem();
            RetryTimes = 0;
            Description = GetType().Name;
            Prefix = "";
            Spec = "";
            DataStoreKey = string.Empty;

        }

        public string FindMultiDeviceName(string dev)
        {
            if (dev.Contains("@"))
            {
                string devName = dev.Replace("@", "");

                if (!string.IsNullOrEmpty(HandleDevice.MultiDeviceTable))
                {
                    // 解析 JSON 字符串
                    JArray data = JArray.Parse(HandleDevice.MultiDeviceTable);

                    foreach (var item in data)
                    {
                        string deviceObject = (string)item["SharedName"];
                        if (deviceObject == devName)
                        {
                            string realName = (string)item["DeviceObject"];
                            return realName;
                        }
                    }

                    LogMessage($"DeviceSel is not found");
                }
            }

            return dev;           
        }
        public virtual bool Action(object o)
        {
            return true;
        }
        public bool SetDataDestination(DUT_BASE obj)
        {
            if (obj != null)
            {
                //if (Dutmp == null)
                Dutmp = obj;

                return true;
            }
            else
            {
                return false;
            }
        }
        public DataCollection GetSummaryWorkSheet()
        {
            if (GlobalNew.g_datacollection != null)
            {
                return GlobalNew.g_datacollection;
            }
            else
            {
                return null;
            }
        }
        public string ReplaceProp(string input_string)
        {
            string originInput = input_string;
            // 正規表達式來匹配 %任意文字%
            Regex regex = new Regex(@"%([^%]+)%");

            // 尋找匹配的 %%
            MatchCollection matches = regex.Matches(input_string);

            // 迭代每個匹配
            foreach (Match match in matches)
            {
                // 取得匹配的 key
                string key = match.Groups[1].Value;

                // 檢查是否為特殊的 Time 鍵
                if (key.StartsWith("NowTime"))
                {
                    // 如果是標準的 %Time% 佔位符
                    if (key.Length == 7)
                    {
                        // 使用標準格式替換 %Time%
                        input_string = input_string.Replace(match.Value, DateTime.Now.ToString("yyyyMMddHHmmss"));
                    }
                    else
                    {
                        // 處理自定義格式的時間佔位符
                        // 從 key 中提取出時間格式
                        string customFormat = key.Substring(7);
                        // 替換自定義格式的時間佔位符
                        input_string = input_string.Replace(match.Value, DateTime.Now.ToString(customFormat));
                    }
                }
                else if (key.StartsWith("UTCTime"))
                {
                    // 如果是標準的 %UTCTime% 佔位符
                    if (key.Length == 7)
                    {
                        // 使用標準格式替換 %UTCTime%
                        input_string = input_string.Replace(match.Value, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
                    }
                    else
                    {
                        // 處理自定義格式的時間佔位符
                        // 從 key 中提取出時間格式
                        string customFormat = key.Substring(7);
                        // 替換自定義格式的時間佔位符
                        input_string = input_string.Replace(match.Value, DateTime.UtcNow.ToString(customFormat));
                    }
                }
                else if (key.StartsWith("ALLMESData"))
                {
                    input_string = input_string.Replace(match.Value, PopALLMESData());
                    
                }
                else if (key.StartsWith("RetryCount"))
                {
                    input_string = input_string.Replace(match.Value, this.RetryCount.ToString("D2"));

                }
                else if (key.StartsWith("Description"))
                {
                    input_string = input_string.Replace(match.Value, $"{this.Description}");

                }
                else if (key.StartsWith("ResultError"))
                {
                    string Failitem = PopMoreData("Failitem");
                    if (Failitem == string.Empty)
                        input_string = input_string.Replace(match.Value, "OK");
                    else
                        input_string = input_string.Replace(match.Value, $"NG;{Failitem}");
                }
                else if (Dutmp != null)
                {
                    // 使用 GetMoreProp 方法取得對應的 value 並進行替換
                    string value = Dutmp.DataCollection.GetMoreProp(key);
                    input_string = input_string.Replace(match.Value, value);
                }
                else
                {
                    // 如果沒有匹配的 key，則移除佔位符
                    input_string = input_string.Replace(match.Value, "");
                }
            }

            // ✅ 只有在有變化時才記錄 log
            if (!input_string.Equals(originInput))
            {
                LogMessage($"ReplaceProp -> {originInput} replaced with {input_string}");
            }


            return input_string;
        }
       
        public string ReplaceKeys(string inputStr, string defaultValue)
        {
            string pattern = @"%([^%]+)%";

            string result = Regex.Replace(inputStr, pattern, match =>
            {
                string key = match.Groups[1].Value;
                return Dutmp.DataCollection.GetData().TryGetValue(key, out var value) ? value : defaultValue;
            });

            return result;
        }
        public bool PushMoreData(string k, string v)
        {
            if (GlobalNew.g_datacollection != null)
            {
                GlobalNew.g_datacollection.SetMoreProp(k, v);

                if (Dutmp != null)
                    Dutmp.DataCollection.SetMoreProp(k, v);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool PushMoreSpecData(string k, string v)
        {

            if (Dutmp != null)
                Dutmp.DataCollection.SetSpecProp(k, v);

            return true;


        }
        public string PopMoreData(string key)
        {
           if (GlobalNew.ProtreeON == "1")
           {
                if (Dutmp != null)
                    return Dutmp.DataCollection.GetMoreProp(key);
                else
                    return "";
           }

            return GlobalNew.g_datacollection.GetMoreProp(key); // 如果值不為空，返回值的字符串表示形式
        }

        public bool PushMESData(string MESDatakey, Tuple<string, string, string> MESData)
        {
            if (GlobalNew.g_datacollection != null)
            {
                GlobalNew.g_datacollection.SetMesData(MESDatakey, MESData);
                if (Dutmp != null)
                    Dutmp.DataCollection.SetMesData(MESDatakey, MESData);
                return true;
            }
            else
            {
                return false;
            }
        }
        public string PopMESData(string meskey)
        {
            if (GlobalNew.ProtreeON == "1")
                return Dutmp.DataCollection.GetMesData(meskey);

            return GlobalNew.g_datacollection.GetMesData(meskey); // Pop 單筆的MES資料
        }

        public string PopALLMESData()
        {
            if (GlobalNew.ProtreeON == "1")
                return Dutmp.DataCollection.GetALLMESData();

            return GlobalNew.g_datacollection.GetALLMESData(); // Pop 所有的MES資料
        }

        public Dictionary<string, string> PopMESLog()
        {
            if (GlobalNew.ProtreeON == "1")
                return Dutmp.DataCollection.GetMESData();

            return GlobalNew.g_datacollection.GetMESData(); // Pop 所有的MES資料
        }

        public bool SaveCSVHeader(string header)
        {
            if (GlobalNew.g_datacollection != null)
            {
                GlobalNew.g_datacollection.SetCSVHeader(header);
                return true;
            }
            else
            {
                return false;
            }
        }

        public string PopCSVHeader(string headerItem)
        {
            return GlobalNew.g_datacollection.GetCSVHeader(headerItem); // 如果值不為空，返回值的字符串表示形式
        }

        public string AddKeyToJSON(string jsonString, string newKey)
        {
            string modifiedJson = string.Empty;
            string chk_samedata = string.Empty;
            string retry = string.Empty;
            retry = PopMoreData("RetryTimes");

            JObject jsonObject = JObject.Parse(jsonString);

            JObject modifiedObject = new JObject();

            foreach (var property in jsonObject.Properties())
            {
                string orginalKey = property.Name;
                string newjson = newKey + "_" + orginalKey;
                var value = property.Value;

                if (retry == "" && GlobalNew.cycles == 1 && GlobalNew.ProtreeON == "0") //not to Retry Test
                {
                    chk_samedata = PopMoreData("CHK_Data");
                    if (newjson == chk_samedata)
                    {
                        DialogResult result = MessageBox.Show($"Data Name is same, Please Check Data Naming Method!!!!(OK:Continue,Cancel:Break)", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                        if (result == DialogResult.OK)
                        {
                            modifiedObject.Add(newjson, value);
                        }
                        else
                        {
                            modifiedObject.Add(newjson, "");
                            break;
                        }
                    }
                    else
                    {
                        modifiedObject.Add(newjson, value);
                        PushMoreData("CHK_Data", newjson);
                    }
                }
                else
                    modifiedObject.Add(newjson, value);
            }

            modifiedJson = modifiedObject.ToString();

            return modifiedJson;
        }
        public void MESUpload(string mes_name, string mes_value, string Pass_Fail)
        {
            bool MES_ret = false;
            if (mes_name != string.Empty && Pass_Fail != string.Empty)
            {
                if (string.IsNullOrEmpty(mes_value))
                    MES_ret = PushMESData(mes_name, Tuple.Create(mes_name, "NULL", Pass_Fail));
                else
                    MES_ret = PushMESData(mes_name, Tuple.Create(mes_name, mes_value, Pass_Fail));

                if (MES_ret)
                {
                    if (Log_ON_OFF)
                        Logger.Debug($"{mes_name} {mes_value} {Pass_Fail} =>Push MES Data successfully!!");
                }
                else
                    Logger.Debug($"{mes_name}=>Push MES Data Fail!!");
            }
            else
            {
                Logger.Error($"{mes_name}=>MES Data is empty Push Fail!!");
            }
        }
        public string CheckRule(string JsonDataInput, dynamic specParams)
        {
            string Pass_Fail = string.Empty;
            string NGResult_Log = string.Empty;
            string mes_name = string.Empty;
            string mes_value = string.Empty;
            string CheckKeyword = string.Empty;
            string Replace_specParams = string.Empty;

            // 先檢查 specParams 是否為空
            if (string.IsNullOrEmpty(specParams))
            {
                return "PASS";
            }

            if (GlobalNew.ProtreeON == "0")
                CheckKeyword = PopMoreData("PrefixName");
            else
            {
                if (Prefix == string.Empty || Prefix == null)
                    CheckKeyword = Description;
                else
                    CheckKeyword = $"{Prefix}_{Description}";
            }

            string ItemData = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(JsonDataInput))
                {
                    return "FAIL";
                }


                JsonDataInput = AddKeyToJSON(JsonDataInput, CheckKeyword);
                JObject data = JObject.Parse(JsonDataInput);
                Replace_specParams = ReplaceProp(specParams);
                SpecParamsContainer specParams2 = JsonConvert.DeserializeObject<SpecParamsContainer>(Replace_specParams);
                SpecParamsContainer CSVParam = JsonConvert.DeserializeObject<SpecParamsContainer>(Replace_specParams);

                foreach (var specStructure in CSVParam.specParams)
                {
                    bool CSVFlag = true;
                    if (!string.IsNullOrEmpty(specStructure.Name))
                    {
                        specStructure.Name = CheckKeyword + "_" + specStructure.Name;

                        if (!data.ContainsKey(specStructure.Name))
                        {
                            continue;
                        }
                        else
                        {
                            if (specStructure.Csv == "OFF")
                                CSVFlag = false;

                            if (CSVFlag == true)
                            {
                                PushMoreData(specStructure.Name, data[specStructure.Name].ToString());
                                string spc = "";
                                if (specStructure.SpecType == SpecType.Range)
                                    spc = $"[{specStructure.MinLimit} , {specStructure.MaxLimit}]";
                                else if (specStructure.SpecType == SpecType.GreaterThan)
                                    spc = $"[> {specStructure.SpecValue}]";
                                else if (specStructure.SpecType == SpecType.LessThan)
                                    spc = $"[< {specStructure.SpecValue}]";
                                else if (specStructure.SpecType == SpecType.Equal)
                                    spc = $"[= {specStructure.SpecValue}]";
                                else if (specStructure.SpecType == SpecType.NotEqual)
                                    spc = $"[!= {specStructure.SpecValue}]";
                                else if (specStructure.SpecType == SpecType.Contain)
                                    spc = $"[Contain {specStructure.SpecValue}]";
                                else if (specStructure.SpecType == SpecType.Regex)
                                    spc = $"[Regex {specStructure.SpecValue}]";
                                else if (specStructure.SpecType == SpecType.Bypass)
                                    spc = $"[Bypass]";
                                PushMoreSpecData(specStructure.Name, spc);

                            }
                            else
                            {
                                PushMoreData(specStructure.Name, "");
                            }

                            ItemData += $"{specStructure.Name}:{data[specStructure.Name]}\n";
                        }
                    }
                    else
                    {
                        return $"{specStructure.Name} is not exist!!!!!";
                    }
                }
                RowDataItem.Value = ItemData.TrimEnd('\n');

                foreach (var param in specParams2.specParams)
                {
                    bool MESFlag = true;
                    if (param.Name != null)
                    {
                        param.Name = CheckKeyword + "_" + param.Name;

                        if (!data.ContainsKey(param.Name))
                        {
                            Logger.Error($"The Spec keyword({param.Name}) is not included in the data({data})!!!! Error");
                            Pass_Fail = "FAIL";
                            MESUpload(param.Name, "NA", Pass_Fail);
                            return "The Spec keyword is not included in the data!!";
                        }
                        else
                        {
                            mes_name = param.Name;
                            mes_value = data[param.Name].ToString();

                            if (param.Mes == "OFF")
                            {
                                MESFlag = false;
                                if (Log_ON_OFF)
                                    Logger.Info($"Key:{param.Name} -> Mes OFF");
                            }
                        }
                    }
                    else
                    {
                        return $"{param.Name} is not exist!!!!!";
                    }


                    switch (param.SpecType)
                    {
                        case SpecType.Bypass:
                            //ItemData += $"{param.Name}:{data[param.Name]}\n";
                            Pass_Fail = "PASS";
                            break;
                        case SpecType.Range:
                            double value = (double)data[param.Name];
                            //ItemData += $"{param.Name}:{value}\n";
                            if (!(value >= param.MinLimit && value <= param.MaxLimit))
                            {
                                //RowDataItem.Value = ItemData.TrimEnd('\n');
                                Pass_Fail = "FAIL";
                                NGResult_Log = $"Check {param.Name}({value}) beyond the allowed range ({param.MinLimit} to {param.MaxLimit}) .Result->Fail";

                                if (MESFlag)
                                    MESUpload(mes_name, mes_value, Pass_Fail);

                                return NGResult_Log;
                            }
                            Pass_Fail = "PASS";
                            break;
                        case SpecType.Regex:
                            string strRegex = (string)data[param.Name];
                            try
                            {
                                if (!Regex.IsMatch(strRegex, param.SpecValue))
                                {
                                    Pass_Fail = "FAIL";
                                    NGResult_Log = $"{param.Name}({strRegex}) does not meet the specifications，Spec(Regex match {param.SpecValue}).Result->Fail";
                                    if (MESFlag)
                                        MESUpload(mes_name, mes_value, Pass_Fail);

                                    return NGResult_Log;
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"Regex Error: {e.Message}");
                                return $"Regex Error: {e.Message}";
                            }
                            Pass_Fail = "PASS";
                            break;

                        case SpecType.Equal:
                            string str = (string)data[param.Name];
                            //ItemData += $"{param.Name}:{str}\n";

                            if (!str.Equals(param.SpecValue))
                            {
                                //RowDataItem.Value = ItemData.TrimEnd('\n');
                                Pass_Fail = "FAIL";
                                NGResult_Log = $"{param.Name}({str}) does not meet the specifications，Spec({param.SpecValue}).Result->Fail";
                                if (MESFlag)
                                    MESUpload(mes_name, mes_value, Pass_Fail);

                                return NGResult_Log;
                            }
                            Pass_Fail = "PASS";
                            break;

                        case SpecType.NotEqual:
                            string str2 = (string)data[param.Name];
                            //ItemData += $"{param.Name}:{str2}\n";

                            if (str2.Equals(param.SpecValue))
                            {
                                //RowDataItem.Value = ItemData.TrimEnd('\n');
                                Pass_Fail = "FAIL";
                                NGResult_Log = $"{param.Name}({str2}) does not meet the specifications，Spec(!= {param.SpecValue}).Result->Fail";
                                if (MESFlag)
                                    MESUpload(mes_name, mes_value, Pass_Fail);

                                return NGResult_Log;
                            }
                            Pass_Fail = "PASS";
                            break;
                        case SpecType.Contain:
                            string str3 = (string)data[param.Name];
                            //ItemData += $"{param.Name}:{str3}\n";

                            if (!str3.Contains(param.SpecValue))
                            {
                                //RowDataItem.Value = ItemData.TrimEnd('\n');
                                Pass_Fail = "FAIL";
                                NGResult_Log = $"{param.Name}({str3}) does not meet the specifications，Spec( Contain {param.SpecValue} ).Result->Fail";
                                if (MESFlag)
                                    MESUpload(mes_name, mes_value, Pass_Fail);

                                return NGResult_Log;
                            }
                            Pass_Fail = "PASS";
                            break;
                        case SpecType.GreaterThan:
                            double valueA = (double)data[param.Name];
                            double valueB;
                            //ItemData += $"{param.Name}:{valueA}\n";

                            try
                            {
                                if (!double.TryParse(param.SpecValue, out valueB))
                                {
                                    valueB = (double)data[param.SpecValue];
                                }
                            }
                            catch (Exception e)
                            {
                                //RowDataItem.Value = ItemData.TrimEnd('\n');
                                Logger.Error($"{param.SpecValue} Parse Date Error {e.Message}");
                                return $"{param.SpecValue} Parse Date Error {e.Message}";
                            }


                            if (!(valueA > valueB))
                            {
                                //RowDataItem.Value = ItemData.TrimEnd('\n');
                                Pass_Fail = "FAIL";
                                NGResult_Log = $"Check {param.Name}({valueA}) > SpecValue({valueB}) .Result->Fail";
                                if (MESFlag)
                                    MESUpload(mes_name, mes_value, Pass_Fail);

                                return NGResult_Log;
                            }
                            Pass_Fail = "PASS";
                            break;
                        case SpecType.LessThan:
                            double A = (double)data[param.Name];
                            //ItemData += $"{param.Name}:{A}\n";
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
                                //RowDataItem.Value = ItemData.TrimEnd('\n');
                                Logger.Error($"{param.SpecValue} Parse Date Error {e.Message}");
                                return $"{param.SpecValue} Parse Date Error {e.Message}";
                            }

                            if (!(A < B))
                            {
                                //RowDataItem.Value = ItemData.TrimEnd('\n');
                                Pass_Fail = "FAIL";
                                NGResult_Log = $"Check {param.Name}({A}) < SpecValue({B}) .Result->Fail";
                                if (MESFlag)
                                    MESUpload(mes_name, mes_value, Pass_Fail);

                                return NGResult_Log;
                            }
                            Pass_Fail = "PASS";
                            break;
                        default:

                            MessageBox.Show($"{param.SpecType} Type Error not define");
                            NGResult_Log = $"{param.SpecType} Type Error not define";
                            Pass_Fail = "FAIL";

                            // 如果 SpecType 不是 Range 或 Equal，可以處理相應的情況
                            break;
                    }
                    if (MESFlag)
                        MESUpload(mes_name, mes_value, Pass_Fail);

                    if (Pass_Fail == "FAIL")
                    {
                        return NGResult_Log;
                    }
                }
                //RowDataItem.Value = ItemData.TrimEnd('\n');

                return "PASS";
                //if (Pass_Fail == "PASS;")
                //    return "PASS";
                //else
                //    return NGResult_Log;
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                return "無法解析輸入數據為 JSON 格式";
            }
            catch (Exception ex)
            {
                PushMoreData("CHK_Data", "");
                return $"處理數據時出現錯誤: {ex.Message}";
            }
        }

        public class FolderSelEditorRelPath : System.Drawing.Design.UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                string initialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                using (var folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.Description = "選擇資料夾";
                    //folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
                    folderBrowserDialog.SelectedPath = initialDirectory;

                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedFolderPath = folderBrowserDialog.SelectedPath;//絕對路徑
                        try
                        {
                            DriveInfo driveInfo = new DriveInfo(selectedFolderPath);
                            if (driveInfo.DriveType == DriveType.Network && selectedFolderPath.StartsWith(driveInfo.Name))
                            {
                                if (GlobalNew.Network_Path == string.Empty || GlobalNew.Network_Path == null)
                                {
                                    MessageBox.Show($"The DriveType {selectedFolderPath} is Network Drive, Please make sure to set up Network_Path already", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return null;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"The DriveType is Network Error:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        // 轉換為相對路徑
                        string relativePath = GetRelativePath(selectedFolderPath);
                        // 將反斜杠轉換為雙反斜杠
                        relativePath = relativePath.Replace("/", "\\");
                        return relativePath;

                    }

                }

                return value; // 如果用戶取消選擇，返回原始值
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }

            private string GetRelativePath(string selectedFolderPath)
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Uri baseUri = new Uri(baseDirectory);
                Uri selectedFolderUri = new Uri(selectedFolderPath);

                Uri relativeUri = baseUri.MakeRelativeUri(selectedFolderUri);

                return Uri.UnescapeDataString(relativeUri.ToString());
            }
        }

        public class NetWorkEditor : UITypeEditor
        {
            private string originalText;

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                IWindowsFormsEditorService editorService =
                    provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

                if (editorService != null)
                {
                    TextBox textBox = new TextBox();
                    textBox.Multiline = true;
                    textBox.ScrollBars = ScrollBars.Both;
                    textBox.Text = value?.ToString().Replace("\n", Environment.NewLine);
                    textBox.Dock = DockStyle.Fill;

                    // 在编辑器弹出前，设置选择范围，避免全选
                    textBox.SelectionStart = 0;
                    textBox.SelectionLength = 0;

                    Button cancelButton = new Button();
                    cancelButton.Text = "Cancel";
                    cancelButton.DialogResult = DialogResult.Cancel;

                    Form form = new Form();
                    form.Text = "Edit NetWork";
                    form.Size = new System.Drawing.Size(900, 300);
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.Controls.Add(textBox);
                    form.Controls.Add(cancelButton);

                    // 保存原始文本，用于在按叉叉时还原
                    originalText = textBox.Text;

                    form.FormClosing += (s, e) =>
                    {
                        string networkIP = ExtractIpAddress(textBox.Text);
                        // 檢查 NetWork 是否有效
                        if (textBox.Text.StartsWith(@"\\") && networkIP != null)
                        {

                            while (true)
                            {
                                //檢查NetWork是否有連線
                                Sleep(1000);
                                PingReply reply = PingNetworkDrive(networkIP);
                                if (reply != null)
                                {
                                    if (reply.Status != IPStatus.Success)
                                    {
                                        DialogResult ret = MessageBox.Show($"Network unable to connect,error message: {reply.Status},Please Check Network:{networkIP} Status!!!", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                                        if (ret == DialogResult.Cancel)
                                        {
                                            // 還原為進入前的修改
                                            textBox.Text = originalText;
                                            editorService.CloseDropDown();
                                            MessageBox.Show("Changes reverted. Closing the editor.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            //檢查NetWork路徑是否存在
                                            if (!Directory.Exists(Path.GetFullPath(textBox.Text)))
                                            {
                                                DialogResult ret = MessageBox.Show($"Network Path is not exist,Please Check Network Path:{Path.GetFullPath(textBox.Text)}", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                                                if (ret == DialogResult.Cancel)
                                                {
                                                    // 還原為進入前的修改
                                                    textBox.Text = originalText;
                                                    editorService.CloseDropDown();
                                                    MessageBox.Show("Changes reverted. Closing the editor.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                    break;
                                                }
                                            }
                                            else
                                                break;
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show($"發生NetWork路徑異常：{ ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            // 還原為進入前的修改
                                            textBox.Text = originalText;
                                            editorService.CloseDropDown();
                                            MessageBox.Show("Changes reverted. Closing the editor.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            DialogResult res = MessageBox.Show("Invalid NetWork format. Do you want to continue editing?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                            if (res == DialogResult.No)
                            {
                                // 還原為進入前的修改
                                textBox.Text = originalText;
                                editorService.CloseDropDown();
                                MessageBox.Show("Changes reverted. Closing the editor.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                                e.Cancel = true; // 如果 NetWork 无效，取消窗口关闭操作
                        }
                    };

                    DialogResult result = editorService.ShowDialog(form);

                    if (result == DialogResult.Cancel)
                    {
                        return textBox.Text.Replace(Environment.NewLine, "\n");
                    }
                }
                return value;
            }

        }

        public void GetNetworkpath(string Input_Path)
        {
            //獲取Networkpath
            GlobalNew.Network_Path = string.Empty;
            GlobalNew.Network_Path = Input_Path;
        }

        public bool IsNetworkDrive(string path, ref string networkPath)
        {
            try
            {
                // 获取网络磁盘的IP地址
                string driveLetter = Path.GetFullPath(path); // Replace with the drive letter you want to check
                DriveInfo driveInfo = new DriveInfo(driveLetter);

                while (driveInfo.IsReady)//檢查磁碟狀態是否就緒
                {
                    if (driveInfo.DriveType == DriveType.Network && path.StartsWith(driveInfo.Name))//網路磁碟
                    {
                        LogMessage($"Network Driver {driveInfo.Name} is connected Succeess....", MessageLevel.Debug);
                        return true;
                    }
                    else
                    {
                        LogMessage($"Checking Backup path:{path} => DriveType is {driveInfo.DriveType}....", MessageLevel.Debug);//其他磁碟
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        return true;
                    }
                }
                //檢查網路磁碟路徑是否存在
                if (GlobalNew.Network_Path != string.Empty && GlobalNew.Network_Path != null)
                {
                    LogMessage($"網路磁碟IP指定路徑=>{GlobalNew.Network_Path}", MessageLevel.Debug);
                    string networkIP = ExtractIpAddress(GlobalNew.Network_Path);
                    if (GlobalNew.Network_Path.StartsWith(@"\\") && networkIP != null)
                    {
                        // 使用Ping來測試網路磁碟的連線狀態
                        PingReply reply = PingNetworkDrive(networkIP);
                        if (reply != null)
                        {
                            if (reply.Status == IPStatus.Success)
                            {
                                LogMessage($"網路磁碟連線狀態：連接成功，延遲時間：{reply.RoundtripTime}ms", MessageLevel.Debug);
                                try
                                {
                                    //確認網路路徑是否存在
                                    if (!Directory.Exists(Path.GetFullPath(GlobalNew.Network_Path)))
                                    {
                                        LogMessage($"Network Path is not exist!!", MessageLevel.Debug);
                                        return false;
                                    }
                                    networkPath = GlobalNew.Network_Path;
                                }
                                catch (Exception ex)
                                {
                                    LogMessage($"發生NetWork路徑異常：{ ex.Message}", MessageLevel.Debug);
                                    return false;
                                }
                                return true;
                            }
                            else
                            {
                                LogMessage($"網路磁碟連線狀態：無法連接，錯誤訊息：{reply.Status}", MessageLevel.Warn);
                                return false;
                            }
                        }
                        else
                            return false;
                    }
                    else
                    {
                        MessageBox.Show($"Invalid NetWork  path format:{GlobalNew.Network_Path}", "Warn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                LogMessage($"Error checking drive type: {ex.Message}", MessageLevel.Error);
                return false;
            }
        }
        public static Dictionary<string, string> ExtractDataFromPatterns(string input, string patterns)
        {
            var result = new Dictionary<string, string>();
            var keyCounts = new Dictionary<string, int>(); // 用於處理重複的鍵

            // 拆分 Patterns 為多個規則
            string[] rules = patterns.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string rule in rules)
            {
                // 分離鍵與正則表達式
                var parts = rule.Split('|');
                if (parts.Length != 2) continue;

                string key = parts[0];          // 鍵
                string regexPattern = parts[1]; // 正則表達式

                // 匹配輸入資料
                var matches = Regex.Matches(input, regexPattern);
                foreach (Match match in matches)
                {
                    if (match.Success && match.Groups.Count > 1)
                    {
                        string finalKey = key;

                        // 如果鍵已經存在於結果中，則添加索引
                        if (result.ContainsKey(finalKey))
                        {
                            if (!keyCounts.ContainsKey(key))
                            {
                                keyCounts[key] = 0; // 初始化索引計數器
                            }
                            finalKey = $"{key}_{++keyCounts[key]}"; // 添加索引
                        }

                        // 儲存匹配結果
                        result[finalKey] = match.Groups[1].Value;
                    }
                }
            }

            return result;
        }
        public static string ExtractIpAddress(string path)
        {
            string pattern = @"\\\\(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\\";
            Match match = Regex.Match(path, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        public static PingReply PingNetworkDrive(string network_IP)
        {
            using (Ping pingSender = new Ping())
            {
                try
                {
                    // 發送Ping請求並等待回覆
                    return pingSender.Send(network_IP);
                }
                catch (PingException ex)
                {
                    // 發生Ping異常
                    MessageBox.Show($"PingException:{ ex.Message}!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
        }

        // 定義資料結構（你可以根據實際需求擴充這個結構）
        public class DataItem
        {
            public int No { get; set; }
            public string Item { get; set; }
            public string Spec { get; set; }
            public string Value { get; set; }
            public string TestResult { get; set; }
            public double TestTime { get; set; }
            public DateTime StartTime { get; set; }
            public double EslapseTime { get; set; }

            public int RetryTimes { get; set; }

            public int TestCount { get; set; }

            public string OutputData { get; set; }

            // ← 新增：欲插入 PDF 的圖片（路徑）
            public string ImagePath { get; set; }   // 可為 null 或空字串
            public void Clear()
            {
                Value = "";
                TestResult = "";
                TestTime = 0;
                EslapseTime = 0;
                //RetryTimes = 0;
                OutputData = "";
                //TestCount = 0;
            }
        }
    }
}
