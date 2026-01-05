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
using ZXing;
using static OpenCvSharp.ML.DTrees;

namespace AutoTestSystem.Script
{
    public class Container_Condition_Jumper : Script_Container_Base, IDisposable
    {
        [Browsable(false)]
        public new string FailJump { set; get; }

        [Category("Condition"), Description("Fail Goto Where.Do not log error codes"), TypeConverter(typeof(GotoList))]
        public string FAIL_GOTO { set; get; }
        [Category("Condition"), Description("Pass Goto Where"), TypeConverter(typeof(GotoList))]
        public string PASS_GOTO { set; get; }



        public Container_Condition_Jumper()
        {

            //Description = "Seq";
        }

        public override void Dispose()
        {

        }

        public override bool PreProcess(string ActionItem, string Paraminput)
        {
            if (string.IsNullOrEmpty(FAIL_GOTO) || string.IsNullOrEmpty(PASS_GOTO))
            {
                MessageBox.Show("FAIL_GOTO or PASS_GOTO is null");
            }
            return true;
        }

        public override int Process(TreeNode tn, object Component)
        {
            object[] context = Component as object[];

            DUT_BASE Dut = null;
            if (context.Length > 2)
            {
                Dut = (DUT_BASE)context[2];
            }


            int ret = 0;
            string tmp_failitem = Dut.DataCollection.GetMoreProp("Failitem");

            ret = Run((TreeNode)tn, Component);

            Dut.DataCollection.SetMoreProp("Failitem", tmp_failitem);


            if (ret == 1)
                throw new DumpException(PASS_GOTO);
            else
            {
                throw new DumpException(FAIL_GOTO);
            }

        }
        public override int Process(TreeNode tn)
        {
            int ret = 0;

            ret = Run((TreeNode)tn);

            return ret;
        }

    }
}
