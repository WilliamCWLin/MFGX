/*
 * "AutoTestSystem.Model --> Test"
 *
 * Corpright William & Zhibin
 *
 *.##.......####.########.########..#######..##....##
 *.##........##.....##....##.......##.....##.###...##
 *.##........##.....##....##.......##.....##.####..##
 *.##........##.....##....######...##.....##.##.##.##
 *.##........##.....##....##.......##.....##.##..####
 *.##........##.....##....##.......##.....##.##...###
 *.########.####....##....########..#######..##....##
 *
 *  1. <Test.cs> is a use for run script (odd one)
 *  2. It have a new version class <TestNew.cs>
 *
 * 
 */

/*****************************************************************************
*                    Includes Definitions
*****************************************************************************/
using AutoTestSystem.DAL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.MainForm;
using static System.String;

/*****************************************************************************
*                    Function code
*****************************************************************************/
namespace AutoTestSystem.Model
{
    public class Test
    {
        /// 扫描SN
        public string SN; //{ get; private set; }

        /// 机种
        public string DutMode;// { get; private set; }

        /// DUT IP address
        public string DUTIP;// { get; private set; }

        public Test()
        {
        }

        public Test(string _SN, string _DutMode)
        {
            SN = _SN;
            DutMode = _DutMode;
        }

        public Test(string _SN, string _DutMode, string _DUTIP)
        {
            SN = _SN;
            DutMode = _DutMode;
            DUTIP = _DUTIP;
        }

