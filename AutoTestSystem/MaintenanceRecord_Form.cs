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

namespace AutoTestSystem
{
    public partial class MaintenanceRecord_Form : Form
    {
        public MaintenanceRecord_Form()
        {
            InitializeComponent();
        }

        private void btnAddMaintenanceRecord_Click(object sender, EventArgs e)
        {
            if (rt_maintenance_record.Text.Length < 10)
            {
                MessageBox.Show("記錄訊息過少");
                
                return;
            }
            InsertLog("pe", rt_maintenance_record.Text);
            INIHelper.Writeini("CountNum", "ABORT_FLAG", "0", Global.IniConfigFile);
            GlobalNew.GlobalFailCount = 0;
            DialogResult = DialogResult.OK;
            this.Close();
            return;
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
    }
}
