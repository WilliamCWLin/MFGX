using AutoTestSystem.Base;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Equipment.Teach;
using AutoTestSystem.Model;
using Newtonsoft.Json;
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
    public partial class MutiIOSelect : Form
    {
        private string DevicesName = string.Empty;
        private string JsonDataField; // 假設這是保存 JSON 字符串的欄位
        public MutiIOSelect(string Devices_Name)
        {
            DevicesName = Devices_Name;
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
            // 創建第一個 DataGridViewComboBoxColumn 並設置資料來源為 List<string>
            var IONameListColumn = new DataGridViewComboBoxColumn
            {
                Name = "IOName",
                HeaderText = "IO名稱",
                DataSource = IO_StatusColumnList() // 這裡添加你的裝置類型
            };
            IO_DataGridView.Columns.Add(IONameListColumn);

            // 創建第二個 DataGridViewComboBoxColumn 並設置資料來源為 bool 值
            var IO_StatusColumn = new DataGridViewComboBoxColumn
            {
                Name = "IOStatus",
                HeaderText = "IO狀態",
                DataSource = new List<string> { "False", "True" } // 添加True和False選項
            };
            IO_DataGridView.Columns.Add(IO_StatusColumn);

            // 設置 DataError 事件處理程序
            IO_DataGridView.DataError += (sender, e) =>
            {
                // 處理數據錯誤
                if (e.Exception != null && e.Context == DataGridViewDataErrorContexts.Commit)
                {
                    MessageBox.Show("無效的數據輸入。請確保所有值都在下拉列表中。");
                    e.ThrowException = false;
                }
            };

        }

        public List<string> IO_StatusColumnList()
        {
            // 創建一個列表來存放裝置物件的描述
            List<string> DeviceDescriptions = new List<string>();
            Dictionary<string, string> sensors_List = new Dictionary<string, string>();

            //List<Sensor> sensors = new List<Sensor>();


            if (string.IsNullOrEmpty(DevicesName))
            {
                MessageBox.Show("Can not Find IO DevicesName!!");
                return null;
            }


            if (GlobalNew.Devices.ContainsKey(DevicesName))
            {
                object deviceObj = GlobalNew.Devices[DevicesName];

                if (deviceObj is IOTeach ioTeach)
                {
                    // 是 IOTeach，使用 GetGetIOKeys()
                    DeviceDescriptions.AddRange(ioTeach.GetGetIOKeys());
                }
                else if (deviceObj is IOBase ioBase)
                {
                    // 是 IOBase，使用 GetDIForm()
                    if (!string.IsNullOrWhiteSpace(ioBase.GetIO_List))
                    {
                        sensors_List = ioBase.GetDIForm();
                        foreach (var sensor in sensors_List)
                        {
                            DeviceDescriptions.Add(sensor.Key);
                        }
                    }
                }
            }


            return DeviceDescriptions;
        }


        private void IO_DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == IO_DataGridView.Columns["IOStatus"].Index)
            {
                var selectedValue = IO_DataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                var deviceObjectCell = (DataGridViewComboBoxCell)IO_DataGridView.Rows[e.RowIndex].Cells["IOStatus"];

                if (selectedValue != null)
                {
                    // 保持選擇的值
                    IO_DataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = selectedValue;
                }

                // 將列表設置為 DataSource，保持 "False" 和 "True" 選項
                deviceObjectCell.DataSource = new List<string> { "False", "True" };
            }
        }


        private void IO_DataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (IO_DataGridView.IsCurrentCellDirty)
            {
                IO_DataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }

        }

        private void IO_DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == IO_DataGridView.Columns["IOName"].Index || e.ColumnIndex == IO_DataGridView.Columns["IOStatus"].Index)
            {
                var editingControl = IO_DataGridView.EditingControl as DataGridViewComboBoxEditingControl;
                if (editingControl != null)
                {
                    editingControl.DroppedDown = true;
                }
            }
        }

        public string GetDataGridViewAsJson()
        {
            try
            {
                var data = IO_DataGridView.Rows
                .Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow)
                .Select(row => new
                {
                    IO_Name = row.Cells["IOName"].Value?.ToString(),
                    IO_Status = row.Cells["IOStatus"].Value?.ToString(),
                })
                .ToList();
                if (data.Count == 0)
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
                    int rowIndex = IO_DataGridView.Rows.Add();
                    var row = IO_DataGridView.Rows[rowIndex];
                    row.Cells["IOName"].Value = item.IO_Name;
                    row.Cells["IOStatus"].Value = item.IO_Status;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加載資料時發生錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void MutiIOSelect_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool hasData = IO_DataGridView.Rows
               .Cast<DataGridViewRow>()
               .Any(row => !row.IsNewRow && row.Cells.Cast<DataGridViewCell>().Any(cell => cell.Value != null && !string.IsNullOrWhiteSpace(cell.Value.ToString())));

            if (hasData)
            {
                // 檢查 SharedName 欄位是否有重複值
                var sharedNames = new HashSet<string>();
                foreach (DataGridViewRow row in IO_DataGridView.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        string IO_Name = row.Cells["IOName"].Value?.ToString();
                        if (IO_Name != null && !sharedNames.Add(IO_Name))
                        {
                            MessageBox.Show("IOName 欄位不能有重複的設定值。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
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


        public class DeviceData
        {
            public string IO_Name { get; set; }
            public string IO_Status { get; set; }
        }


    }
}
