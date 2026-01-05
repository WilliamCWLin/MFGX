
using AutoTestSystem.Model;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.Script.Script_DUT_Related;
using static System.ComponentModel.TypeConverter;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_Jumper : Script_Extra_Base
    {
        string l_strOutData = string.Empty;


        [Category("Condition"), Description("自訂顯示名稱"), Editor(typeof(ConditionEditor), typeof(UITypeEditor))]
        public string Condition { get; set; }
        //public new string Spec
        //{
        //    get { return base.Spec; }
        //    set { base.Spec = value; }
        //}
        [Category("Data"), Description("不設定時為Break模式")]
        public string Data { get; set; } = string.Empty;

        //[Category("Conditional ifelse"), Description("自訂顯示名稱"), Editor(typeof(SpecEditor), typeof(UITypeEditor))]
        //public string Conditional { get; set; }

        //[Category("Conditional ifelse"), Description("if Jump to?"), TypeConverter(typeof(JumpList))]
        //public string if_Goto { get; set; } = "";

        //[Category("Conditional ifelse"), Description("else Jump to?"), TypeConverter(typeof(JumpList))]
        //public string else_Goto { get; set; } = "";



        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            l_strOutData = "";
            return true;
        }
        public override bool Process(ref string strOutData)
        {
            if(string.IsNullOrEmpty(Data) || Data == "Break")
            {
                LogMessage($"Break");
                throw new DumpException($"Break");
            }
            string result = string.Empty;

            string strReplace = ReplaceProp(Data);

            var jsonObject = new { Data = strReplace };
            string jsonString = JsonConvert.SerializeObject(jsonObject);

            strOutData = jsonString;
            result = ConditionCheck(jsonString, Condition);

            if (result == "ExceptionOrFAIL")
                return false;  //有異常無法判別直接FAIL
            else
            {
                var v = new { Data = strReplace , Goto = result };
                string r = JsonConvert.SerializeObject(v,Formatting.Indented);
                strOutData = r;
                throw new DumpException(result);
            }
                

        }
        public override bool PostProcess()
        {
            return true;

        }

        public string ConditionCheck(string JsonDataInput, dynamic Condition)
        {
            string NGResult_Log = string.Empty;
            string CheckKeyword = string.Empty;
            string Replace_specParams = string.Empty;

            // 先檢查 Condition 是否為空
            if (string.IsNullOrEmpty(Condition))
            {
                return "ExceptionOrFAIL";
            }

            //if (GlobalNew.ProtreeON == "0")
            //    CheckKeyword = PopMoreData("PrefixName");
            //else
            //{
            //    if (Prefix == string.Empty || Prefix == null)
            //        CheckKeyword = Description;
            //    else
            //        CheckKeyword = $"{Prefix}_{Description}";
            //}

            string ItemData = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(JsonDataInput))
                {
                    Logger.Error($"Condition IsNullOrEmpty");
                    return "ExceptionOrFAIL";
                }


                //JsonDataInput = AddKeyToJSON(JsonDataInput, CheckKeyword);
                JObject data = JObject.Parse(JsonDataInput);
                Replace_specParams = ReplaceProp(Condition);
                ConditionList condition_list = JsonConvert.DeserializeObject<ConditionList>(Replace_specParams);

                foreach (var param in condition_list.Conditions)
                {
                    if (param.Name == null)
                    {
                        Logger.Error($"Condition IsNullOrEmpty");
                        return "ExceptionOrFAIL";
                    }

                    switch (param.SpecType)
                    {
                        case ConditionType.Bypass:
                            return param.Goto;

                        case ConditionType.Range:
                            double value = (double)data[param.Name];
                            double Min = 0;
                            double Max = 0;
                            if (!double.TryParse(param.Name, out value))
                            {
                                value = (double)data[param.Name];
                            }
                            if (!double.TryParse(param.MinLimit, out Min))
                            {
                                Min = (double)data[param.MinLimit];
                            }
                            if (!double.TryParse(param.MaxLimit, out Max))
                            {
                                Max = (double)data[param.MaxLimit];
                            }


                            if ((value >= Min && value <= Max))
                            {
                                return param.Goto;
                            }
                            
                            break;


                        case ConditionType.Equal:
                            string str = (string)data[param.Name];

                            if (str.Equals(param.SpecValue))
                            {
                                return param.Goto;
                            }
                            
                            break;
                        case ConditionType.Contains:
                            string datastr = (string)data[param.Name];

                            if (datastr.Contains(param.SpecValue))
                            {
                                return param.Goto;
                            }

                            break;
                        case ConditionType.GreaterThan:
                            double valueA = (double)data[param.Name];
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
                                Logger.Error($"{param.SpecValue} Parse Date Error {e.Message}");
                                return "ExceptionOrFAIL";
                            }


                            if ((valueA > valueB))
                            {
                                return param.Goto;
                            }

                            break;
                        case ConditionType.LessThan:
                            double A = (double)data[param.Name];
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
                                Logger.Error($"{param.SpecValue} Parse Date Error {e.Message}");
                                return "ExceptionOrFAIL";
                            }

                            if (A < B)
                            {
                                return param.Goto;
                            }

                            break;
                        default:

                            return "ExceptionOrFAIL";

                    }

                }

                LogMessage("Not meeting the conditions");

                return "ExceptionOrFAIL";

            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                Logger.Error($"無法解析輸入數據為 JSON 格式");
                return "ExceptionOrFAIL";
            }
            catch (Exception ex)
            {
                Logger.Error($"處理數據時出現錯誤: {ex.Message}");

                return "ExceptionOrFAIL";
            }
        }
    }

    public class ConditionEditor : UITypeEditor
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

                originalText = value?.ToString();
                var selectnode = (dynamic)Global_Memory.mySelectedNode.Tag;
                string Data = "";
                if (selectnode.RowDataItem != null)
                {
                    // 現在可以安全地訪問 OutputData 屬性
                    Data = selectnode.RowDataItem.OutputData;
                }
                string str = selectnode.RowDataItem.OutputData;

                List<string> List_ID = new System.Collections.Generic.List<string>();
                if (Global_Memory.select_Node_Parent != null)
                {
                    if (Global_Memory.select_Node_Parent.Nodes != null)
                    {
                        
                        bool flag = false;
                        //List_ID.Add("Continue");
                        List_ID.Add("Break");
                        foreach (System.Windows.Forms.TreeNode _node in Global_Memory.select_Node_Parent.Nodes)
                        {
                            if (((dynamic)Global_Memory.mySelectedNode.Tag).ID == ((dynamic)_node.Tag).ID)
                            {
                                flag = true;
                                continue;
                            }

                            if (flag && _node.Tag.GetType() != Global_Memory.ExHandleType && _node.Tag is ContainerNode)
                            {
                                List_ID.Add(((dynamic)_node.Tag).Description + "(" + ((dynamic)_node.Tag).ID + ")");
                            }
                        }

                        ConditionForm form = new ConditionForm(str, value?.ToString(), List_ID);
                        form.Text = "Edit Condition";
                        form.StartPosition = FormStartPosition.CenterParent;


                        form.FormClosing += (s, e) =>
                        {
                            if (form.GetSpecText() != originalText)
                            {
                                // 檢查 JSON 是否有效
                                if (!IsValidJson(form.GetSpecText()))
                                {
                                    DialogResult res = MessageBox.Show("Invalid JSON format. Do you want to continue editing?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                                    if (res == DialogResult.No)
                                    {
                                        // 還原為進入前的修改
                                        form.SetSpecText(originalText);
                                        editorService.CloseDropDown();
                                        MessageBox.Show("Changes reverted. Closing the editor.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    else
                                        e.Cancel = true; // 如果 JSON 无效，取消窗口关闭操作
                                }
                            }
                        };

                        DialogResult result = editorService.ShowDialog(form);

                        if (result == DialogResult.Cancel)
                        {
                            return form.GetSpecText().Replace(Environment.NewLine, "\n");/*textBox.Text.Replace(Environment.NewLine, "\n")*/
                        }

                        return new StandardValuesCollection(List_ID);
                    }
                    else
                    {
                        return new StandardValuesCollection(new int[] { 0 });
                    }
                }
                else
                {
                    return new StandardValuesCollection(new int[] { 0 });
                }

            }
            return value;
        }

        // 检查 JSON 是否有效
        private bool IsValidJson(string json)
        {
            try
            {
                JToken.Parse(json);
                return true;
            }
            catch (JsonReaderException)
            {
                // Parsing error, the JSON is not valid
                return false;
            }
            catch (JsonException)
            {
                // Other exception, e.g. invalid type, etc.
                return false;
            }
        }
    }

}
