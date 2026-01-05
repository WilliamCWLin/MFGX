using AutoTestSystem.DAL;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.Base.MotionBase;

namespace AutoTestSystem
{
    public partial class LeadShineMotionForm : Form
    {
        public LeadShine motiondevice = null;
        public double Pos = 0;        
        //Modbus 
        //private Timer timer;
        public string JSON;
        public bool CheckEmptyList = false;
        public LeadShineMotionForm(LeadShine dev)
        {
            InitializeComponent();
            //InitializeTimer();
            InitMotion(dev);
            this.FormClosing += new FormClosingEventHandler(MotionControlerDialog_FormClosing);
        }

        private bool InitMotion(LeadShine Dev)
        {
            try
            {
                if(Dev == null)
                {
                    foreach (var value in GlobalNew.Devices.Values)
                    {
                        if(value is LeadShine dev)
                        {
                            Dev = dev;
                            break;
                        }             
                    }
                }
                motiondevice = Dev;
                bool init_ret = motiondevice.Init("");          
                LeadShinePropertyGrid.SelectedObject = motiondevice;
;                if (init_ret)
                {
                    MessageBox.Show("InitMotion Successed", "InitMotion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    motiondevice = null;
                    MessageBox.Show("InitMotion Fail", "InitMotion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch(Exception Ex)
            {
                MessageBox.Show($"Init Fail:{Ex.Message}", "Init Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }


        public string SelectedDeviceID()
        {
            string Dev_ID = string.Empty;
            try
            {
                object selectedObject = LeadShinePropertyGrid.SelectedObject;
                if (selectedObject != null)
                {
                    Type type = selectedObject.GetType();
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        object value = property.GetValue(selectedObject);
                        if (property.Name == "SlaveID")
                        {
                            Dev_ID = $"{value}";
                            break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No object selected.","object error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }catch(Exception ex)
            {
                MessageBox.Show($"SelectedDeviceID Error:{ex.Message}", "object error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return Dev_ID;
        }

        public bool PushMotionListToParaList()
        {
            try
            {
                if (string.IsNullOrEmpty(JSON))
                {
                    MessageBox.Show("MotionList Exist null or Empty", "MotionList error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else
                {
                    object selectedObject = LeadShinePropertyGrid.SelectedObject;
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

        public void PullMotionList_ParamToDataGridViewFrom()
        {
            try
            {
                bool PullResult = false;                         
                object selectedObject = LeadShinePropertyGrid.SelectedObject;
                if (selectedObject != null)
                {
                    Type type = selectedObject.GetType();
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        object value = property.GetValue(selectedObject);
                        if (property.Name == "MotionControl_Param" && !string.IsNullOrEmpty($"{value}"))
                        {
                            PullResult = true;
                            PopulateDataGridViewFromJson(MotionList, $"{value}");
                            JSON = $"{value}";
                            break;
                        }
                    }

                    if(!PullResult)
                    {
                        MessageBox.Show("MotionList is Empty", "MotionList info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("No object selected.", "object error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PullMotionListToParaList Error:{ex.Message}", "object error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void LockUI(object sender, bool EN_Status)
        {

            Button clickedButton = sender as Button;

            // Disable other controls
            foreach (Control control in Controls)
            {
                if (control != clickedButton)
                {
                    control.Enabled = EN_Status;
                }
            }

        }
            private void Trigger_Click(object sender, EventArgs e)
            {
            try
            {               
                if (motiondevice != null)
                {
                    if (CheckTriggerParam())
                    {
                        LockUI(sender, false);
                        bool MoveRet = false;
                        double MovePostion = double.Parse(postion.Text);
                        double MaxVelocity = double.Parse(max_vel.Text);
                        double MoveTacc = double.Parse(tacc.Text);
                        double MoveDac = double.Parse(dac.Text);

                        DialogResult = MessageBox.Show($"Are you Sure to Trigger Motion", "Trigger Motion", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                        if (DialogResult == DialogResult.OK)
                        {
                            MoveRet = motiondevice.Absolute_Move(MovePostion, 0, MaxVelocity, MoveTacc, MoveDac);
                            MoveRet &= CheckMoveDone(motiondevice);
                            if (MoveRet == false)
                            {
                                MessageBox.Show("Absolute Move Fail", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        LockUI(sender, true);
                    }

                }
                else
                {
                    MessageBox.Show("Motiondevice is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }catch(Exception ex)
            {
                MessageBox.Show($"Motiondevice Error:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LockUI(sender, true);
            }
        }

        private void ResetHome_Click(object sender, EventArgs e)
        {
            try
            {
                if (motiondevice != null)
                {
                    if (CheckHomeParam())
                    {
                        LockUI(sender, false);
                        bool MoveRet = false;
                        string RecDataAll = string.Empty;

                        // 獲取 ComboBox 中選擇的枚舉值
                        ResetDirection Dir = (ResetDirection)ResetDir.SelectedItem;
                        SpecifyLocation Specify_location = (SpecifyLocation)SpecifyLocation.SelectedItem;
                        ResetMode Mode = (ResetMode)ResetMode.SelectedItem;

                        double Home_Position = double.Parse(ZeroPoint.Text);
                        double Stop_Position = double.Parse(StopPosition.Text);
                        int Reset_vel_HS = int.Parse(HighSpeed.Text);
                        int Reset_vel_LS = int.Parse(LowSpeed.Text);
                        int Tacc = int.Parse(ResetTacc.Text);
                        int Dac = int.Parse(ResetDac.Text);

                        DialogResult = MessageBox.Show($"Are you Sure to ResetHome", "ResetHome", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                        if (DialogResult == DialogResult.OK)
                        {
                            MoveRet = motiondevice.MotionParamSet(Dir, Specify_location, Mode, Home_Position, Stop_Position, Reset_vel_HS, Reset_vel_LS, Tacc, Dac);
                            MoveRet &= motiondevice.Trigger(TriggerMode.Register_Trigger, "", "0x020", ref RecDataAll);
                            if (MoveRet == false)
                            {
                                motiondevice.EmgStop();
                                MessageBox.Show("ResetHome Fail", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                bool HomeDone = true;
                                bool home_status = false;
                                bool home_end = false;
                                Stopwatch stopwatch = new Stopwatch();
                                int status = 0;

                                stopwatch.Start();
                                do
                                {
                                    
                                    home_status = motiondevice.MotionStatus(6, ref status);
                                    GetMotionPosition(motiondevice);
                                    if (home_status == false)
                                    {
                                        motiondevice.EmgStop();
                                        HomeDone = false;
                                        MessageBox.Show($"Reset Home Fail, Motion Status Fail", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        break;
                                    }
                                    if (stopwatch.ElapsedMilliseconds > 30000)
                                    {
                                        motiondevice.EmgStop();
                                        HomeDone = false;                                
                                        MessageBox.Show($"Reset Home TimeOut, Stop Motor", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        break;
                                    }

                                } while (status != 1);

                                if (status == 1 && HomeDone == true)
                                {
                                    GetMotionPosition(motiondevice);
                                    home_end = motiondevice.Trigger(TriggerMode.Register_Trigger, "", "0x021", ref RecDataAll);//Set Zero Point
                                    if (home_end == false)
                                    {
                                        MessageBox.Show($"Reset Home Fail, Set Zero Point Fail", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                    else
                                    {
                                        MessageBox.Show($"Reset Home {Pos} Done Success", "Move Reset Home", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                }
                            }
                        }
                        LockUI(sender, true);
                    }

                }
                else
                {
                    MessageBox.Show("Motiondevice is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Motiondevice Error:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LockUI(sender, true);
            }
        }
        public void GetMotionPosition(LeadShine CheckMove)
        {
            try
            {
                CheckMove.GetCommandPos(ref Pos);
                CurrentPosition.Text = Pos.ToString();
            }catch(Exception ex)
            {
                MessageBox.Show($"GetCommandPos Error:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public bool CheckMoveDone(LeadShine CheckMove)
        {
            bool Motion_Status = false;
            bool MoveDone = false;
            int cmd_status = 0;
            int path_status = 0;
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            //狀態判斷
            do
            {
                Motion_Status = CheckMove.MotionStatus(4, ref cmd_status);
                Motion_Status &= CheckMove.MotionStatus(5, ref path_status);
                if (Motion_Status == false)
                    return false;
                GetMotionPosition(CheckMove);
                if (cmd_status == 1 && path_status == 1)
                {
                    MoveDone = true;
                    break;
                }
                if (stopwatch.ElapsedMilliseconds > 15000)
                {
                    CheckMove.EmgStop();
                    MessageBox.Show($"Move TimeOut", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //return false;
                    break;
                }
            } while (MoveDone == false);

            if (MoveDone == false)
            {
                MessageBox.Show($"Move Done Fail", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else
            {
                MessageBox.Show($"Move {Pos} Done Success", "Move Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return true;
        }
          
            private void Position_Click(object sender, EventArgs e)
            {
            bool Trigger_ret = false;
            string RecDataAll = string.Empty;
            try
            {
                if (motiondevice != null)
                {
                    if (CheckJOGParam())
                    {
                        LockUI(sender, false);
                        int INTPointVelocity = int.Parse(PointVelocity.Text);
                        int INTTimeTacc = int.Parse(TimeTacc.Text);
                        int INTIntervalTime = int.Parse(IntervalTime.Text);
                        int INTCyclesTimes = int.Parse(CyclesTimes.Text);
                        string IntervalTime_hexValue = INTIntervalTime.ToString("X4");
                        string CyclesTimes_hexValue = INTCyclesTimes.ToString("X4");
                        string PointVelocity_hexValue = INTPointVelocity.ToString("X4"); // 轉換為十六進制表示 //移動Point速度
                        string TimeTacc_hexValue = INTTimeTacc.ToString("X4"); // 轉換為十六進制表示 //移動Tacc速度

                        Trigger_ret = motiondevice.Trigger(TriggerMode.RS485_Trigger, "0x01E1", $"0x{PointVelocity_hexValue}", ref RecDataAll);
                        Trigger_ret &= motiondevice.Trigger(TriggerMode.RS485_Trigger, "0x01E3", $"0x{IntervalTime_hexValue}", ref RecDataAll);
                        Trigger_ret &= motiondevice.Trigger(TriggerMode.RS485_Trigger, "0x01E5", $"0x{CyclesTimes_hexValue}", ref RecDataAll);
                        Trigger_ret &= motiondevice.Trigger(TriggerMode.RS485_Trigger, "0x01E7", $"0x{TimeTacc_hexValue}", ref RecDataAll);
                        Trigger_ret &= motiondevice.Trigger(TriggerMode.RS485_Trigger, "0x1801", "0x4001", ref RecDataAll);
                        Trigger_ret &= CheckMoveDone(motiondevice);
                        if (Trigger_ret == false)
                        {
                            MessageBox.Show("JOG Position Fail", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        LockUI(sender, true);
                    }
                }
                else
                {
                    MessageBox.Show("Motiondevice is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }catch(Exception ex)
            {
                MessageBox.Show($"Motion Trigger Error:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LockUI(sender, true);
            }
        }

        private void Negative_Click(object sender, EventArgs e)
        {
            bool Trigger_ret = false;
            string RecDataAll = string.Empty;
            try
            {
                if (motiondevice != null)
                {
                    if (CheckJOGParam())
                    {
                        LockUI(sender, false);
                        int INTPointVelocity = int.Parse(PointVelocity.Text);
                        int INTTimeTacc = int.Parse(TimeTacc.Text);
                        int INTIntervalTime = int.Parse(IntervalTime.Text);
                        int INTCyclesTimes = int.Parse(CyclesTimes.Text);
                        string IntervalTime_hexValue = INTIntervalTime.ToString("X4");
                        string CyclesTimes_hexValue = INTCyclesTimes.ToString("X4");
                        string PointVelocity_hexValue = INTPointVelocity.ToString("X4"); // 轉換為十六進制表示 //移動Point速度
                        string TimeTacc_hexValue = INTTimeTacc.ToString("X4"); // 轉換為十六進制表示 //移動Tacc速度

                        Trigger_ret = motiondevice.Trigger(TriggerMode.RS485_Trigger, "0x01E1", $"0x{PointVelocity_hexValue}", ref RecDataAll);
                        Trigger_ret &= motiondevice.Trigger(TriggerMode.RS485_Trigger, "0x01E3", $"0x{IntervalTime_hexValue}", ref RecDataAll);
                        Trigger_ret &= motiondevice.Trigger(TriggerMode.RS485_Trigger, "0x01E5", $"0x{CyclesTimes_hexValue}", ref RecDataAll);
                        Trigger_ret &= motiondevice.Trigger(TriggerMode.RS485_Trigger, "0x01E7", $"0x{TimeTacc_hexValue}", ref RecDataAll);
                        Trigger_ret &= motiondevice.Trigger(TriggerMode.RS485_Trigger, "0x1801", "0x4002", ref RecDataAll);
                        Trigger_ret &= CheckMoveDone(motiondevice);
                        if (Trigger_ret == false)
                        {
                            MessageBox.Show("JOG Negative Fail", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        LockUI(sender, true);
                    }
                }
                else
                {
                    MessageBox.Show("Motiondevice is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Motion Trigger Error:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LockUI(sender, true);
            }
        }

        private void Init_Click(object sender, EventArgs e)
        {
            if (motiondevice == null)
            {
                LockUI(sender, false);
                InitMotion(motiondevice);
                LockUI(sender, true);
            }
        }

        private void UnInit_Click(object sender, EventArgs e)
        {
            bool UnInit_Ret = false;
            if (motiondevice != null)
            {
                LockUI(sender, false);
                UnInit_Ret = motiondevice.UnInit();
                if (UnInit_Ret)
                {
                    //timer.Stop();
                    MessageBox.Show("UnInitMotion Successed", "UnInitMotion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    motiondevice = null;
                }
                else
                {
                    //timer.Stop();
                    MessageBox.Show("UnInitMotion Fail", "InitMotion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                LockUI(sender, true);
            }
            else
            {
                //timer.Stop();
                MessageBox.Show("IS Already UnInit Motion", "UnInitMotion", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool CheckTriggerParam()
        {
            if(string.IsNullOrEmpty(postion.Text) || string.IsNullOrEmpty(max_vel.Text) || string.IsNullOrEmpty(tacc.Text) || string.IsNullOrEmpty(dac.Text))
            {
                MessageBox.Show("Check Param exist null or Empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }                     
            return true;
        }
        private bool CheckHomeParam()
        {
            if (string.IsNullOrEmpty(ZeroPoint.Text) || string.IsNullOrEmpty(StopPosition.Text) || string.IsNullOrEmpty(HighSpeed.Text) || string.IsNullOrEmpty(LowSpeed.Text) || string.IsNullOrEmpty(ResetTacc.Text) || string.IsNullOrEmpty(ResetDac.Text))
            {
                MessageBox.Show("Check Param exist null or Empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private bool CheckJOGParam()
        {
            if (string.IsNullOrEmpty(PointVelocity.Text) || string.IsNullOrEmpty(TimeTacc.Text) || string.IsNullOrEmpty(IntervalTime.Text) || string.IsNullOrEmpty(CyclesTimes.Text))
            {
                MessageBox.Show("CheckJOGParam exist null or Empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else
            {
                if (int.Parse(PointVelocity.Text) < 0 || int.Parse(PointVelocity.Text) > 5000)
                {
                    MessageBox.Show("PointVelocity over Range (0~5000)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (int.Parse(TimeTacc.Text) < 0 || int.Parse(TimeTacc.Text) > 10000)
                {
                    MessageBox.Show("TimeTacc over Range (0~10000)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (int.Parse(IntervalTime.Text) < 10 || int.Parse(IntervalTime.Text) > 10000)
                {
                    MessageBox.Show("IntervalTime over Range (10~10000)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (int.Parse(CyclesTimes.Text) < 0 || int.Parse(CyclesTimes.Text) > 30000)
                {
                    MessageBox.Show("CyclesTimes over Range (0~30000)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }

        private void MotionControlerDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 檢查關閉原因是否是使用者點擊叉叉
            if (e.CloseReason != CloseReason.UserClosing)
            {
                // 取消關閉操作
                e.Cancel = true;
            }else
            {
                motiondevice = null;
            }
        }


        public bool ConvertDataGridViewToJson(DataGridView dataGridView,ref string output_str)
        {
            try
            { 
                var rowsList = new List<Dictionary<string, object>>();
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                        if (!row.IsNewRow)
                        {
                            var rowDict = new Dictionary<string, object>();
                            foreach (DataGridViewCell cell in row.Cells)
                            {

                            if (cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString()))
                            {
                                MessageBox.Show("DataGridView List Exist Empty or Null","Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return false;
                            }
                            rowDict[dataGridView.Columns[cell.ColumnIndex].Name] = cell.Value;
                            }
                            rowsList.Add(rowDict);
                        }
                }
                output_str = JsonConvert.SerializeObject(rowsList, Formatting.Indented);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ConvertDataGridViewToJson Fail {ex.Message}", "SaveData", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }    
        }

        public static void PopulateDataGridViewFromJson(DataGridView dataGridView,string json)
        {
            var rowsList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);

            dataGridView.Rows.Clear();
            foreach (var rowDict in rowsList)
            {
                var row = new DataGridViewRow();
                row.CreateCells(dataGridView);

                foreach (var key in rowDict.Keys)
                {
                    int columnIndex = dataGridView.Columns[key].Index;
                    row.Cells[columnIndex].Value = rowDict[key];
                }

                dataGridView.Rows.Add(row);
            }
        }
        public string JsonData()
        {
            return JSON;
        }
        /*public string JsonData()
        {
            // Create a dictionary to store the GroupBox and TextBox data
            Dictionary<string, Dictionary<string, string>> groupBoxData = new Dictionary<string, Dictionary<string, string>>();
            string jsonData = string.Empty;
            try
            {
                // Iterate through all GroupBoxes in the Form
                foreach (Control groupBox in this.Controls.OfType<GroupBox>())
                {
                    // Create a dictionary to store TextBox data for the current GroupBox
                    Dictionary<string, string> textBoxData = new Dictionary<string, string>();

                    // Iterate through all TextBoxes in the GroupBox
                    foreach (Control control in groupBox.Controls)
                    {
                        if (control is TextBox textBox)
                        {
                            textBoxData[textBox.Name] = textBox.Text;
                        }
                    }

                    // Add the GroupBox and its TextBox data to the main dictionary
                    groupBoxData[groupBox.Name] = textBoxData;
                }
                // Convert dictionary to JSON
                jsonData = JsonConvert.SerializeObject(groupBoxData, Formatting.Indented);

                string DataFilePath = $"MotionData\\";
                if (!Directory.Exists(DataFilePath))
                {
                    Console.WriteLine("文件夹不存在，将创建新文件夹。");
                    Directory.CreateDirectory(DataFilePath);
                }
                // Save the JSON string to a text file
                File.WriteAllText($"{DataFilePath}SetParam_{ DateTime.Now.ToString("yyyy_MM_dd")}.txt", jsonData);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Param JsonData Formate Error:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
            return jsonData;
        }*/

        public string JsonResetData()
        {
            // Create a dictionary to store the GroupBox and TextBox data
            Dictionary<string, Dictionary<string, string>> groupBoxData = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> BoxData = new Dictionary<string, string>();
            string jsonData = string.Empty;
            try
            {
                // Iterate through all GroupBoxes in the Form
                foreach (Control groupBox in this.Controls.OfType<GroupBox>())
                {
                    // Create a dictionary to store TextBox data for the current GroupBox
                    if (groupBox.Name == "ResetControl")
                    {
                        // Iterate through all TextBoxes in the GroupBox
                        foreach (Control control in groupBox.Controls)
                        {
                            if(control is  ComboBox comboBox)
                            {
                                BoxData[comboBox.Name] = comboBox.Text;
                            }
                            if (control is TextBox textBox)
                            {
                                BoxData[textBox.Name] = textBox.Text;
                            }
                        }
                        // Add the GroupBox and its TextBox data to the main dictionary
                        //groupBoxData[groupBox.Name] = textBoxData;
                    }
                }
                // Convert dictionary to JSON
                jsonData = JsonConvert.SerializeObject(BoxData, Formatting.Indented);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Param JsonData Formate Error:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
            return jsonData;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool SaveData = false;

            JsonResetData();
            SaveData = ConvertDataGridViewToJson(MotionList,ref JSON);
            SaveData &= PushMotionListToParaList();
            if(SaveData)
            {
                MessageBox.Show("SaveData Success", "SaveData", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("SaveData Fail", "SaveData", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /*private void MotionList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

           

        }*/

        private void AddList_Click(object sender, EventArgs e)
        {
            if (motiondevice != null)
            {
                string Dev_ID = string.Empty;
                LeadShinePropertyGrid.SelectedObject = motiondevice;
                Dev_ID = SelectedDeviceID();

                if (!string.IsNullOrEmpty(Dev_ID))
                {
                    LockUI(sender, false);
                    // 彈出對話框讓使用者輸入路徑名稱
                    string path = Prompt.ShowDialog("請輸入路徑名稱", "輸入路徑");
                    if (!string.IsNullOrEmpty(path))
                    {
                        int rowIndex = MotionList.Rows.Add();
                        MotionList.Rows[rowIndex].Cells[0].Value = path;
                        MotionList.Rows[rowIndex].Cells[1].Value = Dev_ID;
                        MotionList.Rows[rowIndex].Cells[2].Value = postion.Text;
                        MotionList.Rows[rowIndex].Cells[3].Value = max_vel.Text;
                        MotionList.Rows[rowIndex].Cells[4].Value = tacc.Text;
                        MotionList.Rows[rowIndex].Cells[5].Value = dac.Text;

                        postion.Clear();
                        max_vel.Clear();
                        tacc.Clear();
                        dac.Clear();
                    }
                    LockUI(sender, true);
                }
                else
                {
                    MessageBox.Show("Dev_ID is null or Empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            else
            {
                MessageBox.Show("Motiondevice is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }                    
                   
        }

        // 顯示對話框的輔助方法
        public static class Prompt
        {
            public static string ShowDialog(string text, string caption)
            {
                Form prompt = new Form()
                {
                    Width = 300,
                    Height = 180,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterScreen
                };
                Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
                TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 100 };
                Button confirmation = new Button() { Text = "確定", Left = 150, Width = 100, Top = 50, DialogResult = DialogResult.OK };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }

        private void MotionList_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // 確保雙擊的是整行
            if (e.RowIndex >= 0)
            {
                
                DataGridViewRow row = MotionList.Rows[e.RowIndex];
                string path = row.Cells[0].Value.ToString();
                string slaveid = row.Cells[1].Value.ToString();
                string position = row.Cells[2].Value.ToString();
                string velocity = row.Cells[3].Value.ToString();
                string acceleration = row.Cells[4].Value.ToString();
                string deceleration = row.Cells[5].Value.ToString();

                DialogResult = MessageBox.Show($"Run :路徑:{path} SlaveID:{slaveid} 位置: {position} 速度: {velocity} 加速度: {acceleration} 減速度: {deceleration}", "Path Check", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                if (DialogResult == DialogResult.OK)
                {
                    if (motiondevice != null)
                    {
                        if (!string.IsNullOrEmpty(position) && !string.IsNullOrEmpty(slaveid) && !string.IsNullOrEmpty(velocity) && !string.IsNullOrEmpty(acceleration) && !string.IsNullOrEmpty(deceleration))
                        {
                            LockUI(sender, false);
                            bool MoveRet = false;
                            double MovePostion = double.Parse(position);
                            double MaxVelocity = double.Parse(velocity);
                            double MoveTacc = double.Parse(acceleration);
                            double MoveDac = double.Parse(deceleration);

                            motiondevice.ChangeSlaveID(slaveid);
                            MoveRet = motiondevice.Absolute_Move(MovePostion, 0, MaxVelocity, MoveTacc, MoveDac);
                            if (MoveRet == false)
                            {
                                MessageBox.Show("Absolute Move Fail", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            LockUI(sender, true);
                        }
                        else
                        {
                            MessageBox.Show("MotionList Param exist Empty or null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Motiondevice is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }              
            }
        }

        private void LeadShineMotionForm_Load(object sender, EventArgs e)
        {
            // 將枚舉值轉換為列表並綁定到 ComboBox
            ResetDir.DataSource = Enum.GetValues(typeof(ResetDirection));
            // 將枚舉值轉換為列表並綁定到 ComboBox
            ResetMode.DataSource = Enum.GetValues(typeof(ResetMode));
            // 將枚舉值轉換為列表並綁定到 ComboBox
            SpecifyLocation.DataSource = Enum.GetValues(typeof(SpecifyLocation));

            PullMotionList_ParamToDataGridViewFrom();
        }

        private void UpdatePosition_Click(object sender, EventArgs e)
        {
            if(motiondevice != null)
                GetMotionPosition(motiondevice);
            else
                MessageBox.Show("Motiondevice is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void PlusRotateSet_Click(object sender, EventArgs e)
        {
            bool PlusSet_ret = false;
            string RecDataAll = string.Empty;
            if (motiondevice != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(PlusValue.Text))
                    {
                        int INTPlus = int.Parse(PlusValue.Text);
                        string Plus_hexValue = INTPlus.ToString("X4");
                        PlusSet_ret = motiondevice.Trigger(TriggerMode.RS485_Trigger, "0x0001", $"0x{Plus_hexValue}", ref RecDataAll);
                        if (PlusSet_ret == false)
                        {
                            MessageBox.Show($"PlusRotateSet Fail", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                            MessageBox.Show($"PlusRotateSet Successed", "PlusRotateSet", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"PlusValue is Empty or null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                } catch (Exception ex)
                {
                    MessageBox.Show($"PlusRotateSet Error:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                MessageBox.Show("Motiondevice is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
