using ClosedXML.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTestSystem
{
    public partial class TableForm : Form
    {
        public TableForm(string json)
        {
            InitializeComponent();
            PopulateDataGridViewFromJson(json, dataGridView1);
        }

        private void TableForm_Load(object sender, EventArgs e)
        {
            // 設定 DataGridView 右鍵選單
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            // 載入檔案選項
            ToolStripMenuItem loadExcelMenuItem = new ToolStripMenuItem("載入 Excel 檔案");

            loadExcelMenuItem.Click += LoadExcelMenuItem_Click;


            // 把選項加到右鍵選單

            contextMenu.Items.Add(loadExcelMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator()); // 分隔線

            // 設定 DataGridView 使用 ContextMenuStrip
            dataGridView1.ContextMenuStrip = contextMenu;


        }
  
        private void DataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // 確保點擊的是列標題
            if (e.RowIndex == -1) // -1 代表點擊的是列標題
            {
                // 取得被點擊的列
                DataGridViewColumn column = dataGridView1.Columns[e.ColumnIndex];

                // 選擇該列
                dataGridView1.ClearSelection();
                column.Selected = true; // 或者 column.Cells[0].Selected = true;
            }
        }
        // 處理載入 Excel 檔案
        private void LoadExcelMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 使用 XLWorkbook 讀取 Excel 檔案
                    using (var workbook = new XLWorkbook(openFileDialog.FileName))
                    {
                        var worksheet = workbook.Worksheets.Worksheet(1);
                        var dataTable = new DataTable();

                        // 讀取 Excel 第一行作為欄位名稱
                        bool firstRow = true;
                        foreach (var row in worksheet.Rows())
                        {
                            if (firstRow)
                            {
                                foreach (var cell in row.Cells())
                                {
                                    dataTable.Columns.Add(cell.Value.ToString());
                                }
                                firstRow = false;
                            }
                            else
                            {
                                var dataRow = dataTable.NewRow();
                                int columnIndex = 0;
                                foreach (var cell in row.Cells())
                                {
                                    dataRow[columnIndex++] = cell.Value.ToString();
                                }
                                dataTable.Rows.Add(dataRow);
                            }
                        }

                        // 設置 DataGridView 資料來源
                        dataGridView1.DataSource = dataTable;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("載入 Excel 檔案失敗: " + ex.Message);
                }
            }
        }
        public string ConvertDataGridViewToJson()
        {
            try
            {
                if(dataGridView1.Rows.Count == 0)
                    return string.Empty;

                // 建立字典以存放 Key-Value
                var resultDict = new Dictionary<string, string>();

                // 遍歷 DataGridView 的每一行
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    // 跳過空行或新行
                    if (row.IsNewRow) continue;

                    // 取得第 1 欄 (Key) 和第 2 欄 (Value) 的值
                    var key = row.Cells[0].Value?.ToString();
                    var value = row.Cells[1].Value?.ToString();

                    // 如果 Key 不為空，且字典中尚未包含該 Key，則加入字典
                    if (!string.IsNullOrEmpty(key) && !resultDict.ContainsKey(key))
                    {
                        resultDict[key] = value ?? string.Empty; // 如果 Value 為空則設為空字串
                    }
                }

                // 使用 Newtonsoft.Json 序列化為 JSON 字串
                return JsonConvert.SerializeObject(resultDict);
            }
            catch (Exception ex)
            {
                // 回傳錯誤訊息的 JSON 格式
                return string.Empty;
            }
        }
        // 刪除選取的行
        private void DeleteRowMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                int rowIndex = dataGridView1.SelectedCells[0].RowIndex;
                // 確保選擇的行有效
                if (rowIndex >= 0)
                {
                    dataGridView1.Rows.RemoveAt(rowIndex);
                }
            }
            else
            {
                MessageBox.Show("請選擇一行來刪除");
            }
        }

        // 刪除選取的列
        private void DeleteColumnMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                int columnIndex = dataGridView1.SelectedCells[0].ColumnIndex;
                // 確保選擇的列有效
                if (columnIndex >= 0)
                {
                    dataGridView1.Columns.RemoveAt(columnIndex);
                }
            }
            else
            {
                MessageBox.Show("請選擇一列來刪除");
            }
        }
        public void PopulateDataGridViewFromJson(string json, DataGridView dataGridView)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                    return;
                // 清空 DataGridView 的資料
                dataGridView.DataSource = null;
                dataGridView.Rows.Clear();
                dataGridView.Columns.Clear();

                // 建立欄位 (第 1 欄: Key，第 2 欄: Value)
                dataGridView.Columns.Add("KeyColumn", "Key");
                dataGridView.Columns.Add("ValueColumn", "Value");

                // 將 JSON 反序列化為字典
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                // 遍歷字典並將資料加入 DataGridView
                foreach (var kvp in dictionary)
                {
                    dataGridView.Rows.Add(kvp.Key, kvp.Value);
                }

                // 調整 DataGridView 欄位的自動填充模式
                dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("載入 JSON 至 DataGridView 失敗: " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void TableForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 顯示確認對話框
            var dialogResult = MessageBox.Show(
                "要儲存數據嗎？",
                "儲存確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            // 根據用戶選擇執行操作
            if (dialogResult == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                // 用戶選擇不儲存
                this.DialogResult = DialogResult.Cancel;
            }
        }
    }
}