        public bool StepTest(test_phases testPhase, Items item, int retryTimes, phase_items phaseItem)
        {
            
            /*  string info = "";
              error_code = "";
              error_details = "";
              bool rReturn = false;
              // 如果有多行errorcode
              string[] ErrorList = item.ErrorCode.Split(new string[] { "\n", "\r\n" }, 0);
              // debug模式下要skip的测试步骤
              if ((Global.TESTMODE == "debug" || IsDebug) &&
                  (item.TestKeyword.Contains("SetIpaddrEnv") || item.TestKeyword.Contains("GetIpaddrEnv")
                  || item.TestKeyword.Contains("CheckEeroTest") || item.TestKeyword.Contains("Checkroute")
                  || item.TestKeyword.Contains("CheckEeroABA") || item.TestKeyword.Contains("SetDHCP")))
              {
                  Logger.Warn("This is debug mode.Skip this step.");
                  testPhase.phase_details = "This is debug mode.";
                  return rReturn = true;
              }

              // 发送的命令中有变量
              while (!String.IsNullOrEmpty(item.ComdSend) && item.ComdSend.Contains("<") && item.ComdSend.Contains(">"))
              {
                  string verName = GetMidStr(item.ComdSend, "<", ">");
                  item.ComdSend = GetMidStr(item.ComdSend, null, "<") + GetVerReflection(f1, verName) + GetMidStr(item.ComdSend, ">", null);
                  retry = 0;  //！有变量不允许retry
                  retryTimes = 0;
              }
              // Spec值中含有变量
              while (!string.IsNullOrEmpty(item.Spec) && item.Spec.Contains("<") && item.Spec.Contains(">"))
              {
  #if DEBUG
                  MesMac = "9c:a5:70:00:39:60";
  #endif

  #if DEBUG
                  mes_qsdk_version = Global.QSDKVER;
  #else
                  if (item.TestKeyword == "Veritfy_QSDK_Version")
                  {
                      mes_qsdk_version = mescheckroute.getFirmwareFW(SN);
                      Logger.Debug($"mescheckroute.getFirmwareFW:{mes_qsdk_version}");
                  }
  #endif
                  string verName = GetMidStr(item.Spec, "<", ">");
                  item.Spec = GetMidStr(item.Spec, null, "<") + GetVerReflection(f1, verName) + GetMidStr(item.Spec, ">", null);
                  if (string.IsNullOrEmpty(item.Spec))
                  {
                      Logger.Error($"Parsing item.Spec failed, IsNullOrEmpty!!! test FAIL.");
                      retry = 0;  //！有变量不允许retry
                      retryTimes = 0;
                      error_code = ErrorList[0].Split(':')[0].Trim();
                      error_details = ErrorList[0].Split(':')[1].Trim();
                      testPhase.phase_details = error_details;
                      testPhase.error_code = error_code;
                      return rReturn = false;
                  }
                  Logger.Debug($"item.Spec:{item.Spec}");
              }

              if (item.ComdSend == "quit" || item.ComdSend == "0x03")
              {
                  byte[] quit = { 0x03 };
                  item.ComdSend = Encoding.ASCII.GetString(quit).ToUpper();
              }

              try
              {
                  switch (item.TestKeyword)
                  {
                      case "KillProcess":
                          rReturn = KillProcess(item.ComdSend);
                          break;

                      case "StartProcess":
                          rReturn = StartProcess(item.ExpectStr, item.ComdSend);
                          break;

                      case "RestartProcess":
                          rReturn = RestartProcess(item.ExpectStr, item.ComdSend);
                          break;

                      case "Wait":
                      case "ThreadSleep":
                          Sleep(item.ComdSend);
                          rReturn = true;
                          break;

                      case "MessageBoxShow":
                          rReturn = ConfirmMessageBox(item.ComdSend, item.ExpectStr, item.TimeOut == "0" ? MessageBoxButtons.OK : MessageBoxButtons.YesNo);
                          break;

                      case "PingDUT":
                          rReturn = PingIP(!IsNullOrEmpty(item.ComdSend) ? item.ComdSend : DUTIP, int.Parse(item.TimeOut));
                          break;

                      case "SampleTelnetLogin":
                          {
                              SampleComm = new Telnet(new TelnetInfo { _Address = item.ComdSend });
                              rReturn = SampleComm.Open(item.ExpectStr);
                          }
                          break;

                      case "WaitForTelnet":
                          {
                              if (DUTCOMM == null)
                              {
                                  DUTCOMM = new Telnet(telnetInfo);
                              }
                              rReturn = DUTCOMM.Open(Global.PROMPT);
                          }
                          break;
                      case "TelnetAndSendCmd":
                          {
                              STAComm = new Telnet(new TelnetInfo { _Address = item.ComdSend });
                              string revStr = "";
                              if (STAComm.Open(item.ExpectStr) && STAComm.SendCommand(item.CheckStr2, ref revStr, item.ExpectStr, short.Parse(item.TimeOut))
                                  && revStr.CheckStr(item.CheckStr1))
                              {
                                  rReturn = true;
                              }
                              Sleep("2");
                          }
                          break;
                      case "COMPortOpen":
                      case "SerialPortOpen":
                          {
                              if (DUTCOMM == null)
                              {
                                  if (!string.IsNullOrEmpty(item.ComdSend))
                                  {
                                      DUTCOMinfo = new SerialConnetInfo { PortName = item.ComdSend, BaudRate = int.Parse(item.ExpectStr) };
                                  }
                                  DUTCOMM = new Comport(DUTCOMinfo);
                              }
                              rReturn = DUTCOMM.Open();
                          }
                          break;

                      case "CloseDUTCOMM":
                          if (DUTCOMM != null)
                          {
                              DUTCOMM.Close();
                              rReturn = true;
                          }
                          break;

                      case "ClearDirectory":
                          rReturn = ClearDirectory(item.ComdSend);
                          break;

                      case "PassSN":
                          rReturn = WriteSNandMoveFile(SN, $@"{System.Environment.CurrentDirectory}\{item.ComdSend}", item.ExpectStr);
                          break;

                      case "Waitingcsvlog":
                          csvLines = null;
                          rReturn = WaitingCSVlog(item.TimeOut, item.ComdSend, SN, out csvLines);
                          break;
                      case "WaitingcsvlogBT":
                          {
                              csvLines = null;
                              var lngStart = DateTime.Now.AddSeconds(int.Parse(item.TimeOut)).Ticks;
                              while (DateTime.Now.Ticks <= lngStart)
                              {
                                  var files = Directory.GetFileSystemEntries(item.ComdSend);
                                  if (files.Length != 0)
                                  {
                                      foreach (var file in files)
                                      {
                                          if (file.Contains(SN))
                                          {
                                              return rReturn = WaitingCSVlog("2", file, SN, out csvLines);
                                          }
                                      }
                                  }
                                  else
                                      Thread.Sleep(1000);
                              }
                          }
                          break;

                      case "CreateZipFile":
                          {
                              string zipPath = $@"{System.Environment.CurrentDirectory}\{item.ComdSend}";
                              if (Directory.Exists($@"D:\litepoint"))
                              {
                                  Directory.Delete($@"D:\litepoint");
                              }
                              if (File.Exists(zipPath))
                              {
                                  File.Delete(zipPath);
                              }
                              Directory.CreateDirectory($@"D:\litepoint");
                              rReturn = CompressFile($@"D:\litepoint", zipPath);
                          }
                          break;

                      case "CompressFile":
                          {
                              string zipPath = item.ExpectStr.Replace($@"./", $@"{System.Environment.CurrentDirectory}\").Replace("SN", SN).Replace("DateTime.Now:yyyy-MM-dd_hh-mm-ss", $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
                              var files = Directory.GetFileSystemEntries(item.ComdSend);
                              if (files.Length != 0)
                              {
                                  foreach (var file in files)
                                  {
                                      if (file.Contains(SN))
                                      {
                                          return rReturn = CompressFile(file, zipPath);
                                      }
                                  }
                                  Logger.Debug($"No {SN} file found!");
                              }
                              else
                              {
                                  Logger.Debug("Directory is empty!");
                              }
                          }
                          break;

                      case "CheckEeroTest":
                          {
                              if (mescheckroute.CheckEeroTest(SN, Global.TESTMODE, out string mesMsg) && mesMsg.Contains("OK"))
                              {
                                  rReturn = true;
                              }
                              else
                              {
                                  Logger.Error("mesMsg:" + mesMsg);
                              }
                          }
                          break;

                      case "Checkroute":
                          {
                              if (mescheckroute.checkroute(SN, Global.FIXTURENAME, out string mesMsg) && mesMsg.Contains("OK"))
                              {
                                  rReturn = true;
                              }
                              else
                              {
                                  Logger.Error("mesMsg:" + mesMsg);
                                  if (mesMsg.Contains("recheck"))
                                  {
                                      error_code = ErrorList[0].Split(':')[0].Trim();
                                      error_details = ErrorList[0].Split(':')[1].Trim();
                                      testPhase.phase_details = error_details;
                                      testPhase.error_code = error_code;
                                  }
                                  else if (mesMsg.Contains("失败"))
                                  {
                                      error_code = ErrorList[1].Split(':')[0].Trim();
                                      error_details = ErrorList[1].Split(':')[1].Trim();
                                      testPhase.phase_details = error_details;
                                      testPhase.error_code = error_code;
                                  }
                                  else
                                  {
                                      error_code = ErrorList[2].Split(':')[0].Trim();
                                      error_details = ErrorList[2].Split(':')[1].Trim();
                                      testPhase.phase_details = error_details;
                                      testPhase.error_code = error_code;
                                  }
                                  Logger.Error($"test fail,set error_code:{error_code},error_details:{error_details}");
                              }
                          }
                          break;

                      case "CheckEeroABA":
                          {
                              if (mescheckroute.checkEeroABA(SN, Global.FIXTURENAME, Global.STATIONNAME, out string mesMsg) && mesMsg.Contains("OK"))
                              {
                                  rReturn = true;
                              }
                              else
                              {
                                  Logger.Error("mesMsg:" + mesMsg);
                              }
                          }
                          break;

                      case "GetPcbaErroMessage":
                          {
                              if (mescheckroute.GetPcbaErroMessage(SN, out CSN, out MesMac, out string mesMsg))
                              {
                                  Logger.Debug("CUSTOMER_SN:" + CSN + ", GetMesMac:" + MesMac);
                                  rReturn = true;
                              }
                              Logger.Debug("mesMsg:" + mesMsg);
                          }
                          break;

                      case "GetCsnErroMessage":
                          {
                              if (mescheckroute.GetCsnErroMessage(SN, out CSN, out string IPSN, out MesMac, out string mesMsg))
                                  rReturn = true;
                              Logger.Debug("mesMsg:" + mesMsg);
                              Logger.Debug("Get MesMac:" + MesMac + ", sn:" + CSN + ", IPSN:" + IPSN);
                          }
                          break;

                      case "ReportChildBoard":
                          {
                              if (mescheckroute.GetCsnErroMessage(SN, out string serialNum, out string ItemPartSN, out string MesMac, out string mesMsg)
                                  && mesMsg.Contains("OK"))
                              {
                                  phaseItem.serial = ItemPartSN.Trim();
                                  mesPhases.ChildBoardSN = ItemPartSN.Trim();
                                  if (DutMode.ToLower() == "gateway")
                                  {
                                      phaseItem.model = "alma";
                                  }
                                  if (DutMode.ToLower() == "leaf")
                                  {
                                      phaseItem.model = "adventure";
                                  }
                                  if (DutMode.ToLower() == "firefly")
                                  {
                                      phaseItem.model = "finch";
                                  }
                                  rReturn = true;
                                  testPhase.phase_items.Add(phaseItem);
                              }
                          }
                          break;

                      case "GetMesIP":
                          {
                              if (mescheckroute.GETIP(SN, out DUTMesIP, out string mesMsg) && mesMsg.Contains("OK"))
                              {
                                  rReturn = true;
                                  Logger.Debug("mesMsg:" + mesMsg);
                                  Logger.Debug("sn:" + SN + ", MesIP:" + DUTMesIP);
                              }
                          }
                          break;

                      case "CurrentTest":
                      case "FIXCOMSend":
                          {
                              if (Global.FIXTUREFLAG == "1")
                              {
                                  //using (var FIXCOMM = new Comport(FixCOMinfo))
                                  //{
                                  FixSerialPort.OpenCOM();
                                  var revStr = "";
                                  inPutValue = "";
                                  if (FixSerialPort.SendCommandToFix(item.ComdSend, ref revStr, item.ExpectStr, short.Parse(item.TimeOut))
                                       && revStr.CheckStr(item.CheckStr1) && revStr.CheckStr(item.CheckStr2))
                                  {
                                      rReturn = true;
                                      // 需要提取测试值
                                      if (!string.IsNullOrEmpty(item.SubStr1) || !string.IsNullOrEmpty(item.SubStr2))
                                      {
                                        //  item.TestValue = GetValue(revStr, item.SubStr1, item.SubStr2);
                                          if (item.TestKeyword == "CurrentTest") //治具返回的电流是mA，需要转成A
                                          {
                                        //      item.TestValue = (double.Parse(item.TestValue) / 1000).ToString();
                                          }
                                      }
                                      else
                                      {
                                          return rReturn = true;
                                      }

                                      // 需要比较Spec
                                //      if (!string.IsNullOrEmpty(item.Spec) && string.IsNullOrEmpty(item.Limit_min) && string.IsNullOrEmpty(item.Limit_max))
                             //         {
                              //            return rReturn = CheckSpec(item.Spec, item.TestValue);
                             //         }

                                      // 需要比较Limit
                                      if (!string.IsNullOrEmpty(item.Limit_min) || !string.IsNullOrEmpty(item.Limit_max))
                                      {
                                          rReturn = CompareLimit(item.Limit_min, item.Limit_max, item.TestValue, out info);
                                      }
                                  }
                                  //    FIXCOMM.Close(); FIXCOMM.Dispose();
                                  //}
                              }
                              else
                              {
                                  if (!string.IsNullOrEmpty(item.CheckStr2))
                                  {
                                      // 不使用治具，messageboxshow提示作业员操作，然后确认。
                                      rReturn = ConfirmMessageBox(item.CheckStr2, item.ItemName);
                                  }
                                  else
                                  {
                                      Logger.Warn("Attention! FIXTUREFLAG == 0,and no ConfirmMessageBox. ");
                                      rReturn = true;
                                  }
                              }
                          }
                          break;

                      case "VoltageTest":
                      case "LEDTest":
                          if (inPutValue == "")
                          {
                              //using (var FIXCOMM = new Comport(FixCOMinfo))
                              //{
                              FixSerialPort.OpenCOM();
                              Thread.Sleep(500);
                              var revStr = "";
                              if (FixSerialPort.SendCommandToFix(item.ComdSend, ref revStr, item.ExpectStr, short.Parse(item.TimeOut))
                                   && revStr.CheckStr(item.CheckStr1) && revStr.CheckStr(item.CheckStr2))
                              {
                                  rReturn = true;
                                  inPutValue = revStr;
                              }
                              else
                              {
                                  inPutValue = "";
                                  return rReturn = false;
                              }
                              //FIXCOMM.Close();
                              //}
                          }
                          {
                              var revStr = inPutValue;
                              // 需要提取测试值
                              if (!string.IsNullOrEmpty(item.SubStr1) || !string.IsNullOrEmpty(item.SubStr2))
                              {
                                  item.TestValue = GetValue(revStr, item.SubStr1, item.SubStr2);
                              }
                              else
                              {
                                  return rReturn = true;
                              }
                              // 需要比较Spec
                              if (!string.IsNullOrEmpty(item.Spec) && string.IsNullOrEmpty(item.Limit_min) && string.IsNullOrEmpty(item.Limit_max))
                              {
                                  return rReturn = CheckSpec(item.Spec, item.TestValue);
                              }
                              // 需要比较Limit
                              if (!string.IsNullOrEmpty(item.Limit_min) || !string.IsNullOrEmpty(item.Limit_max))
                              {
                                  rReturn = CompareLimit(item.Limit_min, item.Limit_max, item.TestValue, out info);
                              }
                          }
                          if (!rReturn)
                          {
                              inPutValue = "";
                          }
                          break;

                      case "BTPairTest":
                          rReturn = BTConnection(item.ComdSend, BtDevAddress, int.Parse(item.RetryTimes));
                          //rReturn = ConfrimMessageBox($"Pls connect Qorvo Inc.bt and confirm succed than click ok.", item.ItemName, MessageBoxButtons.YesNo);
                          break;

                      case "Zigbee": //SRF
                          {
                              List<string> arrayListTemp = new List<string>();
                              List<string> arrayListTemp_yuqiang = new List<string>();
                              for (int i = 0; i < csvLines.Length; i++)
                              {
                                  string[] temp = csvLines[i].Split(new char[] { ',' }, StringSplitOptions.None);
                                  //#if DEBUG  //获取CSV表头
                                  if (i == 0)
                                  {
                                      arrayListTemp.AddRange(new string[] { temp[56], temp[5], temp[7], temp[8], temp[33], temp[34], temp[36], temp[37], temp[48], temp[50], temp[51], temp[52], temp[54], temp[55] });
                                      arrayListTemp_yuqiang.AddRange(new string[] { temp[33], temp[46], temp[48] });
                                  }
                                  //#endif
                                  if (temp[0] == item.TestKeyword && temp[1] == item.SubStr1 && temp[3] == item.ComdSend)
                                  {
                                      Logger.Info($"find test result in csv line{i + 1}.testResult={temp[52]}");
                                      phaseItem.radio = "0";
                                      phaseItem.chain = "0";
                                      phaseItem.frequency = item.ComdSend;
                                      phaseItem.data_rate = "O-QPSK";
                                      phaseItem.evm = temp[52] == "NA" ? "" : $"{temp[52]}";
                                      phaseItem.freq_error = temp[5] == "NA" ? "" : $"{temp[5]}";
                                      phaseItem.lo_leakage = "";
                                      phaseItem.per_test_power = "";
                                      phaseItem.power_accuracy = temp[33] == "NA" ? "" : $"{temp[33]}";
                                      phaseItem.power_spec = temp[34] == "NA" ? "" : $"{temp[34]}";
                                      phaseItem.rx_per = temp[48] == "NA" ? "" : $"{temp[48]}";
                                      phaseItem.spectral_flatness = "";
                                      phaseItem.spectrum_mask = "";
                                      phaseItem.sym_clk_error = "";
                                      testPhase.phase_items.Add(phaseItem);
                                      rReturn = temp[56].ToLower() == "pass" ? true : false;
                                      if (!rReturn)
                                      {
                                          ErrorList = temp[2].Trim().Split(new string[] { "\n" }, 0);
                                      }

                                      //#if DEBUG  //生成上传SFTP的CSV表头
                                      string newitemName = "";
                                      string newitem = "";
                                      foreach (var items in arrayListTemp)
                                      {
                                          if (!items.Contains("limit_min") && !items.Contains("limit_max"))
                                          {
                                              newitem = $"{item.ItemName}_{items.Trim()}";
                                              newitemName = newitem;
                                          }
                                          else
                                          {
                                              newitem = $"{newitemName}_{items.Trim()}";
                                          }
                                          ArrayListCsvHeader.Add(newitem.ToUpper());
                                      }
                                      // for yuqiang
                                      foreach (var items in arrayListTemp_yuqiang)
                                      {
                                          if ((item.SubStr1 == "Tx" && items.Trim() == "Power"))
                                          {
                                              newitem = $"{item.ItemName}_{items.Trim()}";
                                              ArrayListCsvHeader_yuqiang.Add(newitem.ToUpper());
                                          }
                                          else if (item.SubStr1 == "Rx" && (items.Trim() == "Rx_Power" || items.Trim() == "RxPER"))
                                          {
                                              newitem = $"{item.ItemName}_{items.Trim()}";
                                              ArrayListCsvHeader_yuqiang.Add(newitem.ToUpper());
                                          }
                                          else
                                          {

                                          }
                                      }

                                      //#else
                                      ArrayListCsv.AddRange(new string[] {temp[56].ToLower()=="pass"?"TRUE":"FALSE", temp[5], temp[7], temp[8], temp[33], temp[34], temp[36], temp[37], temp[48], temp[50], temp[51], temp[52], temp[54], temp[55]
                                      });
                                      // for yuqiang
                                      if (temp[33] != "NA")
                                      {
                                          ArrayListCsv_yuqiang.Add(temp[33]);
                                      }
                                      if (temp[46] != "NA")
                                      {
                                          ArrayListCsv_yuqiang.Add(temp[46]);
                                      }
                                      if (temp[48] != "NA")
                                      {
                                          ArrayListCsv_yuqiang.Add(temp[48]);
                                      }
                                      //#endif
                                      return rReturn;
                                  }
                              }
                              Logger.Error($"Don't find test result in csv,test fail!");
                          }
                          break;

                      case "BLE":
                          {
                              List<string> arrayListTemp = new List<string>();
                              List<string> arrayListTemp_yuqiang = new List<string>();
                              for (int i = 0; i < csvLines.Length; i++)
                              {
                                  string[] temp = csvLines[i].Split(new char[] { ',' }, StringSplitOptions.None);
                                  //#if DEBUG  //获取CSV表头
                                  if (i == 0)
                                  {
                                      arrayListTemp.AddRange(new string[] {temp[56],temp[5], temp[7], temp[8], temp[9], temp[11], temp[12], temp[13], temp[15], temp[16],
                                          temp[17], temp[19], temp[20], temp[21], temp[23], temp[24], temp[25], temp[27], temp[28],
                                          temp[29], temp[31], temp[32], temp[33], temp[34], temp[36], temp[37], temp[38], temp[40], temp[41],
                                          temp[42], temp[44], temp[45], temp[46], temp[48], temp[50], temp[51], temp[52], temp[54], temp[55]
                                      });
                                      arrayListTemp_yuqiang.AddRange(new string[] { temp[33], temp[46], temp[48] });
                                  }
                                  //#endif
                                  if (temp[0] == item.TestKeyword && temp[1] == item.SubStr1 && temp[3] == item.ComdSend)
                                  {
                                      Logger.Info($"find test result in csv line{i + 1}.testResult={temp[52]}");
                                      phaseItem.frequency = item.ComdSend;
                                      phaseItem.delta_f0_fn_max = temp[17] == "NA" ? "" : $"{temp[17]}";
                                      phaseItem.delta_f1_f0 = temp[21] == "NA" ? "" : $"{temp[21]}";
                                      phaseItem.delta_f1_avg = temp[38] == "NA" ? "" : $"{temp[38]}";
                                      phaseItem.delta_f2_avg = temp[9] == "NA" ? "" : $"{temp[9]}";
                                      phaseItem.delta_f2_max = temp[13] == "NA" ? "" : $"{temp[13]}";
                                      phaseItem.delta_fn_fn5_max = temp[25] == "NA" ? "" : $"{temp[25]}";
                                      phaseItem.fn_max = temp[29] == "NA" ? "" : $"{temp[29]}";
                                      phaseItem.ini_freq_error = temp[5] == "NA" ? "" : $"{temp[5]}";
                                      phaseItem.per_test_power = temp[46] == "NA" ? "" : $"{temp[46]}";
                                      phaseItem.power = temp[33] == "NA" ? "" : $"{temp[33]}";
                                      phaseItem.power_spec = temp[34] == "NA" ? "" : $"{temp[34]}";
                                      phaseItem.ratio_of_f2_to_f1 = temp[42] == "NA" ? "" : $"{temp[42]}";
                                      phaseItem.rx_per = temp[48] == "NA" ? "" : $"{temp[48]}";
                                      testPhase.phase_items.Add(phaseItem);
                                      rReturn = temp[56].ToLower() == "pass" ? true : false;
                                      if (!rReturn)
                                      {
                                          ErrorList = temp[2].Trim().Split(new string[] { "\n" }, 0);
                                      }

                                      //#if DEBUG  //生成上传SFTP的CSV表头
                                      string newitemName = "";
                                      string newitem = "";
                                      foreach (var items in arrayListTemp)
                                      {
                                          if (!items.Contains("limit_min") && !items.Contains("limit_max"))
                                          {
                                              newitem = $"{item.ItemName}_{items.Trim()}";
                                              newitemName = newitem;
                                          }
                                          else
                                          {
                                              newitem = $"{newitemName}_{items.Trim()}";
                                          }
                                          ArrayListCsvHeader.Add(newitem.ToUpper());
                                      }
                                      // for yuqiang
                                      foreach (var items in arrayListTemp_yuqiang)
                                      {
                                          if ((item.SubStr1 == "Tx" && items.Trim() == "Power"))
                                          {
                                              newitem = $"{item.ItemName}_{items.Trim()}";
                                              ArrayListCsvHeader_yuqiang.Add(newitem.ToUpper());
                                          }
                                          else if (item.SubStr1 == "Rx" && (items.Trim() == "Rx_Power" || items.Trim() == "RxPER"))
                                          {
                                              newitem = $"{item.ItemName}_{items.Trim()}";
                                              ArrayListCsvHeader_yuqiang.Add(newitem.ToUpper());
                                          }
                                          else
                                          {

                                          }
                                      }
                                      //#else
                                      //$"{temp[0]}_{temp[1]}_{temp[3]}",
                                      //Ini_Freq_Error		limit_min	limit_max,	Delta_F2_Avg		limit_min	limit_max,	Delta_F2_Max		limit_min	limit_max,
                                      //Delta_F0_Fn_Max		limit_min	limit_max,	Delta_F1_F0		limit_min	limit_max,	Delta_Fn_Fn5_Max		limit_min	limit_max,
                                      //Fn_Max		limit_min	limit_max,	Power	power_spec		limit_min	limit_max,	Delta_F1_Avg		limit_min	limit_max,
                                      //Ratio_of_F2_To_F1		limit_min	limit_max,	Rx_Power		RxPER		limit_min	limit_max,	Test_Result,
                                      ArrayListCsv.AddRange(new string[] {temp[56].ToLower()=="pass"?"TRUE":"FALSE", temp[5], temp[7], temp[8], temp[9], temp[11], temp[12], temp[13], temp[15], temp[16],
                                          temp[17], temp[19], temp[20], temp[21], temp[23], temp[24], temp[25], temp[27], temp[28],
                                          temp[29], temp[31], temp[32], temp[33], temp[34], temp[36], temp[37], temp[38], temp[40], temp[41],
                                          temp[42], temp[44], temp[45], temp[46], temp[48], temp[50], temp[51], temp[52], temp[54], temp[55]
                                      });

                                      // for yuqiang
                                      if (temp[33] != "NA")
                                      {
                                          ArrayListCsv_yuqiang.Add(temp[33]);
                                      }
                                      if (temp[46] != "NA")
                                      {
                                          ArrayListCsv_yuqiang.Add(temp[46]);
                                      }
                                      if (temp[48] != "NA")
                                      {
                                          ArrayListCsv_yuqiang.Add(temp[48]);
                                      }

                                      //#endif

                                      return rReturn;
                                  }
                              }
                              Logger.Error($"Don't find test result in csv,test fail!");
                          }
                          break;

                      case "2G":
                      case "5G"://SRF
                          {
                              List<string> arrayListTemp = new List<string>();
                              List<string> arrayListTemp_yuqiang = new List<string>();
                              List<string> arrayListTemp_loss = new List<string>();
                              for (int i = 0; i < csvLines.Length; i++)
                              {
                                  string[] temp = csvLines[i].Split(new char[] { ',' }, StringSplitOptions.None);
                                  //#if DEBUG  //获取CSV表头
                                  if (i == 0)
                                  {
                                      arrayListTemp.AddRange(new string[] { temp[15], temp[6], temp[7], temp[9], temp[10], temp[11], temp[12], temp[13] });
                                      arrayListTemp_yuqiang.AddRange(new string[] { temp[7], temp[12], temp[13] });
                                      arrayListTemp_loss.AddRange(new string[] { temp[6], temp[7], temp[11] });
                                  }
                                  //#endif
                                  if (temp[0] == item.TestKeyword && temp[1] == item.SubStr1 && temp[4] == item.SubStr2 && temp[3] == item.ComdSend)
                                  {
                                      Logger.Info($"find test result in csv line{i + 1}.testResult={temp[15]}");
                                      phaseItem.radio = item.TestKeyword == "2G" ? "0" : "1";
                                      phaseItem.chain = item.SubStr2;
                                      phaseItem.frequency = item.ComdSend;
                                      if (item.SubStr1.ToLower() == "tx")
                                      {
                                          phaseItem.measured_power = temp[7] == "NA" ? "" : $"{temp[7]}";       //Measure_Power
                                          phaseItem.absolute_power = temp[6] == "NA" ? "" : $"{temp[6]}";       //Absolute_Power
                                          phaseItem.path_loss = temp[11] == "NA" ? "" : $"{temp[11]}";          //Path_Loss
                                          phaseItem.unit = temp[8] == "NA" ? "" : $"{temp[8]}";
                                      }
                                      else if (item.SubStr1.ToLower() == "rx")
                                      {
                                          phaseItem.rx_power = temp[12] == "NA" ? "" : $"{temp[12]}";         //Rx_Level,rx_power
                                          phaseItem.per = temp[13] == "NA" ? "" : $"{temp[13]}";              //PER
                                          phaseItem.path_loss = temp[11] == "NA" ? "" : $"{temp[11]}";        //Path_Loss
                                          phaseItem.unit = "percent";
                                      }
                                      phaseItem.limit_min = temp[9] == "NA" ? "" : $"{temp[9]}";
                                      phaseItem.limit_max = temp[10] == "NA" ? "" : $"{temp[10]}";
                                      testPhase.phase_items.Add(phaseItem);
                                      rReturn = temp[15].ToLower() == "pass" ? true : false;
                                      if (!rReturn)
                                      {
                                          ErrorList = temp[2].Trim().Split(new string[] { "\n" }, 0);
                                      }
                                      //#if DEBUG  //生成上传SFTP的CSV表头
                                      string newitemName = "";
                                      string newitem = "";
                                      foreach (var items in arrayListTemp)
                                      {
                                          if (!items.Contains("limit_min") && !items.Contains("limit_max"))
                                          {
                                              newitem = $"{item.ItemName}_{items.Trim()}";
                                              newitemName = newitem;
                                          }
                                          else
                                          {
                                              newitem = $"{newitemName}_{items.Trim()}";
                                          }
                                          ArrayListCsvHeader.Add(newitem.ToUpper());
                                      }

                                      // yuqiang
                                      foreach (var items in arrayListTemp_yuqiang)
                                      {
                                          if ((item.SubStr1 == "Tx" && items.Trim() == "Measure_Power"))
                                          {
                                              newitem = $"{item.ItemName}_{items.Trim()}";
                                              ArrayListCsvHeader_yuqiang.Add(newitem.ToUpper());
                                          }
                                          else if (item.SubStr1 == "Rx" && (items.Trim() == "Rx_Level" || items.Trim() == "PER"))
                                          {
                                              newitem = $"{item.ItemName}_{items.Trim()}";
                                              ArrayListCsvHeader_yuqiang.Add(newitem.ToUpper());
                                          }
                                          else
                                          {

                                          }
                                      }

                                      // for loss
                                      foreach (var items in arrayListTemp_loss)
                                      {
                                          if (item.SubStr1 == "Tx" && (items.Trim() == "Absolute_Power" || items.Trim() == "Measure_Power" || items.Trim() == "Path_Loss"))
                                          {
                                              newitem = $"{item.ItemName}_{items.Trim()}";
                                              ArrayListCsvHeader_loss.Add(newitem.ToUpper());
                                          }
                                      }
                                      //#else
                                      //$"{temp[3]}_{temp[1]}_{temp[4]}",
                                      //Absolute_Power	Measure_Power		limit_min	limit_max	Path_Loss	Rx_Level	PER	unit	Test_Result
                                      ArrayListCsv.AddRange(new string[] { temp[15].ToLower() == "pass" ? "TRUE" : "FALSE", temp[6], temp[7], temp[9], temp[10], temp[11], temp[12], temp[13] });

                                      // for yuqiang
                                      if (temp[7] != "NA")
                                      {
                                          ArrayListCsv_yuqiang.Add(temp[7]);
                                      }
                                      if (temp[12] != "NA")
                                      {
                                          ArrayListCsv_yuqiang.Add(temp[12]);
                                      }
                                      if (temp[13] != "NA")
                                      {
                                          ArrayListCsv_yuqiang.Add(temp[13]);
                                      }

                                      // for loss
                                      if (item.SubStr1 == "Tx")
                                      {
                                          if (temp[6] != "NA")
                                              ArrayListCsv_loss.Add(temp[6]);
                                          if (temp[7] != "NA")
                                              ArrayListCsv_loss.Add(temp[7]);
                                          if (temp[11] != "NA")
                                              ArrayListCsv_loss.Add(temp[11]);
                                      }
                                      //#endif
                                      return rReturn;
                                  }
                              }
                              Logger.Error($"Don't find test result in csv,test fail!");
                          }
                          break;

                      case "5G_Calibration":
                      case "2G_Calibration":
                          {
                              List<string> arrayListTemp = new List<string>();
                              /// MBFT
                              for (int i = 0; i < csvLines.Length; i++)
                              {
                                  string[] temp = csvLines[i].Split(new char[] { ',' }, StringSplitOptions.None);
  #if DEBUG  //获取CSV表头
                                  if (i == 0)
                                  {
                                      arrayListTemp.AddRange(new string[] { temp[46], temp[47], temp[48], temp[49] });
                                  }
  #endif
                                  if (temp[0] == item.TestKeyword.Substring(0, 2) && temp[1] == item.SubStr1 && temp[4] == item.SubStr2 && temp[5] == item.ComdSend)
                                  {
                                      Logger.Info($"find test result in csv line{i + 1}.testResult={temp[46]}");
                                      phaseItem.radio = temp[0] == "5G" ? "1" : "0";
                                      phaseItem.chain = temp[4] == "NA" ? "" : $"{temp[4]}";
                                      phaseItem.frequency = temp[5] == "NA" ? "" : $"{temp[5]}";
                                      phaseItem.goal_power = temp[47] == "NA" ? "" : $"{temp[47]}";
                                      phaseItem.measured_power = temp[48] == "NA" ? "" : $"{temp[48]}";
                                      phaseItem.gain = temp[49] == "NA" ? "" : $"{temp[49]}";
                                      testPhase.phase_items.Add(phaseItem);
                                      rReturn = temp[46].ToLower() == "pass" ? true : false;
                                      if (!rReturn)
                                      {
                                          ErrorList = temp[3].Trim().Split(new string[] { "\n" }, 0);
                                      }

  #if DEBUG  //生成上传SFTP的CSV表头
                                      string newitemName = "";
                                      string newitem = "";
                                      foreach (var items in arrayListTemp)
                                      {
                                          if (!items.Contains("limit_min") && !items.Contains("limit_max"))
                                          {
                                              newitem = $"{item.ItemName}_{items.Trim()}";
                                              newitemName = newitem;
                                          }
                                          else
                                          {
                                              newitem = $"{newitemName}_{items.Trim()}";
                                          }
                                          ArrayListCsv.Add(newitem.ToUpper());
                                      }
  #else
                                      //$"{temp[5]}_{temp[1]}_{temp[4]}",
                                      //Test_Result	Goal	MeasurePower	Gain
                                      ArrayListCsv.AddRange(new string[] { temp[46].ToLower() == "pass" ? "TRUE" : "FALSE", temp[47], temp[48], temp[49] });
  #endif
                                      return rReturn;
                                  }
                              }
                              Logger.Error($"Don't find test result in csv,test fail!");
                          }
                          break;

                      case "5G_RadioValidation":
                      case "2G_RadioValidation":
                          {
                              /// MBFT
                              List<string> arrayListTemp = new List<string>();
                              for (int i = 0; i < csvLines.Length; i++)
                              {
                                  string[] temp = csvLines[i].Split(new char[] { ',' }, StringSplitOptions.None);
  #if DEBUG  //获取CSV表头
                                  if (i == 0)
                                  {
                                      arrayListTemp.AddRange(new string[] {temp[46], temp[8], temp[10], temp[11],
                                          temp[12],temp[14],temp[15], temp[16],temp[18],temp[19],temp[20],temp[21],temp[23],temp[24],
                                          temp[25],temp[27],temp[28], temp[29],temp[31],temp[32],temp[33],temp[35],temp[36],temp[37],temp[39],temp[40], temp[41],
                                      });
                                  }
  #endif
                                  if (temp[0] == item.TestKeyword.Substring(0, 2) && temp[1] == item.SubStr1 && temp[4] == item.SubStr2 && temp[5].Trim() == item.ComdSend && temp[6].Trim() == item.ExpectStr)
                                  {
                                      Logger.Info($"find test result in csv line{i + 1}.testResult={temp[46]}");
                                      phaseItem.radio = temp[0] == "5G" ? "1" : "0";
                                      phaseItem.chain = temp[4] == "NA" ? "" : $"{temp[4]}";
                                      phaseItem.frequency = temp[5] == "NA" ? "" : $"{temp[5].Trim()}";
                                      phaseItem.data_rate = temp[6] == "NA" ? "" : $"{temp[6].Trim()}";
                                      phaseItem.evm = temp[8] == "NA" ? "" : $"{temp[8]}";
                                      phaseItem.freq_error = temp[12] == "NA" ? "" : $"{temp[12]}";
                                      phaseItem.lo_leakage = temp[25] == "NA" ? "" : $"{temp[25]}";
                                      phaseItem.per_test_power = temp[41] == "NA" ? "" : $"{temp[41]}";
                                      phaseItem.power_accuracy = temp[20] == "NA" ? "" : $"{temp[20]}";
                                      phaseItem.power_spec = temp[21] == "NA" ? "" : $"{temp[21]}";
                                      phaseItem.rx_per = temp[37] == "NA" ? "" : $"{temp[37]}";
                                      phaseItem.spectral_flatness = temp[29] == "NA" ? "" : $"{temp[29]}";
                                      phaseItem.spectrum_mask = temp[33] == "NA" ? "" : $"{temp[33]}";
                                      phaseItem.sym_clk_error = temp[16] == "NA" ? "" : $"{temp[16]}";
                                      testPhase.phase_items.Add(phaseItem);
                                      rReturn = temp[46].ToLower() == "pass" ? true : false;
                                      if (!rReturn)
                                      {
                                          ErrorList = temp[3].Trim().Split(new string[] { "\n" }, 0);
                                      }

  #if DEBUG  //生成上传SFTP的CSV表头
                                      string newitemName = "";
                                      string newitem = "";
                                      foreach (var items in arrayListTemp)
                                      {
                                          // string newitem = $"{temp[0].Trim()}_{temp[1].Trim()}_{temp[3].Trim()}_{items.Trim()}";
                                          if (!items.Contains("limit_min") && !items.Contains("limit_max"))
                                          {
                                              newitem = $"{item.ItemName}_{items.Trim()}";
                                              newitemName = newitem;
                                          }
                                          else
                                          {
                                              newitem = $"{newitemName}_{items.Trim()}";
                                          }
                                          ArrayListCsv.Add(newitem.ToUpper());
                                      }
  #else
                                      // $"{temp[5]}_{temp[1]}_{temp[4]}_{temp[6]}",
                                      //EVM	limit_min	limit_max
                                      //FrequencyOffset		limit_min	limit_max,	SymClkError	unit	limit_min	limit_max, PowerAccuracy	Power_Spec	limit_min	limit_max,
                                      //LOLeakage		limit_min	limit_max,	SpectralFlatness		limit_min	limit_max,	SpectrumMask		limit_min	limit_max, RxPER		limit_min	limit_max,	PERTestPower
                                      ArrayListCsv.AddRange(new string[] {temp[46].ToLower()=="pass"?"TRUE":"FALSE", temp[8], temp[10], temp[11],
                                          temp[12],temp[14],temp[15], temp[16],temp[18],temp[19],temp[20],temp[21],temp[23],temp[24],
                                          temp[25],temp[27],temp[28], temp[29],temp[31],temp[32],temp[33],temp[35],temp[36],temp[37],temp[39],temp[40], temp[41],
                                      });

  #endif
                                      return rReturn;
                                  }
                              }
                              Logger.Error($"Don't find test result in csv,test fail!");
                          }
                          break;

                      case "RunDosCmdParallel":
                          {
                              dosCmd.SendCommand3(item.ComdSend);
                              return rReturn = true;
                          }
                          break;

                      case "IperfWiFiSpeedTest":
                      case "IperfThroughput":
                      case "IperfTest":
                          //KillProcess("iperf3");
                          {
                              string revStr = "";
                              inPutValue = "";
                              if (dosCmd.SendCommand(item.ComdSend, ref revStr, item.ExpectStr, short.Parse(item.TimeOut))
                                  && revStr.CheckStr(item.CheckStr1) && revStr.CheckStr(item.CheckStr2))
                              {
                                  string[] lines = revStr.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                  foreach (var line in lines)
                                  {
                                      if (line.Contains(item.CheckStr1) && line.Contains(item.CheckStr2))
                                      {
                                          item.TestValue = GetValue(line, item.SubStr1, item.SubStr2);

                                          if (!string.IsNullOrEmpty(item.TestValue) && item.TestValue.EndsWith("G"))
                                          {
                                              string tempValue = item.TestValue.Replace("G", "").Trim();
                                              item.TestValue = ((double.Parse(tempValue)) * 1000).ToString();
                                          }
                                          else if (!string.IsNullOrEmpty(item.TestValue) && item.TestValue.EndsWith("M"))
                                          {
                                              item.TestValue = (double.Parse(item.TestValue.Replace("M", "").Trim())).ToString();
                                          }
                                          else
                                          {
                                              Logger.Debug("Get Speed error!");
                                          }

                                          rReturn = CompareLimit(item.Limit_min, item.Limit_max, item.TestValue, out info);
                                          break;
                                      }
                                  }
                              }
                          }
                          break;

                      case "TempSensorTest_AfterWiFiTXPowerTest":
                          for (int i = 0; i < csvLines.Length; i++)
                          {
                              string[] temp = csvLines[i].Split(new char[] { ',' }, StringSplitOptions.None);
                              if (temp[0].Contains(item.TestKeyword.Substring(item.TestKeyword.IndexOf("_"))))
                              {
                                  item.TestValue = Math.Round(double.Parse(temp[1])).ToString();
                                  //item.TestValue = temp[1];
                                  Logger.Debug($"Get tempSensorTest:{temp[0]},{temp[1]}");
                                  // 需要比较Limit
                                  if (!String.IsNullOrEmpty(item.Limit_min) || !String.IsNullOrEmpty(item.Limit_max))
                                  {
                                      rReturn = CompareLimit(item.Limit_min, item.Limit_max, item.TestValue, out info);
                                  }
                                  //#if DEBUG
                                  ArrayListCsvHeader.AddRange(new string[] { $"TEMP_WIFI_TXPOWER", "TEMP_WIFI_TXPOWER_limit_min".ToUpper(), "TEMP_WIFI_TXPOWER_limit_max".ToUpper() });

                                  //#else
                                  ArrayListCsv.AddRange(new string[] { item.TestValue, "0", "100" });

                                  //#endif
                              }
                          }
                          break;

                      case "TempSensorTest_AfterDesenseWiFiTest":
                          for (int i = 0; i < csvLines.Length; i++)
                          {
                              string[] temp = csvLines[i].Split(new char[] { ',' }, StringSplitOptions.None);
                              if (temp[0].Contains("AfterDesense"))
                              {
                                  item.TestValue = Math.Round(double.Parse(temp[1])).ToString();
                                  //item.TestValue = temp[1];
                                  Logger.Debug($"Get tempSensorTest:{temp[0]},{temp[1]}");
                                  // 需要比较Limit
                                  if (!String.IsNullOrEmpty(item.Limit_min) || !String.IsNullOrEmpty(item.Limit_max))
                                  {
                                      rReturn = CompareLimit(item.Limit_min, item.Limit_max, item.TestValue, out info);
                                  }
                                  //#if DEBUG
                                  ArrayListCsvHeader.AddRange(new string[] { $"TEMP_WIFI_RXPER", "TEMP_WIFI_RXPER_limit_min".ToUpper(), "TEMP_WIFI_RXPER_limit_max".ToUpper() });
                                  //#else
                                  ArrayListCsv.AddRange(new string[] { item.TestValue, "0", "100" });
                                  //#endif

                              }
                          }
                          break;
                      case "SampleTelnetCmd":
                          {
                              string revStr = "";
                              if (SampleComm.SendCommand(item.ComdSend, ref revStr, item.ExpectStr, short.Parse(item.TimeOut))
                                  && revStr.CheckStr(item.CheckStr1) && revStr.CheckStr(item.CheckStr2))
                              {
                                  rReturn = true;
                              }
                          }
                          break;

                      case "RunDosCmd":
                          {
                              var revStr = "";
                              if (dosCmd.SendCommand(item.ComdSend, ref revStr, item.ExpectStr, short.Parse(item.TimeOut))
                                  && revStr.CheckStr(item.CheckStr1) && revStr.CheckStr(item.CheckStr2))
                              {
                                  rReturn = true;
                                  // 需要提取测试值
                                  if (!string.IsNullOrEmpty(item.SubStr1) || !string.IsNullOrEmpty(item.SubStr2))
                                  {
                                      item.TestValue = GetValue(revStr, item.SubStr1, item.SubStr2);
                                  }
                                  else
                                  {
                                      return rReturn = true;
                                  }
                                  // 需要比较Spec
                                  if (!String.IsNullOrEmpty(item.Spec))
                                  {
                                      rReturn = CheckSpec(item.Spec, item.TestValue);
                                  }
                                  // 需要比较Limit
                                  if (!String.IsNullOrEmpty(item.Limit_min) || !String.IsNullOrEmpty(item.Limit_max))
                                  {
                                      rReturn = CompareLimit(item.Limit_min, item.Limit_max, item.TestValue, out info);
                                  }
                              }
                          }
                          break;

                      default:
                          {
                              //Logger.Warn($"Warning!!!,this is default DUT test-method, ErrorList.Length is {ErrorList.Length.ToString()}");
                              var revStr = "";
                              inPutValue = "";

                              if (DUTCOMM.SendCommand(item.ComdSend, ref revStr, item.ExpectStr, short.Parse(item.TimeOut))
                                  && revStr.CheckStr(item.CheckStr1) && revStr.CheckStr(item.CheckStr2))
                              {
                                  rReturn = true;
                                  // 需要提取测试值
                                  if (!string.IsNullOrEmpty(item.SubStr1) || !string.IsNullOrEmpty(item.SubStr2))
                                  {
                                      item.TestValue = GetValue(revStr, item.SubStr1, item.SubStr2);
                                  }
                                  else
                                  {
                                      return rReturn = true;
                                  }

                                  if (item.TestKeyword.Contains("TempSensorTest_After"))
                                  {
                                      if (string.IsNullOrEmpty(item.TestValue))
                                      {
                                          item.TestValue = "0";
                                          return rReturn = false;
                                      }
                                      else
                                      {
                                          item.TestValue = Math.Round(double.Parse(item.TestValue)).ToString(); //取整
                                      }
                                  }
                                  // 需要比较Spec
                                  if (!string.IsNullOrEmpty(item.Spec) && string.IsNullOrEmpty(item.Limit_min) && string.IsNullOrEmpty(item.Limit_max))
                                  {
                                      return rReturn = CheckSpec(item.Spec, item.TestValue);
                                  }

                                  // 需要比较Limit
                                  if (!string.IsNullOrEmpty(item.Limit_min) || !string.IsNullOrEmpty(item.Limit_max))
                                  {
                                      rReturn = CompareLimit(item.Limit_min, item.Limit_max, item.TestValue, out info);
                                  }
                              }

                              if (item.TestKeyword == "QorvoBLEPeripheral")
                              {
                                  BtDevAddress = item.TestValue.Trim();
                              }
                          }
                          break;
                  }
              }
              catch (Exception ex) when (ex.Message.Contains("正在中止线程"))
              {
                  //abort线程忽略报错
                  Logger.Warn(ex.Message);
                  return false;
              }
              catch (Exception ex)
              {
                  //Logger.Fatal("TestStep test Exception!!! return fail.");
                  Logger.Fatal(ex.ToString());
                  rReturn = false;
              }
              finally
              {
                  // finally请不要给rReturn赋值,不生效！！，return true/false会提前返回，不执行最后的return rReturn;。

                  // 设置错误码
                  if ((retryTimes == 0 && !rReturn) && (IsNullOrEmpty(error_code) && IsNullOrEmpty(error_details)))
                  {
                      Logger.Debug($"ErrorList.length {ErrorList.Length},{ErrorList[0].ToString()}");
                      if (ErrorList.Length > 1 && info == "TooHigh") // TooHigh
                      {
                          error_code = ErrorList[1].Split(':')[0].Trim();
                          error_details = ErrorList[1].Split(':')[1].Trim();
                      }
                      else
                      {
                          error_code = ErrorList[0].Split(':')[0].Trim();
                          error_details = ErrorList[0].Split(':')[1].Trim();
                      }
                      testPhase.phase_details = error_details;
                      testPhase.error_code = error_code;
                  }

                  if (retryTimes == 0 || rReturn)
                  {
                      item.ElapsedTime = $"{Convert.ToDouble((DateTime.Now - item.startTime).TotalSeconds),0:F1}";
                      if (item.TestKeyword != "Wait" && item.TestKeyword != "ThreadSleep")
                      {
                          if (rReturn)
                              Logger.Info($"{item.ItemName} {(rReturn ? "PASS" : "FAIL")}!! ElapsedTime:{item.ElapsedTime},{error_code}:{error_details},Spec:{item.Spec},Min:{item.Limit_min},Value:{item.TestValue},Max:{item.Limit_max}");
                          else
                              Logger.Error($"{item.ItemName} {(rReturn ? "PASS" : "FAIL")}!! ElapsedTime:{item.ElapsedTime},{error_code}:{error_details},Spec:{item.Spec},Min:{item.Limit_min},Value:{item.TestValue},Max:{item.Limit_max}");
                          MainForm.f1.UpdateDetailView(SN, item.ItemName, item.Spec, item.Limit_min, item.TestValue, item.Limit_max, item.ElapsedTime, item.startTime.ToString(), rReturn.ToString() == "True" ? "Pass" : "Fail");
                      }

                      // 给Json格式对象赋值
                      if (item.TestKeyword.Contains("VeritfyDUTSN"))
                      {
                          phaseItem.serial = item.TestValue;
                          phaseItem.mac = MesMac;
                          testPhase.phase_items.Add(phaseItem);
                      }
                      else if (item.TestKeyword == "Veritfy_QSDK_Version")
                      {
                          MainForm.f1.Station.luxshare_qsdk_version = item.TestValue;
                      }
                      else if (item.TestKeyword == "ReadBoardRegisterValue")
                      {
                          phaseItem.board_register_value = item.TestValue ?? "";
                          testPhase.phase_items.Add(phaseItem);
                      }
                      else if (item.ItemName.Contains("ReadBTZFwVersion") || item.TestKeyword.Contains("CheckBZTBootloaderVersion") || item.TestKeyword.Contains("CPUVersionTest"))
                      {
                          phaseItem.version = item.TestValue ?? "";
                          testPhase.phase_items.Add(phaseItem);
                      }
                      else if (item.TestKeyword.Contains("TempSensorTest"))
                      {
                          phaseItem.Copy(item);
                          testPhase.phase_items.Add(phaseItem);
                      }
                      else if (item.TestKeyword == "MMCWrite" || item.TestKeyword == "MMCRead")
                      {
                          phaseItem.device = "MMC";
                          phaseItem.test = item.ItemName.Contains("MMCWrite") ? "write" : "read";
                          phaseItem.speed = item.TestValue ?? "-1";
                          phaseItem.unit = "Mbps";
                          phaseItem.limit_min = item.Limit_min;
                          phaseItem.limit_max = item.Limit_max;
                          testPhase.phase_items.Add(phaseItem);
                      }
                      else if (item.TestKeyword == "IperfThroughput")
                      {
                          phaseItem.device = item.ItemName.Contains("Eth0") ? "eth0" : "eth1";
                          phaseItem.test = item.ItemName.Contains("send") ? "send" : "receive";
                          phaseItem.speed = item.TestValue ?? "-1";
                          phaseItem.unit = "Mbps";
                          phaseItem.limit_min = item.Limit_min;
                          phaseItem.limit_max = item.Limit_max;
                          testPhase.phase_items.Add(phaseItem);
                      }
                      else if (item.TestKeyword == "IperfWiFiSpeedTest")
                      {
                          phaseItem.radio = item.ItemName.Contains("WIFI2G") ? "0" : "1";
                          phaseItem.frequency = item.ItemName.Contains("WIFI2G") ? "2412" : "5180";
                          phaseItem.performance = item.TestValue ?? "-1"; ;
                          phaseItem.iperf_cmd = item.ItemName.Contains("serial") ? "serial" : "parallel";
                          phaseItem.unit = item.unit;
                          phaseItem.limit_min = item.Limit_min;
                          phaseItem.limit_max = item.Limit_max;
                          testPhase.phase_items.Add(phaseItem);
                      }
                      else if (item.TestKeyword == "CurrentTest")
                      {
                          phaseItem.name = item.ItemName;
                          phaseItem.value = item.TestValue ?? "-1";
                          phaseItem.voltage = "5";
                          phaseItem.unit = item.unit;
                          phaseItem.limit_min = item.Limit_min;
                          phaseItem.limit_max = item.Limit_max;
                          testPhase.phase_items.Add(phaseItem);
                      }
                      else if (item.TestKeyword == "VoltageTest")
                      {
                          phaseItem.name = item.ItemName;
                          phaseItem.value = item.TestValue ?? "-1";
                          phaseItem.voltage = item.Spec;
                          phaseItem.unit = item.unit;
                          phaseItem.limit_min = item.Limit_min;
                          phaseItem.limit_max = item.Limit_max;
                          testPhase.phase_items.Add(phaseItem);
                      }
                      else if (item.TestKeyword == "LEDTest")
                      {
                          phaseItem.led = item.ItemName.Substring(0, 1);
                          phaseItem.name = item.ItemName.Substring(2);
                          phaseItem.value = item.TestValue ?? "-1";
                          phaseItem.limit_min = item.Limit_min;
                          phaseItem.limit_max = item.Limit_max;
                          testPhase.phase_items.Add(phaseItem);
                      }


                      // 用反射的方法给mesPhases变量赋值
                      if (IsNullOrEmpty(item.SubStr1) && IsNullOrEmpty(item.SubStr2) && !IsNullOrEmpty(item.MES_var))
                      {
                          //没有测试值则赋值测试结果给变量
                          SetVerReflection(mesPhases, item.MES_var, rReturn.ToString().ToUpper());
                      }
                      else
                      {
                          SetVerReflection(mesPhases, item.MES_var, item.TestValue);
                      }

                      //if (item.ItemName == "SetIpaddrEnv" && rReturn)
                      //{
                      //    SetIPflag = true;
                      //}
                      if (item.TestKeyword == "SetIpaddrEnv" && rReturn)
                      {
                          SetIPflag = true;
                          testPhase.phase_details = DUTMesIP;
                      }
                  }

              }
            */
            return true;
           
        }
    }

