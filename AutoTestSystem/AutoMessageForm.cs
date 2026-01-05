using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTestSystem
{
    public partial class AutoMessageForm : Form
    {
        private Timer timer;
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private const int HWND_TOPMOST = -1;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        string str_OK;
        string str_Cancel;
        int mode = -1;
        public AutoMessageForm(string message,string OKkey,string FAILKey)
        {
            InitializeComponent();

            str_OK = OKkey;
            str_Cancel = FAILKey;
            if (FAILKey == "")
                mode = 0;
            
            if (FAILKey == "" && OKkey == "")
            {
                str_OK = "Return";
                mode = 2;
            }
            if (FAILKey != "" && OKkey != "")
            {
                mode = 1;
            }
            if (FAILKey != "" && OKkey == "")
            {
                mode = 3;
            }

            message = message.Replace("@", Environment.NewLine);
            if(mode == 1)
                labelMessage.Text = $"{message} \n 按{OKkey}回傳True,{FAILKey}回傳False";
            else if(mode == 0)
                labelMessage.Text = $"{message} \n 按{OKkey}繼續";
            else if (mode == 2)
                labelMessage.Text = $"{message} \n 按Enter繼續";
            else if (mode == 3)
                labelMessage.Text = $"{message} \n 按{FAILKey}回傳異常";
            Size textSize = TextRenderer.MeasureText(labelMessage.Text, labelMessage.Font);
            int x = (this.ClientSize.Width - textSize.Width) / 2;
            int y = (this.ClientSize.Height - textSize.Height) / 2;
            labelMessage.Location = new Point(x, y);


            // 初始化計時器
            //timer = new Timer();
            //timer.Interval = 1500; // 3秒後關閉窗體
            //timer.Tick += Timer_Tick;
            //timer.Start();

            // 設定窗體在屏幕中央顯示
            this.StartPosition = FormStartPosition.CenterScreen;
            SetWindowPos(this.Handle, (IntPtr)HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

            // 設定 form 的 KeyPreview 屬性為 true，讓 form 可以接收鍵盤事件
            this.KeyPreview = true;

            // 註冊 form 的 KeyUp 事件處理器
            this.KeyUp += AutoMessageForm_KeyUp;
        }
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            // 使用GDI+自定會制窗體邊框
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Blue, 3); // 設置邊框顏色長度
            g.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1); // 繪制邊框
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // 關閉窗體
            //Close();
        }

        private void AutoMessageForm_KeyUp(object sender, KeyEventArgs e)
        {
            if(mode == 1)
            {            // 如果偵測到 enter 鍵被按下
                if (e.KeyCode.ToString() == str_OK)
                {
                    // 設定 form 的 DialogResult 為 true
                    this.DialogResult = DialogResult.Yes;

                    // 關閉 form
                    this.Close();
                }
                // 如果偵測到 space 鍵被按下
                else if (e.KeyCode.ToString() == str_Cancel)
                {

                    // 設定 form 的 DialogResult 為 false
                    this.DialogResult = DialogResult.No;

                    // 關閉 form
                    this.Close();
                }
            }
            else if(mode == 0 || mode == 2)
            {
                if (e.KeyCode.ToString() == str_OK)
                {                 
                    // 設定 form 的 DialogResult 為 true
                    this.DialogResult = DialogResult.Yes;

                    // 關閉 form
                    this.Close();
                }
            }
            else if (mode == 3)
            {
                if (e.KeyCode.ToString() == str_Cancel)
                {
                    this.DialogResult = DialogResult.No;

                    // 關閉 form
                    this.Close();
                }
            }
        }
    }
}
