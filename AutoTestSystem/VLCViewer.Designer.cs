namespace AutoTestSystem
{
    partial class VLCViewer
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
            this.videoPictureBox = new System.Windows.Forms.PictureBox();
            this.PlayBTN = new System.Windows.Forms.Button();
            this.StopBTN = new System.Windows.Forms.Button();
            this.DisplayBTN = new System.Windows.Forms.Button();
            this.UninitBTN = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.videoPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // videoPictureBox
            // 
            this.videoPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.videoPictureBox.Location = new System.Drawing.Point(33, 32);
            this.videoPictureBox.Name = "videoPictureBox";
            this.videoPictureBox.Size = new System.Drawing.Size(873, 567);
            this.videoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.videoPictureBox.TabIndex = 1;
            this.videoPictureBox.TabStop = false;
            // 
            // PlayBTN
            // 
            this.PlayBTN.Location = new System.Drawing.Point(937, 37);
            this.PlayBTN.Name = "PlayBTN";
            this.PlayBTN.Size = new System.Drawing.Size(68, 36);
            this.PlayBTN.TabIndex = 2;
            this.PlayBTN.Text = "Play";
            this.PlayBTN.UseVisualStyleBackColor = true;
            this.PlayBTN.Click += new System.EventHandler(this.PlayBTN_Click);
            // 
            // StopBTN
            // 
            this.StopBTN.Location = new System.Drawing.Point(937, 89);
            this.StopBTN.Name = "StopBTN";
            this.StopBTN.Size = new System.Drawing.Size(68, 36);
            this.StopBTN.TabIndex = 3;
            this.StopBTN.Text = "Stop";
            this.StopBTN.UseVisualStyleBackColor = true;
            this.StopBTN.Click += new System.EventHandler(this.StopBTN_Click);
            // 
            // DisplayBTN
            // 
            this.DisplayBTN.Location = new System.Drawing.Point(937, 147);
            this.DisplayBTN.Name = "DisplayBTN";
            this.DisplayBTN.Size = new System.Drawing.Size(68, 36);
            this.DisplayBTN.TabIndex = 4;
            this.DisplayBTN.Text = "Display";
            this.DisplayBTN.UseVisualStyleBackColor = true;
            this.DisplayBTN.Click += new System.EventHandler(this.DisplayBTN_Click);
            // 
            // UninitBTN
            // 
            this.UninitBTN.Location = new System.Drawing.Point(937, 203);
            this.UninitBTN.Name = "UninitBTN";
            this.UninitBTN.Size = new System.Drawing.Size(68, 36);
            this.UninitBTN.TabIndex = 5;
            this.UninitBTN.Text = "Uninit";
            this.UninitBTN.UseVisualStyleBackColor = true;
            this.UninitBTN.Click += new System.EventHandler(this.UninitBTN_Click);
            // 
            // VLCViewer
            // 
            this.ClientSize = new System.Drawing.Size(1077, 592);
            this.Controls.Add(this.UninitBTN);
            this.Controls.Add(this.DisplayBTN);
            this.Controls.Add(this.StopBTN);
            this.Controls.Add(this.PlayBTN);
            this.Controls.Add(this.videoPictureBox);
            this.Name = "VLCViewer";
            ((System.ComponentModel.ISupportInitialize)(this.videoPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion


        private System.Windows.Forms.PictureBox videoPictureBox;
        private System.Windows.Forms.Button PlayBTN;
        private System.Windows.Forms.Button StopBTN;
        private System.Windows.Forms.Button DisplayBTN;
        private System.Windows.Forms.Button UninitBTN;
    }
}

