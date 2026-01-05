namespace AutoTestSystem
{
    partial class SingleControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.lb_mode = new System.Windows.Forms.Label();
            this.lbl_failCount = new System.Windows.Forms.Label();
            this.lb_testName = new System.Windows.Forms.Label();
            this.lb_cellNum = new System.Windows.Forms.Label();
            this.lb_testTime = new System.Windows.Forms.Label();
            this.tb_sn = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lb_mode
            // 
            this.lb_mode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lb_mode.AutoSize = true;
            this.lb_mode.BackColor = System.Drawing.Color.Transparent;
            this.lb_mode.Location = new System.Drawing.Point(2, 48);
            this.lb_mode.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lb_mode.Name = "lb_mode";
            this.lb_mode.Size = new System.Drawing.Size(29, 12);
            this.lb_mode.TabIndex = 14;
            this.lb_mode.Text = "leaf";
            this.lb_mode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbl_failCount
            // 
            this.lbl_failCount.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_failCount.AutoSize = true;
            this.lbl_failCount.BackColor = System.Drawing.Color.Transparent;
            this.lbl_failCount.Location = new System.Drawing.Point(161, 48);
            this.lbl_failCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_failCount.Name = "lbl_failCount";
            this.lbl_failCount.Size = new System.Drawing.Size(11, 12);
            this.lbl_failCount.TabIndex = 13;
            this.lbl_failCount.Text = "1";
            this.lbl_failCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lb_testName
            // 
            this.lb_testName.BackColor = System.Drawing.Color.Transparent;
            this.lb_testName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lb_testName.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_testName.Location = new System.Drawing.Point(0, 0);
            this.lb_testName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lb_testName.Name = "lb_testName";
            this.lb_testName.Size = new System.Drawing.Size(171, 19);
            this.lb_testName.TabIndex = 5;
            this.lb_testName.Text = "CPUStressTest";
            this.lb_testName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lb_testName.DoubleClick += new System.EventHandler(this.lb_testName_DoubleClick);
            // 
            // lb_cellNum
            // 
            this.lb_cellNum.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lb_cellNum.AutoSize = true;
            this.lb_cellNum.BackColor = System.Drawing.Color.Transparent;
            this.lb_cellNum.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_cellNum.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lb_cellNum.Location = new System.Drawing.Point(2, 3);
            this.lb_cellNum.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lb_cellNum.Name = "lb_cellNum";
            this.lb_cellNum.Size = new System.Drawing.Size(23, 14);
            this.lb_cellNum.TabIndex = 11;
            this.lb_cellNum.Text = "80";
            this.lb_cellNum.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lb_cellNum.Visible = false;
            // 
            // lb_testTime
            // 
            this.lb_testTime.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lb_testTime.AutoSize = true;
            this.lb_testTime.BackColor = System.Drawing.Color.Transparent;
            this.lb_testTime.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_testTime.Location = new System.Drawing.Point(57, 46);
            this.lb_testTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lb_testTime.Name = "lb_testTime";
            this.lb_testTime.Size = new System.Drawing.Size(63, 14);
            this.lb_testTime.TabIndex = 0;
            this.lb_testTime.Text = "00:00:00";
            this.lb_testTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tb_sn
            // 
            this.tb_sn.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tb_sn.AutoSize = true;
            this.tb_sn.BackColor = System.Drawing.Color.Transparent;
            this.tb_sn.Font = new System.Drawing.Font("宋体", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb_sn.Location = new System.Drawing.Point(27, 3);
            this.tb_sn.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.tb_sn.Name = "tb_sn";
            this.tb_sn.Size = new System.Drawing.Size(135, 15);
            this.tb_sn.TabIndex = 15;
            this.tb_sn.Text = "NA5F004BE8DNNX42";
            this.tb_sn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.lb_testName);
            this.panel1.Location = new System.Drawing.Point(2, 23);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(171, 19);
            this.panel1.TabIndex = 16;
            // 
            // SingleControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tb_sn);
            this.Controls.Add(this.lb_testTime);
            this.Controls.Add(this.lb_mode);
            this.Controls.Add(this.lbl_failCount);
            this.Controls.Add(this.lb_cellNum);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SingleControl";
            this.Size = new System.Drawing.Size(176, 63);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label lb_mode;
        public System.Windows.Forms.Label lbl_failCount;
        public System.Windows.Forms.Label lb_testName;
        public System.Windows.Forms.Label lb_cellNum;
        public System.Windows.Forms.Label lb_testTime;
        public System.Windows.Forms.Label tb_sn;
        private System.Windows.Forms.Panel panel1;
    }
}
