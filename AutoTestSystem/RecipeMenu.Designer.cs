
namespace AutoTestSystem
{
    partial class RecipeMenu
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
            this.BTN_ADDRecipe = new System.Windows.Forms.Button();
            this.BTN_EditRecipe = new System.Windows.Forms.Button();
            this.Delete = new System.Windows.Forms.Button();
            this.BTN_APPLY = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.RecipeMenuDataGridView = new System.Windows.Forms.DataGridView();
            this.Project = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Station = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Fixture = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Mode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Memo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CreationTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Creator = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FilePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeMenuDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // BTN_ADDRecipe
            // 
            this.BTN_ADDRecipe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_ADDRecipe.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.BTN_ADDRecipe.Location = new System.Drawing.Point(5, 7);
            this.BTN_ADDRecipe.Name = "BTN_ADDRecipe";
            this.BTN_ADDRecipe.Size = new System.Drawing.Size(83, 35);
            this.BTN_ADDRecipe.TabIndex = 1;
            this.BTN_ADDRecipe.Text = "New";
            this.BTN_ADDRecipe.UseVisualStyleBackColor = true;
            this.BTN_ADDRecipe.Click += new System.EventHandler(this.BTN_ADDRecipe_Click);
            // 
            // BTN_EditRecipe
            // 
            this.BTN_EditRecipe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_EditRecipe.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            this.BTN_EditRecipe.Location = new System.Drawing.Point(5, 49);
            this.BTN_EditRecipe.Name = "BTN_EditRecipe";
            this.BTN_EditRecipe.Size = new System.Drawing.Size(83, 35);
            this.BTN_EditRecipe.TabIndex = 2;
            this.BTN_EditRecipe.Text = "Edit";
            this.BTN_EditRecipe.UseVisualStyleBackColor = true;
            this.BTN_EditRecipe.Click += new System.EventHandler(this.BTN_EditRecipe_Click);
            // 
            // Delete
            // 
            this.Delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Delete.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            this.Delete.Location = new System.Drawing.Point(5, 92);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(83, 35);
            this.Delete.TabIndex = 3;
            this.Delete.Text = "Delete";
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // BTN_APPLY
            // 
            this.BTN_APPLY.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_APPLY.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            this.BTN_APPLY.Location = new System.Drawing.Point(5, 234);
            this.BTN_APPLY.Name = "BTN_APPLY";
            this.BTN_APPLY.Size = new System.Drawing.Size(83, 35);
            this.BTN_APPLY.TabIndex = 4;
            this.BTN_APPLY.Text = "Apply";
            this.BTN_APPLY.UseVisualStyleBackColor = true;
            this.BTN_APPLY.Click += new System.EventHandler(this.BTN_APPLY_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.RecipeMenuDataGridView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.BTN_ADDRecipe);
            this.splitContainer1.Panel2.Controls.Add(this.BTN_APPLY);
            this.splitContainer1.Panel2.Controls.Add(this.BTN_EditRecipe);
            this.splitContainer1.Panel2.Controls.Add(this.Delete);
            this.splitContainer1.Size = new System.Drawing.Size(866, 298);
            this.splitContainer1.SplitterDistance = 768;
            this.splitContainer1.TabIndex = 5;
            // 
            // RecipeMenuDataGridView
            // 
            this.RecipeMenuDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.SkyBlue;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.RecipeMenuDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.RecipeMenuDataGridView.ColumnHeadersHeight = 29;
            this.RecipeMenuDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Project,
            this.Station,
            this.Fixture,
            this.Mode,
            this.Memo,
            this.CreationTime,
            this.Creator,
            this.FilePath});
            this.RecipeMenuDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RecipeMenuDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.RecipeMenuDataGridView.Location = new System.Drawing.Point(0, 0);
            this.RecipeMenuDataGridView.Name = "RecipeMenuDataGridView";
            this.RecipeMenuDataGridView.ReadOnly = true;
            this.RecipeMenuDataGridView.RowHeadersWidth = 51;
            this.RecipeMenuDataGridView.RowTemplate.Height = 27;
            this.RecipeMenuDataGridView.Size = new System.Drawing.Size(768, 298);
            this.RecipeMenuDataGridView.TabIndex = 1;
            this.RecipeMenuDataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.RecipeMenuDataGridView_CellDoubleClick);
            this.RecipeMenuDataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RecipeMenuDataGridView_KeyDown);
            // 
            // Project
            // 
            this.Project.FillWeight = 100.1508F;
            this.Project.HeaderText = "Project";
            this.Project.MinimumWidth = 6;
            this.Project.Name = "Project";
            this.Project.ReadOnly = true;
            // 
            // Station
            // 
            this.Station.FillWeight = 88.34621F;
            this.Station.HeaderText = "Station";
            this.Station.MinimumWidth = 6;
            this.Station.Name = "Station";
            this.Station.ReadOnly = true;
            // 
            // Fixture
            // 
            this.Fixture.FillWeight = 88.34621F;
            this.Fixture.HeaderText = "Fixture";
            this.Fixture.MinimumWidth = 6;
            this.Fixture.Name = "Fixture";
            this.Fixture.ReadOnly = true;
            // 
            // Mode
            // 
            this.Mode.FillWeight = 88.34621F;
            this.Mode.HeaderText = "Mode";
            this.Mode.MinimumWidth = 6;
            this.Mode.Name = "Mode";
            this.Mode.ReadOnly = true;
            this.Mode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // Memo
            // 
            this.Memo.FillWeight = 88.34621F;
            this.Memo.HeaderText = "Memo";
            this.Memo.MinimumWidth = 6;
            this.Memo.Name = "Memo";
            this.Memo.ReadOnly = true;
            // 
            // CreationTime
            // 
            this.CreationTime.FillWeight = 168.2785F;
            this.CreationTime.HeaderText = "CreationTime";
            this.CreationTime.MinimumWidth = 6;
            this.CreationTime.Name = "CreationTime";
            this.CreationTime.ReadOnly = true;
            // 
            // Creator
            // 
            this.Creator.FillWeight = 88.34621F;
            this.Creator.HeaderText = "Creator";
            this.Creator.MinimumWidth = 6;
            this.Creator.Name = "Creator";
            this.Creator.ReadOnly = true;
            // 
            // FilePath
            // 
            this.FilePath.HeaderText = "FilePath";
            this.FilePath.MinimumWidth = 6;
            this.FilePath.Name = "FilePath";
            this.FilePath.ReadOnly = true;
            this.FilePath.Visible = false;
            // 
            // RecipeMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(866, 298);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "RecipeMenu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RecipeMenu";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RecipeMenu_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RecipeMenuDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button BTN_ADDRecipe;
        private System.Windows.Forms.Button BTN_EditRecipe;
        private System.Windows.Forms.Button Delete;
        private System.Windows.Forms.Button BTN_APPLY;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView RecipeMenuDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn Project;
        private System.Windows.Forms.DataGridViewTextBoxColumn Station;
        private System.Windows.Forms.DataGridViewTextBoxColumn Fixture;
        private System.Windows.Forms.DataGridViewTextBoxColumn Mode;
        private System.Windows.Forms.DataGridViewTextBoxColumn Memo;
        private System.Windows.Forms.DataGridViewTextBoxColumn CreationTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn Creator;
        private System.Windows.Forms.DataGridViewTextBoxColumn FilePath;
    }
}