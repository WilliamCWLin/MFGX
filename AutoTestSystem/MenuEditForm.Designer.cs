
namespace AutoTestSystem
{
    partial class MenuEditForm
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
            this.MemorichTextBox = new System.Windows.Forms.RichTextBox();
            this.MemoLabel = new System.Windows.Forms.Label();
            this.BTN_MenuEditSumit = new System.Windows.Forms.Button();
            this.ProjectLabel = new System.Windows.Forms.Label();
            this.ProjecttextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxMode = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.StationTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.FixtureTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.VersionTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // MemorichTextBox
            // 
            this.MemorichTextBox.Location = new System.Drawing.Point(99, 223);
            this.MemorichTextBox.Name = "MemorichTextBox";
            this.MemorichTextBox.Size = new System.Drawing.Size(206, 88);
            this.MemorichTextBox.TabIndex = 1;
            this.MemorichTextBox.Text = "";
            // 
            // MemoLabel
            // 
            this.MemoLabel.AutoSize = true;
            this.MemoLabel.Location = new System.Drawing.Point(36, 223);
            this.MemoLabel.Name = "MemoLabel";
            this.MemoLabel.Size = new System.Drawing.Size(48, 15);
            this.MemoLabel.TabIndex = 3;
            this.MemoLabel.Text = "Memo:";
            // 
            // BTN_MenuEditSumit
            // 
            this.BTN_MenuEditSumit.Location = new System.Drawing.Point(99, 317);
            this.BTN_MenuEditSumit.Name = "BTN_MenuEditSumit";
            this.BTN_MenuEditSumit.Size = new System.Drawing.Size(206, 42);
            this.BTN_MenuEditSumit.TabIndex = 4;
            this.BTN_MenuEditSumit.Text = "Submit";
            this.BTN_MenuEditSumit.UseVisualStyleBackColor = true;
            this.BTN_MenuEditSumit.Click += new System.EventHandler(this.BTN_MenuEditSumit_Click);
            // 
            // ProjectLabel
            // 
            this.ProjectLabel.AutoSize = true;
            this.ProjectLabel.Location = new System.Drawing.Point(37, 31);
            this.ProjectLabel.Name = "ProjectLabel";
            this.ProjectLabel.Size = new System.Drawing.Size(51, 15);
            this.ProjectLabel.TabIndex = 6;
            this.ProjectLabel.Text = "Project:";
            // 
            // ProjecttextBox
            // 
            this.ProjecttextBox.Location = new System.Drawing.Point(99, 26);
            this.ProjecttextBox.Name = "ProjecttextBox";
            this.ProjecttextBox.Size = new System.Drawing.Size(206, 25);
            this.ProjecttextBox.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(44, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 15);
            this.label2.TabIndex = 8;
            this.label2.Text = "Mode:";
            // 
            // comboBoxMode
            // 
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Items.AddRange(new object[] {
            "ENG",
            "PROD",
            "CHECK",
            "GRR"});
            this.comboBoxMode.Location = new System.Drawing.Point(99, 67);
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(206, 23);
            this.comboBoxMode.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 110);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 15);
            this.label1.TabIndex = 11;
            this.label1.Text = "Station:";
            // 
            // StationTextBox
            // 
            this.StationTextBox.Location = new System.Drawing.Point(99, 106);
            this.StationTextBox.Name = "StationTextBox";
            this.StationTextBox.Size = new System.Drawing.Size(206, 25);
            this.StationTextBox.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 147);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 15);
            this.label3.TabIndex = 13;
            this.label3.Text = "Fixture:";
            // 
            // FixtureTextBox
            // 
            this.FixtureTextBox.Location = new System.Drawing.Point(99, 144);
            this.FixtureTextBox.Name = "FixtureTextBox";
            this.FixtureTextBox.Size = new System.Drawing.Size(206, 25);
            this.FixtureTextBox.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(36, 184);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 15);
            this.label4.TabIndex = 15;
            this.label4.Text = "Version:";
            // 
            // VersionTextBox
            // 
            this.VersionTextBox.Location = new System.Drawing.Point(99, 181);
            this.VersionTextBox.Name = "VersionTextBox";
            this.VersionTextBox.Size = new System.Drawing.Size(206, 25);
            this.VersionTextBox.TabIndex = 14;
            // 
            // MenuEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 371);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.VersionTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.FixtureTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.StationTextBox);
            this.Controls.Add(this.comboBoxMode);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ProjectLabel);
            this.Controls.Add(this.ProjecttextBox);
            this.Controls.Add(this.BTN_MenuEditSumit);
            this.Controls.Add(this.MemoLabel);
            this.Controls.Add(this.MemorichTextBox);
            this.Name = "MenuEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MenuEditForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox MemorichTextBox;
        private System.Windows.Forms.Label MemoLabel;
        private System.Windows.Forms.Button BTN_MenuEditSumit;
        private System.Windows.Forms.Label ProjectLabel;
        private System.Windows.Forms.TextBox ProjecttextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxMode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox StationTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox FixtureTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox VersionTextBox;
    }
}