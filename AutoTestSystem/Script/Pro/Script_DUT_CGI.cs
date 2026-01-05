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
using System.Threading.Tasks;
using System.Diagnostics;
using static AutoTestSystem.BLL.Bd;
using System.ComponentModel;
using System.Drawing.Design;
using Manufacture;
using System.Windows.Forms.Design;
using System.Windows.Forms;
namespace AutoTestSystem.Script
{
    internal class Script_DUT_CGI : ScriptDUTBase
    {
        string strOutData = string.Empty;
        int flagMethod = (int)E_request_type.POST;
        [Category("Common Parameters"), Description("支援用%%方式做變數值取代"), Editor(typeof(JsonEditForm), typeof(UITypeEditor))]
        public string JSONData { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Checkstr { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Method { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string CGICMD { get; set; }

        [Category("Common Parameters"), Description("Timeout")]
        public int timeOut { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool PreProcess()
        {
            if (timeOut == null )
            {
                timeOut = 10;

            }

            if (Checkstr == null || Checkstr == string.Empty)
            {
                LogMessage("Checkstr can not be null.", MessageLevel.Error);
                return false;
            }

            if (CGICMD == null || CGICMD == string.Empty)
            {
                LogMessage("CGICMD can not be null.", MessageLevel.Error);
                return false;
            }

            if (Method == null || Method == string.Empty)
            {
                LogMessage("Method can not be null.", MessageLevel.Error);
                return false;
            }
            else
            {
                if (Method.Contains("Get"))
                    flagMethod = (int)E_request_type.GET;
                else
                {
                    flagMethod = (int)E_request_type.POST;
                    if (JSONData == null || JSONData == string.Empty)
                    {
                        LogMessage("JSONData can not be null.", MessageLevel.Error);
                        return false;
                    }
                }
                   
            }

            return true;
        }
        enum E_request_type : ushort
        {
            GET = 0,
            POST = 1
        }
        public override bool Process(DUT_BASE HTTP_CGI, ref string output)
        {
            HTTP_CGI.SetTimeout(0,timeOut);
            if (HTTP_CGI.SendCGICommand(flagMethod, Checkstr, CGICMD, ReplaceProp(JSONData) , ref output) == true)
            {
                strOutData = output;
                return true;
            }
            return false;
        }

        public override bool PostProcess()
        {
            
            string result = CheckRule(strOutData, Spec);
            ExtraProcess(ref result);

            if (result == "PASS" || Spec == "")
            {
                return true;
            }             
            else
            {
                LogMessage($"{result}",MessageLevel.Error);
                return false;
            }
        }

        public void ExtraProcess(ref string output)
        {
        }

        public class JsonEditForm : UITypeEditor
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
                    textBox.SelectionStart = 0;
                    textBox.SelectionLength = 0;

                    Button cancelButton = new Button();
                    cancelButton.Text = "Cancel";
                    cancelButton.DialogResult = DialogResult.Cancel;

                    Form form = new Form();
                    form.Text = "Edit JSON";
                    form.Size = new System.Drawing.Size(900, 300);
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.Controls.Add(textBox);
                    form.Controls.Add(cancelButton);

                    originalText = textBox.Text;

                    form.FormClosing += (s, e) =>
                    {
                        if (textBox.Text != originalText)
                        {
                            // 檢查 JSON 是否有效
                            if (!IsValidJson(textBox.Text))
                            {
                                DialogResult res = MessageBox.Show("Invalid JSON format. Do you want to continue editing?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                                if (res == DialogResult.No)
                                {
                                    // 還原為進入前的修改
                                    textBox.Text = originalText;
                                    editorService.CloseDropDown();
                                    MessageBox.Show("Changes reverted. Closing the editor.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                    e.Cancel = true;
                            }
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
}
