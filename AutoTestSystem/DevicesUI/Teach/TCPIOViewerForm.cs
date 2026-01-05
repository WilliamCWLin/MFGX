using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AutoTestSystem;
using AutoTestSystem.Equipment.Motion;
using System.Threading.Tasks;
using System.Drawing.Design;
using Bulb;
using System.ComponentModel;
using System.Linq;
using AutoTestSystem.DevicesUI.Teach;

namespace AutoTestSystem.Equipment.Teach
{
    public class TCPIOViewerForm : Form
    {
        TCP plcclient = new TCP();
        public string mode = "";
        public string protocal = "";
        public string output;
        private LedBulb[] ledBulbs;
        private bool isRunning = true;
        [Category("Select IO Devices")]
        [Description("編輯路徑清單")]
        [Editor(typeof(SelectMotorDevices), typeof(UITypeEditor))]
        public List<string> SelectedDevices { get; set; } = new List<string>();
        [Browsable(false)]
        public string OriginMode { get; set; } = string.Empty;

        public TCPIOViewerForm(TCPTeach TCPioTeach)
        {
            EnsureDevicesSelected();
            output = "0";
            //plcclient.TCP_Mode = "Client";
            InitTCP();
            plcclient.SEND("IO");
            InitializeLedBulb();
            StartReceivingData();
            this.FormClosing += new FormClosingEventHandler(IOForm_FormClosing);
        }
        private bool EnsureDevicesSelected()
        {
            if (SelectedDevices.Any()) return true;

            using (var dlg = new DeviceSelectionForm(SelectedDevices))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    SelectedDevices = dlg.GetSelectedKeys();
                    OriginMode = dlg.GetSelectedOriginMode();
                }

            }
            if (!SelectedDevices.Any())
            {
                MessageBox.Show("請至少選擇一個控制軸再進行教學", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void InitializeLedBulb()
        {
            int ledsPerRow = 10; // 每排 LED 的數量
            int totalRows = 3; // 總排數
            int startX = 20; // 起始 X 位置
            int startY = 20; // 起始 Y 位置
            int spacingX = 60; // LED 之間的水平間距
            int spacingY = 60; // LED 之間的垂直間距

            ledBulbs = new LedBulb[ledsPerRow * totalRows];

            for (int row = 0; row < totalRows; row++)
            {
                for (int col = 0; col < ledsPerRow; col++)
                {
                    int index = row * ledsPerRow + col;

                    // 創建 LED 控件
                    ledBulbs[index] = new LedBulb
                    {
                        Size = new Size(25, 25),
                        Color = Color.Gray, // 初始顏色設置為灰色
                        On = false,
                        Location = new Point(startX + col * spacingX, startY + row * spacingY)
                    };

                    // 創建標籤控件
                    Label label = new Label
                    {
                        Text = $"Input {index + 1}",
                        Location = new Point(startX + col * spacingX, startY + row * spacingY + 30),
                        AutoSize = true
                    };

                    // 將 LED 和標籤添加到表單
                    this.Controls.Add(ledBulbs[index]);
                    this.Controls.Add(label);
                }
            }
        }

        private async void StartReceivingData()
        {
            while (isRunning)
            {
                plcclient.READTimeout(ref output, 100);
                UpdateLedsBasedOnOutput(output);
                await Task.Delay(500); // 每秒刷新一次
            }
        }

        private void UpdateLedStatus(int ledIndex, bool status)
        {
            if (ledIndex >= 0 && ledIndex < ledBulbs.Length)
            {
                ledBulbs[ledIndex].On = status;
                ledBulbs[ledIndex].Color = status ? Color.LawnGreen : Color.Gray;
            }
        }

        private void UpdateLedsBasedOnOutput(string output)
        {
            // 假設 output 是一個包含 0 和 1 的字符串，例如 "0101010101"
            for (int i = 0; i < output.Length && i < ledBulbs.Length; i++)
            {
                bool status = output[i] == '1';
                UpdateLedStatus(i, status);
            }
        }

        private void InitTCP()
        {
            if (plcclient.TryConnect())
            {
                //MessageBox.Show("Connected to server", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Not TCP connect..", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void IOForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            plcclient.UnInit();
            isRunning = false;
        }
    }
}
