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
    internal class Script_1Mot_CheckTCPAndRelease : Script_1MotBase
    {
        string jsonStr = string.Empty;
        Dictionary<string, string> Output_Data = new Dictionary<string, string>();
        public enum CMD_ACTION
        {
            CheckTCP,
            Release,
        }

        [Category("Command"), Description("CheckTCP確認PLC是否連線\r\nRelease斷開TCP連線")]
        public CMD_ACTION P0_Mode { get; set; } = CMD_ACTION.CheckTCP;

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
                switch (P0_Mode)
                {
                    case CMD_ACTION.CheckTCP:
                        try
                        {
                            if (MotionDev.CheckConnect())
                            {
                                LogMessage("Client connects success.");
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        catch (Exception e)
                        {
                            LogMessage($"Error : {e}", MessageLevel.Error);
                            return false;
                        }
                    case CMD_ACTION.Release:
                        try
                        {
                            if (MotionDev.ReleaseConnect())
                            {
                                LogMessage("Client is closed.");
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        catch (Exception e)
                        {
                            LogMessage($"Error : {e}", MessageLevel.Error);
                            return false;
                        }
                }
                return true;
            }
            catch (Exception e)
            {
                LogMessage($"Error : {e}", MessageLevel.Error);
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
