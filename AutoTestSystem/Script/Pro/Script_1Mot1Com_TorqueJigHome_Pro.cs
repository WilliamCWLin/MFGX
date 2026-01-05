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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.Equipment.Motion.Motion_DIOparam;

namespace AutoTestSystem.Script
{
    internal class Script_1Mot1Com_TorqueJigHome_Pro : Script_1Mot1ComBase
    {
        bool IO_Status_Result;
        string jsonStr = string.Empty;
        Dictionary<string, Dictionary<int, int>> AddKeyName = new Dictionary<string, Dictionary<int, int>>();
        Dictionary<string, string> Output_Data = new Dictionary<string, string>();

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(TorqueMode))]
        public string Mode { get; set; } = "Home";

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public double Pos { get; set; } = 0;

        [Category("Common Parameters"), Description("自訂顯示名稱"), Editor(typeof(DIOEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string MotorDio_Param { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int HomeTimeout { get; set; } = 60000;

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
           
            bool chkhome_ret = false;
            bool home_ret_0 = false;
            double out_pos = 0;

            if (Mode == "GetCommandPos")
            {
                try
                {
                    bool ret = MotionDev.GetCommandPos(ref out_pos);
                    if (ret)
                    {
                        Output_Data.Add("Home_GetCommandPos", $" {out_pos}");
                        if (out_pos != Pos)
                        {
                            LogMessage($"Home GetCommandPos is not {Pos}", MessageLevel.Warn);
                            return false;
                        }                          
                    }
                    else
                    {
                        Output_Data.Add("Home_GetCommandPos", $" Fail");
                        outputjsonStr = JsonConvert.SerializeObject(Output_Data, Formatting.Indented);
                        LogMessage($"Home GetCommandPos Fail", MessageLevel.Warn);
                        return false;
                    }
                }
                catch (Exception e1)
                {
                    LogMessage($"Please check driver is install or not:{e1.Message}", MessageLevel.Error);
                    return false;
                }
            }
            else if(Mode == "SetCommandPos")
            {
                try
                {                   
                    bool ret = MotionDev.SetCommandPos(Pos);
                    if(ret)
                    {
                        Output_Data.Add("Home_SetCommandPos", "Successed");
                        LogMessage($"Home SetCommandPos {Pos}", MessageLevel.Info);
                    }else
                    {
                        Output_Data.Add("Home_SetCommandPos", $"{Pos} Fail");
                        outputjsonStr = JsonConvert.SerializeObject(Output_Data, Formatting.Indented);
                        LogMessage($"Home SetCommandPos {Pos} Fail", MessageLevel.Warn);
                        return false;
                    }
                    
                }
                catch (Exception e1)
                {
                    LogMessage($"Please check driver is install or not:{e1.Message}", MessageLevel.Error);
                    return false;
                }
            }
            else if(Mode == "Home")
            {
                try
                {
                    //string mode = "ResetHome";
                    //var list = JsonConvert.DeserializeObject<List<ResetHome_check>>(json);
                Dictionary<int, int> ResetHome = new Dictionary<int, int>();
                ResetHome = ConvertJsonToDictionary();
                IO_Status_Result = MotionDev.Check_IO_StartStatus(ResetHome);
                if (IO_Status_Result)
                {
                  Logger.Info("move Jig in Save Status already");

                    MotionDev.Relative_Move(100, 10, 300, 0.3, 0.3);

                    chkhome_ret = MotionDev.SyncHome(10, 80, 0, HomeTimeout);
                    chkhome_ret &= MotionDev.Check_IO_StartStatus(ResetHome);

                    if (chkhome_ret)
                    {
                        //MessageBox.Show("Check Home Status:" + chkhome_ret.ToString() +" Reset to Home Ready","Check Home", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LogMessage($"Check Home Status: Reset to Home Ready", MessageLevel.Info);
                        home_ret_0 = MotionDev.SetCommandPos(0);
                            if (home_ret_0)
                            {
                                Output_Data.Add("Home_Status", "Successed");                              
                                LogMessage($"SetCommandPos(0): Successed", MessageLevel.Info);
                            }
                            else
                            {
                                LogMessage($"SetCommandPos(0): Fail", MessageLevel.Warn);
                            }
                    }
                    else
                    {
                       MessageBox.Show($"Reset to Home Fail: Please make sure \"the handle\" and  \"left and right fixing blocks\" is correct", "Check Home", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Home Status Error: Please Check ResetHome IO Status", "Jig Status", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                
                if (chkhome_ret == false && home_ret_0 == false)
                {
                        Output_Data.Add("Home_Status", "Fail");
                        outputjsonStr = JsonConvert.SerializeObject(Output_Data, Formatting.Indented);
                        return false;
                }
                }
                catch (Exception e1)
                {
                    LogMessage($"Please check driver is install or not:{e1.Message}", MessageLevel.Error);
                    return false;
                }
            }

            outputjsonStr = JsonConvert.SerializeObject(Output_Data, Formatting.Indented);
            jsonStr = outputjsonStr;

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

        public class TorqueMode : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> TorqueModeKeys = new List<string>();

                TorqueModeKeys.Add("Home");
                TorqueModeKeys.Add("SetCommandPos");
                TorqueModeKeys.Add("GetCommandPos");

                return new StandardValuesCollection(TorqueModeKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

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

        public Dictionary<int, int> ConvertJsonToDictionary()
        {
            var list = JsonConvert.DeserializeObject<List<KeyValue>>(MotorDio_Param);
            var dictionary = new Dictionary<int, int>();

            foreach (var item in list)
            {
                string name = item.KeyName;
                int key = int.Parse(item.DI.ToString());
                int value = int.Parse(item.Status.ToString());
                dictionary[key] = value;
                AddKeyName[name] = dictionary;
            }
            DictionaryLog();

            return dictionary;
        }

        public void DictionaryLog()
        {
            // 將 AddKeyName 中的資料轉換並存入 Data 中
            foreach (var kvp in AddKeyName)
            {
                string key = kvp.Key;
                Dictionary<int, int> valueDictionary = kvp.Value;

                // 將 dictionary 轉換為字串表示
                string valueString = string.Join(", ", valueDictionary.Select(kv =>$"[{kv.Key}, {kv.Value}]"));

                // 將轉換後的 key 和 value 存入 Data 中
                Output_Data[key] = valueString;
            }
        }

    }
}
