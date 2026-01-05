using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_1Mot1Com_TorqueJig : Script_1Mot1ComBase
    {
        double MaxTorque = 0;      
        string Torque_mode = string.Empty;
        int TorqueMove_value = 0;        
        float TorqueMax_value = 0;
        float in_start_vel = 0;
        float in_max_vel = 0;
        float in_max_vel_limmit = 0;
        float tacc = 0;
        float dac = 0;
        bool IO_Status_Result;
        bool CHK_Cancel_Status = false;
        bool Retry_Home = false;
        int ret = -1;
        Dictionary<int, int> dictionary_FB = new Dictionary<int, int>();        


        string[] Send_comment = null;
        //string test_station = string.Empty;
        string Note_status = string.Empty;
        string Torquedata = string.Empty;
        string cmd = string.Empty;
        string cmdhead = "TEST Checking";

        string directoryPath = @"Record\\";
        string directoryImagePath = @"Image\\";

        public override void Dispose()
        {
            throw new NotImplementedException();
        }      

        public override bool PreProcess(string strParamInput)
        {
            try
            {
                Send_comment = strParamInput.Split(',');
                Torque_mode = Send_comment[1]; //"LensTorque" "GimTorque"
                cmd = Torque_mode + "_Test:Please press the space bar or Enter key to continue testing";                 
                TorqueMove_value = Int32.Parse(Send_comment[2]);              
                in_start_vel = float.Parse(Send_comment[3]);
                in_max_vel = float.Parse(Send_comment[4]);
                tacc = float.Parse(Send_comment[5]);
                dac = float.Parse(Send_comment[6]);
                TorqueMax_value = float.Parse(Send_comment[7]);
                in_max_vel_limmit = 1500;

                if (GlobalNew.ResetHome_status.ContainsKey(3) && GlobalNew.ResetHome_status.ContainsKey(4) && GlobalNew.ResetHome_status.ContainsKey(0) && GlobalNew.ResetHome_check.ContainsKey(0))
                {
                    GlobalNew.ResetHome_status[3] = 0;
                    GlobalNew.ResetHome_status[4] = 0;
                    GlobalNew.ResetHome_status[0] = 0;
                    GlobalNew.ResetHome_check[0] = 0;
                }
                else
                {
                    GlobalNew.ResetHome_status.Add(3, 0);
                    GlobalNew.ResetHome_status.Add(4, 0);
                    GlobalNew.ResetHome_status.Add(0, 0);
                    GlobalNew.ResetHome_check.Add(0, 0);
                }

                if (dictionary_FB.ContainsKey(3) && dictionary_FB.ContainsKey(4))
                {
                    dictionary_FB[3] = 0;
                    dictionary_FB[4] = 0;
                }
                else
                {
                    dictionary_FB.Add(3, 0);
                    dictionary_FB.Add(4, 0);
                }

                if (in_max_vel > in_max_vel_limmit || in_start_vel > in_max_vel_limmit)
                {
                    Logger.Warn("Maximum speed limit exceeded:" + "{" + in_max_vel_limmit + "}" + in_start_vel.ToString() + "/" + in_max_vel.ToString());
                    return false;
                }

                if (!Directory.Exists(directoryPath))
                {
                    Console.WriteLine("文件夹不存在，将创建新文件夹。");
                    Directory.CreateDirectory(directoryPath);                 
                }
                else
                {
                    Console.WriteLine($"文件夹 {directoryPath} 已存在。");
                }

                if (!Directory.Exists(directoryImagePath))
                {
                    Console.WriteLine("文件夹不存在，将创建新文件夹。");
                    Directory.CreateDirectory(directoryImagePath);
                }
                else
                {
                    Console.WriteLine($"文件夹 {directoryImagePath} 已存在。");
                }
                //MessageBox.Show(Station_Mode);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Info($"處理數據時出現錯誤: {ex.Message}");
                return false;
            }
        }

        public override bool Process(ControlDeviceBase ComportDev, MotionBase MotionDev)
        {
            bool Result;
            bool COM_REF;
            bool chk_LensTest_pos = false;
            double out_pos = 0;
            bool isRunning = true;
            string strActItem = string.Empty;
            string strParam = string.Empty;
            string cachedData = string.Empty;
            string receivedData = string.Empty;
            string max_value = string.Empty;
            List<double> ValueList = new List<double>(); // 定义一个动态列表
            Messagebox_show frm = new Messagebox_show();
            ImageShowFrom frm_image = new ImageShowFrom();
            //frm.KeyDown += new KeyEventHandler(Messagebox_show_KeyDown);
            string unit = string.Empty;
            string Data = string.Empty;            
            GlobalNew.Emg_flag = false;
            GlobalNew.image_path = directoryImagePath + Torque_mode + ".png";
          
            try
            {
                MotionDev.GetCommandPos(ref out_pos);                
                Logger.Info(Torque_mode+" OUTPUT Position:" + out_pos.ToString());
            }
            catch (Exception e1)
            {
                Logger.Error("Please check driver is install or not:" + e1.Message);
                return false;
            }

            if (Torque_mode == "LensTorque") {

                MessageBox.Show("Please put product on Jip platform  ", "TEST Checking", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Sleep(1000);
                //frm_image.SetImageLabelText("Please put \"Len Sleeve\" on Jip and \"press down\" handle,then \"cover\" Fix Block", "Lens Test Prepare", 0, GlobalNew.image_path);
                ret = (int)frm_image.ShowDialog();
                if (ret != 1)
                    return false;

                try
                {
                    MotionDev.GetCommandPos(ref out_pos);
                    Logger.Info("OUTPUT Position_After_Lens:" + out_pos.ToString());
                }
                catch (Exception e1)
                {
                    Logger.Error("Please check driver is install or not:" + e1.Message);
                    return false;
                }
                ////////////////////////
                if (!MotionDev.Check_IO_StartStatus(dictionary_FB))
                {
                    //Retry_Home = true;
                    //chk_LensTest_pos = HomeReset(MotionDev);
                    MotionDev.Relative_Move(100, 10, 100, 0.3, 0.3);
                    //chk_LensTest_pos = MotionDev.SyncHome(10, 80, 0);
                    chk_LensTest_pos &= MotionDev.Check_IO_StartStatus(dictionary_FB);

                    if (chk_LensTest_pos)
                    {
                        Logger.Info("<<<<<<==Reset IO Finish=>>>>>>>");
                        Retry_Home = false;
                    }
                    else
                    {
                        Logger.Warn("<<<<<<==Reset IO Fail=>>>>>>>");
                        return false;
                    }                   
                }
                /////////
            }
            else if(Torque_mode == "GimTorque")
            {
                //frm_image.SetImageLabelText("Please \"pull up\" handle and \"open\" Fix Block, then change \"Gimbal Sleeve\" on Jip", "Gimbal Test Prepare", 0, GlobalNew.image_path);
                ret = (int)frm_image.ShowDialog();
                if (ret != 1)
                    return false;
            }
            else
            {
                frm.SetLabelText("The Torque mode is not exist!!", "Check mode", 1);
                ret = (int)frm.ShowDialog();               
                return false;
            }

            /*if (Torque_mode == "LensTorque")
                MaxTorque = 2.1;
            else
                MaxTorque = 2.5;
            return true;*/
            // GIMBAL SHOW
            //Confirmation Lens JIPS is install Successed///
            /*add to Scrpt - Lens JIPS*/
            while(true)
                {               
                    IO_Status_Result = IO_Status_CHK_TM(MotionDev, ref CHK_Cancel_Status, Torque_mode);                
                    if (IO_Status_Result)
                    {
                        Logger.Info("Confirmation"+ Torque_mode + "JIPS install OK!!");

                    //避免不是按spec鍵或Enter鍵讓motion動作持續進行          
                    try
                    {
                        GlobalNew.image_path = string.Empty;
                        //// remind op press enter or Space to test////
                        frm.SetLabelText(cmd, cmdhead, 0);
                        ret = (int)frm.ShowDialog();   // GIMBAL SHOW
                                                       //// remind op press  enter or Space to test////
                        if (ret != 1)
                        {
                            Logger.Error("The Messagebox_show_KeyDown is not SPEC  or Enter retrun false ");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Messagebox_show_KeyDown ERROR!!." + ex.Message + " retrun false");
                        return false;
                    }
                    //避免不是按spec鍵讓motion動作持續進行
                    break;
                }
                else
                    {
                        if (CHK_Cancel_Status)
                        {
                            Logger.Warn("The Testing is already Canceled!!!!!");
                            return false;
                        }
                        else
                            MessageBox.Show("Please retry install"+ Torque_mode + "JIPS again!!!", "Retry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            
                Sleep(1000);
            ////Confirmation JIPS is install Successed///

                COM_REF = ComportDev.READ(ref receivedData);//清除第一筆資料



                Thread readThread = new Thread(() =>    // Read Torque Thread
                {

                    while (isRunning) // 使用共享变量作为循环条件
                    {
                        if (GlobalNew.Emg_flag == true)
                        {
                            isRunning = false;
                            Logger.Warn("Torque is stop Motor !!!!! Please Move JIG");
                            break;
                        }
                        // 读取串口数据
                        COM_REF = ComportDev.READ(ref receivedData);

                        if (receivedData == string.Empty)
                        {
                            isRunning = false;
                            MessageBox.Show("The Torque sensor console is Empty!!!!!", "Check Torque sensor console", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        }                                                   

                        receivedData = cachedData + receivedData;
                        string[] readings = receivedData.Split(new string[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
                           int lastIndex = readings.Length - 1;

                        if (!receivedData.EndsWith("\r") && receivedData != "")
                           {
                              cachedData = readings[lastIndex]; // 緩存記錄最後一條不完整的記錄
                              lastIndex--;
                            }
                            else
                            {
                              cachedData = "";
                            }
                            double torque_value;
                                for (int i = 0; i <= lastIndex; i++)
                                {
                                   string cleanedReading = readings[i].Trim();
                                   // 将数据写入 txt 文件                       

                                   torque_value = 0;
                                       torque_value = Math.Abs(ParseTorqueMeterData(cleanedReading.Replace("\u000241", ""), MotionDev, ref Data, ref unit));


                                       if (torque_value > TorqueMax_value)
                                       {
                                           MotionDev.EmgStop();
                                           isRunning = false;                               
                                           Logger.Warn("Torque in abnormal Max Value stop Motor !!!!! Please Move JIG");
                                           break;
                                       }

                                   ///// Peter add: if Torque > MAX value stop motor motion

                                   //Franklin modify
                                   ValueList.Add(torque_value);

                                       File.AppendAllText(directoryPath + Torque_mode+"_DATA.txt", Data);
                                   //Console.WriteLine(Math.Abs(ParseTorqueMeterData(cleanedReading.Replace("\u000241", ""), ref Data, ref unit)));
                                   Data = "";
                                   }

                               //具有快顯功能表                   
                      }
                 });


             readThread.Start(); // 启动线程 
           
            Result = MotionDev.Relative_Move(TorqueMove_value, in_start_vel, in_max_vel, tacc, dac);
            Sleep(500);

                if (isRunning == false)
                {
                    HomeReset(MotionDev);
                    readThread.Join();
                    return false;
                }
                else
                    Result = MotionDev.Relative_Move(TorqueMove_value * (-1), in_start_vel, in_max_vel, tacc, dac);
                Sleep(500);
                if (isRunning == false)
                {
                    HomeReset(MotionDev);
                    readThread.Join();
                    return false;
                }
                else
                    Result = MotionDev.Relative_Move(TorqueMove_value * (-1), in_start_vel, in_max_vel, tacc, dac);
                Sleep(500);
                if (isRunning == false)
                {
                    HomeReset(MotionDev);
                    readThread.Join();
                    return false;
                }
                else
                    Result = MotionDev.Relative_Move(TorqueMove_value, in_start_vel, in_max_vel, tacc, dac);

                //// 停止线程
                isRunning = false;
                readThread.Join(); // 等待线程结束

                if (ValueList.Count > 0)
                    MaxTorque = ValueList.Max();

            //PushMoreData(Torque_mode+"_Value_Set", MaxTorque.ToString());

            Logger.Info("Max"+Torque_mode+"_Value" + MaxTorque.ToString() + unit);

            Sleep(1000);
            ValueList.Clear();
            isRunning = true;

            return true;
        }

        public override bool PostProcess(string TestKeyword, string strCheckSpec, ref string strDataout)
        {      
            Torquedata = Torque_mode;
            var data = new Dictionary<string, object>
                    {
                        {Torquedata, MaxTorque},
                    };            
          
             string jsonStr;
            try
            {
                jsonStr = JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                Logger.Error($"轉換為 JSON 字串時出現錯誤: {ex.Message}");
                return false;
            }
            
            string ret = CheckRule(jsonStr, strCheckSpec);
            Logger.Info($"CheckRule: {ret}");


            strDataout = jsonStr;

            if (ret == "PASS")
                return true;
            else
                return false;
        }

        public static double ParseTorqueMeterData(string input,MotionBase PositionData ,ref string Data, ref string unit)
        {
            try
            {
                if (input.Length != 12)
                    return 0.0;
                Data += DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + "\t<=TorqueMeterData_Input=>" + input + "\t";
                string unitType = input.Substring(0, 2);
                sbyte polarity = sbyte.Parse(input.Substring(2, 1));
                byte decimalPlaces = byte.Parse(input.Substring(3, 1));
                int intValue;
                double posdata = 0;
                bool result = false;
                result = int.TryParse(input.Substring(8), out intValue);

                if (unitType == "81")
                {
                    unit = " kg/cm";
                }
                else if (unitType == "82")
                {
                    unit = " LB/inch";
                }
                else
                {
                    unit = " N/cm";
                }
                if (result)
                {
                    double value = intValue / Math.Pow(10, decimalPlaces) * (polarity == 0 ? 1 : -1);
                    PositionData.GetCommandPos(ref posdata);                    
                    Data += "<=TorqueMeterData_Output=>" + value.ToString() +unit+ "\t<=TorqueMeter_posdata=>" + posdata.ToString()  + "\r\n";
                    return value;
                }
                else
                {
                    Logger.Error("Parse TorqueMeter Data Fail");
                    return 0.0;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Parse TorqueMeter Data Fail." + ex.Message + " retrun 0");
                return 0.0;
            }
        }

        public bool HomeReset(MotionBase Home_MD)
        {
            bool chkhome_ret = false;
            bool home_ret_0 = false;
            //string mode = "ResetHome";
            while (true)
            {               
                IO_Status_Result = Home_MD.Check_IO_StartStatus(GlobalNew.ResetHome_check);
                if (IO_Status_Result)
                {                 
                    Logger.Info("move Jig in Save Status already");                   

                    if (Retry_Home)
                        Home_MD.Relative_Move(100, 10, 300, 0.3, 0.3);
                    else
                        MessageBox.Show("Jig is already in Save Status ,Confirm and Reset to Home", "Jig Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

                     //chkhome_ret = Home_MD.SyncHome(10, 80, 0);                   
                     chkhome_ret &= Home_MD.Check_IO_StartStatus(GlobalNew.ResetHome_status);
                                        
                    if (chkhome_ret)
                    {                                                               
                        //MessageBox.Show("Check Home Status:" + chkhome_ret.ToString() +" Reset to Home Ready","Check Home", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Logger.Info("Check Home Status:" + chkhome_ret.ToString() + " Reset to Home Ready");
                        home_ret_0 = Home_MD.SetCommandPos(0);
                        Logger.Info("SetCommandPos(0):"+ home_ret_0.ToString());                       
                        break;
                    }
                     else
                     {
                        MessageBox.Show("Check Home Status:"+ chkhome_ret.ToString()+ " Please make sure \"the handle\" and  \"left and right fixing blocks\" is correct then Reset Home again!!!!!!", "Check Home", MessageBoxButtons.OK, MessageBoxIcon.Warning);                                             
                     }
                }
                else
                {
                    if (Retry_Home)
                        MessageBox.Show("The Fix Block IO incorrect ,Please pull up the \"Handle\" and  \"cover\" the left right fix block", "Retry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else
                        MessageBox.Show("Torque sensor console value is not within reasonable range =>stop Motor, Please the \"pull up\" the handle then \"cover\" left and right fixing blocks to ResetHome!!!!", "Jig Status", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            return chkhome_ret;
        }

        public bool IO_Status_CHK_TM(MotionBase MD, ref bool mCancel_Status, string mode)
        {
            bool IO_result = false;
            bool IO_port_status = false;
            bool IO_ret = false;                  
            
            LogMessage("<==========IO_Status_CHK_TM_START===========>", MessageLevel.Warn);

            int handle_port_status = 0, Sleeve1_port_status = 0, Sleeve2_port_status = 0, left_FB_port_status = 0, Right_FB_port_status = 0; 
           
            MD.Get_IO_Status(ref handle_port_status, 0);
            Logger.Info("handle_port" + "status:" + handle_port_status.ToString());  //把手IO_Sensor
            MD.Get_IO_Status(ref Sleeve1_port_status, 1);
            Logger.Info("Sleeve1_port" + "status:" + Sleeve1_port_status.ToString());  //Sleeve1 IO_Sensor
            MD.Get_IO_Status(ref Sleeve2_port_status, 2);
            Logger.Info("Sleeve2_port" + "status:" + Sleeve2_port_status.ToString());  //Sleeve2 IO_Sensor
            MD.Get_IO_Status(ref left_FB_port_status, 3);
            Logger.Info("left_FB_port" + "status:" + left_FB_port_status.ToString());  //左固定塊IO_Sensor
            MD.Get_IO_Status(ref Right_FB_port_status, 4);
            Logger.Info("Right_FB_port" + "status:" + Right_FB_port_status.ToString());  //右固定塊IO_Sensor
            
            /////////////////IO_Status Chose method///////////////////
            if (mode == "LensTorque")
            {
                if (Sleeve1_port_status == 0 && Sleeve2_port_status == 1 && left_FB_port_status == 0 && Right_FB_port_status == 0 && handle_port_status == 1)  //LensTorque  把手下壓 => 放上Lens JIGS Head => 左右固定塊蓋上                             
                    IO_port_status = true;
                else
                {
                    IO_port_status = false;

                    if (Sleeve1_port_status != 0 || Sleeve2_port_status != 1)
                        Note_status = "Please check the \"Lens sleeve\" install is correct or incorret!!!";
                    else if (left_FB_port_status != 0)
                        Note_status = "Please \"cover\" the \"Left\" fixing blocks!!";
                    else if (Right_FB_port_status != 0)
                        Note_status = "Please \"cover\" the \"Right\" fixing blocks!!";
                    else if (handle_port_status != 1) //把手下壓
                        Note_status = "Please \"press down\" the handle!!";
                    else
                        Note_status = "Error:Please check IO port Status!!!";
                }
            }
            else if (mode == "GimTorque")
            {
                if (Sleeve1_port_status == 1 && Sleeve2_port_status == 0 && left_FB_port_status == 1 && Right_FB_port_status == 1 && handle_port_status == 1) //GIMTorque 把手下壓 => 放上GIM JIGS Head => 左右固定塊打開
                {                    
                    IO_port_status = true;
                }
                else
                {
                    IO_port_status = false;
                    
                    if (Sleeve1_port_status != 1 || Sleeve2_port_status != 0)
                        Note_status = "Please check the \"Gimbal sleeve\"install is correct or incorret!!!";
                    else if (left_FB_port_status != 1)
                        Note_status = "Please \"Open\" the \"Left\" fixing blocks!!";
                    else if (Right_FB_port_status != 1)
                        Note_status = "Please \"Open\" the \"Right\"  fixing blocks!!";
                    else if (handle_port_status != 1) //把手下壓
                        Note_status = "Please \"press down\" the handle!!";
                    else
                        Note_status = "Error:Please check IO port Status!!!";
                }
            }
            else
            {
                LogMessage("The mode is not exist!!", MessageLevel.Debug);
                return false;
            }
                     
            /////////////////IO_Status Chose method///////////////////

            if (IO_port_status) //(a_port_status == 0) //A_port
            {              
                LogMessage("Port_status Confirmation OK!!", MessageLevel.Info);
                IO_result = true;
            }
            else
            {
                IO_result = IO_Status_MessageBox(MD, mode, ref mCancel_Status, Note_status);               
            }
            //Sleep(500);
            LogMessage("<==========IO_Status_CHK_TM_END===========>", MessageLevel.Warn);
            return IO_result;
        }

        public bool IO_Status_MessageBox(MotionBase MD_box,string mode,ref bool Cancel_Status, string Note_message)
        {
            bool IO_Status_MessageBoxResult = false;               
                DialogResult result = MessageBox.Show(Note_message, "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.OK)
                {
                    // 用户单击了“确认”按钮，执行相关操作
                    Console.WriteLine("User clicked OK");
                    Sleep(500);                    
                    IO_Status_MessageBoxResult = IO_Status_CHK_TM(MD_box, ref CHK_Cancel_Status, mode);
                    if(IO_Status_MessageBoxResult)
                        LogMessage("Reconfirm IO Status is OK!!", MessageLevel.Info);
                    else
                        LogMessage("Reconfirm IO Status is NG!!", MessageLevel.Error);
                }
                else
                {
                    // 用户单击了“取消”按钮或关闭了对话框，不执行任何操作
                    Console.WriteLine("User clicked Cancel or closed the dialog box");
                    Sleep(500);
                    DialogResult Cancel_result = MessageBox.Show("Are you sure to Cancel Testing", "Cancel Checking", MessageBoxButtons.YesNo,MessageBoxIcon.Question);
                    if (Cancel_result == DialogResult.Yes)
                    {
                        Cancel_Status = true;                     
                    }
                    else
                    {
                        Cancel_Status = false;
                    }
                }
            return IO_Status_MessageBoxResult;
        }

       
    }
}
