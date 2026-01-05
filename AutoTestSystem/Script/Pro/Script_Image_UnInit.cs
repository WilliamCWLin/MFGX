using AutoTestSystem.Base;
using AutoTestSystem.Equipment.Image;
using AutoTestSystem.Model;
using Manufacture;
using MvCamCtrl.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.Equipment.Image.UC930;

namespace AutoTestSystem.Script
{
    internal class Script_Image_UnInit : Script_Image_Base
    {
        string strOutData = string.Empty;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            strOutData = string.Empty;
            return true;
        }

        public override bool Process(Image_Base Image,ref string strOutData)
        {
            try
            {
                if (Image.UnInit())
                {
                    LogMessage($"CCD_Close Suceesed ", MessageLevel.Debug);
                    return true;
                }
                else
                {
                    LogMessage($"CCD_Close Fail ", MessageLevel.Error);
                    return false;
                }
            }
            catch(Exception ex)
            {
                LogMessage($"CCD_Close Error:{ex.Message}", MessageLevel.Error);
                return false;
            }
        }
        public override bool PostProcess()
        {
            //bCloseCamera();
            if (Spec != string.Empty && Spec != null)
            {
                
                string ret = string.Empty;             
                ret = CheckRule(strOutData, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Debug);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;

        }

        
    }
}
