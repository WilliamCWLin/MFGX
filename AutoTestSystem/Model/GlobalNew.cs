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
 *  1. <GlobalNew.cs> is use for parser Config.ini 
 *  2. Old version is Global.cs
 *  3. EntryPoint:  MainForm.cs initial --> GlobalNew.cs  
 *
 *
 */


/*****************************************************************************
*                    Includes Definitions
*****************************************************************************/
using AutoTestSystem.BLL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AutoTestSystem.Model;
using Newtonsoft.Json.Linq;
using System.Linq;
using Renci.SshNet.Messages;
using AutoTestSystem.Base;
using System.Diagnostics;
using static System.Collections.Specialized.BitVector32;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using AutoTestSystem.DAL;

/*****************************************************************************
*                    Function code
*****************************************************************************/
namespace AutoTestSystem.Model
{

    internal class GlobalNew
    {
        public static string IniConfigFile = System.Environment.CurrentDirectory + @"\Config\Config.ini"; //!配置文件路径文件;

        public static MFGX MainFormInstance { get; set; }
        public static List<ItemsNew> SeqItems = new List<ItemsNew>();

        #region ################################ 1.Config.ini setting ############################
        //************************** Station全局配置变量 **********************/
        public static string STATIONALL;            //! all工站名,
        public static string OTPKey;            //! all工站名,
        public static string STATIONNAME;           //! 工站名
        public static string STATIONNO;             //! 工站编号
        public static string MESIP;                 //! MES IP地址
        public static string MESPORT;               //! MES端口号
        public static string LOGFOLDER;             //! log保存路径
        public static string SharePath;             //! 本地Share file路径
        public static string ServerLOGFOLDER;       //! 服务器log保全路径
        public static string FIXTUREFLAG;           //! 自動化治具 (0=不使用自动化治具, 1=使用)
        public static string FIXCOM;                //! 治具串口号
        public static string FIXBaudRate;           //! 治具波特率
        public static string FIX_TCP_Port;          //! 治具ServerTCP设置，VD5001 FT01单独使用，产测程式作为Server，治具作为客户端，连接产测程式的Port
        public static string GPIBADDRESS;           //! Power Supply GPIB地址
        public static string TESTMODE;              //! 测试模式
        public static string TestCasePath;          //! 使用的excel脚本文件，目前支持xlsx格式
        public static string FAIL_CONTINUE;         //! 全局设置测试失败是否繼續，(1继续，0不繼續)
        public static string CYCLEFAIL_CONTINUE;         //! 全局设置测试失败是否繼續，(1继续，0不繼續)
        public static string DeviceListPath;        //! 全局设置测试失败是否繼續，(1继续，0不繼續)
        public static string EQLogPath;             //! 全局设置测试失败是否繼續，(1继续，0不繼續)
        //public static string RecipePath;          //! 全局设置测试失败是否繼續，(1继续，0不繼續)     
        public static string ConfigLogPath;         //! 全局设置测试失败是否繼續，(1继续，0不繼續)
        public static Int32 cycles;                 //! loop 循环次数，production mode 默认1，debug mdoe 生效
        public static string ProtreeON;               //! 啟用進行腳本流程
        public static string LOGSERVER;                 //! log上传到FTP server路径
        public static string LOGSERVERUser;             //! log FTP server登录用户名
        public static string LOGSERVERPwd;              //! log FTP server登录密码
        public static string FIXTURENAME;               //! 治具名称
        public static string RECIPENAME;               //! 治具名称
        public static string CSV_LOG;               //! 治具名称
        public static string ShowTestTime;               //! 治具名称
        public static string FormMode;               //! 
        public static int RunMode;

        //************************** DUT全局配置变量 **********************/
        public static string DUTIP;                 //! 產品IP
        public static string SSH_PORT;              //! 產品SSH端口号
        public static string SSH_USERNAME;          //! 產品SSH用户名
        public static string SSH_PASSWORD;          //! 產品SSH密码
        public static string DUTCOM;                //! 產品串口号
        public static string DUTBaudRate;           //! 產品波特率
        public static string DUTNAME;               //! 產品名稱

