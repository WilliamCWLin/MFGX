using AutoTestSystem.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTestSystem
{
    public partial class MenuEditForm : Form
    {
        public string RecipeName { get; private set; }
        public string Memo { get; private set; }
        public string CreationTime { get; private set; } = "";
        public string Creator { get; private set; }
        public string Version { get; private set; }
        public string Project { get; private set; }
        public string Mode { get; private set; }

        public string Station { get; private set; }

        public string Fixture { get; private set; }

        public TextBox VersionTextbox
        {
            get { return VersionTextBox; }
            set { VersionTextBox = value; }
        }

        public MenuEditForm()
        {
            InitializeComponent();
            comboBoxMode.SelectedIndex = 0;
        }

        public MenuEditForm(string memo,string prj,string mode, string sta, string fix, string time, string version)
        {
            InitializeComponent();
            // 初始化控件和布局
            //nameTextBox.Text = name;
            MemorichTextBox.Text = memo;
            ProjecttextBox.Text = prj;
            comboBoxMode.Text = mode;
            StationTextBox.Text = sta;
            FixtureTextBox.Text = fix;
            CreationTime = time;
            VersionTextBox.Text = version;
            //creatorTextBox.Text = creator;

        }

        private void BTN_MenuEditSumit_Click(object sender, EventArgs e)
        {
            // 設置屬性值
            //RecipeName = nameTextBox.Text;
            Memo = MemorichTextBox.Text;
            Version = VersionTextBox.Text;
            Project = ProjecttextBox.Text;
            Mode = comboBoxMode.Text;
            if(CreationTime == "")
                CreationTime = DateTime.Now.ToString("yyyy_MM_dd_HHmm").ToString();
            Station = StationTextBox.Text;
            Fixture = FixtureTextBox.Text;
            Creator = GlobalNew.CurrentUser;
            // 設置對話框結果並關閉表單
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
