using AutoTestSystem.BLL;
using AutoTestSystem.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace AutoTestSystem
{
    public partial class UnlockForm : Form
    {
        public string SelectedReason { get; private set; }

        public UnlockForm()
        {
            InitializeComponent();
            //InitializeReasonRadioButtons();
            LoadReasonsFromFile();
            AddOtherReasonOption();
        }

        private void InitializeReasonRadioButtons()
        {
            string[] reasons = { "頂針更換 (Pin Replacement)", "線材更換 (Wire Replacement)", "電氣環境問題 (Electrical Environment Issue)" };
            foreach (var reason in reasons)
            {
                AddReasonRadioButton(reason);
            }
        }

        private void AddReasonRadioButton(string reason)
        {
            RadioButton rb = new RadioButton
            {
                Text = reason,
                AutoSize = true,
                BackColor = GetLightColor(),
                Margin = new Padding(3, 3, 3, 10)
            };
            flowLayoutPanelReasons.Controls.Add(rb);
            MoveOtherOptionToEnd();
        }

        private void AddOtherReasonOption()
        {
            if (!flowLayoutPanelReasons.Controls.Contains(radioButtonOther))
            {
                flowLayoutPanelReasons.Controls.Add(radioButtonOther);
            }
            if (!flowLayoutPanelReasons.Controls.Contains(textBoxOtherReason))
            {
                flowLayoutPanelReasons.Controls.Add(textBoxOtherReason);
            }
            MoveOtherOptionToEnd();
        }

        private void MoveOtherOptionToEnd()
        {
            if (flowLayoutPanelReasons.Controls.Contains(radioButtonOther))
            {
                flowLayoutPanelReasons.Controls.SetChildIndex(radioButtonOther, flowLayoutPanelReasons.Controls.Count - 1);
            }
            if (flowLayoutPanelReasons.Controls.Contains(textBoxOtherReason))
            {
                flowLayoutPanelReasons.Controls.SetChildIndex(textBoxOtherReason, flowLayoutPanelReasons.Controls.Count - 1);
            }
        }

        private Color GetLightColor()
        {
            Random rand = new Random();
            int r = rand.Next(200, 256); // 淺色範圍
            int g = rand.Next(200, 256);
            int b = rand.Next(200, 256);
            return Color.FromArgb(r, g, b);
        }

        private void buttonAddReason_Click(object sender, EventArgs e)
        {
            string newReason = textBoxNewReason.Text.Trim();
            if (!string.IsNullOrEmpty(newReason))
            {
                AddReasonRadioButton(newReason);
                textBoxNewReason.Clear();
                SaveReasonsToFile();
            }
        }

        private void radioButtonOther_CheckedChanged(object sender, EventArgs e)
        {
            textBoxOtherReason.Visible = radioButtonOther.Checked;
        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            if (radioButtonOther.Checked)
            {
                SelectedReason = textBoxOtherReason.Text;
            }
            else
            {
                SelectedReason = flowLayoutPanelReasons.Controls.OfType<RadioButton>()
                                    .FirstOrDefault(r => r.Checked)?.Text;
            }

            InsertLog("pe", SelectedReason);
            INIHelper.Writeini("CountNum", "ABORT_FLAG", "0", Global.IniConfigFile);
            GlobalNew.GlobalFailCount = 0;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        static void InsertLog(string name, string method)
        {
            string date = DateTime.Now.ToString("yyyyMMdd");
            string folderPath = $@"{GlobalNew.LOGFOLDER}\Maintenance";
            string dbName = $@"{folderPath}\{date}.db";
            string connectionString = $"Data Source={dbName};Version=3;";

            try
            {
                // Check if the folder exists, if not, create it
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Check if the database file exists, if not, create it
                if (!File.Exists(dbName))
                {
                    SQLiteConnection.CreateFile(dbName);
                }

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Logs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Time TEXT,
                        Name TEXT,
                        Method TEXT
                    )";
                    SQLiteCommand createTableCmd = new SQLiteCommand(createTableQuery, connection);
                    createTableCmd.ExecuteNonQuery();

                    string insertDataQuery = "INSERT INTO Logs (Time, Name, Method) VALUES (@Time, @Name, @Method)";
                    SQLiteCommand insertDataCmd = new SQLiteCommand(insertDataQuery, connection);
                    insertDataCmd.Parameters.AddWithValue("@Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    insertDataCmd.Parameters.AddWithValue("@Name", name);
                    insertDataCmd.Parameters.AddWithValue("@Method", method);
                    insertDataCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void SaveReasonsToFile()
        {
            string filePath = "common_reasons.txt";
            List<string> reasons = flowLayoutPanelReasons.Controls.OfType<RadioButton>()
                                        .Select(rb => rb.Text)
                                        .ToList();
            File.WriteAllLines(filePath, reasons);
        }

        private void LoadReasonsFromFile()
        {
            string filePath = "common_reasons.txt";
            if (!File.Exists(filePath))
            {
                // 如果文件不存在，創建文件並寫入預設原因
                string[] defaultReasons = { "頂針更換 (Pin Replacement)", "線材更換 (Wire Replacement)", "電氣環境問題 (Electrical Environment Issue)" };
                File.WriteAllLines(filePath, defaultReasons);
            }

            string[] reasons = File.ReadAllLines(filePath);
            foreach (var reason in reasons)
            {
                AddReasonRadioButton(reason);
            }
        }

    }
}
