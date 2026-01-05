/*
 * "AutoTestSystem --> Messagebox_show UI"
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
 *  1. <Messagebox_show.cs> is a Messagebox, it use for operator to temporary stop
 *  2. For operator, please press the space bar or Enter key to continue testing
 * 
 */

/*****************************************************************************
*                    Includes Definitions
*****************************************************************************/
using AutoTestSystem.Model;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;


/*****************************************************************************
*                    Function code
*****************************************************************************/
namespace AutoTestSystem
{
    public partial class Messagebox_show : Form
    {
        
        NotifyIcon notifyIcon1 = new NotifyIcon();
        ImageList imageListIcons = new ImageList();       


        //public event EventHandler<string> DataReceived;
        public Messagebox_show()
        {
            InitializeComponent();
        }

        public void SetLabelText(string Commend,string Head_str, int Message_status)
        {
                        
            this.Text = Head_str;
            //this.Icon = new Icon("Icon.ico");
            if (Message_status == 0)
            {
                this.Icon = SystemIcons.Information;
                label1.ImageIndex = 1; //信息图标
                label1.ForeColor = Color.Blue;
            }
            else if (Message_status == 1)
            {
                this.Icon = SystemIcons.Warning;
                label1.ImageIndex = 2; //信息图标
                label1.ForeColor = Color.Orange;
            }
            else
            {
                this.Icon = SystemIcons.Error;
                label1.ImageIndex = 3; //信息图标
                label1.ForeColor = Color.Red;
            }

            imageListIcons.Images.Add(this.Icon.ToBitmap());
            imageListIcons.ImageSize= new System.Drawing.Size(32,32);

            var pictureBox = new PictureBox();
            pictureBox.Size = new System.Drawing.Size(200, 200);
            pictureBox.Paint += (sender, e) =>
            {
                var point = new System.Drawing.Point(20, 80);
                var index = 0;
                imageListIcons.Draw(e.Graphics, point, index);
            };
            this.Controls.Add(pictureBox);

            //label1.ImageList = imageListIcons;
            //label1.ImageAlign = ContentAlignment.MiddleLeft;                       

            label1.Text = Commend;           
            label1.Font = new Font("Times New Roman",18,FontStyle.Regular);
            
          

            /*notifyIcon1.Icon = SystemIcons.Information;
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(5000, "測試", label1.Text, ToolTipIcon.Info);

            notifyIcon1.Visible = false;
            notifyIcon1.Dispose();*/
        }

        private void Messagebox_show_Load(object sender, EventArgs e)
        {
            /*if (GlobalNew.EQ_Mode == "1")
            {               
                if (GlobalNew.image_path != string.Empty)
                {
                    try
                    {
                        if (File.Exists(GlobalNew.image_path))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(GlobalNew.image_path);
                            string[] nameParts = nameWithoutExt.Split('\\');
                            string name = nameParts[nameParts.Length - 1];

                            button3.Enabled = true;
                            button3.Visible = true;
                            button3.Text = name + " picture";
                        }
                        else
                        {
                            Logger.Debug($"The Image Path is not exist!!");
                        }
                    }catch(Exception ex)
                    {
                        MessageBox.Show($"Image_path Error:{ex.Message}");
                    }
                }else
                {
                    button1.Enabled = false;
                    button1.Visible = false;
                    button2.Enabled = false;
                    button2.Visible = false;
                }
            }*/
           
        }

        private void Messagebox_show_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                // 在这里编写您需要执行的代码
                // 当用户按下空格键后，程序会在这里继续执行
                e.Handled = true; // 将按键处理标记设置为 true，防止空格键生成其他事件
                DialogResult = DialogResult.OK;                              
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                DialogResult = DialogResult.Cancel;              
                this.Close();
            }
            else
            {
                e.Handled = true;              
                DialogResult = DialogResult.Retry;
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {          
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {          
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 在 OpenCV 窗口中显示图像           
            /*Mat img = Cv2.ImRead(GlobalNew.image_path);           
            Cv2.ImShow(button3.Text, img);
            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();    */        
        }
    }
}
