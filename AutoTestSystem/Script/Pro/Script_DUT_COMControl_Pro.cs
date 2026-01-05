using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using static AutoTestSystem.BLL.Bd;
using System.ComponentModel;
using AutoTestSystem.Model;
using AutoTestSystem.DAL;
using System.Data;

namespace AutoTestSystem.Script
{
    internal class Script_DUT_COMControl_Pro : ScriptDUTBase
    {
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(FunctionList))]
        public string Function { get; set; }

        DosCmd doscmd = new DosCmd();

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess()
        {
            if (Function == null || Function == string.Empty)
            {
                LogMessage("Function can not be null.", MessageLevel.Error);
                return false;
            }

            return true;
        }
        
        public override bool Process(DUT_BASE DUTDevice,ref string output)
        {
            var data = new Dictionary<string, object> { };

            switch (Function)
            {
                case "Open":
                    if (DUTDevice.OPEN() == true)
                        data.Add("errorCode", "0");
                    else
                        data.Add("errorCode", "-1");
                    break;

                case "Close":
                    if (DUTDevice.UnInit() == true)
                        data.Add("errorCode", "0");
                    else
                        data.Add("errorCode", "-1");
                    break;

                case "Clear Telit COM":
                    int Total_Num = GlobalNew.Total_Pass_Num + GlobalNew.Total_Fail_Num;
                    if (Total_Num != 0 && (Total_Num % 40) == 0)
                    {
                        string result = doscmd.SendCommand3("cd Remove_Telit_COM && bat_remove_Telit_COM.bat");
                        LogMessage($"{result}");
                        if (result.Contains("No") == true || result.Contains("All") == true)
                            data.Add("errorCode", "0");
                        else
                            data.Add("errorCode", "-1");
                        break;
                    }
                    LogMessage($"Waiting {40-(Total_Num % 40)} pcs finished ...");
                    data.Add("errorCode", "0");
                    break;
            }

            output = JsonConvert.SerializeObject(data);
            LogMessage($"{output}");
            strOutData = output;

            return true;
        }

        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
            {
                return true;
            }             
            else
            {
                LogMessage($"{result}",MessageLevel.Error);
                return false;
            }

        }

    }

    public class FunctionList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> typeList = new List<string>() { "Open", "Close", "Clear Telit COM" };

            return new StandardValuesCollection(typeList);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;

        }
    }
}
