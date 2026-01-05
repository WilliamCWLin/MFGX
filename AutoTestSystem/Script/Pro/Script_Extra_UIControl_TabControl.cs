
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
    internal class Script_Extra_UIControl_TabControl : Script_Extra_Base
    {
        [Category("Image"), Description("Image Path")]
        public int Index { get; set; } = 0;
        public override bool PreProcess()
        {
            return true;
        }
        public override bool Process(ref string strOutData)
        {

            try
            {
                HandleDevice.SwitchTabControlIndex(Index);
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
