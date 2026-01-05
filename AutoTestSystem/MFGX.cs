using AutoTestSystem.Base;
using AutoTestSystem.BLL;
using AutoTestSystem.DUT;
using AutoTestSystem.Model;
using AutoTestSystem.Script;
using Manufacture;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing.Common;
using ZXing.Rendering;
using ZXing;
using static AutoTestSystem.BLL.Bd;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using PdfSharp.Drawing.BarCodes;
using System.Data.SQLite;
using AutoTestSystem.Equipment.Teach;

namespace AutoTestSystem
{
    public partial class MFGX : Form
    {
        private INIHelper iniConfig;
        public static MFGX Instance; // 全域實例
        private System.Windows.Forms.Timer uiTimer;
        private bool emergencyAlertShown = false;
        public Image StartButtonImage
        {
            get { return StartBtn.Image; }
            set { StartBtn.Image = value; }
        }
        public MFGX()
        {
            InitializeComponent();
            uiTimer = new System.Windows.Forms.Timer();
            uiTimer.Interval = 1000; // 1秒執行一次，可依需求調整
            uiTimer.Tick += UiTimer_Tick;
            uiTimer.Start();
            Instance = this; // 啟動時把自己記錄起來

            GlobalNew.InitStation();

            GlobalNew.DeviceListPath = $@"{GlobalNew.CurrentRecipePath}";

            iniConfig = new INIHelper(GlobalNew.IniConfigFile);

            // 確保目標目錄存在


            var directory = Path.GetDirectoryName(GlobalNew.LOGFOLDER);
            if (!string.IsNullOrEmpty(directory))
            {
                string root = Path.GetPathRoot(directory);
                if (!Directory.Exists(root))
                {
                    MessageBox.Show($"錯誤：磁碟槽 '{root}' 不存在，無法建立資料夾。", "磁碟槽錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            // 獲取當前執行檔的版本資訊
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            // 設定表單的標題
            this.Text = $"MFGX - Version {version} - {Path.GetFileName(GlobalNew.CurrentRecipePath)}";

            Manufacture.Global_Memory.GlobalCreate("AutoTestSystem.Script", "AutoTestSystem.Base.ScriptBase");

            GlobalNew.MainFormInstance = this;
            UpdatePassLabel(GlobalNew.Total_Pass_Num.ToString());
            UpdateFailLabel(GlobalNew.Total_Fail_Num.ToString());
        }

        private async void StartBtn_Click(object sender, EventArgs e)
        {
            await StartProcessAsync();
        }

        
        private async Task StartProcessAsync()
        {
            this.ActiveControl = null;

            if (!GlobalNew.g_HomeProcessSuccess || !GlobalNew.g_Initial)
            {

                MessageBox.Show(
                    "運行前請先初始化及運行復歸流程\nSystem is not initialized and has not returned to the Home process.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            if (!GlobalNew.g_isRunning)
            {

                if (GlobalNew.CurrentMode != "PROD")
                {
                    MessageBox.Show("注意工程或點檢模式勿用於生產中!!\n Engineering mode should not be used for production.", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    if (GlobalNew.CurrentUser == "rd")
                    {
                        MessageBox.Show("勿使用RD帳號進行生產!!\n Do not use the RD account for production..", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (GlobalNew.CurrentUser == "pe")
                    {
                        MessageBox.Show("勿使用PE帳號進行生產,僅供維修點檢測試用!!\n Do not use the PE account for production..", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                if (GlobalNew.RunMode == 1)
                {
                    GlobalNew.g_shouldStop = false;
                    GlobalNew.g_isRunning = true;
                    StartBtn.Image = Properties.Resources.icons8_stop_30; // 換成停止圖示
                                                                          // 建一個 Task 清單
                                                                          // 直接用 LINQ 撈出所有要跑的 Task，並一次等待它們

                    await Task.WhenAll(
                            GlobalNew.Devices.Values
                                .OfType<DUT_BASE>()
                                .Where(d => d.Enable)
                                .Select(d => d.RunLoopAsync())
                        );

                    GlobalNew.g_isRunning = false;
                    StartBtn.Image = Properties.Resources.icons8_play_30; // 緒結束後換回開始圖示
                }
                if (GlobalNew.RunMode == 2)
                {
                    var rotary = GlobalNew.Devices.Values.OfType<RotaryTestController>().FirstOrDefault();
                    if (rotary == null)
                    {
                        MessageBox.Show("Not Found RotaryTestController Device");
                        return;
                    }

                    rotary.Start();

                    GlobalNew.g_shouldStop = false;
                    GlobalNew.g_isRunning = true;
                    StartBtn.Image = Properties.Resources.icons8_stop_30; // 換成停止圖示
                                                                          // 建一個 Task 清單
                                                                          // 直接用 LINQ 撈出所有要跑的 Task，並一次等待它們
                }

            }
            else
            {
                if (GlobalNew.RunMode == 1)
                {
                    GlobalNew.g_shouldStop = true;
                    StartBtn.Enabled = false; // 防止重複點擊
                    StartBtn.Image = Properties.Resources.icons8_play_30;
                    while (GlobalNew.g_isRunning)
                    {
                        await Task.Delay(100); // 等待緒結束
                    }
                    StartBtn.Enabled = true;

                    //都結束且停後，重置旗標在這邊下是因為如果進處方運行單步時這個旗標會影響單步運行
                    GlobalNew.g_shouldStop = false;
                }
                else if (GlobalNew.RunMode == 2)
                {
                    GlobalNew.g_HomeProcessSuccess = false;// 強制圓盤模式下按停止下次啟動需HOME
                    GlobalNew.g_shouldStop = true;
                    StartBtn.Enabled = false; // 防止重複點擊
                    StartBtn.Image = Properties.Resources.icons8_play_30;
                    var rotary = GlobalNew.Devices.Values.OfType<RotaryTestController>().FirstOrDefault();
                    rotary.Stop();
                    GlobalNew.g_isRunning = false;
                    //while (GlobalNew.g_isRunning)
                    //{
                    //    await Task.Delay(100); // 等待緒結束
                    //}
                    StartBtn.Enabled = true;

                    //都結束且停後，重置旗標在這邊下是因為如果進處方運行單步時這個旗標會影響單步運行
                    //GlobalNew.g_shouldStop = false;
                }
            }
        }

        // UiTimer_Tick：保持你的原本流程與語意
        private async void UiTimer_Tick(object sender, EventArgs e)
        {
            if (GlobalNew.RunMode == 2)
            {
                if (!GlobalNew.g_shouldStop)
                {
                    // 狀態恢復時可再次顯示警報（沿用你的旗標）
                    emergencyAlertShown = false;
                    return;
                }

                // 僅在第一次偵測到停止條件時嘗試顯示（沿用你的語意）
                if (emergencyAlertShown) return;

                // 防重入（比 bool 更安全，避免 Timer 抖動或 Tick 重入）
                if (System.Threading.Interlocked.Exchange(ref _showingAbortDialog, 1) == 1) return;

                try
                {
                    emergencyAlertShown = true;

                    // 有些情況（窗體尚未建立/已被釋放）直接略過
                    if (this.IsDisposed || !this.IsHandleCreated)
                    {
                        return;
                    }

                    // 任何情況，都保證在 UI 執行緒上顯示對話框
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)ShowEmergencyDialogBlocking);
                    }
                    else
                    {
                        ShowEmergencyDialogBlocking();
                    }

                    // 使用者按下「確定」後才繼續
                    await StartProcessAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Error showing alert dialog: " + ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    // 放開防重入鎖
                    System.Threading.Interlocked.Exchange(ref _showingAbortDialog, 0);
                }
            }

        }
        // ===== 成員欄位（防重入：0=未顯示／1=顯示中）=====
        private int _showingAbortDialog = 0;

        // ===== 讓視窗到前景（可失敗，不影響邏輯）=====
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        // 真正顯示對話框（同步阻塞，必定在 UI 執行緒呼叫）
        private void ShowEmergencyDialogBlocking()
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            using (Form alertForm = new Form())
            {
                // ===== 外觀設定 =====
                alertForm.Text = "⚠ 終止警報";
                alertForm.Size = new Size(420, 220);
                alertForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                alertForm.MaximizeBox = false;
                alertForm.MinimizeBox = false;
                alertForm.BackColor = Color.DarkRed;
                alertForm.ForeColor = Color.White;
                alertForm.Font = new Font("Microsoft JhengHei", 12, FontStyle.Bold);
                alertForm.ShowInTaskbar = false;   // 預設關閉，最小化情境時會打開
                alertForm.TopMost = true;          // 避免被其他程式遮住

                // ===== 內容（訊息 + 底部按鈕列）=====
                var messageLabel = new Label
                {
                    Text = "終止運行條件觸發！\nAbort the process due to anomaly!",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.White
                };

                var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 64 };
                var okButton = new Button
                {
                    Text = "確定",
                    Font = new Font("Microsoft JhengHei", 12, FontStyle.Bold),
                    BackColor = Color.White,
                    ForeColor = Color.DarkRed,
                    Size = new Size(110, 38),
                    Anchor = AnchorStyles.None
                };
                bottomPanel.Controls.Add(okButton);
                bottomPanel.Resize += (s, e) =>
                {
                    okButton.Left = (bottomPanel.Width - okButton.Width) / 2;
                    okButton.Top = (bottomPanel.Height - okButton.Height) / 2;
                };
                okButton.Click += (s, e) => alertForm.DialogResult = DialogResult.OK;

                alertForm.AcceptButton = okButton;
                alertForm.Controls.Add(messageLabel);
                alertForm.Controls.Add(bottomPanel);

                // ===== 依主窗狀態決定顯示策略 =====
                bool parentMinimized = (this.WindowState == FormWindowState.Minimized) || !this.Visible;

                if (parentMinimized)
                {
                    // 主窗最小化：改為無 owner 的前景對話框，顯示在工作列，置中在螢幕
                    alertForm.ShowInTaskbar = true;
                    alertForm.StartPosition = FormStartPosition.CenterScreen;

                    alertForm.Shown += (s, e) =>
                    {
                        try { SetForegroundWindow(alertForm.Handle); } catch { }
                        alertForm.Activate();
                    };

                    alertForm.ShowDialog(); // 無 owner
                }
                else
                {
                    // 主窗可見：標準模態 + 以父視窗置中
                    alertForm.ShowInTaskbar = false;
                    alertForm.StartPosition = FormStartPosition.CenterParent;

                    alertForm.Shown += (s, e) => alertForm.Activate();

                    alertForm.ShowDialog(this); // 有 owner
                }
            }
        }


        public void UpdatePassLabel(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(UpdatePassLabel), text);
            }
            else
            {
                lb_passNum.Text = text;
                //iniConfig.Writeini("CountNum", "Total_Pass_Num", GlobalNew.Total_Pass_Num.ToString());
            }
        }

        public void UpdateFailLabel(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(UpdateFailLabel), text);
            }
            else
            {
                lb_FailNum.Text = text;
                //iniConfig.Writeini("CountNum", "Total_Fail_Num", GlobalNew.Total_Fail_Num.ToString());
            }
        }
        private void MFGX_Shown(object sender, EventArgs e)
        {
            string ret = ProTreeView.ProTreeView.Load_DeviceList(GlobalNew.CurrentRecipePath, GlobalNew.Devices);     //! ProTreeView load device
            label_station.Text = GlobalNew.CurrentStation;
            label_mode.Text = GlobalNew.CurrentMode;
            label_project.Text = GlobalNew.CurrentProject;
            label_user.Text = GlobalNew.CurrentUser;
            if (ret == "Load_Devices success")
            {
                InitDevicesUI();
            }
        }

        private void InitDevicesUI()
        {
            InitDUTDashboard();
            //InitDUTDataGridView();
            //foreach (var value in GlobalNew.Devices.Values)
            //{
            //    if (value is DUT_BASE)
            //    {

            //    }
            //}
        }
        
        public bool InitDUTDashboard()
        {
            tableLayoutPanel_dashboard.RowCount = 1;
            tableLayoutPanel_dashboard.ColumnCount = 0;
            tableLayoutPanel_dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel_dashboard.AutoSize = false; // 設置 AutoSize 為 false
            tableLayoutPanel_dashboard.Controls.Clear();

            // 計算設備數量
            int deviceCount = GlobalNew.Devices.Values.Count(value => value is DUT_BASE && ((DUT_BASE)value).Enable != false);

            // 設置 ColumnCount
            tableLayoutPanel_dashboard.ColumnCount = deviceCount;

            // 清除現有的 ColumnStyles
            tableLayoutPanel_dashboard.ColumnStyles.Clear();

            // 添加新的 ColumnStyles
            for (int i = 0; i < deviceCount; i++)
            {
                tableLayoutPanel_dashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / deviceCount));
            }

            int columnIndex = 0;
            foreach (var value in GlobalNew.Devices.Values)
            {
                if (value is DUT_BASE)
                {

                    DUT_BASE temp = (DUT_BASE)value;
                    if (temp.Enable == false)
                        continue;
                    temp.UI_Create(deviceCount == 1);
                    temp.DutDashboard.InitialTreeView();
                    temp.DutDashboard.InitialDatagridview();

                    temp.ProgressChanged += OnProgressChanged;
                    // 添加 Dashboard 控制項到 TableLayoutPanel
                    this.tableLayoutPanel_dashboard.Controls.Add(temp.DutDashboard, columnIndex, 0);
                    columnIndex++;

                    if (GlobalNew.DataGridViewsList.ContainsKey(temp.Description))
                        GlobalNew.DataGridViewsList[temp.Description] = temp.DutDashboard.DataGridView;
                    else
                        GlobalNew.DataGridViewsList.Add(temp.Description, temp.DutDashboard.DataGridView);
                }
            }
            if(GlobalNew.RunMode == 0) 
            {
                StartBtn.Enabled = false;
            }
            else if(GlobalNew.RunMode == 1)
            {
                StartBtn.Enabled = true;
            }
            else if (GlobalNew.RunMode == 2)
            {
                StartBtn.Enabled = true;
            }
            foreach (var value in GlobalNew.Devices.Values)
            {
                if (value is RotaryTestController)
                {
                    RotaryTestController temp = (RotaryTestController)value;

                    // 計算啟用的項目數量
                    int activeCount = temp.ActiveList.Count(b => b);

                    // 初始化 TableLayoutPanel
                    tableLayoutPanel_dashboard.Controls.Clear();
                    tableLayoutPanel_dashboard.ColumnCount = activeCount;
                    tableLayoutPanel_dashboard.ColumnStyles.Clear();
                    tableLayoutPanel_dashboard.RowCount = 1;
                    tableLayoutPanel_dashboard.RowStyles.Clear();
                    tableLayoutPanel_dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

                    // 平均分配欄位寬度
                    for (int i = 0; i < activeCount; i++)
                    {
                        tableLayoutPanel_dashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / activeCount));
                    }

                    int columnIndex2 = 0;
                    for (int i = 0; i < temp.ActiveList.Count; i++)
                    {
                        if (temp.ActiveList[i])
                        {
                            char letter = (char)('A' + i);
                            string dashboardName = letter.ToString(); // 例如 "A"

                            var target = GlobalNew.Devices.Values
                                .OfType<DUT_BASE>()
                                .FirstOrDefault(d => d.Enable && d.Description == dashboardName);

                            if (target != null)
                            {
                                // 加入到對應欄位
                                tableLayoutPanel_dashboard.Controls.Add(target.DutDashboard, columnIndex2, 0);
                                columnIndex2++;
                            }
                        }
                    }

                }
            }
            // 設置 AutoSize 為 true 以自動調整大小
            tableLayoutPanel_dashboard.AutoSize = true;

