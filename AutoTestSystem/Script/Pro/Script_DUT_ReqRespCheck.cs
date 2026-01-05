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
using static AutoTestSystem.Model.IQ_SingleEntry;
using System.Drawing;
using System.Xml.Linq;
using System.Threading;
using System.Windows.Forms;

namespace AutoTestSystem.Script
{

    internal class Script_DUT_ReqRespCheck : ScriptDUTBase
    {
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("支援用%%方式做變數值取代")]
        public string Send_Command { get; set; } = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int TimeOut { get; set; } = 8000;

        [Category("Common Parameters"), Description("自訂顯示名稱"), Editor(typeof(CheckPattern), typeof(UITypeEditor))]
        public string CheckCmd { get; set; } = string.Empty;

        [Category("Parser"), Description("load content"), Editor(typeof(ParserPatterns), typeof(UITypeEditor))]
        public string Patterns { get; set; } = "";
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool PreProcess()
        {

            strOutData = string.Empty;

            return true;
        }

        public override bool Process(DUT_BASE DUTDevice, ref string output)
        {
            string ReadOutput = string.Empty;
            string SendCmd = ReplaceProp(Send_Command);
            bool ret = false;
            try
            {
                DUTDevice.ClearBuffer();
                ret = SEND_COMMAND(DUTDevice, SendCmd);

                if (ret) // 如果發送命令成功，繼續執行
                {
                    if (ret && !string.IsNullOrEmpty(CheckCmd)) // 如果讀取成功並且有指定匹配模式，繼續執行
                    {
                        ret &= ReadAndCheck(DUTDevice, ref ReadOutput);

                        if (ret && !string.IsNullOrEmpty(Patterns)) // 如果讀取成功並且有指定匹配模式，繼續執行
                        {
                            var result = ExtractDataFromPatterns(ReadOutput, Patterns);
                            if (result.Count == 0) // 若無法匹配模式，則認定失敗
                            {
                                ret = false;
                            }
                            else
                            {
                                output = JsonConvert.SerializeObject(result, Formatting.Indented);
                                strOutData = output;
                            }
                        }
                    }
                }
               
                LogMessage(strOutData);
                DUTDevice.ClearBuffer();
                return ret;
            }
            catch (Exception ex)
            {
                var data = new Dictionary<string, object>
                        {
                            { "STATUS", "FAIL" },
                            { "Exception", ex.Message }
                        };
                output = JsonConvert.SerializeObject(data, Formatting.Indented);

                strOutData = output;
                LogMessage(strOutData);
                DUTDevice.ClearBuffer();
                return false;
            }
        }

        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

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

        public bool SEND_COMMAND(DUT_BASE DUTDevice, string cmd)
        {
            if (cmd == "\\r\\n")
                cmd = "\r\n";
            else if (cmd == "\\r")
                cmd = "\r";
            else if (cmd == "\\n")
                cmd = "\n";
            else if (cmd == "CRLF")
                cmd = "\r\n";
            else if (cmd == "CR")
                cmd = "\r";
            else if (cmd == "LF")
                cmd = "\n";

            return DUTDevice.SEND(cmd);
        }
        public bool ReadAndCheck(DUT_BASE dutDevice, ref string outputString)
        {
            // 用於儲存輸出內容
            StringBuilder outputBuilder = new StringBuilder();
            string sectionOutput = string.Empty;
            bool isCheckSuccessful = false;
            string rplcheck = ReplaceProp(CheckCmd);
            // 設置計時器
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds <= TimeOut)
            {
                string tempBuffer = string.Empty;

                // 嘗試從設備中讀取數據
                dutDevice.READ(ref tempBuffer);

                if (!string.IsNullOrEmpty(tempBuffer))
                {
                    outputBuilder.Append(tempBuffer); // 將新數據添加到輸出
                    sectionOutput = outputBuilder.ToString();

                    // 檢查數據是否包含指定命令
                    if (sectionOutput.Contains(rplcheck))
                    {
                        isCheckSuccessful = true;
                        break;
                    }
                }

                Thread.Sleep(5); // 短暫等待以減少 CPU 資源佔用
            }
            outputString = outputBuilder.ToString();
            // 記錄結果到日誌
            LogMessage("\r\n" + outputBuilder.ToString());
            // 若超時，記錄超時訊息
            if (!isCheckSuccessful)
            {
                LogMessage($"Timeout after {TimeOut} ms");
            }
            else
            {
                LogMessage("Check Success.", MessageLevel.Info);
            }

            // 停止計時器並釋放資源
            stopwatch.Stop();
            outputBuilder.Clear();

            return isCheckSuccessful;
        }



    }


    public class ParserPatterns : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            using (Form form = new Form())
            {
                // 設定表單屬性
                form.StartPosition = FormStartPosition.CenterScreen; // 居中顯示
                form.Size = new Size(800, 800); // 設定表單大小
                form.Text = "編輯文字"; // 可選：設定表單標題
                form.MinimumSize = new Size(800, 800); // 可選：設置最小大小

                TextBox textBox = new TextBox
                {
                    Multiline = true,
                    Dock = DockStyle.Fill,
                    ScrollBars = ScrollBars.Both, // 開啟水平與垂直滾動條
                    WordWrap = false, // 可選：關閉自動換行，讓水平方向也能滾動
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

    public class CheckPattern : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            using (Form form = new Form())
            {
                // 設定表單屬性
                form.StartPosition = FormStartPosition.CenterScreen; // 居中顯示
                form.Size = new Size(800, 800); // 設定表單大小
                form.Text = "編輯文字"; // 可選：設定表單標題
                form.MinimumSize = new Size(800, 800); // 可選：設置最小大小

                TextBox textBox = new TextBox
                {
                    Multiline = true,
                    Dock = DockStyle.Fill,
                    ScrollBars = ScrollBars.Both, // 開啟水平與垂直滾動條
                    WordWrap = false, // 可選：關閉自動換行，讓水平方向也能滾動
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
}