        //************************** Motion全局配置变量 **********************/
        public static string CardName;
        public static string ComportName;
        public static string Comport_Basename;
        public static string TorqueCOM;
        public static string TorqueBaudRate;
        public static string RingNo;
        public static string DeviceIP;
        public static string Axis_Number;
        public static string Config_path;
        public static string EQ_Mode;
        public static bool home_flag = false;
        public static bool Emg_flag = false;
        public static int EnableDeviceCount;
        //************************** Product全局配置变量 **********************/
        public static string ProMode;               //! 产品型号
        public static string PSN_LENGTH;            //! SN1条码长度
        public static string MAC_Length;            //! MAC条码长度
        public static string SN2_Length;            //! SN2条码长度；
        public static string SN_PATTERN;             //! SN格式(正則)；
        public static string PSN_REPLACE_KEY;             //! PSN取代
        public static string SN2_REPLACE_KEY;             //! SN2取代
        public static Int32 CheckSN_position;
        public static bool SN_Letter_case = false;
        //! 条码长度设置说明:
        //! SN2为主条码,当SN1 和SN2 同时存在时，会提前比对SN1 和SN2 的内容,
        //! 如果不需要多个条码，只需把其他条码长度设置0，保留SN2 条码长度

        //************************** CountNum全局配置变量 **********************/
        public static Int32 Total_Pass_Num;         //! pass的数量
        public static Int32 Total_Fail_Num;         //! fail的数量
        public static Int32 CycleCheck;
        public static Int32 CONTINUE_FAIL_LIMIT;    //! fail数量规格
        public static Int32 GlobalFailCount;
        #endregion

        #region ################################ 2.HW Setting     ################################
        //************************** HW_IO_DEVICE **********************/
        public static string CardDes;               //! 設定要使用的卡名稱
        public static string IONAME;                //! 用來加入裝置列表使用
        public static string IOTYPE;                //! 物件名稱
        public static string ProfilePath;           //! 設定檔路徑

        public static string CurrentRecipePath;     //! 目前選用的處方路徑
        public static string CurrentMode;         //! 目前選用的裝置設定
        public static string CurrentProject;     //! 目前選用的處方路徑
        public static string CurrentFixture;         //! 目前選用的裝置設定
        public static string CurrentStation;     //! 目前選用的處方路徑
        public static string CurrentConfigVersion;     //! 
        public static string DataPath;     //! 目前選用的處方路徑

        //************************** HW_CCD_DEVICE **********************/
        public static string CCDName;
        public static string CCDDeviceName;
        public static string CCDParamFileName;


        //**************************Control device**********************/
        /// <ControlDevice>  related setting
        public static string ControlDeviceNAME;
        public static string ControlDeviceCOM;
        public static string ControlDeviceBaudRate;
        public static string ControlDeviceInterfaceMethod;
        public static Dictionary<string, string> ProductKeys;
        /// <ControlDevice>   

        #endregion

        #region ################################ 3.Other global value   ##########################
        //**************************配置全局变量**********************/
        //public static string testJsonFile;                                                                        //! 测试Json地址
        public static Version Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;        //! 程序版本
        public static string txtLogPath;
        public static string LogPath;//! txt log储存路径
        public static string imageLogPath;                                                                          //! image log存储路径
        public static string csvLogPath;
        public static string Weblogin;
        public static string image_path = string.Empty;                                                             //! 存放image路徑
                                                                                                                    // 使用Stopwatch來計時
        public static Stopwatch stopwatch = new Stopwatch();                                                            //用於記錄測試時間
        public static bool g_Initial;     //! 目前選用的處方路徑
        public static bool g_HomeProcessSuccess;     //! 目前選用的處方路徑
        public static string Network_Path;
        public static string BarcodeReaderMode;



        //**************************Control device**********************/
        public static string PC_OS;
        public static Dictionary<string, object> Devices;
        public static Dictionary<int, int> ResetHome_status = new Dictionary<int, int>();
        public static Dictionary<int, int> ResetHome_check = new Dictionary<int, int>();
        public static Dictionary<string, NewModBus> comhandler = new Dictionary<string, NewModBus>();
        public static List<User> users;
        public static int UserLevel;
        public static string CurrentUser;
        public static List<string> fail_device = new List<string>();
        public static DataCollection g_datacollection;
        public static Dictionary<string, DataGridView> DataGridViewsList;
        public static string UserToken;
        public static string userList;

