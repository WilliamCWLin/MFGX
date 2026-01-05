using AutoTestSystem.Base;
using AutoTestSystem.Equipment.ControlDevice;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoTestSystem.Script
{
    internal class Script_ControlDevice_OpenShort : Script_ControlDevice_Base
    {
        [Category("Command"), Description("支援用%%方式做變數值取代"), TypeConverter(typeof(CommandConverter))]
        public string Send { get; set; } = "73 74 61 72 74 00 03 00 01 65 6e 64";

        [Category("Mode"), Description("設定模式：SAVE 或 COMPARE")]
        public string Mode { get; set; } = "COMPARE";

        [Category("GoldenFile"), Description("Golden 檔案路徑")]
        public string GoldenFilePath { get; set; } = @"D:\Golden.txt";
        private string l_strOutData = string.Empty;
        public override bool PreProcess()
        {
            l_strOutData = string.Empty;
            return true;
        }

        public override bool Process(ControlDeviceBase ControlDevice, ref string output)
        {
            try
            {
                byte[] packet = null;

                // 發送指令
                ControlDevice.SEND(Send);

                // 讀取回應
                if (!ControlDevice.READ(ref packet, 5000))
                {
                    LogMessage("Timeout or read failed.", MessageLevel.Error);
                    return false;
                }

                // 解析測試結果
                int[,] testBits = ParsePacketToBitArray(packet);

                if (Mode.Equals("SAVE", StringComparison.OrdinalIgnoreCase))
                {
                    SaveGoldenFile(GoldenFilePath, testBits);
                    LogMessage($"Golden file saved at {GoldenFilePath}", MessageLevel.Info);
                }
                else if (Mode.Equals("COMPARE", StringComparison.OrdinalIgnoreCase))
                {
                    if (!File.Exists(GoldenFilePath))
                    {
                        LogMessage("Golden file not found. Please run in SAVE mode first.", MessageLevel.Error);
                        return false;
                    }

                    int[,] goldenBits = LoadGoldenFile(GoldenFilePath);
                    var diffs = CompareBitArrays(testBits, goldenBits);


                    int diffsCount = diffs.Count;

                    // 建立 JSON 結構
                    var resultObj = new
                    {
                        Mode = Mode,
                        GoldenFile = GoldenFilePath,
                        DiffCount = diffsCount,
                    };

                    // 轉成 JSON 字串
                    string jsonResult = JsonConvert.SerializeObject(resultObj, Formatting.Indented);

                    // 保存 JSON 檔案（可選）
                    File.WriteAllText(@"D:\CompareResult.json", jsonResult);

                    // Log 顯示
                    LogMessage(jsonResult, MessageLevel.Info);

                    l_strOutData = jsonResult;
                    output = jsonResult;
                    // 判斷是否回傳 false
                    if (diffsCount == 0)
                    {
                        LogMessage("All pins match golden data.", MessageLevel.Info);
                    }
                    else
                    {

                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"Found {diffs.Count} mismatches:");
                        foreach (var (pin, bit, expected, actual) in diffs)
                        {
                            sb.AppendLine($"Mismatch at PIN_{pin:D3}, Bit_{bit:D3} (Expected={expected}, Actual={actual})");
                        }

                        // 一次輸出所有差異
                        LogMessage(sb.ToString(), MessageLevel.Warn);

                        return false; // 有差異時回傳 false
                    }

                }
                else
                {
                    LogMessage($"Invalid MODE: {Mode}. Use SAVE or COMPARE.", MessageLevel.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Process Exception: {ex.Message}", MessageLevel.Fatal);
                return false;
            }
        }

        private int[,] ParsePacketToBitArray(byte[] packet)
        {
            int headerLen = 3;
            int pinLineLen = 21;
            int footerLen = 3;
            int pinLines = (packet.Length - headerLen - footerLen) / pinLineLen;
            int[,] bitArray = new int[pinLines, 160];

            for (int line = 0; line < pinLines; line++)
            {
                int offset = headerLen + line * pinLineLen;
                byte[] pinBytes = packet.Skip(offset).Take(pinLineLen).ToArray();

                int bitIdx = 0;
                for (int i = 1; i < pinBytes.Length; i++)
                {
                    string bits = Convert.ToString(pinBytes[i], 2).PadLeft(8, '0');
                    bits = new string(bits.Reverse().ToArray());
                    for (int b = 0; b < 8 && bitIdx < 160; b++)
                    {
                        bitArray[line, bitIdx++] = bits[b] == '1' ? 1 : 0;
                    }
                }
            }
            return bitArray;
        }

        private void SaveGoldenFile(string filePath, int[,] bitArray)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    int pinCount = bitArray.GetLength(0);
                    int bitCount = bitArray.GetLength(1);
                    for (int pin = 0; pin < pinCount; pin++)
                    {
                        StringBuilder line = new StringBuilder();
                        for (int bit = 0; bit < bitCount; bit++)
                        {
                            line.Append(bitArray[pin, bit]);
                        }
                        writer.WriteLine(line.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"SaveGoldenFile Exception: {ex.Message}", MessageLevel.Error);
            }
        }

        private int[,] LoadGoldenFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                int pinCount = lines.Length;
                int bitCount = lines[0].Length;
                int[,] bitArray = new int[pinCount, bitCount];

                for (int pin = 0; pin < pinCount; pin++)
                {
                    for (int bit = 0; bit < bitCount; bit++)
                    {
                        bitArray[pin, bit] = lines[pin][bit] == '1' ? 1 : 0;
                    }
                }
                return bitArray;
            }
            catch (Exception ex)
            {
                LogMessage($"LoadGoldenFile Exception: {ex.Message}", MessageLevel.Error);
                return new int[0, 0];
            }
        }

        private List<(int pin, int bit, int expected, int actual)> CompareBitArrays(int[,] testArray, int[,] goldenArray)
        {
            var diffs = new List<(int pin, int bit, int expected, int actual)>();
            try
            {
                int pinCount = testArray.GetLength(0);
                int bitCount = testArray.GetLength(1);

                for (int pin = 0; pin < pinCount; pin++)
                {
                    for (int bit = 0; bit < bitCount; bit++)
                    {
                        if (testArray[pin, bit] != goldenArray[pin, bit])
                        {
                            diffs.Add((pin + 1, bit + 1, goldenArray[pin, bit], testArray[pin, bit]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"CompareBitArrays Exception: {ex.Message}", MessageLevel.Error);
            }
            return diffs;
        }
    }
}