
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
    internal class Script_Extra_Test : Script_Extra_Base
    {
        [Category("Common Parameters"), Description("Directly use Keyname, without adding %%")]
        public string Key { get; set; } = "";

        string jsonresult = "";


        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            jsonresult = string.Empty;
            return true;
        }
        public override bool Process(ref string strOutData)
        {

            return GlobalNew._startBtnPressed;

            

            //return true;
        }
        public override bool PostProcess()
        {

                return true;


        }


    }
}