        public static bool g_isRunning;
        public static bool g_shouldStop;
        public static bool g_recipesteprun;
        #endregion

        #region ################################ F_1.Config parser   #############################
        public static void InitStation()
        {

            INIHelper iniConfig = new INIHelper(Global.IniConfigFile);
            try
            {
                DataGridViewsList = new Dictionary<string, DataGridView>();
                g_datacollection = new DataCollection();
                Devices = new Dictionary<string, object>();

                iniConfig.CheckPath(GlobalNew.IniConfigFile);
                ProductKeys = iniConfig.GetSectionKeyValuePairs("SerialNumber");
                STATIONALL = iniConfig.Readini("Station", "STATIONALL").Trim();
                STATIONNAME = iniConfig.Readini("Station", "STATIONNAME").Trim();
                STATIONNO = iniConfig.Readini("Station", "STATIONNO").Trim();
                MESIP = iniConfig.Readini("Station", "MESIP").Trim();
                MESPORT = iniConfig.Readini("Station", "MESPORT").Trim();
                LOGFOLDER = iniConfig.Readini("Station", "LOGFOLDER").Trim();
                ProtreeON = iniConfig.Readini("Station", "ProtreeON").Trim();
                //DeviceListPath      = iniConfig.Readini("Station", "DeviceListPath").Trim();
                ConfigLogPath = iniConfig.Readini("Station", "ConfigLogPath").Trim();
                EQLogPath = iniConfig.Readini("Station", "EQLogPath").Trim();
                Weblogin = iniConfig.Readini("Station", "Weblogin").Trim();
                ShowTestTime = iniConfig.Readini("Station", "ShowTestTime").Trim();
                BarcodeReaderMode = iniConfig.Readini("Station", "BarcodeReaderMode").Trim();
                FormMode = iniConfig.Readini("Station", "FormMode").Trim();
                DataPath = iniConfig.Readini("Station", "DataPath").Trim();
                //RecipePath = iniConfig.Readini("Station", "RecipePath").Trim();

                CSV_LOG = iniConfig.Readini("Main_LogInfo", "CSV_LOG").Trim();

                SharePath = iniConfig.Readini("Station", "SharePath").Trim();
                ServerLOGFOLDER = iniConfig.Readini("Station", "ServerLOGFOLDER").Trim();
                FIXTUREFLAG = iniConfig.Readini("Station", "FIXTUREFLAG").Trim();
                FIXCOM = iniConfig.Readini("Station", "FIXCOM").Trim();
                FIXBaudRate = iniConfig.Readini("Station", "FIXBaudRate").Trim();
                FIX_TCP_Port = iniConfig.Readini("Station", "FIX_TCP_Port").Trim();
                GPIBADDRESS = iniConfig.Readini("Station", "GPIBADDRESS").Trim();
                TESTMODE = iniConfig.Readini("Station", "TESTMODE").Trim();
                TestCasePath = iniConfig.Readini("Station", "TestCasePath").Trim();
                FAIL_CONTINUE = iniConfig.Readini("Station", "FAIL_CONTINUE").Trim();
                CYCLEFAIL_CONTINUE = iniConfig.Readini("Station", "CYCLE_FAIL_CONTINUE").Trim();
                int.TryParse(iniConfig.Readini("Station", "cycles").Trim(), out cycles);
                EQ_Mode = iniConfig.Readini("Station", "EQ_Mode").Trim();
                DUTIP = iniConfig.Readini("DUT", "DUTIP").Trim();
                SSH_PORT = iniConfig.Readini("DUT", "SSH_PORT").Trim();
                SSH_USERNAME = iniConfig.Readini("DUT", "SSH_USERNAME").Trim();
                SSH_PASSWORD = iniConfig.Readini("DUT", "SSH_PASSWORD").Trim();
                DUTCOM = iniConfig.Readini("DUT", "DUTCOM").Trim();
                DUTNAME = iniConfig.Readini("DUT", "DUTNAME").Trim();
                DUTBaudRate = iniConfig.Readini("DUT", "DUTBaudRate").Trim();

                IONAME = iniConfig.Readini("HW_IO_DEVICE", "IONAME").Trim();
                CardDes = iniConfig.Readini("HW_IO_DEVICE", "CARDNAME").Trim();
                ProfilePath = iniConfig.Readini("HW_IO_DEVICE", "ProfilePath").Trim();
                IOTYPE = iniConfig.Readini("HW_IO_DEVICE", "IOTYPE").Trim();

                ///// MTE Peter Add Global ini setting for  CCD/////
                CCDName = iniConfig.Readini("HW_CCD_DEVICE", "CCDNAME").Trim();
                CCDDeviceName = iniConfig.Readini("HW_CCD_DEVICE", "CCDDeviceName").Trim();
                CCDParamFileName = iniConfig.Readini("HW_CCD_DEVICE", "CCDParamFileName").Trim();
                ///// MTE Peter Add Global ini setting for  CCD/////



                ///// MTE Peter Add Global ini setting for ControlDevice/////
                ControlDeviceNAME = iniConfig.Readini("ControlDevice", "ControlDeviceNAME").Trim();
                ControlDeviceCOM = iniConfig.Readini("ControlDevice", "ControlDeviceCOM").Trim();
                ControlDeviceBaudRate = iniConfig.Readini("ControlDevice", "ControlDeviceBaudRate").Trim();
                if (ControlDeviceBaudRate == "")
                    ControlDeviceBaudRate = "9600";
                ControlDeviceInterfaceMethod = iniConfig.Readini("ControlDevice", "ControlDeviceInterfaceMethod").Trim();
                if (ControlDeviceInterfaceMethod == "")
                    ControlDeviceInterfaceMethod = "0";
                ///// MTE Peter Add Global ini setting for ControlDevice/////
                EnableDeviceCount = 0;

                ProMode = iniConfig.Readini("Product", "ProMode").Trim();
                SN_PATTERN = iniConfig.Readini("Product", "SN_PATTERN").Trim();
                PSN_LENGTH = iniConfig.Readini("Product", "PSN_LENGTH").Trim();
                MAC_Length = iniConfig.Readini("Product", "MAC_Length").Trim();
                SN2_Length = iniConfig.Readini("Product", "SN2_Length").Trim();
                PSN_REPLACE_KEY = iniConfig.Readini("Product", "PSN_REPLACE_KEY").Trim();
                SN2_REPLACE_KEY = iniConfig.Readini("Product", "SN2_REPLACE_KEY").Trim();
                OTPKey = iniConfig.Readini("Product", "OTPKey").Trim();

                bool.TryParse(iniConfig.Readini("Product", "SN_Letter_case").Trim(), out SN_Letter_case);
                int.TryParse(iniConfig.Readini("Product", "CheckSN_position").Trim(), out CheckSN_position);

                int.TryParse(iniConfig.Readini("CountNum", "Total_Pass_Num").Trim(), out Total_Pass_Num);
                int.TryParse(iniConfig.Readini("CountNum", "Total_Fail_Num").Trim(), out Total_Fail_Num);
                int.TryParse(iniConfig.Readini("CountNum", "CONTINUE_FAIL_LIMIT").Trim(), out CONTINUE_FAIL_LIMIT);
                int.TryParse(iniConfig.Readini("CountNum", "CycleCheck").Trim(), out CycleCheck);



                ////MTE Franklin Add Motion///
                CardName = iniConfig.Readini("Motion", "CardName").Trim();
                ComportName = iniConfig.Readini("Motion", "ComportName").Trim();
                Comport_Basename = iniConfig.Readini("Motion", "Comport_Basename").Trim();
                TorqueCOM = iniConfig.Readini("Motion", "TorqueCOM").Trim();
                TorqueBaudRate = iniConfig.Readini("Motion", "TorqueBaudRate").Trim();
                RingNo = iniConfig.Readini("Motion", "RingNo").Trim();
                DeviceIP = iniConfig.Readini("Motion", "DeviceIP").Trim();
                Axis_Number = iniConfig.Readini("Motion", "Axis_Number").Trim();
                Config_path = iniConfig.Readini("Motion", "Config_path").Trim();
                ////MTE Franklin Add Motion///
                ///SW3 MONO add for PC Command
                PC_OS = iniConfig.Readini("PC", "OS").Trim();
                ///SW3 MONO add for PC Command
                ///
                FIXTURENAME = iniConfig.Readini("Station", "FIXTURENAME").Trim();
                LOGSERVER = iniConfig.Readini("Station", "LOGSERVER").Trim();
                LOGSERVERUser = iniConfig.Readini("Station", "LOGSERVERUser").Trim();
                LOGSERVERPwd = iniConfig.Readini("Station", "LOGSERVERPwd").Trim();

                if (ProtreeON != "1")
                    GlobalNew.DeviceListPath = iniConfig.Readini("Station", "DeviceListPath").Trim();

                CurrentRecipePath = iniConfig.Readini("Recipe", "CurrentRecipePath").Trim();
                CurrentProject = iniConfig.Readini("Recipe", "CurrentProject").Trim();
                CurrentMode = iniConfig.Readini("Recipe", "CurrentMode").Trim();
                CurrentStation = iniConfig.Readini("Recipe", "CurrentStation").Trim();
                CurrentFixture = iniConfig.Readini("Recipe", "CurrentFixture").Trim();
                CurrentConfigVersion = iniConfig.Readini("Recipe", "CurrentVersion").Trim();
                g_isRunning = false;
                g_shouldStop = false;
                g_Initial = false;
                g_recipesteprun = false;
                GlobalFailCount = 0;
                RunMode = 0;
                g_HomeProcessSuccess = false;

                DataManager.LoadData("defectCodes.csv");
            }

            catch (Exception ex)
            {
                MessageBox.Show($@"Read ini-config error,initStation:{ex}", "ERROR!");
                throw;
            }
        }
        #endregion

