using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Manufacture
{
    public partial class SpecForm_SingleEntry : Form
    {
        private List<dynamic> specParamsList = new List<dynamic>();
        private List<dynamic> itemParamsList = new List<dynamic>();
        Dictionary<string, List<string>> section__itemDic = new Dictionary<string, List<string>>();

        public SpecForm_SingleEntry(string DataName, string origin)
        {
            InitializeComponent();

            List<string> nameList = new List<string>();
            List<string> nameList2 = new List<string>();

            // 載入原本的 JSON 字串
            if (!string.IsNullOrWhiteSpace(origin))
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(origin);
                    if (data != null && data.ContainsKey("specParams"))
                    {
                        specParamsList = data["specParams"].Cast<dynamic>().ToList();
                        // 將 origin 中的鍵名稱添加到 nameList 中
                        //nameList.AddRange(specParamsList.Select(p => (string)p["Name"]));
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show("Error loading spec parameters: " + ex.Message);
                }
            }

            // 處理 DataName JSON 字串
            if (!string.IsNullOrEmpty(DataName))
            {
                List<string> partsList1 = new List<string>();
                //List<string> partsList2 = new List<string>();
                JObject jsonData = JObject.Parse(DataName);
                var keys = jsonData.Properties().Select(p => p.Name).ToList();
                // 將 DataName 中的鍵名稱添加到 nameList 中
                foreach (var key_name in keys)
                {
                    string[] parts = key_name.Split(new string[] { "__" }, StringSplitOptions.None);
                    partsList1.Add(parts[0]);
                    //partsList2.Add(parts[1]);
                    if (!section__itemDic.ContainsKey(parts[0]))
                    {
                        section__itemDic[parts[0]] = new List<string>();
                    }
                    section__itemDic[parts[0]].Add(parts[1]);

                }
                nameList.AddRange(partsList1);
                //nameList2.AddRange(partsList2);
            }

            // 使用 Distinct() 確保名稱是唯一的，然後添加到 comboBox_DataName 中
            foreach (var name in nameList.Distinct())
            {
                //string[] parts = name.Split(new string[] { "__" }, StringSplitOptions.None);

                comboBox_DataName.Items.Add(name);
                //comboBox1.Items.Add(parts[1]);
            }
            //foreach (var name in nameList2)
            //{
                //comboBox1.Items.Add(name);
            //}    

            if (comboBox_SpecType.Items.Count > 0)
            {
                comboBox_SpecType.SelectedIndex = 0;
            }

            richTextBox_Spec.Text = origin;
        }

        private void AddSpecBtn_Click(object sender, EventArgs e)
        {
            // 創建或更新 specParam 物件
            string selectedName = comboBox_DataName.Text + "__" + comboBox1.Text;
            if (string.IsNullOrEmpty(selectedName) || selectedName == "__")
                return;
            Dictionary<string, object> specParam = specParamsList.Find(p => p.ContainsKey("Name") && p["Name"].ToString() == selectedName);

            // 如果 specParam 為 null，則創建一個新的物件
            if (specParam == null)
            {
                specParam = new Dictionary<string, object>();
                specParam["Name"] = selectedName;
                specParamsList.Add(specParam);
                //if (!comboBox_DataName.Items.Contains(selectedName))
                //{
                //    comboBox_DataName.Items.Add(selectedName);
                //}
            }
            if (comboBox_SpecType.SelectedItem == null)
            {
                MessageBox.Show("Spec Type Error");
                return;
            }


            // 更新 specParam 的值
            string SelectType = comboBox_SpecType.SelectedItem.ToString();
            var dictionary = (IDictionary<string, object>)specParam;
            if (SelectType == "[min,max]")
            {
                double minLimit, maxLimit;
                if (double.TryParse(txtMinLimit.Text, out minLimit) && double.TryParse(txtMaxLimit.Text, out maxLimit))
                {
                    if (maxLimit > minLimit)
                    {
                        dictionary["SpecType"] = "Range";
                        dictionary["MinLimit"] = minLimit;
                        dictionary["MaxLimit"] = maxLimit;
                        dictionary.Remove("SpecValue");
                    }
                    else
                    {
                        MessageBox.Show("MaxLimit must be greater than MinLimit.");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Please enter valid numeric values for MinLimit and MaxLimit.");
                    return;
                }
            }
            else if (SelectType == "=")
            {
                dictionary["SpecType"] = "Equal";
                dictionary["SpecValue"] = SpecValue.Text;
                dictionary.Remove("MinLimit");
                dictionary.Remove("MaxLimit");
            }
            else if (SelectType == ">")
            {
                double value;
                if (double.TryParse(SpecValue.Text, out value))
                {
                    dictionary["SpecType"] = "GreaterThan";
                    dictionary["SpecValue"] = SpecValue.Text;
                    dictionary.Remove("MinLimit");
                    dictionary.Remove("MaxLimit");
                }
                else
                {
                    MessageBox.Show("SpecValue must be numeric.");
                    return;
                }

            }
            else if (SelectType == "<")
            {
                double value;
                if (double.TryParse(SpecValue.Text, out value))
                {
                    dictionary["SpecType"] = "LessThan";
                    dictionary["SpecValue"] = SpecValue.Text;
                    dictionary.Remove("MinLimit");
                    dictionary.Remove("MaxLimit");
                }
                else
                {
                    MessageBox.Show("SpecValue must be numeric.");
                    return;
                }
            }
            else if (SelectType == "Bypass")
            {
                dictionary["SpecType"] = "Bypass";
                dictionary["SpecValue"] = SpecValue.Text;
                dictionary.Remove("MinLimit");
                dictionary.Remove("MaxLimit");
                dictionary.Remove("SpecValue");
            }

            if (checkBox_CSV.Checked == false)
                dictionary["CSV"] = "OFF";
            else
                dictionary.Remove("CSV");
            if (checkBox_MES.Checked == false)
                dictionary["MES"] = "OFF";
            else
                dictionary.Remove("MES");

            // Serialize to JSON and display in RichTextBox
            string jsonOutput = JsonConvert.SerializeObject(new { specParams = specParamsList }, Formatting.Indented);
            richTextBox_Spec.Text = jsonOutput;

            int len = richTextBox_Spec.Text.IndexOf($"\"{selectedName.Trim()}\"");
            if (len > 0)
            {
                richTextBox_Spec.Select(len + 1, selectedName.Length);
                richTextBox_Spec.ScrollToCaret();

                richTextBox_Spec.SelectionBackColor = Color.LightGray;
            }
        }
        private void comboBox_SpecType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isRange = comboBox_SpecType.SelectedItem.ToString() == "[min,max]";
            txtMinLimit.Visible = isRange;
            txtMaxLimit.Visible = isRange;
            label3.Visible = isRange;
            SpecValue.Visible = !isRange;

            bool isBypass = comboBox_SpecType.SelectedItem.ToString() == "Bypass";
            if (isBypass)
                SpecValue.Text = "";
            SpecValue.Enabled = !isBypass;
        }

        private void comboBox_DataName_SelectedIndexChanged(object sender, EventArgs e)
        {

            string selectedName = comboBox_DataName.SelectedItem.ToString();
            Dictionary<string, object> specParam = specParamsList.Find(p => p.ContainsKey("Name") && p["Name"].ToString() == selectedName);

            comboBox1.Items.Clear();
            if (section__itemDic.ContainsKey(selectedName))
            {
                List<string> values = section__itemDic[selectedName];
                foreach (string value in values)
                {
                    comboBox1.Items.Add(value);
                }
            }

            richTextBox_Spec.SelectAll();
            richTextBox_Spec.SelectionBackColor = Color.White;
            if (specParam != null)
            {

                // 設置 SpecType ComboBox 的選項
                comboBox_SpecType.SelectedItem = specParam["SpecType"];

                // 根據 SpecType 設置 MinLimit 和 MaxLimit 的可見性
                bool isRange = specParam["SpecType"].ToString() == "Range";
                txtMinLimit.Visible = isRange;
                txtMaxLimit.Visible = isRange;
                label3.Visible = isRange;
                SpecValue.Visible = !isRange;

                // 如果是 Range，設置 MinLimit 和 MaxLimit 的值
                if (isRange)
                {
                    txtMinLimit.Text = specParam["MinLimit"].ToString();
                    txtMaxLimit.Text = specParam["MaxLimit"].ToString();
                    comboBox_SpecType.SelectedIndex = 1;
                }
                else
                {
                    if (specParam["SpecType"].ToString() == "Equal")
                    {
                        comboBox_SpecType.SelectedIndex = 0;
                    }
                    else if (specParam["SpecType"].ToString() == "GreaterThan")
                    {
                        comboBox_SpecType.SelectedIndex = 2;
                    }
                    else if (specParam["SpecType"].ToString() == "LessThan")
                    {
                        comboBox_SpecType.SelectedIndex = 3;
                    }
                    else if (specParam["SpecType"].ToString() == "Bypass")
                    {
                        comboBox_SpecType.SelectedIndex = 4;
                        SpecValue.Text = "";
                    }

                    bool isBypass = specParam["SpecType"].ToString() == "Bypass";

                    if (!isBypass)
                    {
                        if (specParam.ContainsKey("SpecValue"))
                            SpecValue.Text = specParam["SpecValue"].ToString();
                    }

                    SpecValue.Enabled = !isBypass;
                }
                if (specParam.ContainsKey("CSV"))
                {
                    if (specParam["CSV"].ToString() == "OFF")
                        checkBox_CSV.Checked = false;
                }
                else
                    checkBox_CSV.Checked = true;

                if (specParam.ContainsKey("MES"))
                {
                    if (specParam["MES"].ToString() == "OFF")
                        checkBox_MES.Checked = false;
                }
                else
                    checkBox_MES.Checked = true;

                int len = richTextBox_Spec.Text.IndexOf($"\"{selectedName.Trim()}\"");
                if (len > 0)
                {
                    richTextBox_Spec.Select(len + 1, selectedName.Length);
                    richTextBox_Spec.ScrollToCaret();
                    richTextBox_Spec.SelectionBackColor = Color.LightGray;
                }
            }
            else
            {
                //txtMinLimit.Text = "";
                //txtMaxLimit.Text = "";
                //SpecValue.Text = "";
                //comboBox_SpecType.SelectedIndex = 0;
            }
        }
        public string GetSpecText()
        {
            return richTextBox_Spec.Text;
        }

        public void SetSpecText(string value)
        {
            richTextBox_Spec.Text = value;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
