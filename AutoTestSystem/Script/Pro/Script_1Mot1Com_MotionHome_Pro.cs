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
using static AutoTestSystem.DAL.Communication;
using static AutoTestSystem.Equipment.Motion.Motion_DIOparam;

namespace AutoTestSystem.Script
{
    internal class Script_1Mot1Com_MotionHome_Pro : Script_1Mot1ComBase
    {

        string jsonStr = string.Empty;
        Dictionary<string, Dictionary<int, int>> AddKeyName = new Dictionary<string, Dictionary<int, int>>();
        Dictionary<string, string> Output_Data = new Dictionary<string, string>();

        [Category("Reset Home Parameters Setting for Common Setting"), Description("回零狀態檢查 Note:若設定False時 僅只有回Home觸發功能 請搭配Script_1Mot_CheckMoveDone_Pro控件進行狀態判斷")]
        public bool Status_Check { get; set; } = true;

        [Category("Reset Home Parameters Setting for Common Setting"), Description("Set Reset Home TimeOut : 當 Status_Check 設定true 會使用此參數設定")]
        public int HomeTimeOut { get; set; } = 30000;         

        public override void Dispose()
        {
            throw new NotImplementedException();
        }      

        public override bool PreProcess()
        {
            Output_Data.Clear();
            return true;
        }

        public override bool Process(ControlDeviceBase ComportDev, MotionBase MotionDev,ref string outputjsonStr)
        {
           
            bool home_ret = false;
            Stopwatch stopwatch = new Stopwatch();
            double out_homepos = 0;
            string RecDataAll = string.Empty;
            string RecMotion = string.Empty;


            home_ret = MotionDev.SyncHome();
            if (home_ret == false)
            {
                Output_Data.Add("Motion_Home", "Fail");
                RecMotion = JsonConvert.SerializeObject(Output_Data, Formatting.Indented);
                outputjsonStr = RecMotion;
                LogMessage($"Reset Home Fail", MessageLevel.Error);
                return false;
            }

            if (Status_Check)
            {
                Output_Data.Add("Motion_HomeStatus", "ON");
                bool Homme_Done = ResetHomeDone(MotionDev);
                if (Homme_Done)
                {
                    bool homePos = MotionDev.GetCurrentPos(ref out_homepos);
                    if (!homePos)
                    {
                        LogMessage($"Reset HomePos Fail, ResetHomeDone Fail", MessageLevel.Error);
                        Output_Data.Add("Motion_Home", "Fail");
                        RecMotion = JsonConvert.SerializeObject(Output_Data, Formatting.Indented);
                        outputjsonStr = RecMotion;
                        return false;
                    }
                    LogMessage($"Reset Home Done, ResetHomeDone:Position=>{out_homepos}", MessageLevel.Info);
                }
                else
                {
                    bool homePos = MotionDev.GetCurrentPos(ref out_homepos);
                    if (!homePos)
                    {
                        LogMessage($"Reset HomePos Fail, ResetHomeDone Fail", MessageLevel.Error);
                        Output_Data.Add("Motion_Home", "Fail");
                        RecMotion = JsonConvert.SerializeObject(Output_Data, Formatting.Indented);
                        outputjsonStr = RecMotion;
                        return false;
                    }
                    LogMessage($"Reset Home Fail:Position=>{out_homepos}, ResetHomeDone Fail", MessageLevel.Error);
                    Output_Data.Add("Motion_Home", "Fail");
                    RecMotion = JsonConvert.SerializeObject(Output_Data, Formatting.Indented);
                    outputjsonStr = RecMotion;
                    return false;
                }            
            }
         
            Output_Data.Add("Motion_Home", "Successed");
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

        public  bool ResetHomeDone(MotionBase MotionHomeDev)
        {
            try
            {
                int status = 0;
                var Homestopwatch = Stopwatch.StartNew();
                do
                {

                    if(MotionHomeDev.HomeDone(ref status))
                    { 
                        //LogMessage($"=====Home Status:{status}=====");
                        if (Homestopwatch.ElapsedMilliseconds > HomeTimeOut)
                        {
                            MotionHomeDev.EmgStop();
                            LogMessage($"Reset Home Fail, Home TimeOut!", MessageLevel.Error);
                            return false;
                        }
                    }
                    else
                    {
                        MotionHomeDev.EmgStop();
                        LogMessage($"Reset Home Fail, EmgStop !!!!", MessageLevel.Error);
                        return false;
                    }
                    Sleep(10);
                } while (status != 0);
                //LogMessage($"=====Home Status Done:{status}=====");
            }catch(Exception ex)
            {
                LogMessage($"Motion Home Error:{ex.Message}", MessageLevel.Error);
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
    }
}
