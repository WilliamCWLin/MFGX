using AutoTestSystem.Base;
using AutoTestSystem.Equipment.DosBase;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;
using AutoTestSystem.DAL;
using System.Text.RegularExpressions;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace AutoTestSystem.Script
{
    internal class Script_SingleEntry_Calculater : Script_Extra_Base
    {
        public Dictionary<string, string> tmp_ParseConfig = new Dictionary<string, string>();
        string strActItem = "";
        //string strOutData = string.Empty;
        bool isMaximized = true;
        [Category("Base"), Description("自訂顯示名稱"), Editor(typeof(SpecEditor_SingleEntry), typeof(UITypeEditor))]
        public string Spec { get; set; }


        [Category("Common Parameters"), Description("支援用%%方式做變數值取代")]
        public string Send_Command { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int WaitTime { get; set; } = 3000;

        [Category("Common Parameters"), Description("自訂顯示名稱"), Editor(typeof(CommandEditor), typeof(UITypeEditor))]
        public string ActionItem { get; set; } = "ALL";

        [Category("Common Parameters"), Description("圖檔位置")]
        public string FilePath { get; set; } = "";

        public string FinalCommand = "";
       
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }     

        public override bool PreProcess()
        {
            if (Send_Command == null || Send_Command == string.Empty)
            {
                LogMessage("Send_Command can not be null.", MessageLevel.Error);
                return false;
            }

            strActItem = ActionItem;

            return true;
        }
        public override bool Process(ref string output)
        {
            FinalCommand = Send_Command + " " + FilePath;


            SetTimeout(WaitTime);
            Send(ReplaceProp(FinalCommand), ActionItem);
            LogMessage($"Send:  {ReplaceProp(FinalCommand)}\n");
            READ(ref output);
            LogMessage($"Read END:  {output}\n");
            strOutData = output;
            
            return true;

        }

        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
                return true;
            else
                return false;

        }

        DosCmd doscmd = new DosCmd();
        int waitTime = 0;
        string recvStr = "";
        string checkStr = "";
        string strOutData = string.Empty;
        //public override void Dispose()
        //{
        //    throw new NotImplementedException();
        //}
        //blic override bool Init(string strParamInfo)
        //
        //  return true;
        //
        //blic override bool SEND(string input)
        //
        //  throw new NotImplementedException();
        //
        public void SetTimeout(int time)
        {
            waitTime = time;
        }
        public bool Send(string input, string sectionItemList)
        {
          bool result = doscmd.SendCommand(input, ref recvStr, checkStr, waitTime);
            if (result == true)
            {
                LogMessage($"All of the RAW output data:  {recvStr}\n");

                Dictionary<string, string> configDictionary = ParseConfig(recvStr);
                Dictionary<string, string> FinalResult = ParseConfig(recvStr);
                if (sectionItemList == "ALL")
                {
                    LogMessage($"All of the Dictionary output data:  {configDictionary}\n");
                    Dictionary<string, string> Filter_Final_Result = new Dictionary<string, string> { };
                    Filter_Final_Result = configDictionary.Where(kv => kv.Key.Contains("__")).ToDictionary(kv => kv.Key, kv => kv.Value);
                    LogMessage($"All of the Filter Dictionary output data:  {Filter_Final_Result}\n");
                    strOutData = CreateDataString(Filter_Final_Result);
                    return true;
                }
                else
                {
                    LogMessage($"All of the Dictionary output data:  {configDictionary}\n");
                    var sections = sectionItemList.Split(',');
                    var filte_result = new Dictionary<string, string>();

                    foreach (string section_item in sections)
                    {
                     var partial_result = configDictionary
                    .Where(kvp => kvp.Key.StartsWith(section_item + "__"))
                    .Select(kvp => new
                    {
                        Section = kvp.Key.Split(new[] { "__" }, StringSplitOptions.None)[0],
                        Key = kvp.Key,
                        Value = kvp.Value
                    })
                    .ToDictionary(item => item.Key, item => item.Value);

                        foreach (var kvp in partial_result)
                        {
                            filte_result[kvp.Key] = kvp.Value;
                        }

                        Dictionary<string, string> Final_Result = filte_result;
                        Dictionary<string, string> Filter_Final_Result = new Dictionary<string, string> { };
                        Filter_Final_Result = Final_Result.Where(kv => kv.Key.Contains("__")).ToDictionary(kv => kv.Key, kv => kv.Value);
                        LogMessage($"All of the Filter Dictionary output data:  {configDictionary}\n");
                        strOutData = CreateDataString(Filter_Final_Result);
                    }
                    return true;
                }
            }
            else
                return false;
        }
        Dictionary<string, string> ParseConfig(string configContent)
        {
            var configDict = new Dictionary<string, string>();
            string currentSection = "";
            bool ignoreActionSection = false;
            bool startReading = false;

            foreach (var line in configContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("DLL_Version"))
                {
                    startReading = true;
                }

                if (startReading)
                {
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        currentSection = line.Trim('[', ']');
                        ignoreActionSection = currentSection == "Action";
                    }
                    else if (!ignoreActionSection && line.Contains("="))
                    {
                        var parts = line.Split(new[] { '=' }, 2);
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();
                        if (!string.IsNullOrEmpty(currentSection))
                        {
                            key = $"{currentSection}__{key}";
                        }
                        configDict[key] = value;
                        //PushMoreData(key, value);
                        
                    }
                }
            }
            tmp_ParseConfig = configDict;
            return configDict;
        }



        public bool READ(ref string output)
        {
            output = strOutData;
            return true;
        }
        public string CreateDataString(Dictionary<string, string> data)
        {
            try
            {
                string jsonStr = JsonConvert.SerializeObject(data, Formatting.Indented);
                return jsonStr;
            }
            catch (Exception ex)
            {
                // 處理轉換錯誤
                return $"轉換為 JSON 字串時出現錯誤: {ex.Message}";
            }
        }



        public class SpecEditor_SingleEntry : UITypeEditor
        {
            private string originalText;

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                IWindowsFormsEditorService editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                if (editorService != null)
                {
                    originalText = value?.ToString();
                    dynamic tag = Global_Memory.mySelectedNode.Tag;
                    string text = "";
                    if (tag.RowDataItem != null)
                    {
                        text = tag.RowDataItem.OutputData;
                    }

                    string dataName = tag.RowDataItem.OutputData;
                    SpecForm_SingleEntry form = new SpecForm_SingleEntry(dataName, value?.ToString());
                    form.Text = "Edit SPEC SingleEntry";
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.FormClosing += delegate (object s, FormClosingEventArgs e)
                    {
                        if (form.GetSpecText() != originalText && !IsValidJson(form.GetSpecText()))
                        {
                            DialogResult dialogResult2 = MessageBox.Show("Invalid JSON format. Do you want to continue editing?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Hand);
                            if (dialogResult2 == DialogResult.No)
                            {
                                form.SetSpecText(originalText);
                                editorService.CloseDropDown();
                                MessageBox.Show("Changes reverted. Closing the editor.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                            else
                            {
                                e.Cancel = true;
                            }
                        }
                    };
                    DialogResult dialogResult = editorService.ShowDialog(form);
                    if (dialogResult == DialogResult.Cancel)
                    {
                        return form.GetSpecText().Replace(Environment.NewLine, "\n");
                    }
                }

                return value;
            }

            private bool IsValidJson(string json)
            {
                try
                {
                    JToken.Parse(json);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
                catch (JsonException)
                {
                    return false;
                }
            }
        }





    }
}
