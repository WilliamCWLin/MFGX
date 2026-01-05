
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
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using AutoTestSystem.DAL;
using System.Drawing;
using static AutoTestSystem.Model.IQ_SingleEntry;
using static AutoTestSystem.Base.CCDBase;
using System.Xml.Linq;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_UIControl_ShowImage : Script_Extra_Base
    {
        [Category("Folder"), Description("選擇存取影像文件夾"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string FolderPath { get; set; } = "";

        [Category("File"), Description("選擇存取影像文件夾")]
        public string File1 { get; set; } = string.Empty;
        [Category("File"), Description("選擇存取影像文件夾")]
        public string File2 { get; set; } = string.Empty;
        [Category("File"), Description("選擇存取影像文件夾")]
        public string File3 { get; set; } = string.Empty;
        [Category("File"), Description("選擇存取影像文件夾")]
        public string File4 { get; set; } = string.Empty;
        [Category("File"), Description("選擇存取影像文件夾")]
        public string File5 { get; set; } = string.Empty;
        [Category("File"), Description("選擇存取影像文件夾")]
        public string File6 { get; set; } = string.Empty;

        [Category("UI"), Description("選擇存取影像文件夾")]
        public string TabpageName { get; set; } = "ShowPics";
        public override bool PreProcess()
        {
            // 如果 FolderPath 有填，直接使用資料夾方式
            if (!string.IsNullOrWhiteSpace(FolderPath))
                return true;

            // 否則檢查 File1~File6 是否有任一個有填
            var files = new[] { File1, File2, File3, File4, File5, File6 };
            bool hasAnyFile = files.Any(f => !string.IsNullOrWhiteSpace(f));

            if (!hasAnyFile)
            {
                LogMessage("未設定資料夾路徑，也未指定任何圖片檔案。");
                return false;
            }

            return true;

        }
        public override bool Process(ref string strOutData)
        {
            try
            {
                if (HandleDevice.DutDashboard == null)
                {
                    LogMessage("This feature is not supported.");
                    return true;
                }

                string ReplaceSavePath = ReplaceProp(FolderPath);
                List<string> imagePaths = new List<string>();

                // 加入資料夾中的圖片
                if (!string.IsNullOrWhiteSpace(ReplaceSavePath))
                {
                    string imageFolder = ReplaceProp(ReplaceSavePath);
                    if (!Directory.Exists(imageFolder))
                    {
                        LogMessage($"資料夾不存在：{imageFolder}");
                        return false;
                    }

                    var folderImages = Directory.GetFiles(imageFolder, "*.*")
                        .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                                    f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    imagePaths.AddRange(folderImages);
                }

                // 加入個別指定的檔案
                var fileList = new[] {
            ReplaceProp(File1),
            ReplaceProp(File2),
            ReplaceProp(File3),
            ReplaceProp(File4),
            ReplaceProp(File5),
            ReplaceProp(File6)
        };

                var validFiles = fileList
                    .Where(f => !string.IsNullOrWhiteSpace(f) && File.Exists(f))
                    .ToList();

                imagePaths.AddRange(validFiles);

                if (imagePaths.Count == 0)
                {
                    LogMessage("未找到任何圖片。");
                    return false;
                }

                // 呼叫 Dashboard 的共用函式顯示圖片
                HandleDevice.DutDashboard.ShowImagesInTab(TabpageName, imagePaths);
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

    }

}
