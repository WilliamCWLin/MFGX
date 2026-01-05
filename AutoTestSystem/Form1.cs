/*
 * "Not use"
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
 *  1. <Form1.cs> is out of service   
 * 
 */

using AutoTestSystem.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Runin
{
    public partial class Form1 : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  //Turn on WS_EX_COMPOSITED
                return cp;
            }
        }//!解决刷新控件时窗体闪烁
        public static Form1 f1;
        public static List<string> checkSNlist = new List<string>();
        public static List<SingleControl> SingleControlList = new List<SingleControl>();
        public static Version Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        public static readonly object[] CycleLock = new object[10];     //!互斥锁    

        public Form1()
        {
            //设定按字体来缩放控件
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            InitializeComponent();
            Global.InitStation();
            Global.LoadSequnces();
            //Global.LoadSequncesFromCsv();
            this.lb_testMode.Text = Global.TESTMODE;
            this.label5.Text = Global.FIXTURENAME;
            if (Global.TESTMODE.ToLower() == "debug")
            {
                lb_testMode.BackColor = Color.Red;
            }
            f1 = this;
            this.Text += " V" + Version;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (textBox1.Text.Trim().Length == 3)
                {
                    textBox2.Focus();
                }
                else
                {
                    MessageBox.Show("Location length error");
                }
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                string ScanSN = textBox2.Text.Trim();
                int localNo = Convert.ToInt16(textBox1.Text.Trim().Substring(1).Trim());

                if (ScanSN.Length == Global.SN_Length)
                {
                    if (checkSNlist.Contains(ScanSN))
                    {
                        textBox2.BackColor = Color.Violet;
                        label3.Text = "SN is repetitive!";
                        textBox1.Text = "";
                        textBox2.Text = "";
                        textBox1.Focus();
                        return;
                    }

                    if (!SingleControlList[localNo - 1].startFlag)
                    {
                        Global.LogPath = $@"{Global.LOGFOLDER}\{DateTime.Now.ToString("yyyyMMdd")}";
                        Global.CheckFolder();
                        Global.debugPath = $@"{Global.LogPath}\debug_{DateTime.Now.ToString("yyMMddHHmmss")}.txt";
                        SingleControlList[localNo - 1].cellLogPath = $@"{Global.LogPath}\{ScanSN}_{DateTime.Now.ToString("hh-mm-ss")}.txt";
                        SingleControlList[localNo - 1].tb_sn.Text = ScanSN;
                        SingleControlList[localNo - 1].lb_cellNum.Visible = Enabled;
                        SingleControlList[localNo - 1].lb_testName.Text = "";
                        SingleControlList[localNo - 1].StartTest();
                        textBox1.BackColor = Color.White;
                        textBox2.BackColor = Color.White;
                        checkSNlist.Add(ScanSN);
                    }
                    else
                    {
                        textBox1.BackColor = Color.Yellow;
                        textBox2.BackColor = Color.Yellow;
                        label3.Text = "Location is testing!";
                    }

                    textBox1.Text = "";
                    textBox2.Text = "";
                    textBox1.Focus();
                }
                else
                {
                    textBox2.BackColor = Color.Red;
                    label3.Text = "SN length error!";
                    textBox1.Text = "";
                    textBox2.Text = "";
                    textBox1.Focus();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++) //ROW
            {
                CycleLock[i] = new object();

                for (int j = 1; j < 9; j++) //Columu
                {
                    int index = (i * 8 + j);
                    SingleControl singleControl = new SingleControl(index);
                    singleControl.lb_testName.Text = index.ToString();
                    singleControl.tb_sn.Text = "";
                    singleControl.lb_testTime.Text = "";
                    singleControl.lb_mode.Text = "";
                    singleControl.lbl_failCount.Text = "";
                    singleControl.Dock = DockStyle.Fill;
                    tableLayoutPanel1.Controls.Add(singleControl, singleControl.letoutIndex - 1, singleControl.rowIndex - 1);
                    SingleControlList.Add(singleControl);

                }
            }
        }

        private void LogButton_Click(object sender, EventArgs e)
        {
            //for (int i = 0; i < SingleControlList.Count; i++)
            //{
            //    Global.LogPath = $@"{Global.LOGFOLDER}\{DateTime.Now.ToString("yyyyMMdd")}";
            //    Global.CheckFolder();
            //    Global.debugPath = $@"{Global.LogPath}\debug_{DateTime.Now.ToString("yyMMddHHmmss")}.txt";
            //    SingleControlList[i].cellLogPath = $@"{Global.LogPath}\{i}_{DateTime.Now.ToString("hh-mm-ss")}.txt";

            //    SingleControlList[i].StartTest();
            //}
            try
            {
                System.Diagnostics.Process.Start(Global.debugPath);
            }
            catch (Exception)
            {
                return;
            }
        }

    }
}
