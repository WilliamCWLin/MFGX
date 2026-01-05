namespace AutoTestSystem
{
    partial class SN_Panel
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
            this.label_sn = new System.Windows.Forms.Label();
            this.textBox_sn = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label_sn
            // 
            this.label_sn.AutoSize = true;
            this.label_sn.Location = new System.Drawing.Point(10, 6);
            this.label_sn.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_sn.Name = "label_sn";
            this.label_sn.Size = new System.Drawing.Size(41, 15);
            this.label_sn.TabIndex = 0;
            this.label_sn.Text = "label1";
            // 
            // textBox_sn
            // 
            this.textBox_sn.Location = new System.Drawing.Point(90, 3);
            this.textBox_sn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_sn.Name = "textBox_sn";
            this.textBox_sn.Size = new System.Drawing.Size(123, 25);
            this.textBox_sn.TabIndex = 1;
            // 
            // SN_Panel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBox_sn);
            this.Controls.Add(this.label_sn);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "SN_Panel";
            this.Size = new System.Drawing.Size(239, 39);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_sn;
        private System.Windows.Forms.TextBox textBox_sn;
    }
}
