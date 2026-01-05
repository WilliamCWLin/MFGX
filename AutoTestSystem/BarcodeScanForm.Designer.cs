
namespace AutoTestSystem
{
    partial class BarcodeScanForm
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
            this.textBoxScan = new System.Windows.Forms.TextBox();
            this.ScannedData = new System.Windows.Forms.ListBox();
            this.Torque_List = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // textBoxScan
            // 
            this.textBoxScan.Location = new System.Drawing.Point(12, 12);
            this.textBoxScan.Name = "textBoxScan";
            this.textBoxScan.Size = new System.Drawing.Size(260, 25);
            this.textBoxScan.TabIndex = 0;
            this.textBoxScan.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxScan_KeyDown);
            // 
            // ScannedData
            // 
            this.ScannedData.Location = new System.Drawing.Point(0, 0);
            this.ScannedData.Name = "ScannedData";
            this.ScannedData.Size = new System.Drawing.Size(120, 96);
            this.ScannedData.TabIndex = 0;
            // 
            // Torque_List
            // 
            this.Torque_List.Enabled = false;
            this.Torque_List.FormattingEnabled = true;
            this.Torque_List.Items.AddRange(new object[] {
            "VD5005_Indoor",
            "VD5005_Outdoor",
            "VD5006_Indoor",
            "VD5006_Outdoor"});
            this.Torque_List.Location = new System.Drawing.Point(151, 45);
            this.Torque_List.Name = "Torque_List";
            this.Torque_List.Size = new System.Drawing.Size(121, 23);
            this.Torque_List.TabIndex = 1;
            this.Torque_List.Visible = false;
            this.Torque_List.SelectedIndexChanged += new System.EventHandler(this.Torque_List_SelectedIndexChanged);
            // 
            // BarcodeScanForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 50);
            this.Controls.Add(this.Torque_List);
            this.Controls.Add(this.textBoxScan);
            this.Name = "BarcodeScanForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BarcodeScanForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BarcodeScanForm_FormClosing);
            this.Load += new System.EventHandler(this.BarcodeScanForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxScan;
        private System.Windows.Forms.ListBox ScannedData;
        private System.Windows.Forms.ComboBox Torque_List;
    }
}