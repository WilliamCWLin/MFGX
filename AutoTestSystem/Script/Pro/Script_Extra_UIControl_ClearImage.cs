
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
    internal class Script_Extra_UIControl_ClearImage : Script_Extra_Base
    {

        public override bool PreProcess()
        {
            return true;
        }
        public override bool Process(ref string strOutData)
        {
            try
            {
                if(HandleDevice.DutDashboard == null)
                {
                    LogMessage($"This feature is not supported.");
                    return true;
                }

                int width = HandleDevice.DutDashboard.ImagePicturebox.Width;
                int height = HandleDevice.DutDashboard.ImagePicturebox.Height;


                // 使用 using 確保資源釋放
                using (Bitmap blackImage = new Bitmap(width, height))
                {
                    using (Graphics g = Graphics.FromImage(blackImage))
                    {
                        g.Clear(Color.Black); // 填充為黑色
                    }

                    // 在 UI 執行緒上更新 PictureBox 的圖片
                    HandleDevice.DutDashboard.ImagePicturebox.Invoke((Action)(() =>
                    {
                        HandleDevice.DutDashboard.ImagePicturebox.Image?.Dispose(); // 清理先前的圖片
                        HandleDevice.DutDashboard.ImagePicturebox.Image = (Bitmap)blackImage.Clone(); // 克隆圖片以防止被釋放
                    }));
                }

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
