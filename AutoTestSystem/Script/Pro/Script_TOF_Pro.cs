
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;
using System.Windows.Forms;
using System.ComponentModel;

namespace AutoTestSystem.Script
{

    internal class Script_TOF_Pro : Script_Extra_Base
    {
        string strOutData = string.Empty;

        [Category("Distance Parameters"), Description("自訂顯示名稱")]
        public int MaxLimit { get; set; } = 665;
        
        [Category("Distance Parameters"), Description("自訂顯示名稱")]
        public int MinLimit { get; set; } = 545;

        [Category("Result Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(StatusList))]
        public string UpperLimit { get; set; } = "9 - Range valid with large pulse";

        [Category("Result Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(StatusList))]
        public string LowerLimit { get; set; } = "4 - Target consistency failed";

        JArray TOF_data;
        TOF TOF_param = new TOF();

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess()
        {
            if (PopMoreData("TOF_data") == null || PopMoreData("TOF_data") == string.Empty)
            {
                LogMessage("TOF_data can not be null.", MessageLevel.Error);
                return false;
            }
            else
                TOF_data = JArray.Parse(PopMoreData("TOF_data"));

            return true;
        }
        
        public override bool Process(ref string output)
        {
            TOF_param.Zone = new int[TOF_data.Count];
            TOF_param.Status = new int[TOF_data.Count];
            TOF_param.Distance = new int[TOF_data.Count];

            for (int i = 0; i < TOF_data.Count; i++)
            {
                TOF_param.Zone[i] = (int)TOF_data[i]["zone"];
                TOF_param.Status[i] = (int)TOF_data[i]["result"];
                TOF_param.Distance[i] = (int)TOF_data[i]["Distance"];
            }          
            
            output = ProcessData();
            strOutData = output;

            return true;
        }

        public override bool PostProcess()
        {
            PushMoreData("TOF_data", string.Empty);
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
            {
                return true;
            }
            else
            {
                LogMessage($"{result}", MessageLevel.Error);
                return false;
            }

        }

        
        public string ProcessData()
        {
            var data = new Dictionary<string, object> { };
            int upperLimit = int.Parse(Regex.Split(UpperLimit, " - ")[0]);
            int lowerLimit = int.Parse(Regex.Split(LowerLimit, " - ")[0]);
            string meanResult = TOF_param.Status.Average().ToString();
            string meanDistance = TOF_param.Distance.Average().ToString();

            if (PopMoreData("TOF_Calib") == "Done")
            {
                if (TOF_param.Status.All(x => x >= lowerLimit) &&
                    TOF_param.Status.All(x => x <= upperLimit))
                {
                    if (TOF_param.Distance.All(x => x >= MinLimit) &&
                        TOF_param.Distance.All(x => x <= MaxLimit))
                    {
                        data.Add("errorCode", "0");
                    }
                    else
                    {
                        data.Add("errorCode", "-1");
                        LogMessage($"At least one zone's \"Distance\" beyond the allowed range ({MinLimit} to {MaxLimit}) .Result->Fail", MessageLevel.Warn);
                    }
                }
                else
                {
                    LogMessage($"At least one zone's \"result\" beyond the allowed range ({lowerLimit} to {upperLimit}) .Result->Fail", MessageLevel.Warn);

                    if (TOF_param.Distance.All(x => x >= MinLimit) &&
                        TOF_param.Distance.All(x => x <= MaxLimit))
                    {
                        data.Add("errorCode", "-1");
                    }
                    else
                    {
                        data.Add("errorCode", "-1");
                        LogMessage($"At least one zone's \"Distance\" beyond the allowed range ({MinLimit} to {MaxLimit}) .Result->Fail", MessageLevel.Warn);
                    }
                }

                data.Add("meanResult", meanResult);
                data.Add("meanDistance", meanDistance);
            }
            else
            {
                List<int> Distance32 = new List<int>();
                foreach (int zone in TOF_param.Zone)
                {
                    if (zone % 8 >= 4)
                    {
                        Distance32.Add(TOF_param.Distance[zone]);
                    }
                }

                data.Add("errorCode", "0");
                data.Add("meanDistance32_pre", Distance32.Average().ToString());
                data.Add("meanDistance64_pre", meanDistance);
            }

            string output = JsonConvert.SerializeObject(data);
            LogMessage($"Read END:  {output}");

            return output;

        }

        public class TOF
        {
            public int[] Zone { get; set; }
            public int[] Status { get; set; }
            public int[] Distance { get; set; }
        }

    }

    public class StatusList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> typeList = new List<string>() { "0 - Ranging data not Updated",
                                                         "1 - Signal rate too low on SPAD array",
                                                         "2 - Target phase",
                                                         "3 - Sigma estimator too high",
                                                         "4 - Target consistency failed",
                                                         "5 - Range valid",
                                                         "6 - Wrap around not performed",
                                                         "7 - Rate consistency failed",
                                                         "8 - Signal rate too low for current target",
                                                         "9 - Range valid with large pulse",
                                                         "10 - Range valid but no target detected",
                                                         "11 - Measurement consistency failed",
                                                         "12 - Target blurred by another one",
                                                         "13 - Target detected but inconsistent data",
                                                         "255 - No target detected" };
            
            return new StandardValuesCollection(typeList);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