    //case "CopyFile":
    //    {
    //        if (File.Exists(item.ComdSend))
    //        {
    //            File.Move(item.ComdSend, item.ExpectStr);
    //            rReturn = true;
    //        }
    //    }
    //    break;
    //case "SSHLogin":
    //    {
    //        DUTCOMM = new SSH(sshconInfo);
    //        rReturn = DUTCOMM.Open();
    //    }
    //    break;
    //case "GBIP":
    //    using (GPIB GPIBCOMM = new GPIB(GpibInfo))
    //    {
    //        GPIBCOMM.Open();
    //        string revStr = "";
    //        if (GPIBCOMM.SendCommand(item.ComdSend, ref revStr, item.ExpectStr, Int16.Parse(item.TimeOut))
    //            && revStr.CheckStr(item.CheckStr1) && revStr.CheckStr(item.CheckStr2))
    //        {
    //            rReturn = true;
    //        }
    //        GPIBCOMM.Close();
    //    }
    //    break;

    //case "DUTCOMSend":
    //    {
    //        using (DUTCOMM = new Comport(DUTCOMinfo))
    //        {
    //            DUTCOMM.Open();
    //            string revStr = "";
    //            inPutValue = "";
    //            if (DUTCOMM.SendCommand(item.ComdSend, ref revStr, item.ExpectStr, Int16.Parse(item.TimeOut))
    //           && revStr.CheckStr(item.CheckStr1) && revStr.CheckStr(item.CheckStr2))
    //            {
    //                rReturn = true;
    //                // 需要提取测试值
    //                if (!string.IsNullOrEmpty(item.SubStr1) || !string.IsNullOrEmpty(item.SubStr2))
    //                {
    //                    item.TestValue = GetValue(revStr, item.SubStr1, item.SubStr2);
    //                }
    //                else
    //                {
    //                    return rReturn = true;
    //                }

