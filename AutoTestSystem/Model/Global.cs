/*
 * "AutoTestSystem.Model --> GlobalNew"
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
 *
 *  1. Global.cs is use for parser Config.ini 
 *  2. Not use now, new version is GlobalNew.cs
 *
 * 
 */



using AutoTestSystem.BLL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AutoTestSystem.Model
{
    /// <summary>
    /// 全局变量类
    /// </summary>
    public static class Global
    {
        public static List<Sequence> Sequences  = new List<Sequence>();                                                     //! 测试用例队列
        public static string[] itemHeader;                                                                                  //! 用例表头
        public static Version Version           = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;      //! 程序版本
        public static string debugPath          = System.Environment.CurrentDirectory + @".\debug.txt";                     //! debug log保存的初始路径
        public static readonly object wLock     = new object();                                                             //! 互斥锁
        public static string IniConfigFile      = System.Environment.CurrentDirectory + @"\Config\Config.ini";              //! 配置文件路径文件;
        public static string LogPath;
        public static string JsonFilePath;

        ///**************************ini配置文件测试Station全局配置变量**********************/
        public static string DEBUTBUTTON;
        public static string STATIONNAME;               //! 工站名
        public static string STATIONALL;                //! all工站名,
        public static string FIXTURENAME;               //! 治具名称
        public static string STATIONNO;                 //! MES工站名
        public static string MescheckrouteIP;           //
        public static string MESIP;                     //! MES IP地址
        public static string MESPORT;                   //! MES端口号
        public static string LOGFOLDER;                 //! log保存路径
        public static string Share_path;                //! 本地Share file路径
        public static string ServerLOGFOLDER;           //! 服务器log保全路径
        public static string MESFOLDER;                 //! MES_SendCMD保存路径
        public static string MaxSizeRollBackups = "10";
        public static string LOGSERVER;                 //! log上传到FTP server路径
        public static string LOGSERVERUser;             //! log FTP server登录用户名
        public static string LOGSERVERPwd;              //! log FTP server登录密码
        public static string FIXTUREFLAG;               //! 是否使用治具，0=手动，1=使用治具
        public static string FIXCOM;                    //! 治具串口号
        public static string FIXBaudRate;               //! 治具串口波特率
        public static string FIX_TCP_Port;              //! 治具ServerTCP设置，VD5001 FT01单独使用，产测程式作为Server，治具作为客户端，连接产测程式的Port
        public static string GPIBADDRESS;               //! Power Supply GPIB地址
        public static string TESTMODE;                  //! 测试模式
        public static string TestCasePath;              //! 测试使用的Excel脚本
        public static string PySCRIPT;                  //! 上传结果的JSON脚本
        public static string PROMPT;
        public static string FAIL_CONTINUE;             //! 失败是否继续

        ///**************************ini配置文件DUT全局配置变量**********************/
        public static string DUTIP;
        public static string SSH_PORT;
        public static string SSH_USERNAME;
        public static string SSH_PASSWORD;
        public static string DUTCOM;
        public static string DUTBaudRate;

        ///***************************ini配置文件Product产品全局配置变量**********************/
        public static string ProMode;                   //! 产品机种
        public static string ProMode1;
        public static string ProMode2;
        public static string ProMode3;
        public static int PSN_LENGTH;
        public static int MAC_LENGTH;
        public static int SN2_LENGTH;
        public static string SFC_Mode;
        public static string IsSetAgingMode;
        public static string IsSetQAMode;
        public static string SWVersion;
        public static string BTServerID;                //! Bluetooth ID
        public static string BigTaoLogPath;
        public static string UplaodQCNFilePath;
        public static string SGN5171BIP;
        public static string RFSwitch1P2TIP;
        public static string RFSwitch1P8TIP;
        public static string PowermeterNRP8SSN;
        public static string QSDKVER;

        ///**************************ini配置文件CountNum全局配置变量**********************/
        public static short CONTINUE_FAIL_LIMIT;        //
        public static short ContinueFailNum;
        public static Int32 Total_Fail_Num;
        public static Int32 Total_Pass_Num;
        public static Int32 Total_Abort_Num;

        ///**************************ini配置文件各工站全局配置变量**********************/
        public static string CalTreeName;
        public static string VerTreeName;


        /// <summary>
        /// 把STATIONALL所有Excel测试用例转换成Json文件格式
        /// </summary>
        public static void ExcelConvertToJson()
        {
            try
            {
                SaveLog($"删除Debug和Release下旧的Json用例文件");
                Bd.ClearDirectory($@"{System.Environment.CurrentDirectory}\Config", ".json");
                Bd.ClearDirectory($@"{System.Environment.CurrentDirectory}\Config", "_key.txt");
                string testCasePath2 = $@"{System.Environment.CurrentDirectory}\Config\{TestCasePath}";
                SaveLog($"加载Excel用例文件路径:{testCasePath2}");
                LoadSeq loadSeq = new LoadSeq(testCasePath2);
                string[] tempStation = GlobalNew.STATIONALL.Trim().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < tempStation.Length; i++)
                {
                    DataTable testCaseTable = loadSeq.GetFirstSheetToDT(tempStation[i]);
                    // 获取表头
                    itemHeader = loadSeq.GetColumnsByDataTable(testCaseTable);
                    // 根据testCaseTable加载测试用例到Global.sequences
                    Global.Sequences = loadSeq.GetSeq(testCaseTable, itemHeader, "SeqName", "ItemName");
                    string jsonClintContent = JsonConvert.SerializeObject(Global.Sequences, Formatting.Indented);
                    string jsonPath = $@"{System.Environment.CurrentDirectory}\Config\{tempStation[i]}.json";
                    string jsonSHA256 = $@"{System.Environment.CurrentDirectory}\Config\{tempStation[i]}_key.txt";
                    File.WriteAllText(jsonPath, jsonClintContent, Encoding.Default);
                    // 生成对应的SHA256检验码，以二进制方式保存
                    byte[] shaByte = Bd.GetSHA256Byte(jsonPath);
                    Bd.BinaryWrite(shaByte, jsonSHA256);
                    //File.Copy(jsonPath, $@"{System.Environment.CurrentDirectory.Replace("Debug", "Release")}\Config\{tempStation[i]}.json", true);
                    SaveLog($"生成{tempStation[i]}站Json用例文件成功");
                    File.SetAttributes(jsonPath, FileAttributes.ReadOnly);
                    File.SetAttributes(jsonSHA256, FileAttributes.Hidden | FileAttributes.ReadOnly);
                    Global.Sequences = new List<Sequence>();
                    jsonClintContent = null;
                    testCaseTable = new DataTable();
                    itemHeader = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}");
                //throw;
            }
        }

        public static void LoadSequnces()
        {
            try
            {
                Global.Sequences = null;
                JsonFilePath = $@"{System.Environment.CurrentDirectory}\Config\{STATIONNAME}.json";
                string shaPath = $@"{System.Environment.CurrentDirectory}\Config\{STATIONNAME}_key.txt";
                string sha = Bd.BinaryRead(shaPath);
                SaveLog($"  txtSHA:{sha}");
                string JsonSHA = Bd.GetSHA256(JsonFilePath);
                SaveLog($"jsonSHA:{JsonSHA}");
                if (sha == JsonSHA)
                {
                    if (File.Exists(JsonFilePath))
                    {
                        var jsonStr = "";
                        using (StreamReader sr = new StreamReader(JsonFilePath))
                        {
                            jsonStr = sr.ReadToEnd();
                        }
                        Global.Sequences = JsonConvert.DeserializeObject<List<Sequence>>(jsonStr);
                    }
                    else
                    {
                        MessageBox.Show($"json testCase file {JsonFilePath} isn't exist!");
                        System.Environment.Exit(0);
                    }
                }
                else
                {
                    MessageBox.Show($"json testCase file {JsonFilePath} has been tampered!");
                    System.Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}");
                System.Environment.Exit(0);
                //throw;
            }
        }

        public static bool InitStation()
        {
            File.Delete(debugPath);
            INIHelper iniConfig = new INIHelper(Global.IniConfigFile);
            try
            {
                iniConfig.CheckPath(Global.IniConfigFile);
                DEBUTBUTTON = iniConfig.Readini("Station", "DEBUTBUTTON").Trim();
                STATIONALL = iniConfig.Readini("Station", "STATIONALL").Trim();
                STATIONNAME = iniConfig.Readini("Station", "STATIONNAME").Trim();
                FIXTURENAME = iniConfig.Readini("Station", "FIXTURENAME").Trim();
                STATIONNO = iniConfig.Readini("Station", "STATIONNO").Trim();
                MescheckrouteIP = iniConfig.Readini("Station", "MescheckrouteIP").Trim();
                MESIP = iniConfig.Readini("Station", "MESIP").Trim();
                MESPORT = iniConfig.Readini("Station", "MESPORT").Trim();
                LOGFOLDER = iniConfig.Readini("Station", "LOGFOLDER").Trim();
                Share_path = iniConfig.Readini("Station", "Share_path").Trim();
                ServerLOGFOLDER= iniConfig.Readini("Station", "ServerLOGFOLDER").Trim();
                MESFOLDER= iniConfig.Readini("Station", "MESFOLDER").Trim();
                MaxSizeRollBackups = iniConfig.Readini("Station", "MaxSizeRollBackups").Trim();
                LOGSERVER = iniConfig.Readini("Station", "LOGSERVER").Trim();
                LOGSERVERUser = iniConfig.Readini("Station", "LOGSERVERUser").Trim();
                LOGSERVERPwd = iniConfig.Readini("Station", "LOGSERVERPwd").Trim();
                FIX_TCP_Port= iniConfig.Readini("Station", "FIX_TCP_Port").Trim();

#if DEBUG
                FIXTUREFLAG = "0";
#else
                FIXTUREFLAG = iniConfig.Readini("Station", "FIXTUREFLAG").Trim();
#endif
                FIXCOM = iniConfig.Readini("Station", "FIXCOM").Trim();
                FIXBaudRate = iniConfig.Readini("Station", "FIXBaudRate").Trim();
                GPIBADDRESS = iniConfig.Readini("Station", "GPIBADDRESS").Trim();
#if DEBUG
                TESTMODE = "debug";
#else
                TESTMODE = iniConfig.Readini("Station", "TESTMODE").Trim();
#endif
                TestCasePath = iniConfig.Readini("Station", "TestCasePath").Trim();
                PySCRIPT = iniConfig.Readini("Station", "PySCRIPT").Trim();
                PROMPT = iniConfig.Readini("Station", "PROMPT").Trim();
                FAIL_CONTINUE = iniConfig.Readini("Station", "FAIL_CONTINUE").Trim();

                DUTIP = iniConfig.Readini("DUT", "DUTIP").Trim();
                SSH_PORT = iniConfig.Readini("DUT", "SSH_PORT").Trim();
                SSH_USERNAME = iniConfig.Readini("DUT", "SSH_USERNAME").Trim();
                SSH_PASSWORD = iniConfig.Readini("DUT", "SSH_PASSWORD").Trim();
                DUTCOM = iniConfig.Readini("DUT", "DUTCOM").Trim();
                DUTBaudRate = iniConfig.Readini("DUT", "DUTBaudRate").Trim();
                ProMode = iniConfig.Readini("Product", "ProMode").Trim();
                ProMode1 = iniConfig.Readini("Product", "ProMode1").Trim();
                ProMode2 = iniConfig.Readini("Product", "ProMode2").Trim();
                ProMode3 = iniConfig.Readini("Product", "ProMode3").Trim();
                PSN_LENGTH = Int16.Parse(iniConfig.Readini("Product", "PSN_LENGTH").Trim());
                SN2_LENGTH = Int16.Parse(iniConfig.Readini("Product", "SN2_LENGTH").Trim());
                MAC_LENGTH = Int16.Parse(iniConfig.Readini("Product", "MAC_LENGTH").Trim());
                SFC_Mode = iniConfig.Readini("Product", "SFC_Mode").Trim();
                IsSetAgingMode = iniConfig.Readini("Product", "IsSetAgingMode").Trim();
                IsSetQAMode = iniConfig.Readini("Product", "IsSetQAMode").Trim();
                SWVersion = iniConfig.Readini("Product", "SWVersion").Trim();
                BTServerID = iniConfig.Readini("Product", "BTServerID").Trim();
                BigTaoLogPath = iniConfig.Readini("Product", "BigTaoLogPath").Trim();
                UplaodQCNFilePath = iniConfig.Readini("Product", "UploadQCNFilePath").Trim();
                SGN5171BIP = iniConfig.Readini("Product", "SGN5171BIP").Trim();
                RFSwitch1P2TIP = iniConfig.Readini("Product", "RFSwitch1P2TIP").Trim();
                RFSwitch1P8TIP = iniConfig.Readini("Product", "RFSwitch1P8TIP").Trim();
                PowermeterNRP8SSN = iniConfig.Readini("Product", "DUTBPowermeterNRP8SSNaudRate").Trim();
                QSDKVER = iniConfig.Readini("Product", "QSDKVER").Trim();

                CONTINUE_FAIL_LIMIT = short.Parse(iniConfig.Readini("CountNum", "CONTINUE_FAIL_LIMIT").Trim());
                ContinueFailNum = short.Parse(iniConfig.Readini("CountNum", "ContinueFailNum").Trim());
                Total_Fail_Num = Int32.Parse(iniConfig.Readini("CountNum", "Total_Fail_Num").Trim());
                Total_Pass_Num = Int32.Parse(iniConfig.Readini("CountNum", "Total_Pass_Num").Trim());
                Total_Abort_Num = Int32.Parse(iniConfig.Readini("CountNum", "Total_Abort_Num").Trim());
                CalTreeName = iniConfig.Readini("TMORFTest", "CalTreeName").Trim();
                VerTreeName = iniConfig.Readini("TMORFTest", "CalTreeName").Trim();
                //#if DEBUG
                try
                {
                    string jsonPath = $@"{System.Environment.CurrentDirectory}\Config\{Global.STATIONNAME}.json";
                    string jsonSHAPath = $@"{System.Environment.CurrentDirectory}\Config\{Global.STATIONNAME}_key.txt";
                    if (!File.Exists(jsonPath) && !File.Exists(jsonSHAPath))
                    {
                        ExcelConvertToJson();
                        //Thread.Sleep(3000);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"生成Json用例文件失败:{ex.ToString()}");
                }
                //#endif
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Read ini-config error,initStation:{ex}", "ERROR!");
                throw;
            }
        }

        public static void SaveLog(string log, int type = 2)
        {
            try
            {
                lock (wLock)
                {
                    using (StreamWriter sw = new StreamWriter(debugPath, true, Encoding.Default))
                    {
                        if (type == 1) //不换行打印log
                        {
                            sw.Write(log);
                        }
                        else
                        {
                            sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {log}");
                        }
                    }
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.ToString());
                // SaveLog(ex.ToString());
                //throw;
            }
        }
    }
}