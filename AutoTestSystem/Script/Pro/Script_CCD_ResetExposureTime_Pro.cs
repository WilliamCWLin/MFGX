using AutoTestSystem.Base;
using AutoTestSystem.Equipment.CCD;
using AutoTestSystem.Equipment.ControlDevice;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_CCD_ResetExposureTime_Pro : Script_CCD_Base
    {

        private Dictionary<string, string> OutData = new Dictionary<string, string>();
        string strOutData = string.Empty;
        string Status = string.Empty;
        int Formate;
        int errorcode;
        string SaveImagePath = string.Empty;
        private string _strParamInfoPath;
        private string _strFormat;
        private string _strFileName;        
        string jsonStr = string.Empty;
        Stopwatch stopwatch = new Stopwatch();

        [Category("Intensity"), Description("影像亮度 支援%%變數")]
        public string ImageIntensity { get; set; }

        [Category("Intensity"), Description("設定要改變曝光時間的影像亮度值範圍[Ymin;Ymax],[Ymin1;Ymax1],...."),Editor(typeof(CommandEditor), typeof(UITypeEditor))]
        public string SetChangeExposureTimeY_MinMax { get; set; }

        [Category("Exposure Time"), Description("SetExposureTime 支援%%變數")]
        public string ChangeExposureTime { get; set; } = "%ExposureTime%";

        [Category("DelayTime"), Description("DelayTime after SetExposureTime ")]
        public int ExposureDelayTime { get; set; } = 1000;

        [Category("FileCopyMethod"), Description("資料複製的方式"), TypeConverter(typeof(FileToCopy))]
        public string FileCopyMethod { get; set; } = "File.Copy";

        [Category("FileSave"), Description("存圖副檔名選擇"), TypeConverter(typeof(ImageFormate))]
        //public string SaveFormat { get; set; } = "jpg";
        public string SaveFormat
        {
            get
            {
                if (_strFormat == null)
                    return "jpg";
                else
                    return _strFormat;
            }
            set
            {
                _strFormat = value;
                CheckFormat();
            }
        }

        [Category("FileSave"), Description("存圖檔案名稱(不需要填寫副檔名)支援用%%方式做變數值取代")]
        //public string SaveFileName { get; set; } = "image01";
        public string SaveFileName
        {
            get
            {
                if (_strFileName == null)
                    return "image01";
                else
                    return _strFileName;
            }
            set
            {
                _strFileName = value;
                CheckFormat();
            }
        }

        [Category("FileSave"), Description("選擇存取影像文件夾"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string SavePath { get; set; }

        [Category("FileBackup"), Description("備份檔案名稱(不需要填寫副檔名)(預設為%ProductSN%_%SaveFileName%_%StationName%_%FixtureName%_%NowTime%)支援用%%方式做變數值取代")]
        public string BackupFileName { get; set; } = "%ProductSN%_%SaveFileName%_%StationName%_%FixtureName%_%NowTime%";

        [Category("FileBackup"), Description("選擇存取備份影像文件夾\r\n(若為網路磁盤(Z:\\ H:\\等)，需再填寫 \"網路磁碟IP指定路徑\")"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string SaveBackupPath { get; set; }

        [Category("FileBackup"), Description("網路磁碟IP指定路徑(\\\\xxx.xxx.xxx.xxx\\filename)"), Editor(typeof(NetWorkEditor), typeof(UITypeEditor))]
        public string Network_Path
        {
            get
            {
                GetNetworkpath(_strParamInfoPath);
                return _strParamInfoPath;
            }
            set { _strParamInfoPath = value; }
        }


        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            if (string.IsNullOrEmpty(PopMoreData("ExposureTime")))
            {
                LogMessage($"ExposureTime can not be Empty or null in CCD_Capture_Pro script", MessageLevel.Warn);
                return false;
            }
            if (string.IsNullOrEmpty(ReplaceProp(ImageIntensity)))
            {
                LogMessage($"ImageIntensity can not be Empty or null", MessageLevel.Warn);
                return false;
            }
            if(string.IsNullOrEmpty(SetChangeExposureTimeY_MinMax))
            {
                LogMessage($"Y_Min and Y_Max can not be Empty or null in SetChangeExposureTimeY_MinMax Form", MessageLevel.Warn);
                return false;
            }

            GetNetworkpath(Network_Path);
            SaveImagePath = string.Empty;
            jsonStr = string.Empty;
            Formate = 0;
            errorcode = 99;
            OutData.Clear();
            Status = "Waiting";
            return true;
        }

        public override bool Process(CCDBase CCD,ref string strOutData)
        {
            string ExposureTimeOutputData = string.Empty;
            bool ResetPassFail = ResetExposureTime(CCD,ref ExposureTimeOutputData);
            strOutData = ExposureTimeOutputData;
            jsonStr = strOutData;
            if (ResetPassFail)
                return true;
            else
                return false;
        }
        public override bool PostProcess()
        {
            if (Spec != string.Empty && Spec != null)
            {
                string ret = string.Empty;
                ret = CheckRule(jsonStr, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Debug);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;
        }
        public bool ResetExposureTime(CCDBase CCD,ref string OutputData)
        {
            bool CCD_Capture;
            bool NotChangeEx_Flag = true;
            string DataOutput = string.Empty;
            string pattern = @"\[([^;]+);([^;]+)\]";
            double Intensity = double.Parse(ReplaceProp(ImageIntensity)); //ImageIntensity 值

            OutData.Add("CCD_ImageIntensity", ReplaceProp(ImageIntensity));
            LogMessage($"CCD_ImageIntensity:{ReplaceProp(ImageIntensity)}", MessageLevel.Info);

            int ExposureTime = int.Parse(PopMoreData("ExposureTime"));
            OutData.Add("ExposureTime", PopMoreData("ExposureTime"));
            LogMessage($"ExposureTime:{PopMoreData("ExposureTime")}", MessageLevel.Info);

                MatchCollection Resetmatches = Regex.Matches(SetChangeExposureTimeY_MinMax, pattern);

                foreach (Match Resetmatch in Resetmatches)
                {
                    double ymin = double.Parse(Resetmatch.Groups[1].Value);
                    double ymax = double.Parse(Resetmatch.Groups[2].Value);

                    if (Intensity >= ymin && Intensity <= ymax)
                    {
                        NotChangeEx_Flag = false;
                        bool ExposureRet = replaceWithProp(ChangeExposureTime, ref DataOutput);
                        int roundedData = (int)Math.Floor(double.Parse(DataOutput));
                        PushMoreData("ChangeExposureTime", roundedData.ToString());
                        LogMessage($"{Description}:Set_ChangeExposure({roundedData})", MessageLevel.Debug);
                        OutData.Add("ChangeExposureTime", roundedData.ToString());
                        if (ExposureRet)
                        {
                            if (!CCD.Set_Exposure(roundedData))
                            {
                                Status = "SetChangeExposureTime_error";
                                OutData.Add("CCD_SetParam_Status", Status);
                                OutputData = JsonConvert.SerializeObject(OutData, Formatting.Indented);
                                LogMessage($"Script_CCD_ResetExposureTime Fail:{Status}", MessageLevel.Debug);
                                return false;
                            }

                             Sleep(ExposureDelayTime);
                            CCD_Capture = Capture_CCD_Image(CCD);
                            if (!CCD_Capture)
                            {
                                OutputData = JsonConvert.SerializeObject(OutData, Formatting.Indented);
                                LogMessage($"Script_CCD_ResetExposureTime Fail:{OutputData}", MessageLevel.Warn);
                                return false;
                            }
                        }
                        else
                        {
                            LogMessage($"Replace CCD ExposureTime Fail", MessageLevel.Warn);
                            Status = "Replace CCD ExposureTime Fail";
                            OutData.Add("CCD_SetParam_Status", Status);
                            OutputData = JsonConvert.SerializeObject(OutData, Formatting.Indented);
                            LogMessage($"Script_CCD_ResetExposureTime Fail:{Status}", MessageLevel.Debug);
                            return false;
                        }
                    }
                }

            if (NotChangeEx_Flag)
            {
                PushMoreData("ChangeExposureTime", ExposureTime.ToString());
                LogMessage($"====Not to Change Exposure Time====", MessageLevel.Debug);
                if (!CCD.Set_Exposure(ExposureTime))
                {
                    Status = "SetChangeExposureTime_error";
                    OutData.Add("CCD_SetParam_Status", Status);
                    OutputData = JsonConvert.SerializeObject(OutData, Formatting.Indented);
                    LogMessage($"Script_CCD_ResetExposureTime Fail:{Status}", MessageLevel.Debug);
                    return false;
                }
                Sleep(ExposureDelayTime);
            }
           

            OutputData = JsonConvert.SerializeObject(OutData, Formatting.Indented);
            LogMessage($"Script_CCD_ResetExposureTime Success:{OutputData}", MessageLevel.Debug);

            return true;
        }
        public bool Capture_CCD_Image(CCDBase CCD)
        {
            string strExOutData = string.Empty;
            string Backup_Path = string.Empty;
            string NetworkDrvepath = string.Empty;
            string FileNameReplace = string.Empty;
            string BackupFileNameReplace = string.Empty;
            string TotalBackupFileName = string.Empty;
            bool pass_fail = true;
            if (SavePath != string.Empty && SavePath != null)
            {
                string ReplaceSavePath = ReplaceProp(SavePath);
                //SaveCCDImage
                OutData.Add("SavePath", Path.GetFullPath(ReplaceSavePath));
                //檢查路徑是否存在
                if (!Directory.Exists(Path.GetFullPath(ReplaceSavePath)))
                {
                    Status = "SavePath_not_exist";
                    errorcode = -2;
                    LogMessage($"SavePath is not exist!!", MessageLevel.Warn);
                    pass_fail = false;
                }
                else
                {
                    if (SaveFileName != null && SaveFileName != string.Empty)
                    {
                        FileNameReplace = ReplaceProp(SaveFileName);
                        //Check FileName Subfile is exist or not
                        if (FileNameReplace.Contains(SaveFormat))
                            PushMoreData("SaveFileName", FileNameReplace.Replace($".{SaveFormat}", ""));
                        else
                            PushMoreData("SaveFileName", FileNameReplace);

                        LogMessage($"{Description} The Backup FileName is {FileNameReplace}", MessageLevel.Debug);
                        OutData.Add("FileName", FileNameReplace);
                        if (SaveFormat == "bmp")
                        {
                            Formate = 0;
                        }
                        else if (SaveFormat == "jpg")
                        {
                            Formate = 1;
                        }
                        else
                        {
                            LogMessage($"Formate Error: Formate Undefinded!!!", MessageLevel.Error);
                            return false;
                        }



                        if (FileNameReplace.Contains(SaveFormat))
                            SaveImagePath = ReplaceSavePath + "/" + FileNameReplace;
                        else
                            SaveImagePath = ReplaceSavePath + "/" + FileNameReplace + "." + SaveFormat;

                        if (!CCD.SaveImage(Formate, SaveImagePath))
                        {
                            Status = "CCD_SaveNG";
                            errorcode = -4;
                            LogMessage("CCD.SaveImage Fail", MessageLevel.Debug);
                            pass_fail = false;
                        }
                        else
                        {
                            Sleep(500);
                            if (!File.Exists(SaveImagePath))//確認檔案是否存在
                            {
                                Status = $"ImageFileName:{Path.GetFileName(SaveImagePath)}_not_exist";
                                errorcode = -5;
                                LogMessage(Status, MessageLevel.Debug);
                                pass_fail = false;
                            }
                            else
                            {
                                if (SaveBackupPath != null && SaveBackupPath != string.Empty)
                                {
                                    string ReplaceSaveBackupPath = ReplaceProp(SaveBackupPath);
                                    OutData.Add("BackupPath", Path.GetFullPath(ReplaceSaveBackupPath));

                                    if (BackupFileName == null || BackupFileName == string.Empty)
                                        TotalBackupFileName = PopMoreData("ProductSN") + DateTime.Now.ToString("_yyyyMMdd_HHmmss");
                                    else
                                    {
                                        BackupFileNameReplace = ReplaceProp(BackupFileName);
                                        //Check BackupFile Subfile is exist or not
                                        if (BackupFileNameReplace.Contains(SaveFormat))
                                            TotalBackupFileName = BackupFileNameReplace;
                                        else
                                            TotalBackupFileName = BackupFileNameReplace + "." + SaveFormat;
                                    }
                                    LogMessage($"{Description} The Backup FileName is {TotalBackupFileName}", MessageLevel.Debug);
                                    OutData.Add("BackupFileName", TotalBackupFileName);

                                    //DirectoryInfo di = null;
                                    if (IsNetworkDrive(ReplaceSaveBackupPath, ref NetworkDrvepath))
                                    {
                                        if (NetworkDrvepath != string.Empty)
                                            Backup_Path = NetworkDrvepath + "/" + TotalBackupFileName;
                                        else
                                            Backup_Path = ReplaceSaveBackupPath + "/" + TotalBackupFileName;

                                        try
                                        {
                                            int timeoutMilliseconds = 5000;//5秒
                                                                           // 使用 Task.Run 來執行複製操作，並設定 CancellationTokenSource 來處理 timeout
                                            using (var cancellationTokenSource = new CancellationTokenSource())
                                            {
                                                var cancellationToken = cancellationTokenSource.Token;
                                                var copyTask = Task.Run(() =>
                                                {
                                                    FileCopy(SaveImagePath, Backup_Path, FileCopyMethod);
                                                }, cancellationToken);
                                                if (!copyTask.Wait(timeoutMilliseconds, cancellationToken))
                                                {
                                                    cancellationTokenSource.Cancel(); // 如果超時，取消複製操作
                                                    LogMessage($"Copy File is Timout: [{timeoutMilliseconds / 1000}]s。", MessageLevel.Warn);
                                                }
                                                LogMessage($"File.Copy to {Backup_Path}", MessageLevel.Debug);
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            LogMessage($"File.Copy Error => {ex.Message}, Please Check SavePath:{SaveImagePath} and BackupPath:{Backup_Path}", MessageLevel.Error);
                                            Status = "File.Copy_Error";
                                            OutData.Add("Exception", ex.Message);
                                            errorcode = -9;
                                            pass_fail = false;
                                        }

                                    }
                                    else
                                    {
                                        //LogMessage($"Backup Path:{SaveBackupPath} is not exist or lossing,Please Re-select Backup Path and retry again!!!", MessageLevel.Debug);
                                        Status = "SaveBackupPath_NG";
                                        errorcode = -6;
                                        MessageBox.Show($"[{Description}] Backup Path:{ReplaceSaveBackupPath} is not exist or lossing,Please Re-select Backup Path and retry again!!!", "Path Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        pass_fail = false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Status = "FileName_null";
                        errorcode = -3;
                        LogMessage($"FileName is Empty!!!", MessageLevel.Debug);
                        pass_fail = false;
                    }
                }
            }
            else
            {
                Status = "SavePath_is_empty";
                errorcode = -1;
                LogMessage($"SavePath is empty!!", MessageLevel.Warn);
                pass_fail = false;
            }

            if (pass_fail)
            {
                errorcode = 0;
                Status = "Save_OK";
            }
            OutData.Add("CCD_SaveImage_Status[Errorcode]", $"{Status}[{errorcode}]");
            OutData.Add("errorCode", $"{errorcode}");
            return pass_fail;
        }

        public bool replaceWithProp(string input_string, ref string output_string)
        {
            
            bool pass_fail = true;
            double output_value = 0;
            string originInput = input_string;
            string NGStatus = string.Empty;

            // 正規表達式來匹配 %任意文字%
            Regex regex = new Regex(@"%([^%]+)%");

            // 尋找匹配的 %%
            MatchCollection matches = regex.Matches(input_string);

            // 迭代每個匹配
            foreach (Match match in matches)
            {
                // 取得匹配的 key
                string key = match.Groups[1].Value;

                if (key != null)
                {
                    // 使用 GetMoreProp 方法取得對應的 value 並進行替換
                    string value = PopMoreData(key);
                    input_string = input_string.Replace(match.Value, value);
                }
                else
                {
                    // 如果沒有匹配的 key，則移除佔位符
                    input_string = input_string.Replace(match.Value, "");
                }
            }
            if (ContainsMathExpression(input_string))
            {
                if (IsValidExpression(input_string))
                {
                    try
                    {
                        output_value = EvaluateExpression(input_string);
                        if (double.IsInfinity(output_value) || double.IsNaN(output_value))
                        {
                            NGStatus = $"invalid value[Infinity] or [IsNaN]";
                            LogMessage($"Match Result:{NGStatus}", MessageLevel.Warn);
                            return false;
                        }
                        output_string = output_value.ToString();
                        LogMessage($"Match Result:{originInput} Replace To {output_string}", MessageLevel.Info);
                    }
                    catch (Exception ex)
                    {
                        NGStatus = $"invalid value[{ex.Message}]";
                        LogMessage($"Match Result:{NGStatus}", MessageLevel.Error);
                        return false;
                    }
                }else
                {
                    NGStatus = $"invalid Expression";
                    LogMessage($"Match Result:{NGStatus}", MessageLevel.Warn);
                    return false;
                }
            }
            else
            {
                output_string = input_string;
                LogMessage($"Match Result:{output_string}", MessageLevel.Info);
            }

            return true;
        }

        static bool ContainsMathExpression(string input)
        {
            // 正則表達式匹配簡單的運算式子
            string pattern = @"[\d\s]*[\+\-\*\/][\d\s]*";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }

        static bool IsValidExpression(string input)
        { // 正則表達式匹配有效的運算方程式 
            string pattern = @"^\s*[\+\-]?\s*\d+(\.\d+)?\s*([\+\-\*/\^]\s*[\+\-]?\s*\d+(\.\d+)?\s*)*$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }
        static double EvaluateExpression(string expression)
        {
            // 使用正則表達式處理次方運算，先進行次方運算再用DataTable計算
            string processedExpression = ProcessExponentiation(expression);
            // 使用 DataTable 計算簡單的數學表達式
            var table = new DataTable();
            var result = table.Compute(processedExpression, string.Empty);
            return Convert.ToDouble(result);
        }

        static string ProcessExponentiation(string expression)
        {
            // 正則表達式查找次方運算
            Regex regex = new Regex(@"\d+(\.\d+)?\s*\^\s*\d+(\.\d+)?");
            Match match;
            // 不斷查找並替換次方運算
            while ((match = regex.Match(expression)).Success)
            {
                // 提取匹配到的次方運算子
                string exp = match.Value;
                // 分割次方運算式
                string[] numbers = exp.Split('^');
                double baseNumber = Convert.ToDouble(numbers[0]);
                double exponent = Convert.ToDouble(numbers[1]);
                // 計算次方
                double powerResult = Math.Pow(baseNumber, exponent);
                // 用次方運算結果替換原始表達式
                expression = expression.Replace(exp, powerResult.ToString());
            }
            return expression;
        }

        public void FileCopy(string sourceFilePath, string targetFilePath, string FileCopyMethod)
        {
            stopwatch.Start();
            if (FileCopyMethod == "File.Copy")
                File.Copy(sourceFilePath, targetFilePath);
            else
            {
                using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open))
                {
                    using (var targetStream = new FileStream(targetFilePath, FileMode.Create))
                    {
                        using (var bufferedSource = new BufferedStream(sourceStream))
                        {
                            using (var bufferedTarget = new BufferedStream(targetStream))
                            {
                                bufferedSource.CopyTo(bufferedTarget);
                            }
                        }
                    }
                }
            }
            stopwatch.Stop();
            LogMessage($"Copy File cost:{stopwatch.Elapsed}!!", MessageLevel.Debug);
            stopwatch.Reset();
        }

        public void CheckFormat()
        {
            //Check Filesubfile is eqeal with SaveFormat or not
            if (string.IsNullOrEmpty(SaveFileName))
                SaveFileName = "image01";
            if (string.IsNullOrEmpty(SaveFormat))
                SaveFormat = "jpg";
            else
            {
                string CheckFormat = $".{SaveFormat}";
                if (!CheckFormat.Contains(Path.GetExtension(SaveFileName)))
                {
                    MessageBox.Show($"Formate Error: FileName subfile {Path.GetExtension(SaveFileName)} is not eqeal with SaveFormat {CheckFormat}!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        public class FileToCopy : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> CopyListKeys = new List<string>();

                CopyListKeys.Add("File.Copy");
                CopyListKeys.Add("BufferedStream.Copy");

                return new StandardValuesCollection(CopyListKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }

        public class ImageFormate : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> ImageFormateKeys = new List<string>();

                ImageFormateKeys.Add("bmp");
                ImageFormateKeys.Add("jpg");

                return new StandardValuesCollection(ImageFormateKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }
    }
}
