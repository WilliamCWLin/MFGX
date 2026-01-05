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
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.DAL.Communication;
using static AutoTestSystem.BLL.Bd;
using System.Diagnostics;

namespace AutoTestSystem.Script
{
    internal class Script_1Mot1Com_TorqueJig_Pro:Script_1Mot1ComBase
    {
        private double _StartVelocity;
        private double _MaxVelocity;
        private double _TorqueVelocity;
        private double in_max_vel_limmit = 5000;//1500
        private double StrTorque_Angle;
        private double StrTorque_BackAngle;
        private double StrTorque_LimitAngle;
        int readCount;

        /*[Category("Torque Parameters"), Description("自訂顯示名稱"), Editor(typeof(DIOEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string MotorDio_Param { get; set; }*/

        [Category("Torque Motion Mode"), Description("測試模式")]
        public TorqueMode Torque_mode { get; set; } = TorqueMode.AMAX;

        [Category("Torque Parameters"), Description("設定移動方式")]
        public MoveMethod TorqueMove_Method { get; set; } = MoveMethod.Absolute_Move_Continue;

        [Category("Torque Parameters"), Description("設定移動狀態檢查")]
        public bool TorqueMoveStatus_Check { get; set; } = true;

        [Category("Torque Parameters"), Description("扭力移動角度(mm)")]
        //public double TorqueMove_Angle { get; set; }
        public double TorqueMove_Angle
        {
            get
            {
                if (StrTorque_Angle == 0)
                    return 0;
                else
                    return StrTorque_Angle;
            }
            set
            {
                StrTorque_Angle = CheckAngle(value);
            }
        }
        [Category("Torque Parameters"), Description("扭力移動回測角度(mm)")]
        //public double TorqueMoveBack_Angle { get; set; }
        public double TorqueMoveBack_Angle
        {
            get
            {
                if (StrTorque_BackAngle == 0)
                    return 0;
                else
                    return StrTorque_BackAngle;
            }
            set
            {
                StrTorque_BackAngle = CheckBackAngle(value);
            }
        }

        [Category("Torsiometer value Limit for Torque JIG"), Description("機台最大扭力數值限制")]
        public double TorqueLimit_value { get; set; } = 2.0;
        [Category("Torsiometer value for Collection"), Description("Skip 角度機台位置扭力數值(設定360為全收集)")]
        //public double Skip_TorquePosition { get; set; }
        public double Skip_TorquePosition {
            get
            {
                if (StrTorque_LimitAngle == 0)
                    return 360;
                else
                    return StrTorque_LimitAngle;
            }
            set
            {
                StrTorque_LimitAngle = CheckLimitAngle(value);
            }
        }
        [Category("Torsiometer value for Collection"), Description("Skip前幾筆扭力數值(設定0為全收集)")]
        public int Skip_TorqueTimes { get; set; } = 6;
        [Category("Torsiometer value for Collection"), Description("Skip前幾筆扭力數值Delay時間(ms)")]
        public int Skip_DelayTime { get; set; } = 20;
        [Category("Torsiometer value for Collection"), Description("最小扭力數值")]
        public double TorqueMin_value { get; set; } = 0.004;
        [Category("Torsiometer value for Collection"), Description("最扭力數值OFFSET")]
        public double TorqueOffset_value { get; set; } = 0;
        [Category("Torsiometer value for Ratio"), Description("調整扭力計數值的比例")]
        public double Torque_Ratio { get; set; } = 100;
        [Category("Torsiometer value Parameters"), Description("扭力數值清零")]
        public bool Zero_En { get; set; } = true;
        [Category("Torsiometer value Parameters"), Description("扭力數值存入.csv(Record資料夾內)")]
        public bool RecordValue { get; set; } = true;

        [Category("DelayTime"), Description("扭力移動間隔時間(預設500ms)")]
        public int TorqueMove_DelayTime { get; set; } = 500;
        /*[Category("DelayTime"), Description("讀取扭力計延遲時間(預設500ms)")]
        public int Torsiometer_DelayTime { get; set; } = 500;*/

