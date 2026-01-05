
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_CheckMessageBox : Script_Extra_Base
    {

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private System.Threading.Timer timer;
        string strActItem = string.Empty;
        string strOutputPath = string.Empty;
        string strOutData = string.Empty;
        string msg = string.Empty;

        [Category("Params"), Description("Message")]
        public string Message { get; set; } = "";
        [Category("Params"), Description("設定確認按鍵回傳控件將回傳True")]
        public string KeyOK { get; set; } = "";
        [Category("Params"), Description("設定異常時按鍵回傳時控件將回傳False.\n(注意如果不設定值那將轉為確認模式)")]
        public string KeyNO { get; set; } = "";

        [Category("Failskip Message"), Description("skip Messagebox")]
        public bool isFailskip_msg { get; set; }

        [Category("ShowImageLog"), Description("Image Message")]
        public bool  ON_OFF { get; set; }
        [Category("ShowImageLog"), Description("Image Message")]
        public bool BarcodeForm { get; set; } = false;
        [Category("ShowImageLog"), Description("Button Enable")]
        public bool Button_En { get; set; } = false;
        [Category("ShowImageLog"), Description("選擇文件"), Editor(typeof(FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string sourceFileName { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            return true;
        }
        public override bool Process(ref string output)
        {
            bool isFailskip = true;
            string ReplaceMessage = string.Empty;

            if (isFailskip_msg)
            {
                //當測項出現任何Fail的情形則不跳視窗(須設定Errorcode)
                string checkFailitem = PopMoreData("Failitem");
                if (!string.IsNullOrEmpty(checkFailitem))
                    isFailskip = false;
            }

            if (isFailskip)
            {
                if (!ON_OFF)
                {
                    LogMessage("========Tray Transfer in Progress....========");
                    ReplaceMessage = ReplaceProp(Message);
                    AutoMessageForm customMessageForm = new AutoMessageForm(ReplaceMessage, KeyOK, KeyNO);
                    customMessageForm.TopMost = true;

                    customMessageForm.BringToFront();
                    customMessageForm.Activate();
                    SetForegroundWindow(customMessageForm.Handle);
                    //customMessageForm.ShowDialog(); // 使用 ShowDialog 方法以模態方式顯示視窗
                    DialogResult result = customMessageForm.ShowDialog();
                    if (DialogResult.Yes == result)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    int ret = -1;
                    ImageShowFrom frm_image = new ImageShowFrom();
                    if (!File.Exists(sourceFileName))
                    {
                        MessageBox.Show($"Image File path:{sourceFileName} is not exist!!!", "Waring", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    ReplaceMessage = ReplaceProp(Message);
                    frm_image.SetImageShowForm(ReplaceMessage, KeyOK, KeyNO,0,sourceFileName, BarcodeForm, Button_En);
                    ret = (int)frm_image.ShowDialog();
                    if(ret == 1)
                        return true;
                    else if(ret == 7)
                        return false;
                    else
                        return false;
                }
            }
            return true;
        }
        public override bool PostProcess()
        {

            return true;

        }

        public class FileSelEditorRelPath : System.Drawing.Design.UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "選擇檔案";
                    openFileDialog.Filter = "所有檔案 (*.*)|*.*";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedFilePath = openFileDialog.FileName;

                        // 轉換為相對路徑
                        string relativePath = GetRelativePath(selectedFilePath);
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
            private string GetRelativePath(string selectedFilePath)
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Uri baseUri = new Uri(baseDirectory);
                Uri selectedFileUri = new Uri(selectedFilePath);

                Uri relativeUri = baseUri.MakeRelativeUri(selectedFileUri);

                return Uri.UnescapeDataString(relativeUri.ToString());
            }
        }

        public class Delay
        {
           
            public int DelayTime { get; set; }
            

        }

    }

}
