using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.DevicesUI.Teach;
using AutoTestSystem.DevicesUI.IO;
using AutoTestSystem.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using NAudio.Gui;
using System.Threading.Tasks;
using System.Threading;


namespace AutoTestSystem.Equipment.Teach
{
    public class MotionTeach : TeachBase, IDisposable
    {
        [Category("Select IO Devices")]
        [Description("編輯路徑清單")]
        [Editor(typeof(SelectMotorDevices), typeof(UITypeEditor))]
        public List<string> SelectedDevices { get;  set; } = new List<string>();

        [Browsable(false)]
        public string OriginMode { get; set; } = string.Empty;

        
        [Browsable(false)]
        public MotionPath Path { get;  set; } = new MotionPath();

        public override bool Init(string jsonParam)
        {
            if (!string.IsNullOrWhiteSpace(jsonParam))
            {
                var loaded = GetParametersFromJson<MotionPath>();
                if (loaded != null)
                    Path = loaded;
            }
            return true;
        }
        public override bool UnInit() => true;
        public override bool Show()
        {
            if (!EnsureDevicesSelected()) return false;
            using (var form = new MotionTeachForm(this))
            {
                form.StartPosition = FormStartPosition.CenterScreen;
                return form.ShowDialog() == DialogResult.OK;
            }
        }
        public override T GetParametersFromJson<T>() { try { return base.GetParametersFromJson<T>(); } catch { return default; } }
        protected override string GetJsonParamString() => JsonConvert.SerializeObject(Path);

