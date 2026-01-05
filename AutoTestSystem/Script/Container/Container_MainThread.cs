using AutoTestSystem.Base;
using AutoTestSystem.BLL;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
using Manufacture;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using static AutoTestSystem.MainForm;
using static OpenCvSharp.ML.DTrees;

namespace AutoTestSystem.Script
{
    public class Container_MainThread : ScriptContainer
    {
        [Browsable(false)]
        [Category("Params"), Description("RetryTimes")]
        public int RetryTimes { get; set; }

        // [Browsable(false)]
        [Category("Params"), Description("Cycles")]
        public int Cycles { get; set; }

        [Category("Params"), Description("Cycles")]
        public bool FailContinue { get; set; }
        [Category("Params"), Description("Status")]
        private int iTestStatus { get; set; }
        [Browsable(false)]
        public new string FailJump
        {
            get { return base.FailJump; }
            set { base.FailJump = value; }
        }
        [JsonIgnore]
        [Browsable(false)]
        public int isRunning;


        [JsonIgnore]
        [Browsable(false)]
        public DUT_Simu _DUT_Simu = null;


        [JsonIgnore]
        [Browsable(false)]
        public ManualResetEvent resetEvent = new ManualResetEvent(true);

        DataGridView DataGridView;
        public Container_MainThread()
        {
            //Description = "MainThread";
            RetryTimes = 0;
            Cycles = 1;
            isRunning = 0;
            FailContinue = false;

            _DUT_Simu = new DUT_Simu();
        }


        public void T_Pause()
        {
            isRunning = 2;
            Manufacture.Global_Memory.resetEvent.Reset();
        }

        public void T_PausePro()
        {
            isRunning = 2;
            resetEvent.Reset();
        }
        public void T_Continue()
        {
            isRunning = 1;
            Manufacture.Global_Memory.resetEvent.Set();
        }
        public void T_ContinuePro()
        {
            isRunning = 1;
            resetEvent.Set();
        }
        //public Task Act(TreeNode tn, object Component)
        //{

        //    //LogDetail("======" + Description + " Process Start." + "======");
        //    LogMessage("======" + Description + " Process Start." + "======");
        //    ChangeColorFromGrandparent(tn, Color.White, Color.LightGray);
        //    var task1 = Task.Factory.StartNew((node) =>
        //    {
        //        bool finalres = true;
        //        object[] context = Component as object[];
        //        for (int i = 0; i < Cycles; i++)
        //        {
        //            isRunning = 1;


        //            try
        //            {
        //                //if (tn.Tag is Container_JIG_INIT)
        //                //{

        //                //    if (context.Length >= 6)
        //                //    {
        //                //        TreeNode ResetNode = (TreeNode)context[5];
        //                //        if (ResetNode != null)//如果有初始化節點才做
        //                //        {
        //                //            try
        //                //            {
        //                //                _DUT_Simu.Clear();
        //                //                _DUT_Simu.isSimu = true;
        //                //                object[] Resetcontext = new object[] { context[0], context[1], _DUT_Simu, context[3], context[4], context[5] };
        //                //                Container_JIG_INIT obj = (Container_JIG_INIT)ResetNode.Tag;
        //                //                int ResetResult = Run(ResetNode, Resetcontext);

        //                //                if (ResetResult != 1)
        //                //                    MessageBox.Show("Fixture initialization reset action failed.治具復歸初始動作位置失敗");

        //                //                if (ResetResult == 1)
        //                //                    context[3] = true;

        //                //            }
        //                //            catch (Exception ex)
        //                //            {
        //                //                MessageBox.Show($"Fixture initialization Exception.{ex.Message }");
        //                //                context[3] = false;
        //                //            }
        //                //        }
        //                //    }
        //                //}
        //                //else
        //                //{
        //                int ret = Run((TreeNode)node, Component);

        //                if (ret != 1)                           
        //                    finalres = false;

        //                if (context.Length >= 6)
        //                {
        //                    TreeNode ResetNode = (TreeNode)context[5];
        //                    if (ResetNode != null)//如果有初始化節點才做
        //                    {
        //                        try
        //                        {
        //                            _DUT_Simu.isSimu = true;
        //                            object[] Resetcontext = new object[] { context[0], context[1], _DUT_Simu, context[3], context[4], context[5] };
        //                            Container_JIG_INIT obj = (Container_JIG_INIT)ResetNode.Tag;
        //                            int ResetResult = Run(ResetNode, Resetcontext);

