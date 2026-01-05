using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTestSystem
{
    public partial class IOSetting : Form
    {
        private Dictionary<int, Dictionary<string, object>> dataDictionary = new Dictionary<int, Dictionary<string, object>>();
        //private string Dut_path = "Config\\DUTcom.json";
        private string JSON;
        private string jsonStr = string.Empty;
        List<KeyValue> keyValueList = new List<KeyValue>();
        public IOSetting()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Save_KeyDown);
        }
        private void IOSetting_Form_Load(object sender, EventArgs e)
        {
            try
            {
                bool Load_Flag = true;
                string jsondutData = string.Empty;
                jsondutData = GetIOForm_JSON();
                if (string.IsNullOrEmpty(jsondutData))
                {
                    Load_Flag = false;
                }

                if (Load_Flag)
                {
                    keyValueList = JsonConvert.DeserializeObject<List<KeyValue>>(jsondutData);

                    foreach (var item in keyValueList)
                    {
                        IO_GridView.Rows.Add(item.SensorName, item.Channel);
                    }
                }

            }
            catch (Exception load_ex)
            {
                MessageBox.Show("Error:" + load_ex.Message, "DUTForm Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public string GetIOForm_JSON()
        {
            return JSON;
        }

        public string SetIOForm_JSON(string value)
        {
            JSON = value;
            return JSON;
        }

        public bool PushMotionListToParaList()
        {
            try
            {
                if (string.IsNullOrEmpty(JSON))
                {
                    //MessageBox.Show("MotionList Exist null or Empty", "MotionList error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else
                {
                    object selectedObject = null;
                    if (selectedObject != null)
                    {
                        Type type = selectedObject.GetType();
                        PropertyInfo[] properties = type.GetProperties();

                        foreach (PropertyInfo property in properties)
                        {
                            if (property.Name == "MotionControl_Param")
                            {
                                property.SetValue(selectedObject, JSON, null);
                                break;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No object selected.", "object error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PushMotionListToParaList Error:{ex.Message}", "object error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void IOSetting_Form_FormClosed(object sender, FormClosedEventArgs e)
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
                for (int rowIndex = 0; rowIndex < IO_GridView.Rows.Count; rowIndex++)
                {
                    // 跳過空行（如果DataGridView允許使用者添加新行的話）
                    if (IO_GridView.Rows[rowIndex].IsNewRow)
                    {
                        continue;
                    }
                    // 創建一個字典，用於存儲每一行的資料
                    Dictionary<string, object> rowDictionary = new Dictionary<string, object>();
                    // 遍歷每一列
                    foreach (DataGridViewCell cell in IO_GridView.Rows[rowIndex].Cells)
                    {
                        // 使用列名作為鍵，儲存該列的值
                        if (cell.Value != null)
                        {
                            rowDictionary[IO_GridView.Columns[cell.ColumnIndex].Name] = cell.Value;
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

                SetIOForm_JSON(jsonStr);

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
        private void IO_GridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                string clipboardText = Clipboard.GetText();
                if (string.IsNullOrEmpty(clipboardText))
                    return;

                string[] lines = clipboardText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                int startRow = IO_GridView.CurrentCell?.RowIndex ?? 0;
                int startCol = IO_GridView.CurrentCell?.ColumnIndex ?? 0;

                // 先計算需要的總行數
                int requiredRowCount = startRow + lines.Length;
                if (requiredRowCount > IO_GridView.RowCount)
                {
                    IO_GridView.Rows.Add(requiredRowCount - IO_GridView.RowCount);
                }

                for (int i = 0; i < lines.Length; i++)
                {
                    string[] cells = lines[i].Split('\t');
                    int row = startRow + i;

                    for (int j = 0; j < cells.Length; j++)
                    {
                        int col = startCol + j;
                        if (col < IO_GridView.ColumnCount)
                        {
                            IO_GridView[col, row].Value = cells[j];
                        }
                    }
                }
            }

        }



        public Dictionary<string, string> ConvertDictionaryForm(string json_str)
        {
            try
            {
                Dictionary<string, string> Temp_data = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(json_str))
                {
                    var data = JsonConvert.DeserializeObject<List<KeyValue>>(json_str);

                    foreach (var sensor in data)
                    {
                        if (Temp_data.ContainsKey(sensor.SensorName))
                            Temp_data[sensor.SensorName] = sensor.Channel;
                        else
                            Temp_data.Add(sensor.SensorName, sensor.Channel);
                    }
                }
                if (Temp_data.Count > 0)
                    return Temp_data;
                else
                    return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex.Message, "ConvertDictionaryForm Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public class KeyValue
        {
            public string SensorName { get; set; }
            public string Channel { get; set; }
        }

        private void Save_KeyDown(object sender, KeyEventArgs e)
        {
            // 檢查是否按下 Delete 鍵
            if (e.KeyCode == Keys.Delete)
            {
                foreach (DataGridViewCell cell in IO_GridView.SelectedCells)
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