            return true;
        }

        public bool InitDUTDataGridView()
        {
            tableLayoutPanel_datagridview.RowCount = 1;
            tableLayoutPanel_datagridview.ColumnCount = 0;
            tableLayoutPanel_datagridview.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel_datagridview.AutoSize = false; // 設置 AutoSize 為 false
            tableLayoutPanel_datagridview.Controls.Clear();

            // 計算設備數量
            int deviceCount = GlobalNew.Devices.Values.Count(value => value is DUT_BASE && ((DUT_BASE)value).DutDashboard != null);

            // 設置 ColumnCount
            tableLayoutPanel_datagridview.ColumnCount = deviceCount;

            // 清除現有的 ColumnStyles
            tableLayoutPanel_datagridview.ColumnStyles.Clear();

            // 添加新的 ColumnStyles
            for (int i = 0; i < deviceCount; i++)
            {
                tableLayoutPanel_datagridview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / deviceCount));
            }

            int columnIndex = 0;
            foreach (var value in GlobalNew.Devices.Values)
            {
                if (value is DUT_BASE)
                {
                    DUT_BASE temp = (DUT_BASE)value;

                    if (temp.DutDashboard != null)
                    {
                        temp.DutDashboard.InitialTreeView();
                        //temp.DutDashboard.InitialDatagridview();
                        
                        //if (temp.DutDashboard != null && temp.DutDashboard.Parent != null)
                        //{
                        //    temp.DutDashboard.Parent.Controls.Remove(temp.DutDashboard);
                        //}
                        //temp.DutDashboard.DataGridView.Columns.Clear();
                        //temp.DutDashboard.DataGridView.Rows.Clear();

                        ////temp.DutDashboard.MainProTreeView.ProcessNodeMouseClick += MainProTreeNodeMouseClick;


                        ////Logger.Info($"Load recipe path. {GlobalNew.CurrentRecipePath}");


                        //DataGridView dataGridView = temp.DutDashboard.DataGridView;

                        //// 註冊事件
                        //// 設定外觀
                        //dataGridView.Dock = DockStyle.Fill;

                        //// 設定 RowHeaders 的寬度
                        //dataGridView.RowHeadersWidth = 20;
                        //dataGridView.AllowUserToAddRows = false;

                        //dataGridView.EnableHeadersVisualStyles = false;
                        //dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Calibri", 11, FontStyle.Bold);
                        //dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                        //dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.LightBlue;
                        //dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        //dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                        //dataGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                        //dataGridView.DefaultCellStyle.Font = new Font("Helvetica", 9, FontStyle.Regular);

                        //dataGridView.DefaultCellStyle.SelectionBackColor = dataGridView.DefaultCellStyle.BackColor;
                        //dataGridView.DefaultCellStyle.SelectionForeColor = dataGridView.DefaultCellStyle.ForeColor;
                        ////dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.Single;
                        //dataGridView.GridColor = Color.FromArgb(226, 226, 226);

                        //dataGridView.ReadOnly = true;

                        //dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                        //// 設定列
                        //dataGridView.Columns.Add("No", "No");
                        //dataGridView.Columns.Add("ID", "ID");
                        //dataGridView.Columns["ID"].Visible = false;
                        //dataGridView.Columns.Add("Item", "Item");
                        //dataGridView.Columns.Add("Spec", "Spec");
                        //dataGridView.Columns.Add("Value", "Value");

                        //dataGridView.Columns.Add("Result", "Result");
                        //dataGridView.Columns["Result"].HeaderCell.Style.ForeColor = Color.Blue;



                        //dataGridView.Columns.Add("TestTime", "TestTime(s)");
                        //if (GlobalNew.ShowTestTime == "1")
                        //    dataGridView.Columns["TestTime"].Visible = true;
                        //else
                        //    dataGridView.Columns["TestTime"].Visible = false;
                        //dataGridView.Columns.Add("Eslapse", "Eslapse(s)");
                        //dataGridView.Columns.Add("Retry", "Retry");
                        //dataGridView.Columns["Retry"].Visible = false;
                        //dataGridView.Columns["Eslapse"].Visible = false;
                        //dataGridView.Columns["No"].FillWeight = 5;
                        //dataGridView.Columns["Item"].FillWeight = 18;
                        //dataGridView.Columns["Spec"].FillWeight = 28;
                        //dataGridView.Columns["Spec"].Visible = false;
                        //dataGridView.Columns["Value"].FillWeight = 32;
                        //dataGridView.Columns["Result"].FillWeight = 10;
                        //dataGridView.Columns["TestTime"].FillWeight = 12;
                        //dataGridView.Columns["Eslapse"].Width = 12;
                        //dataGridView.Columns["Retry"].Width = 10;
                        //// 設定文字對齊和欄高
                        //dataGridView.Columns["Value"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                        //dataGridView.Columns["Spec"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                        //dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                        //dataGridView.RowTemplate.Height = 120;  // 設定每一行的高度為 120

                        // 將 DataGridView 加入至 GlobalNew.DataGridViewsList
                        if (GlobalNew.DataGridViewsList.ContainsKey(temp.Description))
                            GlobalNew.DataGridViewsList[temp.Description] = temp.DutDashboard.DataGridView;
                        else
                            GlobalNew.DataGridViewsList.Add(temp.Description, temp.DutDashboard.DataGridView);

                        //Number = 0;
                        //TraverseTreeViewNodes(temp.DutDashboard.MainProTreeView.GetTreeview().Nodes, temp.DutDashboard.DataGridView);


                        //// 設置 Dashboard 控制項的屬性
                        //temp.DutDashboard.Dock = DockStyle.Fill;
                        //temp.DutDashboard.Margin = new Padding(0);

                        //// 添加 Dashboard 控制項到 TableLayoutPanel
                        //this.tableLayoutPanel_datagridview.Controls.Add(temp.DutDashboard, columnIndex, 0);
                        columnIndex++;
                    }
                }
            }

            // 設置 AutoSize 為 true 以自動調整大小
            tableLayoutPanel_datagridview.AutoSize = true;

            return true;
        }

        //private void MainProTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        Manufacture.CoreBase obj = (Manufacture.CoreBase)e.Node.Tag;
        //        int len = MainlogRichTextBox.Text.IndexOf(obj.Description.Substring(2).Trim());
        //        if (len > 0)
        //        {
        //            //! 光标跳到指定行
        //            MainlogRichTextBox.Select(len, 0);
        //            MainlogRichTextBox.ScrollToCaret();
        //        }
        //    }
        //}



        private void ConfigureBtn_Click(object sender, EventArgs e)
        {
            if (GlobalNew.UserLevel == 0)
            {
                MessageBox.Show("Access Denied");
                return;
            }

            if(GlobalNew.g_isRunning)
            {
                MessageBox.Show("The operation cannot be performed while the system is running.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var value in GlobalNew.Devices.Values)
            {
                if (value is DUT_BASE)
                {

                    DUT_BASE temp = (DUT_BASE)value;
                    if (temp.Enable == false)
                        continue;
                    temp.DutDashboard?.LockSN_Textboxs();
                       
                } 
            }

            RecipeMenu menu = new RecipeMenu();
            DialogResult result = menu.ShowDialog();

            iniConfig.Writeini("Recipe", "CurrentRecipePath", GlobalNew.CurrentRecipePath);
            iniConfig.Writeini("Recipe", "CurrentProject", GlobalNew.CurrentProject);
            iniConfig.Writeini("Recipe", "CurrentMode", GlobalNew.CurrentMode);
            iniConfig.Writeini("Recipe", "CurrentStation", GlobalNew.CurrentStation);
            iniConfig.Writeini("Recipe", "CurrentFixture", GlobalNew.CurrentFixture);
            iniConfig.Writeini("Recipe", "CurrentVersion", GlobalNew.CurrentConfigVersion);
            label_station.Text = GlobalNew.CurrentStation;
            label_mode.Text = GlobalNew.CurrentMode;
            label_project.Text = GlobalNew.CurrentProject;

            // 獲取當前執行檔的版本資訊
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            // 設定表單的標題
            this.Text = $"MFGX - Version {version} - {Path.GetFileName(GlobalNew.CurrentRecipePath)}";
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            // another UI
            LoginForm CG_Form = null;
            //LoginForm CG_Form = new LoginForm(false);
            if (GlobalNew.Weblogin != "1")
            {
                CG_Form = new LoginForm(false);
            }
            else
            {
                CG_Form = new LoginForm();
            }
            DialogResult res = CG_Form.ShowDialog();
            if (res == DialogResult.OK)
            {
                label_user.Text = GlobalNew.CurrentUser;
                MessageBox.Show("Login successful");
            }

        }
        public void UpdateContLable()
        {
            lb_passNum.InvokeOnToolStripItem(lb_passNum => lb_passNum.Text = GlobalNew.Total_Pass_Num.ToString());
            lb_FailNum.InvokeOnToolStripItem(lb_FailNum => lb_FailNum.Text = GlobalNew.Total_Fail_Num.ToString());
            //lb_YieldNum.InvokeOnToolStripItem(lb_YieldNum => lb_YieldNum.Text = $@"{(GlobalNew.Total_Pass_Num / (double)(GlobalNew.Total_Pass_Num + GlobalNew.Total_Fail_Num)),0:P2}");
        }
        private void HomeBtn_ClickAsync(object sender, EventArgs e)
        {
            \//***************鎖機***************
            //string abort_flag = INIHelper.Readini("CountNum", "ABORT_FLAG", Global.IniConfigFile);
            //if (abort_flag == "1")
            //{
            //    if (GlobalNew.CurrentUser != "pe")
            //    {
            //        MessageBox.Show("鎖機中，請切換PE帳號進行維修");
            //        return;
            //    }
            //}
            //**********************************
            if ((GlobalNew.RunMode == 2 || GlobalNew.RunMode == 1)  && GlobalNew.g_isRunning)
            {
                //GlobalNew.ShowMessage("請中止運行，再進行初始化及復歸", "錯誤", MessageBoxIcon.Error);
                MessageBox.Show(
                            "請中止運行，再進行初始化及復歸。\r\nPlease stop the operation, then perform initialization and reset.\r\n",
                            "錯誤 / Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                return;
            }
            GlobalNew.g_recipesteprun = false;
            GlobalNew.g_shouldStop = false;
            bool isHomeStart = false;
            bool isHomeInitSuccess = false;
            GlobalNew.g_HomeProcessSuccess = false;
            if (!RecipeManagement.StaticUnInitDevices(GlobalNew.Devices))
            {
                GlobalNew.g_Initial = false;
            }

            GlobalNew.Devices.Clear();

            if (!RecipeManagement.InitDevices(GlobalNew.CurrentRecipePath, GlobalNew.Devices))
            {
                GlobalNew.g_Initial = false;
                string message = "Some devices have not been initialized successfully.\n" + "有裝置尚未初始化成功，請檢查並重試。";
                MessageBox.Show(message);
                return;
            }
            else
                GlobalNew.g_Initial = true;

            InitDevicesUI();

            //***************鎖機***************
            //foreach (var value in GlobalNew.Devices.Values)
            //{
            //    if (value is DUT_BASE)
            //    {
            //        DUT_BASE temp = (DUT_BASE)value;
            //        if (abort_flag == "1")
            //        {
            //            if (temp.Enable)
            //                temp.DutDashboard.BlinkingTitle = true;
            //        }
            //    }
            //}

            foreach (var value in GlobalNew.Devices.Values)
            {
                if (value is DUT_BASE)
                {
                    DUT_BASE temp = (DUT_BASE)value;
                    if (temp.Enable == false)
                        continue;

                    foreach (TreeNode m in temp.DutDashboard.MainProTreeView.GetTreeview().Nodes)
                    {
                        foreach (TreeNode n in m.Nodes)
                        {
                            if (n.Tag is Container_JIG_INIT == true)
                            {
                                //JIG_InitailNode = n;
                                isHomeStart = true;
                                BlockingForm blockingForm = new BlockingForm();

                                //DUT_Simu SimuDUT = new DUT_Simu();
                                temp.isSimu = true;
                                bool TestResult = false;
                                int ResetRes = 0;
                                object[] context = new object[] { temp.DutDashboard.MainProTreeView, false, temp, TestResult, GlobalNew.Devices, n };
                                temp.SetConfig_Param();

                                //===
                                temp.LOGGER.StetupWLog(temp.Description, GlobalNew.LOGFOLDER, temp.DutDashboard.DutLogRichTextBox);
                                CoreBase.SetMLoggerThread(temp.LOGGER);
                                Bd.SetLoggerForCurrentThread(temp.LOGGER);
                                //===
                                //Container_MainThread T = new Container_MainThread();
                                //Task ta = T.Act(n, context);
                                var task1 = Task.Factory.StartNew(() =>
                                {
                                    ResetRes = ((Container_JIG_INIT)n.Tag).Process(n, context);
                                });
                                task1.ContinueWith(t =>
                                {
                                    blockingForm.Close();
                                    //bool result = (bool)context[3];
                                    if (ResetRes == 1)
                                    {
                                        isHomeInitSuccess = true;
                                    }
                                    Container_Sequences.ClearSyncProp();
                                    Script_Extra_GlobalPropManager.ClearSyncProp();
                                    //Script_Extra_CountDownEvent.ClearAll();
                                    temp.DutDashboard.MemoryDataClear(temp);
                                    temp.isSimu = false;
                                }, TaskScheduler.FromCurrentSynchronizationContext()); // 這樣會確保 ContinueWith 在 UI 主線程上執行}

                                blockingForm.ShowDialog();

                                //只能有一個HOME容器運行
                                break;
                            }
                        }
                    }
                    break;
                }
            }

            GlobalNew.g_HomeProcessSuccess = isHomeInitSuccess;
            GlobalNew.EnableDeviceCount = 0;
            foreach (var value in GlobalNew.Devices.Values)
            {
                if (value is DUT_BASE)
                {

                    DUT_BASE temp = (DUT_BASE)value;
                    if (temp.Enable == false)
                        continue;

                    GlobalNew.EnableDeviceCount++;
                    temp.isSimu = false;
                    //當裝置初始化完成且沒有拉HOME流程式可開啟輸號輸入
                    if (isHomeStart == false)
                    {
                        if(GlobalNew.EnableDeviceCount == 1)
                            temp.DutDashboard.ResetUISNtext(true);
                        else
                            temp.DutDashboard.ResetUISNtext();
                    }
                    else
                    {
                        if (isHomeInitSuccess)
                        {
                            if (GlobalNew.EnableDeviceCount == 1)
                                temp.DutDashboard.ResetUISNtext(true);
                            else
                                temp.DutDashboard.ResetUISNtext();
                        }
                        else
                        {
                            string message = "Initialization action failed. Please check and retry..\n" + "有初始化動作未成功執行，請檢查並重試。";
                            MessageBox.Show(message);
                            return;
                        }
                    }
                }          
            }

            if(GlobalNew.RunMode == 1 || GlobalNew.RunMode == 2)
            {
                foreach (var value in GlobalNew.Devices.Values)
                {
                    if (value is DUT_BASE)
                    {

                        DUT_BASE temp = (DUT_BASE)value;
                        if (temp.Enable == false)
                            continue;
                        temp.DutDashboard?.LockSN_Textboxs();

                    }
                }
            }

            Script_Extra_CountDownEvent.ResetAll(GlobalNew.EnableDeviceCount);
        }
        private void BTNReport_Click(object sender, EventArgs e)
        {
            string path = GlobalNew.LOGFOLDER;
            System.Diagnostics.Process.Start(path);
        }
        private string barcode = "";
        private void MFGX_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Config 要設BarcodeReaderMode = 1是代表掃碼槍是有帶Enter
            if (GlobalNew.BarcodeReaderMode == "1")
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    Control focusedControl = this.ActiveControl;

                    //MainLogger.Log($" {focusedControl.Name}");
                    foreach (var value in GlobalNew.Devices.Values)
                    {
                        if (value is DUT_BASE)
                        {
                            DUT_BASE temp = (DUT_BASE)value;
                            if (barcode == temp.Description)
                            {
                                //Control focusedControl = GetFocusedControl(this);
                                //Control focusedControl = this.ActiveControl;
                                //Dashboard ds = focusedControl as Dashboard;
                                //if (ds != null)
                                //{
                                //    ds.ResetUISNtext(false);
                                //    //ResetUISNtext(bool isFocusFirst = false)
                                //}
                                
                                SN_Panel SNPanel = (SN_Panel)temp.DutDashboard.SNPanel.GetControlFromPosition(0, 0);
                                //MainLogger.Log($"Focus");
                                SNPanel.SN_Textbox.Focus();
                                barcode = "";
                                //e.Handled = true;
                            }
                        }
                    }
                    //MainLogger.Log($"{barcode}");
                    // 條碼輸入完成，處理條碼
                    barcode = ""; // 清空條碼變數
                    //MainLogger.Log("清空條碼變數");
                }
                else
                {
                    // 累積條碼字符
                    barcode += e.KeyChar;
                    //MainLogger.Log($"{barcode} += {e.KeyChar}");
                }
            }
            else
            {
                
                if (e.KeyChar == (char)Keys.Enter)
                {
                    //MainLogger.Log($"Enter->{barcode}");

                    Control focusedControl = this.ActiveControl;

                    //MainLogger.Log($" {focusedControl.Name}");
                    foreach (var value in GlobalNew.Devices.Values)
                    {
                        if (value is DUT_BASE)
                        {
                            DUT_BASE temp = (DUT_BASE)value;
                            if (temp.Enable == false)
                                continue;
                            if (barcode.Contains(temp.Description))
                            {
                                SN_Panel SNPanel = (SN_Panel)temp.DutDashboard.SNPanel.GetControlFromPosition(0, 0);
                                MessageBox.Show("Error Input");
                                SNPanel.SN_Textbox.Focus();
                                barcode = "";
                                e.Handled = true;
                            }
                        }
                    }
                    //MainLogger.Log($"{barcode}");
                }
                else
                {
                    // 累積條碼字符
                    barcode += e.KeyChar;
                    //MainLogger.Log($"{barcode} += {e.KeyChar}");

                    Control focusedControl = this.ActiveControl;

                    //MainLogger.Log($" {focusedControl.Name}");
                    foreach (var value in GlobalNew.Devices.Values)
                    {
                        if (value is DUT_BASE)
                        {
                            DUT_BASE temp = (DUT_BASE)value;
                            if (temp.Enable == false)
                                continue;
                            if (barcode.Contains(temp.Description))
                            {
                                SN_Panel SNPanel = (SN_Panel)temp.DutDashboard.SNPanel.GetControlFromPosition(0, 0);
                                //MainLogger.Log($"Focus");
                                SNPanel.SN_Textbox.Focus();
                                barcode = "";
                                e.Handled = true;
                                foreach (var value2 in GlobalNew.Devices.Values)
                                {
                                    if (value2 is DUT_BASE)
                                    {
                                        DUT_BASE temp2 = (DUT_BASE)value2;
                                        if (temp2.Enable == false)
                                            continue;
                                        SN_Panel SNPanel2 = (SN_Panel)temp2.DutDashboard.SNPanel.GetControlFromPosition(0, 0);
                                        if(!temp2.isRunning)
                                            SNPanel2.SN_Textbox.Text = "";
                                        
                                    }
                                }
                            }
                        }
                    }
                    //MainLogger.Log($"{barcode}");

                }
            }
        }
        private Control GetFocusedControl(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child.Focused)
                {
                    return child;
                }
                Control focusedChild = GetFocusedControl(child);
                if (focusedChild != null)
                {
                    return focusedChild;
                }
            }
            return null;
        }
        private void MFGX_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                // 處理 F5 按鍵事件
                foreach (var value in GlobalNew.Devices.Values)
                {
                    if (value is DUT_BASE)
                    {
                        DUT_BASE temp = (DUT_BASE)value;
                        if (temp.Enable == false)
                            continue;

                        if (temp.DutDashboard.MainTableLayout.ColumnStyles[0].Width == 0)
                        {
                            // 將左邊的列寬設置為 100%
                            temp.DutDashboard.MainTableLayout.ColumnStyles[0].SizeType = SizeType.Percent;
                            temp.DutDashboard.MainTableLayout.ColumnStyles[0].Width = 100;
                            temp.DutDashboard.MainTableLayout.ColumnStyles[1].SizeType = SizeType.Percent;
                            temp.DutDashboard.MainTableLayout.ColumnStyles[1].Width = 0;
                        }
                        else
                        {
                            temp.DutDashboard.MainTableLayout.ColumnStyles[0].SizeType = SizeType.Percent;
                            temp.DutDashboard.MainTableLayout.ColumnStyles[0].Width = 0;
                            temp.DutDashboard.MainTableLayout.ColumnStyles[1].SizeType = SizeType.Percent;
                            temp.DutDashboard.MainTableLayout.ColumnStyles[1].Width = 100;
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.F4)
            {
                // 處理 F4 按鍵事件
                foreach (var value in GlobalNew.Devices.Values)
                {
                    if (value is DUT_BASE)
                    {
                        DUT_BASE temp = (DUT_BASE)value;
                        if (temp.Enable == false)
                            continue;
                        if (temp.DutDashboard.MainLogLayout.RowStyles[0].Height == 100)
                        {
                            temp.DutDashboard.MainLogLayout.RowStyles[0].SizeType = SizeType.Percent;
                            temp.DutDashboard.MainLogLayout.RowStyles[0].Height = 80;
                            temp.DutDashboard.MainLogLayout.RowStyles[1].SizeType = SizeType.Percent;
                            temp.DutDashboard.MainLogLayout.RowStyles[1].Height = 20;
                        }
                        else
                        {

                            temp.DutDashboard.MainLogLayout.RowStyles[0].SizeType = SizeType.Percent;
                            temp.DutDashboard.MainLogLayout.RowStyles[0].Height = 100;
                            temp.DutDashboard.MainLogLayout.RowStyles[1].SizeType = SizeType.Percent;
                            temp.DutDashboard.MainLogLayout.RowStyles[1].Height = 0;
                        }

                    }
                }
            }
            else if (e.KeyCode == Keys.F6)
            {
                foreach (var value in GlobalNew.Devices.Values)
                {
                    if (value is DUT_BASE)
                    {
                        DUT_BASE temp = (DUT_BASE)value;
                        if (temp.Enable == false)
                            continue;

                        Dictionary<string, string>  tt = temp.DataCollection.GetSpecData();

                        ShowSpecData(tt);
                        return;
                    }
                }
            }

            //else
            //{

            //Control focusedControl = GetFocusedControl(this);
            ////Control focusedControl = this.ActiveControl;
            //if (focusedControl != null)
            //{
            //    if (focusedControl.Name == "ProductSN")
            //    {
            //        foreach (var value in GlobalNew.Devices.Values)
            //        {
            //            if (value is DUT_BASE)
            //            {
            //                DUT_BASE temp = (DUT_BASE)value;
            //                if (barcode == temp.Description)
            //                {
            //                    SN_Panel SNPanel = (SN_Panel)temp.DutDashboard.SNPanel.GetControlFromPosition(0, 0);

            //                    SNPanel.SN_Textbox.Text = "";
            //                    e.Handled = true;
            //                }
            //            }
            //        }

            //        return;
            //    }


            //}




            //foreach (var value in GlobalNew.Devices.Values)
            //{
            //    if (value is DUT_BASE)
            //    {
            //        DUT_BASE temp = (DUT_BASE)value;
            //        if (barcode == temp.Description)
            //        {
            //            SN_Panel SNPanel = (SN_Panel)temp.DutDashboard.SNPanel.GetControlFromPosition(0, 0);

            //            SNPanel.SN_Textbox.Focus();
            //            barcode = "";
            //            e.Handled = true;
            //        }
            //    }
            //}
            //}
        }
        private void ShowSpecData(Dictionary<string, string> specData)
        {
            Form viewerForm = new Form
            {
                Text = "Spec Data Viewer",
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent
            };

            DataGridView grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            // 加入顏色樣式
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.Black;
            grid.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;

            //grid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

            grid.Columns.Add("Key", "Key");
            grid.Columns.Add("Value", "Value");

            foreach (var kvp in specData)
            {
                grid.Rows.Add(kvp.Key, kvp.Value);
            }

            viewerForm.Controls.Add(grid);
            viewerForm.ShowDialog();
        }


        private void OnProgressChanged(object sender, MyEventArgs e)
        {
            // 更新UI
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => lb_passNum.Text = ""+ e.Pass));
            }
            else
            {
                lb_passNum.Text = "" + e.Pass;
            }

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => lb_FailNum.Text = "" + e.Fail));
            }
            else
            {
                lb_FailNum.Text = "" + e.Fail;
            }
        }

        private void label_totalpass_Click(object sender, EventArgs e)
        {
            // 更新UI
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => lb_passNum.Text = "" + 0));
                
            }
            else
            {
                lb_passNum.Text = "" + 0;
                GlobalNew.Total_Pass_Num = 0;
                //iniConfig.Writeini("CountNum", "Total_Pass_Num", GlobalNew.Total_Pass_Num.ToString());
            }

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => lb_FailNum.Text = "" + 0));
            }
            else
            {
                lb_FailNum.Text = "" + 0;
                GlobalNew.Total_Fail_Num = 0;
                //iniConfig.Writeini("CountNum", "Total_Fail_Num", GlobalNew.Total_Fail_Num.ToString());
            }
        }
        
        private void BTNMaintenance_Click(object sender, EventArgs e)
        {
            var rotary = GlobalNew.Devices.Values.OfType<RotaryTestController>().FirstOrDefault();
            if (rotary == null)
            {
                MessageBox.Show("Not Found RotaryTestController Device");
                return;
            }

            rotary.ShowStatusForm();
            //if (GlobalNew.CurrentUser != "pe")
            //{
            //    MessageBox.Show("請切換PE帳號進行維修");
            //    return;
            //}
            //// another UI
            //UnlockForm m_Form = null;

            //m_Form = new UnlockForm();

            //DialogResult res = m_Form.ShowDialog();
            //if (res == DialogResult.OK)
            //{
            //    foreach (var value in GlobalNew.Devices.Values)
            //    {
            //        if (value is DUT_BASE)
            //        {

            //            DUT_BASE temp = (DUT_BASE)value;
            //            if (temp.Enable == false)
            //                continue;
            //            temp.DutDashboard?.LockSN_Textboxs();

            //        }
            //    }
            //    MessageBox.Show("Record successfull.\nPlease Initialize Devices.");
            //}          
        }

        private void MFGX_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                iniConfig.Writeini("CountNum", "Total_Pass_Num", GlobalNew.Total_Pass_Num.ToString());
                iniConfig.Writeini("CountNum", "Total_Fail_Num", GlobalNew.Total_Fail_Num.ToString());
                //iniConfig.Writeini("CountNum", "Total_Abort_Num", Global.Total_Abort_Num.ToString());
            }
            catch (Exception ex)
            {
                return;
            }
        }


    }

    public class MainLogger
    {
        private static readonly string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log", "MainLog");

        public static void Log(string message)
        {
            try
            {
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string logFilePath = Path.Combine(logDirectory, $"{DateTime.Now:yyyy-MM-dd}.txt");

                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                string decodedJsonData = Regex.Unescape(logMessage);
                File.AppendAllText(logFilePath, decodedJsonData + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                Console.WriteLine($"An error occurred while logging: {ex.Message}");
            }

        }
    }
}
