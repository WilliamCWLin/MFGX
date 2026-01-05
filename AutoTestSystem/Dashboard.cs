using Automation.BDaq;
using AutoTestSystem.Base;
using AutoTestSystem.BLL;
using AutoTestSystem.Model;
using AutoTestSystem.Script;
using CsvHelper;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Manufacture;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Instrumentation;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static AutoTestSystem.MainForm;
using static Manufacture.CoreBase;
using ProTreeView;

namespace AutoTestSystem
{
    public partial class Dashboard : UserControl
    {
        [JsonIgnore]
        public List<SN_Panel> SNtextBoxes;

        public event KeyPressEventHandler KeyPressHandled;

        public Dictionary<string, string> ProductKeys;

        public bool OneDutMode;

        private int failCount = 0;
        private System.Windows.Forms.Timer timer;
        private bool isBlinking; // 控制背景是否閃爍
        private bool isBlinkState; // 用於切換顏色的狀態


        private Dictionary<string, string> envirConfig;

        public ConfigurationManager ConfigManager;
        Container_MainThread CurrentThread = null;
        //TimeSpan elapsedTime;//! 系統執行時間
        DateTime startDateTime;
        DateTime endDateTime;

        public TableLayoutPanel SNPanel
        {
            get { return tableLayoutPanel_sn; }
            set { tableLayoutPanel_sn = value; }
        }

        public Label DashBoardDescription
        {
            get { return label_description; }
            set { label_description = value; }
        }

        public Label DashBoardlabel_result
        {
            get { return label_result; }
            set { label_result = value; }
        }
        
        public PictureBox BarcodePicture
        {
            get { return pictureBox_titlebarcode; }
            set { pictureBox_titlebarcode = value; }
        }
        public List<SN_Panel> g_SNtextBoxes
        {
            get { return SNtextBoxes; }
            set { SNtextBoxes = value; }
        }
        public Button Home_BTN
        {
            get { return HomeBtn; }
            set { HomeBtn = value; }
        }

        public bool BlinkingTitle
        {
            get { return isBlinking; }
            set { isBlinking = value; }
        }

        public ProTreeView.ProTreeView MainProTreeView
        {
            get { return proTreeView_testitem; }
            set { proTreeView_testitem = value; }
        }

        public TabControl _TabControl
        {
            get { return tbc_test; }
            set { tbc_test = value; }
        }

        public PictureBox ImagePicturebox
        {
            get { return pictureBox_image; }
            set { pictureBox_image = value; }
        }
        public DataGridView DataGridView
        {
            get { return dg_testitem; }
            set { dg_testitem = value; }
        }
        public TabControl TestTabControl
        {
            get { return tbc_test; }
            set { tbc_test = value; }
        }

        public RichTextBox DutLogRichTextBox
        {
            get { return richTextBox_dutLog; }
            set { richTextBox_dutLog = value; }
        }
        public TableLayoutPanel MainTableLayout
        {
            get { return tp_main_leftright; }
            set { tp_main_leftright = value; }
        }

        public TableLayoutPanel MainLogLayout
        {
            get { return tp_main_topdown; }
            set { tp_main_topdown = value; }
        }


