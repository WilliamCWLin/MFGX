
namespace Manufacture
{
    partial class SpecForm_SingleEntry
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
            this.comboBox_SpecType = new System.Windows.Forms.ComboBox();
            this.SpecValue = new System.Windows.Forms.TextBox();
            this.richTextBox_Spec = new System.Windows.Forms.RichTextBox();
            this.label_specvalue = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.AddSpecBtn = new System.Windows.Forms.Button();
            this.comboBox_DataName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtMinLimit = new System.Windows.Forms.TextBox();
            this.txtMaxLimit = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBox_MES = new System.Windows.Forms.CheckBox();
            this.checkBox_CSV = new System.Windows.Forms.CheckBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBox_SpecType
            // 
            this.comboBox_SpecType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_SpecType.Font = new System.Drawing.Font("Arial", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox_SpecType.FormattingEnabled = true;
            this.comboBox_SpecType.Items.AddRange(new object[] {
            "=",
            "[min,max]",
            ">",
            "<",
            "Bypass"});
            this.comboBox_SpecType.Location = new System.Drawing.Point(210, 26);
            this.comboBox_SpecType.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_SpecType.Name = "comboBox_SpecType";
            this.comboBox_SpecType.Size = new System.Drawing.Size(87, 24);
            this.comboBox_SpecType.TabIndex = 0;
            this.comboBox_SpecType.SelectedIndexChanged += new System.EventHandler(this.comboBox_SpecType_SelectedIndexChanged);
            // 
            // SpecValue
            // 
            this.SpecValue.Font = new System.Drawing.Font("Arial", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SpecValue.Location = new System.Drawing.Point(310, 26);
            this.SpecValue.Margin = new System.Windows.Forms.Padding(2);
            this.SpecValue.Name = "SpecValue";
            this.SpecValue.Size = new System.Drawing.Size(173, 23);
            this.SpecValue.TabIndex = 1;
            // 
            // richTextBox_Spec
            // 
            this.richTextBox_Spec.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox_Spec.Location = new System.Drawing.Point(9, 58);
            this.richTextBox_Spec.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox_Spec.Name = "richTextBox_Spec";
            this.richTextBox_Spec.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBox_Spec.Size = new System.Drawing.Size(572, 418);
            this.richTextBox_Spec.TabIndex = 2;
            this.richTextBox_Spec.Text = "";
            // 
            // label_specvalue
            // 
            this.label_specvalue.AutoSize = true;
            this.label_specvalue.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_specvalue.Location = new System.Drawing.Point(308, 7);
            this.label_specvalue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_specvalue.Name = "label_specvalue";
            this.label_specvalue.Size = new System.Drawing.Size(60, 16);
            this.label_specvalue.TabIndex = 3;
            this.label_specvalue.Text = "Criteria";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(207, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Spec Type";
            // 
            // AddSpecBtn
            // 
            this.AddSpecBtn.BackColor = System.Drawing.Color.Transparent;
            this.AddSpecBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.AddSpecBtn.FlatAppearance.BorderSize = 0;
            this.AddSpecBtn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.AddSpecBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.AddSpecBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AddSpecBtn.Image = global::AutoTestSystem.Properties.Resources.Comm;
            this.AddSpecBtn.Location = new System.Drawing.Point(542, 17);
            this.AddSpecBtn.Margin = new System.Windows.Forms.Padding(2);
            this.AddSpecBtn.Name = "AddSpecBtn";
            this.AddSpecBtn.Size = new System.Drawing.Size(34, 34);
            this.AddSpecBtn.TabIndex = 5;
            this.AddSpecBtn.UseVisualStyleBackColor = false;
            this.AddSpecBtn.Click += new System.EventHandler(this.AddSpecBtn_Click);
            // 
            // comboBox_DataName
            // 
            this.comboBox_DataName.Font = new System.Drawing.Font("Arial", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox_DataName.FormattingEnabled = true;
            this.comboBox_DataName.Location = new System.Drawing.Point(17, 25);
            this.comboBox_DataName.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_DataName.Name = "comboBox_DataName";
            this.comboBox_DataName.Size = new System.Drawing.Size(79, 24);
            this.comboBox_DataName.Sorted = true;
            this.comboBox_DataName.TabIndex = 6;
            this.comboBox_DataName.SelectedIndexChanged += new System.EventHandler(this.comboBox_DataName_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(14, 7);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 16);
            this.label2.TabIndex = 7;
            this.label2.Text = "Section";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // txtMinLimit
            // 
            this.txtMinLimit.Font = new System.Drawing.Font("Arial", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMinLimit.Location = new System.Drawing.Point(310, 26);
            this.txtMinLimit.Margin = new System.Windows.Forms.Padding(2);
            this.txtMinLimit.Name = "txtMinLimit";
            this.txtMinLimit.Size = new System.Drawing.Size(74, 24);
            this.txtMinLimit.TabIndex = 8;
            this.txtMinLimit.Visible = false;
            // 
            // txtMaxLimit
            // 
            this.txtMaxLimit.Font = new System.Drawing.Font("Arial", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMaxLimit.Location = new System.Drawing.Point(414, 26);
            this.txtMaxLimit.Margin = new System.Windows.Forms.Padding(2);
            this.txtMaxLimit.Name = "txtMaxLimit";
            this.txtMaxLimit.Size = new System.Drawing.Size(69, 24);
            this.txtMaxLimit.TabIndex = 9;
            this.txtMaxLimit.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(394, 30);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(11, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "~";
            this.label3.Visible = false;
            // 
            // checkBox_MES
            // 
            this.checkBox_MES.AutoSize = true;
            this.checkBox_MES.Checked = true;
            this.checkBox_MES.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_MES.Location = new System.Drawing.Point(490, 20);
            this.checkBox_MES.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_MES.Name = "checkBox_MES";
            this.checkBox_MES.Size = new System.Drawing.Size(47, 16);
            this.checkBox_MES.TabIndex = 11;
            this.checkBox_MES.Text = "MES";
            this.checkBox_MES.UseVisualStyleBackColor = true;
            // 
            // checkBox_CSV
            // 
            this.checkBox_CSV.AutoSize = true;
            this.checkBox_CSV.Checked = true;
            this.checkBox_CSV.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_CSV.Location = new System.Drawing.Point(490, 39);
            this.checkBox_CSV.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_CSV.Name = "checkBox_CSV";
            this.checkBox_CSV.Size = new System.Drawing.Size(46, 16);
            this.checkBox_CSV.TabIndex = 12;
            this.checkBox_CSV.Text = "CSV";
            this.checkBox_CSV.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            this.comboBox1.Font = new System.Drawing.Font("Arial", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(114, 25);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(79, 24);
            this.comboBox1.Sorted = true;
            this.comboBox1.TabIndex = 13;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(111, 7);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 16);
            this.label4.TabIndex = 14;
            this.label4.Text = "Item";
            // 
            // SpecForm_SingleEntry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 487);
            this.Controls.Add(this.AddSpecBtn);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.checkBox_CSV);
            this.Controls.Add(this.checkBox_MES);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtMaxLimit);
            this.Controls.Add(this.txtMinLimit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_DataName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label_specvalue);
            this.Controls.Add(this.richTextBox_Spec);
            this.Controls.Add(this.SpecValue);
            this.Controls.Add(this.comboBox_SpecType);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SpecForm_SingleEntry";
            this.Text = "SpecForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_SpecType;
        private System.Windows.Forms.TextBox SpecValue;
        private System.Windows.Forms.RichTextBox richTextBox_Spec;
        private System.Windows.Forms.Label label_specvalue;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button AddSpecBtn;
        private System.Windows.Forms.ComboBox comboBox_DataName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMinLimit;
        private System.Windows.Forms.TextBox txtMaxLimit;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBox_MES;
        private System.Windows.Forms.CheckBox checkBox_CSV;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label4;
    }
}