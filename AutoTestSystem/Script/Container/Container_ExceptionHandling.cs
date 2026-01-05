using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
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
    public  class Container_ExceptionHandling : Script_Container_Base, IDisposable
    {
        [Category("Params"), Description("RetryTimes")]
        public int RetryTimes { get; set; }

        [Browsable(false)]
        [Category("Params"), Description("Cycles")]
        public int Cycles { get; set; }

        public Container_ExceptionHandling()
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

        public override int Process(TreeNode treeNode, object Component)
        {

            object[] context = Component as object[];
            ProTreeView.ProTreeView ProTV = (ProTreeView.ProTreeView)context[0];
            bool failcontinue = (bool)context[1];

            DUT_BASE Dut = null;
            if (context.Length > 2)
            {
                Dut = (DUT_BASE)context[2];
            }

            string fullJson = $"************[{Description}_Start]({GlobalNew.stopwatch.Elapsed.TotalSeconds.ToString("0.00")}s)************";
            Dut.TestInfo.AddTestStep("Test Items", fullJson);
            Dut.TestInfo.AddTestStep("ExHanling", $"************[{Description} Exception Handling Start]************");
            int ret = 0;

            ret = Run((TreeNode)treeNode, Component);
          
            fullJson = $"************[{Description}_End]({GlobalNew.stopwatch.Elapsed.TotalSeconds.ToString("0.00")}s)************\n";
            Dut.TestInfo.AddTestStep("Test Items", fullJson);
            Dut.TestInfo.AddTestStep("ExHanling", $"************[{Description} Exception Handling End]************");
            return ret;

        }
        //public override int Process(TreeNode treeNode, object Component)
        //{

        //    object[] context = Component as object[];
        //    ProTreeView.ProTreeView ProTV = (ProTreeView.ProTreeView)context[0];
        //    bool failcontinue = (bool)context[1];

        //    DUT_BASE Dut = null;
        //    if (context.Length > 2)
        //    {
        //        Dut = (DUT_BASE)context[2];
        //    }

        //    string fullJson = $"************[{Description}_Start]({GlobalNew.stopwatch.Elapsed.TotalSeconds.ToString("0.00")}s)************";
        //    Dut.TestInfo.AddTestStep("Test Items", fullJson);

        //    bool Result = true;
        //    Dictionary<string, string> k = new Dictionary<string, string>();
        //    int count = 0;

        //    foreach (TreeNode n in treeNode.Nodes)
        //    {
        //        if (((Manufacture.CoreBase)n.Tag).ID != null)
        //            k.Add(((Manufacture.CoreBase)n.Tag).Description + "(" + ((Manufacture.CoreBase)n.Tag).ID + ")", count.ToString());
        //        count++;
        //    }

        //    for (int i = 0; i < treeNode.Nodes.Count; i++)
        //    {
        //        if (treeNode.Nodes[i].Checked != true)
        //        {
        //            ChangeColorRecursive(treeNode.Nodes[i], Color.White, Color.LightGray);
        //            continue;
        //        }

        //        bool status = false;

        //        Manufacture.Global_Memory.resetEvent.WaitOne();

        //        if (treeNode.Nodes[i].Tag is ScriptBase)
        //        {
        //            treeNode.Nodes[i].BackColor = Color.LightGray;
        //            ScriptBase nowobject = ((ScriptBase)treeNode.Nodes[i].Tag);

        //            try
        //            {
        //                if (nowobject.Action(Dut) == true)
        //                {
        //                    treeNode.Nodes[i].ForeColor = Color.Green;
        //                    treeNode.Nodes[i].BackColor = Color.White;
        //                    status = true;
        //                }
        //            }

        //            catch (Manufacture.DumpException)
        //            {
        //                treeNode.Nodes[i].ForeColor = Color.Green;
        //                treeNode.Nodes[i].BackColor = Color.White;

        //                int temp = i + 1;
        //                string JumpTo = (((Manufacture.CoreBase)treeNode.Nodes[i].Tag).ExID);

        //                LogDetail("======" + treeNode.Nodes[i].Tag.ToString() + "->" + "Jump to" + JumpTo + "======");
        //                if (JumpTo != null)
        //                    i = Int32.Parse(k[JumpTo]) - 1;

        //                for (int j = temp; j <= i; j++)
        //                {
        //                    treeNode.Nodes[j].ForeColor = Color.Blue;
        //                    treeNode.Nodes[j].BackColor = Color.White;
        //                }

        //                status = true;
        //            }
        //            catch (Exception ex)
        //            {

        //                LogFail("======" + treeNode.Nodes[i].Tag.ToString() + "->" + ex.ToString() + "======");
        //            }

        //        }
        //        else if (treeNode.Nodes[i].Tag is ScriptContainer)
        //        {
        //            if (1 == ((ScriptContainer)treeNode.Nodes[i].Tag).Process(treeNode.Nodes[i], Component))
        //            {
        //                treeNode.Nodes[i].BackColor = Color.White;
        //                status = true;
        //            }
        //        }

        //        treeNode.Nodes[i].BackColor = Color.White;

        //        if (status == false)
        //        {
        //            treeNode.Nodes[i].ForeColor = Color.Red;

        //            string s = (((Manufacture.CoreBase)treeNode.Nodes[i].Tag).ExID);
        //            int temp = i + 1;
        //            if (s != null && s != string.Empty)
        //                i = Int32.Parse(k[s]) - 1;

        //            for (int j = temp; j <= i; j++)
        //            {
        //                treeNode.Nodes[j].ForeColor = Color.Blue;
        //                treeNode.Nodes[j].BackColor = Color.White;
        //            }
        //        }

        //        Result &= status;

        //        if (Result == false)
        //        {
        //            if (treeNode.Nodes[i].Tag is Container_Sequences)
        //            {
        //                if (((Container_Sequences)treeNode.Nodes[i].Tag).FailJump == "Continue")
        //                {

        //                }
        //                else if (((Container_Sequences)treeNode.Nodes[i].Tag).FailJump == "Break")
        //                {
        //                    if (status == false)
        //                        break;
        //                }
        //                else if (((Container_Sequences)treeNode.Nodes[i].Tag).FailJump != string.Empty)
        //                {
        //                    treeNode.Nodes[i].ForeColor = Color.Red;

        //                    string s = (((Container_Sequences)treeNode.Nodes[i].Tag).FailJump);
        //                    int temp = i + 1;
        //                    if (s != null && s != string.Empty)
        //                        i = Int32.Parse(k[s]) - 1;

        //                    treeNode.Nodes[temp].ForeColor = Color.Blue;

        //                    for (int j = temp; j <= i; j++)
        //                    {
        //                        ChangeColorRecursive(treeNode.Nodes[j], Color.White, Color.Blue);
        //                    }
        //                }

        //            }
        //            else if (treeNode.Nodes[i].Tag is ScriptBase)
        //            {
        //                if (status == false)
        //                    break;
        //            }
        //        }
        //    }
        //    fullJson = $"************[{Description}_End]({GlobalNew.stopwatch.Elapsed.TotalSeconds.ToString("0.00")}s)************\n";
        //    Dut.TestInfo.AddTestStep("Test Items", fullJson);
        //    if (Result)
        //    {
        //        treeNode.ForeColor = Color.Green;
        //        return 1;
        //    }
        //    else
        //    {
        //        treeNode.ForeColor = Color.Red;
        //        return 0;
        //    }

        //}
        public override bool Process()
        {
            return true;
        }
        public override bool PostProcess(string strCheckSpec,ref string strDataout)
        {
            return true;
        }
    }
}