        [Category("Torque Velocity"), Description("扭力起始速度")]
        public double Start_vel
        {
            get
            {
                if (_StartVelocity == 0)
                    return 100;
                else
                    return _StartVelocity;
            }
            set
            {
                _StartVelocity = CheckVelocity(value);
            }
        }
        [Category("Torque Velocity"), Description("扭力最大速度")]
        public double Max_vel {
            get
            {
                if (_MaxVelocity == 0)
                    return 500;
                else
                    return _MaxVelocity;
            }
            set
            {
                _MaxVelocity = CheckVelocity(value);
            }
        }

        [Category("Torque Parameters"), Description("自訂顯示名稱")]
        public double tacc { get; set; } = 50;

        [Category("Torque Parameters"), Description("自訂顯示名稱")]
        public double dac { get; set; } = 50;

        [Category("Torque Motion Parameters"), Description("Motion TimeOut")]
        public int TimeOut { get; set; } = 30000;

        double MinTorque;
        double MaxTorque;
        double AvgTorque;

        Dictionary<int, int> dictionary_FB = new Dictionary<int, int>();
        Dictionary<string, object> TorqueData = new Dictionary<string, object>();
        bool isRunning = true;
        string jsonStr = string.Empty;
        string cachedData = string.Empty;
        string receivedData = string.Empty;
        ushort[] readdata;
        string Data = string.Empty;
        List<double> ValueList = new List<double>(); // 定义一个动态列表
        string DataFilePath;

        public override void Dispose()
        {
            throw new NotImplementedException();
        }      

        public override bool PreProcess()
        {
            DataFilePath = string.Empty;
            DataFilePath = $"Record\\{ DateTime.Now.ToString("yyyy_MM_dd")}\\";
            if (!Directory.Exists(DataFilePath))
            {
                Console.WriteLine("文件夹不存在，将创建新文件夹。");
                Directory.CreateDirectory(DataFilePath);
            }

            receivedData = string.Empty;
            cachedData = string.Empty;
            Data = string.Empty;
            MinTorque = 0;
            MaxTorque = 0;
            AvgTorque = 0;
            readCount = 0;
            //TorqueMove_distance = (double)3200 / 360;
            ValueList.Clear();
            TorqueData.Clear();
            readdata = null;
            isRunning = true;
            return true;
        }

