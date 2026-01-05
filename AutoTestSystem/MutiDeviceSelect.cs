using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using AutoTestSystem.Model;
using AutoTestSystem.Base;
using Manufacture;
using System.Web.UI.WebControls;

namespace AutoTestSystem
{
    public partial class MutiDeviceSelect : Form
    {
        private string JsonDataField; // 假設這是保存 JSON 字符串的欄位

        public MutiDeviceSelect()
        {
            InitializeComponent();
            InitializeDataGridView();

            // 加載 JSON 字符串並顯示到 DataGridView 中
            if (!string.IsNullOrEmpty(JsonDataField))
            {
                LoadDataGridViewFromJson(JsonDataField);
            }
        }

        private void InitializeDataGridView()
        {
            // 設置DataGridView的列
            dataGridView1.Columns.Add("SharedName", "多工共用名稱");

            var deviceTypeColumn = new DataGridViewComboBoxColumn
            {
                Name = "DeviceType",
                HeaderText = "裝置類型",
                DataSource = new List<string> { "DUT", "ControlDevice", "IO", "Motor", "CCD", "Teach", "Image" } // 這裡添加你的裝置類型
            };
            dataGridView1.Columns.Add(deviceTypeColumn);

            var deviceObjectColumn = new DataGridViewComboBoxColumn
            {
                Name = "DeviceObject",
                HeaderText = "裝置物件"
            };
            dataGridView1.Columns.Add(deviceObjectColumn);

            // 設置事件處理器
            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;
            dataGridView1.CurrentCellDirtyStateChanged += DataGridView1_CurrentCellDirtyStateChanged;
            dataGridView1.CellClick += DataGridView1_CellClick;
            dataGridView1.RowValidating += DataGridView1_RowValidating;

            // 設置EditMode為EditOnEnter
            dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["DeviceType"].Index)
            {
                var selectedDeviceType = dataGridView1.Rows[e.RowIndex].Cells["DeviceType"].Value.ToString();
                var deviceObjectCell = (DataGridViewComboBoxCell)dataGridView1.Rows[e.RowIndex].Cells["DeviceObject"];
                // 創建一個列表來存放裝置物件的描述
                List<string> DeviceDescriptions = new List<string>();

                // 清空裝置物件的值
                deviceObjectCell.Value = null;

                // 根據選擇的裝置類型設置裝置物件的選項
                switch (selectedDeviceType)
                {
                    case "IO":
                        foreach (var value in GlobalNew.Devices.Values)
                        {
                            if (value is IOBase ioDevice)
                            {
                                DeviceDescriptions.Add(ioDevice.Description);
                            }
                        }
                        break;

                    case "ControlDevice":
                        foreach (var value in GlobalNew.Devices.Values)
                        {
                            if (value is ControlDeviceBase Device)
                            {
                                DeviceDescriptions.Add(Device.Description);
                            }
                        }
                        break;
                    case "Motor":
                        foreach (var value in GlobalNew.Devices.Values)
                        {
                            if (value is MotionBase Device)
                            {
                                DeviceDescriptions.Add(Device.Description);
                            }
                        }
                        break;
                    case "Teach":
                        foreach (var value in GlobalNew.Devices.Values)
                        {
                            if (value is TeachBase Device)
                            {
                                DeviceDescriptions.Add(Device.Description);
                            }
                        }
                        break;
                    case "CCD":
                        foreach (var value in GlobalNew.Devices.Values)
                        {
                            if (value is CCDBase Device)
                            {
                                DeviceDescriptions.Add(Device.Description);
                            }
                        }
                        break;
                    case "DUT":
                        foreach (var value in GlobalNew.Devices.Values)
                        {
                            if (value is DUT_BASE Device)
                            {
                                DeviceDescriptions.Add(Device.Description);
                            }
                        }
                        break;
                    case "Image":
                        foreach (var value in GlobalNew.Devices.Values)
                        {
                            if (value is Image_Base Device)
                            {
                                DeviceDescriptions.Add(Device.Description);
                            }
                        }
                        break;
                }

                // 將列表設置為 DataSource
                deviceObjectCell.DataSource = DeviceDescriptions;
            }
        }

