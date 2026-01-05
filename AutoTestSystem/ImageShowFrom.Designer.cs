
namespace AutoTestSystem
{
    partial class ImageShowFrom
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Confirm = new System.Windows.Forms.Button();
            this.NG = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 66);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(725, 534);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.UseWaitCursor = true;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 28.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label1.Location = new System.Drawing.Point(24, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(701, 69);
            this.label1.TabIndex = 1;
            this.label1.Text = "Gimbal Test:Please press the space bar or Enter key to continue testing";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.label1.UseWaitCursor = true;
            // 
            // Confirm
            // 
            this.Confirm.BackColor = System.Drawing.Color.Lime;
            this.Confirm.Enabled = false;
            this.Confirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Confirm.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Confirm.Location = new System.Drawing.Point(465, 546);
            this.Confirm.Name = "Confirm";
            this.Confirm.Size = new System.Drawing.Size(120, 42);
            this.Confirm.TabIndex = 2;
            this.Confirm.Text = "OK";
            this.Confirm.UseVisualStyleBackColor = false;
            this.Confirm.UseWaitCursor = true;
            this.Confirm.Visible = false;
            this.Confirm.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Confirm_MouseClick);
            // 
            // NG
            // 
            this.NG.BackColor = System.Drawing.Color.Red;
            this.NG.Enabled = false;
            this.NG.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NG.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.NG.ForeColor = System.Drawing.SystemColors.InfoText;
            this.NG.Location = new System.Drawing.Point(605, 546);
            this.NG.Name = "NG";
            this.NG.Size = new System.Drawing.Size(120, 42);
            this.NG.TabIndex = 3;
            this.NG.Text = "NG";
            this.NG.UseVisualStyleBackColor = false;
            this.NG.UseWaitCursor = true;
            this.NG.Visible = false;
            this.NG.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NG_MouseClick);
            // 
            // ImageShowFrom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ClientSize = new System.Drawing.Size(775, 600);
            this.Controls.Add(this.NG);
            this.Controls.Add(this.Confirm);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "ImageShowFrom";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ImageShowFrom";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImageShowFrom_FormClosing);
            this.Load += new System.EventHandler(this.ImageShowFrom_Load);
            this.Shown += new System.EventHandler(this.ImageShowFrom_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Confirm;
        private System.Windows.Forms.Button NG;
    }
}