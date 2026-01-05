
namespace AutoTestSystem
{
    partial class LeadShineMotionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.MotionControl = new System.Windows.Forms.GroupBox();
            this.AddList = new System.Windows.Forms.Button();
            this.Trigger = new System.Windows.Forms.Button();
            this.Daclabel = new System.Windows.Forms.Label();
            this.Tacclabel = new System.Windows.Forms.Label();
            this.dac = new System.Windows.Forms.TextBox();
            this.tacc = new System.Windows.Forms.TextBox();
            this.Velocitylabel = new System.Windows.Forms.Label();
            this.Postionlabel = new System.Windows.Forms.Label();
            this.max_vel = new System.Windows.Forms.TextBox();
            this.postion = new System.Windows.Forms.TextBox();
            this.Init = new System.Windows.Forms.Button();
            this.UnInit = new System.Windows.Forms.Button();
            this.JOGRun = new System.Windows.Forms.GroupBox();
            this.PlusRotateSet = new System.Windows.Forms.Button();
            this.PRSetlabel = new System.Windows.Forms.Label();
            this.PlusRotateSetlabel = new System.Windows.Forms.Label();
            this.PlusValue = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Cycleslabel = new System.Windows.Forms.Label();
            this.IntervalTimelabel = new System.Windows.Forms.Label();
            this.CyclesTimes = new System.Windows.Forms.TextBox();
            this.IntervalTime = new System.Windows.Forms.TextBox();
            this.Negative = new System.Windows.Forms.Button();
            this.rpmlabel = new System.Windows.Forms.Label();
            this.RpmTimelabel = new System.Windows.Forms.Label();
            this.PointVelocitylabel = new System.Windows.Forms.Label();
            this.Position = new System.Windows.Forms.Button();
            this.PointVelocity = new System.Windows.Forms.TextBox();
            this.TimeTacclabel = new System.Windows.Forms.Label();
            this.TimeTacc = new System.Windows.Forms.TextBox();
            this.CurrentPosition = new System.Windows.Forms.TextBox();
            this.CurPoslabel = new System.Windows.Forms.Label();
            this.CurrentPos = new System.Windows.Forms.GroupBox();
            this.UpdatePosition = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LeadShinePropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.MotionList = new System.Windows.Forms.DataGridView();
            this.Path = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DeviceID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MovePosition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MoveVelocity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MoveTacc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MoveDac = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ResetHome = new System.Windows.Forms.Button();
            this.ResetDir = new System.Windows.Forms.ComboBox();
            this.ResetControl = new System.Windows.Forms.GroupBox();
            this.ResetDac = new System.Windows.Forms.TextBox();
            this.HighSpeed = new System.Windows.Forms.TextBox();
            this.ResetDaclabel = new System.Windows.Forms.Label();
            this.ResetTacclabel = new System.Windows.Forms.Label();
            this.ResetMode = new System.Windows.Forms.ComboBox();
            this.ResetModelabel = new System.Windows.Forms.Label();
            this.LowSpeed = new System.Windows.Forms.TextBox();
            this.LowSpeedlabel = new System.Windows.Forms.Label();
            this.StopPosition = new System.Windows.Forms.TextBox();
            this.ResetTacc = new System.Windows.Forms.TextBox();
            this.ZeroPoint = new System.Windows.Forms.TextBox();
            this.HighSpeedlabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ZeroPointlabel = new System.Windows.Forms.Label();
            this.SpecifyLocation = new System.Windows.Forms.ComboBox();
            this.SpecifyLocationLabel = new System.Windows.Forms.Label();
            this.Directionlabel = new System.Windows.Forms.Label();
            this.MotionControl.SuspendLayout();
            this.JOGRun.SuspendLayout();
            this.CurrentPos.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MotionList)).BeginInit();
            this.ResetControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // MotionControl
            // 
            this.MotionControl.Controls.Add(this.AddList);
            this.MotionControl.Controls.Add(this.Trigger);
            this.MotionControl.Controls.Add(this.Daclabel);
            this.MotionControl.Controls.Add(this.Tacclabel);
            this.MotionControl.Controls.Add(this.dac);
            this.MotionControl.Controls.Add(this.tacc);
            this.MotionControl.Controls.Add(this.Velocitylabel);
            this.MotionControl.Controls.Add(this.Postionlabel);
            this.MotionControl.Controls.Add(this.max_vel);
            this.MotionControl.Controls.Add(this.postion);
            this.MotionControl.Location = new System.Drawing.Point(12, 30);
            this.MotionControl.Name = "MotionControl";
            this.MotionControl.Size = new System.Drawing.Size(784, 140);
            this.MotionControl.TabIndex = 0;
            this.MotionControl.TabStop = false;
            this.MotionControl.Text = "運動控制";
            // 
            // AddList
            // 
            this.AddList.Location = new System.Drawing.Point(678, 84);
            this.AddList.Name = "AddList";
            this.AddList.Size = new System.Drawing.Size(75, 33);
            this.AddList.TabIndex = 13;
            this.AddList.Text = "加入表單";
            this.AddList.UseVisualStyleBackColor = true;
            this.AddList.Click += new System.EventHandler(this.AddList_Click);
            // 
            // Trigger
            // 
            this.Trigger.Location = new System.Drawing.Point(678, 36);
            this.Trigger.Name = "Trigger";
            this.Trigger.Size = new System.Drawing.Size(75, 33);
            this.Trigger.TabIndex = 12;
            this.Trigger.Text = "啟動";
            this.Trigger.UseVisualStyleBackColor = true;
            this.Trigger.Click += new System.EventHandler(this.Trigger_Click);
            // 
            // Daclabel
            // 
            this.Daclabel.AutoSize = true;
            this.Daclabel.Location = new System.Drawing.Point(493, 66);
            this.Daclabel.Name = "Daclabel";
            this.Daclabel.Size = new System.Drawing.Size(82, 15);
            this.Daclabel.TabIndex = 7;
            this.Daclabel.Text = "減速度(ms):";
            // 
            // Tacclabel
            // 
            this.Tacclabel.AutoSize = true;
            this.Tacclabel.Location = new System.Drawing.Point(322, 66);
            this.Tacclabel.Name = "Tacclabel";
            this.Tacclabel.Size = new System.Drawing.Size(82, 15);
            this.Tacclabel.TabIndex = 6;
            this.Tacclabel.Text = "加速度(ms):";
            // 
            // dac
            // 
            this.dac.Location = new System.Drawing.Point(581, 63);
            this.dac.Name = "dac";
            this.dac.Size = new System.Drawing.Size(72, 25);
            this.dac.TabIndex = 5;
            this.dac.Text = "50";
            // 
            // tacc
            // 
            this.tacc.Location = new System.Drawing.Point(404, 63);
            this.tacc.Name = "tacc";
            this.tacc.Size = new System.Drawing.Size(72, 25);
            this.tacc.TabIndex = 4;
            this.tacc.Text = "50";
            // 
            // Velocitylabel
            // 
            this.Velocitylabel.AutoSize = true;
            this.Velocitylabel.Location = new System.Drawing.Point(164, 66);
            this.Velocitylabel.Name = "Velocitylabel";
            this.Velocitylabel.Size = new System.Drawing.Size(74, 15);
            this.Velocitylabel.TabIndex = 3;
            this.Velocitylabel.Text = "速度(rpm):";
            // 
            // Postionlabel
            // 
            this.Postionlabel.AutoSize = true;
            this.Postionlabel.Location = new System.Drawing.Point(20, 66);
            this.Postionlabel.Name = "Postionlabel";
            this.Postionlabel.Size = new System.Drawing.Size(59, 15);
            this.Postionlabel.TabIndex = 2;
            this.Postionlabel.Text = "位置(P):";
            // 
            // max_vel
            // 
            this.max_vel.Location = new System.Drawing.Point(244, 63);
            this.max_vel.Name = "max_vel";
            this.max_vel.Size = new System.Drawing.Size(72, 25);
            this.max_vel.TabIndex = 1;
            this.max_vel.Text = "100";
            // 
            // postion
            // 
            this.postion.Location = new System.Drawing.Point(86, 63);
            this.postion.Name = "postion";
            this.postion.Size = new System.Drawing.Size(72, 25);
            this.postion.TabIndex = 0;
            this.postion.Text = "200";
            // 
            // Init
            // 
            this.Init.Location = new System.Drawing.Point(656, 330);
            this.Init.Name = "Init";
            this.Init.Size = new System.Drawing.Size(140, 56);
            this.Init.TabIndex = 18;
            this.Init.Text = "Init";
            this.Init.UseVisualStyleBackColor = true;
            this.Init.Click += new System.EventHandler(this.Init_Click);
            // 
            // UnInit
            // 
            this.UnInit.Location = new System.Drawing.Point(656, 410);
            this.UnInit.Name = "UnInit";
            this.UnInit.Size = new System.Drawing.Size(140, 51);
            this.UnInit.TabIndex = 17;
            this.UnInit.Text = "UnInit";
            this.UnInit.UseVisualStyleBackColor = true;
            this.UnInit.Click += new System.EventHandler(this.UnInit_Click);
            // 
            // JOGRun
            // 
            this.JOGRun.Controls.Add(this.PlusRotateSet);
            this.JOGRun.Controls.Add(this.PRSetlabel);
            this.JOGRun.Controls.Add(this.PlusRotateSetlabel);
            this.JOGRun.Controls.Add(this.PlusValue);
            this.JOGRun.Controls.Add(this.label3);
            this.JOGRun.Controls.Add(this.Cycleslabel);
            this.JOGRun.Controls.Add(this.IntervalTimelabel);
            this.JOGRun.Controls.Add(this.CyclesTimes);
            this.JOGRun.Controls.Add(this.IntervalTime);
            this.JOGRun.Controls.Add(this.Negative);
            this.JOGRun.Controls.Add(this.rpmlabel);
            this.JOGRun.Controls.Add(this.RpmTimelabel);
            this.JOGRun.Controls.Add(this.PointVelocitylabel);
            this.JOGRun.Controls.Add(this.Position);
            this.JOGRun.Controls.Add(this.PointVelocity);
            this.JOGRun.Controls.Add(this.TimeTacclabel);
            this.JOGRun.Controls.Add(this.TimeTacc);
            this.JOGRun.Location = new System.Drawing.Point(12, 308);
            this.JOGRun.Name = "JOGRun";
            this.JOGRun.Size = new System.Drawing.Size(544, 162);
            this.JOGRun.TabIndex = 20;
            this.JOGRun.TabStop = false;
            this.JOGRun.Text = "運行";
            // 
            // PlusRotateSet
            // 
            this.PlusRotateSet.Location = new System.Drawing.Point(343, 129);
            this.PlusRotateSet.Name = "PlusRotateSet";
            this.PlusRotateSet.Size = new System.Drawing.Size(56, 27);
            this.PlusRotateSet.TabIndex = 24;
            this.PlusRotateSet.Text = "設定";
            this.PlusRotateSet.UseVisualStyleBackColor = true;
            this.PlusRotateSet.Click += new System.EventHandler(this.PlusRotateSet_Click);
            // 
            // PRSetlabel
            // 
            this.PRSetlabel.AutoSize = true;
            this.PRSetlabel.Location = new System.Drawing.Point(309, 138);
            this.PRSetlabel.Name = "PRSetlabel";
            this.PRSetlabel.Size = new System.Drawing.Size(28, 15);
            this.PRSetlabel.TabIndex = 29;
            this.PRSetlabel.Text = "P/R";
            // 
            // PlusRotateSetlabel
            // 
            this.PlusRotateSetlabel.AutoSize = true;
            this.PlusRotateSetlabel.Location = new System.Drawing.Point(161, 138);
            this.PlusRotateSetlabel.Name = "PlusRotateSetlabel";
            this.PlusRotateSetlabel.Size = new System.Drawing.Size(86, 15);
            this.PlusRotateSetlabel.TabIndex = 28;
            this.PlusRotateSetlabel.Text = "脈衝數設定:";
            // 
            // PlusValue
            // 
            this.PlusValue.Location = new System.Drawing.Point(253, 131);
            this.PlusValue.Name = "PlusValue";
            this.PlusValue.Size = new System.Drawing.Size(50, 25);
            this.PlusValue.TabIndex = 27;
            this.PlusValue.Text = "10000";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(493, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 15);
            this.label3.TabIndex = 26;
            this.label3.Text = "ms";
            // 
            // Cycleslabel
            // 
            this.Cycleslabel.AutoSize = true;
            this.Cycleslabel.Location = new System.Drawing.Point(329, 78);
            this.Cycleslabel.Name = "Cycleslabel";
            this.Cycleslabel.Size = new System.Drawing.Size(71, 15);
            this.Cycleslabel.TabIndex = 25;
            this.Cycleslabel.Text = "循環次數:";
            // 
            // IntervalTimelabel
            // 
            this.IntervalTimelabel.AutoSize = true;
            this.IntervalTimelabel.Location = new System.Drawing.Point(299, 36);
            this.IntervalTimelabel.Name = "IntervalTimelabel";
            this.IntervalTimelabel.Size = new System.Drawing.Size(101, 15);
            this.IntervalTimelabel.TabIndex = 24;
            this.IntervalTimelabel.Text = "等待間隔時間:";
            // 
            // CyclesTimes
            // 
            this.CyclesTimes.Location = new System.Drawing.Point(406, 75);
            this.CyclesTimes.Name = "CyclesTimes";
            this.CyclesTimes.Size = new System.Drawing.Size(85, 25);
            this.CyclesTimes.TabIndex = 23;
            this.CyclesTimes.Text = "1";
            // 
            // IntervalTime
            // 
            this.IntervalTime.Location = new System.Drawing.Point(406, 33);
            this.IntervalTime.Name = "IntervalTime";
            this.IntervalTime.Size = new System.Drawing.Size(85, 25);
            this.IntervalTime.TabIndex = 22;
            this.IntervalTime.Text = "10";
            // 
            // Negative
            // 
            this.Negative.Location = new System.Drawing.Point(418, 106);
            this.Negative.Name = "Negative";
            this.Negative.Size = new System.Drawing.Size(116, 54);
            this.Negative.TabIndex = 11;
            this.Negative.Text = "反向";
            this.Negative.UseVisualStyleBackColor = true;
            this.Negative.Click += new System.EventHandler(this.Negative_Click);
            // 
            // rpmlabel
            // 
            this.rpmlabel.AutoSize = true;
            this.rpmlabel.Location = new System.Drawing.Point(218, 43);
            this.rpmlabel.Name = "rpmlabel";
            this.rpmlabel.Size = new System.Drawing.Size(30, 15);
            this.rpmlabel.TabIndex = 20;
            this.rpmlabel.Text = "rpm";
            // 
            // RpmTimelabel
            // 
            this.RpmTimelabel.AutoSize = true;
            this.RpmTimelabel.Location = new System.Drawing.Point(218, 78);
            this.RpmTimelabel.Name = "RpmTimelabel";
            this.RpmTimelabel.Size = new System.Drawing.Size(78, 15);
            this.RpmTimelabel.TabIndex = 21;
            this.RpmTimelabel.Text = "ms/1000rpm";
            // 
            // PointVelocitylabel
            // 
            this.PointVelocitylabel.AutoSize = true;
            this.PointVelocitylabel.Location = new System.Drawing.Point(35, 36);
            this.PointVelocitylabel.Name = "PointVelocitylabel";
            this.PointVelocitylabel.Size = new System.Drawing.Size(71, 15);
            this.PointVelocitylabel.TabIndex = 15;
            this.PointVelocitylabel.Text = "點動速度:";
            // 
            // Position
            // 
            this.Position.Location = new System.Drawing.Point(23, 106);
            this.Position.Name = "Position";
            this.Position.Size = new System.Drawing.Size(116, 54);
            this.Position.TabIndex = 8;
            this.Position.Text = "正向";
            this.Position.UseVisualStyleBackColor = true;
            this.Position.Click += new System.EventHandler(this.Position_Click);
            // 
            // PointVelocity
            // 
            this.PointVelocity.Location = new System.Drawing.Point(112, 33);
            this.PointVelocity.Name = "PointVelocity";
            this.PointVelocity.Size = new System.Drawing.Size(100, 25);
            this.PointVelocity.TabIndex = 13;
            this.PointVelocity.Text = "200";
            // 
            // TimeTacclabel
            // 
            this.TimeTacclabel.AutoSize = true;
            this.TimeTacclabel.Location = new System.Drawing.Point(22, 78);
            this.TimeTacclabel.Name = "TimeTacclabel";
            this.TimeTacclabel.Size = new System.Drawing.Size(86, 15);
            this.TimeTacclabel.TabIndex = 16;
            this.TimeTacclabel.Text = "加減速時間:";
            // 
            // TimeTacc
            // 
            this.TimeTacc.Location = new System.Drawing.Point(112, 68);
            this.TimeTacc.Name = "TimeTacc";
            this.TimeTacc.Size = new System.Drawing.Size(100, 25);
            this.TimeTacc.TabIndex = 14;
            this.TimeTacc.Text = "200";
            // 
            // CurrentPosition
            // 
            this.CurrentPosition.Location = new System.Drawing.Point(112, 61);
            this.CurrentPosition.Name = "CurrentPosition";
            this.CurrentPosition.ReadOnly = true;
            this.CurrentPosition.Size = new System.Drawing.Size(100, 25);
            this.CurrentPosition.TabIndex = 22;
            // 
            // CurPoslabel
            // 
            this.CurPoslabel.AutoSize = true;
            this.CurPoslabel.Location = new System.Drawing.Point(5, 66);
            this.CurPoslabel.Name = "CurPoslabel";
            this.CurPoslabel.Size = new System.Drawing.Size(101, 15);
            this.CurPoslabel.TabIndex = 21;
            this.CurPoslabel.Text = "電機當前位置:";
            // 
            // CurrentPos
            // 
            this.CurrentPos.Controls.Add(this.UpdatePosition);
            this.CurrentPos.Controls.Add(this.CurrentPosition);
            this.CurrentPos.Controls.Add(this.CurPoslabel);
            this.CurrentPos.Location = new System.Drawing.Point(12, 176);
            this.CurrentPos.Name = "CurrentPos";
            this.CurrentPos.Size = new System.Drawing.Size(238, 133);
            this.CurrentPos.TabIndex = 26;
            this.CurrentPos.TabStop = false;
            this.CurrentPos.Text = "目前位置";
            // 
            // UpdatePosition
            // 
            this.UpdatePosition.Location = new System.Drawing.Point(157, 97);
            this.UpdatePosition.Name = "UpdatePosition";
            this.UpdatePosition.Size = new System.Drawing.Size(75, 33);
            this.UpdatePosition.TabIndex = 23;
            this.UpdatePosition.Text = "更新";
            this.UpdatePosition.UseVisualStyleBackColor = true;
            this.UpdatePosition.Click += new System.EventHandler(this.UpdatePosition_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1502, 27);
            this.menuStrip1.TabIndex = 27;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(47, 23);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(127, 26);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(127, 26);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // LeadShinePropertyGrid
            // 
            this.LeadShinePropertyGrid.Location = new System.Drawing.Point(819, 65);
            this.LeadShinePropertyGrid.Name = "LeadShinePropertyGrid";
            this.LeadShinePropertyGrid.Size = new System.Drawing.Size(670, 895);
            this.LeadShinePropertyGrid.TabIndex = 28;
            // 
            // MotionList
            // 
            this.MotionList.AllowUserToOrderColumns = true;
            this.MotionList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.MotionList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Path,
            this.DeviceID,
            this.MovePosition,
            this.MoveVelocity,
            this.MoveTacc,
            this.MoveDac});
            this.MotionList.Location = new System.Drawing.Point(12, 476);
            this.MotionList.Name = "MotionList";
            this.MotionList.RowHeadersWidth = 51;
            this.MotionList.RowTemplate.Height = 27;
            this.MotionList.Size = new System.Drawing.Size(795, 445);
            this.MotionList.TabIndex = 29;
            this.MotionList.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.MotionList_CellContentDoubleClick);
            // 
            // Path
            // 
            this.Path.HeaderText = "Path";
            this.Path.MinimumWidth = 6;
            this.Path.Name = "Path";
            this.Path.Width = 125;
            // 
            // DeviceID
            // 
            this.DeviceID.HeaderText = "DeviceID";
            this.DeviceID.MinimumWidth = 6;
            this.DeviceID.Name = "DeviceID";
            this.DeviceID.Width = 125;
            // 
            // MovePosition
            // 
            this.MovePosition.HeaderText = "MovePosition";
            this.MovePosition.MinimumWidth = 6;
            this.MovePosition.Name = "MovePosition";
            this.MovePosition.Width = 125;
            // 
            // MoveVelocity
            // 
            this.MoveVelocity.HeaderText = "MoveVelocity";
            this.MoveVelocity.MinimumWidth = 6;
            this.MoveVelocity.Name = "MoveVelocity";
            this.MoveVelocity.Width = 125;
            // 
            // MoveTacc
            // 
            this.MoveTacc.HeaderText = "MoveTacc";
            this.MoveTacc.MinimumWidth = 6;
            this.MoveTacc.Name = "MoveTacc";
            this.MoveTacc.Width = 125;
            // 
            // MoveDac
            // 
            this.MoveDac.HeaderText = "MoveDac";
            this.MoveDac.MinimumWidth = 6;
            this.MoveDac.Name = "MoveDac";
            this.MoveDac.Width = 125;
            // 
            // ResetHome
            // 
            this.ResetHome.Location = new System.Drawing.Point(465, 88);
            this.ResetHome.Name = "ResetHome";
            this.ResetHome.Size = new System.Drawing.Size(75, 38);
            this.ResetHome.TabIndex = 30;
            this.ResetHome.Text = "回原點";
            this.ResetHome.UseVisualStyleBackColor = true;
            this.ResetHome.Click += new System.EventHandler(this.ResetHome_Click);
            // 
            // ResetDir
            // 
            this.ResetDir.FormattingEnabled = true;
            this.ResetDir.Location = new System.Drawing.Point(238, 23);
            this.ResetDir.Name = "ResetDir";
            this.ResetDir.Size = new System.Drawing.Size(83, 23);
            this.ResetDir.TabIndex = 31;
            this.ResetDir.Text = "Positive";
            // 
            // ResetControl
            // 
            this.ResetControl.Controls.Add(this.ResetDac);
            this.ResetControl.Controls.Add(this.HighSpeed);
            this.ResetControl.Controls.Add(this.ResetHome);
            this.ResetControl.Controls.Add(this.ResetDaclabel);
            this.ResetControl.Controls.Add(this.ResetTacclabel);
            this.ResetControl.Controls.Add(this.ResetMode);
            this.ResetControl.Controls.Add(this.ResetModelabel);
            this.ResetControl.Controls.Add(this.LowSpeed);
            this.ResetControl.Controls.Add(this.LowSpeedlabel);
            this.ResetControl.Controls.Add(this.StopPosition);
            this.ResetControl.Controls.Add(this.ResetTacc);
            this.ResetControl.Controls.Add(this.ZeroPoint);
            this.ResetControl.Controls.Add(this.HighSpeedlabel);
            this.ResetControl.Controls.Add(this.label1);
            this.ResetControl.Controls.Add(this.ZeroPointlabel);
            this.ResetControl.Controls.Add(this.SpecifyLocation);
            this.ResetControl.Controls.Add(this.SpecifyLocationLabel);
            this.ResetControl.Controls.Add(this.Directionlabel);
            this.ResetControl.Controls.Add(this.ResetDir);
            this.ResetControl.Location = new System.Drawing.Point(256, 176);
            this.ResetControl.Name = "ResetControl";
            this.ResetControl.Size = new System.Drawing.Size(551, 133);
            this.ResetControl.TabIndex = 32;
            this.ResetControl.TabStop = false;
            this.ResetControl.Text = "原點控制";
            // 
            // ResetDac
            // 
            this.ResetDac.Location = new System.Drawing.Point(403, 97);
            this.ResetDac.Name = "ResetDac";
            this.ResetDac.Size = new System.Drawing.Size(56, 25);
            this.ResetDac.TabIndex = 46;
            this.ResetDac.Text = "100";
            // 
            // HighSpeed
            // 
            this.HighSpeed.Location = new System.Drawing.Point(267, 58);
            this.HighSpeed.Name = "HighSpeed";
            this.HighSpeed.Size = new System.Drawing.Size(56, 25);
            this.HighSpeed.TabIndex = 45;
            this.HighSpeed.Text = "1000";
            // 
            // ResetDaclabel
            // 
            this.ResetDaclabel.AutoSize = true;
            this.ResetDaclabel.Location = new System.Drawing.Point(344, 100);
            this.ResetDaclabel.Name = "ResetDaclabel";
            this.ResetDaclabel.Size = new System.Drawing.Size(56, 15);
            this.ResetDaclabel.TabIndex = 44;
            this.ResetDaclabel.Text = "減速度:";
            // 
            // ResetTacclabel
            // 
            this.ResetTacclabel.AutoSize = true;
            this.ResetTacclabel.Location = new System.Drawing.Point(344, 66);
            this.ResetTacclabel.Name = "ResetTacclabel";
            this.ResetTacclabel.Size = new System.Drawing.Size(56, 15);
            this.ResetTacclabel.TabIndex = 43;
            this.ResetTacclabel.Text = "加速度:";
            // 
            // ResetMode
            // 
            this.ResetMode.FormattingEnabled = true;
            this.ResetMode.Location = new System.Drawing.Point(77, 24);
            this.ResetMode.Name = "ResetMode";
            this.ResetMode.Size = new System.Drawing.Size(83, 23);
            this.ResetMode.TabIndex = 42;
            this.ResetMode.Text = "Origin_Reset";
            // 
            // ResetModelabel
            // 
            this.ResetModelabel.AutoSize = true;
            this.ResetModelabel.Location = new System.Drawing.Point(6, 29);
            this.ResetModelabel.Name = "ResetModelabel";
            this.ResetModelabel.Size = new System.Drawing.Size(71, 15);
            this.ResetModelabel.TabIndex = 41;
            this.ResetModelabel.Text = "零點模式:";
            // 
            // LowSpeed
            // 
            this.LowSpeed.Location = new System.Drawing.Point(267, 97);
            this.LowSpeed.Name = "LowSpeed";
            this.LowSpeed.Size = new System.Drawing.Size(56, 25);
            this.LowSpeed.TabIndex = 40;
            this.LowSpeed.Text = "800";
            // 
            // LowSpeedlabel
            // 
            this.LowSpeedlabel.AutoSize = true;
            this.LowSpeedlabel.Location = new System.Drawing.Point(191, 100);
            this.LowSpeedlabel.Name = "LowSpeedlabel";
            this.LowSpeedlabel.Size = new System.Drawing.Size(71, 15);
            this.LowSpeedlabel.TabIndex = 39;
            this.LowSpeedlabel.Text = "回零低速:";
            // 
            // StopPosition
            // 
            this.StopPosition.Location = new System.Drawing.Point(128, 97);
            this.StopPosition.Name = "StopPosition";
            this.StopPosition.Size = new System.Drawing.Size(56, 25);
            this.StopPosition.TabIndex = 38;
            this.StopPosition.Text = "-500";
            // 
            // ResetTacc
            // 
            this.ResetTacc.Location = new System.Drawing.Point(403, 61);
            this.ResetTacc.Name = "ResetTacc";
            this.ResetTacc.Size = new System.Drawing.Size(56, 25);
            this.ResetTacc.TabIndex = 37;
            this.ResetTacc.Text = "100";
            // 
            // ZeroPoint
            // 
            this.ZeroPoint.Location = new System.Drawing.Point(77, 58);
            this.ZeroPoint.Name = "ZeroPoint";
            this.ZeroPoint.Size = new System.Drawing.Size(56, 25);
            this.ZeroPoint.TabIndex = 14;
            this.ZeroPoint.Text = "0";
            // 
            // HighSpeedlabel
            // 
            this.HighSpeedlabel.AutoSize = true;
            this.HighSpeedlabel.Location = new System.Drawing.Point(191, 66);
            this.HighSpeedlabel.Name = "HighSpeedlabel";
            this.HighSpeedlabel.Size = new System.Drawing.Size(71, 15);
            this.HighSpeedlabel.TabIndex = 36;
            this.HighSpeedlabel.Text = "回零高速:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 15);
            this.label1.TabIndex = 35;
            this.label1.Text = "回零點停止位置:";
            // 
            // ZeroPointlabel
            // 
            this.ZeroPointlabel.AutoSize = true;
            this.ZeroPointlabel.Location = new System.Drawing.Point(6, 61);
            this.ZeroPointlabel.Name = "ZeroPointlabel";
            this.ZeroPointlabel.Size = new System.Drawing.Size(71, 15);
            this.ZeroPointlabel.TabIndex = 34;
            this.ZeroPointlabel.Text = "零點位置:";
            // 
            // SpecifyLocation
            // 
            this.SpecifyLocation.FormattingEnabled = true;
            this.SpecifyLocation.Location = new System.Drawing.Point(400, 23);
            this.SpecifyLocation.Name = "SpecifyLocation";
            this.SpecifyLocation.Size = new System.Drawing.Size(83, 23);
            this.SpecifyLocation.TabIndex = 33;
            this.SpecifyLocation.Text = "Yes";
            // 
            // SpecifyLocationLabel
            // 
            this.SpecifyLocationLabel.AutoSize = true;
            this.SpecifyLocationLabel.Location = new System.Drawing.Point(329, 27);
            this.SpecifyLocationLabel.Name = "SpecifyLocationLabel";
            this.SpecifyLocationLabel.Size = new System.Drawing.Size(71, 15);
            this.SpecifyLocationLabel.TabIndex = 32;
            this.SpecifyLocationLabel.Text = "指定位置:";
            // 
            // Directionlabel
            // 
            this.Directionlabel.AutoSize = true;
            this.Directionlabel.Location = new System.Drawing.Point(171, 29);
            this.Directionlabel.Name = "Directionlabel";
            this.Directionlabel.Size = new System.Drawing.Size(61, 15);
            this.Directionlabel.TabIndex = 14;
            this.Directionlabel.Text = "方向(D):";
            // 
            // LeadShineMotionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1502, 951);
            this.Controls.Add(this.MotionList);
            this.Controls.Add(this.LeadShinePropertyGrid);
            this.Controls.Add(this.CurrentPos);
            this.Controls.Add(this.JOGRun);
            this.Controls.Add(this.MotionControl);
            this.Controls.Add(this.UnInit);
            this.Controls.Add(this.Init);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.ResetControl);
            this.Name = "LeadShineMotionForm";
            this.Text = "LeadShineMotionForm";
            this.Load += new System.EventHandler(this.LeadShineMotionForm_Load);
            this.MotionControl.ResumeLayout(false);
            this.MotionControl.PerformLayout();
            this.JOGRun.ResumeLayout(false);
            this.JOGRun.PerformLayout();
            this.CurrentPos.ResumeLayout(false);
            this.CurrentPos.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MotionList)).EndInit();
            this.ResetControl.ResumeLayout(false);
            this.ResetControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox MotionControl;
        private System.Windows.Forms.Label Daclabel;
        private System.Windows.Forms.Label Tacclabel;
        private System.Windows.Forms.TextBox dac;
        private System.Windows.Forms.TextBox tacc;
        private System.Windows.Forms.Label Velocitylabel;
        private System.Windows.Forms.Label Postionlabel;
        private System.Windows.Forms.TextBox max_vel;
        private System.Windows.Forms.TextBox postion;
        private System.Windows.Forms.Button Trigger;
        private System.Windows.Forms.Button Init;
        private System.Windows.Forms.Button UnInit;
        private System.Windows.Forms.GroupBox JOGRun;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label Cycleslabel;
        private System.Windows.Forms.Label IntervalTimelabel;
        private System.Windows.Forms.TextBox CyclesTimes;
        private System.Windows.Forms.TextBox IntervalTime;
        private System.Windows.Forms.Button Negative;
        private System.Windows.Forms.Label rpmlabel;
        private System.Windows.Forms.Label RpmTimelabel;
        private System.Windows.Forms.Label PointVelocitylabel;
        private System.Windows.Forms.Button Position;
        private System.Windows.Forms.TextBox PointVelocity;
        private System.Windows.Forms.Label TimeTacclabel;
        private System.Windows.Forms.TextBox TimeTacc;
        private System.Windows.Forms.TextBox CurrentPosition;
        private System.Windows.Forms.Label CurPoslabel;
        private System.Windows.Forms.GroupBox CurrentPos;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.PropertyGrid LeadShinePropertyGrid;
        private System.Windows.Forms.DataGridView MotionList;
        private System.Windows.Forms.Button AddList;
        private System.Windows.Forms.DataGridViewTextBoxColumn Path;
        private System.Windows.Forms.DataGridViewTextBoxColumn DeviceID;
        private System.Windows.Forms.DataGridViewTextBoxColumn MovePosition;
        private System.Windows.Forms.DataGridViewTextBoxColumn MoveVelocity;
        private System.Windows.Forms.DataGridViewTextBoxColumn MoveTacc;
        private System.Windows.Forms.DataGridViewTextBoxColumn MoveDac;
        private System.Windows.Forms.Button ResetHome;
        private System.Windows.Forms.ComboBox ResetDir;
        private System.Windows.Forms.GroupBox ResetControl;
        private System.Windows.Forms.Label Directionlabel;
        private System.Windows.Forms.Label SpecifyLocationLabel;
        private System.Windows.Forms.Label ZeroPointlabel;
        private System.Windows.Forms.ComboBox SpecifyLocation;
        private System.Windows.Forms.Label LowSpeedlabel;
        private System.Windows.Forms.TextBox StopPosition;
        private System.Windows.Forms.TextBox ResetTacc;
        private System.Windows.Forms.TextBox ZeroPoint;
        private System.Windows.Forms.Label HighSpeedlabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox LowSpeed;
        private System.Windows.Forms.ComboBox ResetMode;
        private System.Windows.Forms.Label ResetModelabel;
        private System.Windows.Forms.TextBox ResetDac;
        private System.Windows.Forms.TextBox HighSpeed;
        private System.Windows.Forms.Label ResetDaclabel;
        private System.Windows.Forms.Label ResetTacclabel;
        private System.Windows.Forms.Button UpdatePosition;
        private System.Windows.Forms.Label PRSetlabel;
        private System.Windows.Forms.Label PlusRotateSetlabel;
        private System.Windows.Forms.TextBox PlusValue;
        private System.Windows.Forms.Button PlusRotateSet;
    }
}