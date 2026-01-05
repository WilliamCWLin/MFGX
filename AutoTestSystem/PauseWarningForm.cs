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
    public partial class PauseWarningForm : Form
    {
        public PauseWarningForm()
        {
            InitializeComponent();
            //this.Text = "安全警告";
            //this.Size = new System.Drawing.Size(400, 150);
            //this.StartPosition = FormStartPosition.CenterScreen;
            //this.TopMost = true;
            //this.BackColor = System.Drawing.Color.DarkRed;

            //labelWarning = new Label();
            //labelWarning.Text = "⚠️ 偵測到安全 DI 觸發，馬達已停止！";
            //labelWarning.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
            //labelWarning.ForeColor = System.Drawing.Color.White;
            //labelWarning.AutoSize = true;
            //labelWarning.Location = new System.Drawing.Point(50, 20);
            //this.Controls.Add(labelWarning);

            //labelCountdown = new Label();
            //labelCountdown.Text = "剩餘時間：60 秒";
            //labelCountdown.Font = new System.Drawing.Font("Arial", 14);
            //labelCountdown.ForeColor = System.Drawing.Color.Yellow;
            //labelCountdown.AutoSize = true;
            //labelCountdown.Location = new System.Drawing.Point(120, 60);
            //this.Controls.Add(labelCountdown);
        }

        public void UpdateCountdown(int remainingSeconds)
        {
            labelCountdown.Text = $"剩餘時間：{remainingSeconds} 秒";
            Application.DoEvents(); // 讓 UI 即時更新
        }
    }
}
