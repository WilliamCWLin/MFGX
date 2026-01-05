using AutoTestSystem.Model;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_BackupQueue : Script_Extra_Base
    {
        [Category("Source"), Description("Source file path")]
        public string SourcePath { get; set; } = "";

        [Category("File Backup"), Description("Test item name")]
        public string FileName { get; set; } = "%ProductSN%_TestItemName-RetryNumber_%NowTimeHH_mm_ss%";

        [Category("File Backup"), Description("Subfolder for backup")]
        public string SubFolder { get; set; } = "Meta";

        [Category("Source"), Description("Delete source file after backup")]
        public bool SourceDelete { get; set; } = false;


        public override bool PreProcess()
        {
            return true;
        }

        public override bool Process(ref string strOutData)
        {
            try
            {
                // 創建並添加 TestFileInfo
                AddTestFileInfo(SourcePath);
            }
            catch (Exception ex)
            {
                LogMessage($"{ex.Message}");
            }

            return true;
        }

        public override bool PostProcess()
        {
            return true;
        }

        // 添加方法來創建並添加 TestFileInfo
        private void AddTestFileInfo(string sourcePath)
        {
            TestFileInfo fileInfo = new TestFileInfo
            {
                SourcePath = ReplaceProp(sourcePath),
                FileName = ReplaceProp(FileName),
                SubFolder = ReplaceProp(SubFolder),
                DeleteSourceAfterBackup = SourceDelete
            };
            
            //LogMessage($"SourcePath:{ReplaceProp(SourcePath)}\r\nFileName:{ReplaceProp(FileName)}\r\nSubFolder:{SubFolder}");

            HandleDevice.FileManager?.AddFile(fileInfo);         
        }
    }
}
