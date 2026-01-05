
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
using System.Drawing;
using static AutoTestSystem.Model.IQ_SingleEntry;
using static AutoTestSystem.Base.CCDBase;
using System.Xml.Linq;
using AutoTestSystem.Base;
using static AutoTestSystem.MainForm;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Math;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_UIControl_SetSnTextbox : Script_Extra_Base
    {                          
        [Category("Params"), Description("DUT裝置中設置的SN列表"), TypeConverter(typeof(SN_TypeList))]
        public string Textbox_Name { get; set; } = string.Empty;

        [Category("Params"), Description("要設定到Textbox的SN值，支援%%")]
        public string Value { get; set; } = string.Empty;

        [Category("Params"), Description("要設定到Textbox的SN值，支援%%")]
        public bool InputByForm { get; set; } = false;

        [Category("Params"), Description("要設定到Textbox的SN值，支援%%")]
        public bool ResetUI { get; set; } = false;

        public override bool PreProcess()
        {
            return true;
        }
        public override bool Process(ref string strOutData)
        {
            try
            {
                string labelText = Textbox_Name;

                // 檢查是否有可用的 DutDashboard，如果沒有就記錄訊息並結束流程
                if (HandleDevice.DutDashboard == null)
                {
                    LogMessage($"This feature is not supported.");
                    return true;
                }

                // 在 g_SNtextBoxes 中尋找名稱為 SN_TextboxName 的 TextBox 所屬的 SN_Panel
                SN_Panel foundPanel = HandleDevice.DutDashboard.g_SNtextBoxes.FirstOrDefault(panel =>
                    panel.SN_Textbox != null && panel.SN_Textbox.Name == Textbox_Name);

                // 將 Set_Value 做字串替換處理（可能是變數替換），取得要設定的序號
                string sn = ReplaceProp(Value);

                if (InputByForm)
                {
                    sn = string.Empty;

                        // 嘗試找到主視窗作為 Owner
                        var owner = MFGX.Instance;

                        if (owner != null)
                        {
                            owner.Invoke((Action)(() =>
                            {
                                using (var form = new SNInputForm(labelText, HandleDevice.Description))
                                {

                                    if (form.ShowDialog(owner) == DialogResult.OK)
                                    {
                                        sn = form.SerialNumber;
                                    }
                                    else
                                    {
                                        sn = string.Empty;
                                        GlobalNew.g_shouldStop = true;
                                        
                                    }
                                }
                            }));
                        }
                        else
                        {
                            // 如果找不到 owner，就直接在主 UI 執行緒上跑
                            using (var form = new SNInputForm(labelText, HandleDevice.Description))
                            {
                                if (form.ShowDialog() == DialogResult.OK)
                                {
                                    sn = form.SerialNumber;
                                }
                            }
                        }
                    
                }

                // 預設序號格式檢查為通過
                bool checkSN_format = true;

                // 在 UI 執行緒中設定 TextBox 的值，並進行格式檢查
                foundPanel.Invoke((Action)(() =>
                {
                    // 將處理後的序號設定到對應的 TextBox
                    foundPanel.SN_Textbox.Text = sn;

                    // 記錄設定的訊息
                    LogMessage($"Set {Textbox_Name} = {sn}");

                    // 檢查序號格式是否正確
                    if (!HandleDevice.DutDashboard.CheckTextBoxPattern(foundPanel.SN_Textbox))
                    {
                        // 若格式錯誤，清空 TextBox 並標記格式錯誤
                        //foundPanel.SN_Textbox.Text = "";
                        checkSN_format = false;
                    }
                }));
                HandleDevice.DutDashboard.Invoke((Action)(() =>
                {
                    HandleDevice.DutDashboard.DashBoardDescription.BackColor = System.Drawing.Color.MidnightBlue;
                    HandleDevice.DutDashboard.DashBoardDescription.ForeColor = System.Drawing.Color.White;
                }));
                if (GlobalNew.RunMode == 2)
                {

                   HandleDevice.testUnit.ShowStatus = string.Empty;
                    
                }

                // 如果格式檢查失敗，記錄錯誤訊息並回傳 false
                if (!checkSN_format)
                {
                    LogMessage($"The {Textbox_Name} input format is incorrect({sn}).");
                    if (ResetUI)
                    {
                        HandleDevice.DutDashboard.ResetDutInfo(HandleDevice);
                        LogMessage($"Start testing");
                    }

                    if (GlobalNew.RunMode == 2)
                    {
                        if(string.IsNullOrEmpty(sn))
                        {
                            HandleDevice.DutDashboard.SetTestStatus(HandleDevice, TestStatus.IDLE);
                            HandleDevice.testUnit.IsSkip = true;
                            return true;
                        }
                    }

                    return false;
                }

                if (ResetUI)
                {
                    HandleDevice.DutDashboard.ResetDutInfo(HandleDevice);
                    LogMessage($"Start testing");

                    if (GlobalNew.RunMode == 2)
                    {
                        HandleDevice.DutDashboard.ClearRichText(HandleDevice.DutDashboard.DutLogRichTextBox);
                        LogMessage($"Start testing");
                        
                    }
                }

                PushMoreData(labelText, sn);
                LogMessage($"PushMoreData {labelText} = {sn}");

            }
            catch (Exception ex)
            {
                // 捕捉例外並記錄錯誤訊息
                LogMessage($"{ex.Message}");
            }

            // 若無錯誤且格式正確，回傳 true 表示處理成功
            return true;
        }

        public override bool PostProcess()
        {
            return true;
        }

    }


    public class SN_TypeList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (GlobalNew.Devices.Count != 0)
            {
                List<string> hwListKeys = new List<string>();

                hwListKeys.Add("");

                string multiDeviceTable = string.Empty;
                foreach (var value in GlobalNew.Devices.Values)
                {
                    if (value is DUT_BASE)
                    {
                        if (((DUT_BASE)(value)).Enable)
                        {
                            
                            Dictionary<string, string>  multiDeviceTables = new Dictionary<string, string>();
                            DUT_BASE.ParseConfig(((DUT_BASE)(value)).EnvirVariable, multiDeviceTables);


                            List<string> serialKeysStripped = multiDeviceTables.Keys
                             .Where(key => key.StartsWith("SerialNumber_"))
                             .Select(key => key.Replace("SerialNumber_", ""))
                             .ToList();


                            return new StandardValuesCollection(serialKeysStripped);
                        }
                    }
                }


                return new StandardValuesCollection(hwListKeys);
            }
            else
            {
                return new StandardValuesCollection(new string[] { "" });
            }
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;

        }
    }


    public class SNInputForm : Form
    {
        private Label label;
        private TextBox textBox;

        public string SerialNumber { get; private set; }

        public SNInputForm(string labelText, string Dut_Name = "DUT")
        {
            this.Text = $"Input {Dut_Name} {labelText}";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual; // 關鍵：手動定位
            this.Width = 400;
            this.Height = 150;

            var font = new Font("Microsoft JhengHei", 15, FontStyle.Bold);

            label = new Label
            {
                Text = labelText,
                Left = 20,
                Top = 10,
                Width = 400,
                Height = 30,
                Font = font
            };

            textBox = new TextBox
            {
                Left = 20,
                Top = 50,
                Width = 340,
                Height = 35,
                Font = font
            };

            textBox.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ConfirmInput(sender, e);
                }
            };

            // 在 Shown（或 Load）事件定位：依 Owner 所在螢幕置頂置中
            this.Shown += (s, e) =>
            {
                // 取得要對齊的螢幕（優先以 Owner 為準，否則用本窗所在螢幕）
                Screen screen;
                if (this.Owner != null && this.Owner is Control ownerCtrl)
                    screen = Screen.FromControl(ownerCtrl);
                else
                    screen = Screen.FromHandle(this.Handle);

                var wa = screen.WorkingArea; // 避開工作列的可用區域
                int x = wa.Left + (wa.Width - this.Width) / 2;
                int y = wa.Top + 10; // 距離螢幕上方留 10px
                this.Location = new System.Drawing.Point(x, y);

                this.Activate();
                this.BringToFront();
            };

            // 你的停止計時器邏輯維持不變
            var timer = new System.Windows.Forms.Timer { Interval = 200 };
            timer.Tick += (s, e) =>
            {
                if (GlobalNew.g_shouldStop)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };
            timer.Start();
            this.FormClosed += (s, e) => timer.Stop();

            this.Controls.Add(label);
            this.Controls.Add(textBox);
        }

        private void ConfirmInput(object sender, EventArgs e)
        {
            this.SerialNumber = textBox.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }



}
