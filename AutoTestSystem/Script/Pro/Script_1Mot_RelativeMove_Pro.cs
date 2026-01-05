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
    internal class Script_1Mot_RelativeMove_Pro : Script_1MotBase
    {
        string jsonStr = string.Empty;
        Dictionary<string, Dictionary<int, int>> AddKeyName = new Dictionary<string, Dictionary<int, int>>();
        Dictionary<string, string> Output_Data = new Dictionary<string, string>();

        [Category("Move Status"), Description("到位回傳值")]
        public string CheckMotionDone_String { get; set; }

        [Category("Move Position"), Description("PLC_Command")]
        public string PLC_command { get; set; }

        [Category("Motion TimeOut"), Description("Motion TimeOut")]
        public int TimeOut { get; set; } = 10000;

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool PreProcess()
        {
            Output_Data.Clear();
            return true;
        }

        public override bool Process(MotionBase MotionDev,ref string output)
        {
            try
            {
                if(MotionDev.Relative_Move(PLC_command))
                {
                     if(MotionDev.Recieve_MotionDone(ref output, TimeOut))
                    {
                        if(output == CheckMotionDone_String)
                        {
                            LogMessage($"Move down=>Recieve :{output}",MessageLevel.Debug);
                            return true;
                        }
                        else
                        {
                            LogMessage("Recieved string not equal to Check status string", MessageLevel.Error);
                            return false;
                        }
                    }
                    else
                    {
                        LogMessage("Recieved Fail", MessageLevel.Error);
                        return false;
                    }
                }
                else
                {
                    LogMessage("Send command fail", MessageLevel.Error);
                    return false;
                }

            }
            catch (Exception e)
            {
                LogMessage($"Relative_Move Exception : {e}", MessageLevel.Error);
                return false;
            }
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
        public override bool Process(MotionBase MotionDevice)
        {
            throw new NotImplementedException();
        }

        public override bool PostProcess(string TestKeyword, string strCheckSpec, ref string strDataout)
        {
            throw new NotImplementedException();
        }
    }
}
