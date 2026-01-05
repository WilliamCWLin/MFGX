using Automation.BDaq;
using AutoTestSystem.Base;
using AutoTestSystem.DevicesUI.IO;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AutoTestSystem.Equipment.IO
{
    public class IOSimu2 : IOBase
    {
        private string _strParamInfoPath;
        private static int count = 0;
        private Stopwatch stopwatch = new Stopwatch();
        [Category("GetIO Params"), Description("模擬最終想輸出什麼")]
        public bool GetIO_Return { get; set; } = true;
        [Category("GetIO Params"), Description("模擬幾毫秒後輸出")]
        public int GetIO_Sec { get; set; } = 1000;
        [Category("InstantAI Params"), Description("Simu Random Maxvalue")]
        public double Maxvalue { get; set; }

        [Category("InstantAI Params"), Description("Simu Random Minvalue")]
        public double Minvalue { get; set; }

        private BitArray _doStatus; // 預設64點位，可根據需求調整
        private BitArray _diStatus; // 預設64點位，可根據需求調整
        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool GETIO(int portNum, int pos, ref bool status)
        {
            try
            {
                // 直接從 DI 狀態陣列讀取指定的點位狀態
                if (pos >= 0 && pos < _diStatus.Count)
                {
                    status = _diStatus[pos];
                }
                else
                {
                    LogMessage($"GETIO Error: position {pos} out of range.", MessageLevel.Error);
                    return false;
                }
               
                return true;
            }
            catch (Exception ex)
            {
                LogMessage("IOSimu GETIO Exception." + ex.Message, MessageLevel.Error);
                return false;
            }
        }

        public override bool GETDO(int portNum, int pos, ref bool status)
        {
            try
            {
                // 直接從 DI 狀態陣列讀取指定的點位狀態
                if (pos >= 0 && pos < _diStatus.Count)
                {
                    status = _doStatus[pos];
                }
                else
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogMessage("IOSimu GETIO Exception." + ex.Message, MessageLevel.Error);
                return false;
            }
        }

        public override bool GETALLIO(ref bool[] status)
        {

            _diStatus.CopyTo(status, 0);
            return true;

        }
        public override bool GETALLDO(ref bool[] status)
        {
            _doStatus.CopyTo(status, 0);
            return true;
        }
        public override bool SETIO(int bit, bool output)
        {
            try
            {
                // 確保設定的點位在有效範圍內
                if (bit >= 0 && bit < _diStatus.Count)
                {
                    // 更新 DI 狀態與 DO 狀態保持一致
                    _diStatus[bit] = output;
                    _doStatus[bit] = output;
                }
                else
                {
                    //LogMessage($"SETIO Error: bit {bit} out of range.", MessageLevel.Error);
                    return false;
                }

                //LogMessage($"SETIO({bit},{output})", MessageLevel.Debug);
                return true;
            }
            catch (Exception ex)
            {
                LogMessage("IOSimu SETIO Exception." + ex.Message, MessageLevel.Error);
                return false;
            }
        }
        public override bool Init(string strParamInfo)
        {
            try
            {
                _doStatus = new BitArray(ChannelCount, false); // 預設64點位，可根據需求調整
                _diStatus = new BitArray(ChannelCount, false); // 預設64點位，可根據需求調整
                return true;
            }
            catch (Exception e)
            {
                LogMessage($"ADV_USB4704 Init Fail.{e.Message}");
                return false;
            }

        }

        static public int xxx = 0;
        public override bool InstantAI(string strParamIn, ref string Dataout)
        {
            try
            {
                Random random = new Random();
                double randomDouble = random.NextDouble();
                // 將亂數映射到特定範圍，例如 0 到 10 之間的雙精度亂數
                double minValue = Minvalue/*-0.05*/;
                double maxValue = Maxvalue/*+0.05*/;
                double scaledRandomDouble = minValue + (randomDouble * (maxValue - minValue));
                // 只保留小數點第二位
                double roundedRandomDouble = Math.Round(scaledRandomDouble, 2);
                var data = new Dictionary<string, object>
                    {
                        {
                            "VOLT", Math.Round(roundedRandomDouble, 3)
                        }
                    };
                Dataout = JsonConvert.SerializeObject(data);

                LogMessage($"{Dataout}", MessageLevel.Debug);
            }
            catch (Exception ex)
            {
                LogMessage($"{Description} {ex.Message}", MessageLevel.Error);
            }

            return true;
        }

        public override bool InstantAI(int channel, ref string Dataout)
        {
            try
            {
                Random random = new Random();
                double randomDouble = random.NextDouble();
                // 將亂數映射到特定範圍，例如 0 到 10 之間的雙精度亂數
                double minValue = Minvalue/*-0.05*/;
                double maxValue = Maxvalue/*+0.05*/;
                double scaledRandomDouble = minValue + (randomDouble * (maxValue - minValue));
                // 只保留小數點第二位
                double roundedRandomDouble = Math.Round(scaledRandomDouble, 2);
                var data = new Dictionary<string, object>
                    {
                        { "VOLT", Math.Round(roundedRandomDouble, 3) }
                    };
                Dataout = JsonConvert.SerializeObject(data);

                LogMessage($"{Dataout}", MessageLevel.Debug);
            }
            catch (Exception ex)
            {
                LogMessage($"{Description} {ex.Message}", MessageLevel.Error);
                //DataReceived(this, $"Command Parsing Error.");
            }
            return true;
        }

        public override bool UnInit()
        {
            try
            {
                LogMessage($"UnInit Success");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage("UnInit Exception." + ex.Message, MessageLevel.Error);
                return false;
            }
        }


        public override bool Show()
        {
            //UI_DemoIO form = new UI_DemoIO(this);
            //form.StartPosition = FormStartPosition.CenterScreen; // 設置表單置中
            //form.ShowDialog();

            return true;
        }

    }
}