    //                if (item.ItemName.Contains("TempSensorTest_After"))
    //                {
    //                    if (String.IsNullOrEmpty(item.TestValue))
    //                    {
    //                        item.TestValue = "0";
    //                        return rReturn = false;
    //                    }
    //                    else
    //                    {
    //                        item.TestValue = Math.Round(double.Parse(item.TestValue)).ToString(); //取整
    //                    }
    //                }
    //                // 需要比较Spec
    //                if (!String.IsNullOrEmpty(item.Spec))
    //                {
    //                    rReturn = CheckSpec(item.Spec, item.TestValue);
    //                }

    //                // 需要比较Limit
    //                if (!String.IsNullOrEmpty(item.Limit_min) || !String.IsNullOrEmpty(item.Limit_max))
    //                {
    //                    rReturn = CompareLimit(item.Limit_min, item.Limit_max, item.TestValue, out info);
    //                }
    //            }
    //            DUTCOMM.Close();
    //        }
    //    }
    //    break;
    //case "SSHCommand":
    //    {
    //        using (DUTCOMM = new SSH(sshconInfo))
    //        {
    //            DUTCOMM.Open();
    //            string revStr = "";
    //            inPutValue = "";

    //            if (DUTCOMM.SendCommand(item.ComdSend, ref revStr, item.ExpectStr, Int16.Parse(item.TimeOut))
    //            && revStr.CheckStr(item.CheckStr1) && revStr.CheckStr(item.CheckStr2))
    //            {
    //                rReturn = true;
    //                // 需要提取测试值
    //                if (!string.IsNullOrEmpty(item.SubStr1) || !string.IsNullOrEmpty(item.SubStr2))
    //                {
    //                    item.TestValue = GetValue(revStr, item.SubStr1, item.SubStr2);
    //                }
    //                else
    //                {
    //                    return rReturn = true;
    //                }

