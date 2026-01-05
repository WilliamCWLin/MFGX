
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
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using AutoTestSystem.DAL;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_CalculateDLL : Script_Extra_Base
    {

        [Category("PreProcess Parameters"), Description("DLL file")]
        public string DLL_File { get; set; } = "";

        [Category("PreProcess Parameters"), Description("load content"), Editor(typeof(CommandEditor_MakeWriteLine), typeof(UITypeEditor))]
        public string Load_Content { get; set; } = "";

        [Category("PreProcess Parameters"), Description("execute")]
        public bool Execute { get; set; }

        [Category("PreProcess Parameters"), Description("wait time")]
        public string WaitTime { get; set; } = "3000";

        [Category("Common Parameters"), Description("Data source Path")]
        public string DataSourcePath { get; set; } = "";

        [Category("Common Parameters"), Description("Get All Data of the Path")]
        public bool GetAllData { get; set; }

        [Category("Common Parameters"), Description("DLL Path")]
        public string DLLPath { get; set; } = "";

        [Category("Common Parameters"), Description("Profile Content"), Editor(typeof(CommandEditor_MakeWriteLine), typeof(UITypeEditor))]
        public string content { get; set; } = "";

        [Category("Common Parameters"), Description("Pass Position Parameter Name")]
        public string PassPosition { get; set; } = "";

        public class CommandEditor_MakeWriteLine : UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                using (Form form = new Form())
                {
                    TextBox textBox = new TextBox
                    {
                        Multiline = true,
                        Dock = DockStyle.Fill,
                        Text = value?.ToString().Replace("\\n", Environment.NewLine)
                    };
                    form.Controls.Add(textBox);
                    form.ShowDialog();
                    //return textBox.Text.Replace(Environment.NewLine, "\\n");
                    return textBox.Text;
                }
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
        }

        public string parameter;

        public string strstringoutput = "";

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
            Dictionary<string, string> outputdata = new Dictionary<string, string>();
            StringBuilder tempstrOutData = new StringBuilder ("");
            LogMessage("========CalculateDLL========");
            LogMessage(String.Format("Output Original Content: {0}", content));
            string oricontent = content.Replace("\\n", "\n");

            if (!GetAllData) //單一檔案路徑
            {
                // 有前處理的話
                if (Execute)
                {
                    string strOutData2 = "";
                    Dictionary<string, string> outputdata2 = new Dictionary<string, string>();
                    string Load_Content_Trans = ReplaceProp(Load_Content);
                    IQ_SingleEntry.SE_StartAction(DLL_File, Load_Content_Trans, ref strOutData2, outputdata2);
                    LogMessage("==Preprocess==");
                    LogMessage($"{strOutData2}");
                }
                // 要注意這個地方檔案位置要是正確的，檔名要是正確的
                string oricontent_Trans = ReplaceProp(oricontent);
                IQ_SingleEntry.SE_StartAction(DLLPath, oricontent_Trans, ref strOutData, outputdata);
                LogMessage("==Process==");
                LogMessage($"{strOutData}");
            }
            else // 讀取路徑下所有檔案，適用於 Through Focus 的SFR 計算
            {
                // DataSourcePath 要指定到放檔案的路徑下，抓出所有檔案 .raw 檔
                string DataSourcePath_Trans = ReplaceProp(DataSourcePath);
                string[] files = Directory.GetFiles(DataSourcePath_Trans, "*.raw", SearchOption.TopDirectoryOnly);
                 //將每一個檔案路徑換上，並且跑 DLL，會跑到過，或是跑完為止
                 foreach (string file in files)
                 {
                 string SFRpin = oricontent.Replace("%BMPimagefilepath%", file+".bmp");
                 string Transpin = Load_Content.Replace("%RAWimagefilepath%", file);
                     // 開啟前處理功能
                     if (Execute)
                     {
                         //準備需要參數
                         string strOutData2 = "";
                         Dictionary<string, string> outputdata2 = new Dictionary<string, string>();
                         IQ_SingleEntry.SE_StartAction(DLL_File, Transpin, ref strOutData2, outputdata2);
                        LogMessage("==Preprocess==");
                        LogMessage($"{strOutData2}");


                        //執行原本的 DLL ，要吃 .raw.bmp
                        IQ_SingleEntry.SE_StartAction(DLLPath, SFRpin, ref strOutData, outputdata);
                        LogMessage("==Process==");
                        LogMessage($"{strOutData}");
                    }
                     // 沒有開啟前處理功能，其實不太會用到，因為這是專門屬於 Through Focus 的程式
                     else
                     {
                         //使用正則表達式來匹配 "Path=" 到 "\n" 之間的內容
                         //string pattern = @"(Path=).*?(\n)";
                         //string Newcontent = Regex.Replace(oricontent, pattern, $"$1{file}$2");

                         IQ_SingleEntry.SE_StartAction(DLLPath, SFRpin, ref strOutData, outputdata);
                        LogMessage("==Process==");
                        LogMessage($"{strOutData}");
                    }
                     
                     //開始判斷有沒有過規，適用於 Through Focus
                     //////////////////////////////////////////////////////////////////////////////////////
                     String jsonStr_loop = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
                     strOutData = jsonStr_loop ;
                     strstringoutput = strOutData;


                     string result = CheckRule(strstringoutput, Spec);
                     if (result == "PASS" || Spec == "")
                     {
                         //return true;
                         //記錄下當前 PASS 的檔名
                         string PathFileName = Path.GetFileName(file);
                         string PathFileNamewithoutExtension = Path.GetFileNameWithoutExtension(PathFileName);
                         bool ret = PushMoreData(PassPosition, PathFileNamewithoutExtension);
                         break;
                     }
                     else
                     {
                         LogMessage($"{result}", MessageLevel.Error);
                         //return false;
                     }
                }
            }

            String jsonStr = JsonConvert.SerializeObject(outputdata, Formatting.Indented);

            strOutData = jsonStr;
            strstringoutput = strOutData;

            return true;
        }
        public override bool PostProcess()
        {
            string result = CheckRule(strstringoutput, Spec);
            if (result == "PASS" || Spec == "")
            {
                return true;
            }
            else
            {
                LogMessage($"{result}", MessageLevel.Error);
                return false;
            }

        }
        public static double EvaluateExpression(string expression)
        {
            DataTable table = new DataTable();
            table.Columns.Add("expression", string.Empty.GetType(), expression);
            DataRow row = table.NewRow();
            table.Rows.Add(row);
            return double.Parse((string)row["expression"]);
        }


    }
}
