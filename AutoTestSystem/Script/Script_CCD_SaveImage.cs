using AutoTestSystem.Base;
using AutoTestSystem.Equipment.CCD;
using AutoTestSystem.Equipment.ControlDevice;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_CCD_SaveImage : Script_CCD_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;

        CCD ccd_param = null;

        [Category("Common Parameters"), Description("自訂顯示名稱"), Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        public string Params { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool Process()
        {
            ccd_param = JsonConvert.DeserializeObject<CCD>(Params);
            return Process(CCDDevice);
        }
        public override bool PreProcess(string ActionItem, string Paraminput)
        {

            strActItem = ActionItem;
            strParam = Paraminput;

            ccd_param = JsonConvert.DeserializeObject<CCD>(strParam);

            return true;
        }
        public override bool Process(CCDBase CCD)
        {


            //if (!CCD.Start())
            //{
            //    return false;
            //}

       
            if (!CCD.SaveImage(ccd_param.SaveImageMode, ccd_param.SavePath))
            {
                LogMessage("CCD.SaveImage Fail",MessageLevel.Debug);
                return false;
            }                

            if (ccd_param.BackupMode == 1)
            {
                string Backup_Path = null;
                DirectoryInfo di = null;
                if (ccd_param.SaveImageMode == 0)
                {
                    if (!ccd_param.SavePath.EndsWith(".bmp"))
                    {
                        ccd_param.SavePath = ccd_param.SavePath + ".bmp";
                    }
                    Backup_Path = Path.GetDirectoryName(ccd_param.SavePath) + "/Backup";
                    if (!Directory.Exists(Backup_Path))
                        di = Directory.CreateDirectory(Backup_Path);

                    Backup_Path = Backup_Path + "/" + DateTime.Now.ToString("yyyyMMdd_HHmmss_") + PopMoreData("ProductSN") + "_" + Path.GetFileName(ccd_param.SavePath);
                    File.Copy(ccd_param.SavePath, Backup_Path);
                    LogMessage($"File.Copy to {Backup_Path}", MessageLevel.Debug);
                }
                else if (ccd_param.SaveImageMode == 1)
                {
                    if (!ccd_param.SavePath.EndsWith(".jpg"))
                    {
                        ccd_param.SavePath = ccd_param.SavePath + ".jpg";
                    }
                    Backup_Path = Path.GetDirectoryName(ccd_param.SavePath) +"/Backup";
                    if (!Directory.Exists(Backup_Path))
                        di = Directory.CreateDirectory(Backup_Path);

                    Backup_Path = Backup_Path + "/" + DateTime.Now.ToString("yyyyMMdd_HHmmss_")+PopMoreData("ProductSN")+"_"+ Path.GetFileName(ccd_param.SavePath);
                    File.Copy(ccd_param.SavePath, Backup_Path);
                    LogMessage($"File.Copy to {Backup_Path}", MessageLevel.Debug);
                }
            }

            //if (ccd_param.SaveImageMode == "bmp")
            //{
            //    CCD.Capture(ccd_param.SavePath + "bmp");
            //}
            //else if (ccd_param.SaveImageMode == "jpg")
            //{
            //    CCD.Capture(ccd_param.SavePath + "jpg");
            //}
            strOutData = "OK";
            LogMessage($"Script_CCD_SaveImage:  {strOutData}");
            return true;
        }
        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            //strDataout= ref strOutData;
            //string result = ProcessData(strOutData, strCheckSpec);

            strDataout =  strOutData;
            return true;

        }
       
        public class CCD
        {
            public int SaveImageMode { get; set; }
           
            public string SavePath { get; set; }

            public int BackupMode { get; set; }



        }

    }
}
