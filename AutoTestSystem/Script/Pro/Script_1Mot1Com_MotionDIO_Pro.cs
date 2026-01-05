using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.Base.MotionBase;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.Equipment.Motion.Motion_DIOparam;

namespace AutoTestSystem.Script
{
    internal class Script_1Mot1Com_MotionDIO_Pro : Script_1Mot1ComBase
    {

        /*[Category("Torque Parameters"), Description("自訂顯示名稱")]
        public ushort Port_count { get; set; }*/

        [Category("DIO Parameters Setting"), Description("DIO 參數設定"), Editor(typeof(DIOEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string MotorDio_Param { get; set; }
        [Category("DIO Mode"), Description("Single or Multiple Transmit")]
        public DIOMode DIO_Mode { get; set; } = DIOMode.Single_Transmit;
        [Category("DIO Mode"), Description("SetGet IO選擇")]
        public DIO_Selet SeletIOMethod { get; set; } = DIO_Selet.GetIO;

        [Category("DI Message Params"), Description("Function Enable")]
        public bool DIMessage { get; set; } = false;

        [Category("DI Message Params"), Description("Message")]
        public string Message { get; set; } = "";
        [Category("DI Message Params"), Description("選擇文件"), Editor(typeof(FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string sourceFileName { get; set; }


        [Category("StatusTimeOut"), Description("StatusTimeOut")]
        public int TimeOut { get; set; } = 10000;

        Dictionary<string, Dictionary<int, object>> dictionary_DIO = new Dictionary<string, Dictionary<int, object>>();
        Dictionary<string, string> DIO_Status = new Dictionary<string, string>();
        Dictionary<string, string> PortNum_Status = new Dictionary<string, string>();
        Dictionary<string, object> TorqueData = new Dictionary<string, object>();
        string jsonStr = string.Empty;


        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public enum DIOMode
        {
            Single_Transmit,
            Multiple_Transmit,
            Motion_Status
        }

        public override bool PreProcess()
        {
            PortNum_Status.Clear();
            DIO_Status.Clear();
            return true;
        }

        public override bool Process(ControlDeviceBase ComportDev, MotionBase MotionDev, ref string outputjsonStr)
        {
            string Status_msg = string.Empty;
            try
            {
              
                dictionary_DIO = ConvertJsonToDictionary();

                if (!IO_Status_CHK_TM(MotionDev, ref Status_msg))
                {
                    LogMessage($"<<<<<<==IO Fail=>>>>>>>", MessageLevel.Warn);
                    outputjsonStr = Status_msg;
                    return false;
                }
                
            }catch(Exception Dio_ex)
            {
                LogMessage($"IO or Status Exception:<<<<<<=={Dio_ex.Message}=>>>>>>>", MessageLevel.Error);
                return false;
            }
            jsonStr = Status_msg;
            outputjsonStr = jsonStr;

            return true;
        }

        public override bool PostProcess()
        {
            if (!string.IsNullOrEmpty(Spec))
            {
                string ret = string.Empty;
                ret = CheckRule(jsonStr, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Info);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;
        }


        public Dictionary<string, Dictionary<int, object>> ConvertJsonToDictionary()
        {
            var list = JsonConvert.DeserializeObject<List<KeyValue>>(MotorDio_Param);
            var AddKeyName = new Dictionary<string, Dictionary<int, object>>();
            var dictionary = new Dictionary<int, object>();
            if(list == null)
                return null;
            foreach (var item in list)
            {
                string name = item.KeyName;
                int key = int.Parse(item.DI.ToString());
                int value = int.Parse(item.Status.ToString());
                dictionary[key] = value;
                AddKeyName[name] = dictionary;
            }

            return AddKeyName;
        }

        public bool IO_Status_CHK_TM(MotionBase MD, ref string dio_msg)
        {
            bool Check_IO_ret = true;
            bool IOresult = false;



            LogMessage("<==========IO_Status_CHK_TM_START===========>", MessageLevel.Debug);

            //int handle_port_status = 0, Sleeve1_port_status = 0, Sleeve2_port_status = 0, left_FB_port_status = 0, Right_FB_port_status = 0;

            /*MD.Get_IO_Status(ref handle_port_status, 0);
            LogMessage($"Handle Port Number:0 ,Handle Port Status : {handle_port_status}", MessageLevel.Info);
            MD.Get_IO_Status(ref Sleeve1_port_status, 1);
            LogMessage($"Sleeve1 Port Number:1 ,Sleeve1 Port Status : {Sleeve1_port_status}", MessageLevel.Info);  //Sleeve1 IO_Sensor
            MD.Get_IO_Status(ref Sleeve2_port_status, 2);
            LogMessage($"Sleeve2 Port Number:2 ,Sleeve2 Port Status : {Sleeve2_port_status}", MessageLevel.Info);  //Sleeve2 IO_Sensor
            MD.Get_IO_Status(ref left_FB_port_status, 3);
            LogMessage($"left_FB Port Number:3 ,left_FB Port Status : {left_FB_port_status}", MessageLevel.Info); //左固定塊IO_Sensor 
            MD.Get_IO_Status(ref Right_FB_port_status, 4);
            LogMessage($"left_RB Port Number:4 ,left_RB Port Status : {Right_FB_port_status}", MessageLevel.Info);  //右固定塊IO_Sensor*/
            int port_DIOstatus = 0;
            int port_DIstatus = 0;
            int count_num = 0;
            int Get_count = 0;
            int SetGet_count = 0;
            ushort[] DIoutput = null;
            List<string> DiKey = new List<string>();
            string DiValue = string.Empty;
            Stopwatch stopwatch = new Stopwatch();
            ImageShowFrom frm_image = new ImageShowFrom();

            if (DIMessage)
            {
                // 顯示 Form 並繼續執行
                Thread formThread = new Thread(() =>
                {
                    //if (frm_image.InvokeRequired)
                    //{
                        frm_image.Cancel_Btn(false);
                        frm_image.SetImageShowForm(Message, "NA", "NA", 0, sourceFileName, false, false);
                        frm_image.ShowDialog();
                    //}
                });
                formThread.Start();
            }

            if (dictionary_DIO != null && dictionary_DIO.Count != 0)
            {
                bool allStatusMatch = false;

                switch (DIO_Mode) {
                    case DIOMode.Single_Transmit:
                        stopwatch.Start();
                        while (stopwatch.ElapsedMilliseconds < TimeOut && !allStatusMatch)
                        {
                            foreach (var DiStatus in dictionary_DIO.Values)
                            {
                                foreach (var StatusKey in DiStatus.Keys)
                                {
                                    count_num++;
                                    ushort key = (ushort)StatusKey;
                                    if (SeletIOMethod == DIO_Selet.SetIO)
                                    {
                                        LogMessage($"CheckDIO : Coil_Status Mode", MessageLevel.Info);
                                        IOresult = MD.SetGet_IO(key, (int)DiStatus[StatusKey], ref port_DIOstatus);

                                        if (IOresult == false)
                                        {
                                            LogMessage($"IOresult Fail: Please Check DIO Parameters Setting", MessageLevel.Error);
                                            return false;
                                        }

                                        //DIO_Status.Add($"DIOPortNum[{key}]", port_DIOstatus.ToString());
                                        //ChangeKeyName($"DIOPortNum[{key}]_{DiStatus[StatusKey]}");
                                        if (port_DIOstatus != (int)DiStatus[StatusKey])
                                        {
                                            LogMessage($"Dio_Status Error: PortNum[{key}]: {DiStatus[StatusKey]}, Port Status: {port_DIOstatus}", MessageLevel.Warn);
                                            count_num = 0;
                                            break;
                                        }
                                    }else if(SeletIOMethod == DIO_Selet.GetIO)
                                    {                                       
                                        Thread.Sleep(20);
                                        LogMessage($"CheckDI : Input_Status Mode(Only Read)", MessageLevel.Info);
                                        IOresult = MD.Get_IO_Status(ref port_DIstatus, key);
                                        //port_DIstatus = 0;
                                        //IOresult = true;
                                        if (IOresult == false)
                                        {
                                            LogMessage($"IOresult Fail: Please Check DI Parameters Setting", MessageLevel.Error);
                                            if (DIMessage)
                                            {
                                                if (frm_image.InvokeRequired)
                                                {
                                                    // 關閉 Form
                                                    frm_image.Invoke(new Action(() => frm_image.Close()));
                                                }
                                            }
                                            return false;                                            
                                        }

                                        //DIO_Status.Add($"DIPortNum[{key}]", port_DIstatus.ToString());
                                        //ChangeKeyName($"DIPortNum[{key}]_{DiStatus[StatusKey]}");
                                        if (port_DIstatus != (int)DiStatus[StatusKey])
                                        {
                                            LogMessage($"Di_Status Error: PortNum[{key}]: {DiStatus[StatusKey]}, Port Status: {port_DIstatus}", MessageLevel.Warn);
                                            count_num = 0;
                                            break;
                                        }
                                    }                                    
                                }
                                if (DiStatus.Count() == count_num)
                                {
                                    allStatusMatch = true;
                                    break;
                                }
                            }
                        }
                        if (allStatusMatch == false)
                        {
                            //MessageBox.Show($"IOStatus TimeOut", "TimeOut", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            LogMessage($"IOStatus TimeOut", MessageLevel.Error);
                            Check_IO_ret = false;
                        }
                        if (DIMessage)
                        {
                            if (frm_image.InvokeRequired)
                            {
                                frm_image.Cancel_Btn(false);
                                // 關閉 Form
                                frm_image.Invoke(new Action(() => frm_image.Close()));
                            }
                        }
                        break;
                    case DIOMode.Multiple_Transmit:
                        
                        if (SeletIOMethod == DIO_Selet.GetIO)
                        {
                            LogMessage($"CheckDI : Input_Status Mode(Only Read)", MessageLevel.Info);
                            IOresult = MD.Get_IO_Status(ref DIoutput);

                            if (IOresult == false)
                            {
                                LogMessage($"IOresult Fail: Please Check DIO Parameters Setting", MessageLevel.Error);
                                return false;
                            }

                            foreach (var DI in DIoutput)
                            {
                                DIO_Status.Add($"GetBit[{Get_count}]", DI.ToString());
                                Get_count++;
                            }
                        }else if (SeletIOMethod == DIO_Selet.SetIO)
                        {
                            int stop_flag = 0;
                            string DI_string = string.Empty;
                            LogMessage($"CheckDIO : Coil_Status Mode", MessageLevel.Info);
                            foreach (var DiStatus in dictionary_DIO.Values)
                            {
                                // 初始化一個長度為16的陣列，預設值為0
                                int[] diValues = new int[16];
                                // 根據 DiStatus.Keys 的數量來定義陣列長度
                                //int maxKey = DiStatus.Keys.Count; //> 0 ? Math.Max(15, DiStatus.Keys.Max()) : 15;
                                //int[] diValues = new int[maxKey];

                                // 將有在 DiStatus.Keys 設定的值設為1
                                foreach (var key in DiStatus.Keys)
                                {
                                    if (key >= 0 && key <= 16)
                                    {
                                        diValues[key] = (int)DiStatus[key] == 1 ? 1 : 0;
                                    }
                                    stop_flag++;
                                }
                                if (DiStatus.Count() == stop_flag)
                                {
                                    DiValue = string.Join("", diValues);
                                    break;
                                }
                            }
                            if(string.IsNullOrEmpty(DiValue))
                            {
                                LogMessage($"DiValue is Null oe Empty", MessageLevel.Error);
                                return false;
                            }
                            DiValue = DiValue.PadRight(16, '0');
                            int decimalValue = Convert.ToInt32(DiValue, 2);
                            string hexValue = decimalValue.ToString("X");
                            LogMessage($"DiStatus: {hexValue}", MessageLevel.Info);
                            IOresult = MD.SetGet_IO("0x00", hexValue, ref DIoutput);

                            if (IOresult == false)
                            {
                                LogMessage($"IOresult Fail: Please Check DIO Parameters Setting", MessageLevel.Error);
                                return false;
                            }

                            foreach (var DI in DIoutput)
                            {
                                DIO_Status.Add($"SetGetBit[{SetGet_count}]", DI.ToString());
                                SetGet_count++;
                            }
                        }
                        break;
                    case DIOMode.Motion_Status:
                           /* bool Check_MotionStatus = false;
                            int status = 0;
                            int StatusCount = 0;
                            //Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();                       
                        
                        while (stopwatch.ElapsedMilliseconds < TimeOut && !allStatusMatch)
                        {
                            foreach (var DiStatus in dictionary_DIO.Values)
                            {

                                foreach (var StatusKey in DiStatus.Keys)
                                {
                                    int key = (int)StatusKey;
                                    Check_MotionStatus = MD.MotionStatus(key, ref status); // Check Motion Status
                                    if (Check_MotionStatus == false)
                                    {
                                        MD.EmgStop();
                                        LogMessage($"<<<<<<==Check_Motion Status Fail=>>>>>>>", MessageLevel.Error);
                                        return false;
                                    }
                                    StatusCount++;

                                    if ((int)DiStatus[StatusKey] != status)
                                    {
                                        StatusCount = 0;
                                        break;
                                    }

                                }
                                if(DiStatus.Count() == StatusCount)
                                {
                                    allStatusMatch = true;
                                    break;
                                }
                            }
                        }

                        if(allStatusMatch == false)
                        {
                            MD.EmgStop();
                            LogMessage($"MotionStatus TimeOut", MessageLevel.Error);
                            Check_IO_ret = false;
                        }*/
                        break;
                    default:
                        LogMessage($"DIO Transmit is not defind", MessageLevel.Error);
                        return false;
                }

            }
            else
            {
                LogMessage($"dictionary_DIO.Count is zero", MessageLevel.Warn);
                Check_IO_ret = false;
            }

            if (Check_IO_ret) {
                DIO_Status.Add("DIO_Status", "OK");
            }
            else
            {
                DIO_Status.Add("DIO_Status", "NG");
                dio_msg = JsonConvert.SerializeObject(DIO_Status, Formatting.Indented);
                LogMessage($"DIO_Result:{dio_msg}", MessageLevel.Info);
                return false;
            }

            dio_msg = JsonConvert.SerializeObject(DIO_Status, Formatting.Indented);
            LogMessage($"DIO_Result:{dio_msg}",MessageLevel.Info);
            return true;
        }

        /*public void ChangeKeyName(string Keyname)
        {
                foreach (var name in dictionary_DIO)
                {
                    string oldKey = Keyname;
                    string newKey = name.Key;

                    if (string.IsNullOrEmpty(oldKey) || string.IsNullOrEmpty(newKey))
                    {
                        LogMessage($"The KeyName is exist null or Empty", MessageLevel.Warn);
                        break;
                    }

                    if (PortNum_Status.ContainsKey(oldKey) && !DIO_Status.ContainsKey(newKey))
                    {                       
                        DIO_Status[$"{newKey}_{oldKey}"] = PortNum_Status[oldKey];
                        break;
                    }
                }
         }*/

        public override bool PreProcess(string strParamInput)
        {
            throw new NotImplementedException();
        }

        public override bool Process(ControlDeviceBase comport, MotionBase MotionDev)
        {
            throw new NotImplementedException();
        }

        public override bool PostProcess(string TestKeyword, string strCheckSpec, ref string strDataout)
        {
            throw new NotImplementedException();
        }
        public enum DIO_Selet
        {
            SetIO,
            GetIO
        }
    }
}
