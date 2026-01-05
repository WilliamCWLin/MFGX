// 適用於 .NET Framework 4.7.2（C# 7.x 相容）
// 目的：IO 即時顯示不卡 UI —— 背景快輪詢 + 狀態變化即時推送 UI + UI 節流（預設 ~30FPS）
// 備註：Timer 明確使用 WinForms 的 System.Windows.Forms.Timer（避免與 Threading.Timer 混淆）

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem;

namespace AutoTestSystem.Equipment.Teach
{
    using WinFormsTimer = System.Windows.Forms.Timer;

    public class IOViewerForm : Form
    {
        private readonly IOTeach ioTeach;

        // UI 刷新 Timer（僅讀快取、不打硬體）
        private readonly WinFormsTimer refreshTimer;

        // LED 圖示資源
        private readonly System.Drawing.Image LEDON = Properties.Resources.ledHigh;
        private readonly System.Drawing.Image LEDOFF = Properties.Resources.ledLow;

        // DI 狀態圖示對照
        private readonly Dictionary<string, PictureBox> inputStatusIcons = new Dictionary<string, PictureBox>();

        // 版面配置
        private TableLayoutPanel inputTable;
        private TableLayoutPanel outputTable;

        // 視窗關閉旗標
        private bool isClosing = false;

        // --- IO 快取與輪詢控管 ---
        private readonly object _cacheLock = new object();
        private Dictionary<string, bool> _cachedInputStatus = new Dictionary<string, bool>();
        private Dictionary<string, bool> _cachedOutputStatus = new Dictionary<string, bool>();
        private CancellationTokenSource _pollCts;
        private int _pollIntervalMs = 20;   // 背景輪詢間隔（快速）
        private int _pollInFlight = 0;      // 防重入（0: 無；1: 進行中）

        // --- UI 更新：節流 + 急送 ---
        private volatile bool _uiRefreshBusy = false;
        private int _uiMinRefreshMs = 33;               // ~30FPS
        private long _lastUiRefreshTicks = 0;           // Stopwatch ticks
        private int _uiRefreshPending = 0;              // 0/1，避免重複排程急送

        public IOViewerForm(IOTeach ioTeach)
        {
            this.ioTeach = ioTeach;

            Text = "IO Viewer";
            Size = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
            };

            Load += IOViewerForm_Load;
            FormClosing += IOViewerForm_FormClosing;

            Controls.Add(splitContainer);

            inputTable = CreateBaseTable(4);
            outputTable = CreateBaseTable(5);

            splitContainer.Panel1.Controls.Add(inputTable);
            splitContainer.Panel2.Controls.Add(outputTable);

            // UI Timer：僅讀快取更新畫面，不做任何硬體呼叫
            refreshTimer = new WinFormsTimer { Interval = _uiMinRefreshMs };
            refreshTimer.Tick += (s, e) => RefreshStatusOnly();
            refreshTimer.Start();

            // 背景輪詢：在背景緒批次讀取 DI/DO 狀態到快取（含 timeout 退避、差異檢測）
            _pollCts = new CancellationTokenSource();
            StartPollingLoop();
        }

        private void IOViewerForm_Load(object sender, EventArgs e)
        {
            if (Controls.Count > 0 && Controls[0] is SplitContainer splitContainer)
            {
                splitContainer.SplitterDistance = ClientSize.Width / 2;
            }
            TopMost = true;
            BuildTablesOptimized(); // 只建立 UI；狀態由背景輪詢 + UI Timer 更新
        }

