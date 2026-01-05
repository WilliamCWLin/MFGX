/*
 * "AutoTestSystem.Model --> Station" 
 * "AutoTestSystem.Model --> test_phases" 
 * "AutoTestSystem.Model --> phase_items" 
 * "AutoTestSystem.Model --> MesPhases" 
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
 *  1. <Eerro.cs> is use for show MES Station & Error messgae
 *  2. EntryPoint: ScriptDUTBase --> DUT_UART_Duplex.cs  
 *   
 *
 */


/*****************************************************************************
*                    Includes Definitions
*****************************************************************************/
using System.Collections.Generic;


/*****************************************************************************
*                    Function code
*****************************************************************************/
namespace AutoTestSystem.Model
{
    public class Station
    {
        public string serial = "";
        public string test_station = "";
        public string start_time = "";
        public string finish_time = "";
        public string status = "canceled";
        public string mode = "";
        public string error_code = "";
        public string error_details = "";
        public string luxshare_qsdk_version = "";
        public string test_software_version = "";
        public List<test_phases> test_phases = new List<test_phases>();

        public Station(string _sn, string _test_station, string startTime, string _mode, string _QSDKVER, string swVer)
        {
            serial = _sn;
            test_station = _test_station;
            start_time = startTime;
            mode = _mode;
            luxshare_qsdk_version = _QSDKVER;
            test_software_version = swVer;  //测试软件版本。
        }

        public void CopyToMES(MesPhases mesPhases)
        {
            mesPhases.serial = serial;
            if (status == "passed")
                mesPhases.status = "PASS";
            else if (status == "failed")
                mesPhases.status = "FAIL";
            mesPhases.start_time = start_time;
            mesPhases.finish_time = finish_time;
            mesPhases.error_code = error_code;
            mesPhases.error_details = error_details;
            mesPhases.test_station = test_station;
            mesPhases.test_software_version = test_software_version;
            mesPhases.mode = mode;
        }
    }

    ///sequences
    public class test_phases
    {
        public string phase_name = "";
        public string status = "canceled";
        public string start_time = "";
        public string finish_time = "";
        public string phase_details = "";
        public string error_code = "";
        public List<phase_items> phase_items = new List<phase_items>();

        public void Copy(Sequence sequence, MainForm mainFrom)
        {
            phase_name = sequence.SeqName;
           // status = sequence.TestResult ? "passed" : "failed";
           // start_time = sequence.start_time;
        //    finish_time = sequence.finish_time;
            //error_code = MainForm.error_code;
        }
    }

    //testItem
    public class phase_items
    {
        public string device = null;
        public string test = null;
        public string speed = null;
        public string name = null;
        public string value = null;
        public string unit = null;
        public string limit_min = null;
        public string limit_max = null;
        public string voltage = null;
        public string serial = null;
        public string mac = null;
        public string nor_version = null;
        public string board_register_value = null;
        public string version = null;
        public string model = null;
        public string radio = null;
        public string chain = null;
        public string frequency = null;
        public string measured_power = null;
        public string absolute_power = null;
        public string path_loss = null;
        public string rx_power = null;
        public string per = null;
        public string delta_f0_fn_max = null;
        public string delta_f1_f0 = null;
        public string delta_f1_avg = null;
        public string delta_f2_avg = null;
        public string delta_f2_max = null;
        public string delta_fn_fn5_max = null;
        public string fn_max = null;
        public string ini_freq_error = null;
        public string per_test_power = null;
        public string power = null;
        public string power_spec = null;
        public string ratio_of_f2_to_f1 = null;
        public string rx_per = null;
        public string data_rate = null;
        public string evm = null;
        public string freq_error = null;
        public string lo_leakage = null;
        public string power_accuracy = null;
        public string spectral_flatness = null;
        public string spectrum_mask = null;
        public string sym_clk_error = null;
        public string goal_power;
        public string gain;
        public string led;
        public string performance;
        public string iperf_cmd;

        /// <summary>
        ///  复制部分值到目标测试项中 items--》phase_items
        /// </summary>
        public void Copy(Items testItem)
        {
            name = testItem.ItemName;
         //   value = testItem.TestValue;
            unit = testItem.unit == "" ? null : testItem.unit;
            limit_max = testItem.Limit_max == "" ? null : testItem.Limit_max;
        //    limit_min = testItem.Limit_min == "" ? null : testItem.Limit_min;
        }
    }

