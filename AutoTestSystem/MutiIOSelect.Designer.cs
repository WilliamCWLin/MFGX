
namespace AutoTestSystem
{
    partial class MutiIOSelect
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
            this.IO_DataGridView = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.IO_DataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // IO_DataGridView
            // 
            this.IO_DataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.IO_DataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.IO_DataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IO_DataGridView.Location = new System.Drawing.Point(0, 0);
            this.IO_DataGridView.Name = "IO_DataGridView";
            this.IO_DataGridView.RowHeadersWidth = 51;
            this.IO_DataGridView.RowTemplate.Height = 27;
            this.IO_DataGridView.Size = new System.Drawing.Size(800, 450);
            this.IO_DataGridView.TabIndex = 0;
            this.IO_DataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.IO_DataGridView_CellClick);
            this.IO_DataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.IO_DataGridView_CellValueChanged);
            this.IO_DataGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.IO_DataGridView_CurrentCellDirtyStateChanged);
            // 
            // MutiIOSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.IO_DataGridView);
            this.Name = "MutiIOSelect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MutiIOSelect";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MutiIOSelect_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.IO_DataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView IO_DataGridView;
    }
}