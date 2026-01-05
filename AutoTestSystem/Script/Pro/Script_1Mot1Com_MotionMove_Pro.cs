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
using System.ComponentModel;
using System.Diagnostics;
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
    internal class Script_1Mot1Com_MotionMove_Pro : Script_1Mot1ComBase
    {
        bool IO_Status_Result;
        private double _MaxVelocity;
        private double _TorqueVelocity;
        private double in_max_vel_limmit = 5000;
        string jsonStr = string.Empty;
        Dictionary<string, Dictionary<int, int>> AddKeyName = new Dictionary<string, Dictionary<int, int>>();
        Dictionary<string, string> Output_Data = new Dictionary<string, string>();

        [Category("Move Method"), Description("設定移動方式")]
        public MoveMethod Move_Method { get; set; } = MoveMethod.Absolute_Move;
        [Category("Motion Status"), Description("移動到位確認  =>Note:若設定False時 僅只有回Motion Move觸發功能 請搭配Script_1Mot_CheckMoveDone_Pro控件進行狀態判斷")]
        public bool CheckMotionStatus { get; set; } = true;

        [Category("Move Position"), Description("移動到的位置(mm) 支援%%")]
        public string MovePosition { get; set; }
        [Browsable(false)]
        [Category("Motion Position Setting"), Description("位置參數設定")]
        public double SetPos  { get; set; } = 0;

        [Category("Move Velocity tacc dac"), Description("移動最大速度")]
        public double Velocity
        {
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
        [Category("Move Velocity tacc dac"), Description("移動加速度")]
        public double tacc { get; set; } = 50;

        [Category("Move Velocity tacc dac"), Description("移動減速度")]
        public double dac { get; set; } = 50;

        [Category("Motion TimeOut"), Description("Motion TimeOut : 當 CheckMotionStatus 設定true 會使用此參數設定")]
        public int TimeOut { get; set; } = 30000;

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool PreProcess()
        {
            Output_Data.Clear();
            return true;
        }

        public override bool Process(ControlDeviceBase ComportDev, MotionBase MotionDev, ref string outputjsonStr)
        {

            bool Result = false;
            double Getout_pos = 0;
            double DoubleMovePosition = 0;
            string RecMotion = string.Empty;
            try
            {
                switch (Move_Method)
                {
                    case MoveMethod.Absolute_Move:
                        if (!string.IsNullOrEmpty(MovePosition))
                        {
                            DoubleMovePosition = double.Parse(ReplaceProp(MovePosition));
                            Result = MotionDev.Absolute_Move(DoubleMovePosition, 0, Velocity, tacc, dac);
                            if (CheckMotionStatus)
                                Result &= CheckMoveDone(MotionDev);
                        }else
                        {
                            LogMessage($"MovePosition is null or Empty", MessageLevel.Error);
                        }
                        break;
                    case MoveMethod.Relative_Move:
                        if (!string.IsNullOrEmpty(MovePosition))
                        {
                            Result = MotionDev.Relative_Move(DoubleMovePosition, 0, Velocity, tacc, dac);
                            if (CheckMotionStatus)
                                Result &= CheckMoveDone(MotionDev);
                        }else
                        {
                            LogMessage($"MovePosition is null or Empty", MessageLevel.Error);
                        }
                        break;
                    case MoveMethod.GetPosition:
                        Result = MotionDev.GetCommandPos(ref Getout_pos);
                        LogMessage($"Get Motion Position is {Getout_pos}", MessageLevel.Info);
                        Output_Data.Add("GetPosition", Getout_pos.ToString());
                        break;
                    case MoveMethod.SetPosition:
                        LogMessage($"Set Motion Position is {SetPos}", MessageLevel.Info);
                        Output_Data.Add("SetPosition", SetPos.ToString());
                        Result = MotionDev.SetCommandPos(SetPos);
                        break;
                    default:
                        break;
                }
                if (Result == false)
                {
                    LogMessage($"Motion Move Fail", MessageLevel.Error);
                    return false;
                }
            }catch(Exception ex)
            {
                LogMessage($"Motion Move Error:{ex.Message}", MessageLevel.Error);
                return false;
            }
       

            Output_Data.Add("Motion_Move", "Successed");
            RecMotion = JsonConvert.SerializeObject(Output_Data, Formatting.Indented);
            outputjsonStr = RecMotion;
            jsonStr = RecMotion;

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

        public bool CheckMoveDone(MotionBase CheckMove)
        {
            //bool Motion_Status = false;
            bool MoveDone = false;
            //int cmd_status = 0;
            int path_status = 0;
            var stopwatch = Stopwatch.StartNew();
            //狀態判斷
            do
            {
                CheckMove.GetMotionStatus(ref path_status);
                if (path_status == 0)
                {
                    MoveDone = true;
                    break;
                }
                if (path_status == -99)
                {
                    break;
                }
                if (stopwatch.ElapsedMilliseconds > TimeOut)
                {
                    CheckMove.EmgStop();
                    LogMessage($"Move TimeOut", MessageLevel.Error);
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
        public enum MoveMethod
        {
            Relative_Move,
            Absolute_Move,
            GetPosition,
            SetPosition
        }
    }
}
