namespace AutoTestSystem
{
    partial class DataInfo
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabCtrlMESandData = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dataGridView_Data = new System.Windows.Forms.DataGridView();
            this.DataKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DataValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridView_MES = new System.Windows.Forms.DataGridView();
            this.Key = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Spec = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabCtrlMESandData.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Data)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_MES)).BeginInit();
            this.SuspendLayout();
            // 
            // tabCtrlMESandData
            // 
            this.tabCtrlMESandData.Controls.Add(this.tabPage1);
            this.tabCtrlMESandData.Controls.Add(this.tabPage2);
            this.tabCtrlMESandData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCtrlMESandData.Location = new System.Drawing.Point(0, 0);
            this.tabCtrlMESandData.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabCtrlMESandData.Name = "tabCtrlMESandData";
            this.tabCtrlMESandData.SelectedIndex = 0;
            this.tabCtrlMESandData.Size = new System.Drawing.Size(466, 599);
            this.tabCtrlMESandData.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dataGridView_Data);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Size = new System.Drawing.Size(458, 570);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "DATA";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dataGridView_Data
            // 
            this.dataGridView_Data.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView_Data.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView_Data.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.MidnightBlue;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_Data.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView_Data.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Data.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DataKey,
            this.DataValue,
            this.Spec});
            this.dataGridView_Data.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_Data.EnableHeadersVisualStyles = false;
            this.dataGridView_Data.Location = new System.Drawing.Point(2, 2);
            this.dataGridView_Data.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dataGridView_Data.Name = "dataGridView_Data";
            this.dataGridView_Data.ReadOnly = true;
            this.dataGridView_Data.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dataGridView_Data.RowHeadersVisible = false;
            this.dataGridView_Data.RowHeadersWidth = 50;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataGridView_Data.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView_Data.RowTemplate.Height = 20;
            this.dataGridView_Data.Size = new System.Drawing.Size(454, 566);
            this.dataGridView_Data.TabIndex = 0;
            // 
            // DataKey
            // 
            this.DataKey.FillWeight = 153.8462F;
            this.DataKey.HeaderText = "Key";
            this.DataKey.MinimumWidth = 10;
            this.DataKey.Name = "DataKey";
            this.DataKey.ReadOnly = true;
            // 
            // DataValue
            // 
            this.DataValue.FillWeight = 119.0329F;
            this.DataValue.HeaderText = "Value";
            this.DataValue.MinimumWidth = 10;
            this.DataValue.Name = "DataValue";
            this.DataValue.ReadOnly = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dataGridView_MES);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Size = new System.Drawing.Size(458, 570);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "MES";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridView_MES
            // 
            this.dataGridView_MES.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView_MES.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView_MES.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.LightSteelBlue;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_MES.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView_MES.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_MES.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Key,
            this.Value});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView_MES.DefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridView_MES.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_MES.EnableHeadersVisualStyles = false;
            this.dataGridView_MES.Location = new System.Drawing.Point(2, 2);
            this.dataGridView_MES.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dataGridView_MES.Name = "dataGridView_MES";
            this.dataGridView_MES.ReadOnly = true;
            this.dataGridView_MES.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_MES.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridView_MES.RowHeadersVisible = false;
            this.dataGridView_MES.RowHeadersWidth = 50;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Arial", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataGridView_MES.RowsDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridView_MES.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.dataGridView_MES.RowTemplate.Height = 20;
            this.dataGridView_MES.Size = new System.Drawing.Size(454, 566);
            this.dataGridView_MES.TabIndex = 0;
            // 
            // Key
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.Key.DefaultCellStyle = dataGridViewCellStyle4;
            this.Key.FillWeight = 150F;
            this.Key.HeaderText = "Key";
            this.Key.MinimumWidth = 10;
            this.Key.Name = "Key";
            this.Key.ReadOnly = true;
            // 
            // Value
            // 
            this.Value.FillWeight = 255.7436F;
            this.Value.HeaderText = "Value";
            this.Value.MinimumWidth = 10;
            this.Value.Name = "Value";
            this.Value.ReadOnly = true;
            // 
            // Spec
            // 
            this.Spec.HeaderText = "Spec";
            this.Spec.MinimumWidth = 6;
            this.Spec.Name = "Spec";
            this.Spec.ReadOnly = true;
            // 
            // DataInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 599);
            this.Controls.Add(this.tabCtrlMESandData);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "DataInfo";
            this.Text = "DataInfo";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DataInfo_FormClosing);
            this.tabCtrlMESandData.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Data)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_MES)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabCtrlMESandData;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.DataGridView dataGridView_Data;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dataGridView_MES;
        private System.Windows.Forms.DataGridViewTextBoxColumn DataKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn DataValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn Key;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
        private System.Windows.Forms.DataGridViewTextBoxColumn Spec;
    }
}