    //                // 需要比较Spec
    //                if (!String.IsNullOrEmpty(item.Spec))
    //                {
    //                    rReturn = CheckSpec(item.Spec, item.TestValue);
    //                }

    //                // 需要比较Limit
    //                if (!String.IsNullOrEmpty(item.Limit_min) || !String.IsNullOrEmpty(item.Limit_max))
    //                {
    //                    rReturn = CompareLimit(item.Limit_min, item.Limit_max, item.TestValue, out info);
    //                }
    //            }
    //            DUTCOMM.Close();
    //        }
    //    }
    //    break;

    //case "RunDOSCommd":
    //    {
    //        string revStr = RunDosCmd(item.ComdSend, out string errors, int.Parse(item.TimeOut) * 1000);
    //        //string revStr = RunDosCmd(item.ComdSend, out string errors, int.Parse(item.TimeOut));
    //        if (revStr.CheckStr(item.CheckStr1) && revStr.CheckStr(item.CheckStr2))
    //        {
    //            rReturn = true;
    //            // 需要提取测试值
    //            if (!string.IsNullOrEmpty(item.SubStr1) || !string.IsNullOrEmpty(item.SubStr2))
    //            {
    //                item.TestValue = GetValue(revStr, item.SubStr1, item.SubStr2);
    //            }
    //            else
    //            {
    //                return rReturn = true;
    //            }
    //            // 需要比较Spec
    //            if (!String.IsNullOrEmpty(item.Spec))
    //            {
    //                rReturn = CheckSpec(item.Spec, item.TestValue);
    //            }
    //            // 需要比较Limit
    //            if (!String.IsNullOrEmpty(item.Limit_min) || !String.IsNullOrEmpty(item.Limit_max))
    //            {
    //                rReturn = CompareLimit(item.Limit_min, item.Limit_max, item.TestValue, out info);
    //            }
    //        }
    //    }
    //    break;

