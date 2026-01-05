using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.Equipment.ControlDevice;
using Newtonsoft.Json;
using AutoTestSystem.Script;
using System.Globalization;
using AutoTestSystem.Model;

namespace AutoTestSystem
{
    public partial class ReadTCP : Form
    {
        [JsonIgnore]
        [Browsable(false)]
        public TcpIpClient MotionCtrlDevice = null;//先寫死
        public string MotionDevice { get; set; }
        public string recieved;
        public string stream = null;
        public TcpIpClient Device;
        public ReadTCP(TcpIpClient client)
        {
            Device = client;
            InitializeComponent();

            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            int formWidth = this.Width;
            int formHeight = this.Height;
            int x = (screenWidth - formWidth) / 2; 
            int y = (screenHeight - formHeight) / 3; 

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new System.Drawing.Point(x, y);

        }

        public void label2_Click(object sender, EventArgs e)
        {
            
        }
    }   

}