        #region ################################ F_2.Excel to json (first time setting)  #########
        //public static void ExcelToJson()
        //{
        //    TestCasePath = $@"{System.Environment.CurrentDirectory}\Config\{TestCasePath}";
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        //连接字符串 Office 07及以上版本 不能出现多余的空格 而且分号注意
        //        //string connstring = "Provider=Microsoft.Ace.OLEDB.12.0;" + "Data Source=" + TestCasePath + ";" + "Extended Properties=\'Excel 12.0 Xml;HDR=Yes;IMEX=1;\'";
        //        //string connstring = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source='" + TestCasePath + "';Extended Properties='Excel 12.0 Xml;HDR=Yes;IMEX=1;'";
        //        string connstring = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source='" + TestCasePath + "';Extended Properties='Excel 12.0 Xml;HDR=Yes;IMEX=1;'";
        //        using (OleDbConnection conn = new OleDbConnection(connstring))
        //        {
        //            conn.Open();
        //            DataTable sheetsName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" }); //得到所有sheet的名字
        //            for (int i = 0; i <= sheetsName.Rows.Count - 1; i++)  //循环搜寻Excel表，不搜寻首页Version和尾页_xlnm#_FilterDatabase

        //            {
        //                string sheetName = sheetsName.Rows[i]["TABLE_NAME"].ToString();
        //               //sheetName=sheetName.Replace("$",null); //移除 excel表格取值末尾“$” 字符
        //                int dollarSignIndex = sheetName.IndexOf('$');
        //                if (sheetName.Contains("Version"))
        //                    continue;
        //                if (dollarSignIndex != -1)
        //                {
        //                    sheetName = sheetName.Substring(0, dollarSignIndex);