        public Dashboard(Dictionary<string, string> EnvirConfig)
        {
            InitializeComponent();
            InitializeTrafficLights();
            envirConfig = EnvirConfig;
            ProductKeys = new Dictionary<string, string>();
            SNtextBoxes = new List<SN_Panel>();
            ConfigManager = new ConfigurationManager(EnvirConfig);
            string showproperty = string.Empty;
            string showtree = string.Empty;
            string showtitlebarcode = string.Empty;
            string strfailCount_Abort_limit = string.Empty;
            EnvirConfig.TryGetValue("Tree_PropertyShow", out showproperty);
            EnvirConfig.TryGetValue("Tree_Show", out showtree);
            EnvirConfig.TryGetValue("TitleBarcode_Show", out showtitlebarcode);


            GlobalNew.RunMode = ConfigManager.RunMode;

            if (ConfigManager.TreePropertyShow == 0)
                proTreeView_testitem.GetContainer().Panel2Collapsed = true;
            else
            {
                proTreeView_testitem.GetContainer().Panel2Collapsed = false;
                proTreeView_testitem.GetContainer().Panel2MinSize = 200;
            }

            if (ConfigManager.TreeShow == 0)
                tp_main_leftright.ColumnStyles[0].Width = 0;
            if (ConfigManager.TitleBarcodeShow == 0)
                tp_title_topdown.RowStyles[0].Height = 0;
            if (ConfigManager.SignalLightShow == 0)
                tp_info_leftright.ColumnStyles[1].Width = 0;
            if (ConfigManager.esTime == 0)
                tp_err_time_topdown.RowStyles[1].Height = 0;
            if (ConfigManager.TitleShow == 0)
                tp_info_topdown.RowStyles[0].Height = 0;


            //if (ConfigManager.AbortLimit == 0)
            //    UpdateLabelText(label_result, "PASS/FAIL", Color.Black, Color.White, 20f);
            //else
            //    UpdateLabelText(label_result, $"PASS/FAIL({ConfigManager.AbortLimit - failCount}/{ConfigManager.AbortLimit})", Color.Black, Color.White, 20f);
            //***************鎖機***************
            if (ConfigManager.AbortLimit > 0)
            {
                // 初始化 Timer
                timer = new System.Windows.Forms.Timer
                {
                    Interval = 500 // 設置閃爍間隔，單位為毫秒
                };
                timer.Tick += Timer_Tick; // 綁定 Tick 事件

                timer.Start(); // 啟動 Timer
            }


        }
        public void SetCurrentThread(Container_MainThread T)
        {
            CurrentThread = T;
        }
        private void MainProTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Manufacture.CoreBase obj = (Manufacture.CoreBase)e.Node.Tag;
                //string search = obj.Description.Substring(2).Trim();
                int len = richTextBox_dutLog.Text.IndexOf(obj.Description);
                if (len > 0)
                {
                    richTextBox_dutLog.Select(len, 0);
                    richTextBox_dutLog.ScrollToCaret();
                }
            }
        }

        public bool InitialTreeView()
        {
            string ret = MainProTreeView.Read_Recipe(GlobalNew.CurrentRecipePath);

            if (!ret.Contains("success"))
            {
                return false;
            }
            MainProTreeView.ProcessNodeMouseClick += MainProTreeNodeMouseClick;
            MainProTreeView.SetMode(ProTreeView.ProTreeView.FlowMode.Process_Mode);
            MainProTreeView.TogglePropertyGridVisibility();
            MainProTreeView.ShowTip(true);
            MainProTreeView.Drop(false);
            MainProTreeView.KeyAction(false);

            return true;
        }
        private void InitializeTrafficLights()
        {

            panelRed.BackColor = Color.Gray;
            panelYellow.BackColor = Color.Gray;
            panelGreen.BackColor = Color.Gray;

            SetPanelShape(panelRed);
            SetPanelShape(panelYellow);
            SetPanelShape(panelGreen);
        }

        public void SetTrafficLight(string color)
        {
            panelRed.BackColor = color == "Red" ? Color.Red : Color.Gray;
            panelYellow.BackColor = color == "Yellow" ? Color.Yellow : Color.Gray;
            panelGreen.BackColor = color == "Green" ? Color.Green : Color.Gray;
        }
        private void SetPanelShape(Panel panel)
        {
            panel.Width = 50;
            panel.Height = 50;
            panel.BackColor = Color.Gray;
            panel.BorderStyle = BorderStyle.FixedSingle;

            //System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            //path.AddEllipse(0, 0, panel.Width, panel.Height);
            //panel.Region = new Region(path);
        }

        public void InitialDatagridview()
        {
            //if (temp.DutDashboard != null && temp.DutDashboard.Parent != null)
            //{
            //    temp.DutDashboard.Parent.Controls.Remove(temp.DutDashboard);
            //}
            DataGridView.Columns.Clear();
            DataGridView.Rows.Clear();

            //DataGridView dataGridView = temp.DutDashboard.DataGridView;

            //// 註冊事件
            //// 設定外觀
            //dataGridView.Dock = DockStyle.Fill;

            //// 設定 RowHeaders 的寬度
            DataGridView.RowHeadersWidth = 20;
            DataGridView.AllowUserToAddRows = false;

            DataGridView.EnableHeadersVisualStyles = false;
            DataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Calibri", 11, FontStyle.Bold);
            DataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            DataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.LightBlue;
            DataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            DataGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            DataGridView.DefaultCellStyle.Font = new Font("Helvetica", 9, FontStyle.Regular);

            DataGridView.DefaultCellStyle.SelectionBackColor = DataGridView.DefaultCellStyle.BackColor;
            DataGridView.DefaultCellStyle.SelectionForeColor = DataGridView.DefaultCellStyle.ForeColor;
            ////dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            DataGridView.GridColor = Color.FromArgb(226, 226, 226);

            DataGridView.ReadOnly = true;

            DataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            //// 設定列
            DataGridView.Columns.Add("No", "No");
            DataGridView.Columns.Add("ID", "ID");
            DataGridView.Columns["ID"].Visible = false;
            DataGridView.Columns.Add("Item", "Item");
            DataGridView.Columns.Add("Spec", "Spec");
            DataGridView.Columns.Add("Value", "Value");
            DataGridView.Columns.Add("Result", "Result");
            DataGridView.Columns["Result"].HeaderCell.Style.ForeColor = Color.Blue;



            DataGridView.Columns.Add("TestTime", "TestTime(s)");
            if (GlobalNew.ShowTestTime == "1")
                DataGridView.Columns["TestTime"].Visible = true;
            else
                DataGridView.Columns["TestTime"].Visible = false;
            DataGridView.Columns.Add("Eslapse", "Eslapse(s)");
            DataGridView.Columns.Add("Retry", "Retry");
            DataGridView.Columns["Retry"].Visible = false;
            DataGridView.Columns["Eslapse"].Visible = false;
            DataGridView.Columns["No"].FillWeight = 5;
            DataGridView.Columns["Item"].FillWeight = 18;
            DataGridView.Columns["Spec"].FillWeight = 28;
            DataGridView.Columns["Spec"].Visible = false;
            DataGridView.Columns["Value"].FillWeight = 32;
            DataGridView.Columns["Result"].FillWeight = 10;
            DataGridView.Columns["TestTime"].FillWeight = 12;
            DataGridView.Columns["Eslapse"].Width = 12;
            DataGridView.Columns["Retry"].Width = 10;
            // 設定文字對齊和欄高
            DataGridView.Columns["Value"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            DataGridView.Columns["Spec"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            DataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            DataGridView.RowTemplate.Height = 120;  // 設定每一行的高度為 120


            Number = 0;
            TraverseTreeViewNodes(MainProTreeView.GetTreeview().Nodes, DataGridView);


            //// 設置 Dashboard 控制項的屬性
            //temp.DutDashboard.Dock = DockStyle.Fill;
            //temp.DutDashboard.Margin = new Padding(0);

            //// 添加 Dashboard 控制項到 TableLayoutPanel
            //this.tableLayoutPanel_datagridview.Controls.Add(temp.DutDashboard, columnIndex, 0);
        }
        public static int Number = 0;

        private void TraverseTreeViewNodes(TreeNodeCollection nodes, DataGridView dv)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is Container_JIG_INIT)
                {
                    continue;
                }

                if (node.Tag != null && node.Checked && node.Tag is ScriptBase)
                {
                    ScriptBase tagObject = (ScriptBase)node.Tag;
                    //Logger.Debug($"{tagObject.Description}");
                    if (tagObject.ShowItem == true)
                    {
                        string ShowSpec = string.Empty;

                        try
                        {
                            if (tagObject.Spec != string.Empty)
                            {
                                SpecParamsContainer specParams2 = JsonConvert.DeserializeObject<SpecParamsContainer>(tagObject.Spec);
                                if (specParams2.specParams != null)
                                {
                                    foreach (var param in specParams2.specParams)
                                    {

                                        switch (param.SpecType)
                                        {
                                            case SpecType.Range:
                                                ShowSpec += $"{param.MinLimit} < {param.Name} < {param.MaxLimit}\n";
                                                break;

                                            case SpecType.Equal:
                                                ShowSpec += $"{param.Name} = {param.SpecValue}\n";
                                                break;
                                            case SpecType.GreaterThan:
                                                ShowSpec += $"{param.Name} > {param.SpecValue}\n";

                                                break;
                                            case SpecType.LessThan:
                                                ShowSpec += $"{param.Name} < {param.SpecValue}\n";
                                                break;
                                            default:
                                                ShowSpec += "";
                                                break;
                                        }
                                    }
                                }

                            }

                        }
                        catch (Newtonsoft.Json.JsonReaderException)
                        {
                            ShowSpec += "無法解析輸入數據為 JSON 格式";
                        }
                        catch (Exception ex)
                        {
                            ShowSpec += $"處理數據時出現錯誤: {ex.Message}";
                        }

                        ShowSpec = ShowSpec.TrimEnd('\n');

                        ScriptBase.DataItem newItem = new ScriptBase.DataItem();
                        newItem.No = Number++;
                        newItem.Item = tagObject.Description;
                        if (ShowSpec == string.Empty)
                            newItem.Spec = "N/A";
                        else
                            newItem.Spec = ShowSpec;
                        //newItem.DutList = new List<string> { "Dut1", "Dut2" };
                        newItem.TestResult = "PASS";

                        object[] rowValues = { newItem.No, tagObject.ID, newItem.Item, newItem.Spec/*, newItem.Value, "PASS", newItem.TestResult, newItem.TestTime */};
                        dv.Rows.Add(rowValues);
                        dv.Rows[dv.Rows.Count - 1].Cells[2].Style.BackColor = Color.Aquamarine;
                        dv.Rows[dv.Rows.Count - 1].Cells[3].Style.BackColor = Color.Aquamarine;
                        dv.Rows[dv.Rows.Count - 1].Cells["Result"].Style.Font = new Font("Helvetica", 9, FontStyle.Bold);
                    }
                }

                if (node.Nodes.Count > 0)
                {
                    TraverseTreeViewNodes(node.Nodes, dv);
                }
            }
        }

        public void AttachEventHandler(EventHandler handler)
        {
            Home_BTN.Click += handler;
        }
        public void GenerateTextBoxes(string sn_data)
        {
            ProductKeys.Clear();
            if (!GetKeyValuePairs(ProductKeys, sn_data))
                return;

            // 清除之前生成的 TextBox 控制項
            foreach (SN_Panel textBox in SNtextBoxes)
            {
                SNPanel.Controls.Remove(textBox);
                textBox.Dispose();
            }

            SNtextBoxes.Clear();

            // 設置 TableLayoutPanel 的行和列樣式
            SNPanel.RowStyles.Clear();
            SNPanel.ColumnStyles.Clear();
            SNPanel.RowCount = ProductKeys.Count;
            SNPanel.ColumnCount = 1;

            for (int i = 0; i < SNPanel.RowCount; i++)
            {
                SNPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / SNPanel.RowCount));
            }
            SNPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));


            // 動態生成對應數量的 TextBox 控制項
            int index = 1; // 起始索引
            int maxlabel = -1;
            foreach (var kvp in ProductKeys)
            {
                string key = kvp.Key;
                string value = kvp.Value;

                SN_Panel panel = new SN_Panel();
                panel.SN_Label.Text = key + ":";
                panel.SN_Label.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                //panel.SN_Textbox.Location = new System.Drawing.Point(panel.SN_Label.Location.X + panel.SN_Label.Width + 5, 4);
                if (maxlabel < panel.SN_Label.Width)
                    maxlabel = panel.SN_Label.Width;
                panel.SN_Textbox.Location = new System.Drawing.Point(panel.SN_Label.Location.X + maxlabel + 5, 4);
                panel.SN_Textbox.Name = $"{key}";
                panel.SN_Textbox.Visible = true;
                panel.SN_Textbox.Enabled = false;
                panel.SN_Textbox.Text = "";
                panel.SN_Textbox.KeyPress += TextBox_KeyPress;
                panel.Dock = DockStyle.Fill;
                SNPanel.Controls.Add(panel);
                SNtextBoxes.Add(panel);
                if (index == 1)
                {
                    panel.SN_Textbox.Focus();
                }
                index++;
            }
        }
        public void ResetUISNtext(bool isFocusFirst = false)
        {
            foreach (var kvp in SNtextBoxes)
            {
                kvp.SN_Textbox.Invoke(new Action(() =>
                {
                    //textBox.Enabled = true;
                    kvp.SN_Textbox.Text = "";

                }));
            }
            SNtextBoxes[0].SN_Textbox.Invoke(new Action(() =>
            {
                if (SNtextBoxes.Count > 0)
                {
                    SNtextBoxes[0].SN_Textbox.Enabled = true;
                    if (isFocusFirst)
                        SNtextBoxes[0].SN_Textbox.Focus();
                }

            }));
        }

        public void LockSN_Textboxs()
        {
            foreach (var kvp in SNtextBoxes)
            {
                this.Invoke(new Action(() =>
                {
                    kvp.Enabled = false;
                    kvp.Text = "";

                }));
            }

        }
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (e.KeyChar == (char)Keys.Enter)
            {
                if (textBox != null)
                {
                    // 獲取 TextBox 中的文本
                    string text = textBox.Text;
                    foreach (var value in GlobalNew.Devices.Values)
                    {
                        if (value is DUT_BASE)
                        {
                            DUT_BASE temp = (DUT_BASE)value;
                            if (temp.Description == text)
                            {
                                //MessageBox.Show("The input format is incorrect.。");
                                textBox.Clear();
                                //textBox.Focus();
                                return;
                            }
                        }
                    }

                }
                if (!CheckTextBoxPattern(textBox))
                {
                    MessageBox.Show("The input format is incorrect.。");
                    textBox.Clear();
                    textBox.Focus();
                    return;
                }

                textBox.Enabled = false;
                SN_Panel parentPanel = textBox.Parent as SN_Panel;
                SwitchFocusToNextTextBox(SNtextBoxes, parentPanel);

                if (AllTextBoxesFilledAndPatternsCorrect())
                {
                    // 調用外部事件處理程序
                    KeyPressHandled?.Invoke(sender, e);
                    this.Focus();
                }
            }
        }
        public void Check()
        {

        }

        private bool AllTextBoxesFilledAndPatternsCorrect()
        {
            foreach (SN_Panel textBox in SNtextBoxes)
            {
                if (string.IsNullOrWhiteSpace(textBox.SN_Textbox.Text) || !CheckTextBoxPattern(textBox.SN_Textbox))
                {
                    return false;
                }
            }
            return true;
        }
        public bool CheckTextBoxPattern(TextBox textBox)
        {
            if (ProductKeys.ContainsKey(textBox.Name))
            {
                string pattern = ProductKeys[textBox.Name];
                Regex regex = new Regex(pattern);
                Match match = regex.Match(textBox.Text);

                if (match.Success)
                {
                    textBox.Text = match.Value;
                }
                return regex.IsMatch(textBox.Text);
            }
            return true;
        }

        private void SwitchFocusToNextTextBox(List<SN_Panel> SNtextBoxes, SN_Panel currentTextBox)
        {
            int currentIndex = SNtextBoxes.IndexOf(currentTextBox);

            if (currentIndex < SNtextBoxes.Count - 1)
            {
                SNtextBoxes[currentIndex + 1].SN_Textbox.Enabled = true;
                SNtextBoxes[currentIndex + 1].SN_Textbox.Focus();
            }
        }

        public bool GetKeyValuePairs(Dictionary<string, string> keyValuePairs, string sn_data)
        {
            if (string.IsNullOrEmpty(sn_data))
                return false;

            try
            {
                // 使用 String.Split 方法將多行字符串分割成字符串數組
                string[] lines = sn_data.Split(new[] { '\n' }, StringSplitOptions.None);

                foreach (string line in lines)
                {
                    if (line.Contains("=") && !line.StartsWith(";"))
                    {
                        string[] keyValue = line.Split('=');
                        string key = keyValue[0].Trim();
                        string value = keyValue[1].Trim();
                        keyValuePairs[key] = value;
                    }
                }

                if (keyValuePairs.Count > 0)
                    return true;
                else
                    return false;
            }
            catch
            {
                MessageBox.Show($"GetKeyValuePairs Fail.Data:{sn_data}");
                return false;
            }


        }

        private delegate void UpdateLabelDelegate(Label label, string text, Color? backColor = null, Color? foreColor = null, float? fontSize = null, bool isClear = false);

        public void UpdateLabelText(Label label, string text = "", Color? backColor = null, Color? foreColor = null, float? fontSize = null, bool isClear = false)
        {
            try
            {
                if (label.InvokeRequired)
                {
                    UpdateLabelDelegate d = new UpdateLabelDelegate(UpdateLabelText);
                    this.Invoke(d, new object[] { label, text, backColor, foreColor, fontSize, isClear });
                }
                else
                {
                    if (!label.IsDisposed)
                    {
                        if (isClear)
                        {
                            label.Text = string.Empty;
                            return;
                        }

                        label.Text = text;

                        if (backColor.HasValue)
                        {
                            label.BackColor = backColor.Value;
                        }

                        if (foreColor.HasValue)
                        {
                            label.ForeColor = foreColor.Value;
                        }

                        if (fontSize.HasValue)
                        {
                            label.Font = new Font(label.Font.FontFamily, fontSize.Value, label.Font.Style);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 處理例外，如紀錄日誌
                // Logger.Fatal(ex.ToString());
            }
        }
        private delegate void MainInfoRichTextDelegatePro(
    RichTextBox textbox,
    string info,
    bool isClear,
    bool isNewline,
    Color? textColor = null,
    RichTextBoxStreamType a = RichTextBoxStreamType.PlainText); // 添加可選參數 textColor

        public void MainInfoRichText(
            RichTextBox textbox,
            string info = "",
            bool isClear = false,
            bool isNewline = true,
            Color? textColor = null,
            RichTextBoxStreamType a = RichTextBoxStreamType.PlainText
            ) // 添加可選參數 textColor
        {
            try
            {
                if (textbox.InvokeRequired)
                {
                    MainInfoRichTextDelegatePro d = new MainInfoRichTextDelegatePro(MainInfoRichText);
                    this.Invoke(d, new object[] { textbox, info, isClear, isNewline, textColor, a });
                }
                else
                {
                    if (!textbox.IsDisposed)
                    {
                        if (isClear)
                        {
                            textbox.Clear();
                            return;
                        }

                        int selectionStart = textbox.TextLength; // 紀錄當前文字長度作為選擇起點

                        if (isNewline)
                        {
                            textbox.AppendText(info + "\n");
                        }
                        else
                        {
                            textbox.AppendText(info);
                        }

                        if (textColor.HasValue) // 如果提供了文字顏色
                        {
                            textbox.SelectionStart = selectionStart; // 設定選擇的開始位置
                            textbox.SelectionLength = info.Length + (isNewline ? 1 : 0); // 設定選擇範圍
                            textbox.SelectionColor = textColor.Value; // 設置文字顏色
                            textbox.SelectionLength = 0; // 清除選擇範圍


                        }
                        textbox.ScrollToCaret();
                    }
                }
            }
            catch (Exception ex)
            {
                // Logger.Fatal(ex.ToString());
            }
        }
        public void SetInfoTextboxMessage(string message)
        {
            MainInfoRichText(richTextBox_info, message);
        }

        public void ClearTestItemView(DUT_BASE CurrentTempDUT)
        {
            MainProTreeView.GetTreeview().BeginUpdate();
            MainProTreeView.ClearNodeColor();
            MainProTreeView.GetTreeview().EndUpdate();

            richTextBox_dutLog.Clear();
            richTextBox_info.Clear();

            foreach (string key in GlobalNew.DataGridViewsList.Keys)
            {
                foreach (DataGridViewRow row in GlobalNew.DataGridViewsList[key].Rows)
                {
                    row.Cells["Result"].Value = "";
                    row.Cells["Result"].Style.ForeColor = Color.Black;

                    if (key == CurrentTempDUT.Description)
                    {
                        row.Cells["Value"].Value = "";
                        row.Cells["TestTime"].Value = "";
                        row.Cells["Eslapse"].Value = "";
                        row.Cells["Retry"].Value = "";
                    }
                }
            }
        }

        public void ResetDutInfo(DUT_BASE CurrentTempDUT)
        {
            this.Invoke(new Action(() =>
            {
                MainProTreeView.GetTreeview().BeginUpdate();
                MainProTreeView.ClearNodeColor();
                MainProTreeView.GetTreeview().EndUpdate();

                if (CurrentTempDUT == null)
                {
                    //finalTestResult = "FAIL";
                    //error_details_firstfail = "Not Found DUT";
                    UpdateLabelText(label_result, "PASS/FAIL", Color.Black, Color.White, 16F);

                    //***************鎖機***************
                    //if (ConfigManager.AbortLimit == 0)
                    //    UpdateLabelText(label_result, "PASS/FAIL", Color.Red, Color.Black, 20f);
                    //else
                    //    UpdateLabelText(label_result, $"PASS/FAIL({ConfigManager.AbortLimit - failCount}/{ConfigManager.AbortLimit})", Color.Red, Color.Black, 20f);

                    UpdateLabelText(label_errorcode, "Not Found DUT");
                    CurrentTempDUT.LogMessage("Not Found DUT", MessageLevel.Fatal);
                    //SetButton(bt_errorCode, error_details_firstfail);
                    //SetTestStatus(TestStatus.FAIL);
                    return;
                }

                foreach (DataGridViewRow row in DataGridView.Rows)
                {
                    row.Cells["Result"].Value = "";
                    row.Cells["Result"].Style.ForeColor = Color.Black;


                    row.Cells["Value"].Value = "";
                    row.Cells["TestTime"].Value = "";
                    row.Cells["Eslapse"].Value = "";
                    row.Cells["Retry"].Value = "";
                }

                isUploadTestSummy = false;
                StartInit(CurrentTempDUT);
                SetTestStatus(CurrentTempDUT, TestStatus.START);

            }));

        }

        public void SetTestStatus(DUT_BASE CurrentTempDUT, TestStatus testStatus)
        {
            try
            {
                switch (testStatus)
                {
                    case TestStatus.START:

                        CurrentTempDUT.stopwatch.Reset();
                        CurrentTempDUT.stopwatch.Start();
                        UpdateLabelText(label_result, "Testing", Color.Yellow, Color.Black, 20f);
                        UpdateLabelText(label_errorcode, "N/A", Color.Transparent, Color.Black, 18f);
                        UpdateLabelText(label_elapsetime, "0s");
                        break;

                    case TestStatus.FAIL:
                        CurrentTempDUT.stopwatch.Stop();
                        CurrentTempDUT.stopwatch.Reset();
                        string error_details_firstfail = CurrentTempDUT.DataCollection.GetMoreProp("Failitem");
                        UpdateLabelText(label_errorcode, error_details_firstfail, Color.Red, Color.Black, 12f);


                        TestFailed(CurrentTempDUT);

                        //***************鎖機***************
                        if (ConfigManager.AbortLimit == 0)
                            UpdateLabelText(label_result, "FAIL", Color.Red, Color.Black, 20f);
                        else
                        {

                            string flag = INIHelper.Readini("CountNum", "ABORT_FLAG", Global.IniConfigFile);
                            if (flag == "1")
                            {
                                UpdateLabelText(label_result, $"FAIL({ConfigManager.AbortLimit - failCount}/{ConfigManager.AbortLimit})", Color.Red, Color.Black, 20f);
                            }
                            else
                            {
                                UpdateLabelText(label_result, $"FAIL({ConfigManager.AbortLimit - failCount}/{ConfigManager.AbortLimit})", Color.Red, Color.Black, 20f);
                            }
                        }

                        break;

                    case TestStatus.PASS:
                        CurrentTempDUT.stopwatch.Stop();
                        CurrentTempDUT.stopwatch.Reset();
                        TestPassed(CurrentTempDUT);
                        UpdateLabelText(label_result, "PASS", Color.Green, Color.Black, 20f);
                        break;

                    case TestStatus.IDLE:
                        UpdateLabelText(label_result, "IDLE", Color.Aqua, Color.Black, 20f);
                        break;
                    case TestStatus.ABORT:
                        //***************鎖機***************
                        if (ConfigManager.AbortLimit == 0)
                            UpdateLabelText(label_result, "FAIL", Color.Red, Color.Black, 20f);
                        else
                            UpdateLabelText(label_result, $"FAIL({ConfigManager.AbortLimit - failCount}/{ConfigManager.AbortLimit})\nMachine Locked!", Color.DarkRed, Color.Yellow, 18f);

                        isBlinking = true;
                        tableLayoutPanel_sn.Invoke((MethodInvoker)delegate
                        {
                            tableLayoutPanel_sn.Enabled = false;
                        });

                        try
                        {
                            INIHelper.Writeini("CountNum", "ABORT_FLAG", "1", Global.IniConfigFile);
                        }
                        catch (Exception ex)
                        {
                            Bd.Logger.Debug(ex.ToString());
                        }

                        //UpdateLabelText(label_result, "ABORT", Color.DarkRed, Color.Yellow, 18f);
                        CurrentTempDUT.stopwatch.Stop();
                        CurrentTempDUT.stopwatch.Reset();
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                CurrentTempDUT.LogMessage(ex.ToString());
            }
            finally
            {
                try
                {
                    switch (testStatus)
                    {
                        case TestStatus.START:
                            if (GlobalNew.stopwatch.IsRunning)
                            {
                                GlobalNew.stopwatch.Reset();
                            }
                            GlobalNew.stopwatch.Start();
                            break;

                        case TestStatus.FAIL:
                        case TestStatus.PASS:
                        case TestStatus.ABORT:
                            GlobalNew.stopwatch.Stop();
                            GlobalNew.stopwatch.Reset();

                            break;

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    CurrentTempDUT.LogMessage("SetTestStatus finally Exception:" + ex.ToString());
                    //throw;
                }
                finally
                {
                }

            }
        }
        public void MemoryDataClear(DUT_BASE CurrentTempDUT)
        {
            try
            {
                CurrentTempDUT.TestInfo.ClearTestSteps();
                CurrentTempDUT.DataCollection.Clear();
                CurrentTempDUT.DataCollection.SetMoreProp("Failitem", "");
                TraverseTreeViewClearDataItem(CurrentTempDUT.DutDashboard.MainProTreeView.GetTreeview().Nodes);
            }
            catch (Exception ex)
            {
                CurrentTempDUT.LogMessage($"MemoryDataClear Fail.Message:{ex.Message}", MessageLevel.Fatal);
            }
        }

        private void TraverseTreeViewClearDataItem(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                // 對節點的 Tag 做一些事情
                if (node.Tag is ScriptBase)
                {
                    // 清除測試資料
                    ScriptBase tagObject = (ScriptBase)node.Tag;
                    tagObject.RowDataItem.Clear();
                    tagObject.RowDataItem.RetryTimes = 0;
                    tagObject.RowDataItem.TestCount = 0;
                }
                if (node.Tag is CoreBase)
                {
                    // 清除測試資料
                    CoreBase tagObject = (CoreBase)node.Tag;
                    tagObject.DurationSec = -1;
                    tagObject.RetryCount = -1;
                    tagObject.StartTime = DateTime.Now;
                    tagObject.EndTime = DateTime.Now;
                    tagObject.Result = "N/A";

                }
                // 如果節點有子節點，遞迴地呼叫 TraverseTreeView 方法
                if (node.Nodes.Count > 0)
                {
                    TraverseTreeViewClearDataItem(node.Nodes);
                }
            }
        }
        public void StartInit(DUT_BASE tempDUT)
        {
            MainInfoRichText(richTextBox_info, $"", true);
            if (GlobalNew.RunMode == 0)
                SaveRichTextPro(tempDUT, DutLogRichTextBox, true);
            //MainInfoRichText(richTextBox_info, $"=======Devices Info=======");
            //foreach (var value in GlobalNew.Devices.Values)
            //{
            //    string status = string.Empty;
            //    switch (value)
            //    {
            //        case DUT_BASE D:

            //            D.Status(ref status);
            //            MainInfoRichText(richTextBox_info, $"{D.Description} : {status}");

            //            break;
            //        case Manufacture.Equipment D:

            //            D.Status(ref status);
            //            MainInfoRichText(richTextBox_info, $"{D.Description} : {status}");

            //            break;
            //    }
            //}
            //MainInfoRichText(richTextBox_info, $"");
            MainInfoRichText(richTextBox_info, $"=======Test Info=======");

            if (tempDUT != null)
            {
                tempDUT.SwitchTabControlIndex(0);
                foreach (SN_Panel panel in SNtextBoxes)
                {
                    tempDUT.DataCollection.SetMoreProp(panel.SN_Textbox.Name, panel.SN_Textbox.Text);
                    GlobalNew.g_datacollection.SetMoreProp(panel.SN_Textbox.Name, panel.SN_Textbox.Text);
                    MainInfoRichText(richTextBox_info, $"{panel.SN_Textbox.Name} : {panel.SN_Textbox.Text}");

                    tempDUT.LogMessage($"SetMoreProp:{panel.SN_Textbox.Name} -> {panel.SN_Textbox.Text}");
                }
                tempDUT.DataCollection.SetMoreProp("DataFolder", GlobalNew.LOGFOLDER);
                tempDUT.DataCollection.SetMoreProp("ServerFolder", GlobalNew.ServerLOGFOLDER);
                tempDUT.DataCollection.SetMoreProp("WorkID", GlobalNew.CurrentUser);
                //MainInfoRichText(MainInfoRichTextBox,$"WorkID : {GlobalNew.CurrentUser}");
                string StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                startDateTime = DateTime.Parse(StartTime);

                tempDUT.DataCollection.SetMoreProp("StartTime", $"\"{StartTime}\"");
                tempDUT.DataCollection.SetMoreProp("StationName", GlobalNew.CurrentStation);
                tempDUT.DataCollection.SetMoreProp("ProjectName", GlobalNew.CurrentProject);
                tempDUT.DataCollection.SetMoreProp("RunMode", GlobalNew.CurrentMode);
                tempDUT.DataCollection.SetMoreProp("FixtureName", GlobalNew.CurrentFixture);
                tempDUT.DataCollection.SetMoreProp("FixturePart", tempDUT.Description);
                tempDUT.DataCollection.SetMoreProp("ConfigVersion", GlobalNew.CurrentConfigVersion);
                tempDUT.DataCollection.SetMoreProp("OTPKey", GlobalNew.OTPKey);

                StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                MainInfoRichText(richTextBox_info, $"WorkID : {GlobalNew.CurrentUser}");
                MainInfoRichText(richTextBox_info, $"Start Time : {StartTime}");
                try
                {
                    // 取得目前正在執行的程式的進程
                    Process currentProcess = Process.GetCurrentProcess();
                    // 取得進程的主模組，即執行檔本身
                    ProcessModule mainModule = currentProcess.MainModule;
                    // 取得執行檔的路徑
                    string exePath = mainModule.FileName;

                    // 建立 FileInfo 物件以取得檔案資訊
                    FileInfo fileInfo = new FileInfo(exePath);

                    // 取得執行檔的版本資訊
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(exePath);
                    string exeName = System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString();

                    tempDUT.TestInfo.AddTestStep($"Title", $"Application Release Time:{fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                    tempDUT.TestInfo.AddTestStep($"Title", $"Application Version:{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
                    tempDUT.TestInfo.AddTestStep($"Title", $"Test Computer Name:{Environment.MachineName}");
                    tempDUT.TestInfo.AddTestStep($"Title", $"Test Computer User Name:{Environment.UserName}");
                    if (Environment.Is64BitOperatingSystem)
                        tempDUT.TestInfo.AddTestStep($"Title", $"Test Computer Operating System Bit: 64");
                    else
                        tempDUT.TestInfo.AddTestStep($"Title", $"Test Computer Operating System Bit: 32");

                    if (Environment.Is64BitProcess)
                        tempDUT.TestInfo.AddTestStep($"Title", $"Test Process System Bit: 64");
                    else
                        tempDUT.TestInfo.AddTestStep($"Title", $"Test Process System Bit: 32");

                    FileInfo ConfigfileInfo = new FileInfo(GlobalNew.CurrentRecipePath);

                    tempDUT.TestInfo.AddTestStep($"Title", $"Test Config File Name:{System.IO.Path.GetFileName(GlobalNew.CurrentRecipePath)}");
                    tempDUT.TestInfo.AddTestStep($"Title", $"Test Config File LastWriteTime :{ConfigfileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                    tempDUT.TestInfo.AddTestStep($"Title", $"Login User :{GlobalNew.CurrentUser}");

                    // 取得 CPU 名稱與核心數
                    string cpuName = "";
                    try
                    {
                        using (var searcher = new ManagementObjectSearcher("select Name from Win32_Processor"))
                        {
                            foreach (var item in searcher.Get())
                            {
                                cpuName = item["Name"]?.ToString()?.Trim();
                                break;
                            }
                        }
                    }
                    catch { cpuName = "Unknown"; }

                    tempDUT.TestInfo.AddTestStep($"Title", $"CPU Info: {cpuName}, Cores: {Environment.ProcessorCount}");

                    // 取得記憶體資訊
                    try
                    {
                        using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize,FreePhysicalMemory FROM Win32_OperatingSystem"))
                        {
                            foreach (var item in searcher.Get())
                            {
                                ulong totalMemoryKB = Convert.ToUInt64(item["TotalVisibleMemorySize"]);
                                ulong freeMemoryKB = Convert.ToUInt64(item["FreePhysicalMemory"]);
                                ulong usedMemoryKB = totalMemoryKB - freeMemoryKB;

                                // 轉換為 GB，並保留小數點後 1 位
                                double totalMemoryGB = totalMemoryKB / 1024.0 / 1024.0;
                                double usedMemoryGB = usedMemoryKB / 1024.0 / 1024.0;

                                string memoryInfo = $"Memory : {totalMemoryGB:F1} GB";
                                tempDUT.TestInfo.AddTestStep("Title", memoryInfo);
                            }
                        }
                    }
                    catch
                    {
                        tempDUT.TestInfo.AddTestStep($"Title", $"Memory Info: Unknown");
                    }

                    // 取得本機 IP 及 MAC
                    try
                    {

                        string ipAddress = "N/A";
                        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                        foreach (var ip in host.AddressList)
                        {
                            if (ip.AddressFamily == AddressFamily.InterNetwork)
                            {
                                ipAddress = ip.ToString();
                                break;
                            }
                        }
                        tempDUT.TestInfo.AddTestStep($"Title", $"Local IP Address: {ipAddress}");

                        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                            .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                        foreach (var nic in interfaces)
                        {
                            var mac = string.Join("-", nic.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")));
                            string interfaceInfo = $"{nic.NetworkInterfaceType} / {nic.Description}";
                            tempDUT.TestInfo.AddTestStep("Title", $"MAC Address ({interfaceInfo}): {mac}");

                            break; // 只取第一張啟用的網卡
                        }
                    }
                    catch
                    {
                        tempDUT.TestInfo.AddTestStep($"Title", $"Network Info: Unknown");
                    }
                }
                catch (Exception ex)
                {
                    tempDUT.LogMessage(($"TestInfo Get Error.{ex.Message}"), Manufacture.CoreBase.MessageLevel.Error);
                }
            }

        }
        private bool isUploadTestSummy = false;
        public void TestSummary(DUT_BASE tempDUT, bool result)
        {
            if (isUploadTestSummy)
                return;
            string EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            endDateTime = DateTime.Parse(EndTime);
            TimeSpan elapsedTime = endDateTime - startDateTime;

            string elapsedTimeString = string.Format("{0:hh\\:mm\\:ss\\.fff}", elapsedTime);
            double totalSeconds = elapsedTime.TotalSeconds;
            tempDUT.DataCollection.SetMoreProp("TestResult", result ? "PASS" : "FAIL");
            tempDUT.DataCollection.SetMoreProp("EndTotalTime", elapsedTimeString);
            tempDUT.DataCollection.SetMoreProp("EndTime", $"\"{EndTime}\"");
            tempDUT.TestInfo.AddTestStep($"Test Summary", $"ProductSN: {tempDUT.DataCollection.GetMoreProp("ProductSN")}");
            tempDUT.TestInfo.AddTestStep($"Test Summary", $"Start Test Time: {tempDUT.DataCollection.GetMoreProp("StartTime")}");

            GlobalNew.LogPath = $@"{GlobalNew.LOGFOLDER}\Log\{GlobalNew.CurrentProject}\{GlobalNew.CurrentMode}\{GlobalNew.CurrentStation}\{GlobalNew.CurrentFixture}\{DateTime.Now.ToString("yyyyMMdd")}";
            string ServerPath = $@"{GlobalNew.ServerLOGFOLDER}\Log\{GlobalNew.CurrentProject}\{GlobalNew.CurrentMode}\{GlobalNew.CurrentStation}\{GlobalNew.CurrentFixture}\{DateTime.Now.ToString("yyyyMMdd")}";
            GlobalNew.csvLogPath = $@"{GlobalNew.LOGFOLDER}\CSV\{GlobalNew.CurrentProject}\{GlobalNew.CurrentMode}\{GlobalNew.CurrentStation}\{GlobalNew.CurrentFixture}\{DateTime.Now.ToString("yyyyMM")}";
            string DailyCSV = $@"{GlobalNew.LOGFOLDER}\CSV\{GlobalNew.CurrentProject}\{GlobalNew.CurrentMode}\{GlobalNew.CurrentStation}\{GlobalNew.CurrentFixture}\{DateTime.Now.ToString("yyyyMMdd")}";
            string TotalPASSFAIL = $@"{GlobalNew.LOGFOLDER}\CSV\{GlobalNew.CurrentProject}\{GlobalNew.CurrentMode}\{GlobalNew.CurrentStation}\{GlobalNew.CurrentFixture}";

            string CSVFile_path = $"{DailyCSV}\\{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result.csv";//csv path
            string CSVFile_path_PASS = $"{TotalPASSFAIL}\\PASS_{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result.csv";
            string CSVFile_path_FAIL = $"{TotalPASSFAIL}\\FAIL_{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result.csv";
            string logpath = string.Empty;
            string SN_Define = ReplaceProp(tempDUT, ConfigManager.LogFileDefineName);
            string Fixture_Port = ConfigManager.FixturePort;
            int UseItemCode = ConfigManager.LogUseItemCode;
            string IReportLogPath = string.Empty;
            string tmp_log = tempDUT.LOGGER.LogFilePath;
            int RecordCount = 0;
            if (result)
            {
                IReportLogPath = $"{SN_Define}_{totalSeconds.ToString("0.00")}";
                //================WRITE Report================
                tempDUT.DataCollection.SetMoreProp("Result", "PASS");
                tempDUT.TestInfo.AddTestStep($"Test Summary", $"TestResult: PASS");
                tempDUT.TestInfo.AddTestStep($"Test Summary", $"TotalTestTime: {totalSeconds.ToString("0.00")}s");
                tempDUT.TestInfo.AddTestStep($"Test Summary", $"End Test Time: {tempDUT.DataCollection.GetMoreProp("EndTime")}");
                string passReportPath = $@"{GlobalNew.LOGFOLDER}\\Report\\{GlobalNew.CurrentProject}\\{GlobalNew.CurrentStation}\\{GlobalNew.CurrentFixture}\\{GlobalNew.CurrentMode}\\{DateTime.Now.ToString("yyyyMMdd")}\\PASS\\{tempDUT.DataCollection.GetMoreProp("ProductSN")}_{DateTime.Now.ToString("yyyMMdd_HHmmss")}.txt";
                string passPDFReportPath = $@"{GlobalNew.LOGFOLDER}\\Report\\{GlobalNew.CurrentProject}\\{GlobalNew.CurrentStation}\\{GlobalNew.CurrentFixture}\\{GlobalNew.CurrentMode}\\{DateTime.Now.ToString("yyyyMMdd")}\\PASS\\{tempDUT.DataCollection.GetMoreProp("ProductSN")}_{DateTime.Now.ToString("yyyMMdd_HHmmss")}.pdf";
                string passReportJsonPath = $@"{GlobalNew.LOGFOLDER}\\Report\\{GlobalNew.CurrentProject}\\{GlobalNew.CurrentStation}\\{GlobalNew.CurrentFixture}\\{GlobalNew.CurrentMode}\\{DateTime.Now.ToString("yyyyMMdd")}\\PASS\\{tempDUT.DataCollection.GetMoreProp("ProductSN")}_{DateTime.Now.ToString("yyyMMdd_HHmmss")}.json";
                
                try
                {
                    WriteReport(tempDUT, passReportPath);
                    WriteResult(passReportJsonPath);
                }
                catch (Exception ex)
                {

                }
                //================WRITE CSV================
                RecordCount = WriteCSVResult(CSVFile_path_PASS, tempDUT, false);


                //================WRITE UI================
                SetInfoTextboxMessage($"Test Result : PASS");

                //================WRITE LOG================
                string cellLogPath = $@"PASS\{tempDUT.DataCollection.GetMoreProp("ProductSN")}_{DateTime.Now.ToString("HH-mm-ss")}.txt";
                logpath = cellLogPath;
                //SaveRichTextPro(tempDUT, richTextBox_dutLog, false, cellLogPath);
                //SaveRichTextBoxLog($"{tempDUT.DataCollection.GetMoreProp("DataPassMetaPath")}\\{tempDUT.DataCollection.GetMoreProp("ProductSN")}_{DateTime.Now.ToString("HH_mm_ss")}.txt");
                GlobalNew.IncrementPassCount();
                //System.Threading.Interlocked.Increment(ref GlobalNew.Total_Pass_Num);
                if (RecordCount > 0)
                {
                    TestFileInfo PassCsv = new TestFileInfo
                    {
                        SourcePath = CSVFile_path_PASS,
                        FileName = "Pass",
                        SubFolder = "",
                        IncludeTimeFolder = false,
                        IncludeResultFolder = false,
                        DeleteSourceAfterBackup = false
                    };
                    tempDUT.FileManager?.AddFile(PassCsv);
                }


                TestFileInfo PassReport = new TestFileInfo
                {
                    SourcePath = passPDFReportPath,
                    FileName = $"{SN_Define}",
                    SubFolder = tempDUT.DataCollection.GetMoreProp("ProductSN"),
                    DeleteSourceAfterBackup = false
                };
                tempDUT.FileManager?.AddFile(PassReport);

                TestFileInfo PassResult = new TestFileInfo
                {
                    SourcePath = passReportJsonPath,
                    FileName = $"{SN_Define}",
                    SubFolder = "Result",
                    DeleteSourceAfterBackup = false
                };
                tempDUT.FileManager?.AddFile(PassResult);
            }
            else
            {
                string ErrorCode = tempDUT.DataCollection.GetMoreProp("Failitem");

                IReportLogPath = $"{SN_Define}_{(UseItemCode == 1 ? DataManager.GetKeyByValue(ErrorCode) : ErrorCode)}_{totalSeconds.ToString("0.00")}";
                //================WRITE Report================
                tempDUT.DataCollection.SetMoreProp("Result", "FAIL");
                tempDUT.TestInfo.AddTestStep($"Test Summary", $"TestResult: FAIL ({ErrorCode})");
                tempDUT.TestInfo.AddTestStep($"Test Summary", $"TotalTestTime: {totalSeconds.ToString("0.00")}s");
                tempDUT.TestInfo.AddTestStep($"Test Summary", $"End Test Time: {tempDUT.DataCollection.GetMoreProp("EndTime")}");
                string FailReprotPath = $@"{GlobalNew.LOGFOLDER}\\Report\\{GlobalNew.CurrentProject}\\{GlobalNew.CurrentStation}\\{GlobalNew.CurrentFixture}\\{GlobalNew.CurrentMode}\\{DateTime.Now.ToString("yyyyMMdd")}\\FAIL\\{tempDUT.DataCollection.GetMoreProp("ProductSN")}{ErrorCode}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
                string FailPDFReprotPath = $@"{GlobalNew.LOGFOLDER}\\Report\\{GlobalNew.CurrentProject}\\{GlobalNew.CurrentStation}\\{GlobalNew.CurrentFixture}\\{GlobalNew.CurrentMode}\\{DateTime.Now.ToString("yyyyMMdd")}\\FAIL\\{tempDUT.DataCollection.GetMoreProp("ProductSN")}{ErrorCode}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.pdf";
                string FailJsonPath = $@"{GlobalNew.LOGFOLDER}\\Report\\{GlobalNew.CurrentProject}\\{GlobalNew.CurrentStation}\\{GlobalNew.CurrentFixture}\\{GlobalNew.CurrentMode}\\{DateTime.Now.ToString("yyyyMMdd")}\\FAIL\\{tempDUT.DataCollection.GetMoreProp("ProductSN")}{ErrorCode}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.json";

                try
                {
                    WriteReport(tempDUT, FailReprotPath);
                    WriteResult(FailJsonPath);
                }
                catch (Exception ex)
                {

                }
                //================WRITE CSV================
                if (GlobalNew.g_shouldStop != true)
                {
                    RecordCount = WriteCSVResult(CSVFile_path_FAIL, tempDUT, false);
                }

                //================WRITE UI================
                SetInfoTextboxMessage($"Test Result : \"FAIL\"");
                SetInfoTextboxMessage($"Error Code : {tempDUT.DataCollection.GetMoreProp("Failitem")}");

                //================WRITE LOG================
                string cellLogPath = $@"FAIL\{tempDUT.DataCollection.GetMoreProp("ProductSN")}_{ErrorCode}_{DateTime.Now.ToString("HH-mm-ss")}.txt";
                logpath = cellLogPath;
                //SaveRichTextPro(tempDUT, richTextBox_dutLog, false, cellLogPath);
                //SaveRichTextBoxLog($"{tempDUT.DataCollection.GetMoreProp("DataFailMetaPath")}\\{tempDUT.DataCollection.GetMoreProp("ProductSN")}_{ErrorCode}_{DateTime.Now.ToString("HH_mm_ss")}.txt");
                GlobalNew.IncrementFailCount();
                if (GlobalNew.g_shouldStop != true)
                {
                    if (RecordCount > 0)
                    {
                        TestFileInfo FailCsv = new TestFileInfo
                        {
                            SourcePath = CSVFile_path_FAIL,
                            FileName = "Fail",
                            SubFolder = "",
                            IncludeTimeFolder = false,
                            IncludeResultFolder = false,
                            DeleteSourceAfterBackup = false
                        };
                        tempDUT.FileManager?.AddFile(FailCsv);
                    }
                }

                TestFileInfo FailReport = new TestFileInfo
                {
                    SourcePath = FailPDFReprotPath,
                    FileName = $"{SN_Define}_{ErrorCode}",
                    SubFolder = tempDUT.DataCollection.GetMoreProp("ProductSN"),
                    DeleteSourceAfterBackup = false
                };

                tempDUT.FileManager?.AddFile(FailReport);

                TestFileInfo FailResult = new TestFileInfo
                {
                    SourcePath = FailJsonPath,
                    FileName = $"{SN_Define}_{(UseItemCode == 1 ? DataManager.GetKeyByValue(ErrorCode) : ErrorCode)}",
                    SubFolder = "Result",
                    DeleteSourceAfterBackup = false
                };

                tempDUT.FileManager?.AddFile(FailResult);
            }
            string Failitem = tempDUT.DataCollection.GetMoreProp("Failitem");
            if (GlobalNew.g_shouldStop != true)
            {
                RecordCount = WriteCSVResult(CSVFile_path, tempDUT, true);//Total log in csv

                if (RecordCount > 0)
                {
                    TestFileInfo CSV_Daily = new TestFileInfo
                    {
                        SourcePath = CSVFile_path,
                        FileName = "Daily",
                        SubFolder = "",
                        IncludeResultFolder = false,
                        DeleteSourceAfterBackup = false
                    };

                    tempDUT.FileManager?.AddFile(CSV_Daily);
                }
            }





            SetInfoTextboxMessage($"End Time: {EndTime}");
            SetInfoTextboxMessage($"Total Time : {totalSeconds.ToString("0.00")}s");


            Thread.Sleep(200);
            ////備份到原本舊的LOG路徑
            ////=============================
            string srcLogPath = string.Empty;
            string srcServerLogPath = string.Empty;

            srcServerLogPath = $@"{ServerPath}\{logpath}";
            srcLogPath = $@"{GlobalNew.LogPath}\{logpath}";

            CopyFile(tmp_log, srcLogPath);
            CopyFile(tmp_log, srcServerLogPath);
            ////=============================
            TestFileInfo LogFileInfo = new TestFileInfo
            {
                SourcePath = tmp_log,
                FileName = IReportLogPath,
                SubFolder = tempDUT.DataCollection.GetMoreProp("ProductSN"),
                DeleteSourceAfterBackup = false
            };
            
            Thread.Sleep(100);
            tempDUT.FileManager?.AddFile(LogFileInfo);
            List<string> backupPaths = ReplaceProp(tempDUT,DUT_BASE.ParseSectionData(tempDUT.EnvirVariable, "Path"))
             .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
             .Select(line => line.Split('=')[1])
             .ToList();
            tempDUT.FileManager.SetbackupDirs(backupPaths);
            tempDUT.FileManager.BackupAll(result ? "PASS" : "FAIL");


            isUploadTestSummy = true;
        }

        public string ReplaceProp(DUT_BASE Dutmp, string input_string)
        {
            string originInput = input_string;
            // 正規表達式來匹配 %任意文字%
            Regex regex = new Regex(@"%([^%]+)%");

            // 尋找匹配的 %%
            MatchCollection matches = regex.Matches(input_string);

            // 迭代每個匹配
            foreach (Match match in matches)
            {
                // 取得匹配的 key
                string key = match.Groups[1].Value;

                // 檢查是否為特殊的 Time 鍵
                if (key.StartsWith("NowTime"))
                {
                    // 如果是標準的 %Time% 佔位符
                    if (key.Length == 7)
                    {
                        // 使用標準格式替換 %Time%
                        input_string = input_string.Replace(match.Value, DateTime.Now.ToString("yyyyMMddHHmmss"));
                    }
                    else
                    {
                        // 處理自定義格式的時間佔位符
                        // 從 key 中提取出時間格式
                        string customFormat = key.Substring(7);
                        // 替換自定義格式的時間佔位符
                        input_string = input_string.Replace(match.Value, DateTime.Now.ToString(customFormat));
                    }
                }
                else if (key.StartsWith("UTCTime"))
                {
                    // 如果是標準的 %UTCTime% 佔位符
                    if (key.Length == 7)
                    {
                        // 使用標準格式替換 %UTCTime%
                        input_string = input_string.Replace(match.Value, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
                    }
                    else
                    {
                        // 處理自定義格式的時間佔位符
                        // 從 key 中提取出時間格式
                        string customFormat = key.Substring(7);
                        // 替換自定義格式的時間佔位符
                        input_string = input_string.Replace(match.Value, DateTime.UtcNow.ToString(customFormat));
                    }
                }
                else if (Dutmp != null)
                {
                    // 使用 GetMoreProp 方法取得對應的 value 並進行替換
                    string value = Dutmp.DataCollection.GetMoreProp(key);
                    input_string = input_string.Replace(match.Value, value);
                }
                else
                {
                    // 如果沒有匹配的 key，則移除佔位符
                    input_string = input_string.Replace(match.Value, "");
                }
            }

            return input_string;
        }
        public void WriteReport(DUT_BASE tmp, string filePath)
        {
            lock (_Reportlock)
            {
                string directoryPath = Path.GetDirectoryName(filePath);

                // 檢查目錄是否存在，如果不存在則創建
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (StreamWriter file = new StreamWriter(filePath))
                {
                    string[] headers = { "No", "Item", "Result", "TestTime", "Eslapse", "Retry" };
                    string formattedHeader = String.Format("{0,-4} {1,-35} {2,-8} {3,-10} {4,-8} {5,-10}", headers);
                    tmp.TestInfo.AddTestStep("Table", formattedHeader);

                    foreach (DataGridViewRow row in tmp.DataGridView.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            StringBuilder rowData = new StringBuilder();

                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                if (headers.Contains(cell.OwningColumn.Name))
                                {
                                    string value = cell.Value?.ToString() ?? ""; // null 安全處理
                                    rowData.Append(value).Append("\t");
                                }
                            }
                            string formattedStep = String.Format("{0,-4} {1,-35} {2,-8} {3,-10} {4,-8} {5,-10}", rowData.ToString().Split('\t'));
                            tmp.TestInfo.AddTestStep("Table", formattedStep);
                        }
                    }
                }

                //Variable Table
                string[] Varheaders = { "Key", "Value" };
                string VarformattedHeader = String.Format("{0,-30} {1,-55}", Varheaders);
                tmp.TestInfo.AddTestStep("Variable", VarformattedHeader);
                foreach (var item in tmp.DataCollection.GetData())
                {
                    StringBuilder rowData = new StringBuilder();

                    rowData.Append(item.Key + "\t" + item.Value);

                    string formattedStep = String.Format("{0,-30} {1,-55}", rowData.ToString().Split('\t'));
                    tmp.TestInfo.AddTestStep("Variable", formattedStep);
                }

                //MES Table
                string[] MESheaders = { "No.", "Value" };
                string MESformattedHeader = String.Format("{0,-10} {1,-35}", MESheaders);
                tmp.TestInfo.AddTestStep("MES", MESformattedHeader);
                int no_count = 0;
                foreach (var item in tmp.DataCollection.GetMESData())
                {
                    StringBuilder rowData = new StringBuilder();

                    rowData.Append(no_count++ + "\t" + item.Value);

                    string formattedStep = String.Format("{0,-10} {1,-35}", rowData.ToString().Split('\t'));
                    tmp.TestInfo.AddTestStep("MES", formattedStep);
                }

                tmp.TestInfo.ExportToPdf(filePath.Replace(".txt", ".pdf"));

                tmp.TestInfo.ExportToFile(filePath);
                if (!string.IsNullOrEmpty(GlobalNew.ServerLOGFOLDER))
                {
                    string des = filePath.Replace(GlobalNew.LOGFOLDER, GlobalNew.ServerLOGFOLDER);
                    CopyFile(filePath, des);
                }
            }
        }
        public void ShowImagesInTab(string tabName, List<string> imagePaths)
        {
            if (imagePaths == null || imagePaths.Count == 0)
            {
                //MessageBox.Show("無圖片可顯示", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            this.Invoke((Action)(() =>
            {
                var panel = new DoubleBufferedPanel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black
                };

                float zoom = 1.0f;
                Point panOffset = Point.Empty;
                Point lastMousePos = Point.Empty;
                bool isDragging = false;
                bool isZoomedIn = false;
                string zoomedImagePath = null;
                Dictionary<Rectangle, string> imageMap = new Dictionary<Rectangle, string>();

                panel.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.TranslateTransform(panOffset.X, panOffset.Y);
                    g.ScaleTransform(zoom, zoom);

                    imageMap.Clear();

                    if (isZoomedIn && zoomedImagePath != null)
                    {
                        try
                        {
                            using (Image img = Image.FromFile(zoomedImagePath))
                            {
                                float scale = Math.Min((float)panel.Width / img.Width, (float)panel.Height / img.Height);
                                int drawWidth = (int)(img.Width * scale);
                                int drawHeight = (int)(img.Height * scale);
                                int x = (panel.Width - drawWidth) / 2;
                                int y = (panel.Height - drawHeight) / 2;

                                g.DrawImage(img, new Rectangle(x, y, drawWidth, drawHeight));
                            }
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show($"圖片載入失敗：{zoomedImagePath} - {ex.Message}");
                        }
                        return;
                    }

                    int totalImages = imagePaths.Count;
                    int columns = (int)Math.Ceiling(Math.Sqrt(totalImages));
                    int rows = (int)Math.Ceiling((double)totalImages / columns);
                    int cellWidth = panel.Width / columns;
                    int cellHeight = panel.Height / rows;
                    int padding = 10;

                    int count = 0;
                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < columns; c++)
                        {
                            if (count >= imagePaths.Count) break;

                            int cellX = c * cellWidth;
                            int cellY = r * cellHeight;

                            g.FillRectangle(Brushes.DarkGray, new Rectangle(cellX, cellY, cellWidth, cellHeight));

                            try
                            {
                                using (Image img = Image.FromFile(imagePaths[count]))
                                {
                                    float scale = Math.Min((float)(cellWidth - 2 * padding) / img.Width,
                                                           (float)(cellHeight - 2 * padding) / img.Height);
                                    int drawWidth = (int)(img.Width * scale);
                                    int drawHeight = (int)(img.Height * scale);
                                    int x = cellX + (cellWidth - drawWidth) / 2;
                                    int y = cellY + (cellHeight - drawHeight) / 2;

                                    var imageRect = new Rectangle(x, y, drawWidth, drawHeight);
                                    imageMap[imageRect] = imagePaths[count];

                                    g.DrawImage(img, imageRect);
                                }
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show($"圖片載入失敗：{imagePaths[count]} - {ex.Message}");
                            }

                            count++;
                        }
                    }
                };

                panel.MouseWheel += (s, e) =>
                {
                    Point mousePos = e.Location;
                    PointF logicalPos = new PointF(
                        (mousePos.X - panOffset.X) / zoom,
                        (mousePos.Y - panOffset.Y) / zoom
                    );

                    zoom += e.Delta > 0 ? 0.1f : -0.1f;
                    zoom = Math.Max(0.2f, Math.Min(zoom, 5.0f));

                    panOffset.X = (int)(mousePos.X - logicalPos.X * zoom);
                    panOffset.Y = (int)(mousePos.Y - logicalPos.Y * zoom);

                    panel.Invalidate();
                };

                panel.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        isDragging = true;
                        lastMousePos = e.Location;
                        panel.Cursor = Cursors.Hand;
                    }
                };

                panel.MouseMove += (s, e) =>
                {
                    if (isDragging)
                    {
                        var delta = new Point(e.X - lastMousePos.X, e.Y - lastMousePos.Y);
                        panOffset.X += delta.X;
                        panOffset.Y += delta.Y;
                        lastMousePos = e.Location;
                        panel.Invalidate();
                    }
                };

                panel.MouseUp += (s, e) =>
                {
                    isDragging = false;
                    panel.Cursor = Cursors.Default;
                };

                panel.MouseDoubleClick += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left && !isZoomedIn)
                    {
                        foreach (var kvp in imageMap)
                        {
                            if (kvp.Key.Contains(e.Location))
                            {
                                isZoomedIn = true;
                                zoomedImagePath = kvp.Value;
                                zoom = 1.0f;
                                panOffset = Point.Empty;
                                panel.Invalidate();
                                break;
                            }
                        }
                    }
                };

                panel.MouseClick += (s, e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        isZoomedIn = false;
                        zoomedImagePath = null;
                        zoom = 1.0f;
                        panOffset = Point.Empty;
                        panel.Invalidate();
                    }
                };

                // 檢查是否已存在 TabPage
                TabPage imageTab = _TabControl.TabPages
                    .Cast<TabPage>()
                    .FirstOrDefault(tab => tab.Name == tabName);

                if (imageTab == null)
                {
                    imageTab = new TabPage(tabName)
                    {
                        Name = tabName
                    };
                    _TabControl.TabPages.Add(imageTab);
                }

                imageTab.Controls.Clear();
                imageTab.Controls.Add(panel);
                _TabControl.SelectedTab = imageTab;
            }));
        }
        private Dictionary<string, Panel> _imagePanels = new Dictionary<string, Panel>();
        private Dictionary<string, Bitmap> _currentBitmaps = new Dictionary<string, Bitmap>();


        public void ShowSingleImageInTab(string tabName, Bitmap image)
        {
            if (image == null)
            {
                //MessageBox.Show("圖片為空", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.Invoke((Action)(() =>
            {
                Panel panel;
                float zoom = 1.0f;
                Point panOffset = Point.Empty;
                Point lastMousePos = Point.Empty;
                bool isDragging = false;

                if (!_imagePanels.ContainsKey(tabName))
                {
                    panel = new DoubleBufferedPanel
                    {
                        Dock = DockStyle.Fill,
                        BackColor = Color.Black
                    };

                    panel.Paint += (s, e) =>
                    {
                        if (!_currentBitmaps.ContainsKey(tabName)) return;

                        var bmp = _currentBitmaps[tabName];
                        var g = e.Graphics;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.TranslateTransform(panOffset.X, panOffset.Y);
                        g.ScaleTransform(zoom, zoom);

                        float scale = Math.Min((float)panel.Width / bmp.Width, (float)panel.Height / bmp.Height);
                        int drawWidth = (int)(bmp.Width * scale);
                        int drawHeight = (int)(bmp.Height * scale);
                        int x = (panel.Width - drawWidth) / 2;
                        int y = (panel.Height - drawHeight) / 2;

                        g.DrawImage(bmp, new Rectangle(x, y, drawWidth, drawHeight));
                    };

                    panel.MouseWheel += (s, e) =>
                    {
                        Point mousePos = e.Location;
                        PointF logicalPos = new PointF(
                            (mousePos.X - panOffset.X) / zoom,
                            (mousePos.Y - panOffset.Y) / zoom
                        );

                        zoom += e.Delta > 0 ? 0.1f : -0.1f;
                        zoom = Math.Max(0.2f, Math.Min(zoom, 5.0f));

                        panOffset.X = (int)(mousePos.X - logicalPos.X * zoom);
                        panOffset.Y = (int)(mousePos.Y - logicalPos.Y * zoom);

                        panel.Invalidate();
                    };

                    panel.MouseDown += (s, e) =>
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            isDragging = true;
                            lastMousePos = e.Location;
                            panel.Cursor = Cursors.Hand;
                        }
                    };

                    panel.MouseMove += (s, e) =>
                    {
                        if (isDragging)
                        {
                            var delta = new Point(e.X - lastMousePos.X, e.Y - lastMousePos.Y);
                            panOffset.X += delta.X;
                            panOffset.Y += delta.Y;
                            lastMousePos = e.Location;
                            panel.Invalidate();
                        }
                    };

                    panel.MouseUp += (s, e) =>
                    {
                        isDragging = false;
                        panel.Cursor = Cursors.Default;
                    };

                    panel.MouseClick += (s, e) =>
                    {
                        if (e.Button == MouseButtons.Right)
                        {
                            zoom = 1.0f;
                            panOffset = Point.Empty;
                            panel.Invalidate();
                        }
                    };

                    var tab = new TabPage(tabName) { Name = tabName };
                    tab.Controls.Add(panel);
                    _TabControl.TabPages.Add(tab);

                    _imagePanels[tabName] = panel;
                }
                else
                {
                    panel = _imagePanels[tabName];
                }

                // 更新圖片

                if (_currentBitmaps.ContainsKey(tabName))
                {
                    _currentBitmaps[tabName]?.Dispose();
                }
                _currentBitmaps[tabName] = (Bitmap)image.Clone();

                panel.Invalidate();

                _TabControl.SelectedTab = _TabControl.TabPages[tabName];
            }));
        }


        private bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    //stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }
        private static readonly object _Reportlock = new object();
        private static readonly object _lock = new object();
        public int WriteCSVResult(string csvFilePath, DUT_BASE tmpDut, bool showolg)
        {
            try
            {
                //if (IsFileLocked(csvFilePath))
                //{
                //    MessageBox.Show($"文件 {Path.GetFileName(csvFilePath)} 正在被其他程式使用，請關閉後重試。 (The file {Path.GetFileName(csvFilePath)} is being used by another program. Please close it and try again.)", "文件占用 (File Occupied)", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    //return;
                //}
                lock (_lock)
                {
                    string filePath = "Config\\Header.json";
                    string HeaderFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                    string directoryPath = Path.GetDirectoryName(csvFilePath);
                    string csvlog = string.Empty;
                    // 檢查目錄是否存在，如果不存在則創建
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    string temp = string.Empty;
                    string jsonHeaderData = File.ReadAllText(HeaderFilePath, encoding: Encoding.UTF8);
                    var headerData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonHeaderData);
                    List<string> headers = headerData["headers"];

                    if (headers.Count == 0)
                    {
                        tmpDut.LogMessage("No data available to record in the CSV.");
                        return 0;
                    }
                    bool fileExists = File.Exists(csvFilePath);

                    string directoryName = Path.GetDirectoryName(csvFilePath);
                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    bool writeHeaders = true;

                    if (fileExists)
                    {
                        var fileContent = File.ReadAllText(csvFilePath, encoding: Encoding.UTF8);
                        if (headers[0] != null)
                            writeHeaders = !fileContent.Contains(headers[0]);
                        else
                            MessageBox.Show("The first of Header is null rewrite Header to csv File", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    using (var writer = new StreamWriter(csvFilePath, append: true, encoding: Encoding.UTF8))
                    using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
                    {
                        if (writeHeaders)
                        {
                            foreach (var header in headers)
                            {
                                csv.WriteField(header);
                            }
                            csv.NextRecord();
                        }
                        int count = 1;
                        // 將解析後的數據逐個添加到 CSV 檔案的一列中
                        foreach (var header in headers)
                        {
                            string fielddata = "";
                            //foreach (var data in dataList)
                            {
                                // 解析 JSON 字符串
                                //Dictionary<string, object> jsonData;
                                try
                                {
                                    temp = tmpDut.DataCollection.GetMoreProp(header);

                                    if (temp != "")
                                    {
                                        fielddata = temp;

                                        csvlog += $"{count++}.[{header}] = {temp}\n";
                                    }
                                    else
                                    {
                                        csvlog += $"{count++}.[{header}] = NULL\n";
                                    }



                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }
                            }
                            csv.WriteField(fielddata);
                        }

                        csv.NextRecord();
                    }


                    if (showolg)
                        Bd.Logger.Debug($"\n=============[ExportCSV]=============\n{csvlog}");

                    //Bd.Logger.Debug($"Save test CSV OK.{csvFilePath}");
                    if (!string.IsNullOrEmpty(GlobalNew.ServerLOGFOLDER))
                    {
                        string destinationPath = csvFilePath.Replace(GlobalNew.LOGFOLDER, GlobalNew.ServerLOGFOLDER);
                        CopyFile(csvFilePath, destinationPath);
                    }

                    return headers.Count;
                }
            }
            catch (IOException e1)
            {
                // 處理例外狀況             
                Bd.Logger.Debug($"Save test CSV Fail.{csvFilePath} Exception:{e1.Message}");
                MessageBox.Show($"CSV Warn: {e1.Message},Please Close {Path.GetFileName(csvFilePath)}", "CSV File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 0;
            }
            catch (Exception ex)
            {
                Bd.Logger.Debug($"Save test CSV Fail.{csvFilePath} Exception:{ex.Message}");
                return 0;
            }


        }
        private delegate void ClearRichTextDelegate(RichTextBox textbox);

        public void ClearRichText(RichTextBox textbox)
        {
            try
            {
                if (textbox.InvokeRequired)
                {
                    ClearRichTextDelegate d = new ClearRichTextDelegate(ClearRichText);
                    this.Invoke(d, new object[] { textbox });
                }
                else
                {
                    if (!textbox.IsDisposed)
                    {
                        textbox.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清除文字時發生錯誤：{ex.Message}");
            }
        }

        private delegate void SaveRichTextDelegatePro(DUT_BASE tmpDut, RichTextBox textbox, bool isClear, string path, RichTextBoxStreamType a);//定义更新log委托
        public void SaveRichTextPro(DUT_BASE tmpDut, RichTextBox textbox, bool isClear = false, string path = "", RichTextBoxStreamType a = RichTextBoxStreamType.PlainText)
        {
            try
            {
                if (textbox.InvokeRequired)
                {
                    SaveRichTextDelegatePro d = new SaveRichTextDelegatePro(SaveRichTextPro);
                    this.Invoke(d, new object[] { tmpDut, textbox, isClear, path, a });
                }
                else
                {
                    if (!textbox.IsDisposed)
                    {
                        if (isClear)
                        {
                            textbox.Clear();
                            return;
                        }

                        string directoryPath = string.Empty;

                        if (!string.IsNullOrEmpty(ConfigManager.LogPath))
                        {
                            string File_Path = ReplaceProp(tmpDut, ConfigManager.LogPath);
                            string ServerFile_Path = ReplaceProp(tmpDut, ConfigManager.ServerLogPath);
                            string directory = Path.GetDirectoryName(File_Path);
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            textbox.SaveFile($"{File_Path}", a);
                            //MessageBox.Show(textbox.Text);
                            if (!string.IsNullOrEmpty(ServerFile_Path))
                            {
                                string Serverdirectory = Path.GetDirectoryName(ServerFile_Path);
                                if (!Directory.Exists(Serverdirectory))
                                {
                                    Directory.CreateDirectory(Serverdirectory);
                                }

                                File.Copy(File_Path, ServerFile_Path, true);


                                //CopyFile(File_Path, ServerFile_Path);
                            }
                            Bd.Logger.Debug($"Save test log OK.{File_Path}");

                            string Msg = CopyLogToServer("Z", path);

                            Bd.Logger.Debug($"Backup log to {ServerFile_Path}");
                        }
                        else
                        {
                            //Path.GetDirectoryName($"{GlobalNew.LogPath}\\{path}");
                            directoryPath = Path.GetDirectoryName($"{GlobalNew.LogPath}\\{path}");
                            // 檢查目錄是否存在，如果不存在則創建
                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }
                            textbox.SaveFile($"{GlobalNew.LogPath}\\{path}", a);
                            MessageBox.Show(textbox.Text);
                            if (!string.IsNullOrEmpty(GlobalNew.ServerLOGFOLDER))
                            {
                                string org = $"{GlobalNew.LogPath}\\{path}";
                                string des = org.Replace(GlobalNew.LOGFOLDER, GlobalNew.ServerLOGFOLDER);
                                CopyFile(org, des);
                            }
                            Bd.Logger.Debug($"Save test log OK.{GlobalNew.LogPath}\\{path}");

                            string Msg = CopyLogToServer("Z", path);

                            Bd.Logger.Debug($"Backup log to {path}");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                tmpDut.LogMessage(ex.ToString());
                // throw;
            }
        }
        private void SaveRichTextBoxLog(string filePath)
        {
            try
            {
                if (richTextBox_dutLog.InvokeRequired)
                {
                    richTextBox_dutLog.Invoke(new Action(() => SaveRichTextBoxLog(filePath)));
                }
                else
                {
                    richTextBox_dutLog.SaveFile(filePath, RichTextBoxStreamType.PlainText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Please check if the path({filePath}) exists.{ex.ToString()}");
            }

        }

        public void WriteResult(string path)
        {
            try
            {

                // 1) 指定白名單（支援繼承：TerminalNode 會套用到所有子類）
                var propsMap = new Dictionary<string, string[]>();

                propsMap["ContainerNode"] = new[]
                {
                    "Enable",
                    "ClassName",
                    "Description",
                    "RetryCount",
                    "StartTime",
                    "EndTime",
                    "Result",
                    "isTestItem",
                    "DurationSec"  // 自動四捨五入到小數第 3 位
                };

                propsMap["TerminalNode"] = new[]
                {
                    "Enable",
                    "ClassName",
                    "Description",
                    "Spec",
                    "RetryCount",
                    "RowDataItem.OutputData=>OutputData",
                    "StartTime",
                    "EndTime",
                    "Result",
                    "DurationSec"  // 自動四捨五入到小數第 3 位
                };

                // 2) 取得 TreeView
                TreeView tv = MainProTreeView.GetTreeview();
                var root = tv.Nodes[0].Nodes[0]; // 你想指定的根
                var json = SlimTreeExporter.ExportStrictJsonKeepAll(root, propsMap);
                File.WriteAllText(path, json, Encoding.UTF8);

                string htmlpath = path.Replace(".json",".html");
                SlimTreeHtmlExporter.ExportStrictHtml(
                    root,
                    propsMap,
                     htmlpath,
                    htmlpath,
                    expandAll: false);

            }
            catch (Exception ex)
            {
                return ;
            }


        }
        private string CopyLogToServer(string mapDrive, string logfile)
        {
            try
            {
                string FileName = Path.GetFileName($@"{GlobalNew.txtLogPath}\\{logfile}");
                string destPath = $@"{GlobalNew.LOGSERVER}\{GlobalNew.CurrentProject}\{GlobalNew.CurrentStation}\{GlobalNew.CurrentFixture}\{DateTime.Now.ToString("yyyyMMdd")}\{logfile}";
                string directoryPath = Path.GetDirectoryName(destPath);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.Copy($@"{GlobalNew.LogPath}\{logfile}", destPath, true);

                return "Upload test log to logServer success.";
                //  }
            }
            catch (Exception ex)
            {
                return "Upload test log to logServer Exception:" + ex.Message;
            }

        }

        private void TestPassed(DUT_BASE CurrentTempDUT)
        {
            failCount = 0;
            GlobalNew.GlobalFailCount = 0;
            SetTrafficLight("Green");
        }
        //***************鎖機***************
        private void TestFailed(DUT_BASE CurrentTempDUT)
        {

            string abortCountStr = CurrentTempDUT.DataCollection.GetMoreProp("AbortCount");
            string warningCountStr = CurrentTempDUT.DataCollection.GetMoreProp("WarningCount");

            int abortCount = 0;
            int warningCount = 0;

            if (!string.IsNullOrEmpty(abortCountStr) && int.TryParse(abortCountStr, out int tempAbortCount))
            {
                abortCount = tempAbortCount;
            }

            if (!string.IsNullOrEmpty(warningCountStr) && int.TryParse(warningCountStr, out int tempWarningCount))
            {
                warningCount = tempWarningCount;
            }

            failCount++;
            GlobalNew.GlobalFailCount++;

            if (failCount == ConfigManager.AbortLimit - 1 && ConfigManager.AbortLimit > 0)
            {
                SetTrafficLight("Yellow");
            }
            else if (failCount >= ConfigManager.AbortLimit && ConfigManager.AbortLimit > 0)
            {
                SetTrafficLight("Red");
                SetTestStatus(CurrentTempDUT, TestStatus.ABORT);

            }

            if (GlobalNew.GlobalFailCount == GlobalNew.CONTINUE_FAIL_LIMIT - 1 && GlobalNew.CONTINUE_FAIL_LIMIT > 0)
            {
                SetTrafficLight("Yellow");
            }
            else if (GlobalNew.GlobalFailCount >= GlobalNew.CONTINUE_FAIL_LIMIT && GlobalNew.CONTINUE_FAIL_LIMIT > 0)
            {
                SetTrafficLight("Red");
                SetTestStatus(CurrentTempDUT, TestStatus.ABORT);

            }
        }
        //***************鎖機***************
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isBlinking) return; // 如果未啟動，直接返回

            //label_description.Text = "鎖機中，請通知PE維修";
            //// 切換背景色
            //label_description.BackColor = isBlinkState ? Color.Red : Color.DarkBlue;
            UpdateLabelText(label_description, "鎖機中，請通知PE維修", isBlinkState ? Color.Red : Color.DarkBlue);
            isBlinkState = !isBlinkState; // 切換狀態
        }
        private void ClearStatus()
        {
            failCount = 0;
            GlobalNew.GlobalFailCount = 0;
            SetTrafficLight("");
        }

        private void label_result_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.Clicks == 2)
            {
                if (CurrentThread == null)
                    return;

                if (CurrentThread.isRunning == 1)
                {
                    UpdateLabelText(label_result, "Pause", Color.Red, Color.White, 20f);
                    CurrentThread.T_PausePro();
                }
                else if (CurrentThread.isRunning == 2)
                {
                    UpdateLabelText(label_result, "Testing", Color.Yellow, Color.Black, 20f);
                    CurrentThread.T_ContinuePro();
                }
            }
        }

        private DateTime lastRightClickTime = DateTime.MinValue;
        private const int DoubleClickThreshold = 300; // 毫秒

        private void richTextBox_dutLog_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right)
            {

                var now = DateTime.Now;
                if ((now - lastRightClickTime).TotalMilliseconds <= DoubleClickThreshold)
                {
                    if (MainLogLayout.RowStyles[1].Height == 50)
                    {
                        MainLogLayout.RowStyles[0].SizeType = SizeType.Percent;
                        MainLogLayout.RowStyles[0].Height = 80;
                        MainLogLayout.RowStyles[1].SizeType = SizeType.Percent;
                        MainLogLayout.RowStyles[1].Height = 20;
                    }
                    else
                    {

                        MainLogLayout.RowStyles[0].SizeType = SizeType.Percent;
                        MainLogLayout.RowStyles[0].Height = 50;
                        MainLogLayout.RowStyles[1].SizeType = SizeType.Percent;
                        MainLogLayout.RowStyles[1].Height = 50;
                    }
                }
                lastRightClickTime = now;


            }
        }
    }

    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }
}
