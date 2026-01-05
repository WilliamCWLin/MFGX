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

using System.Diagnostics;
using System.ComponentModel;
using System.Drawing.Design;
using System.Threading;
using System.Windows.Forms;
using AutoTestSystem.Model;
using AutoTestSystem.Equipment.Teach;
using static AutoTestSystem.Equipment.Teach.Teach_IQ_Tuning;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using AutoTestSystem.Equipment.ControlDevice;
using DocumentFormat.OpenXml.Spreadsheet;

namespace AutoTestSystem.Script
{

    internal class Script_Extra_MoveFile : Script_Extra_Base
    {
        public enum Move_ACTION
        {
            MoveFileWithOverwrite,
            MoveDirectoryWithOverwrite,
            MergeDirectoryWithOverwrite
        }


        string strOutData = string.Empty;

        [Category("Command"), Description("自訂顯示名稱")]
        public Move_ACTION Mode { get; set; } = Move_ACTION.MoveDirectoryWithOverwrite;

        [Category("Command"), Description("支援用%%方式做變數值取代")]
        public string sourcePath { get; set; } = string.Empty;
        [Category("Command"), Description("支援用%%方式做變數值取代")]
        public string destPath { get; set; } = string.Empty;
        public override void Dispose()
        {

        }

        public override bool PreProcess()
        {
            strOutData = string.Empty;

            return true;
        }

        public override bool Process(ref string output)
        {
            string source_path = ReplaceProp(sourcePath);
            string dest_path = ReplaceProp(destPath);
            bool ret = false;
            try
            {
                switch (Mode)
                {
                    case Move_ACTION.MoveFileWithOverwrite:
                        ret = MoveFileWithOverwrite(source_path, dest_path);

                        break;
                    case Move_ACTION.MergeDirectoryWithOverwrite:
                        ret = MergeDirectoryWithOverwrite(source_path, dest_path);

                        break;
                    case Move_ACTION.MoveDirectoryWithOverwrite:
                        ret = MoveDirectoryWithOverwrite(source_path, dest_path);

                        break;
                }
                output = SaveResultAsJson(ret.ToString(), source_path, dest_path);
                return ret;
            }
            catch (Exception ex)
            {
                output = SaveResultAsJson(ex.Message, source_path, dest_path);
                LogMessage(ex.Message);
                return false;
            }
        }

  
        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
            {
                LogMessage("Spec check passed. or Spec is empty");
                return true;
            }
            else
            {
                LogMessage($"{result}", MessageLevel.Error);
                return false;
            }

        }
        // Save the result as JSON
        private string SaveResultAsJson(string result, string sourcePath, string destPath)
        {
            var resultObject = new
            {
                Status = result,
                SourcePath = sourcePath,
                DestinationPath = destPath,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return JsonConvert.SerializeObject(resultObject, Formatting.Indented);

        }
        public bool MoveFileWithOverwrite(string sourcePath, string destPath)
        {
            bool result;
            try
            {
                // Ensure the target directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                // Copy the file and overwrite if it exists
                File.Copy(sourcePath, destPath, true);
                // Delete the original file
                File.Delete(sourcePath);

                LogMessage($"File successfully moved to {destPath} and overwritten if existed.");
                result = true;
            }
            catch (Exception ex)
            {
                LogMessage($"File move error: {ex.Message}");
                result = false;
            }

            return result;
        }

        // Move directory and overwrite (delete target directory before moving)
        public bool MoveDirectoryWithOverwrite(string sourceDir, string destDir)
        {
            bool result;
            try
            {
                // If the target directory exists, delete it
                if (Directory.Exists(destDir))
                {
                    Directory.Delete(destDir, true);  // Recursive delete
                }

                // Move the directory
                Directory.Move(sourceDir, destDir);

                LogMessage($"Directory successfully moved to {destDir} and overwritten if existed.");
                result = true;
            }
            catch (Exception ex)
            {
                LogMessage($"Directory move error: {ex.Message}");
                result = false;
            }

            return result;
        }

        // Merge directory contents, overwrite existing files but keep other files
        public bool MergeDirectoryWithOverwrite(string sourceDir, string destDir)
        {
            bool result;
            try
            {
                CopyDirectory(sourceDir, destDir, true);
                Directory.Delete(sourceDir, true);  // Delete source directory after move

                LogMessage($"Directory successfully merged to {destDir} and existing files were overwritten.");
                result = true;
            }
            catch (Exception ex)
            {
                LogMessage($"Directory merge error: {ex.Message}");
                result = false;
            }

            return result;
        }

        // Helper method to recursively copy directory contents
        private void CopyDirectory(string sourceDir, string destDir, bool overwrite)
        {
            Directory.CreateDirectory(destDir);  // Ensure the target directory exists

            // Copy files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));  // Ensure the directory for the file exists
                File.Copy(file, destFile, overwrite);
            }

            // Recursively copy subdirectories
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir, overwrite);
            }
        }

    }


}
