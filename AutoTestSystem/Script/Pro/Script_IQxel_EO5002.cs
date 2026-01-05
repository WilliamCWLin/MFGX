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
    internal class Script_IQxel_EO5002 : Script_Extra_Base
    {
        string strOutData = string.Empty;
        string errorCode = string.Empty;


        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess()
        {
            return true;
        }
        
        public override bool Process(ref string output)
        {
            // Read all lines from the file
            IEnumerable<string> lines = File.ReadAllLines(@"./Utility/EO5002_IQxel_BT/Log/Log_Current.txt");

            // Check each line for the presence of "1LE, Tx"
            var txTitles = lines.Select((line, index) => new { Line = Regex.Replace(line, @"[\t]", ""), Index = index + 1 })
                               .Where(rxItem => rxItem.Line.Contains("1LE, Tx"));

            // Check each line for the presence of "1LE, Rx"
            var rxTitles = lines.Select((line, index) => new { Line = Regex.Replace(line, @"[\t]", ""), Index = index + 1 })
                               .Where(txItem => txItem.Line.Contains("1LE, Rx"));
            
            var titleList = new List<Dictionary<string, object>>();
            var IQxData = new Dictionary<string, object> { };

            if (lines.Any(line => line.Contains("[ERROR]")))
            {
                foreach (var line in lines)
                {
                    if (line.Contains("[ERROR]"))
                    {
                        LogMessage($"[Parser LOG] {line}");
                    }
                }
            }
            else
            {
                foreach (var title in txTitles)
                {
                    int retry = 0;
                    int index = title.Index - 1;
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
                        //LogMessage($"[Parser LOG] {lines.ElementAt(index)}");
                        if (lines.ElementAt(index).Contains("\tIni Freq Error") == true)
                        {
                            pattern = @"Frequency: (\d+),.*Tx Power: (\d+\.\d+)";
                            Match match = Regex.Match(title.Line, pattern);
                            if (match.Success)
                            {
                                freq = match.Groups[1].Value;
                                power = match.Groups[2].Value;
                                mes_name = $"Freq_{freq}_Packet_1LE_TxPower_{power}_FreqError";
                                LogMessage($"[Parser LOG] {mes_name}");
                            }

                            pattern = @"\s+(-?\d+\.\d+\s+KHz)\s+(\(-?[\d.]+\s*~\s*-?[\d.]+\))\s*<--\s*(\S+)$";
                            match = Regex.Match(lines.ElementAt(index), pattern);
                            if (match.Success)
                            {
                                value = match.Groups[1].Value;
                                criteria = match.Groups[2].Value;
                                result = match.Groups[3].Value;
                                LogMessage($"[Parser LOG] {lines.ElementAt(index)}");
                                LogMessage($"[Parser LOG] value: {value}");
                                LogMessage($"[Parser LOG] criteria: {criteria}");
                                LogMessage($"[Parser LOG] result: {result}");
                            }

                            if (result == "fail" && retry < 2)
                            {
                                retry++;
                                continue;
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

                            IQxData.Add(mes_name, value.Split(' ')[0]);
                        }

                        if (lines.ElementAt(index).Contains("\tPower") == true)
                        {
                            pattern = @"Frequency: (\d+),.*Tx Power: (\d+\.\d+)";
                            Match match = Regex.Match(title.Line, pattern);
                            if (match.Success)
                            {
                                freq = match.Groups[1].Value;
                                power = match.Groups[2].Value;
                                mes_name = $"Freq_{freq}_Packet_1LE_TxPower_{power}_Power";
                                LogMessage($"[Parser LOG] {mes_name}");
                            }

                            pattern = @"\s+(-?\d+\.\d+\s+dBm)\s+(\(-?[\d.]+\s*~\s*-?[\d.]+\))\s*<--\s*(\S+)$";
                            match = Regex.Match(lines.ElementAt(index), pattern);
                            if (match.Success)
                            {
                                value = match.Groups[1].Value;
                                criteria = match.Groups[2].Value;
                                result = match.Groups[3].Value;
                                LogMessage($"[Parser LOG] value: {value}");
                                LogMessage($"[Parser LOG] criteria: {criteria}");
                                LogMessage($"[Parser LOG] result: {result}");
                            }

                            if (result == "fail" && retry < 2)
                            {
                                retry++;
                                continue;
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
                               {"title", mes_name},
                               {"data", itemList}
                            };
                            titleList.Add(data);

                            if (result == "pass")
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "PASS"));
                            else
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "FAIL"));

                            IQxData.Add(mes_name, value.Split(' ')[0]);
                            break;
                        }
                    }
                }

                foreach (var title in rxTitles)
                {
                    int retry = 0;
                    int index = title.Index - 1;
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
                        //LogMessage($"[Parser LOG] {lines.ElementAt(index)}");
                        if (lines.ElementAt(index).Contains("\tPER") == true)
                        {
                            pattern = @"Frequency: (\d+),.*Rx Power: ([-\d\.]+)";
                            Match match = Regex.Match(title.Line, pattern);
                            if (match.Success)
                            {
                                freq = match.Groups[1].Value;
                                power = match.Groups[2].Value;
                                mes_name = $"Freq_{freq}_Packet_1LE_RxPower_{power}_PER";
                                LogMessage($"[Parser LOG] {mes_name}");
                            }

                            pattern = @"\s+(-?\d+\.\d+\s+%)\s+(\(-?[\d.]+\s*~\s*-?[\d.]+\))\s*<--\s*(\S+)$";
                            match = Regex.Match(lines.ElementAt(index), pattern);
                            if (match.Success)
                            {
                                value = match.Groups[1].Value;
                                criteria = match.Groups[2].Value;
                                result = match.Groups[3].Value;
                                LogMessage($"[Parser LOG] value: {value}");
                                LogMessage($"[Parser LOG] criteria: {criteria}");
                                LogMessage($"[Parser LOG] result: {result}");
                            }

                            if (result == "fail" && retry < 2)
                            {
                                retry++;
                                continue;
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
                                {"title", mes_name},
                                {"data", data}
                            };
                            titleList.Add(data);

                            if (result == "pass")
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "PASS"));
                            else
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "FAIL"));

                            IQxData.Add(mes_name, value.Split(' ')[0]);
                            break;
                        }

                    }
                }
            }

            if (lines.Any(line => line.Contains("**** P A S S ****")))
                errorCode = "0";
            else
                errorCode = "-1";

            // var IQxData = new Dictionary<string, object>
            // {
            //     {"errorCode", errorCode},
            //     {"BTData", titleList}
            // };
            IQxData.Add("errorCode", errorCode);
            output = JsonConvert.SerializeObject(IQxData);
            strOutData = output;

            return true;
        }
        
        public override bool PostProcess()
        {
            if (!Directory.Exists(@"./EO5002_BT_data")) { Directory.CreateDirectory(@"./EO5002_BT_data"); }
            File.WriteAllText($@"./EO5002_BT_data/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}.json", strOutData);
            File.Move(@"./Utility/EO5002_IQxel_BT/Log/Log_Current.txt", 
                     $@"./Utility/EO5002_IQxel_BT/Log/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}.txt");

            string result = CheckRule(strOutData, Spec);
            if (result == "PASS" || Spec == "")
                return true;
            else
                return false;

        }

    }
}
