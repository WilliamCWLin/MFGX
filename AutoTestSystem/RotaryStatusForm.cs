// RotaryStatusForm.cs
// 適用於 .NET Framework 4.7.2 (C# 7.0 相容語法)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AutoTestSystem.Equipment.Teach;
using AutoTestSystem.Base;
using AutoTestSystem.Model;
using System.IO;
using System.Drawing.Drawing2D;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace AutoTestSystem
{
    public class DoubleBufferedPnl : Panel
    {
        public DoubleBufferedPnl()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            UpdateStyles();
        }
    }

    public class RotaryStatusForm : Form
    {
        private RotaryTestController controller;
        private DoubleBufferedPnl canvasPanel;
        private RichTextBox logBox;
        private SplitContainer splitContainer;
        //private Timer animationTimer;
        private ToolTip toolTip;
        private List<Rectangle> dutHitAreas = new List<Rectangle>();

        // ----- 自訂 DI 狀態監控區 -----
        private DataGridView dgvMutiDI;
        private List<MutiIOSelect.DeviceData> mutiDIWatchList = new List<MutiIOSelect.DeviceData>();
        //private Timer mutiDITimer;
        private IOTeach ioTeach => (GlobalNew.Devices.TryGetValue(controller.IODeviceSel, out var dev) && dev is IOTeach i) ? i : null;

        // 其它原有成員
        private double animatedAngle;
        private double targetAngle;
        private string logFilePath;
        private int lastTipIndex = -1;
        private IOViewerForm IOForm;
        private TableLayoutPanel mainLayout;  // 方便操作
        private TableLayoutPanel topLayout;
        private bool leftVisible = true, logVisible = true;

        private readonly System.Drawing.Image LEDON = Properties.Resources.ledHigh;
        private readonly System.Drawing.Image LEDOFF = Properties.Resources.ledLow;
        private readonly System.Drawing.Image Rotate = Properties.Resources.circle;
        // 在 RotaryStatusForm 的成員變數加上
        private Label statusLabel;
        private TableLayoutPanel canvasLayout;

        private Image obsidianImage;

        // ====== 新增：統一關閉/資源管理所需欄位 ======
        private System.Windows.Forms.Timer mainTimer;
        private System.Windows.Forms.Timer _logUiTimer;
        private System.Windows.Forms.Timer closeTimer;   // 由原本建構子區域變數改為欄位
        private bool _isClosing = false;

        // 將 Paint 內字型抽成欄位，關閉時可釋放
        private Font _fontBig = new Font("Arial", 14, FontStyle.Bold);
        private Font _fontInner = new Font("Arial", 14, FontStyle.Bold);

        private int tickCounter = 0;
        private readonly ConcurrentQueue<string> _logUiQueue = new ConcurrentQueue<string>();

        public RotaryStatusForm(RotaryTestController controller)
        {
            this.controller = controller;
            this.Text = "圓盤測試狀態";
            this.Size = new Size(800, 600);
            //this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 250, 255);
            this.TopMost = true;

            // 主 Layout：上下兩列
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.FromArgb(245, 250, 255)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70)); // 上方 70%
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // 下方 30%
            this.Controls.Add(mainLayout);

            // 上方 Layout：兩欄（左 DI、右動畫區）
            topLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                BackColor = Color.FromArgb(245, 250, 255)
            };
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280)); // 左
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // 右

            // 左：DI監控
            dgvMutiDI = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Arial", 9),
                BackgroundColor = Color.WhiteSmoke,
                ScrollBars = ScrollBars.Both
            };
            dgvMutiDI.Columns.Add("IO_Name", "IO 名稱");
            dgvMutiDI.Columns["IO_Name"].HeaderCell.Style.Font = new Font("微軟正黑體", 10F, FontStyle.Bold);

            var expectedCol = new DataGridViewImageColumn
            {
                Name = "ExpectedStatus",
                HeaderText = "預期狀態",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
            };
            expectedCol.HeaderCell.Style.Font = new Font("微軟正黑體", 10F, FontStyle.Bold);
            dgvMutiDI.Columns.Add(expectedCol);

            var imgCol = new DataGridViewImageColumn
            {
                Name = "CurrentStatus",
                HeaderText = "目前狀態",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
            };
            imgCol.HeaderCell.Style.Font = new Font("微軟正黑體", 10F, FontStyle.Bold);
            dgvMutiDI.Columns.Add(imgCol);

            dgvMutiDI.CellFormatting += DgvMutiDI_CellFormatting;
            dgvMutiDI.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMutiDI.DefaultCellStyle.SelectionBackColor = Color.White;
            dgvMutiDI.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvMutiDI.ColumnHeadersHeight = 30;

            var diGroup = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "自訂 DI 狀態監控",
                Font = new Font("微軟正黑體", 11, FontStyle.Bold),
            };
            diGroup.Controls.Add(dgvMutiDI);
            topLayout.Controls.Add(diGroup, 0, 0);

            // ----------- 右側圓盤動畫區塊 + 狀態Label -----------
            // 1. 先建立圓盤 Panel
            canvasPanel = new DoubleBufferedPnl();
            canvasPanel.Dock = DockStyle.Fill;
            canvasPanel.BackColor = Color.FromArgb(245, 250, 255);
            canvasPanel.Paint += CanvasPanel_Paint;
            canvasPanel.MouseMove += CanvasPanel_MouseMove;
            canvasPanel.MouseDoubleClick += CanvasPanel_MouseDoubleClick;

            // 2. 建立 TableLayoutPanel，放 canvasPanel（上）與 statusLabel（下）
            canvasLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.FromArgb(245, 250, 255)
            };
            canvasLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 85F)); // 上 85% 放圓盤
            canvasLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15F)); // 下 15% 放Label
            canvasLayout.Controls.Add(canvasPanel, 0, 0);

            // 3. 建立狀態顯示Label
            statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkGreen,
                BackColor = Color.Transparent,
                Text = "",
            };
            canvasLayout.Controls.Add(statusLabel, 0, 1);

            // 4. 放到 topLayout
            topLayout.Controls.Add(canvasLayout, 1, 0);

            // ---- 加入 topLayout 到主畫面
            mainLayout.Controls.Add(topLayout, 0, 0);

            // ---- 下方：Log 顯示區
            logBox = new RichTextBox();
            logBox.Dock = DockStyle.Fill;
            logBox.Font = new Font("Consolas", 9);
            logBox.ReadOnly = true;
            logBox.BackColor = Color.White;
            logBox.ForeColor = Color.Black;
            logBox.WordWrap = false;
            logBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            mainLayout.Controls.Add(logBox, 0, 1);

            // 其它初始化
            animatedAngle = controller.CurrentAngle;
            targetAngle = controller.CurrentAngle;

            InitMainTimer();

            //animationTimer = new Timer();
            //animationTimer.Interval = 5;
            //animationTimer.Tick += AnimationTimer_Tick;
            //animationTimer.Start();

            _logUiTimer = new Timer { Interval = 100 };
            _logUiTimer.Tick += (s, e) => FlushLogUI(80);
            _logUiTimer.Start();

            closeTimer = new Timer();
            closeTimer.Interval = 200;
            closeTimer.Tick += delegate
            {
                if (GlobalNew.g_shouldStop)
                {
                    this.DialogResult = DialogResult.Cancel;
                    if (IOForm != null && !IOForm.IsDisposed)
                    {
                        IOForm.Close();
                    }
                    this.Close();
                    if (!this.IsDisposed && this.Visible)
                    {
                        MessageBox.Show(
                            "流程中止。請檢查問題後再重新執行流程。\n\nProcess Aborted. Please check the issue before restarting the process.",
                            "錯誤 / Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            };
            closeTimer.Start();

            // 統一的關閉處理（合併你原本的兩段 FormClosed +=）
            this.FormClosed += (s, e) =>
            {
                _isClosing = true;
                SafeStopAndDisposeTimers();

                // 關閉子視窗
                try { if (IOForm != null && !IOForm.IsDisposed) IOForm.Close(); } catch { }

                // 釋放 UI/GDI 物件
                try { toolTip?.Dispose(); toolTip = null; } catch { }
                try { _fontBig?.Dispose(); _fontBig = null; } catch { }
                try { _fontInner?.Dispose(); _fontInner = null; } catch { }

                // 清空 log 佇列
                try { while (_logUiQueue.TryDequeue(out _)) { } } catch { }
            };

            logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log", "RotaryLog_" + DateTime.Now.ToString("yyyyMMdd_HH") + ".txt");

            toolTip = new ToolTip();
            toolTip.InitialDelay = 200;
            toolTip.ReshowDelay = 100;
            toolTip.AutoPopDelay = 10000;
            toolTip.ShowAlways = true;

            SetMutiGetDIWatchList(controller.MutiGetDI);

            // DI監控自動刷新
            //mutiDITimer = new Timer();
            //mutiDITimer.Interval = 5;
            //mutiDITimer.Tick += (s, e) => UpdateMutiDIWatch();
            //mutiDITimer.Start();

            this.KeyPreview = true;
            this.KeyDown += RotaryStatusForm_KeyDown;
        }

        // RotaryStatusForm.cs
        private void InitMainTimer()
        {
            mainTimer = new Timer();
            mainTimer.Interval = 10; // 保留你原本設定；想省資源可改 33ms
            mainTimer.Tick += MainTimer_Tick;
            mainTimer.Start();
        }

        private void SafeStopAndDisposeTimers()
        {
            try { mainTimer?.Stop(); mainTimer?.Dispose(); mainTimer = null; } catch { }
            try { _logUiTimer?.Stop(); _logUiTimer?.Dispose(); _logUiTimer = null; } catch { }
            try { closeTimer?.Stop(); closeTimer?.Dispose(); closeTimer = null; } catch { }
        }

        private int _logTick = 0;
        private int _diTick = 0;

        public void UpdateAnimatedAngle(double curPos)
        {
            // 每次都更新動畫與狀態標籤
            animatedAngle = curPos;
            if (!_isClosing)
            {
                canvasPanel.Invalidate();
                UpdateStatusLabel();
            }
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (_isClosing) return;

            // 1) 角度：每 Tick 刷
            double angle = controller.UiGetLatestAngle();
            UpdateAnimatedAngle(angle);

            // 2) Log：批次刷（約 100ms）
            _logTick++;
            if (_logTick >= 3) // 3*~33ms ≈ 100ms（你的 interval=10 時頻率更高，仍可視為節流）
            {
                _logTick = 0;
                var batch = new List<string>(256);
                int n = controller.UiDrainLogs(batch, 500); // 一次最多 500 行
                for (int i = 0; i < n; i++)
                {
                    AppendLog(batch[i]);
                }
            }

            // 3) DI 監控：不要太頻繁（約 200ms）
            _diTick++;
            if (_diTick >= 6)
            {
                _diTick = 0;
                var di = controller.UiGetLatestIo();
                if (di != null && di.Count > 0)
                    UpdateMutiDIWatch(di);
            }

            // 4) 狀態標籤
            UpdateStatusLabel();
        }

        private void UpdateStatusLabel()
        {
            string resultText = string.Empty;
            bool allTesting = true;

            foreach (var unit in controller.UnitsOnDisk)
            {
                if (!string.IsNullOrEmpty(unit.testUnit.ShowStatus))
                {
                    resultText = unit.testUnit.ShowStatus;
                    allTesting = false;
                    break;
                }
            }

            if (allTesting)
            {
                if (statusLabel.Text != "Testing" || statusLabel.BackColor != Color.Yellow)
                {
                    statusLabel.Text = "Testing";
                    statusLabel.ForeColor = Color.Black;
                    statusLabel.BackColor = Color.Yellow;
                }
                return;
            }

            // 根據 resultText 設定樣式
            Color newBackColor;
            if (resultText == "SKIP")
                newBackColor = Color.Turquoise;
            else if (resultText.Contains(":PASS"))
                newBackColor = Color.ForestGreen;
            else
                newBackColor = Color.OrangeRed;

            if (statusLabel.Text != resultText || statusLabel.BackColor != newBackColor)
            {
                statusLabel.Text = resultText;
                statusLabel.ForeColor = Color.Black;
                statusLabel.BackColor = newBackColor;
            }
        }

        private void RotaryStatusForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                leftVisible = !leftVisible;
                topLayout.Controls[0].Visible = leftVisible;
                topLayout.ColumnStyles[0].Width = leftVisible ? 330 : 0;
            }
            else if (e.KeyCode == Keys.F2)
            {
                logVisible = !logVisible;
                mainLayout.Controls[1].Visible = logVisible;
                mainLayout.RowStyles[1].Height = logVisible ? 20 : 0; // 20% 或 0%
            }
        }

        /// <summary>
        /// 設定自訂DI關注清單（JSON字串）
        /// </summary>
        public void SetMutiGetDIWatchList(string json)
        {
            try
            {
                mutiDIWatchList = JsonConvert.DeserializeObject<List<MutiIOSelect.DeviceData>>(json) ?? new List<MutiIOSelect.DeviceData>();
            }
            catch
            {
                mutiDIWatchList = new List<MutiIOSelect.DeviceData>();
            }
            ReloadMutiDIWatchTable();
        }

        private void ReloadMutiDIWatchTable()
        {
            dgvMutiDI.Rows.Clear();
            foreach (var item in mutiDIWatchList)
            {
                // 預期狀態用圖片（LEDON/LEDOFF）
                Image expectImg = null;
                if (item.IO_Status == "True")
                    expectImg = LEDON;
                else if (item.IO_Status == "False")
                    expectImg = LEDOFF;
                dgvMutiDI.Rows.Add(item.IO_Name, expectImg, null);
            }
            dgvMutiDI.ClearSelection();
        }

        /// <summary>
        /// 定時更新目前 DI 狀態
        /// </summary>
        private void UpdateMutiDIWatch()
        {
            if (mutiDIWatchList == null || mutiDIWatchList.Count == 0 || ioTeach == null) return;

            Dictionary<string, bool> diDict;
            try
            {
                diDict = ioTeach.GetAllInputStatusFromCards();
            }
            catch
            {
                diDict = new Dictionary<string, bool>();
            }

            for (int i = 0; i < mutiDIWatchList.Count; i++)
            {
                var item = mutiDIWatchList[i];
                string ioName = item.IO_Name;
                string currentStr;
                if (diDict.TryGetValue(ioName, out bool curr))
                {
                    currentStr = curr ? "True" : "False";
                }
                else
                {
                    currentStr = ""; // 沒查到時顯示空
                }

                // 用圖片顯示
                if (currentStr == "True")
                    dgvMutiDI.Rows[i].Cells[2].Value = LEDON;
                else if (currentStr == "False")
                    dgvMutiDI.Rows[i].Cells[2].Value = LEDOFF;
                else
                    dgvMutiDI.Rows[i].Cells[2].Value = null;
            }
            dgvMutiDI.Refresh();

            dgvMutiDI.ClearSelection();
        }

        public void UpdateMutiDIWatch(Dictionary<string, bool> allinput)
        {
            if (mutiDIWatchList == null || mutiDIWatchList.Count == 0 || ioTeach == null) return;

            Dictionary<string, bool> diDict;
            try
            {
                diDict = allinput;
            }
            catch
            {
                diDict = new Dictionary<string, bool>();
            }

            for (int i = 0; i < mutiDIWatchList.Count; i++)
            {
                var item = mutiDIWatchList[i];
                string ioName = item.IO_Name;
                string currentStr;
                if (diDict.TryGetValue(ioName, out bool curr))
                {
                    currentStr = curr ? "True" : "False";
                }
                else
                {
                    currentStr = ""; // 沒查到時顯示空
                }

                // 用圖片顯示
                if (currentStr == "True")
                    dgvMutiDI.Rows[i].Cells[2].Value = LEDON;
                else if (currentStr == "False")
                    dgvMutiDI.Rows[i].Cells[2].Value = LEDOFF;
                else
                    dgvMutiDI.Rows[i].Cells[2].Value = null;
            }
            dgvMutiDI.Refresh();

            dgvMutiDI.ClearSelection();
        }

        /// <summary>
        /// 異常顏色高亮
        /// </summary>
        private void DgvMutiDI_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            // 只針對目前狀態欄才處理
            if (e.ColumnIndex == 2)
            {
                var expectedStr = mutiDIWatchList[e.RowIndex].IO_Status; // "True"/"False"
                // 取得目前圖示實際是ON還是OFF
                var currentCell = dgvMutiDI.Rows[e.RowIndex].Cells[2];
                bool currValue = false;
                if (currentCell.Value == LEDON)
                    currValue = true;
                else if (currentCell.Value == LEDOFF)
                    currValue = false;

                bool expectValue = (expectedStr == "True");

                if (expectedStr != "" && currValue != expectValue)
                {
                    dgvMutiDI.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightPink;
                    dgvMutiDI.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Red;
                    dgvMutiDI.Rows[e.RowIndex].DefaultCellStyle.Font = new Font(dgvMutiDI.Font, FontStyle.Bold);
                }
                else
                {
                    dgvMutiDI.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                    dgvMutiDI.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
                    dgvMutiDI.Rows[e.RowIndex].DefaultCellStyle.Font = dgvMutiDI.Font;
                }
            }
        }

        // ================== 下方維持你的原本動畫與狀態邏輯 ==================

        private void CanvasPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Point center = new Point(canvasPanel.Width / 2, canvasPanel.Height / 2);
            int radius = 50;

            double dx = e.X - center.X;
            double dy = e.Y - center.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance <= radius)
            {
                if (GlobalNew.Devices.TryGetValue(controller.IODeviceSel, out var device) && device is IOTeach iotech)
                {
                    if (IOForm == null || IOForm.IsDisposed)
                    {
                        IOForm = new IOViewerForm(iotech);
                    }
                    IOForm.Show();
                }
            }
        }

        public void AppendLog(string message)
        {
            if (_isClosing) return;
            if (this.IsDisposed || logBox.IsDisposed) return;
            string logLine = string.Format("[{0:HH:mm:ss.fff}] {1}", DateTime.Now, message);
            _logUiQueue.Enqueue(logLine);   // 只入佇列，不直接動 UI
        }

        private void FlushLogUI(int maxLinesPerTick)
        {
            if (_isClosing) return;
            if (!this.IsHandleCreated || this.IsDisposed || logBox.IsDisposed) return;

            string line;
            int n = 0;
            while (n < maxLinesPerTick && _logUiQueue.TryDequeue(out line))
            {
                AppendLogToUI(line); // 你原本的方法：logBox.AppendText(...)+捲動
                n++;
            }

            // 可選：做個輕量的裁切，避免行數太多拖慢排版
            // if (logBox.TextLength > 200_000) logBox.Clear();
        }

        private void AppendLogToUI(string logLine)
        {
            if (this.IsDisposed || logBox.IsDisposed) return;

            if (logBox.TextLength > 40000)
                logBox.Clear();

            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionLength = 0;

            // 根據關鍵字設定格式
            if (logLine.Contains("Warning") || logLine.Contains("Error") || logLine.Contains("Fail") || logLine.Contains("錯誤") || logLine.Contains("失敗") || logLine.Contains("警告"))
            {
                logBox.SelectionColor = Color.Red;
                logBox.SelectionBackColor = Color.LightYellow;
            }
            else
            {
                logBox.SelectionColor = Color.Black;
                logBox.SelectionBackColor = logBox.BackColor;
            }

            logBox.AppendText(logLine + "\n");

            logBox.SelectionStart = logBox.Text.Length;
            logBox.ScrollToCaret();
        }

        public void RotateTo(double newAngle)
        {
            targetAngle = ((newAngle % 360) + 360) % 360;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            animatedAngle = controller.CurrentPhysicalAngle;
            canvasPanel.Invalidate();
            UpdateStatusLabel();
        }

        private void CanvasPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int nStations = controller.MotionSegments.Count;
            int diameter = Math.Min(canvasPanel.Width, canvasPanel.Height) - 150;
            Point center = new Point(canvasPanel.Width / 2, canvasPanel.Height / 2);
            Rectangle circleRect = new Rectangle(center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);

            // 背景漸層
            ColorBlend blend = new ColorBlend();
            blend.Positions = new float[] { 0f, 0.32f, 0.52f, 0.76f, 1f };
            blend.Colors = new Color[]
            {
                Color.FromArgb(45, 148, 170),    // 最外圈 深松石藍 (深青藍)
                Color.FromArgb(30, 197, 211),    // 松石亮藍
                Color.FromArgb(180, 240, 250),   // 高光冷白（淺松石藍）
                Color.FromArgb(30, 197, 211),    // 松石亮藍
                Color.FromArgb(45, 148, 170)     // 最外圈 深松石藍
            };

            using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                canvasPanel.ClientRectangle,
                Color.White, Color.White,
                LinearGradientMode.Vertical))
            {
                bgBrush.InterpolationColors = blend;
                g.FillRectangle(bgBrush, canvasPanel.ClientRectangle);
            }

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(circleRect);
                using (PathGradientBrush diskBrush = new PathGradientBrush(path))
                {
                    diskBrush.CenterColor = Color.FromArgb(20, 20, 20); // 中心亮灰
                    diskBrush.SurroundColors = new Color[] { Color.FromArgb(40, 40, 40) }; // 外圍深灰
                    diskBrush.FocusScales = new PointF(1.2f, 1.2f); // 拉大高光區域
                    g.FillEllipse(diskBrush, circleRect);
                }
            }

            g.DrawEllipse(new Pen(Color.MidnightBlue, 4), circleRect);

            var bigFont = _fontBig;
            var innerFont = _fontInner;

            dutHitAreas.Clear();

            for (int i = 0; i < nStations; i++)
            {
                double relativeAngle = (controller.GetStationAngle(i) + animatedAngle) % 360;
                double visualAngle = (450 - relativeAngle) % 360;
                double rad = visualAngle * Math.PI / 180;

                int r = diameter / 2 - 30;
                int x = (int)(center.X + r * Math.Cos(rad));
                int y = (int)(center.Y + r * Math.Sin(rad));

                int dutIdx = (i == 0) ? 0 : nStations - i;
                var unit = controller.UnitsOnDisk[dutIdx];

                Brush dutBrush;
                Color resultColor = Color.CadetBlue;
                string resultText = string.Empty;

                if (!unit.testUnit.IsActive)
                    dutBrush = Brushes.Gray;
                else
                {
                    string errorcode = unit.DataCollection?.GetMoreProp("Failitem") ?? "";
                    if (unit.testUnit.IsSkip)
                        dutBrush = Brushes.AliceBlue;
                    else
                    {
                        if (string.IsNullOrEmpty(unit.testUnit.ShowStatus))
                        {
                            dutBrush = string.IsNullOrEmpty(errorcode) ? Brushes.Yellow : Brushes.OrangeRed;
                        }
                        else if (unit.testUnit.ShowStatus.Contains(":PASS"))
                            dutBrush = Brushes.ForestGreen;         // PASS → 綠色
                        else
                            dutBrush = Brushes.OrangeRed;          // 其它 → 紅色
                    }
                }

                Rectangle hitRect = new Rectangle(x - 22, y - 22, 44, 44);
                dutHitAreas.Add(hitRect);
                g.FillEllipse(dutBrush, hitRect);
                g.DrawEllipse(new Pen(Color.DimGray, 2), hitRect);

                SizeF numSize = g.MeasureString(unit.Description, innerFont);
                g.DrawString(unit.Description, innerFont, Brushes.Black, x - numSize.Width / 2, y - numSize.Height / 2);
            }

            for (int i = 0; i < nStations; i++)
            {
                double visualAngle = (450 - controller.GetStationAngle(i)) % 360;
                double rad = visualAngle * Math.PI / 180;

                int r = diameter / 2 - 40;
                int outerR = r + 70;

                int tx = (int)(center.X + outerR * Math.Cos(rad));
                int ty = (int)(center.Y + outerR * Math.Sin(rad));
                string stationLabel = "T " + (i + 1);
                SizeF txtSize = g.MeasureString(stationLabel, bigFont);

                // 改成只畫一次：
                g.DrawString(stationLabel, bigFont, Brushes.Blue, tx - txtSize.Width / 2, ty - txtSize.Height / 2);
            }
        }

        private void CanvasPanel_MouseMove(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < dutHitAreas.Count; i++)
            {
                if (dutHitAreas[i].Contains(e.Location))
                {
                    if (lastTipIndex != i)
                    {
                        lastTipIndex = i;
                        int nStations = controller.MotionSegments.Count;
                        int dutIdx = (i == 0) ? 0 : nStations - i;
                        DUT_BASE unit = controller.UnitsOnDisk[dutIdx];
                        TestUnit t = unit.testUnit;

                        string sn = unit.DataCollection?.GetMoreProp("ProductSN") ?? "(noSN)";
                        string errorcode = unit.DataCollection?.GetMoreProp("Failitem") ?? "";

                        string completedFlags = "";
                        if (t.StationCompleted != null)
                        {
                            for (int j = 0; j < t.StationCompleted.Length; j++)
                                completedFlags += t.StationCompleted[j] ? "✅" : "❌";
                        }

                        string statusFlags = string.Format(
                            "Active: {0}\nEnd: {1}\nSkip: {2}",
                            t.IsActive ? "✅" : "❌",
                            t.IsTestEnd ? "✅" : "❌",
                            t.IsSkip ? "✅" : "❌"
                        );

                        string tip = string.Format(
                            "DUT: {0}\nStatus: {1}\nSN: {2}\n{3}\nStations: {4}\nFailitem:{5}",
                            unit.Description,
                            t.ShowStatus,
                            sn,
                            statusFlags,
                            completedFlags,
                            errorcode
                        );

                        toolTip.SetToolTip(canvasPanel, tip);
                    }
                    return;
                }
            }

            toolTip.SetToolTip(canvasPanel, "");
            lastTipIndex = -1;
        }
    }
}
