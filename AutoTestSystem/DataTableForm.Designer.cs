

namespace AutoTestSystem
{
    partial class DataTableForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ProptableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.LoadBTN = new System.Windows.Forms.Button();
            this.PropDataGridView = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.ProptableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PropDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ProptableLayoutPanel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.PropDataGridView);
            this.splitContainer1.Size = new System.Drawing.Size(1187, 541);
            this.splitContainer1.SplitterDistance = 148;
            this.splitContainer1.TabIndex = 0;
            // 
            // ProptableLayoutPanel
            // 
            this.ProptableLayoutPanel.ColumnCount = 1;
            this.ProptableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 132F));
            this.ProptableLayoutPanel.Controls.Add(this.LoadBTN, 0, 0);
            this.ProptableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ProptableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.ProptableLayoutPanel.Name = "ProptableLayoutPanel";
            this.ProptableLayoutPanel.RowCount = 1;
            this.ProptableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ProptableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ProptableLayoutPanel.Size = new System.Drawing.Size(148, 69);
            this.ProptableLayoutPanel.TabIndex = 0;
            // 
            // LoadBTN
            // 
            this.LoadBTN.BackColor = System.Drawing.Color.GhostWhite;
            this.LoadBTN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LoadBTN.Font = new System.Drawing.Font("Calibri", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoadBTN.Location = new System.Drawing.Point(3, 3);
            this.LoadBTN.Name = "LoadBTN";
            this.LoadBTN.Size = new System.Drawing.Size(142, 63);
            this.LoadBTN.TabIndex = 0;
            this.LoadBTN.Text = "Load";
            this.LoadBTN.UseVisualStyleBackColor = false;
            this.LoadBTN.Click += new System.EventHandler(this.LoadBTN_Click);
            // 
            // PropDataGridView
            // 
            this.PropDataGridView.AllowUserToAddRows = false;
            this.PropDataGridView.AllowUserToDeleteRows = false;
            this.PropDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.PropDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PropDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropDataGridView.Location = new System.Drawing.Point(0, 0);
            this.PropDataGridView.Name = "PropDataGridView";
            this.PropDataGridView.ReadOnly = true;
            this.PropDataGridView.RowHeadersWidth = 51;
            this.PropDataGridView.RowTemplate.Height = 27;
            this.PropDataGridView.Size = new System.Drawing.Size(1035, 541);
            this.PropDataGridView.TabIndex = 0;
            // 
            // PropDataTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1187, 541);
            this.Controls.Add(this.splitContainer1);
            this.Name = "PropDataTable";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PropDataTable";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PropDataTable_FormClosing);
            this.Shown += new System.EventHandler(this.PropDataTable_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ProptableLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PropDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView PropDataGridView;
        private System.Windows.Forms.TableLayoutPanel ProptableLayoutPanel;
        private System.Windows.Forms.Button LoadBTN;
    }
}