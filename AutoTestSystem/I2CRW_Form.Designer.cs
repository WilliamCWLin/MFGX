
namespace AutoTestSystem
{
    partial class I2CRW_Form
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.KeyName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Hight_Bytes = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Low_Bytes = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.KeyName,
            this.Hight_Bytes,
            this.Low_Bytes});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 27;
            this.dataGridView1.Size = new System.Drawing.Size(711, 400);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView1_KeyDown);
            // 
            // KeyName
            // 
            this.KeyName.HeaderText = "KeyName";
            this.KeyName.MinimumWidth = 6;
            this.KeyName.Name = "KeyName";
            // 
            // Hight_Bytes
            // 
            this.Hight_Bytes.HeaderText = "Hight_Bytes";
            this.Hight_Bytes.MinimumWidth = 6;
            this.Hight_Bytes.Name = "Hight_Bytes";
            // 
            // Low_Bytes
            // 
            this.Low_Bytes.HeaderText = "Low_Bytes";
            this.Low_Bytes.MinimumWidth = 6;
            this.Low_Bytes.Name = "Low_Bytes";
            // 
            // I2CRW_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(711, 400);
            this.Controls.Add(this.dataGridView1);
            this.Name = "I2CRW_Form";
            this.Text = "I2CRW_Form";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.I2CRW_Form_FormClosed);
            this.Load += new System.EventHandler(this.I2CRW_Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn KeyName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Hight_Bytes;
        private System.Windows.Forms.DataGridViewTextBoxColumn Low_Bytes;
    }
}