namespace AutoTestSystem
{
    partial class MFGX
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MFGX));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.label_totalpass = new System.Windows.Forms.ToolStripStatusLabel();
            this.lb_passNum = new System.Windows.Forms.ToolStripStatusLabel();
            this.lb_totalFail = new System.Windows.Forms.ToolStripStatusLabel();
            this.lb_FailNum = new System.Windows.Forms.ToolStripStatusLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel_datagridview = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel_dashboard = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel_Ver = new System.Windows.Forms.TableLayoutPanel();
            this.label_version = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.BTNReport = new System.Windows.Forms.Button();
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.BTNMaintenance = new System.Windows.Forms.Button();
            this.ConfigureBtn = new System.Windows.Forms.Button();
            this.BtnLogin = new System.Windows.Forms.Button();
            this.HomeBtn = new System.Windows.Forms.Button();
            this.tableLayoutPanel_mode = new System.Windows.Forms.TableLayoutPanel();
            this.label_mode = new System.Windows.Forms.Label();
            this.labelMode = new System.Windows.Forms.Label();
            this.tableLayoutPanel_station = new System.Windows.Forms.TableLayoutPanel();
            this.label_station = new System.Windows.Forms.Label();
            this.labelStation = new System.Windows.Forms.Label();
            this.tableLayoutPanel_project = new System.Windows.Forms.TableLayoutPanel();
            this.label_project = new System.Windows.Forms.Label();
            this.label_ProjectTitle = new System.Windows.Forms.Label();
            this.tableLayoutPanel_user = new System.Windows.Forms.TableLayoutPanel();
            this.label_user = new System.Windows.Forms.Label();
            this.labelUser = new System.Windows.Forms.Label();
            this.StartBtn = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel_Ver.SuspendLayout();
            this.tableLayoutPanel_mode.SuspendLayout();
            this.tableLayoutPanel_station.SuspendLayout();
            this.tableLayoutPanel_project.SuspendLayout();
            this.tableLayoutPanel_user.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.label_totalpass,
            this.lb_passNum,
            this.lb_totalFail,
            this.lb_FailNum});
            this.statusStrip1.Location = new System.Drawing.Point(0, 633);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 9, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1184, 26);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // label_totalpass
            // 
            this.label_totalpass.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label_totalpass.Name = "label_totalpass";
            this.label_totalpass.Size = new System.Drawing.Size(124, 20);
            this.label_totalpass.Text = "   Total_PASS: ";
            this.label_totalpass.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.label_totalpass.Click += new System.EventHandler(this.label_totalpass_Click);
            // 
            // lb_passNum
            // 
            this.lb_passNum.Name = "lb_passNum";
            this.lb_passNum.Size = new System.Drawing.Size(18, 20);
            this.lb_passNum.Text = "0";
            // 
            // lb_totalFail
            // 
            this.lb_totalFail.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lb_totalFail.Name = "lb_totalFail";
            this.lb_totalFail.Size = new System.Drawing.Size(115, 20);
            this.lb_totalFail.Text = "   Total_FAIL: ";
            this.lb_totalFail.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lb_FailNum
            // 
            this.lb_FailNum.Name = "lb_FailNum";
            this.lb_FailNum.Size = new System.Drawing.Size(18, 20);
            this.lb_FailNum.Text = "0";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.144417F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 91.85558F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 906F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1184, 633);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel_datagridview, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel_dashboard, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(98, 2);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1084, 629);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel_datagridview
            // 
            this.tableLayoutPanel_datagridview.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel_datagridview.ColumnCount = 1;
            this.tableLayoutPanel_datagridview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_datagridview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel_datagridview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel_datagridview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel_datagridview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_datagridview.Location = new System.Drawing.Point(2, 631);
            this.tableLayoutPanel_datagridview.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel_datagridview.Name = "tableLayoutPanel_datagridview";
            this.tableLayoutPanel_datagridview.RowCount = 1;
            this.tableLayoutPanel_datagridview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_datagridview.Size = new System.Drawing.Size(1080, 1);
            this.tableLayoutPanel_datagridview.TabIndex = 1;
            // 
            // tableLayoutPanel_dashboard
            // 
            this.tableLayoutPanel_dashboard.ColumnCount = 1;
            this.tableLayoutPanel_dashboard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_dashboard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel_dashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_dashboard.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel_dashboard.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel_dashboard.Name = "tableLayoutPanel_dashboard";
            this.tableLayoutPanel_dashboard.RowCount = 1;
            this.tableLayoutPanel_dashboard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_dashboard.Size = new System.Drawing.Size(1080, 625);
            this.tableLayoutPanel_dashboard.TabIndex = 2;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel_Ver, 0, 4);
            this.tableLayoutPanel3.Controls.Add(this.BTNReport, 0, 10);
            this.tableLayoutPanel3.Controls.Add(this.BTNMaintenance, 0, 10);
            this.tableLayoutPanel3.Controls.Add(this.ConfigureBtn, 0, 8);
            this.tableLayoutPanel3.Controls.Add(this.BtnLogin, 0, 8);
            this.tableLayoutPanel3.Controls.Add(this.HomeBtn, 0, 6);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel_mode, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel_station, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel_project, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel_user, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.StartBtn, 0, 7);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 12;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 53F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(92, 629);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // tableLayoutPanel_Ver
            // 
            this.tableLayoutPanel_Ver.ColumnCount = 1;
            this.tableLayoutPanel_Ver.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_Ver.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel_Ver.Controls.Add(this.label_version, 0, 1);
            this.tableLayoutPanel_Ver.Controls.Add(this.labelVersion, 0, 0);
            this.tableLayoutPanel_Ver.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_Ver.Location = new System.Drawing.Point(4, 384);
            this.tableLayoutPanel_Ver.Name = "tableLayoutPanel_Ver";
            this.tableLayoutPanel_Ver.RowCount = 2;
            this.tableLayoutPanel_Ver.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_Ver.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_Ver.Size = new System.Drawing.Size(84, 88);
            this.tableLayoutPanel_Ver.TabIndex = 51;
            // 
            // label_version
            // 
            this.label_version.AutoSize = true;
            this.label_version.BackColor = System.Drawing.Color.Turquoise;
            this.label_version.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_version.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_version.ForeColor = System.Drawing.Color.Blue;
            this.label_version.Location = new System.Drawing.Point(3, 44);
            this.label_version.Name = "label_version";
            this.label_version.Size = new System.Drawing.Size(78, 44);
            this.label_version.TabIndex = 4;
            this.label_version.Text = "V3.0.0.5";
            this.label_version.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.BackColor = System.Drawing.Color.Black;
            this.labelVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelVersion.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVersion.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.labelVersion.Location = new System.Drawing.Point(3, 0);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(78, 44);
            this.labelVersion.TabIndex = 3;
            this.labelVersion.Text = "Version";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BTNReport
            // 
            this.BTNReport.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.BTNReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTNReport.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveCaption;
            this.BTNReport.FlatAppearance.BorderSize = 0;
            this.BTNReport.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.BTNReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTNReport.ImageIndex = 8;
            this.BTNReport.ImageList = this.imageList2;
            this.BTNReport.Location = new System.Drawing.Point(3, 576);
            this.BTNReport.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.BTNReport.Name = "BTNReport";
            this.BTNReport.Size = new System.Drawing.Size(86, 50);
            this.BTNReport.TabIndex = 48;
            this.BTNReport.UseVisualStyleBackColor = false;
            this.BTNReport.Click += new System.EventHandler(this.BTNReport_Click);
            // 
            // imageList2
            // 
            this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList2.Images.SetKeyName(0, "setting2.png");
            this.imageList2.Images.SetKeyName(1, "log-in.png");
            this.imageList2.Images.SetKeyName(2, "lock.png");
            this.imageList2.Images.SetKeyName(3, "login-page.png");
            this.imageList2.Images.SetKeyName(4, "icons8-login-64.png");
            this.imageList2.Images.SetKeyName(5, "icons8-login-64.png");
            this.imageList2.Images.SetKeyName(6, "engineering.png");
            this.imageList2.Images.SetKeyName(7, "smart-home.png");
            this.imageList2.Images.SetKeyName(8, "report.png");
            this.imageList2.Images.SetKeyName(9, "icons8-play-30.png");
            this.imageList2.Images.SetKeyName(10, "icons8-stop-30.png");
            // 
            // BTNMaintenance
            // 
            this.BTNMaintenance.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.BTNMaintenance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTNMaintenance.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveCaption;
            this.BTNMaintenance.FlatAppearance.BorderSize = 0;
            this.BTNMaintenance.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.BTNMaintenance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTNMaintenance.ImageIndex = 6;
            this.BTNMaintenance.ImageList = this.imageList2;
            this.BTNMaintenance.Location = new System.Drawing.Point(3, 522);
            this.BTNMaintenance.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.BTNMaintenance.Name = "BTNMaintenance";
            this.BTNMaintenance.Size = new System.Drawing.Size(86, 49);
            this.BTNMaintenance.TabIndex = 47;
            this.BTNMaintenance.UseVisualStyleBackColor = false;
            this.BTNMaintenance.Click += new System.EventHandler(this.BTNMaintenance_Click);
            // 
            // ConfigureBtn
            // 
            this.ConfigureBtn.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ConfigureBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConfigureBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveCaption;
            this.ConfigureBtn.FlatAppearance.BorderSize = 0;
            this.ConfigureBtn.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ConfigureBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ConfigureBtn.ImageIndex = 0;
            this.ConfigureBtn.ImageList = this.imageList2;
            this.ConfigureBtn.Location = new System.Drawing.Point(3, 465);
            this.ConfigureBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ConfigureBtn.Name = "ConfigureBtn";
            this.ConfigureBtn.Size = new System.Drawing.Size(86, 52);
            this.ConfigureBtn.TabIndex = 45;
            this.ConfigureBtn.UseVisualStyleBackColor = false;
            this.ConfigureBtn.Click += new System.EventHandler(this.ConfigureBtn_Click);
            // 
            // BtnLogin
            // 
            this.BtnLogin.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.BtnLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BtnLogin.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveCaption;
            this.BtnLogin.FlatAppearance.BorderSize = 0;
            this.BtnLogin.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.BtnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BtnLogin.ImageIndex = 1;
            this.BtnLogin.ImageList = this.imageList2;
            this.BtnLogin.Location = new System.Drawing.Point(3, 409);
            this.BtnLogin.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.BtnLogin.Name = "BtnLogin";
            this.BtnLogin.Size = new System.Drawing.Size(86, 51);
            this.BtnLogin.TabIndex = 44;
            this.BtnLogin.UseVisualStyleBackColor = false;
            this.BtnLogin.Click += new System.EventHandler(this.BtnLogin_Click);
            // 
            // HomeBtn
            // 
            this.HomeBtn.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.HomeBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HomeBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveCaption;
            this.HomeBtn.FlatAppearance.BorderSize = 0;
            this.HomeBtn.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.HomeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.HomeBtn.Font = new System.Drawing.Font("微軟正黑體", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.HomeBtn.ImageIndex = 7;
            this.HomeBtn.ImageList = this.imageList2;
            this.HomeBtn.Location = new System.Drawing.Point(3, 295);
            this.HomeBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.HomeBtn.Name = "HomeBtn";
            this.HomeBtn.Size = new System.Drawing.Size(86, 52);
            this.HomeBtn.TabIndex = 36;
            this.HomeBtn.UseVisualStyleBackColor = false;
            this.HomeBtn.Click += new System.EventHandler(this.HomeBtn_ClickAsync);
            // 
            // tableLayoutPanel_mode
            // 
            this.tableLayoutPanel_mode.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel_mode.ColumnCount = 1;
            this.tableLayoutPanel_mode.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_mode.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel_mode.Controls.Add(this.label_mode, 0, 1);
            this.tableLayoutPanel_mode.Controls.Add(this.labelMode, 0, 0);
            this.tableLayoutPanel_mode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_mode.Location = new System.Drawing.Point(4, 194);
            this.tableLayoutPanel_mode.Name = "tableLayoutPanel_mode";
            this.tableLayoutPanel_mode.RowCount = 2;
            this.tableLayoutPanel_mode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 51.72414F));
            this.tableLayoutPanel_mode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 48.27586F));
            this.tableLayoutPanel_mode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel_mode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel_mode.Size = new System.Drawing.Size(84, 88);
            this.tableLayoutPanel_mode.TabIndex = 9;
            // 
            // label_mode
            // 
            this.label_mode.AutoSize = true;
            this.label_mode.BackColor = System.Drawing.Color.Turquoise;
            this.label_mode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_mode.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_mode.ForeColor = System.Drawing.Color.Blue;
            this.label_mode.Location = new System.Drawing.Point(4, 45);
            this.label_mode.Name = "label_mode";
            this.label_mode.Size = new System.Drawing.Size(76, 42);
            this.label_mode.TabIndex = 2;
            this.label_mode.Text = "Mode";
            this.label_mode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelMode
            // 
            this.labelMode.AutoSize = true;
            this.labelMode.BackColor = System.Drawing.Color.Black;
            this.labelMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelMode.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMode.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.labelMode.Location = new System.Drawing.Point(4, 1);
            this.labelMode.Name = "labelMode";
            this.labelMode.Size = new System.Drawing.Size(76, 43);
            this.labelMode.TabIndex = 1;
            this.labelMode.Text = "Mode";
            this.labelMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel_station
            // 
            this.tableLayoutPanel_station.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel_station.ColumnCount = 1;
            this.tableLayoutPanel_station.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_station.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel_station.Controls.Add(this.label_station, 0, 1);
            this.tableLayoutPanel_station.Controls.Add(this.labelStation, 0, 0);
            this.tableLayoutPanel_station.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_station.Location = new System.Drawing.Point(4, 99);
            this.tableLayoutPanel_station.Name = "tableLayoutPanel_station";
            this.tableLayoutPanel_station.RowCount = 2;
            this.tableLayoutPanel_station.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 52.87356F));
            this.tableLayoutPanel_station.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 47.12644F));
            this.tableLayoutPanel_station.Size = new System.Drawing.Size(84, 88);
            this.tableLayoutPanel_station.TabIndex = 8;
            // 
            // label_station
            // 
            this.label_station.AutoSize = true;
            this.label_station.BackColor = System.Drawing.Color.Turquoise;
            this.label_station.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_station.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_station.ForeColor = System.Drawing.Color.Blue;
            this.label_station.Location = new System.Drawing.Point(4, 46);
            this.label_station.Name = "label_station";
            this.label_station.Size = new System.Drawing.Size(76, 41);
            this.label_station.TabIndex = 2;
            this.label_station.Text = "Station";
            this.label_station.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelStation
            // 
            this.labelStation.BackColor = System.Drawing.Color.Black;
            this.labelStation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelStation.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStation.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.labelStation.Location = new System.Drawing.Point(4, 1);
            this.labelStation.Name = "labelStation";
            this.labelStation.Size = new System.Drawing.Size(76, 44);
            this.labelStation.TabIndex = 1;
            this.labelStation.Text = "Station";
            this.labelStation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel_project
            // 
            this.tableLayoutPanel_project.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel_project.ColumnCount = 1;
            this.tableLayoutPanel_project.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_project.Controls.Add(this.label_project, 0, 1);
            this.tableLayoutPanel_project.Controls.Add(this.label_ProjectTitle, 0, 0);
            this.tableLayoutPanel_project.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_project.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel_project.Name = "tableLayoutPanel_project";
            this.tableLayoutPanel_project.RowCount = 2;
            this.tableLayoutPanel_project.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 57.47126F));
            this.tableLayoutPanel_project.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42.52874F));
            this.tableLayoutPanel_project.Size = new System.Drawing.Size(84, 88);
            this.tableLayoutPanel_project.TabIndex = 7;
            // 
            // label_project
            // 
            this.label_project.AutoSize = true;
            this.label_project.BackColor = System.Drawing.Color.Turquoise;
            this.label_project.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_project.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_project.ForeColor = System.Drawing.Color.Blue;
            this.label_project.Location = new System.Drawing.Point(4, 50);
            this.label_project.Name = "label_project";
            this.label_project.Size = new System.Drawing.Size(76, 37);
            this.label_project.TabIndex = 1;
            this.label_project.Text = "Project";
            this.label_project.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_ProjectTitle
            // 
            this.label_ProjectTitle.AutoSize = true;
            this.label_ProjectTitle.BackColor = System.Drawing.Color.Black;
            this.label_ProjectTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_ProjectTitle.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_ProjectTitle.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label_ProjectTitle.Location = new System.Drawing.Point(4, 1);
            this.label_ProjectTitle.Name = "label_ProjectTitle";
            this.label_ProjectTitle.Size = new System.Drawing.Size(76, 48);
            this.label_ProjectTitle.TabIndex = 0;
            this.label_ProjectTitle.Text = "Project";
            this.label_ProjectTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel_user
            // 
            this.tableLayoutPanel_user.ColumnCount = 1;
            this.tableLayoutPanel_user.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_user.Controls.Add(this.label_user, 0, 1);
            this.tableLayoutPanel_user.Controls.Add(this.labelUser, 0, 0);
            this.tableLayoutPanel_user.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_user.Location = new System.Drawing.Point(4, 289);
            this.tableLayoutPanel_user.Name = "tableLayoutPanel_user";
            this.tableLayoutPanel_user.RowCount = 2;
            this.tableLayoutPanel_user.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_user.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_user.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel_user.Size = new System.Drawing.Size(84, 88);
            this.tableLayoutPanel_user.TabIndex = 49;
            // 
            // label_user
            // 
            this.label_user.AutoSize = true;
            this.label_user.BackColor = System.Drawing.Color.Turquoise;
            this.label_user.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_user.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_user.ForeColor = System.Drawing.Color.Blue;
            this.label_user.Location = new System.Drawing.Point(3, 44);
            this.label_user.Name = "label_user";
            this.label_user.Size = new System.Drawing.Size(78, 44);
            this.label_user.TabIndex = 3;
            this.label_user.Text = "User";
            this.label_user.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelUser
            // 
            this.labelUser.AutoSize = true;
            this.labelUser.BackColor = System.Drawing.Color.Black;
            this.labelUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelUser.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUser.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.labelUser.Location = new System.Drawing.Point(3, 0);
            this.labelUser.Name = "labelUser";
            this.labelUser.Size = new System.Drawing.Size(78, 44);
            this.labelUser.TabIndex = 2;
            this.labelUser.Text = "User";
            this.labelUser.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // StartBtn
            // 
            this.StartBtn.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.StartBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StartBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveCaption;
            this.StartBtn.FlatAppearance.BorderSize = 0;
            this.StartBtn.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.StartBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StartBtn.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.StartBtn.ImageIndex = 9;
            this.StartBtn.ImageList = this.imageList2;
            this.StartBtn.Location = new System.Drawing.Point(4, 353);
            this.StartBtn.Name = "StartBtn";
            this.StartBtn.Size = new System.Drawing.Size(84, 50);
            this.StartBtn.TabIndex = 52;
            this.StartBtn.UseVisualStyleBackColor = false;
            this.StartBtn.Click += new System.EventHandler(this.StartBtn_Click);
            // 
            // MFGX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 659);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MFGX";
            this.Text = "MFGX";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MFGX_FormClosing);
            this.Shown += new System.EventHandler(this.MFGX_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MFGX_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MFGX_KeyPress);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel_Ver.ResumeLayout(false);
            this.tableLayoutPanel_Ver.PerformLayout();
            this.tableLayoutPanel_mode.ResumeLayout(false);
            this.tableLayoutPanel_mode.PerformLayout();
            this.tableLayoutPanel_station.ResumeLayout(false);
            this.tableLayoutPanel_station.PerformLayout();
            this.tableLayoutPanel_project.ResumeLayout(false);
            this.tableLayoutPanel_project.PerformLayout();
            this.tableLayoutPanel_user.ResumeLayout(false);
            this.tableLayoutPanel_user.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_datagridview;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_dashboard;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button ConfigureBtn;
        private System.Windows.Forms.Button BtnLogin;
        private System.Windows.Forms.Button HomeBtn;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_mode;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_station;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_project;
        private System.Windows.Forms.Label label_ProjectTitle;
        private System.Windows.Forms.Label labelMode;
        private System.Windows.Forms.Label labelStation;
        private System.Windows.Forms.Label label_mode;
        private System.Windows.Forms.Label label_station;
        private System.Windows.Forms.Label label_project;
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.ToolStripStatusLabel label_totalpass;
        private System.Windows.Forms.ToolStripStatusLabel lb_passNum;
        private System.Windows.Forms.ToolStripStatusLabel lb_totalFail;
        private System.Windows.Forms.ToolStripStatusLabel lb_FailNum;
        private System.Windows.Forms.Button BTNReport;
        private System.Windows.Forms.Button BTNMaintenance;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_user;
        private System.Windows.Forms.Label label_user;
        private System.Windows.Forms.Label labelUser;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Ver;
        private System.Windows.Forms.Label label_version;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Button StartBtn;
    }
}