    public class MesPhases
    {
        public string serial = "";
        public string start_time = "";
        public string finish_time = "";
        public string status = "";
        public string mode = "";
        public string error_code = "";
        public string error_details = "";
        public string test_station = "";
        public string IP;
        public string NO;
        public string FIRST_FAIL;
        public string test_software_version = "";
        /// <summary>
        /// 测试大项名字和测试时间
        /// </summary>
        public string VerifySFIS = null;

        public string CurrentShortTest = null;
        public string CurrentTest = null;
        public string VoltageTest = null;
        public string EnterUBootTransition = null;
        public string ThermalShutdownTest = null;
        public string ReadBoardRegisterValue = null;
        public string ReadAndUpdateLuxQsdkVersion = null;
        public string SaveIdentityInEnv = null;
        public string CPUVersionTest = null;
        public string CheckBZTBootloaderVersion = null;

        //public string LoadBZTFirmware;
        public string SubsystemTest = null;

        public string MMCReadWriteSpeedTest = null;
        public string USBReadWriteTest = null;
        public string ResetButtonTest = null;
        public string LEDFunctionTest = null;
        public string EthernetFunctionTest = null;
        public string CheckArtPartition = null;
        public string SpruceCanCommunicateTest = null;

        public string VerifySFIS_Time = null;
        public string CurrentShortTest_Time = null;
        public string CurrentTest_Time = null;
        public string VoltageTest_Time = null;
        public string EnterUBootTransition_Time = null;
        public string ThermalShutdownTest_Time = null;

        public string ReadBoardRegisterValue_Time = null;
        public string ReadAndUpdateLuxQsdkVersion_Time = null;
        public string SaveIdentityInEnv_Time = null;
        public string CPUVersionTest_Time = null;
        public string CheckBZTBootloaderVersion_Time = null;
        public string SubsystemTest_Time = null;
        public string MMCReadWriteSpeedTest_Time = null;
        public string USBReadWriteTest_Time = null;
        public string ResetButtonTest_Time = null;
        public string LEDFunctionTest_Time = null;
        public string EthernetFunctionTest_Time = null;
        public string CheckArtPartition_Time = null;
        public string SpruceCanCommunicateTest_Time = null;

        public string VerifyDUT = null;
        public string ThermalShutdownCheck = null;
        public string RadioValidation = null;
        public string RadioCalibration = null;
        public string RadioCalibration_5G = null;
        public string RadioValidation_5G = null;
        public string RadioCalibration_2G = null;
        public string RadioValidation_2G = null;

        public string VerifyDUT_Time = null;
        public string ThermalShutdownCheck_Time = null;
        public string RadioValidation_Time = null;
        public string RadioCalibration_Time = null;
        public string RadioCalibration_5G_Time = null;
        public string RadioValidation_5G_Time = null;
        public string RadioCalibration_2G_Time = null;
        public string RadioValidation_2G_Time = null;

        public string OpenShortCurrentTest = null;
        public string ReportChildBoard = null;
        public string LEDIrradianceTest = null;
        public string EthernetSpeedTest = null;

        public string OpenShortCurrentTest_Time = null;
        public string ReportChildBoard_Time = null;
        public string LEDIrradianceTest_Time = null;
        public string EthernetSpeedTest_Time = null;

        public string TemperatureResult_AfterBoot = null;
        public string WiFiTransmitPowerTest = null;
        public string TempSensorTest_AfterWiFiTXPowerTest = null;
        public string DesenseTest_WiFi = null;
        public string TempSensorTest_AfterDesenseWiFiTest = null;
        public string BluetoothValidation = null;
        public string RadioValidation_Zigbee = null;
        public string SetIPAddress = null;

        public string TemperatureResult_AfterBoot_Time = null;
        public string WiFiTransmitPowerTest_Time = null;
        public string TempSensorTest_AfterWiFiTXPowerTest_Time = null;
        public string DesenseTest_WiFi_Time = null;
        public string TempSensorTest_AfterDesenseWiFiTest_Time = null;
        public string BluetoothValidation_Time = null;
        public string RadioValidation_Zigbee_Time = null;
        public string SetIPAddress_Time = null;