    //case "IperfWiFiSpeedTest":
    //    KillProcess("iperf3");
    //    {
    //        string revStr = "";
    //        inPutValue = "";
    //        if (dosCmd.SendCommand(item.ComdSend, ref revStr, item.ExpectStr, short.Parse(item.TimeOut))
    //            && revStr.CheckStr(item.CheckStr1) && revStr.CheckStr(item.CheckStr2))
    //        {
    //            string[] lines = revStr.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    //            foreach (var line in lines)
    //            {
    //                if (line.Contains(item.CheckStr1) && line.Contains(item.CheckStr2))
    //                {
    //                    item.TestValue = GetValue(line, item.SubStr1, item.SubStr2);

    //                    if (!string.IsNullOrEmpty(item.TestValue) && item.TestValue.EndsWith("G"))
    //                    {
    //                        string tempValue = item.TestValue.Replace("G", "").Trim();
    //                        item.TestValue = ((double.Parse(tempValue)) * 1000).ToString();
    //                    }
    //                    else if (!string.IsNullOrEmpty(item.TestValue) && item.TestValue.EndsWith("M"))
    //                    {
    //                        item.TestValue = (double.Parse(item.TestValue.Replace("M", "").Trim())).ToString();
    //                    }
    //                    else
    //                    {
    //                        Logger.Debug("Get Speed error!");
    //                    }

