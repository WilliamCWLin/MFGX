using AutoTestSystem.Base;
using AutoTestSystem.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Drawing;

namespace AutoTestSystem.DevicesUI.Teach
{
    public partial class DeviceSelectionForm : Form
    {
        private Dictionary<string, object> Devices;
        private CheckedListBox checkedListBoxKeys;
        private ComboBox comboBoxOriginMode;
        private TableLayoutPanel tableLayoutPanel;

        public DeviceSelectionForm(List<string> selectedKeys)
        {
            InitializeComponent();
            Devices = GlobalNew.Devices;
            selectedKeys = selectedKeys ?? new List<string>(); // 防止 selectedKeys 是空的情況
            InitializeTableLayoutPanel();
            InitializeComboBoxOriginMode();
            InitializeCheckedListBox(selectedKeys);
        }

        private void InitializeTableLayoutPanel()
        {
            tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Height = 300,
            };
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            Controls.Add(tableLayoutPanel);
        }

        private void InitializeComboBoxOriginMode()
        {
            comboBoxOriginMode = new ComboBox
            {
                Name = "comboBoxOriginMode",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Top,
                Width = 50
            };

            comboBoxOriginMode.Items.Add("Mechanical Origin");
            comboBoxOriginMode.Items.Add("Program Origin");
            comboBoxOriginMode.SelectedIndex = 0;
            //tableLayoutPanel.Controls.Add(comboBoxOriginMode, 0, 1);
        }

        private void InitializeCheckedListBox(List<string> selectedKeys)
        {
            checkedListBoxKeys = new CheckedListBox
            {
                Dock = DockStyle.Fill
            };

            foreach (var key in Devices.Keys)
            {
                var value = Devices[key];
                if (value is MotionBase || value is IOBase) // Replace YourDesiredType with the actual type you want to check
                {
                    checkedListBoxKeys.Items.Add(key);
                    if (selectedKeys.Contains(key))
                    {
                        checkedListBoxKeys.SetItemChecked(checkedListBoxKeys.Items.IndexOf(key), true);
                    }
                }
            }

            checkedListBoxKeys.SelectedIndexChanged += CheckedListBoxKeys_SelectedIndexChanged;
            tableLayoutPanel.Controls.Add(checkedListBoxKeys, 0, 0);
        }

        private void CheckedListBoxKeys_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkedListBoxKeys.SelectedIndex != -1)
            {
                int index = checkedListBoxKeys.SelectedIndex;
                bool isChecked = checkedListBoxKeys.GetItemChecked(index);
                checkedListBoxKeys.SetItemChecked(index, !isChecked);
            }
        }

        public List<string> GetSelectedKeys()
        {
            List<string> selectedKeys = new List<string>();
            foreach (var item in checkedListBoxKeys.CheckedItems)
            {
                selectedKeys.Add(item.ToString());
            }
            return selectedKeys;
        }

        public string GetSelectedOriginMode()
        {
            return comboBoxOriginMode.SelectedItem?.ToString();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            var result = MessageBox.Show("Do you want to save the settings?", "Save Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }
    }


}