        public string TempSensorTest_AfterBoot = null;
        public string LoadBZTFirmware = null;
        public string WiFiSpeedTest = null;
        public string TempSensorTest_AfterWiFiSpeedTest = null;
        public string BluetoothFunctionTest = null;
        public string ZigbeeFunctionTest = null;
        public string SetBootcmdtoDHCP = null;

        public string TempSensorTest_AfterBoot_Time = null;
        public string LoadBZTFirmware_Time = null;
        public string WiFiSpeedTest_Time = null;
        public string TempSensorTest_AfterWiFiSpeedTest_Time = null;
        public string BluetoothFunctionTest_Time = null;
        public string ZigbeeFunctionTest_Time = null;
        public string SetBootcmdtoDHCP_Time = null;

        public string ReadBTZFwVersion = null;
        public string PowerCycleTest = null;
        public string WaitForTelnet = null;
        public string RAMStressTest = null;
        public string CPUStressTest = null;
        public string TempSensorTest_AfterCPUStressTest = null;
        public string MMCStressTest = null;
        public string HardwareReset = null;
        public string TempSensorTest_AfterMMCReadWriteSpeedTest = null;
        public string JSON_UPLOAD = null;

        public string ReadBTZFwVersion_Time = null;
        public string PowerCycleTest_Time = null;
        public string WaitForTelnet_Time = null;
        public string RAMStressTest_Time = null;
        public string CPUStressTest_Time = null;
        public string TempSensorTest_AfterCPUStressTest_Time = null;
        public string MMCStressTest_Time = null;
        public string HardwareReset_Time = null;
        public string TempSensorTest_AfterMMCReadWriteSpeedTest_Time = null;
        public string JSON_UPLOAD_Time = null;

        /// <summary>
        /// 测试参数上传
        /// </summary>
        public string Ledoff = null;

        public string W_x = null;
        public string W_y = null;
        public string W_L = null;
        public string B_x = null;
        public string B_y = null;
        public string B_L = null;
        public string G_x = null;
        public string G_y = null;
        public string G_L = null;
        public string R_x = null;
        public string R_y = null;
        public string R_L = null;

        public string USBC_VBUS = null;
        public string GND = null;
        public string DVDD3_3 = null;
        public string DVDD1_95 = null;
        public string DVDD2_2 = null;
        public string DVDD0_912 = null;
        public string DVDD1_29_MP_CORE = null;
        public string DVDD1_35_MP_DDR = null;

        /// <summary>
        ///  MBLT
        /// </summary>
        public string FW_VERSION = null;

        public string HW_REVISION = null;
        public string SW_VERSION = null;
        public string CURRENT_OPEN = null;
        public string CURRENT_IDLE = null;
        public string EMMC_VENDOR = null;
        public string MMCWrite117 = null;
        public string MMCWrite120 = null;
        public string MMCRead017 = null;
        public string MMCRead020 = null;
        public string LED_R_ON = null;
        public string LED_B_ON = null;
        public string LED_G_ON = null;
        public string LED_W_ON = null;
        public string ETH0_THROUGHPUT = null;
        public string ETH1_THROUGHPUT = null;
        public string CURRENT_SHORT;
        public string LED_ALLOFF;
        public string Temp_AfterBoot;
        public string LoadWiFiDrivers;
        public string Temp_AfterWiFiSpeedTest;
        public string SetBootcmdToDHCP;
        public string USB_WRITE_SPEED;
        public string USB_READ_SPEED;
        public string WIFI2G_THROUGHPUT_SERIAL;
        public string WIFI5G_THROUGHPUT_SERIAL;
        public string WIFI2G_THROUGHPUT_PARALLEL;
        public string WIFI5G_THROUGHPUT_PARALLEL;
        public string MES_UPLOAD;
        public string ETH0_THROUGHPUT_SEND;
        public string ETH0_THROUGHPUT_RECEIVE;
        public string ETH1_THROUGHPUT_SEND;
        public string ETH1_THROUGHPUT_RECEIVE;
        public string LoadBZTFDrivers;
        public string LoadBZTFDrivers_Times;
        public string LED_MODEL;
        public string PHY_STATUS;
        public string FUSB_STATUS;
        public string LED_OFF_LUM;
        public string ChildBoardSN;
    }
}