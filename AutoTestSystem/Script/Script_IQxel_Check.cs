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
    internal class Script_IQxel_Check : Script_Extra_Base
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
            IEnumerable<string> lines = File.ReadAllLines(@"./Utility/IQexl_BT_check/Log/Log_Current.txt");

            // Check each line for the presence of "1LE, Tx"
            var txLines = lines.Select((line, index) => new { Line = Regex.Replace(line, @"[\t]", ""), Index = index + 1 })
                               .Where(rxItem => rxItem.Line.Contains("1LE, Tx"));

            // Check each line for the presence of "1LE, Rx"
            var rxLines = lines.Select((line, index) => new { Line = Regex.Replace(line, @"[\t]", ""), Index = index + 1 })
                               .Where(txItem => txItem.Line.Contains("1LE, Rx"));
            var titleList = new List<Dictionary<string, object>>();

            foreach (var item in txLines)
            {
                int index = item.Index - 1;
                string freq = string.Empty;
                string power = string.Empty;
                string value = string.Empty;
                string criteria = string.Empty;
                string result = string.Empty;
                string mes_name = string.Empty;
                string pattern = string.Empty;
                var itemList = new List<Dictionary<string, object>>();

                while (true)
                {
                    index++;
                    Logger.Info($"[Parser LOG] {lines.ElementAt(index)}");
                    if (lines.ElementAt(index).Contains("\tIni Freq Error") == true)
                    {
                        pattern = @"Frequency: (\d+).*Tx Power: (\d+\.\d+)";
                        Match match = Regex.Match(item.Line, pattern);
                        if (match.Success)
                        {
                            freq = match.Groups[1].Value;
                            power = match.Groups[2].Value;
                        }
                        mes_name = $"Freq_{freq}_Packet_1LE_TxPower_{power}_FreqError";
                        
                        pattern = @"\s+(-?\d+\.\d+\s+KHz)\s+(\(-?[\d.]+\s*~\s*-?[\d.]+\))\s*<--\s*(\S+)$";
                        match = Regex.Match(lines.ElementAt(index), pattern);
                        if (match.Success)
                        {
                            value = match.Groups[1].Value;
                            criteria = match.Groups[2].Value;
                            result = match.Groups[3].Value;
                        }

                        var data = new Dictionary<string, object>
                        {
                            {"item", "Ini Freq Error"},
                            {"value", value},
                            {"criteria", criteria},
                            {"result", result}
                        };
                        itemList.Add(data);

                        if (result == "pass")
                            PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "PASS"));
                        else
                            PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "FAIL"));
                    }

                    if (lines.ElementAt(index).Contains("\tPower") == true)
                    {
                        pattern = @"Frequency: (\d+).*Tx Power: (\d+\.\d+)";
                        Match match = Regex.Match(item.Line, pattern);
                        if (match.Success)
                        {
                            freq = match.Groups[1].Value;
                            power = match.Groups[2].Value;
                        }
                        mes_name = $"Freq_{freq}_Packet_1LE_TxPower_{power}_Power";

                        pattern = @"\s+(-?\d+\.\d+\s+dBm)\s+(\(-?[\d.]+\s*~\s*-?[\d.]+\))\s*<--\s*(\S+)$";
                        match = Regex.Match(lines.ElementAt(index), pattern);
                        if (match.Success)
                        {
                            value = match.Groups[1].Value;
                            criteria = match.Groups[2].Value;
                            result = match.Groups[3].Value;
                            Logger.Info($"[Parser LOG] value= {value}");
                            Logger.Info($"[Parser LOG] criteria={criteria}");
                            Logger.Info($"[Parser LOG] result={result}");
                        }

                        var data = new Dictionary<string, object>
                        {
                            {"item", "Power"},
                            {"value", value},
                            {"criteria", criteria},
                            {"result", result}
                        };
                        itemList.Add(data);
                        
                        data = new Dictionary<string, object>
                        {
                           {"title", item.Line},
                           {"data", itemList}
                        };
                        titleList.Add(data);
                        
                        if (result == "pass")
                            PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "PASS"));
                        else
                            PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "FAIL"));
                        break;
                    }
                }
            }

            foreach (var item in rxLines)
            {
                LogMessage($"RXindex  {item.Index}");
                int index = item.Index - 1;
                string freq = string.Empty;
                string power = string.Empty;
                string value = string.Empty;
                string criteria = string.Empty;
                string result = string.Empty;
                string mes_name = string.Empty;
                string pattern = string.Empty;
                while (true)
                {
                    index++;
                    Logger.Info($"[Parser LOG] {lines.ElementAt(index)}");
                    if (lines.ElementAt(index).Contains("\tPER") == true)
                    {
                        pattern = @"Frequency: (\d+).*Rx Power: (\d+\.\d+)";
                        Match match = Regex.Match(item.Line, pattern);
                        if (match.Success)
                        {
                            freq = match.Groups[1].Value;
                            power = match.Groups[2].Value;
                        }
                        mes_name = $"Freq_{freq}_Packet_1LE_RxPower_{power}_PER";

                        pattern = @"\s+(-?\d+\.\d+\s+%)\s+(\(-?[\d.]+\s*~\s*-?[\d.]+\))\s*<--\s*(\S+)$";
                        match = Regex.Match(lines.ElementAt(index), pattern);
                        if (match.Success)
                        {
                            value = match.Groups[1].Value;
                            criteria = match.Groups[2].Value;
                            result = match.Groups[3].Value;
                            Logger.Info($"[Parser LOG] value= {value}");
                            Logger.Info($"[Parser LOG] criteria={criteria}");
                            Logger.Info($"[Parser LOG] result={result}");
                        }

                        var data = new Dictionary<string, object>
                        {
                            {"item", "PER"},
                            {"value", value},
                            {"criteria", criteria},
                            {"result", result}
                        };
                        
                        data = new Dictionary<string, object>
                        {
                            {"title", item.Line},
                            {"data", data}
                        };
                        titleList.Add(data);

                        if (result == "pass")
                            PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "PASS"));
                        else
                            PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "FAIL"));
                        break;
                    }

                }

            }

            if (lines.Any(line => line.Contains("**** P A S S ****")))
                errorCode = "0";
            else
                errorCode = "-1";

            var IQxelData = new Dictionary<string, object>
            {
                {"errorCode", errorCode},
                {"BTData", titleList}
            };
            strOutData = JsonConvert.SerializeObject(IQxelData);

            return true;
        }
        
        public override bool PostProcess()
        {
            if (!Directory.Exists(@"./IQxel_BT")) { Directory.CreateDirectory(@"./IQxel_BT"); }
            File.WriteAllText($@"./IQxel_BT/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}.json", strOutData);
            File.Move(@"./Utility/IQexl_BT_check/Log/Log_Current.txt", 
                     $@"./Utility/IQexl_BT_check/Log/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}.txt");

            if (errorCode == "0")
                return true;
            else
                return false;

        }

    }
}