        //                            if (ResetResult != 1)
        //                                MessageBox.Show("Fixture initialization reset action failed.治具初始化動作失敗");
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            MessageBox.Show($"Fixture initialization Exception.{ex.Message }");
        //                        }
        //                    }
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                // 這裡處理其他所有的異常
        //                LogMessage($"Error in Run method: {ex.Message}", MessageLevel.Error);
        //            }

        //            isRunning = 0;
        //        }

        //        if(finalres)
        //            context[3] = true;

        //    }, tn);
        //    //LogDetail(Description + "非同步執行中.");
        //    //MT_Controls.MTBase.Global_Memory.TaskList.Add(task1);
        //    return task1;
        //}
        public Task ActPro(TreeNode tn, object Component)
        {
            ChangeColorFromGrandparent(tn, Color.White, Color.LightGray);
            var task1 = Task.Factory.StartNew((node) =>
            {
                bool finalres = false;
                object[] context = Component as object[];

                DUT_BASE tmpDut = (DUT_BASE)context[2];

                SetMLoggerThread(tmpDut.LOGGER);
                Bd.SetLoggerForCurrentThread(tmpDut.LOGGER);

                for (int i = 0; i < Cycles; i++)
                {
                    isRunning = 1;
                    //Container_Sequences.ClearSyncProp();
                    Script_Extra_GlobalPropManager.ClearSyncProp();
                    //Script_Extra_CountDownEvent.ClearAll();
                    tmpDut.DutDashboard.MemoryDataClear(tmpDut);
                    if (GlobalNew.RunMode == 0)
                        tmpDut.DutDashboard.ResetDutInfo(tmpDut);
                    if (GlobalNew.RunMode == 1 || GlobalNew.RunMode == 2)
                        tmpDut.DutDashboard.SaveRichTextPro(tmpDut, tmpDut.DutDashboard.DutLogRichTextBox, true);
                    if (i>0)
                        tmpDut.DutDashboard.SetInfoTextboxMessage($"Cycle({i})");
                    tmpDut.SetConfig_Param();

                    tmpDut?.ClearBuffer();

                    try
                    {
                        LogMessage($"Start testing");
                        int ret = Run((TreeNode)node, Component);

                        if (ret == 1)
                        {
                            finalres = true;
                            tmpDut.DutDashboard.SetTestStatus(tmpDut, TestStatus.PASS);
                            context[3] = true;
                        }
                        else
                        {
                            tmpDut.DutDashboard.SetTestStatus(tmpDut, TestStatus.FAIL);
                            //tmpDut.testUnit?.MarkAsFailed();
                            context[3] = false;
                        }
                        tmpDut.PopAllLog();
                        LogMessage($"Test End - Result: {(ret == 1 ? "PASS" : "FAIL")}");
                        
                    }
                    catch (Exception ex)
                    {
                        // 這裡處理其他所有的異常
                        LogMessage($"Error in Run method: {ex.Message}", MessageLevel.Error);
                    }
                    try
                    {
                        tmpDut.DutDashboard.TestSummary(tmpDut, finalres);

                        if(GlobalNew.RunMode != 2)
                        {
                            if (i == Cycles - 1)
                            {
                                if (GlobalNew.EnableDeviceCount == 1)
                                    tmpDut.DutDashboard.ResetUISNtext(true);
                                else
                                    tmpDut.DutDashboard.ResetUISNtext();
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    isRunning = 0;
                }

            }, tn);

            return task1;
        }
        public Task ActRotate(TreeNode tn, object Component)
        {
            ChangeColorFromGrandparent(tn, Color.White, Color.LightGray);
            var task1 = Task.Factory.StartNew((node) =>
            {
                bool finalres = false;
                object[] context = Component as object[];

                DUT_BASE tmpDut = (DUT_BASE)context[2];

                SetMLoggerThread(tmpDut.LOGGER);
                Bd.SetLoggerForCurrentThread(tmpDut.LOGGER);

                for (int i = 0; i < Cycles; i++)
                {
                    isRunning = 1;

                    tmpDut.DutDashboard.MemoryDataClear(tmpDut);

                    //tmpDut.DutDashboard.SaveRichTextPro(tmpDut, tmpDut.DutDashboard.DutLogRichTextBox);
                    if (i > 0)
                        tmpDut.DutDashboard.SetInfoTextboxMessage($"Cycle({i})");
                    tmpDut.SetConfig_Param();

                    tmpDut?.ClearBuffer();

                    try
                    {
                        LogMessage($"Start testing");
                        int ret = Run((TreeNode)node, Component);

                        if (ret == 1)
                        {
                            finalres = true;
                            
                            context[3] = true;
                            tmpDut.TestResult = true;
                        }
                        else
                        {
                            
                            //tmpDut.testUnit?.MarkAsFailed();
                            context[3] = false;
                            tmpDut.TestResult = false;
                        }
                        tmpDut.PopAllLog();
                        LogMessage($"Test End - Result: {(ret == 1 ? "PASS" : "FAIL")}");

                    }
                    catch (Exception ex)
                    {
                        // 這裡處理其他所有的異常
                        LogMessage($"Error in Run method: {ex.Message}", MessageLevel.Error);
                    }
                    try
                    {
                        tmpDut.DutDashboard.TestSummary(tmpDut, finalres);

                        if (GlobalNew.RunMode != 2)
                        {
                            if (i == Cycles - 1)
                            {
                                if (GlobalNew.EnableDeviceCount == 1)
                                    tmpDut.DutDashboard.ResetUISNtext(true);
                                else
                                    tmpDut.DutDashboard.ResetUISNtext();
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    isRunning = 0;
                }

            }, tn);

            return task1;
        }
        public Task Act(TreeNode tn, object Component)
        {

            //LogDetail("======" + Description + " Process Start." + "======");
            LogMessage("======" + Description + " Process Start." + "======");
            ChangeColorFromGrandparent(tn, Color.White, Color.LightGray);
            var task1 = Task.Factory.StartNew((node) =>
            {
                bool finalres = false;
                object[] context = Component as object[];
                for (int i = 0; i < Cycles; i++)
                {
                    isRunning = 1;
                    MainForm tempForm = (MainForm)context[6];

                    tempForm.ResetDutInfo();
                    try
                    {

                        int ret = Run((TreeNode)node, Component);

                        if (ret == 1)                           
                            finalres = true;

                        if (context.Length >= 6)
                        {
                            TreeNode ResetNode = (TreeNode)context[5];
                            if (ResetNode != null)//如果有初始化節點才做
                            {
                                try
                                {
                                    _DUT_Simu.isSimu = true;
                                    object[] Resetcontext = new object[] { context[0], context[1], _DUT_Simu, context[3], context[4], context[5] };
                                    Container_JIG_INIT obj = (Container_JIG_INIT)ResetNode.Tag;
                                    int ResetResult = Run(ResetNode, Resetcontext);

                                    if (ResetResult != 1)
                                        MessageBox.Show("Fixture initialization reset action failed.治具初始化動作失敗");
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Fixture initialization Exception.{ex.Message }");
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        // 這裡處理其他所有的異常
                        LogMessage($"Error in Run method: {ex.Message}", MessageLevel.Error);
                    }

                    tempForm.RecordInfo(finalres);
                    //tempForm.ResetUISNtext();
                    isRunning = 0;
                }

                //if(finalres)
                //    context[3] = true;

            }, tn);
            //LogDetail(Description + "非同步執行中.");
            //MT_Controls.MTBase.Global_Memory.TaskList.Add(task1);
            return task1;
        }
        public override int Process(TreeNode tn, object Component)
        {

            LogDetail("======" + Description + " Process Start." + "======");

            ChangeColorFromGrandparent(tn, Color.White, Color.LightGray);
            var task1 = Task.Factory.StartNew((node) =>
            {

                Run((TreeNode)node, Component);
            }, tn);
            LogDetail(Description + "非同步執行中.");
            //MT_Controls.MTBase.Global_Memory.TaskList.Add(task1);
            return 1;
        }
        private void ChangeColorFromGrandparent(TreeNode node, Color backcolor, Color forecolor)
        {
            // 遞歷處理所有子節點
            foreach (TreeNode childNode in node.Nodes)
            {
                // 檢查父節點的類別是否為目標類別
                if (childNode.Parent != null && childNode.Tag.GetType() == Global_Memory.ExHandleType)
                {
                    // 如果是目標類別，則改變其顏色及以下的所有子節點的顏色
                    ChangeExceptionColor(childNode, backcolor, forecolor);
                }

                // 遞歷處理子節點的子節點
                ChangeColorFromGrandparent(childNode, backcolor, forecolor);
            }
        }
        static public void ChangeExceptionColor(TreeNode node, Color backcolor, Color forecolor)
        {
            // 改變當前節點的顏色
            node.BackColor = backcolor;
            node.ForeColor = forecolor;

            // 遞歷處理子節點
            foreach (TreeNode childNode in node.Nodes)
            {
                ChangeExceptionColor(childNode, backcolor, forecolor);
            }


        }
    }
}
