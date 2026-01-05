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
    internal class Script_1Mot_CheckMoveDone_Pro : Script_1MotBase
    {
        string jsonStr = string.Empty;
        Dictionary<string, string> Output_Data = new Dictionary<string, string>();

        [Category("Motion Parameters Setting for Timeout"), Description("Set Status TimeOut : 該控件使用的TimeOut設定")]
        public int StatusTimeOut { get; set; } = 10000;


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
                int status = 0;
                var stopwatch = Stopwatch.StartNew();

                do
                {
                    MotionDev.MotionDone(ref status);
                    if (stopwatch.ElapsedMilliseconds > StatusTimeOut)
                    {
                        MotionDev.EmgStop();
                        LogMessage($"Status TimeOut , Motion Fail", MessageLevel.Error);
                        return false;
                    }

                } while (status != 0);


                LogMessage($"Motion is Done", MessageLevel.Info);
                return true;

            }
            catch (Exception e)
            {
                LogMessage($"CheckMotionDone Exception : {e}", MessageLevel.Error);
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
