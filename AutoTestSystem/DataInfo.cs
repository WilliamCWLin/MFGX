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
    public partial class DataInfo : Form
    {
        private Timer timer;
        private Dictionary<string, string> VariableDict;
        private Dictionary<string, string> MesDict;
        private Dictionary<string, string> SpecDict;
        private Dictionary<string, string> lastDictionary = new Dictionary<string, string>();
        private bool isFirstRefresh = true;

        public DataInfo(Dictionary<string, string> dict, Dictionary<string, string> Mdict, Dictionary<string, string> Specdict)
        {
            InitializeComponent();
            VariableDict = dict;
            MesDict = Mdict;
            SpecDict = Specdict;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            this.Size = new Size(this.Width, Screen.PrimaryScreen.WorkingArea.Height);
        }

        public void StartTimer(int interval)
        {
            // 初始化 Timer
            timer = new Timer();
            timer.Interval = interval; 
            timer.Tick += Timer_Tick; 

            timer.Start();
        }
        public void UpdateDictData(Dictionary<string, string> dict, Dictionary<string, string> Mdict, Dictionary<string, string> Specdict)
        {
            VariableDict = dict;
            MesDict = Mdict;
            SpecDict = Specdict;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // 更新 DataGridView
            UpdateDataGridView(VariableDict, SpecDict);
            UpdateMesDataGridView(MesDict);
        }


        // Define a field to store the indexes of rows that need to be updated
        private List<int> updatedRows = new List<int>();
        private List<int> MesRows = new List<int>();
        private void UpdateDataGridView(Dictionary<string, string> dict,Dictionary<string, string> Spec)
        {
            Dictionary<string, string> currentDictionary = dict; // Get the current dictionary data
            try
            {
                // Iterate through DataGridView rows
                foreach (DataGridViewRow row in dataGridView_Data.Rows)
                {
                    // Check if cell values are NULL
                    if (row.Cells["DataKey"].Value == null || row.Cells["DataValue"].Value == null)
                        continue; // Skip this iteration if they are NULL

                    string key = row.Cells["DataKey"].Value.ToString();

                    // Check if the key exists in the current dictionary, if not, remove the row
                    if (!currentDictionary.ContainsKey(key))
                    {
                        dataGridView_Data.Rows.Remove(row);
                        continue; // Skip the remaining part of this iteration and move to the next one
                    }

                    // Check if the value in the DataGridView differs from the value in the current dictionary, if so, update the value in the DataGridView
                    if (currentDictionary[key] != row.Cells["DataValue"].Value.ToString())
                    {
                        row.Cells["DataValue"].Value = currentDictionary[key];
                        if (Spec.ContainsKey(key))
                            row.Cells["Spec"].Value = Spec[key];
                        // Set the new background color
                        row.DefaultCellStyle.BackColor = Color.LightYellow;



                        // Add the index of the row to the list of updated rows
                        updatedRows.Add(row.Index);
                    }
                }

                // Iterate through each key-value pair in the current dictionary
                foreach (var item in currentDictionary)
                {
                    string key = item.Key;

                    // If the key from the current dictionary does not exist in the DataGridView, add a new row
                    bool existsInDataGridView = false;
                    foreach (DataGridViewRow row in dataGridView_Data.Rows)
                    {
                        // Check if cell value is NULL
                        if (row.Cells["DataKey"].Value == null)
                            continue; // Skip this iteration if it's NULL

                        if (row.Cells["DataKey"].Value.ToString() == key)
                        {
                            existsInDataGridView = true;
                            break;
                        }
                    }

                    // If the key from the current dictionary does not exist in the DataGridView, add a new row
                    if (!existsInDataGridView)
                    {
                        int rowIndex = dataGridView_Data.Rows.Add();
                        dataGridView_Data.Rows[rowIndex].Cells["DataKey"].Value = key;
                        dataGridView_Data.Rows[rowIndex].Cells["DataValue"].Value = item.Value;
                        if (Spec.ContainsKey(item.Key))
                            dataGridView_Data.Rows[rowIndex].Cells["Spec"].Value = Spec[item.Key];
                        // Set the style for the new row
                        dataGridView_Data.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Turquoise; // Set initial background color for new row
                                                                                                       // Add the index of the new row to the list of updated rows
                        updatedRows.Add(rowIndex);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                // Handle the exception
                // Log the exception, display a message, etc.
                //MessageBox.Show(ex.Message);
            }
            // Create a timer to revert the background color changes
            Timer timer = new Timer();
            timer.Interval = 200; // Set the timer interval to 2 seconds
            timer.Tick += (sender, e) =>
            {
                // Revert the background color changes for updated rows
                foreach (int rowIndex in updatedRows)
                {
                    dataGridView_Data.Rows[rowIndex].DefaultCellStyle.BackColor = Color.White;
                }
                // Clear the list of updated rows
                updatedRows.Clear();
                // Stop the timer
                timer.Stop();
            };
            // Start the timer
            timer.Start();
        }
        private void UpdateMesDataGridView(Dictionary<string, string> dict)
        {
            Dictionary<string, string> currentDictionary = dict; // Get the current dictionary data
            try
            {
                // Iterate through DataGridView rows
                foreach (DataGridViewRow row in dataGridView_MES.Rows)
                {
                    // Check if cell values are NULL
                    if (row.Cells["Key"].Value == null || row.Cells["Value"].Value == null)
                        continue; // Skip this iteration if they are NULL

                    string key = row.Cells["Key"].Value.ToString();

                    // Check if the key exists in the current dictionary, if not, remove the row
                    if (!currentDictionary.ContainsKey(key))
                    {
                        dataGridView_MES.Rows.Remove(row);
                        continue; // Skip the remaining part of this iteration and move to the next one
                    }

                    // Check if the value in the DataGridView differs from the value in the current dictionary, if so, update the value in the DataGridView
                    if (currentDictionary[key] != row.Cells["Value"].Value.ToString())
                    {
                        row.Cells["Value"].Value = currentDictionary[key];
                        // Set the new background color
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        // Add the index of the row to the list of updated rows
                        MesRows.Add(row.Index);
                    }
                }

                // Iterate through each key-value pair in the current dictionary
                foreach (var item in currentDictionary)
                {
                    string key = item.Key;

                    // If the key from the current dictionary does not exist in the DataGridView, add a new row
                    bool existsInDataGridView = false;
                    foreach (DataGridViewRow row in dataGridView_MES.Rows)
                    {
                        // Check if cell value is NULL
                        if (row.Cells["Key"].Value == null)
                            continue; // Skip this iteration if it's NULL

                        if (row.Cells["Key"].Value.ToString() == key)
                        {
                            existsInDataGridView = true;
                            break;
                        }
                    }

                    // If the key from the current dictionary does not exist in the DataGridView, add a new row
                    if (!existsInDataGridView)
                    {
                        int rowIndex = dataGridView_MES.Rows.Add();
                        dataGridView_MES.Rows[rowIndex].Cells["Key"].Value = key;
                        dataGridView_MES.Rows[rowIndex].Cells["Value"].Value = item.Value;
                        // Set the style for the new row
                        dataGridView_MES.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Turquoise; // Set initial background color for new row
                                                                                                       // Add the index of the new row to the list of updated rows
                        MesRows.Add(rowIndex);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                // Handle the exception
                // Log the exception, display a message, etc.
                //MessageBox.Show(ex.Message);
            }
            // Create a timer to revert the background color changes
            Timer timer = new Timer();
            timer.Interval = 200; // Set the timer interval to 2 seconds
            timer.Tick += (sender, e) =>
            {
                // Revert the background color changes for updated rows
                foreach (int rowIndex in MesRows)
                {
                    dataGridView_MES.Rows[rowIndex].DefaultCellStyle.BackColor = Color.White;
                }
                // Clear the list of updated rows
                MesRows.Clear();
                // Stop the timer
                timer.Stop();
            };
            // Start the timer
            timer.Start();
        }

        private void DataInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
