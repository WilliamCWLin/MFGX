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
    internal class Script_Image_PowerWrite : Script_Image_Base
    {
        string strOutData = string.Empty;
        //[Category("Parameter"), Description("Profile Path"), Editor(typeof(Manufacture.FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        //public string IniPath { get; set; }

        [Category("Param_Setting"), Description("Power Write AVDD")]
        public int AVDD { get; set; } = 0;

        [Category("Param_Setting"), Description("Power Write DOVDD")]
        public int DOVDD { get; set; } = 0;

        [Category("Param_Setting"), Description("Power Write DVDD")]
        public int DVDD { get; set; } = 0;

        [Category("Param_Setting"), Description("Power Write AFVCC")]
        public int AFVCC { get; set; } = 0;

        [Category("Param_Setting"), Description("Power Write VPP")]
        public int VPP { get; set; } = 0;


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



            Image.PowerWrite(AVDD, DOVDD, DVDD, AFVCC, VPP);




            //if (Image.Init(""))
            //{
            //    LogMessage($"Load Ini Success", MessageLevel.Debug);
            //    try
            //    {
            //        if(Image.Start())
            //        {
            //            LogMessage($"Open Decive Success", MessageLevel.Debug);
            //            return true;
            //        }
            //        else
            //        {
            //            LogMessage($"Open Decive Fail", MessageLevel.Error);
            //            return false;
            //        }

            //    }
            //    catch(Exception ex)
            //    {
            //        LogMessage($"Open Decive Fail :{ ex.Message}", MessageLevel.Error);
            //        return false;
            //    }
            //}
            //else
            //{
            //    LogMessage($"Load Ini Fail", MessageLevel.Error);
            //    return false;
            //}
            return true;
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
