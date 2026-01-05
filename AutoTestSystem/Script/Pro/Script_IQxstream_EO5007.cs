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
    internal class Script_IQxstream_EO5007 : Script_Extra_Base
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
            IEnumerable<string> lines = File.ReadAllLines(@"./Utility/EO5007_IQxsteram_LTE/Bin/Log/Log_Current.txt");

            // Check each line for the presence of "LTE: "
            var titles = lines.Select((line, index) => new { Line = Regex.Replace(line, @"[\t]", ""), Index = index + 1 })
                               .Where(rxItem => rxItem.Line.Contains("LTE: "));

            var titleList = new List<Dictionary<string, object>>();

            if (lines.Any(line => line.Contains("[ERROR]")))
            {
                foreach (var line in lines)
                {
                    if (line.Contains("[ERROR]"))
                    {
                        LogMessage($"[Parser LOG] {line.Replace("\t", "")}", MessageLevel.Error);
                    }
                }
            }
            else
            {
                foreach (var title in titles)
                {
                    int retry = 0;
                    int index = title.Index - 1;
                    string band = string.Empty;
                    string freq = string.Empty;
                    string power = string.Empty;
                    string ant = string.Empty;
                    string value = string.Empty;
                    string criteria = string.Empty;
                    string result = string.Empty;
                    string mes_name = string.Empty;
                    string pattern = string.Empty;

                    while (true)
                    {
                        index++;
                        if (lines.ElementAt(index).Contains("\tPower") == true)
                        {
                            pattern = @"Band: (\d+).*Frequency: (\d+\.\d+).*Antenna: ANT_(\d+).*Tx Power: (\d+\.\d+)";
                            Match match = Regex.Match(title.Line, pattern);
                            if (match.Success)
                            {
                                band = match.Groups[1].Value;
                                freq = match.Groups[2].Value;
                                ant = match.Groups[3].Value;
                                power = match.Groups[4].Value;
                                mes_name = $"Band_{band}_Freq_{freq}_TxPower_{power}_ANT_{ant}_Power";
                                LogMessage($"[Parser LOG] {mes_name}");
                            }

                            pattern = @"\s+([\d\.]+\s+dBm)\s+(\([\d.]+\s*~\s*[\d.]+\))\s*<--\s*(\S+)$";
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

                            data = new Dictionary<string, object>
                            {
                                {"title", mes_name},
                                {"data", data}
                            };
                            LogMessage(JsonConvert.SerializeObject(data));
                            titleList.Add(data);


                            if (result == "pass")
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "PASS"));
                            else
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "FAIL"));

                            break;
                        }

                        if (lines.ElementAt(index).Contains("\tRSSI") == true)
                        {
                            pattern = @"Band: (\d+).*Frequency: (\d+\.\d+).*Antenna: ANT_(\d+).*Rx Power: (-\d+\.\d+)";
                            Match match = Regex.Match(title.Line, pattern);
                            if (match.Success)
                            {
                                band = match.Groups[1].Value;
                                freq = match.Groups[2].Value;
                                ant = match.Groups[3].Value;
                                power = match.Groups[4].Value;
                                mes_name = $"Band_{band}_Freq_{freq}_RxPower_{power}_ANT_{ant}_RSSI";
                                LogMessage($"[Parser LOG] {mes_name}");
                            }

                            pattern = @"\s+(-[\d\.]+\s+dBm)\s+(\(-[\d.]+\s*~\s*-[\d.]+\))\s*<--\s*(\S+)$";
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

                            data = new Dictionary<string, object>
                            {
                                {"title", mes_name},
                                {"data", data}
                            };
                            LogMessage(JsonConvert.SerializeObject(data));
                            titleList.Add(data);


                            if (result == "pass")
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "PASS"));
                            else
                                PushMESData($"{mes_name}", Tuple.Create($"{mes_name}", value.Split(' ')[0], "FAIL"));

                            break;
                        }
                    }
                }                
            }

            if (lines.Any(line => line.Contains("**** P A S S ****")))
                errorCode = "0";
            else
                errorCode = "-1";

            var IQxData = new Dictionary<string, object>
            {
                {"errorCode", errorCode},
                {"LTEData", titleList}
            };
            output = JsonConvert.SerializeObject(IQxData);
            strOutData = output;

            return true;
        }
        
        public override bool PostProcess()
        {
            if (!Directory.Exists(@"./EO5007_LTE_data")) { Directory.CreateDirectory(@"./EO5007_LTE_data"); }
            File.WriteAllText($@"./EO5007_LTE_data/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}.json", strOutData);
            File.Move(@"./Utility/EO5007_IQxsteram_LTE/Bin/Log/Log_Current.txt", 
                     $@"./Utility/EO5007_IQxsteram_LTE/Bin/Log/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}.txt");

            if (errorCode == "0")
                return true;
            else
                return false;

        }

    }
}
