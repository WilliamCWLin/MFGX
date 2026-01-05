namespace AutoTestSystem
{
    partial class Dashboard
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Dashboard));
            this.tp_main_topdown = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl_log = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.richTextBox_dutLog = new System.Windows.Forms.RichTextBox();
            this.tp_main_leftright = new System.Windows.Forms.TableLayoutPanel();
            this.tp_InfoTestitem_topdown = new System.Windows.Forms.TableLayoutPanel();
            this.tp_info_topdown = new System.Windows.Forms.TableLayoutPanel();
            this.tp_info_leftright = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.label_result = new System.Windows.Forms.Label();
            this.tp_err_time_topdown = new System.Windows.Forms.TableLayoutPanel();
            this.label_errorcode = new System.Windows.Forms.Label();
            this.tableLayoutPanel_time_home = new System.Windows.Forms.TableLayoutPanel();
            this.label_elapsetime = new System.Windows.Forms.Label();
            this.tableLayoutPanel_sn = new System.Windows.Forms.TableLayoutPanel();
            this.richTextBox_info = new System.Windows.Forms.RichTextBox();
            this.tp_lightpanel = new System.Windows.Forms.TableLayoutPanel();
            this.HomeBtn = new System.Windows.Forms.Button();
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.panelGreen = new System.Windows.Forms.Panel();
            this.panelYellow = new System.Windows.Forms.Panel();
            this.panelRed = new System.Windows.Forms.Panel();
            this.tp_title_topdown = new System.Windows.Forms.TableLayoutPanel();
            this.label_description = new System.Windows.Forms.Label();
            this.pictureBox_titlebarcode = new System.Windows.Forms.PictureBox();
            this.tbc_test = new System.Windows.Forms.TabControl();
            this.tabPage_dataitem = new System.Windows.Forms.TabPage();
            this.dg_testitem = new System.Windows.Forms.DataGridView();
            this.tabPage_image = new System.Windows.Forms.TabPage();
            this.pictureBox_image = new System.Windows.Forms.PictureBox();
            this.tbc_scripttree = new System.Windows.Forms.TabControl();
            this.tabPage_processtree = new System.Windows.Forms.TabPage();
            this.proTreeView_testitem = new ProTreeView.ProTreeView();
            this.tp_main_topdown.SuspendLayout();
            this.tabControl_log.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tp_main_leftright.SuspendLayout();
            this.tp_InfoTestitem_topdown.SuspendLayout();
            this.tp_info_topdown.SuspendLayout();
            this.tp_info_leftright.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            this.tp_err_time_topdown.SuspendLayout();
            this.tableLayoutPanel_time_home.SuspendLayout();
            this.tp_lightpanel.SuspendLayout();
            this.tp_title_topdown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_titlebarcode)).BeginInit();
            this.tbc_test.SuspendLayout();
            this.tabPage_dataitem.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dg_testitem)).BeginInit();
            this.tabPage_image.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_image)).BeginInit();
            this.tbc_scripttree.SuspendLayout();
            this.tabPage_processtree.SuspendLayout();
            this.SuspendLayout();
            // 
            // tp_main_topdown
            // 
            this.tp_main_topdown.ColumnCount = 1;
            this.tp_main_topdown.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tp_main_topdown.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tp_main_topdown.Controls.Add(this.tabControl_log, 0, 1);
            this.tp_main_topdown.Controls.Add(this.tp_main_leftright, 0, 0);
            this.tp_main_topdown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tp_main_topdown.Location = new System.Drawing.Point(0, 0);
            this.tp_main_topdown.Name = "tp_main_topdown";
            this.tp_main_topdown.RowCount = 2;
            this.tp_main_topdown.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80.35714F));
            this.tp_main_topdown.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.64286F));
            this.tp_main_topdown.Size = new System.Drawing.Size(728, 701);
            this.tp_main_topdown.TabIndex = 0;
            // 
            // tabControl_log
            // 
            this.tabControl_log.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl_log.Controls.Add(this.tabPage1);
            this.tabControl_log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_log.ItemSize = new System.Drawing.Size(0, 1);
            this.tabControl_log.Location = new System.Drawing.Point(3, 566);
            this.tabControl_log.Name = "tabControl_log";
            this.tabControl_log.SelectedIndex = 0;
            this.tabControl_log.Size = new System.Drawing.Size(722, 132);
            this.tabControl_log.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl_log.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.richTextBox_dutLog);
            this.tabPage1.Location = new System.Drawing.Point(4, 5);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(714, 123);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Log";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // richTextBox_dutLog
            // 
            this.richTextBox_dutLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox_dutLog.Location = new System.Drawing.Point(3, 3);
            this.richTextBox_dutLog.Name = "richTextBox_dutLog";
            this.richTextBox_dutLog.Size = new System.Drawing.Size(708, 117);
            this.richTextBox_dutLog.TabIndex = 0;
            this.richTextBox_dutLog.Text = "";
            this.richTextBox_dutLog.MouseDown += new System.Windows.Forms.MouseEventHandler(this.richTextBox_dutLog_MouseDown);
            // 
            // tp_main_leftright
            // 
            this.tp_main_leftright.ColumnCount = 2;
            this.tp_main_leftright.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.03878F));
            this.tp_main_leftright.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.96122F));
            this.tp_main_leftright.Controls.Add(this.tp_InfoTestitem_topdown, 1, 0);
            this.tp_main_leftright.Controls.Add(this.tbc_scripttree, 0, 0);
            this.tp_main_leftright.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tp_main_leftright.Location = new System.Drawing.Point(3, 3);
            this.tp_main_leftright.Name = "tp_main_leftright";
            this.tp_main_leftright.RowCount = 1;
            this.tp_main_leftright.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tp_main_leftright.Size = new System.Drawing.Size(722, 557);
            this.tp_main_leftright.TabIndex = 0;
            // 
            // tp_InfoTestitem_topdown
            // 
            this.tp_InfoTestitem_topdown.ColumnCount = 1;
            this.tp_InfoTestitem_topdown.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tp_InfoTestitem_topdown.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tp_InfoTestitem_topdown.Controls.Add(this.tp_info_topdown, 0, 0);
            this.tp_InfoTestitem_topdown.Controls.Add(this.tbc_test, 0, 1);
            this.tp_InfoTestitem_topdown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tp_InfoTestitem_topdown.Location = new System.Drawing.Point(191, 3);
            this.tp_InfoTestitem_topdown.Name = "tp_InfoTestitem_topdown";
            this.tp_InfoTestitem_topdown.RowCount = 2;
            this.tp_InfoTestitem_topdown.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 37.5F));
            this.tp_InfoTestitem_topdown.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 62.5F));
            this.tp_InfoTestitem_topdown.Size = new System.Drawing.Size(528, 551);
            this.tp_InfoTestitem_topdown.TabIndex = 0;
            // 
            // tp_info_topdown
            // 
            this.tp_info_topdown.ColumnCount = 1;
            this.tp_info_topdown.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tp_info_topdown.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tp_info_topdown.Controls.Add(this.tp_info_leftright, 0, 1);
            this.tp_info_topdown.Controls.Add(this.tp_title_topdown, 0, 0);
            this.tp_info_topdown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tp_info_topdown.Location = new System.Drawing.Point(3, 3);
            this.tp_info_topdown.Name = "tp_info_topdown";
            this.tp_info_topdown.RowCount = 2;
            this.tp_info_topdown.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 24.23398F));
            this.tp_info_topdown.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75.76601F));
            this.tp_info_topdown.Size = new System.Drawing.Size(522, 200);
            this.tp_info_topdown.TabIndex = 0;
            // 
            // tp_info_leftright
            // 
            this.tp_info_leftright.ColumnCount = 2;
            this.tp_info_leftright.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 91.70985F));
            this.tp_info_leftright.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.290155F));
            this.tp_info_leftright.Controls.Add(this.tableLayoutPanel_main, 0, 0);
            this.tp_info_leftright.Controls.Add(this.tp_lightpanel, 1, 0);
            this.tp_info_leftright.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tp_info_leftright.Location = new System.Drawing.Point(3, 51);
            this.tp_info_leftright.Name = "tp_info_leftright";
            this.tp_info_leftright.RowCount = 1;
            this.tp_info_leftright.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tp_info_leftright.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 146F));
            this.tp_info_leftright.Size = new System.Drawing.Size(516, 146);
            this.tp_info_leftright.TabIndex = 0;
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel_main.ColumnCount = 2;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65.37531F));
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34.6247F));
            this.tableLayoutPanel_main.Controls.Add(this.label_result, 1, 0);
            this.tableLayoutPanel_main.Controls.Add(this.tp_err_time_topdown, 1, 1);
            this.tableLayoutPanel_main.Controls.Add(this.tableLayoutPanel_sn, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.richTextBox_info, 0, 1);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 2;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 57.07965F));
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42.92035F));
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(469, 142);
            this.tableLayoutPanel_main.TabIndex = 3;
            // 
            // label_result
            // 
            this.label_result.AutoSize = true;
            this.label_result.BackColor = System.Drawing.Color.Black;
            this.label_result.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_result.Font = new System.Drawing.Font("Arial", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_result.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label_result.Location = new System.Drawing.Point(308, 0);
            this.label_result.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_result.Name = "label_result";
            this.label_result.Size = new System.Drawing.Size(159, 81);
            this.label_result.TabIndex = 2;
            this.label_result.Text = "PASS/FAIL";
            this.label_result.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_result.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label_result_MouseDown);
            // 
            // tp_err_time_topdown
            // 
            this.tp_err_time_topdown.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tp_err_time_topdown.ColumnCount = 1;
            this.tp_err_time_topdown.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tp_err_time_topdown.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tp_err_time_topdown.Controls.Add(this.label_errorcode, 0, 0);
            this.tp_err_time_topdown.Controls.Add(this.tableLayoutPanel_time_home, 0, 1);
            this.tp_err_time_topdown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tp_err_time_topdown.Location = new System.Drawing.Point(308, 83);
            this.tp_err_time_topdown.Margin = new System.Windows.Forms.Padding(2);
            this.tp_err_time_topdown.Name = "tp_err_time_topdown";
            this.tp_err_time_topdown.RowCount = 2;
            this.tp_err_time_topdown.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tp_err_time_topdown.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tp_err_time_topdown.Size = new System.Drawing.Size(159, 57);
            this.tp_err_time_topdown.TabIndex = 0;
            // 
            // label_errorcode
            // 
            this.label_errorcode.AutoSize = true;
            this.label_errorcode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_errorcode.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_errorcode.Location = new System.Drawing.Point(3, 1);
            this.label_errorcode.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_errorcode.Name = "label_errorcode";
            this.label_errorcode.Size = new System.Drawing.Size(153, 27);
            this.label_errorcode.TabIndex = 0;
            this.label_errorcode.Text = "ErrorCode";
            this.label_errorcode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel_time_home
            // 
            this.tableLayoutPanel_time_home.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel_time_home.ColumnCount = 1;
            this.tableLayoutPanel_time_home.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_time_home.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel_time_home.Controls.Add(this.label_elapsetime, 0, 0);
            this.tableLayoutPanel_time_home.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_time_home.Location = new System.Drawing.Point(4, 32);
            this.tableLayoutPanel_time_home.Name = "tableLayoutPanel_time_home";
            this.tableLayoutPanel_time_home.RowCount = 1;
            this.tableLayoutPanel_time_home.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_time_home.Size = new System.Drawing.Size(151, 21);
            this.tableLayoutPanel_time_home.TabIndex = 1;
            // 
            // label_elapsetime
            // 
            this.label_elapsetime.AutoSize = true;
            this.label_elapsetime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_elapsetime.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_elapsetime.Location = new System.Drawing.Point(3, 1);
            this.label_elapsetime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_elapsetime.Name = "label_elapsetime";
            this.label_elapsetime.Size = new System.Drawing.Size(145, 19);
            this.label_elapsetime.TabIndex = 2;
            this.label_elapsetime.Text = "23s";
            this.label_elapsetime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel_sn
            // 
            this.tableLayoutPanel_sn.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel_sn.ColumnCount = 1;
            this.tableLayoutPanel_sn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_sn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel_sn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_sn.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel_sn.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_sn.Name = "tableLayoutPanel_sn";
            this.tableLayoutPanel_sn.RowCount = 1;
            this.tableLayoutPanel_sn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_sn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 76F));
            this.tableLayoutPanel_sn.Size = new System.Drawing.Size(302, 77);
            this.tableLayoutPanel_sn.TabIndex = 3;
            // 
            // richTextBox_info
            // 
            this.richTextBox_info.BackColor = System.Drawing.Color.White;
            this.richTextBox_info.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox_info.Location = new System.Drawing.Point(2, 83);
            this.richTextBox_info.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox_info.Name = "richTextBox_info";
            this.richTextBox_info.ReadOnly = true;
            this.richTextBox_info.Size = new System.Drawing.Size(302, 57);
            this.richTextBox_info.TabIndex = 4;
            this.richTextBox_info.Text = "";
            // 
            // tp_lightpanel
            // 
            this.tp_lightpanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tp_lightpanel.ColumnCount = 1;
            this.tp_lightpanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tp_lightpanel.Controls.Add(this.HomeBtn, 0, 3);
            this.tp_lightpanel.Controls.Add(this.panelGreen, 0, 2);
            this.tp_lightpanel.Controls.Add(this.panelYellow, 0, 1);
            this.tp_lightpanel.Controls.Add(this.panelRed, 0, 0);
            this.tp_lightpanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tp_lightpanel.Location = new System.Drawing.Point(475, 2);
            this.tp_lightpanel.Margin = new System.Windows.Forms.Padding(2);
            this.tp_lightpanel.Name = "tp_lightpanel";
            this.tp_lightpanel.RowCount = 4;
            this.tp_lightpanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tp_lightpanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tp_lightpanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tp_lightpanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tp_lightpanel.Size = new System.Drawing.Size(39, 142);
            this.tp_lightpanel.TabIndex = 4;
            // 
            // HomeBtn
            // 
            this.HomeBtn.BackColor = System.Drawing.Color.Transparent;
            this.HomeBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HomeBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveCaption;
            this.HomeBtn.FlatAppearance.BorderSize = 0;
            this.HomeBtn.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.HomeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.HomeBtn.Font = new System.Drawing.Font("微軟正黑體", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.HomeBtn.ImageIndex = 8;
            this.HomeBtn.ImageList = this.imageList2;
            this.HomeBtn.Location = new System.Drawing.Point(3, 108);
            this.HomeBtn.Margin = new System.Windows.Forms.Padding(2);
            this.HomeBtn.Name = "HomeBtn";
            this.HomeBtn.Size = new System.Drawing.Size(48, 31);
            this.HomeBtn.TabIndex = 38;
            this.HomeBtn.UseVisualStyleBackColor = false;
            this.HomeBtn.Visible = false;
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
            this.imageList2.Images.SetKeyName(8, "workflow.png");
            // 
            // panelGreen
            // 
            this.panelGreen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelGreen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGreen.Location = new System.Drawing.Point(3, 73);
            this.panelGreen.Margin = new System.Windows.Forms.Padding(2);
            this.panelGreen.Name = "panelGreen";
            this.panelGreen.Size = new System.Drawing.Size(48, 30);
            this.panelGreen.TabIndex = 2;
            // 
            // panelYellow
            // 
            this.panelYellow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelYellow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelYellow.Location = new System.Drawing.Point(3, 38);
            this.panelYellow.Margin = new System.Windows.Forms.Padding(2);
            this.panelYellow.Name = "panelYellow";
            this.panelYellow.Size = new System.Drawing.Size(48, 30);
            this.panelYellow.TabIndex = 1;
            // 
            // panelRed
            // 
            this.panelRed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRed.Location = new System.Drawing.Point(3, 3);
            this.panelRed.Margin = new System.Windows.Forms.Padding(2);
            this.panelRed.Name = "panelRed";
            this.panelRed.Size = new System.Drawing.Size(48, 30);
            this.panelRed.TabIndex = 0;
            // 
            // tp_title_topdown
            // 
            this.tp_title_topdown.ColumnCount = 1;
            this.tp_title_topdown.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tp_title_topdown.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tp_title_topdown.Controls.Add(this.label_description, 0, 1);
            this.tp_title_topdown.Controls.Add(this.pictureBox_titlebarcode, 0, 0);
            this.tp_title_topdown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tp_title_topdown.Location = new System.Drawing.Point(3, 3);
            this.tp_title_topdown.Name = "tp_title_topdown";
            this.tp_title_topdown.RowCount = 2;
            this.tp_title_topdown.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 29.87013F));
            this.tp_title_topdown.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70.12987F));
            this.tp_title_topdown.Size = new System.Drawing.Size(516, 42);
            this.tp_title_topdown.TabIndex = 1;
            // 
            // label_description
            // 
            this.label_description.AutoSize = true;
            this.label_description.BackColor = System.Drawing.Color.MidnightBlue;
            this.label_description.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_description.Font = new System.Drawing.Font("Arial", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_description.ForeColor = System.Drawing.Color.White;
            this.label_description.Location = new System.Drawing.Point(3, 12);
            this.label_description.Name = "label_description";
            this.label_description.Size = new System.Drawing.Size(510, 30);
            this.label_description.TabIndex = 2;
            this.label_description.Text = "Title";
            this.label_description.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox_titlebarcode
            // 
            this.pictureBox_titlebarcode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_titlebarcode.Location = new System.Drawing.Point(3, 3);
            this.pictureBox_titlebarcode.Name = "pictureBox_titlebarcode";
            this.pictureBox_titlebarcode.Size = new System.Drawing.Size(510, 6);
            this.pictureBox_titlebarcode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox_titlebarcode.TabIndex = 0;
            this.pictureBox_titlebarcode.TabStop = false;
            // 
            // tbc_test
            // 
            this.tbc_test.Controls.Add(this.tabPage_dataitem);
            this.tbc_test.Controls.Add(this.tabPage_image);
            this.tbc_test.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbc_test.ItemSize = new System.Drawing.Size(0, 23);
            this.tbc_test.Location = new System.Drawing.Point(3, 209);
            this.tbc_test.Name = "tbc_test";
            this.tbc_test.SelectedIndex = 0;
            this.tbc_test.Size = new System.Drawing.Size(522, 339);
            this.tbc_test.TabIndex = 1;
            // 
            // tabPage_dataitem
            // 
            this.tabPage_dataitem.Controls.Add(this.dg_testitem);
            this.tabPage_dataitem.Location = new System.Drawing.Point(4, 27);
            this.tabPage_dataitem.Name = "tabPage_dataitem";
            this.tabPage_dataitem.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_dataitem.Size = new System.Drawing.Size(514, 308);
            this.tabPage_dataitem.TabIndex = 0;
            this.tabPage_dataitem.Text = "DataItem";
            this.tabPage_dataitem.UseVisualStyleBackColor = true;
            // 
            // dg_testitem
            // 
            this.dg_testitem.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dg_testitem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dg_testitem.Location = new System.Drawing.Point(3, 3);
            this.dg_testitem.Name = "dg_testitem";
            this.dg_testitem.RowHeadersWidth = 51;
            this.dg_testitem.RowTemplate.Height = 27;
            this.dg_testitem.Size = new System.Drawing.Size(508, 302);
            this.dg_testitem.TabIndex = 2;
            // 
            // tabPage_image
            // 
            this.tabPage_image.Controls.Add(this.pictureBox_image);
            this.tabPage_image.Location = new System.Drawing.Point(4, 27);
            this.tabPage_image.Name = "tabPage_image";
            this.tabPage_image.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_image.Size = new System.Drawing.Size(514, 308);
            this.tabPage_image.TabIndex = 1;
            this.tabPage_image.Text = "Image";
            this.tabPage_image.UseVisualStyleBackColor = true;
            // 
            // pictureBox_image
            // 
            this.pictureBox_image.BackColor = System.Drawing.Color.Black;
            this.pictureBox_image.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox_image.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_image.Location = new System.Drawing.Point(3, 3);
            this.pictureBox_image.Name = "pictureBox_image";
            this.pictureBox_image.Size = new System.Drawing.Size(508, 302);
            this.pictureBox_image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_image.TabIndex = 0;
            this.pictureBox_image.TabStop = false;
            // 
            // tbc_scripttree
            // 
            this.tbc_scripttree.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tbc_scripttree.Controls.Add(this.tabPage_processtree);
            this.tbc_scripttree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbc_scripttree.ItemSize = new System.Drawing.Size(0, 1);
            this.tbc_scripttree.Location = new System.Drawing.Point(3, 3);
            this.tbc_scripttree.Name = "tbc_scripttree";
            this.tbc_scripttree.SelectedIndex = 0;
            this.tbc_scripttree.Size = new System.Drawing.Size(182, 551);
            this.tbc_scripttree.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tbc_scripttree.TabIndex = 1;
            // 
            // tabPage_processtree
            // 
            this.tabPage_processtree.Controls.Add(this.proTreeView_testitem);
            this.tabPage_processtree.Location = new System.Drawing.Point(4, 5);
            this.tabPage_processtree.Name = "tabPage_processtree";
            this.tabPage_processtree.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_processtree.Size = new System.Drawing.Size(174, 542);
            this.tabPage_processtree.TabIndex = 0;
            this.tabPage_processtree.Text = "流程";
            this.tabPage_processtree.UseVisualStyleBackColor = true;
            // 
            // proTreeView_testitem
            // 
            this.proTreeView_testitem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.proTreeView_testitem.Location = new System.Drawing.Point(3, 3);
            this.proTreeView_testitem.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.proTreeView_testitem.Name = "proTreeView_testitem";
            this.proTreeView_testitem.Size = new System.Drawing.Size(168, 536);
            this.proTreeView_testitem.TabIndex = 4;
            // 
            // Dashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tp_main_topdown);
            this.Name = "Dashboard";
            this.Size = new System.Drawing.Size(728, 701);
            this.tp_main_topdown.ResumeLayout(false);
            this.tabControl_log.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tp_main_leftright.ResumeLayout(false);
            this.tp_InfoTestitem_topdown.ResumeLayout(false);
            this.tp_info_topdown.ResumeLayout(false);
            this.tp_info_leftright.ResumeLayout(false);
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.tp_err_time_topdown.ResumeLayout(false);
            this.tp_err_time_topdown.PerformLayout();
            this.tableLayoutPanel_time_home.ResumeLayout(false);
            this.tableLayoutPanel_time_home.PerformLayout();
            this.tp_lightpanel.ResumeLayout(false);
            this.tp_title_topdown.ResumeLayout(false);
            this.tp_title_topdown.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_titlebarcode)).EndInit();
            this.tbc_test.ResumeLayout(false);
            this.tabPage_dataitem.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dg_testitem)).EndInit();
            this.tabPage_image.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_image)).EndInit();
            this.tbc_scripttree.ResumeLayout(false);
            this.tabPage_processtree.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tp_main_topdown;
        private System.Windows.Forms.TableLayoutPanel tp_main_leftright;
        private System.Windows.Forms.TableLayoutPanel tp_InfoTestitem_topdown;
        private System.Windows.Forms.TableLayoutPanel tp_info_topdown;
        private System.Windows.Forms.TabControl tbc_scripttree;
        private System.Windows.Forms.TabPage tabPage_processtree;
        private ProTreeView.ProTreeView proTreeView_testitem;
        private System.Windows.Forms.TabControl tabControl_log;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox richTextBox_dutLog;
        private System.Windows.Forms.TableLayoutPanel tp_info_leftright;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.Label label_result;
        private System.Windows.Forms.TableLayoutPanel tp_err_time_topdown;
        private System.Windows.Forms.Label label_errorcode;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_time_home;
        private System.Windows.Forms.Label label_elapsetime;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_sn;
        private System.Windows.Forms.RichTextBox richTextBox_info;
        private System.Windows.Forms.TableLayoutPanel tp_title_topdown;
        private System.Windows.Forms.Label label_description;
        private System.Windows.Forms.PictureBox pictureBox_titlebarcode;
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.TableLayoutPanel tp_lightpanel;
        private System.Windows.Forms.Panel panelGreen;
        private System.Windows.Forms.Panel panelYellow;
        private System.Windows.Forms.Panel panelRed;
        private System.Windows.Forms.Button HomeBtn;
        private System.Windows.Forms.TabControl tbc_test;
        private System.Windows.Forms.TabPage tabPage_dataitem;
        private System.Windows.Forms.DataGridView dg_testitem;
        private System.Windows.Forms.TabPage tabPage_image;
        private System.Windows.Forms.PictureBox pictureBox_image;
    }
}
