using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;


namespace AutoTestSystem.Script
{
    internal class Script_IQgig_Check : Script_Extra_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        string errorCode = string.Empty;


        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess(string ActionItem, string Paraminput)
        {

            strActItem = ActionItem;
            strParam = Paraminput;

            return true;
        }
        
        public override bool Process()
        {
            // Read all lines from the file
            IEnumerable<string> lines = File.ReadAllLines(@"./Utility/IQgig_UWB_check/Log/logOutput.txt");

            // Check each line for the presence of "TX_VERIFY"
            var txLines = lines.Select((line, index) => new { Line = Regex.Replace(line, @"[\t]", ""), Index = index + 1 })
                               .Where(rxItem => rxItem.Line.Contains("TX_VERIFY"));

            // Check each line for the presence of "1LE, Rx"
            var rxLines = lines.Select((line, index) => new { Line = Regex.Replace(line, @"[\t]", ""), Index = index + 1 })
                               .Where(txItem => txItem.Line.Contains("RX_VERIFY"));
            var titleList = new List<Dictionary<string, object>>();

            foreach (var item in txLines)
            {
                
                int index = item.Index - 1;
                string titleName1 = string.Empty;
                string titleName2 = string.Empty;
                string titleChannel = string.Empty;
                string pattern = string.Empty;
                string mes_name = string.Empty;
                string carrierData = string.Empty;
                string power = string.Empty;
                string symbol = string.Empty;
                while (true)
                {
                    if(lines.ElementAt(index).Contains("ERROR_MESSAGE") 
                    || lines.ElementAt(index).Contains("Skipped") 
                    || lines.ElementAt(index).Contains("Test Function Time") 
                    || item.Line.Contains("Skipped")== true)
                        break;
                    
                    if (lines.ElementAt(index).Contains("CARRIER_FREQ_OFFSET_PPM") == true)
                    {
                        Logger.Info($" item name  {item.Line} ");
                        pattern = @"\b.TX_VERIFY\s+(\S+)\s+(\S+)\s+(\S+)\b";
                        Match match = Regex.Match(item.Line, pattern);
                        if (match.Success)
                        {
                            titleName1 = match.Groups[1].Value;
                            titleChannel = match.Groups[2].Value;
                            titleName2 = match.Groups[3].Value;
                        }
                        mes_name = $"TX_VERIFY_{titleName1}_{titleChannel}_{titleName1}_CARRIER";

                        pattern = @"CARRIER_FREQ_OFFSET_PPM\s*:\s+(\S+)";
                        match = Regex.Match(lines.ElementAt(index), pattern);
                        if (match.Success){
                            carrierData = match.Groups[1].Value;
                            Logger.Info($" carrierData  {carrierData} ");
                            if (lines.ElementAt(index).Contains("[Failed]") == true)
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", carrierData, "FAIL"));
                            else
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", carrierData, "PASS"));
                        }
                    }
                    
                    if (lines.ElementAt(index).Contains("SPECTRUM_PEAK_AVERAGE_POWER") == true)
                    {
                        pattern = @"\b.TX_VERIFY\s+(\S+)\s+(\S+)\s+(\S+)\b";
                        Match match = Regex.Match(item.Line, pattern);
                        if (match.Success)
                        {
                            titleName1 = match.Groups[1].Value;
                            titleChannel = match.Groups[2].Value;
                            titleName2 = match.Groups[3].Value;
                        }
                        mes_name = $"TX_VERIFY_{titleName1}_{titleChannel}_{titleName1}_POWER";
                        pattern = @"SPECTRUM_PEAK_AVERAGE_POWER\s*:\s+(\S+)";
                        match = Regex.Match(lines.ElementAt(index), pattern);
                        if (match.Success){
                            power = match.Groups[1].Value;
                            Logger.Info($" power  {power} ");
                            if (lines.ElementAt(index).Contains("[Failed]") == true)
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", power, "FAIL"));
                            else
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", power, "PASS"));
                        }
                    }
                    
                    if (lines.ElementAt(index).Contains("SYMBOL_MODULATION_ACCURACY") == true)
                    {
                        pattern = @"\b.TX_VERIFY\s+(\S+)\s+(\S+)\s+(\S+)\b";
                        Match match = Regex.Match(item.Line, pattern);
                        if (match.Success)
                        {
                            titleName1 = match.Groups[1].Value;
                            titleChannel = match.Groups[2].Value;
                            titleName2 = match.Groups[3].Value;
                        }
                        mes_name = $"TX_VERIFY_{titleName1}_{titleChannel}_{titleName1}_SYMBOL";
                        pattern = @"SYMBOL_MODULATION_ACCURACY\s*:\s+(\S+)";
                        match = Regex.Match(lines.ElementAt(index), pattern);
                        if (match.Success){
                            symbol = match.Groups[1].Value;
                            Logger.Info($" symbol  {symbol} ");
                            if (lines.ElementAt(index).Contains("[Failed]") == true)
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", symbol, "FAIL"));
                            else
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", symbol, "PASS"));
                        }
                    }
                    index++;
                }
                if (item.Line.Contains("Skipped") 
                || item.Line.Contains(":")== true)
                    continue;
                    
                var data = new Dictionary<string, object>
                {
                            {"CARRIER_FREQ_OFFSET_PPM", carrierData},
                            {"SPECTRUM_PEAK_AVERAGE_POWER", power},
                            {"SYMBOL_MODULATION_ACCURACY", symbol}
                };
                        
                data = new Dictionary<string, object>
                {
                            {"title", item.Line},
                            {"data", data}
                };
                titleList.Add(data);

            }

            foreach (var item in rxLines)
            {
                
                
                int index = item.Index - 1;
                string titleName1 = string.Empty;
                string titleName2 = string.Empty;
                string titleChannel = string.Empty;
                string pattern = string.Empty;
                string mes_name = string.Empty;
                string perData = string.Empty;
                while (true)
                {
                   if(lines.ElementAt(index).Contains("ERROR_MESSAGE") 
                    || lines.ElementAt(index).Contains("Skipped") 
                    || lines.ElementAt(index).Contains("Test Function Time") 
                    || item.Line.Contains("Skipped")
                    || item.Line.Contains(":")== true)
                        break;


                    if (lines.ElementAt(index).Contains("PER") == true)
                    {
                        //RX_VERIFY  6P81 CH_5 62P40 SP_0_127 64 AUTO ANT_ID_0 PAIR
                        Logger.Info($" item name  {item.Line} ");
                        pattern = @"\b.RX_VERIFY\s+(\S+)\s+(\S+)\s+(\S+)\b";
                        Match match = Regex.Match(item.Line, pattern);
                        if (match.Success)
                        {
                            titleName1 = match.Groups[1].Value;
                            titleChannel = match.Groups[2].Value;
                            titleName2 = match.Groups[3].Value;
                        }
                        mes_name = $"RX_VERIFY_{titleName1}_{titleChannel}_{titleName1}";
                        //PER                                               :         0.00 %      (, 1)
                        pattern = @"PER\s*:\s+(\S+)";
                        match = Regex.Match(lines.ElementAt(index), pattern);
                        if (match.Success){
                            perData = match.Groups[1].Value;
                            Logger.Info($" perData  {perData} ");
                        }
                        
                        var data = new Dictionary<string, object>
                        {
                            {"item", "PER"},
                            {"value", perData},
                        };
                        
                        data = new Dictionary<string, object>
                        {
                            {"title", item.Line},
                            {"data", data}
                        };
                        titleList.Add(data);

                        if (lines.ElementAt(index).Contains("[Failed]") == true)
                            PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", perData, "FAIL"));
                        else
                            PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", perData, "PASS"));

                        break;
                    }   
                    index++;
                }
            }

            if (lines.Any(line => line.Contains("*  P A S S  *")))
                errorCode = "0";
            else
                errorCode = "-1";

            var IQgigData = new Dictionary<string, object>
            {
                {"errorCode", errorCode},
                {"UWBData", titleList}
            };
            strOutData = JsonConvert.SerializeObject(IQgigData);

            return true;
        }
        
        public override bool PostProcess()
        {
            
            if (!Directory.Exists(@"./IQgig_UWB")) { Directory.CreateDirectory(@"./IQgig_UWB"); }
            File.WriteAllText($@"./IQgig_UWB/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}.json", strOutData);
            File.Move(@"./Utility/IQgig_UWB_check/Log/logOutput.txt", 
                     $@"./Utility/IQgig_UWB_check/Log/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}.txt");
            
            if (errorCode == "0")
                return true;
            else
                return false;

        }

    }
}
