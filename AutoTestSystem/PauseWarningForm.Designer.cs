namespace AutoTestSystem
{
    partial class PauseWarningForm
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
            this.labelWarning = new System.Windows.Forms.Label();
            this.labelCountdown = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelWarning
            // 
            this.labelWarning.AutoSize = true;
            this.labelWarning.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.labelWarning.ForeColor = System.Drawing.Color.White;
            this.labelWarning.Location = new System.Drawing.Point(50, 20);
            this.labelWarning.Name = "labelWarning";
            this.labelWarning.Size = new System.Drawing.Size(300, 19);
            this.labelWarning.TabIndex = 0;
            this.labelWarning.Text = "⚠️ 偵測到安全 DI 觸發，馬達已停止！";
            // 
            // labelCountdown
            // 
            this.labelCountdown.AutoSize = true;
            this.labelCountdown.Font = new System.Drawing.Font("Arial", 14F);
            this.labelCountdown.ForeColor = System.Drawing.Color.Yellow;
            this.labelCountdown.Location = new System.Drawing.Point(120, 60);
            this.labelCountdown.Name = "labelCountdown";
            this.labelCountdown.Size = new System.Drawing.Size(180, 22);
            this.labelCountdown.TabIndex = 1;
            this.labelCountdown.Text = "剩餘時間：60 秒";
            // 
            // PauseWarningForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkRed;
            this.ClientSize = new System.Drawing.Size(400, 150);
            this.Controls.Add(this.labelCountdown);
            this.Controls.Add(this.labelWarning);
            this.Name = "PauseWarningForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "安全警告";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelWarning;
        private System.Windows.Forms.Label labelCountdown;
    }

}