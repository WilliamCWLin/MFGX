
using AutoTestSystem.Base;
using AutoTestSystem.Model;
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
    public partial class Form_SetVariable : Form
    {
        private Dictionary<int, Dictionary<string, object>> dataDictionary = new Dictionary<int, Dictionary<string, object>>();
        //private string Dut_path = "Config\\DUTcom.json";
        private string JSON;
        private string jsonStr = string.Empty;
        List<KeyValue> keyValueList = new List<KeyValue>();
        public Form_SetVariable()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Save_KeyDown);
        }

        private void DUTForm_Load_1(object sender, EventArgs e)
        {
            try
            {
                bool Load_Flag = true;
                string jsondutData = string.Empty;
                jsondutData = GetDUTForm_JSON();
                if (string.IsNullOrEmpty(jsondutData))
                {
                    Load_Flag = false;
                }

                if (Load_Flag)
                {
                    keyValueList = JsonConvert.DeserializeObject<List<KeyValue>>(jsondutData);

                    foreach (var item in keyValueList)
                    {
                        dataGridView1.Rows.Add(item.Key, item.Value);
                    }
                }

            }
            catch (Exception load_ex)
            {
                MessageBox.Show("Error:" + load_ex.Message, "DUTForm Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string GetDUTForm_JSON()
        {
            return JSON;
        }

        public string SetDUTForm_JSON(string value)
        {
            JSON = value;
            return JSON;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            SaveData();
        }

        private void DUTForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveData();
        }
        private void SaveData()
        {
            try
            {
                bool checkpair = true;
                jsonStr = string.Empty;
                // 清空字典
                dataDictionary.Clear();
                // 遍歷DataGridView的每一列
                for (int rowIndex = 0; rowIndex < dataGridView1.Rows.Count; rowIndex++)
                {
                    // 跳過空行（如果DataGridView允許使用者添加新行的話）
                    if (dataGridView1.Rows[rowIndex].IsNewRow)
                    {
                        continue;
                    }
                    // 創建一個字典，用於存儲每一行的資料
                    Dictionary<string, object> rowDictionary = new Dictionary<string, object>();
                    // 遍歷每一列
                    foreach (DataGridViewCell cell in dataGridView1.Rows[rowIndex].Cells)
                    {
                        // 使用列名作為鍵，儲存該列的值
                        if (cell.Value != null)
                        {
                            rowDictionary[dataGridView1.Columns[cell.ColumnIndex].Name] = cell.Value;
                        }
                    }

                    // 將行Dictionary添加到主字典中，使用行索引作為鍵
                    if (rowDictionary.Count > 0)
                    {
                        dataDictionary[rowIndex] = rowDictionary;
                        // Convert the dictionary to a JSON string
                        jsonStr = JsonConvert.SerializeObject(dataDictionary.Values);
                    }
                }

                SetDUTForm_JSON(jsonStr);

                if (!string.IsNullOrEmpty(jsonStr))
                {
                    List<Dictionary<string, string>> keyValuePairs = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonStr);
                    foreach (var pair in keyValuePairs)
                    {
                        if (pair.Count <= 1)
                        {
                            checkpair = false;
                            break;
                        }
                    }
                }

                if (checkpair)
                {
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    DialogResult = DialogResult.Cancel;
                }
                // Write the JSON string to a file
                //File.WriteAllText(Dut_path, jsonStr);
                this.Close();
            }
            catch (Exception save_ex)
            {
                MessageBox.Show("Error:" + save_ex.Message, "DUTForm Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        public class KeyValue
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        private void Save_KeyDown(object sender, KeyEventArgs e)
        {
            // 檢查是否按下 Delete 鍵
            if (e.KeyCode == Keys.Delete)
            {
                foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
                {
                    // 將選中欄位的值設為 null
                    cell.Value = null;
                }
            }

            // 檢查是否按下 Ctrl+S
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveData();
                e.SuppressKeyPress = true; // 防止發出 "叮" 的聲音
            }
        }
    }
}

