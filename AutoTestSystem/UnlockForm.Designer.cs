namespace AutoTestSystem
{
    partial class UnlockForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelReasons;
        private System.Windows.Forms.TextBox textBoxNewReason;
        private System.Windows.Forms.Button buttonAddReason;
        private System.Windows.Forms.Button buttonSubmit;
        private System.Windows.Forms.RadioButton radioButtonOther;
        private System.Windows.Forms.TextBox textBoxOtherReason;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.flowLayoutPanelReasons = new System.Windows.Forms.FlowLayoutPanel();
            this.textBoxNewReason = new System.Windows.Forms.TextBox();
            this.buttonAddReason = new System.Windows.Forms.Button();
            this.buttonSubmit = new System.Windows.Forms.Button();
            this.radioButtonOther = new System.Windows.Forms.RadioButton();
            this.textBoxOtherReason = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // flowLayoutPanelReasons
            // 
            this.flowLayoutPanelReasons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelReasons.Location = new System.Drawing.Point(16, 14);
            this.flowLayoutPanelReasons.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.flowLayoutPanelReasons.Name = "flowLayoutPanelReasons";
            this.flowLayoutPanelReasons.Size = new System.Drawing.Size(317, 367);
            this.flowLayoutPanelReasons.TabIndex = 0;
            // 
            // textBoxNewReason
            // 
            this.textBoxNewReason.Location = new System.Drawing.Point(18, 444);
            this.textBoxNewReason.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBoxNewReason.Name = "textBoxNewReason";
            this.textBoxNewReason.Size = new System.Drawing.Size(194, 25);
            this.textBoxNewReason.TabIndex = 1;
            // 
            // buttonAddReason
            // 
            this.buttonAddReason.Location = new System.Drawing.Point(233, 442);
            this.buttonAddReason.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonAddReason.Name = "buttonAddReason";
            this.buttonAddReason.Size = new System.Drawing.Size(100, 27);
            this.buttonAddReason.TabIndex = 2;
            this.buttonAddReason.Text = "Add Reason";
            this.buttonAddReason.UseVisualStyleBackColor = true;
            this.buttonAddReason.Click += new System.EventHandler(this.buttonAddReason_Click);
            // 
            // buttonSubmit
            // 
            this.buttonSubmit.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.buttonSubmit.Location = new System.Drawing.Point(16, 476);
            this.buttonSubmit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonSubmit.Name = "buttonSubmit";
            this.buttonSubmit.Size = new System.Drawing.Size(100, 44);
            this.buttonSubmit.TabIndex = 3;
            this.buttonSubmit.Text = "Submit";
            this.buttonSubmit.UseVisualStyleBackColor = true;
            this.buttonSubmit.Click += new System.EventHandler(this.buttonSubmit_Click);
            // 
            // radioButtonOther
            // 
            this.radioButtonOther.AutoSize = true;
            this.radioButtonOther.Location = new System.Drawing.Point(16, 406);
            this.radioButtonOther.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.radioButtonOther.Name = "radioButtonOther";
            this.radioButtonOther.Size = new System.Drawing.Size(58, 19);
            this.radioButtonOther.TabIndex = 4;
            this.radioButtonOther.TabStop = true;
            this.radioButtonOther.Text = "其它";
            this.radioButtonOther.UseVisualStyleBackColor = true;
            this.radioButtonOther.CheckedChanged += new System.EventHandler(this.radioButtonOther_CheckedChanged);
            // 
            // textBoxOtherReason
            // 
            this.textBoxOtherReason.Location = new System.Drawing.Point(87, 405);
            this.textBoxOtherReason.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBoxOtherReason.Name = "textBoxOtherReason";
            this.textBoxOtherReason.Size = new System.Drawing.Size(246, 25);
            this.textBoxOtherReason.TabIndex = 5;
            this.textBoxOtherReason.Visible = false;
            // 
            // UnlockForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 525);
            this.Controls.Add(this.textBoxOtherReason);
            this.Controls.Add(this.radioButtonOther);
            this.Controls.Add(this.buttonSubmit);
            this.Controls.Add(this.buttonAddReason);
            this.Controls.Add(this.textBoxNewReason);
            this.Controls.Add(this.flowLayoutPanelReasons);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "UnlockForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Unlock";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}