using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using static AutoTestSystem.BLL.Bd;
using System.ComponentModel;
using System.Drawing.Design;
using Manufacture;
using AutoTestSystem.Model;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AutoTestSystem.Equipment.ControlDevice;
using static AutoTestSystem.Script.Script_ControlDevice_Base;

namespace AutoTestSystem.Script
{
    internal class Script_DUT_IQ_Process_Tuning : ScriptDUTBase
    {
        string strOutData = string.Empty;
        int Format = 0;
        int errorcode;
        string SaveImagePath = string.Empty;
        string receivedData = string.Empty;
        string messageBoxMessage = string.Empty;
        bool isTimeout = true;
        TcpIpClient Tcp;

        //[JsonIgnore]
        //public Shift_Spec ShiftSpeclist;

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(CTRL_DevList))]
        public string ControlDeviceSel { get; set; }

        [Category("Camera Control Parameter"), Description("Camera URL")]
        public string URL_Name { get; set; } = "";

        [Category("ImageSave"), Description("存圖檔案名稱(不需要填寫副檔名)支援用%%方式做變數值取代")]
        public string SaveFileName { get; set; } = "image01";

        [Category("ImageSave"), Description("存圖副檔名選擇"), TypeConverter(typeof(ImageFormat))]
        public string SaveFormat { get; set; } = "jpg";

        [Category("ImageSave"), Description("選擇存取影像文件夾"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string SavePath { get; set; }


        [Category("SE Parameters"), Description("SingleEntry Command")]
        public string SetCommand { get; set; }

        [Category("SE Parameters"), Description("SingleEntry Profile Path"),Editor(typeof(FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string Img_source { get; set; }

        [Category("Tuning Parameters"), Description("Tuning Parameters"),Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        public string Tuning_Comment { get; set; }
        [Category("Time Set"), Description("Timeout")]
        public int Timeout { get; set; } = 10000;
        [Category("Time Set"), Description("Delay Time")]
        public int DelayTime { get; set; } = 1000;
       
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess()
        {
            SaveImagePath = string.Empty;
            isTimeout = true;

            return true;
        }
        
        public override bool Process(DUT_BASE DUTDevice,ref string OutData)
        {
            bool Tuning_result = true;
            string FileNameReplace = string.Empty;
            string SingleEntry_output = string.Empty;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            FileNameReplace = ReplaceProp(URL_Name);
            while (stopwatch.ElapsedMilliseconds < Timeout * 2)
            {
                DUTDevice.Init(FileNameReplace);


                DUTDevice.Play();
                //DUTDevice.Play(ref buffer);

                //buffer 餵入kj算法  Oscar 新增buffer mode

                if (SaveFormat == "bmp")
                    Format = 0;
                else if (SaveFormat == "jpg")
                    Format = 1;

                FileNameReplace = ReplaceProp(SaveFileName);
                if (FileNameReplace.Contains(SaveFormat))
                    SaveImagePath = SavePath + "/" + FileNameReplace;
                else
                    SaveImagePath = SavePath + "/" + FileNameReplace + "." + SaveFormat;


                if (!DUTDevice.SaveImage(Format, SaveImagePath))
                {
                    //Status = "CCD_SaveNG";
                    errorcode = -4;
                    LogMessage("DUT.SaveImage Fail", MessageLevel.Debug);
                    return false;
                }

                Tuning_result = Tuning_Action(ref SingleEntry_output);

                DUTDevice.UnInit();

                if (Tuning_result)
                {
                    break;
                }else
                {
                    LogMessage($"Script_DUT_IQ_Process_Tuning: Tuning_Action Fail", MessageLevel.Warn);
                    return false;
                }
            }
            OutData = SingleEntry_output;
            strOutData = OutData;

            if (isTimeout)
            {
                LogMessage($"Script_DUT_IQ_Process_Tuning: TimeOut", MessageLevel.Warn);
                return false;
            }

            return true;
        }

        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);
            //ExtraProcess(ref result);

            if (result == "PASS" || Spec == "")
            {
                return true;
            }             
            else
            {
                LogMessage($"{result}",MessageLevel.Error);
                return false;
            }
    
        } 
        
        public bool Tuning_Action(ref string output)
        {
            string TCP_type = string.Empty;
            string TCP_Send = string.Empty;
            string TCP_Read = string.Empty;
            string OC_Shift_X = string.Empty;
            string OC_Shift_Y = string.Empty;
            string Rotate_Shift = string.Empty;
            string SE_output = string.Empty;
            string TCP_msg = string.Empty;
            bool tcp_ret = false;
            bool Tuning_result = false;
            try
            {
                var SingleEntry = new Script_SingleEntry_Calculater
                {
                    Send_Command = SetCommand,
                    FilePath = Img_source
                };

                if (SingleEntry.PreProcess())
                {
                    if (!SingleEntry.Process(ref SE_output))
                    {
                        LogMessage("SingleEntry processing failed.", MessageLevel.Error);
                        return false;
                    }
                }

                output = SE_output;
                //ShiftSpec_Parse(ShiftSpecParam);
                /*string OC_Shift_X = PopMoreData("OC_Shift_X");
                string OC_Shift_Y = PopMoreData("OC_Shift_Y");
                string Rotate_Shift = PopMoreData("Rotate_Shift");*/
                if (SingleEntry.tmp_ParseConfig != null)
                {
                    foreach (var Shift in SingleEntry.tmp_ParseConfig)
                    {
                        if (Shift.Key == "Result__Shift_X_mm")
                        {
                            OC_Shift_X = Shift.Value;
                        }
                        else if (Shift.Key == "Result__Shift_Y_mm")
                        {
                            OC_Shift_Y = Shift.Value;
                        }
                        else if (Shift.Key == "Result__Rotation_Angle")
                        {
                            Rotate_Shift = Shift.Value;
                        }
                    }
                }
                else
                {
                    LogMessage($"SingleEntry ParseConfig is Null", MessageLevel.Warn);
                    return false;
                }

                if (string.IsNullOrEmpty(OC_Shift_X) && string.IsNullOrEmpty(OC_Shift_Y) && string.IsNullOrEmpty(Rotate_Shift))
                {
                    LogMessage($"OC_Shift_X:{OC_Shift_X} OC_Shift_Y:{OC_Shift_Y} Rotate_Shift:{Rotate_Shift} exist Empty or Null", MessageLevel.Warn);
                    return false;
                }

                double Shift_X = double.Parse(OC_Shift_X);
                double Shift_Y = double.Parse(OC_Shift_Y);
                double Rotate = double.Parse(Rotate_Shift);
                LogMessage($"Shift_X :{Shift_X},Shift_Y:{Shift_Y}, Rotate:{Rotate}", MessageLevel.Info);
                //TCP_Tuning-Start
                if (!string.IsNullOrEmpty(Tuning_Comment))
                {
                    List<Tuning_Cmd> tcp_list = JsonConvert.DeserializeObject<List<Tuning_Cmd>>(Tuning_Comment);
                    double X_min = 99999, X_max = -99999, Y_min = 99999, Y_max = -99999, U_min = 99999, U_max = -99999;

                    foreach (var list in tcp_list)
                    {
                        string TcpData = string.Empty;
                        tcp_ret = false;
                        LogMessage($"TCP_Cmd: {list.TcpCmd}, TCP_Value: {list.TcpValue}, ShiftSpec_min :{list.ShiftSpec_min},ShiftSpec_max:{list.ShiftSpec_max}", MessageLevel.Info);
                        //TcpData = $"++{list.Key}{list.Value}";
                        //tcp_ret = Tcp_Tuning(TcpData, ref TCP_msg);
                        if (list.TcpCmd == "TX" && Shift_X <= list.ShiftSpec_min || Shift_X >= list.ShiftSpec_max)
                        {
                            X_min = list.ShiftSpec_min;
                            X_max = list.ShiftSpec_max;
                            if (Shift_X <= X_min)
                                TcpData = $"++{list.TcpCmd}{list.TcpValue}";
                            if (Shift_X >= X_max)
                                TcpData = $"--{list.TcpCmd}{list.TcpValue}";

                            tcp_ret = Tcp_Tuning(TcpData, ref TCP_msg);
                            if (tcp_ret == false)
                            {
                                LogMessage($"Tcp Fail: {TCP_msg}", MessageLevel.Warn);
                                return false;
                            }
                        }
                        else if (list.TcpCmd == "TY" && Shift_Y <= list.ShiftSpec_min || Shift_Y >= list.ShiftSpec_max)
                        {
                            Y_min = list.ShiftSpec_min;
                            Y_max = list.ShiftSpec_max;
                            if (Shift_Y <= Y_min)
                                TcpData = $"++{list.TcpCmd}{list.TcpValue}";
                            if (Shift_Y >= Y_max)
                                TcpData = $"--{list.TcpCmd}{list.TcpValue}";

                            tcp_ret = Tcp_Tuning(TcpData, ref TCP_msg);
                            if (tcp_ret == false)
                            {
                                LogMessage($"Tcp Fail: {TCP_msg}", MessageLevel.Warn);
                                return false;
                            }
                        }
                        else if (list.TcpCmd == "TU" && Rotate <= list.ShiftSpec_min || Rotate >= list.ShiftSpec_max)
                        {
                            U_min = list.ShiftSpec_min;
                            U_max = list.ShiftSpec_max;
                            if (Rotate <= U_min)
                                TcpData = $"++{list.TcpCmd}{list.TcpValue}";
                            if (Rotate >= U_max)
                                TcpData = $"--{list.TcpCmd}{list.TcpValue}";

                            tcp_ret = Tcp_Tuning(TcpData, ref TCP_msg);
                            if (tcp_ret == false)
                            {
                                LogMessage($"Tcp Fail: {TCP_msg}", MessageLevel.Warn);
                                return false;
                            }
                        }
                        Sleep(DelayTime);
                    }

                    //停止條件
                    if(Shift_X >= X_min && Shift_X <= X_max && Shift_Y >= Y_min && Shift_Y <= Y_max && Rotate >= U_min && Rotate <= U_max)
                    {
                        isTimeout = false;
                        Tuning_result = true;
                    }
                    //TCP_Tuning-END
                }
                else
                {
                    LogMessage($"TCP_Comment is Empty or Null", MessageLevel.Warn);
                    return false;
                }
            }catch(Exception TA_ex)
            {
                LogMessage($"Tuning_Action Error:{TA_ex.Message}", MessageLevel.Error);
                return false;
            }
          return Tuning_result;
        }

        public bool Tcp_Tuning(string tcp_Data, ref string Tcpoutput)
        {
            try
            {
                if (GlobalNew.Devices != null)
                {
                  
                    if((ControlDeviceBase)GlobalNew.Devices[ControlDeviceSel] is TcpIpClient)
                    {
                        Tcp = (TcpIpClient)GlobalNew.Devices[ControlDeviceSel];
                    }else
                    {
                        LogMessage($"Device is not TcpIpClient", MessageLevel.Warn);
                        return false;
                    }

                }
                    string hexSendString = $"{tcp_Data}\r\n";

                    if (!Tcp.SEND(hexSendString))
                    {
                        LogMessage("Failed to send string to server.", MessageLevel.Error);
                        if (!Tcp.ReadAfterReconnect(ref receivedData)) // 30 seconds timeout
                        {
                            Tcpoutput = receivedData;
                            messageBoxMessage = "Connected Fail，請人員檢查機台連線";
                            ShowMessageBox(messageBoxMessage);
                            LogMessage("Failed to reconnect to server within timeout.", MessageLevel.Error);
                            return false;
                        }
                    }
                    if (!Tcp.ReadAfterReconnect(ref receivedData)) // 30 seconds total timeout for read and reconnect
                    {
                        Tcpoutput = receivedData;
                        messageBoxMessage = "Connected Fail，請人員檢查機台連線";
                        ShowMessageBox(messageBoxMessage);
                        ShowMessageBox(messageBoxMessage);
                        LogMessage("Failed to receive data after reconnecting within the total timeout.", MessageLevel.Error);
                        return false;
                    }
                    else
                    {
                        string receivedString = receivedData.Trim();
                        if (receivedString == "OK")
                        {
                            Tcpoutput = receivedData;
                            LogMessage($"TCP_ReceivedData: {Tcpoutput}", MessageLevel.Info);
                        }
                        else
                        {
                            Tcpoutput = receivedData;
                            messageBoxMessage = "接收指令錯誤，請人員檢查機台是否正常運作";
                            ShowMessageBox(messageBoxMessage);
                            return false;
                        }
                    }
            }
            catch(Exception tcp_ex)
            {
                Tcpoutput = $"TcpIpClient Error: {tcp_ex.Message}\r\n";
                messageBoxMessage = $"Tcp錯誤:{tcp_ex.Message}";
                ShowMessageBox(messageBoxMessage);
                return false;
            }
            return true;
        }
        public class ImageFormat : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> ImageFormateKeys = new List<string>();

                ImageFormateKeys.Add("bmp");
                ImageFormateKeys.Add("jpg");
                ImageFormateKeys.Add("png");

                return new StandardValuesCollection(ImageFormateKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }

        private void ShowMessageBox(string message)
        {
            // Instantiate and configure Script_Extra_CheckMessageBox
            var messageBoxScript = new Script_Extra_CheckMessageBox
            {
                Message = message,
                KeyOK = "",
                KeyNO = "N",
                isFailskip_msg = false, // Example setting; adjust as needed
                ON_OFF = false // Example setting; adjust as needed
            };

            string output = string.Empty;
            if (!messageBoxScript.Process(ref output))
            {
                LogMessage("Message box processing failed or was cancelled.", MessageLevel.Error);
            }
        }

        public class FileSelEditorRelPath : System.Drawing.Design.UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "選擇檔案";
                    openFileDialog.Filter = "所有檔案 (*.*)|*.*";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedFilePath = openFileDialog.FileName;

                        // 轉換為相對路徑
                        string relativePath = GetRelativePath(selectedFilePath);
                        // 將反斜杠轉換為雙反斜杠
                        relativePath = relativePath.Replace("/", "\\");
                        return relativePath;
                    }
                }

                return value; // 如果用戶取消選擇，返回原始值
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
            private string GetRelativePath(string selectedFilePath)
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Uri baseUri = new Uri(baseDirectory);
                Uri selectedFileUri = new Uri(selectedFilePath);

                Uri relativeUri = baseUri.MakeRelativeUri(selectedFileUri);

                return Uri.UnescapeDataString(relativeUri.ToString());
            }
        }


        public class Tuning_Cmd
        {
            public string TcpCmd { get; set; }
            public string TcpValue { get; set; }
            public double ShiftSpec_min { get; set; }
            public double ShiftSpec_max { get; set; }
        }
    }
}
