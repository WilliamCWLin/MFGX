/*
 * "AutoTestSystem --> MainForm UI"
 *
 * Corpright Jordan
 *
 *.##.......####.########.########..#######..##....##
 *.##........##.....##....##.......##.....##.###...##
 *.##........##.....##....##.......##.....##.####..##
 *.##........##.....##....######...##.....##.##.##.##
 *.##........##.....##....##.......##.....##.##..####
 *.##........##.....##....##.......##.....##.##...###
 *.########.####....##....########..#######..##....##
 *
 *  1. <IQxel.cs> is RF instrument to calibrate BT & Wifi
 *  2. <IQxel.cs> override the <Visa.cs> to follow its standard functions.
 *  3. <IQxel.cs> use the <TCP_NI_Visa.cs> class to communicate device. 
 *  4. EntryPoint: <VisaBase.cs> entry point --> <IQxel.cs>
 * 
 */


/*****************************************************************************
*                    Includes Definitions
*****************************************************************************/
using Automation.BDaq;
using AutoTestSystem.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using AutoTestSystem.DAL;

/*****************************************************************************
*                    Function code
*****************************************************************************/
namespace AutoTestSystem.Equipment.VISA
{
    class IQxel : VisaBase
    {

        //////////////////////////////////////////////////////////////////////////////////////
        // 1. Visa Parameter
        //                                                        
        //////////////////////////////////////////////////////////////////////////////////////
 
        TCP_NI_VISA VISA_Class = null;
        private InstantAiCtrl instantAIContrl;
        private string _strParamInfoPath;


        //////////////////////////////////////////////////////////////////////////////////////
        // 2. IQxel Command (please refer web Document)
        //                                                        
        //////////////////////////////////////////////////////////////////////////////////////
        private string Get_Device  = "*IDN?\\n";

        //////////////////////////////////////////////////////////////////////////////////////
        // 3. User input parameter
        //                                                        
        //////////////////////////////////////////////////////////////////////////////////////
        [Category("Parameter"), Description("TCP IP")]
        public string VISA_IP
        {
            set; get;
        }

        [Category("Parameter"), Description("Port")]
        public string VISA_Port
        {
            set; get;
        }


        //////////////////////////////////////////////////////////////////////////////////////
        // 4. Common function
        //                                                        
        //////////////////////////////////////////////////////////////////////////////////////
        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            //////////////////////
            // 4-1. Check input //
            //////////////////////
            if (VISA_IP == "")
            {
                LogMessage($"VISA_IP Keyin Fail.");
                return false;
            }

            if (VISA_Port == "")
            {
                LogMessage($"VISA_Port Keyin Fail.");
                return false;
            }
           
            ///////////////////
            // 4-2. open TCP //
            ///////////////////
            try
            {
            
                VISA_Class = new TCP_NI_VISA();
                // default = "TCPIP0::192.168.1.10::24000::SOCKET"
                VISA_Class.TCP_ConInfo =  "TCPIP0::" + VISA_IP + "::" + VISA_Port + "::SOCKET";

                // wait feedback time (ms)
                VISA_Class.TCP_ConTimeOut = 3000;

                if (VISA_Class.Open()==false)
                {
                    LogMessage($"IQxel Open Fail. Please check NI Visa driver & IP");
                    return false;
                }

            }
            catch(Exception e)
            {
                LogMessage($"IQxel Open Fail.{e.Message}");
                return false;
            }

            ///////////////////
            // 4-3.write TCP //
            ///////////////////
            // VISA_Class.Write(Get_Device);

            ///////////////////
            // 4-4. open TCP //
            ///////////////////       

            string VISA_Feedback = VISA_Class.Query(Get_Device);                 
            if(VISA_Feedback!=""){
                LogMessage($"IQxel Init Success, Device = {VISA_Feedback}.");
                return true;
            }else{
                LogMessage($"IQxel Init Fail. Please check comport.");
                return false;
            }


        }

        public override bool Get_Device_Info(string strParamInfo){
            throw new System.NotImplementedException();
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 5. InstantAiCtrlDeviceList
        //    下拉式選單                                                    
        //////////////////////////////////////////////////////////////////////////////////////
        public class InstantAiCtrlDeviceList : TypeConverter  
        {
            public static readonly InstantAiCtrl AiCtrl = new InstantAiCtrl();
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if (AiCtrl.SupportedDevices.Count > 0)
                {
                    List<string> hwList = new List<string>();
                    foreach (DeviceTreeNode node in AiCtrl.SupportedDevices)
                    {
                        if (node.Description.Contains("Demo") == false)
                            hwList.Add(node.Description);
                    }
                    return new StandardValuesCollection(hwList.ToArray());
                }
                else
                {
                    return new StandardValuesCollection(new int[] { 0 });
                }
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
    }
}
