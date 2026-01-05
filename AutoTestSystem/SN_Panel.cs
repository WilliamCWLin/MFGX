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

    public partial class SN_Panel : UserControl
    {
        public TextBox SN_Textbox
        {
            get { return textBox_sn; }
            set { textBox_sn = value; }
        }

        public Label SN_Label
        {
            get { return label_sn; }
            set { label_sn = value; /*AdjustLayout();*/ }
        }
        public SN_Panel()
        {
            InitializeComponent();

        }

        //private void AdjustLayout()
        //{
        //    textBox_sn.Location = new System.Drawing.Point(label_sn.Width+5, label_sn.Height + 5);
        //    this.Size = new System.Drawing.Size(textBox_sn.Width, label_sn.Height + textBox_sn.Height + 5);
        //}


    }


}
