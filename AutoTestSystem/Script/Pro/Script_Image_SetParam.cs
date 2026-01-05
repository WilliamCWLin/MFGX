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
    internal class Script_Image_SetParam : Script_Image_Base
    {
        string strOutData = string.Empty;
        public Dictionary<string, int> _paramInfo = new Dictionary<string, int>();
        [Category("Choose function"), Description("選擇要使用的功能"), TypeConverter(typeof(Function_List))]
        public string Function_Name { get; set; }
        [Category("Data"), Description("如果選擇I2C mode只需要在更改曝光時間時輸入WhiteBoard_ParaList\n如果選擇ICR mode則需要使用小寫英文字母且為json格式輸入\nPin:GPIO 0-4\nHigh_Low:true=High or false=Low"), Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        public string P3_Body { get; set; } = "{\n\"Pin\": \"\",\"\n\"High_Low\": \"\"\n}";


        public override void Dispose()
        {
            throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            strOutData = string.Empty;
            return true;
        }

        public override bool Process(Image_Base Image, ref string strOutData)
        {
            if(Image.SetParam(Function_Name, P3_Body))
            {
                LogMessage($"SetParam Success", MessageLevel.Debug);
                return true;
            }
            else
            {
                LogMessage($"SetParam Fail", MessageLevel.Error);
                return false;
            }          
        }
        public override bool PostProcess()
        {
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

        public class Function_List : TypeConverter
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                dynamic currentObject = context.Instance;
                List<string> Sample = new List<string>();
                try
                {
                    if (string.IsNullOrEmpty(currentObject.DeviceSel))
                    {
                        return new StandardValuesCollection(new string[] { });
                    }
                    if (GlobalNew.Devices.ContainsKey(currentObject.DeviceSel) == false)
                    {
                        return new StandardValuesCollection(new string[] { });
                    }

                    Image_Base Dothink = (Image_Base)GlobalNew.Devices[currentObject.DeviceSel];
                    Sample = Dothink.Function;

                    return new StandardValuesCollection(Sample);
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
                {
                    return new StandardValuesCollection(new string[] { });
                }
                catch (Exception)
                {
                    return new StandardValuesCollection(new string[] { });
                }
            }
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
    }
}
