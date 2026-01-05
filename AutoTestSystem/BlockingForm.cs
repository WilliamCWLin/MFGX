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
    public partial class BlockingForm : Form
    {

        public BlockingForm()
        {
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

    }
}
