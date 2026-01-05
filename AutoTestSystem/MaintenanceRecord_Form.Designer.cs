namespace AutoTestSystem
{
    partial class MaintenanceRecord_Form
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
            this.btnAddMaintenanceRecord = new System.Windows.Forms.Button();
            this.rt_maintenance_record = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // btnAddMaintenanceRecord
            // 
            this.btnAddMaintenanceRecord.Font = new System.Drawing.Font("新細明體", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAddMaintenanceRecord.Location = new System.Drawing.Point(505, 12);
            this.btnAddMaintenanceRecord.Name = "btnAddMaintenanceRecord";
            this.btnAddMaintenanceRecord.Size = new System.Drawing.Size(100, 45);
            this.btnAddMaintenanceRecord.TabIndex = 6;
            this.btnAddMaintenanceRecord.Text = "新增維修日誌";
            this.btnAddMaintenanceRecord.UseVisualStyleBackColor = true;
            this.btnAddMaintenanceRecord.Click += new System.EventHandler(this.btnAddMaintenanceRecord_Click);
            // 
            // rt_maintenance_record
            // 
            this.rt_maintenance_record.Location = new System.Drawing.Point(12, 12);
            this.rt_maintenance_record.Name = "rt_maintenance_record";
            this.rt_maintenance_record.Size = new System.Drawing.Size(476, 160);
            this.rt_maintenance_record.TabIndex = 7;
            this.rt_maintenance_record.Text = "";
            // 
            // MaintenanceRecord_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 196);
            this.Controls.Add(this.rt_maintenance_record);
            this.Controls.Add(this.btnAddMaintenanceRecord);
            this.Name = "MaintenanceRecord_Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MaintenanceRecord_Form";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnAddMaintenanceRecord;
        private System.Windows.Forms.RichTextBox rt_maintenance_record;
    }
}