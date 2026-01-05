using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
using Manufacture;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OpenCvSharp.ML.DTrees;

namespace AutoTestSystem.Script
{
    public class Container_JIG_INIT : Script_Container_Base, IDisposable
    {
        [Category("Params"), Description("RetryTimes")]
        public int RetryTimes { get; set; }

        [Category("Params"), Description("Cycles")]
        public int Cycles { get; set; }



        public Container_JIG_INIT()
        {
            RetryTimes = 0;
            Cycles = 1;
            //Description = "Seq";
        }

        public override void Dispose()
        {

        }

        public override bool PreProcess(string ActionItem, string Paraminput)
        {
            return true;
        }

        public override int Process(TreeNode tn, object Component)
        {
            int ret = 0;

            ret = Run((TreeNode)tn, Component);

            return ret;
        }
        public override int Process(TreeNode tn)
        {
            int ret = 0;

            ret = Run((TreeNode)tn);

            return ret;
        }

    }
}
