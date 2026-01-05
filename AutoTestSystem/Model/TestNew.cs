/*
 * "AutoTestSystem.Model --> TestNew"
 *
 * Corpright William & Zhibin
 *
 *.##.......####.########.########..#######..##....##
 *.##........##.....##....##.......##.....##.###...##
 *.##........##.....##....##.......##.....##.####..##
 *.##........##.....##....######...##.....##.##.##.##
 *.##........##.....##....##.......##.....##.##..####
 *.##........##.....##....##.......##.....##.##...###
 *.########.####....##....########..#######..##....##
 *
 *  1. <TestNew.cs> is a use for run script
 *  2. It can support thread task at the same time
 *  3. EntryPoint: <MainForm.cs> theme_8 --> TestNew.cs 
 *
 * 
 */

/*****************************************************************************
*                    Includes Definitions
*****************************************************************************/
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.CCD;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Equipment.DosBase;
//using AutoTestSystem.Equipment.VISA;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Script;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.MainForm;
using static System.String;


/*****************************************************************************
*                    Function code
*****************************************************************************/
namespace AutoTestSystem.Model
{
    public class TestNew
    {
        public List<string> StepTest(ItemsNew iteminfo, Dictionary<string, object> DictDevices, int timeOut)
        {

            List<string> tResult = new List<string>();
            tResult.Clear();
            string strDataOut = string.Empty;
            try
            {
                //====================Add Call By BaseType(by WilliamCWLin)====================
                Assembly assembly = Assembly.GetExecutingAssembly();
                Type[] types = assembly.GetTypes();
                string parentClassName = string.Empty;
                foreach (Type type in types)
                {
                    if (type.Name.Equals(iteminfo.StriptType))
                    {
                        parentClassName = type.BaseType.Name;
                        break;
                    }
                }                
                string tmp_DataOut = string.Empty;
                bool preProcessResult = false;
                bool ProcessResult = false;
                bool result = false;

                if(iteminfo.Prefix != string.Empty)
                    GlobalNew.g_datacollection.SetMoreProp("PrefixName", $"{iteminfo.Prefix}_{iteminfo.ItemName}");
                else
                    GlobalNew.g_datacollection.SetMoreProp("PrefixName", iteminfo.ItemName);

                switch (parentClassName)
                {
                    case "ScriptDUTBase":
                        //====================Add Call By BaseType(by WilliamCWLin)====================
                        DUT_BASE dut_device = (DUT_BASE)DictDevices[iteminfo.DeviceName];
                        ScriptDUTBase _obj = (ScriptDUTBase)System.Activator.CreateInstance(Assembly.GetAssembly(this.GetType()).FullName, "AutoTestSystem.Script." + iteminfo.StriptType).Unwrap();
                        preProcessResult = _obj.PreProcess(iteminfo.TestKeyword, iteminfo.ComdSend);

                        if (preProcessResult)
                        {
                            ProcessResult = _obj.Process(dut_device);

                            if (ProcessResult)

                            {
                                result = _obj.PostProcess(iteminfo.SpecRule, ref strDataOut);
                            }
                        }
                        if (result)
                        {
                            tResult.Add("PASS");
                        }
                        else
                        {
                            tResult.Add("FAIL");
                        }

                        tResult.Add(strDataOut);

                        break;

                    case "ScriptIOBase":
                        //====================Add Call By BaseType(by WilliamCWLin)====================
                        if(!DictDevices.ContainsKey(iteminfo.DeviceName))
                        {
                            tResult.Add("FAIL");
                            tResult.Add("Can not Detect IO Device");
                            break;
                        }
                            
                        IOBase IO_DEV = (IOBase)DictDevices[iteminfo.DeviceName];
                        ScriptIOBase _IOBase_obj = (ScriptIOBase)System.Activator.CreateInstance(Assembly.GetAssembly(this.GetType()).FullName, "AutoTestSystem.Script." + iteminfo.StriptType).Unwrap();
                        preProcessResult = _IOBase_obj.PreProcess(iteminfo.TestKeyword, iteminfo.ComdSend);

                        if (preProcessResult)
                        {
                            ProcessResult = _IOBase_obj.Process(IO_DEV);
                            
                            if (ProcessResult)
                            {
                                result = _IOBase_obj.PostProcess(iteminfo.SpecRule, ref strDataOut);
                            }
                            else
                            {
                                Logger.Warn(iteminfo.StriptType +" Process Fail");
                            }
                        }
                        else
                        {
                            Logger.Warn(iteminfo.StriptType +" Preprocess Fail");
                        }

                        if (result)
                        {
                            tResult.Add("PASS");
                        }
                        else
                        {
                            tResult.Add("FAIL");
                        }

                        tResult.Add(strDataOut);

                        break;

                    case "ScriptVisaBase":
                        //====================Add Call By BaseType(by WilliamCWLin)====================
                        if(!DictDevices.ContainsKey(iteminfo.DeviceName))
                        {
                            tResult.Add("FAIL");
                            tResult.Add("Can not Detect IO Device");
                            break;
                        }
                            
                        VisaBase Visa_DEV = (VisaBase)DictDevices[iteminfo.DeviceName];
                        ScriptVisaBase _VisaBase_obj = (ScriptVisaBase)System.Activator.CreateInstance(Assembly.GetAssembly(this.GetType()).FullName, "AutoTestSystem.Script." + iteminfo.StriptType).Unwrap();
                        preProcessResult = _VisaBase_obj.PreProcess(iteminfo.TestKeyword, iteminfo.ComdSend);

                        if (preProcessResult)
                        {
                            ProcessResult = _VisaBase_obj.Process(Visa_DEV);
                            
                            if (ProcessResult)
                            {
                                result = _VisaBase_obj.PostProcess(iteminfo.SpecRule, ref strDataOut);
                            }
                            else
                            {
                                Logger.Warn(iteminfo.StriptType +" Process Fail");
                            }
                        }
                        else
                        {
                            Logger.Warn(iteminfo.StriptType +" Preprocess Fail");
                        }

                        if (result)
                        {
                            tResult.Add("PASS");
                        }
                        else
                        {
                            tResult.Add("FAIL");
                        }

                        tResult.Add(strDataOut);

                        break;

                    case "Script_ControlDevice_Base":
                        //====================Add Call By BaseType(by WilliamCWLin)====================
                        if (!DictDevices.ContainsKey(iteminfo.DeviceName))
                        {
                            tResult.Add("FAIL");
                            tResult.Add("Can not Detect ControlDevice");
                            break;
                        }

                        ControlDeviceBase CTRL_DEV = (ControlDeviceBase)DictDevices[iteminfo.DeviceName];
                        Script_ControlDevice_Base _CTRL_Script_obj = (Script_ControlDevice_Base)System.Activator.CreateInstance(Assembly.GetAssembly(this.GetType()).FullName, "AutoTestSystem.Script." + iteminfo.StriptType).Unwrap();
                        preProcessResult = _CTRL_Script_obj.PreProcess(iteminfo.TestKeyword, iteminfo.ComdSend);

                        if (preProcessResult)
                        {
                            ProcessResult = _CTRL_Script_obj.Process(CTRL_DEV);

                            if (ProcessResult)
                            {
                                result = _CTRL_Script_obj.PostProcess(iteminfo.SpecRule, ref strDataOut);
                            }
                            else
                            {
                                Logger.Warn(iteminfo.StriptType + " Process Fail");
                            }
                        }
                        else
                        {
                            Logger.Warn(iteminfo.StriptType + " Preprocess Fail");
                        }

                        if (result)
                        {
                            tResult.Add("PASS");
                        }
                        else
                        {
                            tResult.Add("FAIL");
                        }

                        tResult.Add(strDataOut);

                        break;
                    case "Script_CCD_Base":

                        if (!DictDevices.ContainsKey(iteminfo.DeviceName))
                        {
                            tResult.Add("FAIL");
                            tResult.Add("Check HW Device in Script");
                            break;
                        }


                        CCDBase ccd_device = (CCDBase)DictDevices[iteminfo.DeviceName];
                        Script_CCD_Base _CCD_Script_obj = (Script_CCD_Base)System.Activator.CreateInstance(Assembly.GetAssembly(this.GetType()).FullName, "AutoTestSystem.Script." + iteminfo.StriptType).Unwrap();
                        preProcessResult = _CCD_Script_obj.PreProcess(iteminfo.TestKeyword, iteminfo.ComdSend);

                        if (preProcessResult)
                        {
                            ProcessResult = _CCD_Script_obj.Process(ccd_device);

                            if (ProcessResult)
                            {
                                result = _CCD_Script_obj.PostProcess(iteminfo.SpecRule, ref strDataOut);
                            }
                            else
                            {
                                Logger.Warn(iteminfo.StriptType + " Process Fail");
                            }
                        }
                        else
                        {
                            Logger.Warn(iteminfo.StriptType + " Preprocess Fail");
                        }

                        if (result)
                        {
                            tResult.Add("PASS");
                        }
                        else
                        {
                            tResult.Add("FAIL");
                        }

                        tResult.Add(strDataOut);

                        break;

                    case "Script_1Mot1ComBase":

                        //if (!DictDevices.ContainsKey(iteminfo.DeviceName))
                        //{
                        //    tResult.Add("FAIL");
                        //    tResult.Add("未偵測到設定的1Mot1Com裝置");
                        //    break;
                        //}

                        string[] Device_com = iteminfo.ComdSend.Split(',');
                        MotionBase Motion_Device;
                        ControlDeviceBase Comport_Device;
                        try
                        {
                            Motion_Device = (MotionBase)DictDevices[iteminfo.DeviceName];
                            Comport_Device = (ControlDeviceBase)DictDevices[Device_com[0]];
                        }catch(Exception ex)
                        {                           
                            tResult.Add("FAIL");
                            tResult.Add($"未偵測到設定的1Mot1Com裝置{iteminfo.DeviceName} {Device_com[0]} {ex.Message}");
                            break;
                        }
                        Script_1Mot1ComBase Motion_Device_Script_obj = (Script_1Mot1ComBase)System.Activator.CreateInstance(Assembly.GetAssembly(this.GetType()).FullName, "AutoTestSystem.Script." + iteminfo.StriptType).Unwrap();
                        preProcessResult = Motion_Device_Script_obj.PreProcess(iteminfo.ComdSend);

                        if (preProcessResult)
                        {
                            //Communication comport = null;
                            ProcessResult = Motion_Device_Script_obj.Process(Comport_Device, Motion_Device);

                            if (ProcessResult)
                            {
                                result = Motion_Device_Script_obj.PostProcess(iteminfo.TestKeyword, iteminfo.SpecRule, ref strDataOut);
                            }
                            else
                            {
                                Logger.Warn(iteminfo.StriptType + " Process Fail");
                            }
                        }
                        else
                        {
                            Logger.Warn(iteminfo.StriptType + " Preprocess Fail");
                        }

                        if (result)
                        {
                            tResult.Add("PASS");
                        }
                        else
                        {
                            tResult.Add("FAIL");                            
                        }

                        tResult.Add(strDataOut);

                        break;

                    case "Script_Extra_Base":
                        Script_Extra_Base _Extra_Script_obj = (Script_Extra_Base)System.Activator.CreateInstance(Assembly.GetAssembly(this.GetType()).FullName, "AutoTestSystem.Script." + iteminfo.StriptType).Unwrap();
                        preProcessResult = _Extra_Script_obj.PreProcess(iteminfo.TestKeyword, iteminfo.ComdSend);

                        if (preProcessResult)
                        {
                            ProcessResult = _Extra_Script_obj.Process();

                            if (ProcessResult)
                            {
                                result = _Extra_Script_obj.PostProcess();
                            }
                            else
                            {
                                Logger.Warn(iteminfo.StriptType + " Process Fail");
                            }
                        }
                        else
                        {
                            Logger.Warn(iteminfo.StriptType + " Preprocess Fail");
                        }                       

                        if (result)
                        {
                            tResult.Add("PASS");
                            tResult.Add("OK");
                        }
                        else
                        {
                            tResult.Add("FAIL");
                            tResult.Add("Error");                                                                         
                        }
                        

                        break;
                    case "Script_Tool_Base":
                        Script_Tool_Base _Tool_Script_obj = (Script_Tool_Base)System.Activator.CreateInstance(Assembly.GetAssembly(this.GetType()).FullName, "AutoTestSystem.Script." + iteminfo.StriptType).Unwrap();
                        preProcessResult = _Tool_Script_obj.PreProcess(iteminfo.TestKeyword, iteminfo.ComdSend);

                        if (preProcessResult)
                        {
                           if(iteminfo.Mode != string.Empty)
                                ProcessResult = _Tool_Script_obj.Process(iteminfo.TestKeyword,iteminfo.Mode);
                           else
                                ProcessResult = _Tool_Script_obj.Process(iteminfo.TestKeyword, "");

                            if (ProcessResult)
                            {
                                result = _Tool_Script_obj.PostProcess(iteminfo.SpecRule, ref strDataOut);
                            }
                            else
                            {
                                Logger.Warn(iteminfo.StriptType + " Process Fail");
                            }
                        }
                        else
                        {
                            Logger.Warn(iteminfo.StriptType + " Preprocess Fail");
                        }                       

                        if (result)
                        {
                            tResult.Add("PASS");
                        }
                        else
                        {
                            tResult.Add("FAIL");                            
                        }
                        tResult.Add(strDataOut);

                        break;

                    default:
                        tResult.Add("FAIL");
                        tResult.Add("設定錯誤，未知的腳本類別");
                        break;
                }
            }
            catch (Exception ex) when (ex.Message.Contains("正在中止线程"))
            {
                //abort线程忽略报错
                Logger.Warn(ex.Message);
                tResult.Clear();
                tResult.Add("FAIL");
                tResult.Add("");
                GlobalNew.g_datacollection.SetMoreProp($"{iteminfo.ItemName}", ex.Message);
                return tResult;
            }
            catch (Exception ex)
            {
                //Logger.Fatal("TestStep test Exception!!! return fail.");
                Logger.Fatal(ex.ToString());
                tResult.Clear();
                tResult.Add("FAIL");
                tResult.Add("");
                GlobalNew.g_datacollection.SetMoreProp($"{iteminfo.ItemName}", ex.Message);
                return tResult;
            }


            GlobalNew.g_datacollection.SetMoreProp($"{iteminfo.ItemName}", tResult[1]);

            return tResult;
        }

        public void Test()
        {
            Logger.Debug("sssssss");
        }

    }
}