        //                }
        //                if (GlobalNew.STATIONALL.Contains(sheetName)==true)   //
        //                {
        //                    string tempSeqName=null;
        //                    JArray Sequences = new JArray();

        //                    string sql = string.Format("SELECT * FROM [{0}$]", sheetName); //执行SQL查询语句，把Excel 表格内容读取出来
        //                    using (OleDbDataAdapter ada = new OleDbDataAdapter(sql, connstring))
        //                    {

        //                        ada.Fill(dt);
        //                        for (int j = 0; j < dt.Rows.Count; j++)
        //                        {

        //                            ////Items 赋值
        //                            ItemsNew tempItem = new ItemsNew();
        //                            tempItem.ItemName = dt.Rows[j][1].ToString();
        //                            tempItem.TestKeyword = dt.Rows[j][2].ToString();
        //                            tempItem.ErrorCode = dt.Rows[j][3].ToString();
        //                            tempItem.RetryTimes = dt.Rows[j][4].ToString();
        //                            tempItem.TimeOut = dt.Rows[j][5].ToString();
        //                            tempItem.SubStr1 = dt.Rows[j][6].ToString();
        //                            tempItem.SubStr2 = dt.Rows[j][7].ToString();
        //                            tempItem.IfElse = dt.Rows[j][8].ToString();
        //                            tempItem.For = dt.Rows[j][9].ToString();
        //                            tempItem.Mode = dt.Rows[j][10].ToString();
        //                            tempItem.ComdSend = dt.Rows[j][11].ToString();
        //                            tempItem.ExpectStr = dt.Rows[j][12].ToString();
        //                            tempItem.CheckStr1 = dt.Rows[j][13].ToString();
        //                            tempItem.CheckStr2 = dt.Rows[j][14].ToString();
        //                            tempItem.Spec = dt.Rows[j][15].ToString();
        //                            tempItem.Limit_min = dt.Rows[j][16].ToString();
        //                            tempItem.Limit_max = dt.Rows[j][17].ToString();
        //                            tempItem.unit = dt.Rows[j][18].ToString();
        //                            tempItem.Bypass = dt.Rows[j][19].ToString();
        //                            tempItem.DllPlugin = dt.Rows[j][20].ToString();
        //                            tempItem.StriptType = dt.Rows[j][21].ToString();
        //                            tempItem.DeviceName = dt.Rows[j][22].ToString();
        //                            tempItem.SpecRule = dt.Rows[j][23].ToString();

