using AutoTestSystem.Base;
using AutoTestSystem.Model;
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
    public partial class MotionDeviceSelectionForm : Form
    {
        private Dictionary<string, object> Devices;
        private CheckedListBox checkedListBoxKeys;

        public MotionDeviceSelectionForm()
        {
            InitializeComponent();
            Devices = GlobalNew.Devices;
            InitializeCheckedListBox();
        }

        private void InitializeCheckedListBox()
        {
            checkedListBoxKeys = new CheckedListBox
            {
                Dock = DockStyle.Fill
            };

            foreach (var key in Devices.Keys)
            {

                var value = Devices[key];
                if (value is MotionBase || value is Io) 
                {
                    checkedListBoxKeys.Items.Add(key);
                }

            }

            checkedListBoxKeys.ItemCheck += CheckedListBoxKeys_ItemCheck;
            Controls.Add(checkedListBoxKeys);
        }

        private void CheckedListBoxKeys_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string key = checkedListBoxKeys.Items[e.Index].ToString();
            if (e.NewValue == CheckState.Checked)
            {
                MessageBox.Show($"Selected: {key}");
            }
            else
            {
                MessageBox.Show($"Deselected: {key}");
            }
        }
    }
}
