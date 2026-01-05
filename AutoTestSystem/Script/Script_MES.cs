using AutoTestSystem.Base;
using AutoTestSystem.Equipment.DosBase;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;


namespace AutoTestSystem.Script
{
    internal class Script_MES : Script_ControlDevice_Base
    {
        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("Select Parse Case"), TypeConverter(typeof(MESCommandList))]
        public string MES_Command { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess(string Actionitem, string strParamInput)
        {
            //設定要執行的ITEM及初始化一些參數
            //strActItem = Actionitem;
            //strParam = strParamInput;
            //mesCmd = JsonConvert.DeserializeObject<MESCmd>(strParam);
            return true;
        }
        
        public override bool Process(ControlDeviceBase Cmd)
        {
            bool pass_fail = true;
            //string ack_data = "";
            //string end_data = "";
            //string send_data = "";
            //string Popmes_data = "";
            //string Failitem = PopMoreData("Failitem");

            //Cmd.SetCheckstr(mesCmd.CheckStr);
            //Cmd.SetMEStcmd(mesCmd.Cmd);
            
            //if (string.Equals(mesCmd.Cmd, "C003") == true)
            //{
            //    if (Failitem == string.Empty)
            //    {
            //        Cmd.SetMEStdata($"{mesCmd.Data}OK");
            //    }
            //    else
            //    {
            //        Cmd.SetMEStdata($"{mesCmd.Data}NG;{Failitem}");
            //    }
            //}
            //else if(string.Equals(mesCmd.Cmd, "C004") == true)
            //{
            //    /*string testmes2 = PopOneMESData(mesCmd.Data);
            //    Cmd.SetMEStdata($"{mesCmd.Data}{testmes2}");*/

            //    Popmes_data = PopALLMESData();             
            //    Cmd.SetMEStdata($"{mesCmd.Data}{Popmes_data}");                
            //}
            //else
            //{
            //    Cmd.SetMEStdata(mesCmd.Data);
            //}

            //Logger.Info($"Script_HTTPMESCmd_Related Send: {mesCmd.Cmd}\n");
            //if (Cmd.Send("", strActItem) != true)
            //{
            //    return false;
            //}
            
            //Cmd.READ(ref end_data);
            //strOutData = end_data;
            //Logger.Info($"Script_HTTPMESCmd_Related Read END: {end_data}");
            return pass_fail;

        }

        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            //strDataout =  strOutData;
            //string result = CheckRule(strOutData, strCheckSpec);

            //if (string.Equals(mesCmd.Cmd, "C003") == true)
            //{
            //    PushMoreData("Failitem", string.Empty);
            //}

            //if (result == "PASS" || strCheckSpec == "")
            //    return true;
            //else
                return false;

        }

        public class MESCmd
        {
            public string Cmd { get; set; }
            public string Data { get; set; }
            public string CheckStr { get; set;}
        }

        public enum MESCommand
        {
            C001,     //   檢查員工編號是否正確, 結果是OK表示檢查成功,
            C002,     //   檢查序號以及檢驗流程, 結果前兩碼是OK表示檢查成功,
            C003,     //   用來記錄當站的測試結果, 結果只能是OK或是NG, 檢驗結果如果是NG, 需要附帶不良代碼。多個不良代碼須以’@’隔開,有不良位置需以’:’隔開
            C004      //   用來記錄非量測值但是希望記錄各項的檢驗結果, 各項檢驗的結果內容不限定是數字
        }

        public class MESCommandList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                // Get enum values array
                Array enumValues = Enum.GetValues(typeof(MESCommand));
                List<string> enumList = new List<string>(enumValues.Length);

                if (enumValues.Length > 0)
                {
                    foreach (var enumValue in enumValues)
                    {
                        enumList.Add(enumValue.ToString());
                    }

                    return new StandardValuesCollection(enumList);
                }
                else
                {
                    return new StandardValuesCollection(new string[] { });
                }
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

        }
    }
}