        //                            if (tempItem.ItemName!="")
        //                            {
        //                                if ((dt.Rows[j][0].ToString() != "" && j != 0))
        //                                {

        //                                    JObject tempSeq = new JObject();
        //                                    JArray Sequence = JArray.Parse(JsonConvert.SerializeObject(GlobalNew.SeqItems, Formatting.Indented));

        //                                    //赋值Seq成员参数
        //                                    tempSeq["SeqName"] = tempSeqName;
        //                                    tempSeq["TotalNumber"] = SeqItems.Count.ToString();
        //                                    tempSeq["SeqItems"] = Sequence;
        //                                    Sequences.Add(tempSeq);
        //                                    SeqItems.Clear();
        //                                }
        //                                if (dt.Rows[j][0].ToString() != "")
        //                                {
        //                                    ////首列SeqName取值

        //                                    tempSeqName = dt.Rows[j][0].ToString();

        //                                }

        //                                if (tempItem.ItemName!="" && tempItem.TestKeyword!="")   //ItemName，TestKeyword 空白的项目自动跳过
        //                                {
        //                                    SeqItems.Add(tempItem);                                                             
        //                                }
        //                                if (j == dt.Rows.Count - 1)
        //                                {
        //                                    JObject tempSeq = new JObject();
        //                                    JArray Sequence = JArray.Parse(JsonConvert.SerializeObject(GlobalNew.SeqItems, Formatting.Indented));

