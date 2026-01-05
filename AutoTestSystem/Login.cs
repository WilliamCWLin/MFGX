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
    public partial class Login : Form
    {
        public string ScanfixName;
        public string Fixture_Number;

        public Login()
        {
            InitializeComponent();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            txt_Fixture.Text = "";
            txt_Fixture.Focus();
            txt_Fixture.Enabled = true;
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            if (txt_Fixture.Text.Length > 0)
            {
                this.DialogResult = DialogResult.OK;
                this.Hide();
                // this.Close();
                // this.Dispose();
            }
        }

        private void txt_Fixture_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                ScanfixName = txt_Fixture.Text.Trim();

                if (ScanfixName.Contains("RUNIN") && ScanfixName.Length == 11 && (ScanfixName.Substring(ScanfixName.Length - 1, 1) == "A" || ScanfixName.Substring(ScanfixName.Length - 1, 1) == "B"))
                {
                    Fixture_Number = ScanfixName.Substring(0, 10).Trim();
                    //ABFace = fixName.Substring(10, 1).Trim();
                    txt_Fixture.Enabled = false;
                    btn_OK.Focus();
                    this.DialogResult = DialogResult.OK;
                    this.Hide();
                    //this.Close();
                    //this.Dispose();
                }
                else
                {
                    MessageBox.Show("Station is Wrong!");
                    txt_Fixture.Text = "";
                    txt_Fixture.Focus();
                    ScanfixName = "RUNIN-xxxxX";
                    label2.Text = ScanfixName;
                    Fixture_Number = "";
                    // ABFace = "";
                    return;
                }
            }
        }
    }
}
