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
    public class IOSimu : IOBase
    {
        private string _strParamInfoPath;
        private static int count = 0;
        private Stopwatch stopwatch = new Stopwatch();
        [Category("GetIO Params"), Description("模擬最終想輸出什麼")]
        public bool GetIO_Return { get; set; } = true;
        [Category("GetIO Params"), Description("模擬幾毫秒後輸出")]
        public int GetIO_Sec { get; set; } = 1000;
        [Category("InstantAI Params"), Description("Simu Random Maxvalue")]
        public double Maxvalue{ get; set; }

        [Category("InstantAI Params"), Description("Simu Random Minvalue")]
        public double Minvalue { get; set; }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool GETIO(int portNum, int pos, ref bool status)
        {
            try
            {
                if (!stopwatch.IsRunning)
                {
                    stopwatch.Start();
                }
                if (stopwatch.Elapsed.TotalMilliseconds > GetIO_Sec)
                {
                    status = GetIO_Return;
                    stopwatch.Reset();
                }
                else
                    status = !GetIO_Return;



                //count++;

                //if (count == 30)
                //{
                //    status = GetIO_Return;
                //    count = 0;
                //}
                //else
                //    status = !GetIO_Return;

                LogMessage($"GETIO({pos},{status})", MessageLevel.Debug);
                return true;
            }
            catch (Exception ex)
            {
                LogMessage("ADV_USB4761 GETIO Exception." + ex.Message, MessageLevel.Error);
                return false;
            }

        }
        public override bool SETIO(int portNum, int bit, bool output)
        {
            LogMessage($"SETIO({bit},{output})", MessageLevel.Debug);

            return true;
        }
        public override bool Init(string strParamInfo)
        {
            try
            {
                return true;
            }
            catch(Exception e)
            {
                LogMessage($"ADV_USB4704 Init Fail.{e.Message}");
                return false;
            }

        }

        static public int  xxx = 0;
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
