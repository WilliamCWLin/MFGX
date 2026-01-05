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
using System.Windows.Forms.Design;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.Script.Script_DUT_Related;
using static System.ComponentModel.TypeConverter;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_SyncClear : Script_Extra_Base
    {
        string l_strOutData = string.Empty;

        [Category("Clear"), Description("Clear Consequence Only Flag and Lock object")]
        public bool Consequence { get; set; } = false;

        [Category("Clear"), Description("Clear CountDown")]
        public bool CountDown { get; set; } = false;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool PreProcess()
        {


            return true;
        }

        public override bool Process(ref string strOutData)
        {
            try
            {
                if (Consequence)
                    Container_Sequences.ClearSyncProp();
                if (CountDown)
                    Script_Extra_CountDownEvent.ClearAll();
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                return true;
            }

            return true;
        }

        public override bool PostProcess()
        {
            return true;
        }

    }
}
