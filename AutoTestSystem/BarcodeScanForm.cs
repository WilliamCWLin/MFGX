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
    public partial class BarcodeScanForm : Form
    {
        public bool Barcode_pass_fail = false;
        private string BarcodeStr;
        string scannedData = string.Empty;

        public TextBox BarcodeTextbox
        {
            get { return textBoxScan; }
            set { textBoxScan = value; }
        }

        public BarcodeScanForm()
        {
            InitializeComponent();
        }

        private void BarcodeScanForm_Load(object sender, EventArgs e)
        {
            // 設定 TextBox 專注於掃碼輸入
            textBoxScan.Focus();
            /*string savedSelection = Properties.Settings.Default.ComboBoxSelection;
            if (!string.IsNullOrEmpty(savedSelection))
            {
                Torque_List.SelectedItem = savedSelection;
            }*/

        }

        private void textBoxScan_KeyDown(object sender, KeyEventArgs e)
        {
            bool checksum_flag = false;
            scannedData = string.Empty;
            
            if (e.KeyCode == Keys.Enter)
            {

                // 獲取掃到的字串
                scannedData = textBoxScan.Text.Trim();
                // 將掃到的資料記錄到 ListBox 中
                if (!string.IsNullOrEmpty(scannedData))
                {
                    //ScannedData.Items.Add(scannedData);
                    checksum_flag = CheckSum();
                    textBoxScan.Clear(); // 清空輸入欄位以便下一次掃描
                    if (checksum_flag)
                    {
                       this.Close();
                    }                   
                }
                // 設定專注於 TextBox 以準備下一次掃描
                textBoxScan.Focus();

            }
        }

        public bool CheckSum()
        {
            string folderPath = @"BarcodeCheckSum\\";
            string filePath = Path.Combine(folderPath, "BarCode.txt");
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("文件夹不存在，将创建新文件夹。");
                Directory.CreateDirectory(folderPath);
            }

            // 檢查文件是否存在，若不存在則建立
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose(); // 使用 Dispose() 來釋放資源
                Console.WriteLine($"文件 {filePath} 已建立。");
            }

            try
            {
                // 讀取文件的所有內容
                string fileContent = File.ReadAllText(filePath);
                // 以換行符號區分字串
                string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                if (lines.Length == 0 || lines.Count() == 0)
                {
                    MessageBox.Show("不可讀取空的文件","txtfile",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    return false;
                }

                /*存取txt裡的 keyName(;)以及Value 
                => txt Format:
                ;VD5005_Indoor
                  MSCOLL5632
                ; VD5005_Outdoor
                  BLDOKK8555
                ; VD5006_Indoor
                  IFREFNL6544
                ; VD5006_Outdoor
                  ZDTGHJI2149
                */
                Dictionary<string, List<string>> barcodeDict = new Dictionary<string, List<string>>();
                string currentKey = null;

                foreach (string line in lines)
                {
                    if (line.StartsWith(";"))
                    {
                        currentKey = line.Trim(';');
                        if (!barcodeDict.ContainsKey(currentKey))
                        {
                            barcodeDict[currentKey] = new List<string>();
                        }
                    }
                    else if (!string.IsNullOrEmpty(line) && currentKey != null)
                    {
                        barcodeDict[currentKey].Add(line);
                    }
                }

                bool found = false;
                foreach (var key in barcodeDict.Keys)
                {
                    if (!string.IsNullOrEmpty(BarcodeStr) && BarcodeStr.Contains(key))
                    {
                        if (barcodeDict[key].Contains(scannedData))
                        {
                            Barcode_pass_fail = true;
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                    MessageBox.Show($"The Barcode \"{scannedData}\" is incorrect Please Scan again!", "txtfile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Console.WriteLine("讀取文件時發生錯誤: " + ex.Message);
                MessageBox.Show("讀取文件時發生錯誤: " + ex.Message,"txtfile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return Barcode_pass_fail;

        }
        public void BarcodeCmd(string Barcode_cmd)
        {
            BarcodeStr = Barcode_cmd;
        }
        public bool BarcodeResult()
        {
            return Barcode_pass_fail;
        }
        private void BarcodeScanForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Barcode_pass_fail)
            {
                // 取消關閉事件
                e.Cancel = true;
                MessageBox.Show("Don't close the window!", "BarcodeScanForm",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }

        private void Torque_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 保存選擇
            //Properties.Settings.Default.ComboBoxSelection = Torque_List.SelectedItem.ToString();
            //Properties.Settings.Default.Save();

        }
    }
}
