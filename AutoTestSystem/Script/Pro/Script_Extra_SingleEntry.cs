using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using static AutoTestSystem.BLL.Bd;
using Newtonsoft.Json.Linq;
using Renci.SshNet.Security;
using Manufacture;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using AutoTestSystem.Model;
using System.Text.RegularExpressions;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Drawing;

namespace AutoTestSystem.Script
{

    internal class Script_Extra_SingleEntry : Script_Extra_Base
    {
        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strJsonResult = string.Empty;
        SingleEntry singleEntry = null;
        private static string strDllpath = string.Empty;
        private static string strFunctionName = string.Empty;
        private static string strPin = string.Empty;
        StringBuilder strbuilderResult;
        byte[] input;
        [Category("Image Params"), Description("SE PIN Content.")]
        public int Width { get; set; } = 2592;
        [Category("Image Params"), Description("SE PIN Content.")]
        public int Height { get; set; } = 1944;

        [Category("Image Params"), Description("SE PIN Content.")]
        public int Pixelformat { get; set; } = 3;

        [Category("Params"), Description("SE PIN Content."), Editor(typeof(PINEditor), typeof(UITypeEditor))]
        public string PIN { get; set; } = "";


        [DllImport("D:\\Project\\github\\MTE_MFGX\\AutoTestSystem\\bin\\Debug\\Config\\IQ\\SE_IVS.dll", EntryPoint = "StartAction", CallingConvention = CallingConvention.StdCall)]
        public static extern void API_Entry(string parm, StringBuilder result);

        public override bool PreProcess()
        {
            int raw_size = (int)(Width * Height * Pixelformat);

            if (input == null || input.Length != raw_size)
            {
                input = new byte[raw_size];
            }
            return true;
        }
        public override bool Process(ref string strOutData)
        {
            string imagePath = @"D:\Project\github\10218_2mFocus_SFR_s608_0db_day_xy3_raw_2592x1944_5184.raw_上下倒轉.bmp";

            int raw_size = (int)(Width * Height * Pixelformat);

            try
            {
                using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    fs.Seek(54, SeekOrigin.Begin);
                    fs.Read(input, 0, raw_size);
                }

                GCHandle handle = GCHandle.Alloc(input, GCHandleType.Pinned);

                IntPtr start_ptr = handle.AddrOfPinnedObject();
                string str_Address = $"0x{start_ptr.ToString("X")}";
                LogMessage($"add:\n{str_Address}");
                string PIN_tmp = PIN.Replace("%buf_addr%", str_Address);
                PIN_tmp = ReplaceProp(PIN_tmp);

                string SE_Pout = string.Empty;
                IQ_SingleEntry.SE_IVS(PIN_tmp, ref SE_Pout);

                LogMessage($"PIN:\n{PIN_tmp}\nPOUT:\n{SE_Pout}");

                IQ_SingleEntry.SaveImage(input, Width, Height, $"output_before_clear.bmp");

                // 清空 input 陣列
                Array.Clear(input, 0, input.Length);
            }
            catch (Exception ex)
            {
                LogMessage($"{Description} Exception:{ex.Message}");
            }

            return true;
        }
        public override bool PostProcess()
        {
            // Check Ruler
            try
            {
                // Check Ruler
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
                                    string object_key = NameInSuareBreakets + "-" + key;
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

        public class PINEditor : UITypeEditor
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
                    TextBox textBox = new TextBox();
                    textBox.Multiline = true;
                    textBox.ScrollBars = ScrollBars.Both;
                    textBox.Text = value?.ToString();
                    textBox.Dock = DockStyle.Fill;
                    textBox.Font = new Font("Calibri", 12/*, FontStyle.Bold*/); // Adjust the font size here

                    Button okButton = new Button();
                    okButton.Text = "OK";
                    okButton.DialogResult = DialogResult.OK;

                    Button cancelButton = new Button();
                    cancelButton.Text = "Cancel";
                    cancelButton.DialogResult = DialogResult.Cancel;

                    Form form = new Form();
                    form.Text = "Enter Command";
                    form.Size = new System.Drawing.Size(500, 600);
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.Controls.Add(textBox);
                    form.Controls.Add(okButton);
                    form.Controls.Add(cancelButton);

                    originalText = textBox.Text;

                    form.FormClosing += (s, e) =>
                    {
                        if (textBox.Text != originalText)
                        {
                            DialogResult res = MessageBox.Show("Do you want to save changes?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (res == DialogResult.No)
                            {
                                textBox.Text = originalText;
                            }
                        }
                    };

                    DialogResult result = editorService.ShowDialog(form);

                    if (result == DialogResult.Cancel)
                    {
                        return textBox.Text.Replace("\n", Environment.NewLine);
                    }
                }

                return value;
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
