using AutoTestSystem.Base;
using MvCamCtrl.NET;
using MvCamCtrl.NET.CameraParams;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;


namespace AutoTestSystem.Equipment.CCD
{
    class CCDSimu : CCDBase
    {
        private string _strParamInfoPath;

        [Category("Params"), Description("Init Retrun")]
        public string CCDDeviceName { get; set; }

        public override bool Capture(string strSavePath)
        {
            Logger.Debug("Capture Succeed!");

            return true;
        }

        public override bool SaveImage(string strSavePath)
        {
            Logger.Debug("Save Succeed!");

            return true;
        }


        public override bool SaveImage(int Format_Mode, string strSavePath)
        {
            Logger.Debug($"Save Succeed! Path:{strSavePath}");

            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            LogMessage($"Init Succeed! strParamInfo:{strParamInfo}");
            return true;
        }

        public override bool Start()
        {
            LogMessage("Start Succeed!");

            return true;
        }

        public override bool UnInit()
        {
            LogMessage("UnInit Succeed!");
            return true;
        }

        public override bool Set_Exposure(int value)
        {
            LogMessage($"Set_Exposure : {value}");
            return true;
        }

        public override bool ReadParamFile(string strSavePath)
        {
            LogMessage("ReadParamFile Succeed!");
            return true;
        }


    }
}
