
namespace AutoTestSystem
{
    partial class IOSetting
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
            this.IO_GridView = new System.Windows.Forms.DataGridView();
            this.SensorName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Channel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.IO_GridView)).BeginInit();
            this.SuspendLayout();
            // 
            // IO_GridView
            // 
            this.IO_GridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.IO_GridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.IO_GridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SensorName,
            this.Channel});
            this.IO_GridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IO_GridView.Location = new System.Drawing.Point(0, 0);
            this.IO_GridView.Name = "IO_GridView";
            this.IO_GridView.RowHeadersWidth = 51;
            this.IO_GridView.RowTemplate.Height = 27;
            this.IO_GridView.Size = new System.Drawing.Size(800, 450);
            this.IO_GridView.TabIndex = 0;
            this.IO_GridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.IO_GridView_KeyDown);
            // 
            // SensorName
            // 
            this.SensorName.HeaderText = "SensorName";
            this.SensorName.MinimumWidth = 6;
            this.SensorName.Name = "SensorName";
            // 
            // Channel
            // 
            this.Channel.HeaderText = "Channel";
            this.Channel.MinimumWidth = 6;
            this.Channel.Name = "Channel";
            // 
            // IOSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.IO_GridView);
            this.Name = "IOSetting";
            this.Text = "IO_List";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.IOSetting_Form_FormClosed);
            this.Load += new System.EventHandler(this.IOSetting_Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.IO_GridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView IO_GridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn SensorName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Channel;
    }
}