    //                    rReturn = CompareLimit(item.Limit_min, item.Limit_max, item.TestValue, out info);
    //                    break;
    //                }
    //            }
    //        }
    //    }
    //    break;

    //case "IperfThroughput":
    //    KillProcess("iperf3");
    //    if (inPutValue == "")
    //    {
    //        var revStr = "";
    //        if (dosCmd.SendCommand(item.ComdSend, ref revStr, item.ExpectStr, short.Parse(item.TimeOut))
    //            && revStr.CheckStr(item.CheckStr1) && revStr.CheckStr(item.CheckStr2))
    //        {
    //            inPutValue = revStr;
    //        }
    //        else
    //        {
    //            return rReturn = false;
    //        }
    //    }
    //    {
    //        var revStr = inPutValue;
    //        string[] lines = revStr.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    //        foreach (var line in lines)
    //        {
    //            //if (item.ItemName.Contains("send") && line.Contains("[SUM]") && line.Contains("sender"))
    //            if (line.Contains(item.CheckStr1) && line.Contains(item.CheckStr2))
    //            {
    //                item.TestValue = GetValue(line, item.SubStr1, item.SubStr2);
    //                //[SUM]   0.00-9.99   sec  2.41 GBytes  2.07 Gbits/sec    0             sender
    //                if (!string.IsNullOrEmpty(item.TestValue) && item.TestValue.EndsWith("G"))
    //                {
    //                    string tempValue = item.TestValue.Replace("G", "").Trim();
    //                    item.TestValue = ((double.Parse(tempValue)) * 1000).ToString();
    //                }
    //                else if (!string.IsNullOrEmpty(item.TestValue) && item.TestValue.EndsWith("M"))
    //                {
    //                    item.TestValue = (double.Parse(item.TestValue.Replace("M", "").Trim())).ToString();
    //                }
    //                else
    //                {
    //                    Logger.Debug("Get Speed error!");
    //                }
    //                rReturn = CompareLimit(item.Limit_min, item.Limit_max, item.TestValue, out info);
    //                break;
    //            }
    //        }
    //    }
    //    break;
    //KillProcess("iperf3");
    //{
    //    delegate void SaveTestResult();
    //    SaveTestResult saveTestResult;
    //}
    //if (inPutValue == "")
    //{
    //    var revStr = "";
    //    if (dosCmd.SendCommand3(item.ComdSend, ref revStr, item.ExpectStr, short.Parse(item.TimeOut))
    //        && revStr.CheckStr(item.CheckStr1) && revStr.CheckStr(item.CheckStr2))
    //    {
    //        inPutValue = revStr;
    //    }
    //    else
    //    {
    //        return rReturn = false;
    //    }
    //}
    //{
    //    var revStr = inPutValue;
    //    string[] lines = revStr.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    //    foreach (var line in lines)
    //    {
    //        //if (item.ItemName.Contains("send") && line.Contains("[SUM]") && line.Contains("sender"))
    //        if (line.Contains(item.CheckStr1) && line.Contains(item.CheckStr2))
    //        {
    //            item.TestValue = GetValue(line, item.SubStr1, item.SubStr2);
    //            //[SUM]   0.00-9.99   sec  2.41 GBytes  2.07 Gbits/sec    0             sender
    //            if (!string.IsNullOrEmpty(item.TestValue) && item.TestValue.EndsWith("G"))
    //            {
    //                string tempValue = item.TestValue.Replace("G", "").Trim();
    //                item.TestValue = ((double.Parse(tempValue)) * 1000).ToString();
    //            }
    //            else if (!string.IsNullOrEmpty(item.TestValue) && item.TestValue.EndsWith("M"))
    //            {
    //                item.TestValue = (double.Parse(item.TestValue.Replace("M", "").Trim())).ToString();
    //            }
    //            else
    //            {
    //                Logger.Debug("Get Speed error!");
    //            }
    //            rReturn = CompareLimit(item.Limit_min, item.Limit_max, item.TestValue, out info);
    //            break;
    //        }
    //    }
    //}
}