        //                                    //赋值Seq成员参数
        //                                    tempSeq["SeqName"] = tempSeqName;
        //                                    tempSeq["TotalNumber"] = SeqItems.Count.ToString();
        //                                    tempSeq["SeqItems"] = Sequence;
        //                                    Sequences.Add(tempSeq);
        //                                    SeqItems.Clear();
        //                                }
        //                            }
        //                        }                                
        //                    }                            
        //                    dt.Reset();                          
        //                    string jsonClintContent = Sequences.ToString();
        //                    string jsonPath = $@"{System.Environment.CurrentDirectory}\Config\{sheetName}.json";
        //                    File.WriteAllText(jsonPath, jsonClintContent, Encoding.UTF8);
        //                }
        //            }
        //            conn.Close();
        //        }
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        MessageBoxButtons buttons = MessageBoxButtons.OK;
        //        DialogResult result;
        //        string linkUrl = "http://www.microsoft.com/zh-CN/download/confirmation.aspx?id=23734";
        //        string infoMessage = "请安装AccessDatabaseEngine,下载链接:\r\n " + linkUrl;
        //        result = MessageBox.Show(infoMessage + "\n\n点击 '确定' 按钮复制链接", "Error", buttons);
        //        if (result == System.Windows.Forms.DialogResult.OK)
        //        {
        //            Clipboard.SetText(linkUrl);
        //        }
        //        throw ex;
        //    }
        //    catch (OleDbException ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //        throw ex;
        //    }

        //}
        #endregion

        #region ################################ F_3.initial TreeView in json  ###################
        public static void JsonToTreeViewSeq()
        {
            string jsonPath = $@"{System.Environment.CurrentDirectory}\Config\{STATIONNAME}.json";
            //string json = File.ReadAllText("data.json"); // 读取文件内容
            JArray Sequences = JArray.Parse(File.ReadAllText(jsonPath));
            for (int i = 0; i < Sequences.Count; i++)
            {

                TreeNode tempSeq = new TreeNode();
                tempSeq.Text = Sequences[i]["SeqName"].ToString();
                tempSeq.ImageIndex = 1;

            }

        }
        #endregion
        private static readonly object lockObject = new object();
        public static void IncrementPassCount()
        {
            lock (lockObject)
            {
                GlobalNew.Total_Pass_Num++;
                MainFormInstance?.UpdatePassLabel($"{Total_Pass_Num}");
            }
        }
        public static void IncrementFailCount()
        {
            lock (lockObject)
            {
                GlobalNew.Total_Fail_Num++;
                MainFormInstance?.UpdateFailLabel($"{Total_Fail_Num}");
            }
        }

        public static object ParseAndConvert(string sourceValue, string format)
        {
            // 修改正則表達式：運算符和數字部分分開處理
            Regex regex = new Regex(@"(\w+)\((\w+)\)([+\-*/]?)(\d*)"); // 解析格式，例如 double(value1)+1
            Match match = regex.Match(format);
            if (!match.Success) return sourceValue;

            string conversionType = match.Groups[1].Value;
            string value = sourceValue;  // 來源數值

            string operation = match.Groups[3].Value;  // 運算符
            string operandString = match.Groups[4].Value;  // 數字部分

            object convertedValue = ConvertValue(conversionType, value);

            // 如果有運算操作，則處理
            if (!string.IsNullOrEmpty(operation) && !string.IsNullOrEmpty(operandString))
            {
                if (double.TryParse(operandString, out double operand))
                {
                    convertedValue = ApplyOperation(convertedValue, operand, operation); // 傳遞運算符
                }
            }

            return convertedValue;
        }
        static object ConvertValue(string type, string value)
        {
            try
            {
                switch (type.ToLower())
                {
                    case "double":
                        return double.Parse(value, CultureInfo.InvariantCulture);
                    case "int":
                        return int.Parse(value);
                    case "scientific":
                        return double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                    case "mac":
                        return FormatMacAddress(value);
                    default:
                        return value;
                }
            }
            catch
            {
                return null;
            }
        }

        static object ApplyOperation(object convertedValue, double operand, string operation)
        {
            if (convertedValue is double)
            {
                if (operation == "+")
                {
                    return (double)convertedValue + operand;
                }
                else if (operation == "-")
                {
                    return (double)convertedValue - operand;
                }
                else if (operation == "*")
                {
                    return (double)convertedValue * operand;
                }
                else if (operation == "/")
                {
                    return (double)convertedValue / operand;
                }
            }
            if (convertedValue is int)
            {
                if (operation == "+")
                {
                    return (int)convertedValue + operand;
                }
                else if (operation == "-")
                {
                    return (int)convertedValue - operand;
                }
                else if (operation == "*")
                {
                    return (int)convertedValue * operand;
                }
                else if (operation == "/")
                {
                    return (int)convertedValue / operand;
                }
            }
            if (convertedValue is string)
            {
                // 對 MAC 地址進行處理，將每個字元轉為十六進制並加上運算
                return ApplyMacOperation((string)convertedValue, operand, operation);
            }
            return convertedValue; // 如果無法進行運算，返回原值
        }

