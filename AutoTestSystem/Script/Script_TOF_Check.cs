
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

namespace AutoTestSystem.Script
{

    internal class Script_TOF_Check : Script_Extra_Base
    {
        string strActItem = string.Empty;
        string strParam = string.Empty;
        JArray TOF_data;

        TOF TOF_param = new TOF();
        Distance distance = null;

        public enum Status
        {
            data_not_updated = 0,                //   0 -> Ranging data are not updated
            signal_rate_too_low_on_SPAD = 1,     //   1 -> Signal rate too low on SPAD array
            target_phase = 2,                    //   2 -> Target phase
            sigma_estimator_too_high = 3,        //   3 -> Sigma estimator too high
            target_consistency_failed = 4,       //   4 -> Target consistency failed
            range_valid = 5,                     //   5 -> Range valid
            wrap_around_not_performed = 6,       //   6 -> Wrap around not performed (Typically the first range)
            rate_consistency_failed = 7,         //   7 -> Rate consistency failed
            signal_rate_too_low_for_current = 8, //   8 -> Signal rate too low for the current target
            range_valid_with_large_pulse = 9,    //   9 -> Range valid with large pulse (may be due to a merged target)
            range_valid_but_no_target = 10,      //  10 -> Range valid, but no target detected at previous range
            measurement_consistency_failed = 11, //  11 -> Measurement consistency failed
            target_blurred = 12,                 //  12 -> Target blurred by another one, due to sharpener
            target_inconsistent_data = 13,       //  11 -> Target detected but inconsistent data. Frequently happens for secondary targets.
            no_target_detected = 255             // 255 -> No target detected (only if number of target detected is enabled)
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess(string ActionItem, string Paraminput)
        {
            strActItem = ActionItem;
            strParam = Paraminput;
            distance = JsonConvert.DeserializeObject<Distance>(strParam);
            TOF_data = JArray.Parse(PopMoreData("TOF_data"));

            return true;
        }
        
        public override bool Process()
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

            return true;
        }

        public override bool PostProcess()
        {
            PushMoreData("TOF_data", string.Empty);
            string result = ProcessData();           

            if (result == "PASS")
                return true;
            else
                return false;

        }

        public string ProcessData()
        {
            string pass_fail = string.Empty;
            string meanResult = TOF_param.Status.Average().ToString();
            string meanDistance = TOF_param.Distance.Average().ToString();
            if (PopMoreData("TOF_Calib") == "Done")
            {
                if (TOF_param.Status.All(x => x >= (int)Status.target_consistency_failed) &
                TOF_param.Status.All(x => x <= (int)Status.range_valid_with_large_pulse))
                {
                    PushMESData("meanResult", Tuple.Create("meanResult", meanResult, "PASS"));
                    
                    if (TOF_param.Distance.All(x => x >= distance.MinLimit) &
                        TOF_param.Distance.All(x => x <= distance.MaxLimit))
                    {
                        PushMESData("meanDistance", Tuple.Create("meanDistance", meanDistance, "PASS"));
                        pass_fail = "PASS";
                    }
                    else
                    {
                        PushMESData("meanDistance", Tuple.Create("meanDistance", meanDistance, "FAIL"));
                        LogMessage($"At least one zone's \"Distance\" beyond the allowed range  ({distance.MinLimit} to {distance.MaxLimit}) .Result->Fail", MessageLevel.Warn);
                    }
                }
                else
                {
                    PushMESData("meanResult", Tuple.Create("meanResult", meanResult, "FAIL"));
                    LogMessage($"At least one zone's \"result\" beyond the allowed range  ({(int)Status.target_consistency_failed} to {(int)Status.range_valid_with_large_pulse}) .Result->Fail", MessageLevel.Warn);
                    
                    if (TOF_param.Distance.All(x => x >= distance.MinLimit) &
                        TOF_param.Distance.All(x => x <= distance.MaxLimit))
                    {
                        PushMESData("meanDistance", Tuple.Create("meanDistance", meanDistance, "PASS"));
                    }
                    else
                    {
                        PushMESData("meanDistance", Tuple.Create("meanDistance", meanDistance, "FAIL"));
                        LogMessage($"At least one zone's \"Distance\" beyond the allowed range  ({distance.MinLimit} to {distance.MaxLimit}) .Result->Fail", MessageLevel.Warn);
                    }
                }
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

                string meanDistance32 = Distance32.Average().ToString();
                if (Distance32.All(x => x >= distance.MinLimit) &
                    Distance32.All(x => x <= distance.MaxLimit))
                {
                    PushMESData("meanDistance32_pre", Tuple.Create("meanDistance32_pre", meanDistance32, "PASS"));
                }
                else
                {
                    PushMESData("meanDistance32_pre", Tuple.Create("meanDistance32_pre", meanDistance32, "FAIL"));
                }

                if (TOF_param.Distance.All(x => x >= distance.MinLimit) &
                    TOF_param.Distance.All(x => x <= distance.MaxLimit))
                {
                    PushMESData("meanDistance64_pre", Tuple.Create("meanDistance64_pre", meanDistance, "PASS"));
                }
                else
                {
                    PushMESData("meanDistance64_pre", Tuple.Create("meanDistance64_pre", meanDistance, "FAIL"));
                }

                pass_fail = "PASS";
            }

            return pass_fail;
        }

        public class TOF
        {
            public int[] Zone { get; set; }
            public int[] Status { get; set; }
            public int[] Distance { get; set; }
        }

        public class Distance
        {
            public int MaxLimit { get; set; }
            public int MinLimit { get; set; }
        }

    }
}
