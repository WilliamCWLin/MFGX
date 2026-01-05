using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTestSystem
{
    public partial class ImageShowFrom : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private const int HWND_TOPMOST = -1;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        string str_OK;
        string str_Cancel;
        int mode = -1;

        private BarcodeScanForm barcodeScanForm;
        private string Barcode_Commend;
        public bool barcode_enable = false;
        public bool Cancel_enable = true;
        ImageList imageListIcons = new ImageList();
        public ImageShowFrom()
        {
            InitializeComponent();
        }
        public void Cancel_Btn(bool Cancel_en)
        {
            Cancel_enable = Cancel_en;
        }

        public void SetImageShowForm(string message, string OKkey, string FAILKey,int Message_status , string img_path,bool barcode,bool button_en)
        {
            // 設定窗體在屏幕中央顯示
            this.StartPosition = FormStartPosition.CenterScreen;
            SetWindowPos(this.Handle, (IntPtr)HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

            // 設定 form 的 KeyPreview 屬性為 true，讓 form 可以接收鍵盤事件
            this.KeyPreview = true;

            // 註冊 form 的 KeyUp 事件處理器
            this.KeyUp += ImageShowFrom_KeyUp;

            if (button_en)
            {
                NG.Enabled = true;
                NG.Visible = true;
                Confirm.Enabled = true;
                Confirm.Visible = true;
                message = message.Replace("@", Environment.NewLine);
                label1.Text = $"{message} \n 按Button:\"OK\"回傳True, Button:\"NG\"回傳False";
                this.KeyUp -= ImageShowFrom_KeyUp;
            }
            else
            {

                str_OK = OKkey;
                str_Cancel = FAILKey;
                if (FAILKey == "NA" && OKkey == "NA")
                {
                    str_OK = "Return";
                    mode = 4;
                }
                else
                {

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
                }

                message = message.Replace("@", Environment.NewLine);
                if (mode == 1)
                    label1.Text = $"{message} \n 按{OKkey}回傳True,{FAILKey}回傳False";
                else if (mode == 0)
                    label1.Text = $"{message} \n 按{OKkey}繼續";
                else if (mode == 2)
                    label1.Text = $"{message} \n 按Enter繼續";
                else if (mode == 3)
                    label1.Text = $"{message} \n 按{FAILKey}回傳異常";
                else if (mode == 4)
                    label1.Text = $"{message}";
            }

            //this.Text = Head_str;
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
            imageListIcons.ImageSize = new Size(32, 32);

            label1.ImageList = imageListIcons;
            label1.ImageAlign = ContentAlignment.MiddleLeft;

            label1.Dock = DockStyle.Top;
            label1.Font = new Font("Times New Roman", 18, FontStyle.Regular);
            barcode_enable = barcode;

            if (!string.IsNullOrEmpty(img_path))
                PictureShow(img_path);
            else
                this.Size = new System.Drawing.Size(label1.Width + 40, label1.Height + 80); // 加上額外的空間以適應邊框和其他控件
        }

        public bool PictureShow(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show($"Image File path:{path} is not exist!!!", "Waring", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            //创建PictureBox控件
            Image image = Image.FromFile(path);
            pictureBox1.Image = image;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.Dock = DockStyle.Fill;

            //Confirm.AutoSize;
            NG.Dock = DockStyle.Bottom;
            NG.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            //注册窗口大小改变的事件
            this.Resize += new EventHandler(MainForm_Resize);
            return true;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            //调整PictureBox控件的大小以适应窗口大小
            pictureBox1.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - label1.Height - NG.Height);
            //调整Label控件和Button控件的位置以适应窗口大小
            label1.Location = new Point(0, this.ClientSize.Height - label1.Height - NG.Height);
            NG.Location = new Point(this.ClientSize.Width - NG.Width - 10, this.ClientSize.Height - NG.Height - 10);
            // 调整 NG 按钮的位置，确保它在 Confirm 按钮的右边且不重叠
            Confirm.Location = new Point(NG.Location.X - Confirm.Width - 10, this.ClientSize.Height - Confirm.Height - 10);

            // 根据 label1 的大小调整 Form 的大小
            this.Size = new Size(label1.Width + 40, label1.Height + NG.Height + 80); // 加上额外的空間以适应边框和其他控件
        }

        private void ImageShowFrom_Load(object sender, EventArgs e)
        {

        }

        private void ImageShowFrom_Shown(object sender, EventArgs e)
        {
            if (barcode_enable)
                GetBarCodeForm();
        }

        public void GetBarCodeForm()
        {
            //int BarcodeScan =0;
            barcodeScanForm = new BarcodeScanForm();
            barcodeScanForm.Owner = this; // 設置 BarcodeScanForm 的 Owner 為 ImageShowForm
            barcodeScanForm.StartPosition = FormStartPosition.Manual;
            barcodeScanForm.Location = new Point(
                this.Location.X + (this.Width - barcodeScanForm.Width) / 2,
                this.Location.Y + (this.Height - barcodeScanForm.Height) / 2
            );
            barcodeScanForm.Show();
            barcodeScanForm.TopMost = true; // 設置 BarcodeScanForm 為頂層窗體
            barcodeScanForm.BarcodeTextbox.Focus();
            barcodeScanForm.FormClosed += BarcodeScanForm_FormClosed;
            Barcode_Commend = label1.Text;
            barcodeScanForm.BarcodeCmd(Barcode_Commend);//存取Msg Commend字串當作Barcode KeyName使用
        }
        private void BarcodeScanForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ImageShowFrom_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (barcode_enable == true)
            {
                if (barcodeScanForm != null && !barcodeScanForm.BarcodeResult())
                {
                    // 取消關閉事件
                    e.Cancel = true;
                    MessageBox.Show("Please Scan BarcodeScanForm！", "BarcodeScanForm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                if (Cancel_enable)
                {
                    if (DialogResult != DialogResult.No && DialogResult != DialogResult.OK)
                    {
                        e.Cancel = true;
                        MessageBox.Show("Unable or invalid to Close Window！", "ImageShowFrom", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void ImageShowFrom_KeyUp(object sender, KeyEventArgs e)
        {

            if (mode == 1)
            {            // 如果偵測到 enter 鍵被按下
                if (e.KeyCode.ToString() == str_OK)
                {
                    // 設定 form 的 DialogResult 為 true
                    this.DialogResult = DialogResult.OK;

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
            else if (mode == 0 || mode == 2)
            {
                if (e.KeyCode.ToString() == str_OK)
                {
                    // 設定 form 的 DialogResult 為 true
                    this.DialogResult = DialogResult.OK;

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

        private void Confirm_MouseClick(object sender, MouseEventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void NG_MouseClick(object sender, MouseEventArgs e)
        {
            DialogResult = DialogResult.No;
            this.Close();
        }
    }
}