        private void IOViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            isClosing = true;
            try { refreshTimer?.Stop(); } catch { }
            try { _pollCts?.Cancel(); } catch { }
        }

        // 背景輪詢：批次取得 DI/DO 狀態，偵測變化就急送 UI 刷新；失敗/timeout 退避
        private void StartPollingLoop()
        {
            var ct = _pollCts.Token;

            Task.Run(() =>
            {
                int delay = _pollIntervalMs; // 成功用快速輪詢；錯誤時會退避

                Dictionary<string, bool> lastInputs = null;
                Dictionary<string, bool> lastOutputs = null;

                while (!ct.IsCancellationRequested)
                {
                    if (Interlocked.Exchange(ref _pollInFlight, 1) == 1)
                    {
                        if (ct.WaitHandle.WaitOne(delay)) break;
                        continue;
                    }

                    try
                    {
                        // 真正的硬體呼叫在背景緒
                        Dictionary<string, bool> inputs = ioTeach.GetAllInputStatusFromCards();
                        Dictionary<string, bool> outputs = ioTeach.GetAllOutputStatusFromCards();

                        bool changed = !DictionaryEqual(lastInputs, inputs) || !DictionaryEqual(lastOutputs, outputs);

                        lock (_cacheLock)
                        {
                            _cachedInputStatus = inputs ?? new Dictionary<string, bool>();
                            _cachedOutputStatus = outputs ?? new Dictionary<string, bool>();
                        }

                        lastInputs = inputs;
                        lastOutputs = outputs;

                        if (changed)
                            RequestUiRefreshUrgent();

                        // 成功就恢復快速輪詢
                        delay = _pollIntervalMs;
                    }
                    catch
                    {
                        // 失敗/timeout：退避，避免猛打驅動；下限 50ms，上限 500ms
                        delay = Math.Min(Math.Max(delay * 2, 50), 500);
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _pollInFlight, 0);
                    }

                    if (ct.WaitHandle.WaitOne(delay)) break;
                }
            }, ct);
        }

        // 僅讀快取更新 UI（避免任何硬體呼叫） + FPS 節流
        private void RefreshStatusOnly()
        {
            if (isClosing) return;
            if (_uiRefreshBusy) return;
            _uiRefreshBusy = true;

            try
            {
                // 以 Stopwatch 進行節流（避免 UI 過度刷新）
                long now = Stopwatch.GetTimestamp();
                double msSinceLast = (now - _lastUiRefreshTicks) * 1000.0 / Stopwatch.Frequency;
                if (msSinceLast < _uiMinRefreshMs)
                    return; // 太快就略過，下一輪或急送再補

                _lastUiRefreshTicks = now;

                Dictionary<string, bool> inputsSnapshot;
                Dictionary<string, bool> outputsSnapshot;

                lock (_cacheLock)
                {
                    inputsSnapshot = _cachedInputStatus;
                    outputsSnapshot = _cachedOutputStatus;
                }

                // 更新 DI LED
                foreach (KeyValuePair<string, PictureBox> kvp in inputStatusIcons)
                {
                    bool s = false;
                    bool status = inputsSnapshot != null && inputsSnapshot.TryGetValue(kvp.Key, out s) && s;
                    System.Drawing.Image img = status ? LEDON : LEDOFF;

                    if (!object.ReferenceEquals(kvp.Value.Image, img))
                        kvp.Value.Image = img;
                }

                // 更新 DO Toggle
                foreach (Control ctl in outputTable.Controls)
                {
                    CheckBox cb = ctl as CheckBox;
                    if (cb != null && cb.Tag is string name)
                    {
                        bool sVal = false;
                        if (outputsSnapshot != null)
                            outputsSnapshot.TryGetValue(name, out sVal);

                        if (cb.Checked != sVal)
                        {
                            cb.CheckedChanged -= SetIOCheckBox_CheckedChanged;
                            cb.Checked = sVal;
                            cb.Text = sVal ? "ON" : "OFF";
                            cb.CheckedChanged += SetIOCheckBox_CheckedChanged;
                        }
                    }
                }
            }
            finally
            {
                _uiRefreshBusy = false;
            }
        }

        // 變化即時推送 UI：避免堆積重複請求
        private void RequestUiRefreshUrgent()
        {
            if (isClosing || !IsHandleCreated || IsDisposed) return;
            if (Interlocked.Exchange(ref _uiRefreshPending, 1) == 1) return;

            try
            {
                BeginInvoke((Action)(() =>
                {
                    _uiRefreshPending = 0;
                    RefreshStatusOnly();
                }));
            }
            catch
            {
                // 忽略關閉中例外
            }
        }

        // 建立左右兩表
        private void BuildTablesOptimized()
        {
            BuildInputTableOptimized();
            BuildOutputTableOptimized();
        }

        // 建立 DI 表（LED 初始為 OFF，之後由快取更新）
        private void BuildInputTableOptimized()
        {
            inputTable.SuspendLayout();
            inputTable.Controls.Clear();

            AddHeaderRow(inputTable, new[] { "Sensor Name", "Status", "Card", "Channel" });

            int row = 1;
            foreach (string sensorName in ioTeach.GetInputSensorNames())
            {
                var entry = ioTeach.GetEntry(sensorName, true);

                var statusIcon = new PictureBox
                {
                    Image = LEDOFF,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(40, 40),
                    Anchor = AnchorStyles.None,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(2)
                };

                inputStatusIcons[sensorName] = statusIcon;

                inputTable.Controls.Add(new Label { Text = sensorName, AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 0, row);
                inputTable.Controls.Add(statusIcon, 1, row);
                inputTable.Controls.Add(new Label { Text = entry != null && entry.Card != null ? entry.Card.Description : "N/A", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 2, row);
                inputTable.Controls.Add(new Label { Text = entry != null ? entry.Channel.ToString() : "?", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 3, row);

                inputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
                row++;
            }

            // 補一行空白 Row，避免最後一列貼邊
            inputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            for (int col = 0; col < inputTable.ColumnCount; col++)
                inputTable.Controls.Add(new Label { Text = "", AutoSize = false, Dock = DockStyle.Fill }, col, row);

            inputTable.ResumeLayout(true);
        }

        // 建立 DO 表（Toggle 初始值由快取讀；之後 UI Timer 會持續同步）
        private void BuildOutputTableOptimized()
        {
            outputTable.SuspendLayout();
            outputTable.Controls.Clear();

            AddHeaderRow(outputTable, new[] { "Sensor Name", "Card", "Channel", "Control" });

            Dictionary<string, bool> outputsSnapshot;
            lock (_cacheLock)
            {
                outputsSnapshot = _cachedOutputStatus;
            }

            int row = 1;
            foreach (string sensorName in ioTeach.GetOutputSensorNames())
            {
                var entry = ioTeach.GetEntry(sensorName, false);

                var toggleButton = new CheckBox
                {
                    Appearance = Appearance.Button,
                    Text = "Toggle",
                    AutoSize = false,
                    Width = 80,
                    Height = 32,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Tag = sensorName,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(2)
                };

                bool currentStatus = false;
                if (outputsSnapshot != null)
                    outputsSnapshot.TryGetValue(sensorName, out currentStatus);

                toggleButton.Checked = currentStatus;
                toggleButton.Text = currentStatus ? "ON" : "OFF";
                toggleButton.CheckedChanged += SetIOCheckBox_CheckedChanged;

                outputTable.Controls.Add(new Label { Text = sensorName, AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 0, row);
                outputTable.Controls.Add(new Label { Text = entry != null && entry.Card != null ? entry.Card.Description : "N/A", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 1, row);
                outputTable.Controls.Add(new Label { Text = entry != null ? entry.Channel.ToString() : "?", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill }, 2, row);
                outputTable.Controls.Add(toggleButton, 3, row);

                outputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
                row++;
            }

            // 補一行空白 Row，避免最後一列貼邊
            outputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            for (int col = 0; col < outputTable.ColumnCount; col++)
                outputTable.Controls.Add(new Label { Text = "", AutoSize = false, Dock = DockStyle.Fill }, col, row);

            outputTable.ResumeLayout(true);
        }

        // DO 切換：在背景緒呼叫 SetIO，避免卡 UI；成功後等背景輪詢更新快取，UI 會自動跟上
        private void SetIOCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            string sensorName = checkBox != null ? checkBox.Tag as string : null;
            if (sensorName == null) return;

            bool desired = checkBox.Checked;

            // 暫時停用按鈕，避免連點造成多次呼叫
            checkBox.Enabled = false;

            Task.Run(() =>
            {
                try
                {
                    ioTeach.SetIO(sensorName, desired);
                    // 成功：等下一輪背景輪詢把 _cachedOutputStatus 同步
                }
                catch
                {
                    // 失敗：將期望值翻回去（僅 UI 回退）
                    desired = !desired;
                }
            })
            .ContinueWith(_ =>
            {
                if (isClosing || !IsHandleCreated || IsDisposed) return;

                try
                {
                    checkBox.CheckedChanged -= SetIOCheckBox_CheckedChanged;
                    checkBox.Checked = desired;
                    checkBox.Text = desired ? "ON" : "OFF";
                    checkBox.CheckedChanged += SetIOCheckBox_CheckedChanged;
                }
                finally
                {
                    checkBox.Enabled = true;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        // 建立共用表格
        private TableLayoutPanel CreateBaseTable(int columnCount)
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = columnCount,
                AutoScroll = true,
                Padding = new Padding(10),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            float[] columnWidths = columnCount == 4
                ? new float[] { 40f, 20f, 20f, 20f }      // DI：Name / Status / Card / Channel
                : new float[] { 35f, 15f, 20f, 15f, 15f };// DO：Name / Card / Channel / Control / (保留)

            for (int i = 0; i < columnWidths.Length; i++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, columnWidths[i]));

            return table;
        }

        // 加入表頭
        private void AddHeaderRow(TableLayoutPanel table, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                var headerLabel = new Label
                {
                    Text = headers[i],
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Font = new Font(Font, FontStyle.Bold),
                    BackColor = Color.FromArgb(220, 230, 250),
                    ForeColor = Color.Navy,
                    Padding = new Padding(2)
                };
                table.Controls.Add(headerLabel, i, 0);
            }
        }

        // 傳統相等比較（避免外部產生新 Dictionary 造成 ReferenceEquals 失效）
        private static bool DictionaryEqual(Dictionary<string, bool> a, Dictionary<string, bool> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;

            foreach (KeyValuePair<string, bool> kv in a)
            {
                bool val;
                if (!b.TryGetValue(kv.Key, out val)) return false;
                if (val != kv.Value) return false;
            }
            return true;
        }

        // 保留：按鈕版 DO 切換（若 elsewhere 需要）
        private void SetIOButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string sensorName = btn != null ? btn.Tag as string : null;
            if (sensorName == null) return;

            bool desired = true;
            lock (_cacheLock)
            {
                bool cur;
                if (_cachedOutputStatus != null && _cachedOutputStatus.TryGetValue(sensorName, out cur))
                    desired = !cur;
            }

            Task.Run(() =>
            {
                try { ioTeach.SetIO(sensorName, desired); } catch { }
            });
        }
    }
}
