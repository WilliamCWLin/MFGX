using AutoTestSystem.BLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Model
{
    public class TestFileManager
    {
        private List<TestFileInfo> fileList = new List<TestFileInfo>();
        private List<string> backupDirs;

        public TestFileManager(List<string> backupPaths)
        {
            backupDirs = backupPaths;
        }
        public TestFileManager()
        {

        }
        public void SetbackupDirs(List<string> backupPaths)
        {
            backupDirs = backupPaths;
        }
        public void AddFile(TestFileInfo fileInfo)
        {
            //string sourcePath = fileInfo.GetAbsoluteSourcePath();
            //if (File.Exists(sourcePath))
            //{
                fileList.Add(fileInfo);
            //}
        }

        public void BackupAll(string result) // PASS 或 FAIL
        {
            foreach (var fileInfo in fileList)
            {
                foreach (var rootBackupDir in backupDirs)
                {
                    try
                    {
                        string testRootFolder = rootBackupDir;

                        if (fileInfo.IncludeTimeFolder)
                        {
                            string timeFolder = DateTime.Now.ToString("yyyy-MM-dd");
                            testRootFolder = Path.Combine(testRootFolder, timeFolder);
                        }

                        if (fileInfo.IncludeResultFolder)
                        {
                            testRootFolder = Path.Combine(testRootFolder, result.ToUpper());
                        }

                        string newFileName = fileInfo.GetNewFileName();
                        string destFolder = string.IsNullOrEmpty(fileInfo.SubFolder)
                            ? testRootFolder
                            : Path.Combine(testRootFolder, fileInfo.SubFolder);


                        // 檢查網路硬盤是否可用
                        if (IsNetworkDriveAvailable(destFolder))
                        {
                            Directory.CreateDirectory(destFolder);

                            string destPath = Path.Combine(destFolder, newFileName);
                            string sourcePath = fileInfo.GetAbsoluteSourcePath();

                            if (File.Exists(sourcePath))
                            {
                                File.Copy(sourcePath, destPath, true);
                            }
                            else
                            {
                                Bd.Logger.Debug($"來源檔案不存在：{sourcePath}");
                            }
                        }
                        else
                        {
                            Bd.Logger.Debug($"網路硬盤不可用(Network drive is not available)：{fileInfo.GetAbsoluteSourcePath()} backup fail.");
                        }

                    }
                    catch (Exception ex)
                    {
                        Bd.Logger.Debug($"備份錯誤：{fileInfo.SourcePath} - {ex.Message}");
                    }
                }

                // 在所有備份位置都完成備份後，刪除來源檔案（如果需要）
                if (fileInfo.DeleteSourceAfterBackup && File.Exists(fileInfo.SourcePath))
                {
                    try
                    {
                        File.Delete(fileInfo.SourcePath);
                    }
                    catch (Exception ex)
                    {
                        Bd.Logger.Debug($"刪除來源檔案錯誤：{fileInfo.SourcePath} - {ex.Message}");
                    }
                }
            }

            // 清空 fileList
            fileList.Clear();

        }


        private bool IsNetworkDriveAvailable(string path)
        {
            try
            {
                string rootPath = Path.GetPathRoot(path);
                if (string.IsNullOrEmpty(rootPath))
                {
                    return false;
                }

                DriveInfo driveInfo = new DriveInfo(rootPath);
                return driveInfo.IsReady;
            }
            catch
            {
                return false;
            }
        }

    }

    public class TestFileInfo
    {
        public string SourcePath { get; set; }         // 原始檔案完整路徑
        public string SubFolder { get; set; }          // 要備份進的子資料夾（可為空）
        public string FileName { get; set; }           // 檔案名稱（不含路徑）
        public bool IncludeTimeFolder { get; set; } = true;    // 是否包含時間資料夾，預設為 true
        public bool IncludeResultFolder { get; set; } = true;  // 是否包含結果資料夾，預設為 true
        public bool DeleteSourceAfterBackup { get; set; } = false; // 是否刪除來源檔案，預設為 false

        public TestFileInfo()
        {

        }

        public string GetNewFileName()
        {
            string ext = Path.GetExtension(SourcePath);
            return $"{FileName}{ext}";
        }

        public string GetAbsoluteSourcePath(string baseDir = null)
        {
            if (Path.IsPathRooted(SourcePath))
            {
                // 已是絕對路徑
                return SourcePath;
            }

            // 相對路徑 → 用 baseDir（如果沒給，預設為目前工作目錄）
            string basePath = baseDir ?? Directory.GetCurrentDirectory();
            return Path.GetFullPath(Path.Combine(basePath, SourcePath));
        }
    }
}