        private bool EnsureDevicesSelected()
        {
            if (SelectedDevices.Any()) return true;

            using (var dlg = new DeviceSelectionForm(SelectedDevices))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    SelectedDevices = dlg.GetSelectedKeys();
                    OriginMode = dlg.GetSelectedOriginMode();
                }

            }
            if (!SelectedDevices.Any())
            {
                MessageBox.Show("請至少選擇一個控制軸再進行教學", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        public override void Dispose()
        {
            Path?.Clear();
            GC.SuppressFinalize(this);
        }




    }

    //public class DeviceSelectionEditor : UITypeEditor
    //{
    //    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;
    //    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    //    {
    //        var current = value as List<string> ?? new List<string>();
    //        using (var form = new DeviceSelectionForm(new List<string>(current)))
    //        {
    //            if (form.ShowDialog() == DialogResult.OK) return form.GetSelectedKeys();
    //        }
    //        return current;
    //    }
    //}

    public enum ControlBaseType { MotionBase, IOBase }

    public enum MoveType { Linear, Arc, Spline, Independent, StepByStep }
    public enum ORG_MODE{ Linear, Arc, Spline, Independent, StepByStep }

    public abstract class Motion : ICloneable
    {
        [Browsable(false)] public ControlBaseType Type { get; protected set; }
        protected Motion(ControlBaseType type) { Type = type; }
        public abstract Motion Clone();
        object ICloneable.Clone() => Clone();
    }

    public class MotorMotion : Motion
    {

        [Editor(typeof(NumericUpDownProperty), typeof(UITypeEditor))]
        [Category("參數")] public double Program_Relvalue { get; set; }
        [Browsable(false)]
        [Category("參數")] public double Position { get; set; }
        [Category("參數")] public double StartSpeed { get; set; }
        [Category("參數")] public double Acceleration { get; set; }
        [Category("參數")] public double Deceleration { get; set; }
        [Category("參數"), Description("最大速度")] public double MaxVel { get; set; }
        public MotorMotion() : base(ControlBaseType.MotionBase) { }
        public override Motion Clone() => new MotorMotion { Position = Position, StartSpeed = StartSpeed, Acceleration = Acceleration, Deceleration = Deceleration, MaxVel = MaxVel };
    }

    public class IOMotion : Motion
    {
        [Category("參數")] public int Channel { get; set; }
        [Category("參數")] public bool OnOff { get; set; }
        public IOMotion() : base(ControlBaseType.IOBase) { }
        public override Motion Clone() => new IOMotion { Channel = Channel, OnOff = OnOff };
    }

    public class MotionSegment
    {
        [Category("段落設定")] public MoveType MoveType { get; set; }
        [Category("段落設定"), DisplayName("名稱")] public string SegmentName { get; set; }
        private readonly Dictionary<string, Motion> _motions = new Dictionary<string, Motion>();
        [Browsable(false)] public IReadOnlyDictionary<string, Motion> Motions => _motions;
        public void Add(string key, Motion motion)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key 不可為空白", nameof(key));
            if (motion == null) throw new ArgumentNullException(nameof(motion));
            if (_motions.ContainsKey(key)) throw new InvalidOperationException($"已存在 Key: {key}");
            _motions[key] = motion;
        }
        public bool Remove(string key) => _motions.Remove(key);
        public void Clear() => _motions.Clear();
    }
    public class OffsetForm : Form
    {
        private Dictionary<string, TextBox> _textBoxes = new Dictionary<string, TextBox>();
        public Dictionary<string, double> Offsets { get; private set; }
        public OffsetForm(IEnumerable<string> axes, Dictionary<string, double> currentOffsets)
        {
            StartPosition = FormStartPosition.CenterScreen;
            Offsets = new Dictionary<string, double>();
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = axes.Count(), ColumnCount = 2, AutoSize = true };
            int row = 0;
            foreach (var axis in axes)
            {
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                panel.Controls.Add(new Label { Text = axis, AutoSize = true }, 0, row);
                var tb = new TextBox { Width = 80 };
                // 顯示目前 offset
                if (currentOffsets != null && currentOffsets.TryGetValue(axis, out double val))
                    tb.Text = val.ToString();
                panel.Controls.Add(tb, 1, row);
                _textBoxes[axis] = tb;
                row++;
            }
            var btnOK = new Button { Text = "確定", Dock = DockStyle.Bottom };
            btnOK.Click += (s, e) =>
            {
                Offsets.Clear();
                foreach (var kv in _textBoxes)
                {
                    if (double.TryParse(kv.Value.Text, out double val))
                        Offsets[kv.Key] = val;
                    else
                        Offsets[kv.Key] = 0;
                }
                DialogResult = DialogResult.OK;
            };
            Controls.Add(panel);
            Controls.Add(btnOK);
            Text = "設定各軸 Offset";
            AutoSize = true;
        }

    }

    public class MotionPath
    {
        //暫無使用
        public Dictionary<string, double> Offsets { get;  set; }
        public Dictionary<string, double> ProgramOrigin { get; set; }
        private readonly List<MotionSegment> _segments = new List<MotionSegment>();
        [Browsable(false)] public IReadOnlyList<MotionSegment> Segments => _segments;
        public void AddSegmentAt(int index, MotionSegment segment)
        {
            if (index < 0 || index > _segments.Count) index = _segments.Count;
            if (segment == null) throw new ArgumentNullException(nameof(segment));
            if (_segments.Any(s => s.SegmentName == segment.SegmentName))
            {
                MessageBox.Show($"已存相同名稱段落: {segment.SegmentName}");
                return;
            }

            _segments.Insert(index, segment);
        }
        public void AddSegment(string name, MotionSegment segment) => AddSegmentAt(_segments.Count, segment);
        public bool RemoveSegment(string name)
        {
            var seg = _segments.FirstOrDefault(s => s.SegmentName == name);
            if (seg != null) return _segments.Remove(seg);
            return false;
        }
        public void MoveSegment(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= _segments.Count || newIndex < 0 || newIndex >= _segments.Count) return;
            var seg = _segments[oldIndex]; _segments.RemoveAt(oldIndex);
            _segments.Insert(newIndex, seg);
        }

        public void RemapAxisNames(Dictionary<string, string> axisMap)
        {
            // Motions
            foreach (var segment in _segments)
            {
                var motions = segment.Motions.ToList(); // 避免遍歷同時增刪
                foreach (var kv in motions)
                {
                    if (axisMap.ContainsKey(kv.Key) && axisMap[kv.Key] != kv.Key)
                    {
                        var motion = kv.Value;
                        segment.Remove(kv.Key);
                        segment.Add(axisMap[kv.Key], motion);
                    }
                }
            }
            // Offsets/Origin
            if (Offsets != null)
                Offsets = Offsets.ToDictionary(
                    kv => axisMap.ContainsKey(kv.Key) ? axisMap[kv.Key] : kv.Key,
                    kv => kv.Value);

            if (ProgramOrigin != null)
                ProgramOrigin = ProgramOrigin.ToDictionary(
                    kv => axisMap.ContainsKey(kv.Key) ? axisMap[kv.Key] : kv.Key,
                    kv => kv.Value);
        }

        public void Clear() => _segments.Clear();
    }

    public class MotionTeachForm : Form
    {
        private readonly MotionTeach _parent;
        private readonly Dictionary<string, object> _deviceInstances;
        private readonly Dictionary<string, object> _deviceMoveParam;
        private FlowLayoutPanel FlowPanelMoveParam;
        private Label lblSegmentName;
        private TextBox txtSegmentName;
        private ComboBox comboMoveType;
        private ListView lvSegments;
        private Button btnAdd, btnDelete, btnUp, btnDown, btnOffset, btnUpdate,btnProgramOrg;
        // 1. 在你的成員區加上
        private Button btnCopyToOtherDevice;
        private Label lblPercentage;
        private TrackBar trackBarPercentage;
        private double percentageValue = 1.0;

        private ContextMenuStrip contextMenu;

        public MotionTeachForm(MotionTeach parent)
        {
            _parent = parent;
            _deviceInstances = new Dictionary<string, object>();
            _deviceMoveParam = new Dictionary<string, object>();

            // 初始化 _parent.Path.Offsets 為所有軸的預設值 0
            // ProgramOrigin：只補 SelectedDevices 裡還沒有的軸，預設為 0
            if (_parent.Path.ProgramOrigin == null)
            {
                _parent.Path.ProgramOrigin = new Dictionary<string, double>();
            }
            foreach (var axis in _parent.SelectedDevices)
            {
                if (!_parent.Path.ProgramOrigin.ContainsKey(axis))
                    _parent.Path.ProgramOrigin[axis] = 0;
            }

            // Offsets：同理
            if (_parent.Path.Offsets == null)
            {
                _parent.Path.Offsets = new Dictionary<string, double>();
            }
            foreach (var axis in _parent.SelectedDevices)
            {
                if (!_parent.Path.Offsets.ContainsKey(axis))
                    _parent.Path.Offsets[axis] = 0;
            }


            foreach (var axis in _parent.SelectedDevices)
            {
                if (GlobalNew.Devices.TryGetValue(axis, out var dev))
                    _deviceInstances[axis] = dev;
            }

            InitializeComponent();
            InitializeTabs();
            PopulateSegmentsList();
            lvSegments.SelectedIndexChanged += LvSegments_SelectedIndexChanged;
            lvSegments.DoubleClick += LvSegments_DoubleClick;
        }

        private void InitializeComponent()
        {
            lblSegmentName = new Label { Text = "段落名稱:", Dock = DockStyle.Top, Height = 20 };
            txtSegmentName = new TextBox { Dock = DockStyle.Top, Height = 34 };
            comboMoveType = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Height = 24 };
            comboMoveType.Items.AddRange(Enum.GetValues(typeof(MoveType)).Cast<object>().ToArray());
            // 預設第一筆
            if (comboMoveType.Items.Count > 0)
                comboMoveType.SelectedIndex = 3;
            lvSegments = new ListView { Dock = DockStyle.Top, Height = 200, View = System.Windows.Forms.View.Details, FullRowSelect = true, GridLines = true, HideSelection = false };
            lvSegments.Columns.Add("段落名稱", 200);
            lvSegments.Columns.Add("移動類型", 120);
            lvSegments.Columns.Add("機械位置", 200); // 新增
            lvSegments.Columns.Add("程式位置", 200); // 新增
            lvSegments.Columns.Add("速度數值", 200); // 新增
            btnAdd = new Button { Text = "新增段落", Width = 80, Height = 40 };
            btnDelete = new Button { Text = "刪除段落", Width = 80, Height = 40 };
            btnUp = new Button { Text = "上移", Width = 60, Height = 40 };
            btnDown = new Button { Text = "下移", Width = 60, Height = 40 };
            btnOffset = new Button { Text = "完成", Width = 80, Height = 40 };
            btnUpdate = new Button { Text = "更新段落", Width = 80, Height = 40 };
            btnProgramOrg = new Button { Text = "原點設定", Width = 120, Height = 40 };
            // 2. 在 InitializeComponent() 最後加上：
            btnCopyToOtherDevice = new Button { Text = "匯入", Width = 120, Height = 40 };

            // 新增 TrackBar

            trackBarPercentage = new TrackBar
            {
                Dock = DockStyle.Top,
                Minimum = 1, // Start from 1
                Maximum = 100,
                TickFrequency = 10,
                LargeChange = 10,
                SmallChange = 1,
                Height = 40
            };

            trackBarPercentage.Value = 100;
            // 設定 TrackBar 的 ValueChanged 事件
            trackBarPercentage.ValueChanged += (sender, e) =>
            {
                lblPercentage.Text = trackBarPercentage.Value + "%";
                percentageValue = trackBarPercentage.Value / 100.0;
                MotionParamPanel.Percentage = percentageValue;
            };

            // 新增 Label 顯示 TrackBar 的值
            lblPercentage = new Label
            {
                Dock = DockStyle.Fill,
                Height = 20,
                Text = "100%" // 初始值
            };

            btnProgramOrg.Click += BtnOrgin_Click;
            btnAdd.Click += BtnAdd_Click;
            btnDelete.Click += BtnDelete_Click;
            btnUp.Click += (s, e) => MoveSelected(-1);
            btnDown.Click += (s, e) => MoveSelected(+1);
            btnOffset.Click += (s, e) =>
            {
                using (var dlg = new OffsetForm(_parent.SelectedDevices, _parent.Path.Offsets))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        _parent.Path.Offsets = new Dictionary<string, double>(dlg.Offsets);
                        // 你可以在這裡做進一步處理
                    }
                }
            };

            btnCopyToOtherDevice.Click += BtnImportFromOldTeach_Click;

            btnUpdate.Click += BtnUpdate_Click;

            var panelButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 60, FlowDirection = FlowDirection.LeftToRight };
            panelButtons.Controls.AddRange(new Control[] { btnAdd, btnDelete, btnUp, btnDown, btnOffset, btnUpdate , btnProgramOrg, btnCopyToOtherDevice, trackBarPercentage , lblPercentage });
            FlowPanelMoveParam = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 560, FlowDirection = FlowDirection.LeftToRight };

            Controls.Add(FlowPanelMoveParam);
            Controls.Add(panelButtons);
            Controls.Add(lvSegments);
            Controls.Add(comboMoveType);
            Controls.Add(txtSegmentName);
            Controls.Add(lblSegmentName);

            //Controls.Add(tabControl1);



            // Initialize context menu
            contextMenu = new ContextMenuStrip();
            var menuItem = new ToolStripMenuItem("Show Details");
            menuItem.Click += MenuItem_Click;
            contextMenu.Items.Add(menuItem);

            // Add context menu to ListView
            lvSegments.ContextMenuStrip = contextMenu;

            Text = "Motion Teach";
            ClientSize = new System.Drawing.Size(1000, 900);


        }
        private void BtnImportFromOldTeach_Click(object sender, EventArgs e)
        {
            // 1. 收集可用舊Teach，排除自己且需有 Path 和 SelectedDevices
            var teachList = GlobalNew.Devices.Values
                .OfType<MotionTeach>()
                .Where(t => t != _parent && t.Path != null && t.SelectedDevices.Count > 0)
                .ToList();

            if (teachList.Count == 0)
            {
                MessageBox.Show("目前沒有其他可用的舊 Teach 設定。");
                return;
            }

            // 2. 選單UI，顯示裝置名稱與軸列表
            var pickForm = new Form { Text = "選擇要匯入的舊 Teach", Width = 600, Height = 350, MinimumSize = new Size(600, 350) };
            var lb = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft JhengHei", 12),
                HorizontalScrollbar = true,
                ItemHeight = 26
            };

            // 改善顯示，ToString顯示軸名及路徑段數
            foreach (var t in teachList)
                lb.Items.Add($"{string.Join(",", t.SelectedDevices)}（段數:{t.Path.Segments.Count}）|ID:{t.Description}");

            // 建立 index->teach 的對應
            var idxTeachDict = teachList.Select((teach, idx) => new { idx, teach })
                                        .ToDictionary(x => x.idx, x => x.teach);

            pickForm.Controls.Add(lb);

            var btnOk = new Button { Text = "確定", Dock = DockStyle.Bottom, Height = 40 };
            btnOk.Click += (s, ev) => pickForm.DialogResult = DialogResult.OK;
            pickForm.Controls.Add(btnOk);

            if (pickForm.ShowDialog() != DialogResult.OK || lb.SelectedIndex < 0)
                return;

            // 3. 取得被選中的舊Teach
            var oldTeach = idxTeachDict[lb.SelectedIndex];
            var oldAxes = oldTeach.SelectedDevices;
            var newAxes = _parent.SelectedDevices;

            if (oldAxes.Count != newAxes.Count)
            {
                MessageBox.Show("舊Teach與新Teach的軸數不相符，請檢查。");
                return;
            }

            // 4. 對應軸名（順序預設，可調整）
            using (var mapForm = new AxisMappingForm(oldAxes, newAxes))
            {
                if (mapForm.ShowDialog() != DialogResult.OK)
                    return;

                var axisMap = mapForm.AxisMapping;

                // 5. 複製 Path，使用 TypeNameHandling.Auto 支援多型
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                string json = JsonConvert.SerializeObject(oldTeach.Path, settings);
                var newPath = JsonConvert.DeserializeObject<MotionPath>(json, settings);

                // 6. 軸名 Remap
                newPath.RemapAxisNames(axisMap);

                // 7. 套用到目前 MotionTeach
                _parent.Path = newPath;

                // 8. UI重載（若需同步面板，請一併刷新）
                //InitializeTabs();
                PopulateSegmentsList();

                MessageBox.Show("匯入舊Teach與軸名對應完成！");
            }
        }
        public class AxisMappingForm : Form
        {
            private readonly List<string> oldAxes;
            private readonly List<string> newAxes;
            private readonly Dictionary<string, ComboBox> comboBoxes = new Dictionary<string, ComboBox>();
            private readonly Button btnOK;
            private readonly Button btnCancel;

            public Dictionary<string, string> AxisMapping { get; private set; }

            public AxisMappingForm(IEnumerable<string> oldAxes, IEnumerable<string> newAxes)
            {
                this.oldAxes = oldAxes.ToList();
                this.newAxes = newAxes.ToList();

                Text = "軸名稱對應設定";
                Font = new Font("Microsoft JhengHei UI", 12);
                BackColor = Color.White;
                Width = 550;
                Height = 80 + 52 * this.oldAxes.Count;
                StartPosition = FormStartPosition.CenterScreen;
                MinimumSize = new Size(550, 180);

                // 標題
                var lblTitle = new Label
                {
                    Text = "請確認舊軸名稱對應新軸名稱",
                    Dock = DockStyle.Top,
                    Font = new Font("Microsoft JhengHei UI", 16, FontStyle.Bold),
                    Height = 44,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.DarkSlateGray
                };
                Controls.Add(lblTitle);

                // 主內容 Panel
                var mainPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    RowCount = this.oldAxes.Count,
                    ColumnCount = 3,
                    AutoSize = true,
                    Padding = new Padding(32, 8, 32, 8)
                };
                mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
                mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36));
                mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));

                for (int i = 0; i < this.oldAxes.Count; i++)
                {
                    // 左欄：原軸名
                    var oldLabel = new Label
                    {
                        Text = this.oldAxes[i],
                        AutoSize = false,
                        Width = 180,
                        Height = 36,
                        TextAlign = ContentAlignment.MiddleRight,
                        Font = new Font("Microsoft JhengHei UI", 13, FontStyle.Bold),
                        Padding = new Padding(0, 7, 8, 0),
                        BackColor = Color.Transparent
                    };
                    mainPanel.Controls.Add(oldLabel, 0, i);

                    // 箭頭
                    var arrow = new Label
                    {
                        Text = "→",
                        Font = new Font("Segoe UI", 15, FontStyle.Bold),
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Width = 36,
                        Height = 36
                    };
                    mainPanel.Controls.Add(arrow, 1, i);

                    // ComboBox
                    var cb = new ComboBox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Width = 220,
                        Font = new Font("Microsoft JhengHei UI", 13),
                        DropDownWidth = 260,
                        Height = 36
                    };
                    cb.Items.AddRange(this.newAxes.ToArray());
                    if (i < this.newAxes.Count)
                        cb.SelectedIndex = i;

                    // Tooltip 顯示完整軸名
                    ToolTip tip = new ToolTip();
                    cb.SelectedIndexChanged += (s, e) =>
                    {
                        if (cb.SelectedIndex >= 0)
                            tip.SetToolTip(cb, cb.Items[cb.SelectedIndex].ToString());
                    };

                    mainPanel.Controls.Add(cb, 2, i);

                    comboBoxes[this.oldAxes[i]] = cb;
                }

                // 分隔線
                var sep = new Label
                {
                    BorderStyle = BorderStyle.Fixed3D,
                    Height = 2,
                    Dock = DockStyle.Top,
                    Margin = new Padding(24, 6, 24, 6)
                };

                // 按鈕區
                btnOK = new Button { Text = "確定", Width = 88, Height = 36, DialogResult = DialogResult.OK };
                btnCancel = new Button { Text = "取消", Width = 88, Height = 36, DialogResult = DialogResult.Cancel };
                btnOK.Font = btnCancel.Font = new Font("Microsoft JhengHei UI", 13);
                btnOK.BackColor = Color.SteelBlue;
                btnOK.ForeColor = Color.White;
                btnCancel.BackColor = Color.Gainsboro;
                btnCancel.ForeColor = Color.Black;

                btnOK.Click += BtnOK_Click;

                var btnPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.RightToLeft,
                    Dock = DockStyle.Bottom,
                    Height = 44,
                    Padding = new Padding(0, 10, 24, 12),
                    BackColor = Color.Transparent
                };
                btnPanel.Controls.Add(btnOK);
                btnPanel.Controls.Add(btnCancel);

                // 排版
                Controls.Add(btnPanel);
                Controls.Add(sep);
                Controls.Add(mainPanel);
                Controls.SetChildIndex(lblTitle, 0); // 標題在最上

                // 自動調整視窗高度
                this.Height = lblTitle.Height + sep.Height + mainPanel.Height + btnPanel.Height + 48;
            }

            private void BtnOK_Click(object sender, EventArgs e)
            {
                var selected = new HashSet<string>();
                AxisMapping = new Dictionary<string, string>();
                foreach (var oldAxis in oldAxes)
                {
                    var cb = comboBoxes[oldAxis];
                    var sel = cb.SelectedItem as string;
                    if (string.IsNullOrEmpty(sel))
                    {
                        MessageBox.Show($"請選擇對應的新軸名給原軸：{oldAxis}");
                        DialogResult = DialogResult.None;
                        return;
                    }
                    if (selected.Contains(sel))
                    {
                        MessageBox.Show($"新軸名「{sel}」不可重複指派！");
                        DialogResult = DialogResult.None;
                        return;
                    }
                    AxisMapping[oldAxis] = sel;
                    selected.Add(sel);
                }
                DialogResult = DialogResult.OK;
            }
        }


        private void LvSegments_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var item = lvSegments.HitTest(e.Location).Item;
                if (item != null)
                {
                    lvSegments.SelectedItems.Clear();
                    item.Selected = true;
                    contextMenu.Show(lvSegments, e.Location);
                }
            }
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            if (lvSegments.SelectedItems.Count == 0) return;

            var item = lvSegments.SelectedItems[0];
            var seg = _parent.Path.Segments.FirstOrDefault(s => s.SegmentName == item.Text);
            if (seg == null) return;


            var detailsForm = new DetailsForm(seg);
            detailsForm.ShowDialog(); // 使用 ShowDialog 以模態形式顯示表單

            // 依據 UI 內容更新各軸 Motion
            foreach (var kv in _deviceInstances)
            {
                if (kv.Value is MotionBase)
                {
                    if (_deviceMoveParam.ContainsKey(kv.Key))
                    {
                        MotorMotion motor = (MotorMotion)seg.Motions[kv.Key];
                        motor.Position = motor.Program_Relvalue + _parent.Path.ProgramOrigin[kv.Key];
                        seg.Remove(kv.Key);
                        seg.Add(kv.Key, motor);
                    }
                }
                else if (kv.Value is IOBase)
                {
                    if (_deviceMoveParam.ContainsKey(kv.Key))
                    {
                        var i = new IOMotion();
                        seg.Remove(kv.Key);
                        seg.Add(kv.Key, i.Clone());
                    }
                }
            }
            // 重新整理 ListView
            PopulateSegmentsList();
        }

        private void InitializeTabs()
        {
            string eachOrg = string.Empty;
            foreach (var axis in _parent.SelectedDevices)
            {
                var device = _deviceInstances.ContainsKey(axis) ? _deviceInstances[axis] : null;
                bool isIO = device is IOBase;
                //Motion initialMotion = isIO ? new IOMotion() : (Motion)new MotorMotion();

                var ctrlPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Width = 200,Height = 100 };

                if (isIO)
                {
                    var ioparm = new IOParamPanel(axis, (IOBase)device);
                    FlowPanelMoveParam.Controls.Add(ioparm);
                    _deviceMoveParam.Add(axis, ioparm);
                }
                else
                {
                    var mparam = new MotionParamPanel(axis, (MotionBase)device);
                    FlowPanelMoveParam.Controls.Add(mparam);
                    _deviceMoveParam.Add(axis, mparam);

                    eachOrg += _parent.Path.ProgramOrigin[axis] + ","; // Add comma after each position


                }
            }

            eachOrg = eachOrg.TrimEnd(',');

            btnProgramOrg.Text = $"原點設定\n({eachOrg})";

        }
        private void UpdateRtoASegmentsList()
        {
            lvSegments.Items.Clear();
            for (int i = 0; i < _parent.Path.Segments.Count; i++)
            {
                var seg = _parent.Path.Segments[i];
                // 取得各軸 Position
                var positions = _parent.SelectedDevices
                    .Select(axis =>
                    {
                        if (seg.Motions.TryGetValue(axis, out var motion) && motion is MotorMotion mm)
                        {
                            mm.Position = mm.Program_Relvalue + _parent.Path.ProgramOrigin[axis];
                            return mm.Position.ToString("0.###");
                        }

                        else if (seg.Motions.TryGetValue(axis, out var IO) && IO is IOMotion I)
                            return I.OnOff ? $"ON({I.Channel})" : $"OFF({I.Channel})";

                        return "-";
                    });
                var OriginDiffs = _parent.SelectedDevices
                .Select(axis =>
                {
                    if (seg.Motions.TryGetValue(axis, out var motion) && motion is MotorMotion mm)
                        return mm.Program_Relvalue.ToString("0.###");
                    else if (seg.Motions.TryGetValue(axis, out var IO) && IO is IOMotion I)
                        return I.OnOff ? $"ON({I.Channel})" : $"OFF({I.Channel})";

                    return "-";
                });
                var vel = _parent.SelectedDevices
                .Select(axis =>
                {
                    if (seg.Motions.TryGetValue(axis, out var motion) && motion is MotorMotion mm)
                    {
                        return mm.MaxVel.ToString("0.###");
                    }

                    return "-";
                });

                string posStr = $"({string.Join(",", positions)})";
                string posrelStr = $"({string.Join(",", OriginDiffs)})";
                string VelrelStr = $"({string.Join(",", vel)})";
                var item = new ListViewItem(new[] { seg.SegmentName, seg.MoveType.ToString(), posStr, posrelStr, VelrelStr}) { Tag = i };
                lvSegments.Items.Add(item);
            }
        }
        private void PopulateSegmentsList()
        {
            lvSegments.Items.Clear();
            for (int i = 0; i < _parent.Path.Segments.Count; i++)
            {
                var seg = _parent.Path.Segments[i];
                // 取得各軸 Position
                var positions = _parent.SelectedDevices
                    .Select(axis =>
                    {
                        if (seg.Motions.TryGetValue(axis, out var motion) && motion is MotorMotion mm)
                            return mm.Position.ToString("0.###");
                        else if (seg.Motions.TryGetValue(axis, out var IO) && IO is IOMotion I)
                            return I.OnOff ? $"ON({I.Channel})" : $"OFF({I.Channel})";

                            return "-";
                    });
                var OriginDiffs = _parent.SelectedDevices
                .Select(axis =>
                {
                    if (seg.Motions.TryGetValue(axis, out var motion) && motion is MotorMotion mm)
                        return mm.Program_Relvalue.ToString("0.###");
                    else if (seg.Motions.TryGetValue(axis, out var IO) && IO is IOMotion I)
                        return I.OnOff ? $"ON({I.Channel})" : $"OFF({I.Channel})";

                    return "-";
                });
                var vel = _parent.SelectedDevices
                .Select(axis =>
                {
                    if (seg.Motions.TryGetValue(axis, out var motion) && motion is MotorMotion mm)
                    {
                        return mm.MaxVel.ToString("0.###");
                    }

                    return "-";
                });

                string posStr = $"({string.Join(",", positions)})";
                string posrelStr = $"({string.Join(",", OriginDiffs)})";
                string VelrelStr = $"({string.Join(",", vel)})";
                var item = new ListViewItem(new[] { seg.SegmentName, seg.MoveType.ToString(), posStr, posrelStr, VelrelStr }) { Tag = i };
                lvSegments.Items.Add(item);
            }
        }

        private void LvSegments_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvSegments.SelectedItems.Count == 0) return;
            var name = lvSegments.SelectedItems[0].Text;
            var seg = _parent.Path.Segments.FirstOrDefault(s => s.SegmentName == name);
            if (seg == null) return;
            foreach (var axis in _parent.SelectedDevices)
            {
                if (seg.Motions.TryGetValue(axis, out var motion))
                {
                    if (motion is MotorMotion)
                    {
                        if (_deviceMoveParam.ContainsKey(axis))
                            ((MotionParamPanel)_deviceMoveParam[axis]).UpdateTextBoxFromMotorMotion((MotorMotion)motion);

                    }
                    else if (motion is IOMotion)
                    {
                        if (_deviceMoveParam.ContainsKey(axis))
                            ((IOParamPanel)_deviceMoveParam[axis]).UpdateTextBoxFromMotorMotion((IOMotion)motion);
                    }
                }
            }
        }
        // 事件處理函式：
        private async void LvSegments_DoubleClick(object sender, EventArgs e)
        {
            if (lvSegments.SelectedItems.Count == 0) return;

            var item = lvSegments.SelectedItems[0];
            string name = item.Text;
            // 找到對應的段落
            var seg = _parent.Path.Segments.FirstOrDefault(s => s.SegmentName == name);
            if (seg == null) return;

            // 跳出確認視窗
            var dr = MessageBox.Show(
                $"您確定要執行段落「{name}」嗎？",
                "執行確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (dr == DialogResult.Yes)
            {
                await ExecuteSegmentAsync(seg);
            }
        }
        private async Task ExecuteSegmentAsync(MotionSegment seg)
        {
            try
            {
                if (seg.MoveType == MoveType.Independent)
                {
                    var tasks = new List<Task>();

                    foreach (var kv in seg.Motions)
                    {
                        string axis = kv.Key;
                        Motion m = kv.Value;

                        if (_deviceInstances.TryGetValue(axis, out var dev) && m is MotorMotion mm && dev is MotionBase mot)
                        {
                            this.BeginInvoke((Action)(() =>
                            {
                                ((MotionParamPanel)_deviceMoveParam[axis]).UpdateTextBoxFromMotorMotion((MotorMotion)mm);
                                ((MotionParamPanel)_deviceMoveParam[axis]).UpdateLabelColor(Color.GreenYellow);
                            }));

                            var t = Task.Run(async () =>
                            {
                                if (!mot.Absolute_Move(mm.Position, mm.StartSpeed, mm.MaxVel * percentageValue, mm.Acceleration, mm.Deceleration))
                                {
                                    this.BeginInvoke((Action)(() =>
                                    {
                                        ((MotionParamPanel)_deviceMoveParam[axis]).UpdateErrMessage(mot.GetErrorMessage());
                                        ((MotionParamPanel)_deviceMoveParam[axis]).UpdateLabelColor(Color.Red);
                                    }));
                                    return;
                                }
                                int status = 1;
                                double pos = 0;

                                var sw = System.Diagnostics.Stopwatch.StartNew(); // 開始計時
                                int timeoutMs = 30000; // 例如 30 秒

                                while (true)
                                {
                                    mot.GetMotionStatus(ref status);
                                    mot.GetCurrentPos(ref pos);
                                    this.BeginInvoke((Action)(() =>
                                    {
                                        ((MotionParamPanel)_deviceMoveParam[axis]).UpdatePosition(pos);
                                        
                                        //顯示原點相對移動量((MotionParamPanel)_deviceMoveParam[axis]).UpdateRelPosition(pos- _parent.Path.ProgramOrigin[axis]);
                                        ((MotionParamPanel)_deviceMoveParam[axis]).UpdateRelPosition(mm.Position - pos);

                                    }));
                                    if (status == 0)
                                    {
                                        this.BeginInvoke((Action)(() =>
                                        {
                                            ((MotionParamPanel)_deviceMoveParam[axis]).UpdateLabelColor(SystemColors.Control);
                                        }));
                                        break;
                                    }
                                    if(status == -99 || sw.ElapsedMilliseconds > timeoutMs)
                                    {
                                        mot.EmgStop();
                                        this.BeginInvoke((Action)(() =>
                                        {
                                            ((MotionParamPanel)_deviceMoveParam[axis]).UpdateErrMessage("Timeout");
                                            ((MotionParamPanel)_deviceMoveParam[axis]).UpdateLabelColor(Color.Red);
                                        }));
                                        break;
                                    }

                                    await Task.Delay(10);
                                }
                            });
                            tasks.Add(t);
                        }
                        else if (_deviceInstances.TryGetValue(axis, out var IOdev) && m is IOMotion ioParam && dev is IOBase IOControl)
                        {
                            this.BeginInvoke((Action)(() =>
                            {
                                ((IOParamPanel)_deviceMoveParam[axis]).UpdateTextBoxFromMotorMotion(ioParam);
                                ((IOParamPanel)_deviceMoveParam[axis]).UpdateLabelColor(Color.GreenYellow);
                            }));

                            IOControl.SETIO(0, ioParam.Channel, ioParam.OnOff);

                            this.BeginInvoke((Action)(() =>
                            {
                                ((IOParamPanel)_deviceMoveParam[axis]).UpdateLabelColor(SystemColors.Control);
                            }));
                        }
                    }
                    await Task.WhenAll(tasks);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"執行段落「{seg.SegmentName}」時發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (lvSegments.SelectedItems.Count == 0)
            {
                MessageBox.Show("請先選擇要更新的段落");
                return;
            }
            var idx = lvSegments.SelectedItems[0].Index;
            if (idx < 0 || idx >= _parent.Path.Segments.Count) return;

            var seg = _parent.Path.Segments[idx];
            //// 更新 MoveType 與名稱（如有需要）
            //if (comboMoveType.SelectedItem != null)
            //    seg.MoveType = (MoveType)comboMoveType.SelectedItem;
            //seg.SegmentName = txtSegmentName.Text.Trim();

            // 依據 UI 內容更新各軸 Motion
            foreach (var kv in _deviceInstances)
            {
                if (kv.Value is MotionBase)
                {
                    if (_deviceMoveParam.ContainsKey(kv.Key) )
                    {
                        var motor = new MotorMotion();
                        ((MotionParamPanel)_deviceMoveParam[kv.Key]).UpdateMotorMotionFromTextBox((MotorMotion)motor);
                        motor.Program_Relvalue = motor.Position - _parent.Path.ProgramOrigin[kv.Key];
                        seg.Remove(kv.Key);
                        seg.Add(kv.Key, motor);
                    }
                }
                else if(kv.Value is IOBase)
                {
                    if (_deviceMoveParam.ContainsKey(kv.Key))
                    {
                        var i = new IOMotion();
                        ((IOParamPanel)_deviceMoveParam[kv.Key]).UpdateMotorMotionFromTextBox((IOMotion)i);
                        seg.Remove(kv.Key);
                        seg.Add(kv.Key, i.Clone());
                    }
                }
            }
            // 重新整理 ListView
            PopulateSegmentsList();

            // 重新選取剛剛更新的段落
            if (lvSegments.Items.Count > idx)
            {
                lvSegments.Items[idx].Selected = true;
                lvSegments.Select(); // 讓 ListView 取得焦點
                                     // 手動觸發選取事件，確保 TextBox 更新
                LvSegments_SelectedIndexChanged(lvSegments, EventArgs.Empty);
            }
        }
        private void BtnOrgin_Click(object sender, EventArgs e)
        {

            DialogResult result = MessageBox.Show("重設原點將更新所有段落機械位置?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            string eachOrg = string.Empty;
            foreach (var kv in _deviceInstances)
            {
                
                if (kv.Value is MotionBase)
                {
                    var m = new MotorMotion();
                    ((MotionParamPanel)_deviceMoveParam[kv.Key]).UpdateMotorMotionFromTextBox(m);
                    _parent.Path.ProgramOrigin[kv.Key] = m.Position;
                    eachOrg += m.Position.ToString() + ","; // Add comma after each position
                    //seg.Add(kv.Key, m.Clone());
                }
            }
            eachOrg = eachOrg.TrimEnd(',');
            this.BeginInvoke((Action)(() =>
            {
                btnProgramOrg.Text = $"原點設定\n({eachOrg})";
            }));
            UpdateRtoASegmentsList();

        }
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var name = txtSegmentName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { MessageBox.Show("請輸入名稱"); return; }
            if (comboMoveType.SelectedItem == null) { MessageBox.Show("請選擇移動類型"); return; }
            var newIndex = lvSegments.SelectedItems.Count > 0 ? lvSegments.SelectedItems[0].Index + 1 : _parent.Path.Segments.Count;
            var seg = new MotionSegment { SegmentName = name, MoveType = (MoveType)comboMoveType.SelectedItem };
            
            foreach (var kv in _deviceInstances)
            {
                if (kv.Value is MotionBase)
                {
                    var m = new MotorMotion();
                    ((MotionParamPanel)_deviceMoveParam[kv.Key]).UpdateMotorMotionFromTextBox(m);
                    m.Program_Relvalue = m.Position - _parent.Path.ProgramOrigin[kv.Key];
                    seg.Add(kv.Key, m);
                }
                else if (kv.Value is IOBase)
                {
                    var i = new IOMotion();
                    ((IOParamPanel)_deviceMoveParam[kv.Key]).UpdateMotorMotionFromTextBox(i);
                    seg.Add(kv.Key, i.Clone());
                }
            }

            _parent.Path.AddSegmentAt(newIndex, seg);
            PopulateSegmentsList();
            txtSegmentName.Clear(); 
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lvSegments.SelectedItems.Count == 0) return;
            var name = lvSegments.SelectedItems[0].Text;
            _parent.Path.RemoveSegment(name);
            PopulateSegmentsList();
            txtSegmentName.Clear(); comboMoveType.SelectedIndex = -1;
        }

        private void MoveSelected(int offset)
        {
            if (lvSegments.SelectedItems.Count == 0) return;
            var idx = lvSegments.SelectedItems[0].Index;
            var newIndex = idx + offset;
            if (newIndex < 0 || newIndex >= _parent.Path.Segments.Count) return;
            _parent.Path.MoveSegment(idx, newIndex);
            PopulateSegmentsList();
            lvSegments.Items[newIndex].Selected = true;
        }

    }


    public class DetailsForm : Form
    {
        private FlowLayoutPanel flowPanel;

        public DetailsForm(MotionSegment segment)
        {
            InitializeComponent();
            PopulatePropertyGrids(segment);
        }

        private void InitializeComponent()
        {
            flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false
            };
            Controls.Add(flowPanel);
            Text = "Segment Details";
            ClientSize = new System.Drawing.Size(800, 400);
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void PopulatePropertyGrids(MotionSegment segment)
        {
            foreach (var kv in segment.Motions)
            {
                var axis = kv.Key;
                var motion = kv.Value;

                // Create a panel to hold the label and property grid
                var panel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    AutoSize = true,
                    Width = 400
                };

                // Create a label for the axis
                var label = new Label { Text = $"Axis: {axis}", AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

                // Create a PropertyGrid for the motion
                var propertyGrid = new PropertyGrid { SelectedObject = motion, Width = 250, Height = 300 };

                // Add the label and PropertyGrid to the panel
                panel.Controls.Add(label);
                panel.Controls.Add(propertyGrid);

                // Add the panel to the main FlowLayoutPanel
                flowPanel.Controls.Add(panel);
            }
        }
    }





    public class NumericUpDownProperty : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (editorService != null)
            {
                NumericUpDown numericUpDown = new NumericUpDown
                {
                    Minimum = -1000,
                    Maximum = 1000,
                    Value = Convert.ToDecimal(value),
                    DecimalPlaces = 2 // 設置小數位數
                };

                editorService.DropDownControl(numericUpDown);
                value = Convert.ToDouble(numericUpDown.Value); // 確保返回值為 double 類型


                var motionSegment = context.Instance as MotorMotion;
                if (motionSegment != null)
                {
                    if (context.PropertyDescriptor.Name == nameof(MotorMotion.Position))
                    {
                        motionSegment.Position = Convert.ToDouble(numericUpDown.Value);
                    }
                    else if (context.PropertyDescriptor.Name == nameof(MotorMotion.Program_Relvalue))
                    {
                        motionSegment.Program_Relvalue = Convert.ToDouble(numericUpDown.Value);
                    }
                }

            }
            return value;
        }
    }
    public class SelectMotorDevices : System.Drawing.Design.UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var Motionteach = context.Instance as MotionTeach;

            using (var dlg = new DeviceSelectionForm(Motionteach.SelectedDevices))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    return dlg.GetSelectedKeys();
                }
            }

            return Motionteach.SelectedDevices; // 如果用戶取消選擇，返回原始值
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }

    public class AxisMappingForm : Form
    {
        private readonly List<string> oldAxes;
        private readonly List<string> newAxes;
        private readonly Dictionary<string, ComboBox> comboBoxes = new Dictionary<string, ComboBox>();
        private readonly Button btnOK;
        private readonly Button btnCancel;

        public Dictionary<string, string> AxisMapping { get; private set; }

        public AxisMappingForm(IEnumerable<string> oldAxes, IEnumerable<string> newAxes)
        {
            this.oldAxes = oldAxes.ToList();
            this.newAxes = newAxes.ToList();

            Text = "軸名稱對應設定";
            Width = 400;
            Height = 100 + 40 * this.oldAxes.Count;
            StartPosition = FormStartPosition.CenterScreen;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = this.oldAxes.Count + 1,
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(10)
            };

            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            for (int i = 0; i < this.oldAxes.Count; i++)
            {
                var oldLabel = new Label { Text = this.oldAxes[i], AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
                panel.Controls.Add(oldLabel, 0, i);

                panel.Controls.Add(new Label { Text = "→", AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleCenter }, 1, i);

                var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
                cb.Items.AddRange(this.newAxes.ToArray());
                if (i < this.newAxes.Count)
                    cb.SelectedIndex = i; // 預設順序對應
                panel.Controls.Add(cb, 2, i);

                comboBoxes[this.oldAxes[i]] = cb;
            }

            btnOK = new Button { Text = "確定", Width = 80, Dock = DockStyle.Bottom };
            btnCancel = new Button { Text = "取消", Width = 80, Dock = DockStyle.Bottom };

            btnOK.Click += BtnOK_Click;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            var btnPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, Height = 40 };
            btnPanel.Controls.Add(btnOK);
            btnPanel.Controls.Add(btnCancel);

            Controls.Add(panel);
            Controls.Add(btnPanel);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            var selected = new HashSet<string>();
            AxisMapping = new Dictionary<string, string>();
            foreach (var oldAxis in oldAxes)
            {
                var cb = comboBoxes[oldAxis];
                var sel = cb.SelectedItem as string;
                if (string.IsNullOrEmpty(sel))
                {
                    MessageBox.Show($"請選擇對應的「新軸名」給原軸：{oldAxis}");
                    return;
                }
                if (selected.Contains(sel))
                {
                    MessageBox.Show($"新軸名「{sel}」不可重複指派！");
                    return;
                }
                AxisMapping[oldAxis] = sel;
                selected.Add(sel);
            }
            DialogResult = DialogResult.OK;
        }
    }
}