        private void DataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["DeviceType"].Index || e.ColumnIndex == dataGridView1.Columns["DeviceObject"].Index)
            {
                var editingControl = dataGridView1.EditingControl as DataGridViewComboBoxEditingControl;
                if (editingControl != null)
                {
                    editingControl.DroppedDown = true;
                }
            }
        }

        private void DataGridView1_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            var row = dataGridView1.Rows[e.RowIndex];
            if (row.IsNewRow) return;

            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString()))
                {
                    dataGridView1.Rows[e.RowIndex].ErrorText = "所有欄位都必須填寫";
                    e.Cancel = true;
                    return;
                }
            }

            dataGridView1.Rows[e.RowIndex].ErrorText = string.Empty;
        }

        private void MutiDeviceSelect_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool hasData = dataGridView1.Rows
                .Cast<DataGridViewRow>()
                .Any(row => !row.IsNewRow && row.Cells.Cast<DataGridViewCell>().Any(cell => cell.Value != null && !string.IsNullOrWhiteSpace(cell.Value.ToString())));

            if (hasData)
            {
                // 檢查 SharedName 欄位是否有重複值
                var sharedNames = new HashSet<string>();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        string sharedName = row.Cells["SharedName"].Value?.ToString();
                        if (sharedName != null && !sharedNames.Add(sharedName))
                        {
                            MessageBox.Show("SharedName 欄位不能有重複的設定值。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Cancel = true;
                            return;
                        }

                        // 檢查所有欄位是否有值
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell.Value == null || string.IsNullOrEmpty(cell.Value.ToString()))
                            {
                                MessageBox.Show("所有欄位都必須要有值。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                e.Cancel = true;
                                return;
                            }
                        }
                    }
                }

                this.DialogResult = DialogResult.OK;
                // 將 DataGridView 的資料轉換為 JSON 字符串並保存到欄位中
                var jsonData = GetDataGridViewAsJson();
                this.JsonDataField = jsonData;
            }
            else
            {
                this.JsonDataField = "";
            }
        }



        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete && dataGridView1.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        dataGridView1.Rows.Remove(row);
                    }
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public string GetDataGridViewAsJson()
        {
            try
            {
                var data = dataGridView1.Rows
                .Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow)
                .Select(row => new
                {
                    SharedName = row.Cells["SharedName"].Value?.ToString(),
                    DeviceType = row.Cells["DeviceType"].Value?.ToString(),
                    DeviceObject = row.Cells["DeviceObject"].Value?.ToString()
                })
                .ToList();
                if(data.Count == 0)
                    return string.Empty;
                else
                    return JsonConvert.SerializeObject(data);
            }
            catch
            {
                return string.Empty;
            }
        }

        public void LoadDataGridViewFromJson(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                return; // 如果 jsonData 為空或 null，直接返回
            }

            try
            {
                var data = JsonConvert.DeserializeObject<List<DeviceData>>(jsonData);

                foreach (var item in data)
                {
                    int rowIndex = dataGridView1.Rows.Add();
                    var row = dataGridView1.Rows[rowIndex];
                    row.Cells["SharedName"].Value = item.SharedName;
                    row.Cells["DeviceType"].Value = item.DeviceType;


                    foreach (var value in GlobalNew.Devices.Values)
                    {
                        if (value is CoreBase coreBase && coreBase.Description == item.DeviceObject)
                        {
                            row.Cells["DeviceObject"].Value = item.DeviceObject;
                        }                                              
                    }                                      
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加載資料時發生錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public class DeviceData
        {
            public string SharedName { get; set; }
            public string DeviceType { get; set; }
            public string DeviceObject { get; set; }
        }
    }
}
