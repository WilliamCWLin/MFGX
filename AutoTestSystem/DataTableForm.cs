
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTestSystem
{
    public partial class DataTableForm : Form
    {
        public string JsonData { get; private set; } = null;
        public DataTableForm(string jsonData = null)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(jsonData))
            {
                JsonData = jsonData;
                DataTable dataTable = JsonConvert.DeserializeObject<DataTable>(JsonData);
                PropDataGridView.DataSource = dataTable;
                // 確保在設置 DataSource 之後調用
                PropDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                PropDataGridView.AutoResizeColumnHeadersHeight();

            }

            PropDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
            PropDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.LightBlue;
            PropDataGridView.EnableHeadersVisualStyles = false; // 禁用視覺樣式以應用自定義樣式
        }

        private void LoadBTN_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    DataTable dataTable = LoadCsv(filePath);
                    PropDataGridView.DataSource = dataTable;
                    PropDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    PropDataGridView.AutoResizeColumnHeadersHeight();
                }
            }
        }

        private DataTable LoadCsv(string filePath)
        {
            DataTable dataTable = new DataTable();
            using (StreamReader sr = new StreamReader(filePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dataTable.Columns.Add(header);
                }

                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = dataTable.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dataTable.Rows.Add(dr);
                }
            }
            return dataTable;
        }

        private void PropDataTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            DataTable dataTable = (DataTable)PropDataGridView.DataSource;
            if (dataTable != null)
            {
                JsonData = JsonConvert.SerializeObject(dataTable);
                this.DialogResult = DialogResult.OK; // 設置 DialogResult 為 OK
            }
            else
            {
                this.DialogResult = DialogResult.Cancel; // 如果沒有數據，設置為 Cancel
            }
        }

        private void PropDataTable_Shown(object sender, EventArgs e)
        {
            // 確保在設置 DataSource 之後調用
            PropDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            PropDataGridView.AutoResizeColumnHeadersHeight();
        }
    }
}
