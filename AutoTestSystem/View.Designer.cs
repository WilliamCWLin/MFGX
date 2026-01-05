namespace AutoTestSystem
{
    partial class View
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

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip_video_status = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_videostatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.comboBox_url = new System.Windows.Forms.ComboBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.videoPictureBox = new System.Windows.Forms.PictureBox();
            this.JSONData_comboBox = new System.Windows.Forms.ComboBox();
            this.Action_Status = new System.Windows.Forms.Label();
            this.Send_CGIcmd = new System.Windows.Forms.Button();
            this.CGI_CMDcomboBox1 = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.Y_ratio_box = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.X_ratio_box = new System.Windows.Forms.TextBox();
            this.Save_BTN = new System.Windows.Forms.Button();
            this.Reset_BTN = new System.Windows.Forms.Button();
            this.Wide_BTN = new System.Windows.Forms.Button();
            this.Tele_BTN = new System.Windows.Forms.Button();
            this.comboBox_CGI = new System.Windows.Forms.ComboBox();
            this.comboBox_plcdevices = new System.Windows.Forms.ComboBox();
            this.CtrlGRP = new System.Windows.Forms.GroupBox();
            this.U_Shift = new System.Windows.Forms.TextBox();
            this.Z_Shift = new System.Windows.Forms.TextBox();
            this.Y_Shift = new System.Windows.Forms.TextBox();
            this.X_Shift = new System.Windows.Forms.TextBox();
            this.XY_Change = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.U_N_BTN = new System.Windows.Forms.Button();
            this.Z_N_BTN = new System.Windows.Forms.Button();
            this.U_P_BTN = new System.Windows.Forms.Button();
            this.Z_P_BTN = new System.Windows.Forms.Button();
            this.comboBox_Degree = new System.Windows.Forms.ComboBox();
            this.Y_P_BTN = new System.Windows.Forms.Button();
            this.Y_N_BTN = new System.Windows.Forms.Button();
            this.X_N_BTN = new System.Windows.Forms.Button();
            this.YrevCB = new System.Windows.Forms.CheckBox();
            this.XrevCB = new System.Windows.Forms.CheckBox();
            this.X_P_BTN = new System.Windows.Forms.Button();
            this.textBox_f = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_h = new System.Windows.Forms.TextBox();
            this.textBox_w = new System.Windows.Forms.TextBox();
            this.Action = new System.Windows.Forms.Button();
            this.richTextBox_SEPin = new System.Windows.Forms.RichTextBox();
            this.DisplayBTN = new System.Windows.Forms.Button();
            this.StopBTN = new System.Windows.Forms.Button();
            this.PlayBTN = new System.Windows.Forms.Button();
            this.TeachBTN = new System.Windows.Forms.Button();
            this.statusStrip_video_status.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.videoPictureBox)).BeginInit();
            this.CtrlGRP.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip_video_status
            // 
            this.statusStrip_video_status.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip_video_status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_videostatus});
            this.statusStrip_video_status.Location = new System.Drawing.Point(0, 796);
            this.statusStrip_video_status.Name = "statusStrip_video_status";
            this.statusStrip_video_status.Size = new System.Drawing.Size(1540, 22);
            this.statusStrip_video_status.TabIndex = 9;
            this.statusStrip_video_status.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_videostatus
            // 
            this.toolStripStatusLabel_videostatus.Name = "toolStripStatusLabel_videostatus";
            this.toolStripStatusLabel_videostatus.Size = new System.Drawing.Size(72, 17);
            this.toolStripStatusLabel_videostatus.Text = "Video Status";
            // 
            // comboBox_url
            // 
            this.comboBox_url.FormattingEnabled = true;
            this.comboBox_url.Items.AddRange(new object[] {
            "rtsp://10.0.0.2/stream1",
            "rtsp://127.0.0.1:8900/live"});
            this.comboBox_url.Location = new System.Drawing.Point(28, 12);
            this.comboBox_url.Name = "comboBox_url";
            this.comboBox_url.Size = new System.Drawing.Size(356, 21);
            this.comboBox_url.TabIndex = 10;
            this.comboBox_url.UseWaitCursor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.videoPictureBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.TeachBTN);
            this.splitContainer1.Panel2.Controls.Add(this.JSONData_comboBox);
            this.splitContainer1.Panel2.Controls.Add(this.Action_Status);
            this.splitContainer1.Panel2.Controls.Add(this.Send_CGIcmd);
            this.splitContainer1.Panel2.Controls.Add(this.CGI_CMDcomboBox1);
            this.splitContainer1.Panel2.Controls.Add(this.label9);
            this.splitContainer1.Panel2.Controls.Add(this.Y_ratio_box);
            this.splitContainer1.Panel2.Controls.Add(this.label8);
            this.splitContainer1.Panel2.Controls.Add(this.X_ratio_box);
            this.splitContainer1.Panel2.Controls.Add(this.Save_BTN);
            this.splitContainer1.Panel2.Controls.Add(this.Reset_BTN);
            this.splitContainer1.Panel2.Controls.Add(this.Wide_BTN);
            this.splitContainer1.Panel2.Controls.Add(this.Tele_BTN);
            this.splitContainer1.Panel2.Controls.Add(this.comboBox_CGI);
            this.splitContainer1.Panel2.Controls.Add(this.comboBox_plcdevices);
            this.splitContainer1.Panel2.Controls.Add(this.CtrlGRP);
            this.splitContainer1.Panel2.Controls.Add(this.textBox_f);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.textBox_h);
            this.splitContainer1.Panel2.Controls.Add(this.textBox_w);
            this.splitContainer1.Panel2.Controls.Add(this.Action);
            this.splitContainer1.Panel2.Controls.Add(this.richTextBox_SEPin);
            this.splitContainer1.Panel2.Controls.Add(this.DisplayBTN);
            this.splitContainer1.Panel2.Controls.Add(this.StopBTN);
            this.splitContainer1.Panel2.Controls.Add(this.PlayBTN);
            this.splitContainer1.Panel2.Controls.Add(this.comboBox_url);
            this.splitContainer1.Panel2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.splitContainer1.Size = new System.Drawing.Size(1540, 796);
            this.splitContainer1.SplitterDistance = 1032;
            this.splitContainer1.TabIndex = 11;
            // 
            // videoPictureBox
            // 
            this.videoPictureBox.BackColor = System.Drawing.Color.Black;
            this.videoPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.videoPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoPictureBox.Location = new System.Drawing.Point(0, 0);
            this.videoPictureBox.Name = "videoPictureBox";
            this.videoPictureBox.Size = new System.Drawing.Size(1032, 796);
            this.videoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.videoPictureBox.TabIndex = 2;
            this.videoPictureBox.TabStop = false;
            // 
            // JSONData_comboBox
            // 
            this.JSONData_comboBox.FormattingEnabled = true;
            this.JSONData_comboBox.Items.AddRange(new object[] {
            "{\"mode\":\"Wide\"}",
            "{\"mode\":\"Tele\"}"});
            this.JSONData_comboBox.Location = new System.Drawing.Point(162, 38);
            this.JSONData_comboBox.Name = "JSONData_comboBox";
            this.JSONData_comboBox.Size = new System.Drawing.Size(108, 21);
            this.JSONData_comboBox.TabIndex = 39;
            // 
            // Action_Status
            // 
            this.Action_Status.AutoSize = true;
            this.Action_Status.Location = new System.Drawing.Point(97, 292);
            this.Action_Status.Name = "Action_Status";
            this.Action_Status.Size = new System.Drawing.Size(38, 13);
            this.Action_Status.TabIndex = 38;
            this.Action_Status.Text = "Ready";
            // 
            // Send_CGIcmd
            // 
            this.Send_CGIcmd.Location = new System.Drawing.Point(299, 37);
            this.Send_CGIcmd.Name = "Send_CGIcmd";
            this.Send_CGIcmd.Size = new System.Drawing.Size(85, 27);
            this.Send_CGIcmd.TabIndex = 36;
            this.Send_CGIcmd.Text = "Send CGI cmd";
            this.Send_CGIcmd.UseVisualStyleBackColor = true;
            this.Send_CGIcmd.Click += new System.EventHandler(this.Send_CGIcmd_Click);
            // 
            // CGI_CMDcomboBox1
            // 
            this.CGI_CMDcomboBox1.FormattingEnabled = true;
            this.CGI_CMDcomboBox1.Items.AddRange(new object[] {
            "/mp/AF",
            "/mp/Focus",
            "/mp/Zoom"});
            this.CGI_CMDcomboBox1.Location = new System.Drawing.Point(28, 38);
            this.CGI_CMDcomboBox1.Name = "CGI_CMDcomboBox1";
            this.CGI_CMDcomboBox1.Size = new System.Drawing.Size(108, 21);
            this.CGI_CMDcomboBox1.TabIndex = 35;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 741);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(40, 13);
            this.label9.TabIndex = 34;
            this.label9.Text = "Y_ratio";
            // 
            // Y_ratio_box
            // 
            this.Y_ratio_box.Location = new System.Drawing.Point(2, 762);
            this.Y_ratio_box.Name = "Y_ratio_box";
            this.Y_ratio_box.Size = new System.Drawing.Size(140, 20);
            this.Y_ratio_box.TabIndex = 33;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 688);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(40, 13);
            this.label8.TabIndex = 32;
            this.label8.Text = "X_ratio";
            // 
            // X_ratio_box
            // 
            this.X_ratio_box.Location = new System.Drawing.Point(2, 709);
            this.X_ratio_box.Name = "X_ratio_box";
            this.X_ratio_box.Size = new System.Drawing.Size(140, 20);
            this.X_ratio_box.TabIndex = 31;
            // 
            // Save_BTN
            // 
            this.Save_BTN.Location = new System.Drawing.Point(72, 662);
            this.Save_BTN.Name = "Save_BTN";
            this.Save_BTN.Size = new System.Drawing.Size(70, 23);
            this.Save_BTN.TabIndex = 30;
            this.Save_BTN.Text = "Save";
            this.Save_BTN.UseVisualStyleBackColor = true;
            this.Save_BTN.Click += new System.EventHandler(this.Save_BTN_Click);
            // 
            // Reset_BTN
            // 
            this.Reset_BTN.Location = new System.Drawing.Point(2, 662);
            this.Reset_BTN.Name = "Reset_BTN";
            this.Reset_BTN.Size = new System.Drawing.Size(70, 23);
            this.Reset_BTN.TabIndex = 29;
            this.Reset_BTN.Text = "Reset";
            this.Reset_BTN.UseVisualStyleBackColor = true;
            this.Reset_BTN.Click += new System.EventHandler(this.Reset_BTN_Click);
            // 
            // Wide_BTN
            // 
            this.Wide_BTN.Location = new System.Drawing.Point(72, 633);
            this.Wide_BTN.Name = "Wide_BTN";
            this.Wide_BTN.Size = new System.Drawing.Size(70, 23);
            this.Wide_BTN.TabIndex = 28;
            this.Wide_BTN.Text = "Wide";
            this.Wide_BTN.UseVisualStyleBackColor = true;
            this.Wide_BTN.Click += new System.EventHandler(this.Wide_BTN_Click);
            // 
            // Tele_BTN
            // 
            this.Tele_BTN.Location = new System.Drawing.Point(2, 633);
            this.Tele_BTN.Name = "Tele_BTN";
            this.Tele_BTN.Size = new System.Drawing.Size(70, 23);
            this.Tele_BTN.TabIndex = 27;
            this.Tele_BTN.Text = "Tele";
            this.Tele_BTN.UseVisualStyleBackColor = true;
            this.Tele_BTN.Click += new System.EventHandler(this.Tele_BTN_Click);
            // 
            // comboBox_CGI
            // 
            this.comboBox_CGI.FormattingEnabled = true;
            this.comboBox_CGI.Location = new System.Drawing.Point(390, 38);
            this.comboBox_CGI.Name = "comboBox_CGI";
            this.comboBox_CGI.Size = new System.Drawing.Size(121, 21);
            this.comboBox_CGI.TabIndex = 26;
            // 
            // comboBox_plcdevices
            // 
            this.comboBox_plcdevices.FormattingEnabled = true;
            this.comboBox_plcdevices.Location = new System.Drawing.Point(390, 12);
            this.comboBox_plcdevices.Name = "comboBox_plcdevices";
            this.comboBox_plcdevices.Size = new System.Drawing.Size(121, 21);
            this.comboBox_plcdevices.TabIndex = 26;
            // 
            // CtrlGRP
            // 
            this.CtrlGRP.BackColor = System.Drawing.Color.Chocolate;
            this.CtrlGRP.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.CtrlGRP.Controls.Add(this.U_Shift);
            this.CtrlGRP.Controls.Add(this.Z_Shift);
            this.CtrlGRP.Controls.Add(this.Y_Shift);
            this.CtrlGRP.Controls.Add(this.X_Shift);
            this.CtrlGRP.Controls.Add(this.XY_Change);
            this.CtrlGRP.Controls.Add(this.label7);
            this.CtrlGRP.Controls.Add(this.label6);
            this.CtrlGRP.Controls.Add(this.label5);
            this.CtrlGRP.Controls.Add(this.label4);
            this.CtrlGRP.Controls.Add(this.U_N_BTN);
            this.CtrlGRP.Controls.Add(this.Z_N_BTN);
            this.CtrlGRP.Controls.Add(this.U_P_BTN);
            this.CtrlGRP.Controls.Add(this.Z_P_BTN);
            this.CtrlGRP.Controls.Add(this.comboBox_Degree);
            this.CtrlGRP.Controls.Add(this.Y_P_BTN);
            this.CtrlGRP.Controls.Add(this.Y_N_BTN);
            this.CtrlGRP.Controls.Add(this.X_N_BTN);
            this.CtrlGRP.Controls.Add(this.YrevCB);
            this.CtrlGRP.Controls.Add(this.XrevCB);
            this.CtrlGRP.Controls.Add(this.X_P_BTN);
            this.CtrlGRP.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.CtrlGRP.Location = new System.Drawing.Point(2, 331);
            this.CtrlGRP.Name = "CtrlGRP";
            this.CtrlGRP.Size = new System.Drawing.Size(134, 296);
            this.CtrlGRP.TabIndex = 25;
            this.CtrlGRP.TabStop = false;
            this.CtrlGRP.Text = "DirectionCtrl";
            // 
            // U_Shift
            // 
            this.U_Shift.Location = new System.Drawing.Point(98, 249);
            this.U_Shift.Name = "U_Shift";
            this.U_Shift.Size = new System.Drawing.Size(29, 20);
            this.U_Shift.TabIndex = 43;
            // 
            // Z_Shift
            // 
            this.Z_Shift.Location = new System.Drawing.Point(98, 213);
            this.Z_Shift.Name = "Z_Shift";
            this.Z_Shift.Size = new System.Drawing.Size(29, 20);
            this.Z_Shift.TabIndex = 42;
            // 
            // Y_Shift
            // 
            this.Y_Shift.Location = new System.Drawing.Point(98, 177);
            this.Y_Shift.Name = "Y_Shift";
            this.Y_Shift.Size = new System.Drawing.Size(29, 20);
            this.Y_Shift.TabIndex = 41;
            // 
            // X_Shift
            // 
            this.X_Shift.Location = new System.Drawing.Point(98, 141);
            this.X_Shift.Name = "X_Shift";
            this.X_Shift.Size = new System.Drawing.Size(29, 20);
            this.X_Shift.TabIndex = 40;
            // 
            // XY_Change
            // 
            this.XY_Change.AutoSize = true;
            this.XY_Change.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.XY_Change.Location = new System.Drawing.Point(4, 79);
            this.XY_Change.Name = "XY_Change";
            this.XY_Change.Size = new System.Drawing.Size(105, 20);
            this.XY_Change.TabIndex = 39;
            this.XY_Change.Text = "XY exchange";
            this.XY_Change.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(38, 255);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(15, 13);
            this.label7.TabIndex = 38;
            this.label7.Text = "U";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(39, 219);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(20, 18);
            this.label6.TabIndex = 37;
            this.label6.Text = "Z";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(38, 183);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 13);
            this.label5.TabIndex = 36;
            this.label5.Text = "Y";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(38, 147);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 13);
            this.label4.TabIndex = 35;
            this.label4.Text = "X";
            // 
            // U_N_BTN
            // 
            this.U_N_BTN.Location = new System.Drawing.Point(4, 249);
            this.U_N_BTN.Name = "U_N_BTN";
            this.U_N_BTN.Size = new System.Drawing.Size(29, 29);
            this.U_N_BTN.TabIndex = 34;
            this.U_N_BTN.Text = "←";
            this.U_N_BTN.UseVisualStyleBackColor = true;
            this.U_N_BTN.Click += new System.EventHandler(this.U_N_BTN_Click);
            // 
            // Z_N_BTN
            // 
            this.Z_N_BTN.Location = new System.Drawing.Point(4, 213);
            this.Z_N_BTN.Name = "Z_N_BTN";
            this.Z_N_BTN.Size = new System.Drawing.Size(29, 29);
            this.Z_N_BTN.TabIndex = 33;
            this.Z_N_BTN.Text = "←";
            this.Z_N_BTN.UseVisualStyleBackColor = true;
            this.Z_N_BTN.Click += new System.EventHandler(this.Z_N_BTN_Click);
            // 
            // U_P_BTN
            // 
            this.U_P_BTN.Location = new System.Drawing.Point(61, 249);
            this.U_P_BTN.Name = "U_P_BTN";
            this.U_P_BTN.Size = new System.Drawing.Size(29, 29);
            this.U_P_BTN.TabIndex = 32;
            this.U_P_BTN.Text = "→";
            this.U_P_BTN.UseVisualStyleBackColor = true;
            this.U_P_BTN.Click += new System.EventHandler(this.U_P_BTN_Click);
            // 
            // Z_P_BTN
            // 
            this.Z_P_BTN.Location = new System.Drawing.Point(61, 213);
            this.Z_P_BTN.Name = "Z_P_BTN";
            this.Z_P_BTN.Size = new System.Drawing.Size(29, 29);
            this.Z_P_BTN.TabIndex = 31;
            this.Z_P_BTN.Text = "→";
            this.Z_P_BTN.UseVisualStyleBackColor = true;
            this.Z_P_BTN.Click += new System.EventHandler(this.Z_P_BTN_Click);
            // 
            // comboBox_Degree
            // 
            this.comboBox_Degree.FormattingEnabled = true;
            this.comboBox_Degree.Items.AddRange(new object[] {
            "0.05",
            "0.1",
            "0.5",
            "1",
            "2"});
            this.comboBox_Degree.Location = new System.Drawing.Point(4, 109);
            this.comboBox_Degree.Name = "comboBox_Degree";
            this.comboBox_Degree.Size = new System.Drawing.Size(89, 21);
            this.comboBox_Degree.TabIndex = 30;
            this.comboBox_Degree.TextChanged += new System.EventHandler(this.comboBox_Degree_TextChange);
            // 
            // Y_P_BTN
            // 
            this.Y_P_BTN.Location = new System.Drawing.Point(61, 177);
            this.Y_P_BTN.Name = "Y_P_BTN";
            this.Y_P_BTN.Size = new System.Drawing.Size(29, 29);
            this.Y_P_BTN.TabIndex = 29;
            this.Y_P_BTN.Text = "→";
            this.Y_P_BTN.UseVisualStyleBackColor = true;
            this.Y_P_BTN.Click += new System.EventHandler(this.Y_P_BTN_Click);
            // 
            // Y_N_BTN
            // 
            this.Y_N_BTN.Location = new System.Drawing.Point(4, 177);
            this.Y_N_BTN.Name = "Y_N_BTN";
            this.Y_N_BTN.Size = new System.Drawing.Size(29, 29);
            this.Y_N_BTN.TabIndex = 28;
            this.Y_N_BTN.Text = "←";
            this.Y_N_BTN.UseVisualStyleBackColor = true;
            this.Y_N_BTN.Click += new System.EventHandler(this.Y_N_BTN_Click);
            // 
            // X_N_BTN
            // 
            this.X_N_BTN.Location = new System.Drawing.Point(4, 141);
            this.X_N_BTN.Name = "X_N_BTN";
            this.X_N_BTN.Size = new System.Drawing.Size(29, 29);
            this.X_N_BTN.TabIndex = 27;
            this.X_N_BTN.Text = "←";
            this.X_N_BTN.UseVisualStyleBackColor = true;
            this.X_N_BTN.Click += new System.EventHandler(this.X_N_BTN_Click);
            // 
            // YrevCB
            // 
            this.YrevCB.AutoSize = true;
            this.YrevCB.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.YrevCB.Location = new System.Drawing.Point(4, 51);
            this.YrevCB.Name = "YrevCB";
            this.YrevCB.Size = new System.Drawing.Size(88, 20);
            this.YrevCB.TabIndex = 26;
            this.YrevCB.Text = "Y_reverse";
            this.YrevCB.UseVisualStyleBackColor = true;
            // 
            // XrevCB
            // 
            this.XrevCB.AutoSize = true;
            this.XrevCB.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.XrevCB.Location = new System.Drawing.Point(4, 23);
            this.XrevCB.Name = "XrevCB";
            this.XrevCB.Size = new System.Drawing.Size(87, 20);
            this.XrevCB.TabIndex = 25;
            this.XrevCB.Text = "X_reverse";
            this.XrevCB.UseVisualStyleBackColor = true;
            // 
            // X_P_BTN
            // 
            this.X_P_BTN.Location = new System.Drawing.Point(61, 141);
            this.X_P_BTN.Name = "X_P_BTN";
            this.X_P_BTN.Size = new System.Drawing.Size(29, 29);
            this.X_P_BTN.TabIndex = 24;
            this.X_P_BTN.Text = "→";
            this.X_P_BTN.UseVisualStyleBackColor = true;
            this.X_P_BTN.Click += new System.EventHandler(this.X_P_BTN_Click);
            // 
            // textBox_f
            // 
            this.textBox_f.Location = new System.Drawing.Point(375, 71);
            this.textBox_f.Name = "textBox_f";
            this.textBox_f.Size = new System.Drawing.Size(100, 20);
            this.textBox_f.TabIndex = 23;
            this.textBox_f.TextChanged += new System.EventHandler(this.textBox_f_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(340, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "F:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(187, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(18, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "H:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "W:";
            // 
            // textBox_h
            // 
            this.textBox_h.Location = new System.Drawing.Point(225, 71);
            this.textBox_h.Name = "textBox_h";
            this.textBox_h.Size = new System.Drawing.Size(100, 20);
            this.textBox_h.TabIndex = 19;
            this.textBox_h.TextChanged += new System.EventHandler(this.textBox_h_TextChanged);
            // 
            // textBox_w
            // 
            this.textBox_w.Location = new System.Drawing.Point(75, 71);
            this.textBox_w.Name = "textBox_w";
            this.textBox_w.Size = new System.Drawing.Size(100, 20);
            this.textBox_w.TabIndex = 18;
            this.textBox_w.TextChanged += new System.EventHandler(this.textBox_w_TextChanged);
            // 
            // Action
            // 
            this.Action.Location = new System.Drawing.Point(2, 278);
            this.Action.Name = "Action";
            this.Action.Size = new System.Drawing.Size(93, 47);
            this.Action.TabIndex = 17;
            this.Action.Text = "Action";
            this.Action.UseVisualStyleBackColor = true;
            this.Action.Click += new System.EventHandler(this.Action_Click);
            // 
            // richTextBox_SEPin
            // 
            this.richTextBox_SEPin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox_SEPin.Location = new System.Drawing.Point(148, 123);
            this.richTextBox_SEPin.Name = "richTextBox_SEPin";
            this.richTextBox_SEPin.Size = new System.Drawing.Size(337, 660);
            this.richTextBox_SEPin.TabIndex = 16;
            this.richTextBox_SEPin.Text = "";
            this.richTextBox_SEPin.TextChanged += new System.EventHandler(this.richTextBox_SEPin_TextChanged);
            // 
            // DisplayBTN
            // 
            this.DisplayBTN.Location = new System.Drawing.Point(28, 186);
            this.DisplayBTN.Name = "DisplayBTN";
            this.DisplayBTN.Size = new System.Drawing.Size(93, 33);
            this.DisplayBTN.TabIndex = 13;
            this.DisplayBTN.Text = "Save";
            this.DisplayBTN.UseVisualStyleBackColor = true;
            this.DisplayBTN.Click += new System.EventHandler(this.SaveBTN_Click);
            // 
            // StopBTN
            // 
            this.StopBTN.Location = new System.Drawing.Point(28, 147);
            this.StopBTN.Name = "StopBTN";
            this.StopBTN.Size = new System.Drawing.Size(93, 33);
            this.StopBTN.TabIndex = 12;
            this.StopBTN.Text = "Stop";
            this.StopBTN.UseVisualStyleBackColor = true;
            this.StopBTN.Click += new System.EventHandler(this.StopBTN_Click);
            // 
            // PlayBTN
            // 
            this.PlayBTN.Location = new System.Drawing.Point(28, 108);
            this.PlayBTN.Name = "PlayBTN";
            this.PlayBTN.Size = new System.Drawing.Size(93, 33);
            this.PlayBTN.TabIndex = 11;
            this.PlayBTN.Text = "Play";
            this.PlayBTN.UseVisualStyleBackColor = true;
            this.PlayBTN.Click += new System.EventHandler(this.PlayBTN_Click);
            // 
            // TeachBTN
            // 
            this.TeachBTN.Location = new System.Drawing.Point(28, 225);
            this.TeachBTN.Name = "TeachBTN";
            this.TeachBTN.Size = new System.Drawing.Size(93, 33);
            this.TeachBTN.TabIndex = 40;
            this.TeachBTN.Text = "Teach";
            this.TeachBTN.UseVisualStyleBackColor = true;
            this.TeachBTN.Click += new System.EventHandler(this.TeachBTN_Click);
            // 
            // View
            // 
            this.ClientSize = new System.Drawing.Size(1540, 818);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip_video_status);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "View";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.View_Load);
            this.statusStrip_video_status.ResumeLayout(false);
            this.statusStrip_video_status.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.videoPictureBox)).EndInit();
            this.CtrlGRP.ResumeLayout(false);
            this.CtrlGRP.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip_video_status;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_videostatus;
        private System.Windows.Forms.ComboBox comboBox_url;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox videoPictureBox;
        private System.Windows.Forms.Button Action;
        private System.Windows.Forms.RichTextBox richTextBox_SEPin;
        private System.Windows.Forms.Button DisplayBTN;
        private System.Windows.Forms.Button StopBTN;
        private System.Windows.Forms.Button PlayBTN;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_h;
        private System.Windows.Forms.TextBox textBox_w;
        private System.Windows.Forms.TextBox textBox_f;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox CtrlGRP;
        private System.Windows.Forms.Button Y_P_BTN;
        private System.Windows.Forms.Button Y_N_BTN;
        private System.Windows.Forms.Button X_N_BTN;
        private System.Windows.Forms.CheckBox YrevCB;
        private System.Windows.Forms.CheckBox XrevCB;
        private System.Windows.Forms.Button X_P_BTN;
        private System.Windows.Forms.ComboBox comboBox_Degree;
        private System.Windows.Forms.ComboBox comboBox_plcdevices;
        private System.Windows.Forms.Button U_N_BTN;
        private System.Windows.Forms.Button Z_N_BTN;
        private System.Windows.Forms.Button U_P_BTN;
        private System.Windows.Forms.Button Z_P_BTN;
        private System.Windows.Forms.CheckBox XY_Change;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox X_Shift;
        private System.Windows.Forms.TextBox U_Shift;
        private System.Windows.Forms.TextBox Z_Shift;
        private System.Windows.Forms.TextBox Y_Shift;
        private System.Windows.Forms.Label label9;
        public System.Windows.Forms.TextBox Y_ratio_box;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.TextBox X_ratio_box;
        private System.Windows.Forms.Button Save_BTN;
        private System.Windows.Forms.Button Reset_BTN;
        private System.Windows.Forms.Button Wide_BTN;
        private System.Windows.Forms.Button Tele_BTN;
        private System.Windows.Forms.ComboBox CGI_CMDcomboBox1;
        private System.Windows.Forms.Button Send_CGIcmd;
        private System.Windows.Forms.ComboBox comboBox_CGI;
        private System.Windows.Forms.Label Action_Status;
        private System.Windows.Forms.ComboBox JSONData_comboBox;
        private System.Windows.Forms.Button TeachBTN;
    }
}

