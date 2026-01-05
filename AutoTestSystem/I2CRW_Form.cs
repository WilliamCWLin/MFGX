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
    public partial class I2CRW_Form : Form
    {
        private Dictionary<int, Dictionary<string, object>> dataDictionary = new Dictionary<int, Dictionary<string, object>>();
        //private string Dio_path = "Config\\DIO_Param.json";
        private string JSON;
        private string jsonStr = string.Empty;
        public List<Key_Value> keyValueList = new List<Key_Value>();
        //public List<AddressData> AddressDataValueList = new List<AddressData>();
        public I2CRW_Form()
        {
            InitializeComponent();
        }

        private void SaveData()
        {
            try
            {
                bool checkpair = true;
                jsonStr = string.Empty;
                Dictionary<int, int> result = new Dictionary<int, int>();
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

                SetParam(jsonStr);

                if (!string.IsNullOrEmpty(jsonStr))
                {
                    List<Dictionary<string, string>> keyValuePairs = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonStr);
                    foreach (var pair in keyValuePairs)
                    {
                        if (pair.Count <= 2)
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
                //File.WriteAllText(Dio_path, jsonStr);
                this.Close();
            }
            catch (Exception save_ex)
            {
                MessageBox.Show("Error:" + save_ex.Message, "DIOParam Form Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        public string GetParam()
        {
            return JSON;
        }

        public string SetParam(string value)
        {
            JSON = value;
            return JSON;
        }

        public class Key_Value
        {
            public string KeyName { get; set; }
            public int Hight_Bytes { get; set; }
            public int Low_Bytes { get; set; }
        }

       /* public class AddressData
        {
            public string KeyName { get; set; }
            public string Address { get; set; }
            public string Data { get; set; }
        }*/

        private void I2CRW_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveData();
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
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

        private void I2CRW_Form_Load(object sender, EventArgs e)
        {
            try
            {
                bool Load_Flag = true;
                string jsondioData = string.Empty;
                jsondioData = GetParam();
                if (string.IsNullOrEmpty(jsondioData)) 
                {
                    Load_Flag = false;
                }
                
                if (Load_Flag)
                {                  
                    keyValueList = JsonConvert.DeserializeObject<List<Key_Value>>(jsondioData);

                    foreach (var item in keyValueList)
                    {
                       dataGridView1.Rows.Add(item.KeyName, item.Hight_Bytes, item.Low_Bytes);
                    }
                }

            }
            catch (Exception load_ex)
            {
                MessageBox.Show("Error:" + load_ex.Message, "DIOParam Form Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
