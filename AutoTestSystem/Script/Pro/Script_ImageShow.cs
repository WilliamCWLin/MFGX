
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using OpenCvSharp;

using System.Drawing;
using AutoTestSystem.Base;
using AutoTestSystem.Model;
using System.Drawing.Design;
using System.ComponentModel;
using System.IO;

namespace AutoTestSystem.Script
{
    internal class Script_ImageShow : Script_Extra_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        [Category("Common Parameters"), Description("選擇文件"), Editor(typeof(FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string sourceFileName { get; set; }
        [Category("Common Parameters"), Description("Windows Form Mode"), TypeConverter(typeof(WindowsForm))]
        public string Mode { get; set; } = "Trigger";

        //Message message_param = null;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            return true;
        }
        public override bool Process(ref string strOutData)
        {
            bool loading_flag = false;
            bool pass_fail = true;
            string status = string.Empty;
            var out_data = new Dictionary<string, object>();
            ImageShowFrom frm_image = new ImageShowFrom();
            if(sourceFileName != string.Empty && sourceFileName != null)
                out_data.Add("SourceFileName", Path.GetFullPath(sourceFileName));
            else
            {
                LogMessage($"SourceFileName is Empty or null", MessageLevel.Debug);
                return false;
            }

            loading_flag = frm_image.PictureShow(sourceFileName);
            if (loading_flag)
            {
                try
                {
                    if (Mode == "Trigger")
                    {
                        frm_image.ShowDialog();
                    }
                    else 
                    { 
                        System.Threading.Thread t = new System.Threading.Thread(() =>
                        {
                            frm_image.ShowDialog();
                        });
                        t.Start();
                    }
                    status = "LoadingImage_OK";
                }
                catch(Exception ex)
                {
                    LogMessage($"LoadingImage Exception:{ex.Message}",MessageLevel.Error);
                    status = $"LoadingImage Exception";
                    out_data.Add("Exception", ex.Message);
                    pass_fail = false;
                }
            }
            else
            {
                //MessageBox.Show($"{sourceFileName} is not exit", "FileName Error", MessageBoxButtons.OK,MessageBoxIcon.Error);
                LogMessage($"{Path.GetFileName(sourceFileName)} is not exit", MessageLevel.Debug);
                status = $"{Path.GetFileName(sourceFileName)} is not exit";
                pass_fail = false;
            }
            out_data.Add("Status", status);
            strOutData = JsonConvert.SerializeObject(out_data, Formatting.Indented);
            return pass_fail;
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

        public class WindowsForm : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> ExposureFunction = new List<string>();

                ExposureFunction.Add("Trigger");
                ExposureFunction.Add("Continuous");

                return new StandardValuesCollection(ExposureFunction);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }
    }
}