        public override bool Process(ControlDeviceBase ComportDev, MotionBase MotionDev, ref string outputjsonStr)
        {
            bool Motion_Ret;
            bool COM_REF;
            string unit = string.Empty;

            /*if(Description.Contains("Pan"))
            {
                MinTorque = 0.8;
                MaxTorque = 3.2;
                AvgTorque = 2.1;
            }
            else if(Description.Contains("Rotate"))
            {
                MinTorque = 0.6;
                MaxTorque = 2.7;
                AvgTorque = 1.8;
            }
            TorqueData.Add($"{Description}_Min", MinTorque.ToString("F3"));
            TorqueData.Add($"{Description}_Max", MaxTorque.ToString("F3"));
            TorqueData.Add($"{Description}_Avg", AvgTorque.ToString("F3"));
            jsonStr = JsonConvert.SerializeObject(TorqueData, Formatting.Indented);
            outputjsonStr = jsonStr;
            LogMessage($"TorqueData => {outputjsonStr}", MessageLevel.Info);
            return true;*/
            try
            {
                switch (Torque_mode)
                {
                    case TorqueMode.AMAX:
                        COM_REF = ComportDev.READ(ref receivedData);//清除前第五筆資料
                        LogMessage($"Frist Torsiometer ComportData => {receivedData}", MessageLevel.Info);
                        if (!COM_REF)
                        {
                            LogMessage($"Frist Torsiometer ComportData Read Fail => {receivedData}", MessageLevel.Error);
                            return false;
                        }
                        break;
                    case TorqueMode.LeadShine:
                        if (ComportDev is SIMBA_Sensor)
                        {
                            SIMBA_Sensor device = (SIMBA_Sensor)ComportDev;
                            bool Reset_simba = false;
                            if (Zero_En)
                            {
                                Sleep(500);
                                Reset_simba = ComportDev.Init("");
                                if (Reset_simba == false)
                                {
                                    LogMessage($"Reset SIMBA Torsiometer Fail", MessageLevel.Error);
                                    return false;
                                }
                            }
                            COM_REF = device.READ("0x50", 2, ref readdata, TransmitMode.Holding_registers_Single);
                            if (!COM_REF)
                            {
                                LogMessage($"Frist Torsiometer ComportData Read Fail => {readdata}", MessageLevel.Error);
                                return false;
                            }
                        }else
                        {
                            LogMessage($"ComportDev is not SIMBA_Sensor", MessageLevel.Error);
                            return false;
                        }
                        break;

                }

                Thread readThread = new Thread(() =>    // Read Torque Thread
                {
                   Torsiometer(ComportDev, MotionDev);
                });


                readThread.Start(); // 启动线程 
                LogMessage("\nMotion Start\n", MessageLevel.Info);

                switch (TorqueMove_Method)
                {
                    case MoveMethod.Absolute_Move_Continue:
                        Motion_Ret = AbsoluteMotionMoveContinue(MotionDev);
                        if (Motion_Ret == false)
                        {
                            LogMessage("\nMotion Stop\n", MessageLevel.Error);
                            isRunning = false;
                            readThread.Join(); // 等待线程结束
                            return false;
                        }
                        break;
                    case MoveMethod.Relative_Move_Continue:
                        Motion_Ret = RelativeMotionMoveContinue(MotionDev);
                        if (Motion_Ret == false)
                        {
                            LogMessage("\nMotion Stop\n", MessageLevel.Error);
                            isRunning = false;
                            readThread.Join(); // 等待线程结束
                            return false;
                        }
                        break;
                    case MoveMethod.Absolute_Move_Triger:
                        Motion_Ret = AbsoluteMotionMoveTriger(MotionDev);
                        if (Motion_Ret == false)
                        {
                            LogMessage("\nMotion Stop\n", MessageLevel.Error);
                            isRunning = false;
                            readThread.Join(); // 等待线程结束
                            return false;
                        }
                        break;
                    case MoveMethod.Relative_Move_Triger:
                        Motion_Ret = RelativeMotionMoveTriger(MotionDev);
                        if (Motion_Ret == false)
                        {
                            LogMessage("\nMotion Stop\n", MessageLevel.Error);
                            isRunning = false;
                            readThread.Join(); // 等待线程结束
                            return false;
                        }
                        break;
                    default:
                        break;
                }
                //MessageBox.Show("Test");

                LogMessage("\nMotion End\n", MessageLevel.Info);
                //// 停止线程
                isRunning = false;
                readThread.Join(); // 等待线程结束

                if (ValueList.Count > 0)
                {
                    MinTorque = ValueList.Min();
                    MaxTorque = ValueList.Max();
                    AvgTorque = ValueList.Average();
                }

                Sleep(1000);
                //ValueList.Clear();
                //isRunning = true;

                TorqueData.Add($"{Description}_Min", MinTorque.ToString("F2"));
                TorqueData.Add($"{Description}_Max", MaxTorque.ToString("F2"));
                TorqueData.Add($"{Description}_Avg", AvgTorque.ToString("F2"));

                jsonStr = JsonConvert.SerializeObject(TorqueData, Formatting.Indented);
                outputjsonStr = jsonStr;
                LogMessage($"TorqueData => {outputjsonStr}", MessageLevel.Info);
            }
            catch(Exception ex_Torque)
            {
                LogMessage($"Torque Test Exception => {ex_Torque.Message}", MessageLevel.Error);
                return false;
            }

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
        public bool RelativeMotionMoveTriger(MotionBase RelativeMove)
        {
            bool Result;
            //MessageBox.Show("Test_1");
            LogMessage($"TorqueMove_Angle:{TorqueMove_Angle}", MessageLevel.Info);
            if (isRunning == false)
            {
                RelativeMove.EmgStop();
                LogMessage("Torsiometer have some problem", MessageLevel.Warn);
                return false;
            }
            else
            {
                Result = RelativeMove.Relative_Move(TorqueMove_Angle, Start_vel, Max_vel, tacc, dac);
                if (TorqueMoveStatus_Check)
                    Result &= CheckMoveDone(RelativeMove);
            }
            if (isRunning == false || Result == false)
            {
                RelativeMove.EmgStop();
                LogMessage("Torsiometer have some problem", MessageLevel.Warn);
                return false;
            }
            Sleep(TorqueMove_DelayTime);

            if (TorqueMoveBack_Angle != 0)
            {
                LogMessage($"TorqueMoveBack_Angle:{TorqueMoveBack_Angle}", MessageLevel.Info);
                Result = RelativeMove.Relative_Move(TorqueMoveBack_Angle, Start_vel, Max_vel, tacc, dac);
                if (TorqueMoveStatus_Check)
                    Result &= CheckMoveDone(RelativeMove);
                if (isRunning == false || Result == false)
                {
                    RelativeMove.EmgStop();
                    LogMessage("Torsiometer have some problem", MessageLevel.Warn);
                    return false;
                }
                Sleep(TorqueMove_DelayTime);
            }

            return true;
        }
        public bool AbsoluteMotionMoveTriger(MotionBase AbsoluteMove)
        {
            bool Result;
            //MessageBox.Show("Test_1");
            LogMessage($"TorqueMove_Angle:{TorqueMove_Angle}", MessageLevel.Info);
            if (isRunning == false)
            {
                AbsoluteMove.EmgStop();
                LogMessage("Torsiometer have some problem", MessageLevel.Warn);
                return false;
            }
            else
            {
                Result = AbsoluteMove.Absolute_Move(TorqueMove_Angle, Start_vel, Max_vel, tacc, dac);
                if(TorqueMoveStatus_Check)
                    Result &= CheckMoveDone(AbsoluteMove);
            }
            if (isRunning == false || Result == false)
            {
                AbsoluteMove.EmgStop();
                LogMessage("Torsiometer have some problem", MessageLevel.Warn);
                return false;
            }
            Sleep(TorqueMove_DelayTime);

            if(TorqueMoveBack_Angle != 0)
            {
                LogMessage($"TorqueMoveBack_Angle:{TorqueMoveBack_Angle}", MessageLevel.Info);             
                Result = AbsoluteMove.Absolute_Move(TorqueMoveBack_Angle, Start_vel, Max_vel, tacc, dac);
                if (TorqueMoveStatus_Check)
                    Result &= CheckMoveDone(AbsoluteMove);
                if (isRunning == false || Result == false)
                {
                    AbsoluteMove.EmgStop();
                    LogMessage("Torsiometer have some problem", MessageLevel.Warn);
                    return false;
                }
                Sleep(TorqueMove_DelayTime);
            }

            return true;
        }

        public bool RelativeMotionMoveContinue(MotionBase RelativeMove)
        {
            bool Result;

            //MessageBox.Show("Test_1");
            LogMessage($"TorqueMove_Angle:{TorqueMove_Angle}", MessageLevel.Info);
            if (isRunning == false)
            {
                RelativeMove.EmgStop();
                LogMessage("Torsiometer have some problem", MessageLevel.Warn);
                return false;
            }
            else
            {
                Result = RelativeMove.Relative_Move(TorqueMove_Angle, Start_vel, Max_vel, tacc, dac);
                if (TorqueMoveStatus_Check)
                    Result &= CheckMoveDone(RelativeMove);
            }
            //MessageBox.Show("Test_2");
            Sleep(TorqueMove_DelayTime);

            if (isRunning == false || Result == false)
            {
                RelativeMove.EmgStop();
                LogMessage("Torsiometer or Positive(+) Motion Move have some problem", MessageLevel.Warn);
                return false;
            }
            else
            {
                Result = RelativeMove.Relative_Move(TorqueMove_Angle * (-1), Start_vel, Max_vel, tacc, dac);
                if (TorqueMoveStatus_Check)
                    Result &= CheckMoveDone(RelativeMove);
            }
            //MessageBox.Show("Test_3");
            Sleep(TorqueMove_DelayTime);

            if (isRunning == false || Result == false)
            {
                RelativeMove.EmgStop();
                LogMessage("Torsiometer or Negative(-) Move Motion have some problem", MessageLevel.Warn);
                return false;
            }
            else
            {
                Result = RelativeMove.Relative_Move(TorqueMove_Angle * (-1), Start_vel, Max_vel, tacc, dac);
                if (TorqueMoveStatus_Check)
                    Result &= CheckMoveDone(RelativeMove);
            }
            //MessageBox.Show("Test_4");
            Sleep(TorqueMove_DelayTime);

            if (isRunning == false || Result == false)
            {
                RelativeMove.EmgStop();
                LogMessage("Torsiometer or Negative(-) Move Motion have some problem", MessageLevel.Warn);
                return false;
            }
            else
            {
                Result = RelativeMove.Relative_Move(TorqueMove_Angle, Start_vel, Max_vel, tacc, dac);
                if (TorqueMoveStatus_Check)
                    Result &= CheckMoveDone(RelativeMove);
            }
            //MessageBox.Show("Test_5");
            Sleep(TorqueMove_DelayTime);

            if (isRunning == false || Result == false)
            {
                RelativeMove.EmgStop();
                LogMessage("Torsiometer or Positive(+) Motion Move have some problem", MessageLevel.Warn);
                return false;
            }
            return true;
        }
        public bool AbsoluteMotionMoveContinue(MotionBase AbsoluteMove)
        {
            bool Result;

            //MessageBox.Show("Test_1");
            LogMessage($"TorqueMove_Angle:{TorqueMove_Angle}", MessageLevel.Info);
            if (isRunning == false)
            {
                AbsoluteMove.EmgStop();
                LogMessage("Torsiometer have some problem", MessageLevel.Warn);
                return false;
            }
            else
            {
                readCount = 0;
                Result = AbsoluteMove.Absolute_Move(0, Start_vel, Max_vel, tacc, dac);
                if (TorqueMoveStatus_Check)
                    Result &= CheckMoveDone(AbsoluteMove);
            }
            Sleep(TorqueMove_DelayTime);
            if (isRunning == false || Result == false)
            {
                AbsoluteMove.EmgStop();
                LogMessage("Torsiometer have some problem", MessageLevel.Warn);
                return false;
            }
            else
            {
                readCount = 0;
                Result = AbsoluteMove.Absolute_Move(TorqueMove_Angle, Start_vel, Max_vel, tacc, dac);
                if (TorqueMoveStatus_Check)
                    Result &= CheckMoveDone(AbsoluteMove);
            }
            //MessageBox.Show("Test_2");
            Sleep(TorqueMove_DelayTime);

            if (isRunning == false || Result == false)
            {
                AbsoluteMove.EmgStop();
                LogMessage("Torsiometer or Positive(+) Motion Move have some problem", MessageLevel.Warn);
                return false;
            }
            else
            {
                readCount = 0;
                Result = AbsoluteMove.Absolute_Move(TorqueMove_Angle * (-1), Start_vel, Max_vel, tacc, dac);
                if (TorqueMoveStatus_Check)
                    Result &= CheckMoveDone(AbsoluteMove);
            }
            //MessageBox.Show("Test_3");
            Sleep(TorqueMove_DelayTime);

            if (isRunning == false || Result == false)
            {
                AbsoluteMove.EmgStop();
                LogMessage("Torsiometer or Negative(-) Move Motion have some problem", MessageLevel.Warn);
                return false;
            }
            else
            {
                readCount = 0;
                Result = AbsoluteMove.Absolute_Move(0, Start_vel, Max_vel, tacc, dac);
                if (TorqueMoveStatus_Check)
                    Result &= CheckMoveDone(AbsoluteMove);
            }
            //MessageBox.Show("Test_4");
            Sleep(TorqueMove_DelayTime);

            if (isRunning == false || Result == false)
            {
                AbsoluteMove.EmgStop();
                LogMessage("Torsiometer or Positive(+) Motion Move have some problem", MessageLevel.Warn);
                return false;
            }

            return true;
        }

        public bool CheckMoveDone(MotionBase CheckMove)
        {
            bool MoveDone = false;
            //int cmd_status = 0;
            int path_status = 0;
            var stopwatch =  Stopwatch.StartNew();

            //狀態判斷
            do
            {
                //Motion_Status = CheckMove.MotionStatus(4, ref cmd_status);
                //Motion_Status &= CheckMove.MotionStatus(5, ref path_status);
                /*if (Motion_Status == false)
                    return false;*/
                CheckMove.GetMotionStatus(ref path_status);
                if (path_status == 0)
                {
                    MoveDone = true;
                    break;
                }
                if(path_status == -99)
                {
                    break;
                }
                if (stopwatch.ElapsedMilliseconds > TimeOut)
                {
                    CheckMove.EmgStop();
                    LogMessage($"Move TimeOut", MessageLevel.Error);
                    //return false;
                    break;
                }
            } while (MoveDone == false);

            double Pos = 0;
            bool PosRet = CheckMove.GetCommandPos(ref Pos);
            if (PosRet == false)
                return false;
            if (MoveDone == false)
            {
                LogMessage($"Move Done Fail", MessageLevel.Error);
                return false;
            }
            else
            {
                LogMessage($"Move {Pos} Done Success", MessageLevel.Info);
            }
            return true;
        }

        public void Torsiometer(ControlDeviceBase ComportDev,MotionBase MotionDev)
        {
            bool COM_REF;
            double Pos = 0;
            string Data_Log = string.Empty;
            string[] lines = null;

            try
            {
                while (isRunning) // 使用共享变量作为循环条件
                {

                    switch (Torque_mode)
                    {
                        case TorqueMode.AMAX:
                            Sleep(500);
                            // 读取串口数据
                            COM_REF = ComportDev.READ(ref receivedData);
                            LogMessage($"READ ComportData => {receivedData}", MessageLevel.Info);
                            if (!COM_REF)
                            {
                                LogMessage($"READ ComportData is Fail,Please Delay more Time", MessageLevel.Error);
                                isRunning = false;
                                break;
                            }

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
                                torque_value = Math.Abs(ParseTorqueMeterData(cleanedReading.Replace("\u000241", ""), MotionDev, ref Data));
                                LogMessage($"ParseTorqueMeterData END\n", MessageLevel.Info);
                                if (torque_value <= TorqueMin_value)
                                    break;
                                //Franklin modify
                                ValueList.Add(torque_value);

                                File.AppendAllText($"{DataFilePath}{PopMoreData("ProductSN")}_{Description}_{DateTime.Now.ToString("yyyy_MM_dd")}_DATA.txt", Data);
                                Data = string.Empty;

                                if (torque_value > TorqueLimit_value)
                                {
                                    isRunning = false;
                                    MotionDev.EmgStop();
                                    LogMessage($"Torque in abnormal Max Value stop Motor !!!!! Please Move JIG", MessageLevel.Warn);
                                    break;
                                }

                            }
                            break;
                        case TorqueMode.LeadShine:

                            if (ComportDev is SIMBA_Sensor)
                            {
                                double Value = 0.00;
                                SIMBA_Sensor device = (SIMBA_Sensor) ComportDev;

                                COM_REF = device.READ("0x50", 2, ref readdata, TransmitMode.Holding_registers_Single);
                                if (!COM_REF)
                                {
                                    LogMessage($"READ ComportData is Fail,Please Delay more Time", MessageLevel.Error);
                                    isRunning = false;
                                    break;
                                }

                                //LogMessage($"SIMBATOUCH 的裝置\n", MessageLevel.Info);
                                // 將ushort數組轉換為int數組
                                int[] intArray = readdata.Select(ushortValue => (int)ushortValue).ToArray();
                                if (intArray[0] == 65535)//負數
                                {
                                    Value = ~(intArray[0] - intArray[1]) / Torque_Ratio;//補數
                                    if (TorqueOffset_value != 0)
                                    {
                                        Value = Value + TorqueOffset_value;
                                    }
                                }
                                else
                                {
                                    Value = intArray[1] / Torque_Ratio;    // 根據你的設備可能需要調整比例
                                    if (TorqueOffset_value != 0)
                                    {
                                        Value = Value - TorqueOffset_value;
                                    }
                                }
                                readCount++;
                                if (readCount <= Skip_TorqueTimes)
                                {
                                    Sleep(Skip_DelayTime);
                                    LogMessage($"Skip {Skip_TorqueTimes} Times TorqueMeterData Output Data: {Math.Abs(Value)}", MessageLevel.Info);
                                    continue;
                                }

                                if (Math.Abs(Value) <= TorqueMin_value) //去掉數值太小
                                {
                                    //LogMessage($"Invalid TorqueMeterData_Output=>{Math.Abs(Value)}", MessageLevel.Warn);
                                    Sleep(Skip_DelayTime);
                                    continue;
                                }

                                //LogMessage($"SIMBATOUCH數值: {Value}", MessageLevel.Info);

                                bool PostionGet = MotionDev.GetCommandPos(ref Pos);
                                if (PostionGet == false)
                                {
                                    isRunning = false;
                                    LogMessage($"Torque Position Error stop Motor !!!!! Please Move JIG", MessageLevel.Warn);
                                    break;
                                }

                                //Pos = Pos * TorqueMove_distance;

                                if (Math.Abs(Pos) > Skip_TorquePosition) //skip 位置的數值收集 360 全收集
                                {
                                    //isRunning = false;
                                    //MotionDev.EmgStop();
                                    //LogMessage($"Position is over TorqueLimit_Position : {Pos} stop Motor !!!!! Please Move JIG", MessageLevel.Warn);
                                    Sleep(Skip_DelayTime);
                                    LogMessage($"Skip Position {Pos} and continue", MessageLevel.Warn);
                                    continue;
                                }

                                //Franklin modify Add Torque Value
                                ValueList.Add(Math.Abs(Value));

                                Data_Log += $"{Pos}\t{Math.Abs(Value)}\n";
                                                              
                                if (Math.Abs(Value) > TorqueLimit_value)
                                {
                                    MotionDev.EmgStop();
                                    isRunning = false;
                                    LogMessage($"Torque in abnormal Max: {Math.Abs(Value)} Value stop Motor !!!!! Please Move JIG", MessageLevel.Warn);
                                    break;
                                }

                            }else
                            {
                                isRunning = false;
                                MotionDev.EmgStop();
                                LogMessage($"ComportDev is not SIMBA_Sensor stop Motor !!!!! Please Move JIG", MessageLevel.Error);
                                break;
                            }
                            break;
                    }
                    //具有快顯功能表                   
                }

                // 確認列表中有足夠的數值
                /*if (ValueList.Count >= Skip_Torque_value)
                {
                    // 移除最後 n 個數值
                    ValueList.RemoveRange(ValueList.Count - Skip_Torque_value, Skip_Torque_value);
                    LogMessage($"ValueList RemoveRange {Skip_Torque_value} value", MessageLevel.Warn);
                }*/

                if (RecordValue)
                {
                    string Final_Data = string.Empty;
                    if (!string.IsNullOrEmpty(Data_Log))
                    {
                        string Header = $"Position\tTorqueValue\n";
                        Final_Data = $"{Header}{Data_Log}";
                        // 將字符串分割成行
                        lines = Final_Data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        // 使用StreamWriter寫入CSV文件
                        using (StreamWriter writer = new StreamWriter($"{DataFilePath}{PopMoreData("ProductSN")}_{Description}_{DateTime.Now.ToString("yyyy_MM_dd_hh_ss_ff")}_DATA.csv"))
                        {
                            foreach (string line in lines)
                            {
                                // 將每行分割成列並寫入CSV
                                string[] columns = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                writer.WriteLine(string.Join(",", columns));
                            }
                        }

                        LogMessage($"Data:{Final_Data}", MessageLevel.Info);
                        Data_Log = string.Empty;
                    }else
                        LogMessage($"Data_Log is Empty or null", MessageLevel.Warn);
                }
            }
            catch(Exception ex)
            {
                LogMessage($"Torsiometer Error: {ex.Message}", MessageLevel.Error);
            }
        }
        public string Collection_Data(string input)
        {
            Data += input;
            return Data;
        }