        static string FormatMacAddress(string value)
        {
            value = Regex.Replace(value, @"[^0-9A-Fa-f]", ""); // 移除所有非十六進制字符
            value = value.ToUpper(); // 統一轉大寫

            if (value.Length % 2 != 0)
            {
                value = "0" + value; // 若長度為奇數，前補 0
            }

            return string.Join(":", Regex.Matches(value, @"..")
                                                .Cast<Match>()  // 轉換為 IEnumerable<Match>
                                                .Select(m => m.Value));  // 使用 Select 提取值
        }

        static object ApplyMacOperation(string mac, double operand, string operation)
        {
            // 確保 MAC 地址長度為 12 位（6 字節）
            if (mac.Length != 12)
            {
                throw new ArgumentException("MAC 地址必須為 12 個字符");
            }

            // 提取最後一個字節（即最後 2 個字符）
            byte lastByte = Convert.ToByte(mac.Substring(10, 2), 16); // 取最後 2 個字符並轉為 byte

            // 根據操作類型對最後一個字節進行運算
            if (operation == "+")
            {
                lastByte = (byte)(lastByte + (byte)operand);
            }
            else if (operation == "-")
            {
                lastByte = (byte)(lastByte - (byte)operand);
            }
            else if (operation == "*")
            {
                lastByte = (byte)(lastByte * (byte)operand);
            }
            else if (operation == "/")
            {
                lastByte = (byte)(lastByte / (byte)operand);
            }

            // 用更新後的最後一個字節組成新的 MAC 地址
            string updatedMac = mac.Substring(0, 10) + lastByte.ToString("X2"); // 保留前 10 位，更新最後一個字節

            return updatedMac; // 返回更新後的 MAC 地址
        }


        public static DialogResult ShowMessage(
                string message,
                string title = "訊息",
                MessageBoxIcon icon = MessageBoxIcon.Information,
                MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            Form owner = Form.ActiveForm;

            if (owner != null && owner.InvokeRequired)
            {
                DialogResult result = DialogResult.None;
                owner.Invoke(new Action(() =>
                {
                    result = MessageBox.Show(owner, message, title, buttons, icon);
                }));
                return result;
            }
            else
            {
                return MessageBox.Show(owner, message, title, buttons, icon);
            }
        }


        private static Dictionary<string, ImageShowFrom> messageForms = new Dictionary<string, ImageShowFrom>();

        public static void ManageNotifyFormOpen(string key, string message, string img_path)
        {
            // 如果主視窗不存在，直接跳出
            if (GlobalNew.MainFormInstance == null) return;

            // 在主視窗 UI 執行緒中執行整個流程
            GlobalNew.MainFormInstance.Invoke(new Action(() =>
            {
                // 建立或重建視窗
                if (!messageForms.ContainsKey(key) || messageForms[key] == null || messageForms[key].IsDisposed)
                {
                    messageForms[key] = new ImageShowFrom();
                }

                var form = messageForms[key];
                form.TopMost = true;
                form.SetImageShowForm(message, "NA", "NA", 0, img_path, false, false);

                // 使用主視窗作為 owner 顯示
                form.Show(GlobalNew.MainFormInstance);
                form.Activate();
                form.Update();
            }));
        }


        public static void ManageNotifyFormClose(string key)
        {
            if (messageForms.ContainsKey(key) && messageForms[key] != null)
            {
                messageForms[key].Cancel_Btn(false);

                if (messageForms[key].InvokeRequired)
                {
                    messageForms[key].Invoke(new Action(() => messageForms[key].Close()));
                }
                else
                {
                    messageForms[key].Close();
                }

                messageForms[key] = null;
            }
        }


    }


}


