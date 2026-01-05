
using AutoTestSystem.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;


namespace AutoTestSystem.Script
{
    internal class Script_FileProcessing_Pro : Script_Extra_Base
    {
        private string _strParamInfoPath;
        private string _strParamInfoPath_1;
        string NetworkDrvepath = string.Empty;
        string status = string.Empty;
        string File_status = string.Empty;
        Dictionary<string, object> data = new Dictionary<string, object>();
        //private bool IsFilenameBrowsable = true;
        private bool isBrowsable = true;

        [Category("FileProcessingType"), Description("資料複製的方式"), TypeConverter(typeof(FileProcessing))]
        public string FileProcessing_type
        {
            get
            {
               return _strParamInfoPath_1;
            }
            set { _strParamInfoPath_1 = value; }
        }

        [Category("Common Parameters"), Description("Is Visible")]
        [Browsable(false)]
        public bool IsFilenameBrowsable
        {
            get { return isBrowsable; }
            set
            {
                isBrowsable = value;
                //UpdateBrowsable();
            }
        }

        [Category("FileDest"), Description("文件存取/移動路徑\r\n(若為網路磁盤(Z:\\ H:\\等)，需再填寫 \"網路磁碟IP指定路徑\")"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        [Browsable(true)]
        public string destFilePath { get; set; }

        [Category("FileDest"), Description("檔案名稱支援%%變數")]
        [Browsable(true)]
        public string FileName { get; set; } = "%ProductSN%_%NowTime%";

        [Category("FileDest"), Description("網路磁碟IP指定路徑(\\\\xxx.xxx.xxx.xxx\\filename)"), Editor(typeof(NetWorkEditor), typeof(UITypeEditor))]
        //[Browsable(true)] // 這個屬性會隱藏在屬性網格中
        public string Network_Path
        {
            get
            {
                GetNetworkpath(_strParamInfoPath);
                return _strParamInfoPath;
            }
            set { _strParamInfoPath = value; }
        }

        [Category("FileSource(Only Select one File)"), Description("選擇\"單一文件檔名\""), Editor(typeof(FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string sourceFileName { get; set; }
        [Category("Select FolderSource Path (Select Source Folder Path )"), Description("選擇\"文件夾路徑\""), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string sourceFolderPath { get; set; }

        public void TogglePropertyVisibility(bool showProperty2)
        {
            // 根據使用者輸入更新 [Browsable] 屬性
            destFilePath = showProperty2 ? "Visible" : "Hidden";
        }

        /*public void UpdateBrowsable()
        {
            Type type = GetType();
            PropertyInfo property = type.GetProperty("Filename");
            if (property != null)
            {
                BrowsableAttribute existingAttribute = (BrowsableAttribute)Attribute.GetCustomAttribute(property, typeof(BrowsableAttribute));
                if (existingAttribute != null)
                {
                    bool newValue = isBrowsable; // 新的值
                    BrowsableAttribute newAttribute = new BrowsableAttribute(newValue);
                    // 使用反射將現有的屬性替換為新的屬性
                    FieldInfo field = typeof(BrowsableAttribute).GetField("browsable", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field != null)
                    {
                        field.SetValue(existingAttribute, newValue);
                    }
                }
            }
        }*/

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            GetNetworkpath(Network_Path);
            File_status = string.Empty;
            status = string.Empty;
            data.Clear();
            return true;

        }
        public override bool Process(ref string output)
        {
            bool pass_fail = false;
            string fullpath = string.Empty;
            string ReplaceFileName = string.Empty;
            string FileFormat = string.Empty;
            string File_Name = string.Empty;
            string file_name = string.Empty;
            data.Add("FileProcessType", FileProcessing_type);
            if (FileProcessing_type == "FileDestReName")
            {
                bool ReName = FileProcess("", "");
                if(ReName)
                    return true;
                else
                    return false;
            }
            if (sourceFileName != string.Empty && sourceFileName != null)
            {
                string ReplaceSourceFile = string.Empty;
                string ReplaceDestFile = string.Empty;
                ReplaceSourceFile = ReplaceProp(sourceFileName);
                data.Add("SourceFileName", Path.GetFullPath(ReplaceSourceFile));
                if (!File.Exists(ReplaceSourceFile))
                {
                    status = $"SourceFilePath:{Path.GetFileName(ReplaceSourceFile)}_is_not_Exists";
                    data.Add("Status", status);
                    output = JsonConvert.SerializeObject(data, Formatting.Indented);
                    LogMessage($"FileProcessing status: {status}", MessageLevel.Debug);
                    return false;
                }
                if (FileName != string.Empty)
                {
                    ReplaceFileName = ReplaceProp(FileName);
                    File_Name = Path.GetFileNameWithoutExtension(ReplaceSourceFile);
                    FileFormat = Path.GetExtension(ReplaceSourceFile);
                    if (ReplaceFileName.Contains(File_Name))
                        file_name = ReplaceFileName + FileFormat;
                    else
                        file_name = ReplaceFileName + "_" + Path.GetFileName(ReplaceSourceFile);                   
                }else
                    file_name = Path.GetFileName(ReplaceSourceFile);
                LogMessage($"FileProcessing file_name: {file_name}", MessageLevel.Debug);

                if (FileProcessing_type != "FileDelete")
                {
                    if (destFilePath != string.Empty && destFilePath != null)
                    {
                        ReplaceDestFile = ReplaceProp(destFilePath);
                        data.Add("DestFileName", Path.GetFullPath(ReplaceDestFile));

                        string ReplacesourcefileName = Path.GetFileName(ReplaceSourceFile);
                        string ReplacedestFile = Path.Combine(ReplaceDestFile, ReplacesourcefileName);
                        // 檢查目標資料夾中是否已存在相同名稱的檔案
                        if (File.Exists(ReplacedestFile))
                        {
                            LogMessage($"檔案 {ReplacesourcefileName} 已存在於目標資料夾中。", MessageLevel.Warn);
                            return false;
                        }
                        if (!IsNetworkDrive(ReplaceDestFile, ref NetworkDrvepath))
                        {
                            status = "destFilePath_NG";
                            data.Add("Status", status);
                            output = JsonConvert.SerializeObject(data, Formatting.Indented);
                            MessageBox.Show($"[{Description}] DestFilePath:{ReplaceDestFile} is not exist or lossing,Please Re-select Backup Path and retry again!!!", "Path Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }

                        if (NetworkDrvepath != string.Empty)
                            fullpath = NetworkDrvepath + "/" + file_name;
                        else
                            fullpath = ReplaceDestFile + "/" + file_name;
                    }
                    else
                    {
                        status = "DestFilePath_is_Empty";
                        data.Add("Status", status);
                        output = JsonConvert.SerializeObject(data, Formatting.Indented);
                        LogMessage($"FileProcessing status: {status}", MessageLevel.Debug);
                        return false;
                    }
                }

                pass_fail = FileProcess(ReplaceSourceFile, fullpath);

            }
            else if(sourceFolderPath != string.Empty && sourceFolderPath != null)
            {
                string ReplaceSourceFolder = string.Empty;
                string ReplaceDestFolder = string.Empty;

                ReplaceSourceFolder = ReplaceProp(sourceFolderPath);
                if (!Directory.Exists(ReplaceSourceFolder))
                {
                    Directory.CreateDirectory(ReplaceSourceFolder);

                }
                if (FileProcessing_type != "FileDelete")
                {
                    if (destFilePath != string.Empty && destFilePath != null)
                    {
                        ReplaceDestFolder = ReplaceProp(destFilePath);
                        // 確認目標資料夾是否存在，若不存在則建立
                        if (!Directory.Exists(ReplaceDestFolder))
                        {
                            Directory.CreateDirectory(ReplaceDestFolder);
                        }
                        /*if (!IsNetworkDrive(ReplaceDestFolder, ref NetworkDrvepath))
                        {
                            //MessageBox.Show($"[{Description}] DestFilePath:{destDir} is not exist or lossing,Please Re-select Backup Path and retry again!!!", "Path Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }*/
                    }
                    else
                    {
                        status = "DestFilePath_is_Empty";
                        data.Add("Status", status);
                        output = JsonConvert.SerializeObject(data, Formatting.Indented);
                        LogMessage($"FileProcessing status: {status}", MessageLevel.Debug);
                        return false;
                    }
                }

                pass_fail = ProcessDirectory(ReplaceSourceFolder, ReplaceDestFolder);

            }
            else
            {
                status = "SourceFilePath_is_Empty";
                data.Add("Status", status);
                output = JsonConvert.SerializeObject(data, Formatting.Indented);
                LogMessage($"FileProcessing status: {status}", MessageLevel.Debug);
                return false;
            }

            if (pass_fail)
            {
                status = "FileProcessing_Sucessed";
                data.Add("Status", status);
                output = JsonConvert.SerializeObject(data, Formatting.Indented);
                LogMessage($"FileProcessing status: {status}", MessageLevel.Debug);
            }
            else
            {
                status = "FileProcessing_Fail";
                data.Add("Status", status);
                output = JsonConvert.SerializeObject(data, Formatting.Indented);
                LogMessage($"FileProcessing status: {status}", MessageLevel.Warn);
                return false;
            }
                return true;
            
        }
        public bool ProcessDirectory(string sourceDir, string destDir)
        {
            try
            {
                // 處理檔案
                string[] files = Directory.GetFiles(sourceDir);             

                foreach (string file in files)
                {
                    bool file_pass_fail = false;
                    string sourcefileName = Path.GetFileName(file);
                    string destFile = string.Empty;

                    destFile = Path.Combine(destDir, sourcefileName);

                    // 檢查目標資料夾中是否已存在相同名稱的檔案
                    if (File.Exists(destFile))
                    {
                        string changeFileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}-CopyTime_{sourcefileName}";
                        LogMessage($"檔案 {sourcefileName} 已存在於目標資料夾中。更新為{changeFileName}", MessageLevel.Warn);
                        destFile = Path.Combine(destDir, changeFileName);
                        file_pass_fail = FileProcess(file, destFile);
                        if (!file_pass_fail)
                        {
                            LogMessage($"{FileProcessing_type} Fail", MessageLevel.Warn);
                            return false;
                        }
                    }
                    else
                    {
                        file_pass_fail = FileProcess(file, destFile);
                        if (!file_pass_fail)
                        {
                            LogMessage($"{FileProcessing_type} Fail", MessageLevel.Warn);
                            return false;
                        }
                    }
                }

                // 處理資料夾
                string[] directories = Directory.GetDirectories(sourceDir);
                foreach (string directory in directories)
                {
                    string dirName = Path.GetFileName(directory);
                    string destSubDir = string.Empty;
                    destSubDir = Path.Combine(destDir, dirName);

                    // 檢查目標資料夾中是否已存在相同名稱的資料夾
                    if (Directory.Exists(destSubDir))
                    {
                        LogMessage($"資料夾 {dirName} 已存在於目標資料夾中。", MessageLevel.Warn);
                        return false;
                    }

                    if (FileProcessing_type == "FileDelete")
                    {
                        Directory.Delete(directory, true);
                    }
                    else if (FileProcessing_type == "FileMove")
                    {
                        Directory.Move(directory, destSubDir);
                    }
                    else
                    {
                        bool folder_pass_fail = ProcessDirectory(directory, destSubDir);
                        if (!folder_pass_fail)
                        {
                            LogMessage($"{FileProcessing_type} Fail", MessageLevel.Warn);
                            return false;
                        }
                    }
                }

                if (files.Count() == 0 && directories.Count() == 0)
                {
                    LogMessage($"File Directory is exist Empty or Null Folder", MessageLevel.Warn);
                    return false;
                }
            }catch(Exception ex1)
            {
                LogMessage($"ProcessDirectory Exception:{ex1.Message}", MessageLevel.Error);
                return false;
            }
            return true;
        }
        public bool FileProcess(string Sourcepath,string Destpath)
        {
            bool pass_fail = true;
            Stopwatch stopwatch = new Stopwatch();
            int timeoutMilliseconds = 10000;//10秒
            // 使用 Task.Run 來執行複製操作，並設定 CancellationTokenSource 來處理 timeout             
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var cancellationToken = cancellationTokenSource.Token;
                try
                {
                    stopwatch.Start();
                    var copyTask = Task.Run(() =>
                    {
                        switch (FileProcessing_type)
                        {
                            case "FileCopy":
                                File_status = $"FileProcessing:FileCopy";
                                File.Copy(Sourcepath, Destpath);
                                // 確認來源檔案與目標檔案是否相同
                                bool areImagesIdentical = AreFilesIdentical(Sourcepath, Destpath);
                                if (!areImagesIdentical)
                                {
                                    LogMessage($"AreFilesIdentical Fail!!", MessageLevel.Error);
                                    pass_fail = false;
                                }
                                break;
                            case "FileDelete":
                                File_status = $"FileProcessing:FileDelete";
                                File.Delete(Sourcepath);
                                break;
                            case "FileMove":
                                File_status = $"FileProcessing:FileMove";
                                File.Move(Sourcepath, Destpath);
                                break;
                            case "FileDestReName":
                                File_status = $"FileProcessing:FileDestReName";
                                pass_fail = FileReName();
                                break;
                            default:
                                File_status = $"FileProcessing type is not defind";
                                LogMessage($"FileProcessing type is not defind", MessageLevel.Debug);
                                break;
                        }
                    }, cancellationToken);

                    if (!copyTask.Wait(timeoutMilliseconds, cancellationToken))
                    {
                        cancellationTokenSource.Cancel(); // 如果超時，取消複製操作
                        LogMessage($"FileProcessing is Timout: [{timeoutMilliseconds / 1000}]s。", MessageLevel.Warn);
                    }
                    if (pass_fail == false)
                    {
                        stopwatch.Stop();
                        LogMessage($"FileProcess cost:{stopwatch.Elapsed}!!", MessageLevel.Debug);
                        stopwatch.Reset();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    status = $"{File_status}_Exception => {ex.Message}";
                    data.Add($"Exception", status);
                    string outmsg = JsonConvert.SerializeObject(data, Formatting.Indented);
                    LogMessage($"FileProcessing Error: {outmsg}", MessageLevel.Error);
                    stopwatch.Stop();
                    LogMessage($"FileProcess cost:{stopwatch.Elapsed}!!", MessageLevel.Debug);
                    stopwatch.Reset();
                    return false;
                }
            }
            stopwatch.Stop();
            LogMessage($"FileProcess cost:{stopwatch.Elapsed}!!", MessageLevel.Debug);
            stopwatch.Reset();

            return true;
        }
        public override bool PostProcess()
        {
            
            return true;

        }
        public bool FileReName()
        {
            if (!string.IsNullOrEmpty(destFilePath) && !string.IsNullOrEmpty(FileName))
            {
                string ReplaceFileName = ReplaceProp(destFilePath);
                // 取得新資料夾的完整路徑
                string parentDirectory = Path.GetDirectoryName(ReplaceFileName);
                // 新的資料夾名稱
                string newFolderName = ReplaceProp(FileName);
                string newFolderPath = Path.Combine(parentDirectory, newFolderName);
                try
                {
                    // 檢查原始資料夾是否存在
                    if (!Directory.Exists(ReplaceFileName))
                    {
                        Directory.CreateDirectory(ReplaceFileName);
                    }
                        // 檢查新的資料夾是否已存在
                        if (!Directory.Exists(newFolderPath))
                        {
                            // 重命名資料夾
                            Directory.Move(ReplaceFileName, newFolderPath);
                            LogMessage($"The folder has been renamed to: {newFolderPath}", MessageLevel.Debug);
                        }
                        else
                        {
                            LogMessage("The new folder name already exists and cannot be renamed.", MessageLevel.Warn);
                            return false;
                        }
                    
                }
                catch (Exception ex)
                {
                    LogMessage($"Folder Exception: {ex.Message}", MessageLevel.Error);
                    return false;
                }

            }else
            {
                LogMessage($"The source folder path or destination file name cannot be empty or null", MessageLevel.Warn);
                return false;
            }
          return true;
        }
        static bool IsImageFile(string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" , ".raw"};
            string fileExtension = Path.GetExtension(filePath).ToLower();

            foreach (string extension in imageExtensions)
            {
                if (fileExtension == extension)
                {
                    return true;
                }
            }
            return false;
        }

        public bool AreFilesIdentical(string filePath1, string filePath2)
        {
            try
            {
                if (IsImageFile(filePath1) && IsImageFile(filePath2))
                {
                    if (!File.Exists(filePath1))
                    {
                        LogMessage($"{filePath1} is not Exists!!,Please Check Soruce File", MessageLevel.Error);
                        return false;
                    }
                    if (!File.Exists(filePath2))
                    {
                        LogMessage($"{filePath2} is not Exists!! Copy Fail", MessageLevel.Error);
                        return false;
                    }
                    using (var hashAlgorithm = SHA256.Create()) //SHA - 256 雜湊值
                    {
                        byte[] hash1 = ComputeFileHash(filePath1, hashAlgorithm);
                        byte[] hash2 = ComputeFileHash(filePath2, hashAlgorithm);

                        return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
                    }
                }else
                {
                    LogMessage($"Source path or Destination path is not an image Format", MessageLevel.Debug);
                }
            }
            catch (Exception File_ex)
            {
                LogMessage($"FilesIdentical Error : {File_ex.Message}", MessageLevel.Error);
                return false;
            }
            return true;
        }

        static byte[] ComputeFileHash(string filePath, HashAlgorithm hashAlgorithm)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return hashAlgorithm.ComputeHash(stream);
            }
        }

        public class FileProcessing : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> FileProcessingKeys = new List<string>();

                FileProcessingKeys.Add("FileCopy");
                FileProcessingKeys.Add("FileDelete");
                FileProcessingKeys.Add("FileMove");
                FileProcessingKeys.Add("FileDestReName");

                return new StandardValuesCollection(FileProcessingKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

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

    }
}
