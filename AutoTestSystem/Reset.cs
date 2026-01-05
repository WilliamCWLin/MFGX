/*
 * "AutoTestSystem --> Reset UI"
 *
 * Corpright William & Zhibin
 *
 *.##.......####.########.########..#######..##....##
 *.##........##.....##....##.......##.....##.###...##
 *.##........##.....##....##.......##.....##.####..##
 *.##........##.....##....######...##.....##.##.##.##
 *.##........##.....##....##.......##.....##.##..####
 *.##........##.....##....##.......##.....##.##...###
 *.########.####....##....########..#######..##....##
 *
 *  1. <Reset.cs> is a UI, it use for user keyword setting
 *  2. For example, if you key the wrong keyword, it will show "Fail consecutively exceeds,Please input Reset Password:"
 * 
 */

/*****************************************************************************
*                    Includes Definitions
*****************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;

/*****************************************************************************
*                    Function code
*****************************************************************************/
namespace AutoTestSystem
{
    public partial class Reset : Form
    {
        private bool isPassword = true;

        public Reset()
        {
            InitializeComponent();
            this.ActiveControl = textBox1; //设置当前窗口的活动控件为textBox1
            textBox1.Focus();
        }

        public Reset(string _str, string title)
        {
            InitializeComponent();
            label1.Text = _str;
            this.Text = title;
            isPassword = false;
            textBox1.UseSystemPasswordChar = false;
            this.ActiveControl = textBox1; //设置当前窗口的活动控件为textBox1
            textBox1.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isPassword)
            {
                if (this.textBox1.Text == "123")
                {
                    MainForm.f1.UpdateContinueFail(true);
                    this.Close();
                }
                else
                {
                    label2.Visible = Enabled;
                    label2.ForeColor = Color.Red;
                    label2.Text = "Worng password! please input again!";
                    textBox1.Text = "";
                    textBox1.Focus();
                }
            }
            else
            {
                MainForm.inPutValue = textBox1.Text.Trim();
                this.Close();
            }
        }
    }
}