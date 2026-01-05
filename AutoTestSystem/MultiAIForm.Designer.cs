namespace AutoTestSystem
{
    partial class MutiAIForm
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
            this.AI_DataGridView = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.AI_DataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // AI_DataGridView
            // 
            this.AI_DataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.AI_DataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.AI_DataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AI_DataGridView.Location = new System.Drawing.Point(0, 0);
            this.AI_DataGridView.Name = "AI_DataGridView";
            this.AI_DataGridView.RowHeadersWidth = 51;
            this.AI_DataGridView.RowTemplate.Height = 27;
            this.AI_DataGridView.Size = new System.Drawing.Size(800, 450);
            this.AI_DataGridView.TabIndex = 1;
            // 
            // MultiAIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.AI_DataGridView);
            this.Name = "MultiAIForm";
            this.Text = "MultiAIForm";
            ((System.ComponentModel.ISupportInitialize)(this.AI_DataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView AI_DataGridView;
    }
}