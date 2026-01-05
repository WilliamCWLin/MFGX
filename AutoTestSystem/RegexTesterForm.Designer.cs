using System.Windows.Forms;

namespace AutoTestSystem
{
    partial class RegexTesterForm
    {
        private System.ComponentModel.IContainer components = null;
        private RichTextBox regexInput;  // 這裡變更為 RichTextBox
        private RichTextBox testStringInput;
        private Button clearButton;
        private DataGridView resultGrid;
        private DataGridView newResultGrid;
        private Label statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.regexInput = new System.Windows.Forms.RichTextBox();
            this.testStringInput = new System.Windows.Forms.RichTextBox();
            this.clearButton = new System.Windows.Forms.Button();
            this.resultGrid = new System.Windows.Forms.DataGridView();
            this.newResultGrid = new System.Windows.Forms.DataGridView();
            this.statusLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.resultGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.newResultGrid)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // regexInput
            // 
            this.regexInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.regexInput.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.regexInput.Location = new System.Drawing.Point(3, 3);
            this.regexInput.Name = "regexInput";
            this.regexInput.Size = new System.Drawing.Size(794, 84);
            this.regexInput.TabIndex = 0;
            this.regexInput.Text = "";
            this.regexInput.TextChanged += new System.EventHandler(this.Input_TextChanged);
            this.regexInput.GotFocus += new System.EventHandler(this.regexInput_GotFocus);
            this.regexInput.LostFocus += new System.EventHandler(this.regexInput_LostFocus);
            this.regexInput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.richTextBox_MouseDown);
            this.regexInput.MouseMove += new System.Windows.Forms.MouseEventHandler(this.richTextBox_MouseMove);
            this.regexInput.MouseUp += new System.Windows.Forms.MouseEventHandler(this.richTextBox_MouseUp);
            // 
            // testStringInput
            // 
            this.testStringInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.testStringInput.Location = new System.Drawing.Point(3, 93);
            this.testStringInput.Name = "testStringInput";
            this.testStringInput.Size = new System.Drawing.Size(794, 177);
            this.testStringInput.TabIndex = 1;
            this.testStringInput.Text = "";
            this.testStringInput.TextChanged += new System.EventHandler(this.Input_TextChanged);
            this.testStringInput.GotFocus += new System.EventHandler(this.testStringInput_GotFocus);
            this.testStringInput.LostFocus += new System.EventHandler(this.testStringInput_LostFocus);
            this.testStringInput.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.testStringInput_MouseDoubleClick);
            // 
            // clearButton
            // 
            this.clearButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.clearButton.Location = new System.Drawing.Point(725, 0);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(75, 62);
            this.clearButton.TabIndex = 2;
            this.clearButton.Text = "清除";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // resultGrid
            // 
            this.resultGrid.AllowUserToAddRows = false;
            this.resultGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.resultGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.resultGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultGrid.Location = new System.Drawing.Point(3, 276);
            this.resultGrid.Name = "resultGrid";
            this.resultGrid.RowHeadersWidth = 51;
            this.resultGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.resultGrid.Size = new System.Drawing.Size(794, 200);
            this.resultGrid.TabIndex = 3;
            this.resultGrid.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.resultGrid_CellMouseDown);
            // 
            // newResultGrid
            // 
            this.newResultGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.newResultGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.newResultGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.newResultGrid.Location = new System.Drawing.Point(3, 482);
            this.newResultGrid.Name = "newResultGrid";
            this.newResultGrid.RowHeadersWidth = 51;
            this.newResultGrid.Size = new System.Drawing.Size(794, 201);
            this.newResultGrid.TabIndex = 4;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(12, 580);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 24);
            this.statusLabel.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.regexInput, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.testStringInput, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.newResultGrid, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.resultGrid, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.11953F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 26.67638F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 686);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.clearButton);
            this.splitContainer1.Size = new System.Drawing.Size(800, 752);
            this.splitContainer1.SplitterDistance = 686;
            this.splitContainer1.TabIndex = 7;
            // 
            // RegexTesterForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 752);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusLabel);
            this.Name = "RegexTesterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "正則表達式編輯器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RegexTesterForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.resultGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.newResultGrid)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private TableLayoutPanel tableLayoutPanel1;
        private SplitContainer splitContainer1;
    }
}
