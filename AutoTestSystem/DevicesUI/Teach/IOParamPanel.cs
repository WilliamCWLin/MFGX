using AutoTestSystem.Base;
using AutoTestSystem.Equipment.Teach;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTestSystem.DevicesUI.Teach
{
    public partial class IOParamPanel : UserControl
    {
        private ComboBox comboBoxChannels;
        private CheckBox checkBoxOnOff;

        public IOParamPanel(string Name, IOBase motion)
        {
            InitializeComponent();
            label1.Text = Name;
            GenerateControls();
        }

        public void UpdateLabelColor(Color c)
        {
            label1.BackColor = c;
        }

        private void GenerateControls()
        {
            comboBoxChannels = new ComboBox
            {
                Name = "comboBoxChannels",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(10, 10),
                Width = 100
            };

            for (int i = 0; i < 16; i++)
            {
                comboBoxChannels.Items.Add($"Channel {i}");
            }

            checkBoxOnOff = new CheckBox
            {
                Name = "checkBoxOnOff",
                Text = "On/Off",
                AutoSize = true,
                Location = new Point(10, 50)
            };
            checkBoxOnOff.CheckedChanged += new EventHandler(checkBoxOnOff_CheckedChanged);
            flowLayoutPanel_CH.Controls.Add(checkBoxOnOff);
            flowLayoutPanel_CH.Controls.Add(comboBoxChannels);

        }

        private void checkBoxOnOff_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.Checked)
            {
                checkBox.BackColor = Color.LightGreen;
            }
            else
            {
                checkBox.BackColor = SystemColors.Control;
            }
        }

        public void UpdateTextBoxFromMotorMotion(IOMotion m)
        {
            comboBoxChannels.SelectedIndex = m.Channel;
            checkBoxOnOff.Checked = m.OnOff;
        }

        public void UpdateMotorMotionFromTextBox(IOMotion m)
        {
            m.Channel = comboBoxChannels.SelectedIndex;
            m.OnOff = checkBoxOnOff.Checked;
        }
    }


}