        public double ParseTorqueMeterData(string input, MotionBase PositionData, ref string Data)
        {
            LogMessage($"\nParseTorqueMeterData Start\n", MessageLevel.Info);
            try
            {
                if (input.Length != 12)
                    return 0.0;

                string unit = string.Empty;
                Data += $"{DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss")} <=TorqueMeterData_Input=> {input} ";
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
                    if (Math.Abs(value) <= 0.2)
                    {
                        LogMessage($"Invalid TorqueMeterData_Output=>{Math.Abs(value)}{unit} Return 0.0", MessageLevel.Warn);
                        return 0.0;
                    }
                    else
                    {
                        PositionData.GetCommandPos(ref posdata);
                        //posdata = posdata / TorqueMove_distance;
                        Data += $"<=TorqueMeterData_Output=> {value}{unit} <=TorqueMeter_posdata=> {posdata}\n";
                        LogMessage($"Parse TorqueMeter Data:{Data}", MessageLevel.Info);
                        return value;
                    }
                }
                else
                {
                    LogMessage($"Parse TorqueMeter Data Fail", MessageLevel.Error);
                    return 0.0;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Parse TorqueMeter Data Error :{ex.Message} retrun 0", MessageLevel.Error);
                return 0.0;
            }
        }

        public bool IO_Status_CHK_TM(MotionBase MD)
        {
            bool IO_result = false;
            bool IO_port_status = false;
            bool IO_ret = false;                  
            
            LogMessage("<==========IO_Status_CHK_TM_START===========>", MessageLevel.Warn);

            int handle_port_status = 0, Sleeve1_port_status = 0, Sleeve2_port_status = 0, left_FB_port_status = 0, Right_FB_port_status = 0;

            MD.Get_IO_Status(ref handle_port_status, 0);
            LogMessage($"Handle Port Number:0 ,Handle Port Status : {handle_port_status}", MessageLevel.Info);
            MD.Get_IO_Status(ref Sleeve1_port_status, 1);
            LogMessage($"Sleeve1 Port Number:1 ,Sleeve1 Port Status : {Sleeve1_port_status}", MessageLevel.Info);  //Sleeve1 IO_Sensor
            MD.Get_IO_Status(ref Sleeve2_port_status, 2);
            LogMessage($"Sleeve2 Port Number:2 ,Sleeve2 Port Status : {Sleeve2_port_status}", MessageLevel.Info);  //Sleeve2 IO_Sensor
            MD.Get_IO_Status(ref left_FB_port_status, 3);
            LogMessage($"left_FB Port Number:3 ,left_FB Port Status : {left_FB_port_status}", MessageLevel.Info); //左固定塊IO_Sensor 
            MD.Get_IO_Status(ref Right_FB_port_status, 4);
            LogMessage($"left_RB Port Number:4 ,left_RB Port Status : {Right_FB_port_status}", MessageLevel.Info);  //右固定塊IO_Sensor

            int port_status = 0;
            if (dictionary_FB != null && dictionary_FB.Count !=0)
            {
                foreach(var DiStatus in dictionary_FB)
                {
                    ushort DiKey = (ushort)DiStatus.Key;
                    MD.Get_IO_Status(ref port_status, DiKey);

                    if(port_status != DiStatus.Value)
                    {
                        LogMessage($"DiStatus Error: Port Number:{DiKey},Port Status : {port_status}",MessageLevel.Warn);
                        return false;
                    }
                }
            }          
           return true;
        }

        private double CheckVelocity(double _Velocity)
        {
            if (_Velocity > in_max_vel_limmit)
            {
                MessageBox.Show($"Torque Velocity is over Range", "Torque Velocity Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return _TorqueVelocity;
            }
            _TorqueVelocity = _Velocity;
            return _Velocity;
        }
        private double CheckAngle(double _Angle)
        {
            if (_Angle < -360 || _Angle > 360)
            {
                MessageBox.Show($"Torque Angle is over Range", "Torque Angle Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return StrTorque_Angle;
            }
            StrTorque_Angle = _Angle;
            return _Angle;
        }
        private double CheckBackAngle(double _Angle)
        {
            if (_Angle < -360 || _Angle > 360)
            {
                MessageBox.Show($"Torque BackAngle is over Range", "Torque BackAngle Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return StrTorque_BackAngle;
            }
            StrTorque_BackAngle = _Angle;
            return _Angle;
        }
        private double CheckLimitAngle(double _Angle)
        {
            if (_Angle < -360 || _Angle > 360)
            {
                MessageBox.Show($"Torque LimitAngle is over Range", "Torque LimitAngle Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return StrTorque_LimitAngle;
            }
            StrTorque_LimitAngle = _Angle;
            return _Angle;
        }
        public enum TorqueMode
        {
            AMAX,
            LeadShine
        }
        public enum MoveMethod
        {
            Relative_Move_Continue,
            Absolute_Move_Continue,
            Relative_Move_Triger,
            Absolute_Move_Triger
        }

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
    }
}
