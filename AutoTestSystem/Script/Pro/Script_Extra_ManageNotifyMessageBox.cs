
using AutoTestSystem.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_ManageNotifyMessageBox : Script_Extra_Base
    {

        [Category("Params"), Description("Message")]
        public string Message { get; set; } = "";

        [Category("Params"), Description("Message Windows")]
        public Win_Status WinEnable { get; set; } = Win_Status.Open;

        [Category("ShowImagePath"), Description("選擇文件"), Editor(typeof(FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
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
            try
            {
                switch (WinEnable)
                {
                    case Win_Status.Open:
                        GlobalNew.ManageNotifyFormOpen(HandleDevice.ID,Message, sourceFileName);
                        break;
                    case Win_Status.Close:
                        GlobalNew.ManageNotifyFormClose(HandleDevice.ID);
                        break;
                    default:
                        break;
                }
            }catch(Exception Ex)
            {
                LogMessage($"ManageNotify Form Exception Error:{Ex.Message}",MessageLevel.Error);
                return false;
            }

            return true;

        }
        public override bool PostProcess()
        {

            return true;

        }

        public enum Win_Status
        { 
            Open,
            Close
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
