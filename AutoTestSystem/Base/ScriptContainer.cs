using AutoTestSystem.Base;
using AutoTestSystem.BLL;
using AutoTestSystem.Model;
using AutoTestSystem.Script;
using Manufacture;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OpenCvSharp.ML.DTrees;

namespace AutoTestSystem.Base
{
    public partial class ScriptContainer : Manufacture.ContainerNode
    {
        public ScriptContainer()
        {
            Description = GetType().Name;

            ClassName = GetType().ToString();
        }

        public bool PushMoreData(string k, string v)
        {
            if (GlobalNew.g_datacollection != null)
            {
                GlobalNew.g_datacollection.SetMoreProp(k, v);
                return true;
            }
            else
            {
                return false;
            }
        }

        public string PopMoreData(string key)
        {
            return GlobalNew.g_datacollection.GetMoreProp(key); // 如果值不為空，返回值的字符串表示形式
        }
        public virtual int Process(TreeNode tn, object Component)
        {
            return 1;
        }
        static public void ChangeColorRecursive(TreeNode node, Color backcolor, Color forecolor)
        {
            //node.TreeView.BeginUpdate();
            // 在UI執行緒上執行操作
            if (node.TreeView != null && node.TreeView.InvokeRequired)
            {
                node.TreeView.BeginInvoke((MethodInvoker)delegate
                {
                    // 改變當前節點的顏色
                    node.BackColor = backcolor;
                    node.ForeColor = forecolor;
                });
            }
            else
            {
                // 直接在當前執行緒上執行操作
                node.BackColor = backcolor;
                node.ForeColor = forecolor;
            }
            //node.TreeView.EndUpdate();
            // 遞歷處理子節點
            foreach (TreeNode childNode in node.Nodes)
            {
                ChangeColorRecursive(childNode, backcolor, forecolor);
            }
        }
        static public void ChangeDataGridRecursive(TreeNode node, DUT_BASE dut_)
        {
            if (dut_.isSimu == true)
                return;

            if (node.Tag is ScriptBase)
            {
                // 改變當前節點的顏色
                dut_.DataGridView.BeginInvoke((MethodInvoker)delegate
                {
                    InsertRow(dut_, (ScriptBase)node.Tag);
                });

            }

            // 遞歷處理子節點
            foreach (TreeNode childNode in node.Nodes)
            {
                ChangeDataGridRecursive(childNode, dut_);
            }
        }

        static public void ClearRowDataItemRecursive(TreeNode node)
        {
            if (node.Tag is ScriptBase)
            {
                // 改變當前節點的顏色
                ((ScriptBase)node.Tag).RowDataItem.Clear();

            }

            // 遞歷處理子節點
            foreach (TreeNode childNode in node.Nodes)
            {
                ClearRowDataItemRecursive(childNode);
            }
        }
        static public int Run(TreeNode treeNode)
        {
            bool Result = true;
            Dictionary<string, string> k = new Dictionary<string, string>();
            int count = 0;

            foreach (TreeNode n in treeNode.Nodes)
            {
                if (((Manufacture.CoreBase)n.Tag).ID != null)
                    k.Add(((Manufacture.CoreBase)n.Tag).Description + "(" + ((Manufacture.CoreBase)n.Tag).ID + ")", count.ToString());
                count++;
            }

            for (int i = 0; i < treeNode.Nodes.Count; i++)
            {
                if (treeNode.Nodes[i].Checked != true)
                {
                    ChangeColorRecursive(treeNode.Nodes[i], Color.White, Color.LightGray);
                    continue;
                }
                if (treeNode.Nodes[i].Tag is Container_ExceptionHandling)
                {
                    //ChangeColorRecursive(treeNode.Nodes[i], Color.White, Color.LightGray);
                    continue;
                }

                bool status = false;

                Manufacture.Global_Memory.resetEvent.WaitOne();

                //ReflashPropertyData((treeNode.Nodes[i].Tag).GetObjectValues());

                if (treeNode.Nodes[i].Tag is ScriptBase)
                {
                    treeNode.Nodes[i].BackColor = Color.LightGray;
                    ScriptBase ScriptItem = ((ScriptBase)treeNode.Nodes[i].Tag);
                    //nowobject.CurrentRunNode = treeNode.Nodes[i];

                    try
                    {
                        if (ScriptItem.Action("") == true)
                        {

                            treeNode.Nodes[i].ForeColor = Color.Green;
                            treeNode.Nodes[i].BackColor = Color.White;
                            status = true;
                        }
                        //if (nowobject.Preprocess() == 1)
                        //{
                        //    if (nowobject.Process(""))
                        //    {
                        //        if (nowobject.Postprocess() == 1)
                        //        {
                        //            treeNode.Nodes[i].ForeColor = Color.Green;
                        //            treeNode.Nodes[i].BackColor = Color.White;
                        //            status = true;
                        //        }
                        //    }
                        //}
                    }

                    catch (DumpException e)
                    {
                        treeNode.Nodes[i].ForeColor = Color.Green;
                        treeNode.Nodes[i].BackColor = Color.White;

                        int temp = i + 1;
                        string JumpTo = e.Message;

                        LogDetail("======" + treeNode.Nodes[i].Tag.ToString() + "->" + "Jump to" + JumpTo + "======");
                        if (JumpTo != null)
                            i = Int32.Parse(k[JumpTo]) - 1;


                        for (int j = temp; j <= i; j++)
                        {
                            //treeNode.Nodes[j].ForeColor = Color.Blue;
                            //treeNode.Nodes[j].BackColor = Color.White;
                            ChangeColorRecursive(treeNode.Nodes[j], Color.White, Color.LightGray);
                        }

                        status = true;
                    }
                    catch (Exception ex)
                    {

                        LogFail("======" + treeNode.Nodes[i].Tag.ToString() + "->" + ex.ToString() + "======");
                    }

                }
                else if (treeNode.Nodes[i].Tag is ScriptContainer)
                {
                    if (treeNode.Nodes[i].Tag is Container_Sequences)
                    {
                        int ret = 0;
                        for (int s = 0; s < ((Container_Sequences)treeNode.Nodes[i].Tag).Cycles; s++)
                        {
                            ChangeColorRecursive(treeNode.Nodes[i], Color.White, Color.Black);

                            ret = ((ScriptContainer)treeNode.Nodes[i].Tag).Process(treeNode.Nodes[i]);

                            if (ret == 0)
                            {

                                for (int m = 0; m < ((Container_Sequences)treeNode.Nodes[i].Tag).RetryTimes; m++)
                                {
                                    ChangeColorRecursive(treeNode.Nodes[i], Color.White, Color.Black);
                                    ret = ((ScriptContainer)treeNode.Nodes[i].Tag).Process(treeNode.Nodes[i]);
                                    if (ret == 1)
                                    {
                                        status = true;
                                        m = ((Container_Sequences)treeNode.Nodes[i].Tag).RetryTimes;
                                    }

                                }
                            }
                            else
                            {
                                status = true;
                            }
                        }

                    }
                    else
                    {
                        if (1 == ((ScriptContainer)treeNode.Nodes[i].Tag).Process(treeNode.Nodes[i]))
                        {
                            treeNode.Nodes[i].BackColor = Color.White;
                            status = true;
                        }
                    }

                }

                treeNode.Nodes[i].BackColor = Color.White;

                if (status == false)
                {
                    treeNode.Nodes[i].ForeColor = Color.Red;

                    string s = (((Manufacture.CoreBase)treeNode.Nodes[i].Tag).ExID);
                    int temp = -1;
                    if (s != null && s != string.Empty)
                        temp = Int32.Parse(k[s]);

                    if (temp != -1)
                    {
                        ScriptContainer Jumpobj = ((ScriptContainer)treeNode.Nodes[temp].Tag);

                        try
                        {
                            treeNode.Nodes[temp].ForeColor = Color.Green;
                            //treeNode.Nodes[temp].BackColor = Color.DarkBlue;
                            if (Jumpobj.Process(treeNode.Nodes[temp]) == 1)
                            {
                                treeNode.Nodes[temp].ForeColor = Color.Green;
                                treeNode.Nodes[temp].BackColor = Color.White;
                                //status = true;
                            }

                        }
                        catch
                        {
                            status = false;
                        }
                    }
                }

                Result &= status;

                if (Result == false)
                {
                    if (treeNode.Nodes[i].Tag is Container_Sequences)
                    {
                        if (((Container_Sequences)treeNode.Nodes[i].Tag).FailJump != string.Empty)
                        {
                            treeNode.Nodes[i].ForeColor = Color.Red;

                            string s = (((Container_Sequences)treeNode.Nodes[i].Tag).FailJump);
                            int temp = i + 1;
                            if (s != null && s != string.Empty)
                                i = Int32.Parse(k[s]) - 1;

                            treeNode.Nodes[temp].ForeColor = Color.Blue;

                            for (int j = temp; j <= i; j++)
                            {
                                ChangeColorRecursive(treeNode.Nodes[j], Color.White, Color.LightGray);
                            }
                        }

                    }
                    else if (treeNode.Nodes[i].Tag is ScriptBase)
                    {
                        if (status == false)
                            break;
                    }
                }

            }

            if (Result)
            {
                treeNode.ForeColor = Color.Green;
                return 1;
            }
            else
            {
                treeNode.ForeColor = Color.Red;
                return 0;
            }
        }

        static public int Run(TreeNode treeNode, object Component)
        {
            object[] context = Component as object[];
            ProTreeView.ProTreeView ProTV = (ProTreeView.ProTreeView)context[0];
            bool failcontinue = (bool)context[1];
            Dictionary<string, object> Devices=null;
            DUT_BASE Dut = null;
            if (context.Length > 2)
            {
                Dut = (DUT_BASE)context[2];
            }
            if (context.Length > 3)
            {
                Devices = (Dictionary<string, object>)context[4];
            }

            bool Result = true;
            Dictionary<string, string> k = new Dictionary<string, string>();
            int count = 0;

            foreach (TreeNode n in treeNode.Nodes)
            {
                if (((Manufacture.CoreBase)n.Tag).ID != null)
                    k.Add(((Manufacture.CoreBase)n.Tag).Description + "(" + ((Manufacture.CoreBase)n.Tag).ID + ")", count.ToString());
                count++;
            }

            for (int i = 0; i < treeNode.Nodes.Count; i++)
            {
                TreeNode CurrentNode = treeNode.Nodes[i];
                if (CurrentNode.Checked != true)
                {
                    ChangeColorRecursive(CurrentNode, Color.White, Color.LightGray);
                    continue;
                }
                if (CurrentNode.Tag is Container_ExceptionHandling)
                {
                    ChangeColorRecursive(CurrentNode, Color.White, Color.LightGray);
                    continue;
                }
                if (CurrentNode.Tag is Container_JIG_INIT)
                {
                    ChangeColorRecursive(CurrentNode, Color.White, Color.LightGray);
                    continue;
                }
                bool status = false;

                Dut.MainThread?.resetEvent.WaitOne();

                //g_isRunning代表主測試頁面運行中才偵測，防止設定頁面單步執行因只判定g_shouldStop而無法運行
                if (GlobalNew.g_shouldStop && GlobalNew.g_recipesteprun == false)
                {
                    //GlobalNew.g_HomeProcessSuccess = false;// 強制圓盤模式下按停止下次啟動需HOME
                    string tmp_failitem = Dut.DataCollection.GetMoreProp("Failitem");
                    if (string.IsNullOrEmpty(tmp_failitem))
                    {
                        Dut.DataCollection.SetMoreProp("Failitem", "ABORT");
                    }
                    return 0;
                }
                ((Manufacture.CoreBase)CurrentNode.Tag).StartTime = DateTime.Now;

                if (CurrentNode.Tag is ScriptBase)
                {

                    treeNode.TreeView.Invoke((MethodInvoker)delegate
                    {
                        CurrentNode.BackColor = Color.LightGray;
                        CurrentNode.ForeColor = Color.Blue;
                        CurrentNode.EnsureVisible();
                    });
                    ScriptBase ScriptItem = ((ScriptBase)CurrentNode.Tag);
                    ScriptItem.SetDataDestination(Dut);
                    //nowobject.CurrentRunNode = CurrentNode;
                    DateTime startTime = DateTime.Now;
                    ScriptItem.RowDataItem.Clear();
                    ScriptItem.RowDataItem.StartTime = startTime;
                    try
                    {
                        ScriptItem.RowDataItem.Item = ScriptItem.Description;
                        ScriptItem.RowDataItem.Spec = ScriptItem.Spec;
                        ScriptItem.RetryCount++;
                        bool testres = ScriptItem.Action(Devices);
                        
                        if (ScriptItem.RowDataItem.TestCount > 0)
                        {
                            ScriptItem.RowDataItem.RetryTimes++;
                            treeNode.TreeView.Invoke((MethodInvoker)delegate
                            {

                                CurrentNode.Text = ($"{ScriptItem.Description}({ScriptItem.RowDataItem.RetryTimes})");
                            });
                        }
                        ScriptItem.RowDataItem.TestCount++;
                        if (testres)
                        {
                            
                            //treeNode.TreeView.Invoke((MethodInvoker)delegate
                            //{
                            //    CurrentNode.ForeColor = Color.Green;
                            //    CurrentNode.BackColor = Color.White;
                            //});
                            Dut.TestInfo.AddTestStep("DetailItem", ScriptItem.RowDataItem);
                            status = true;
                        }
                        else
                        {
                            Dut.TestInfo.AddTestStep("DetailItem", ScriptItem.RowDataItem);
                            //有設異常停止則不做Retry
                            if (string.IsNullOrEmpty(ScriptItem.FailStopGoto))
                            {
                                for (int m = 0; m < ScriptItem.RetryTimes; m++)
                                {
                                    ScriptItem.RetryCount++;
                                    bool res = ScriptItem.Action(Devices);

                                    treeNode.TreeView.Invoke((MethodInvoker)delegate
                                    {
                                        ScriptItem.RowDataItem.RetryTimes++;
                                        CurrentNode.Text = ($"{ScriptItem.Description}({ScriptItem.RowDataItem.RetryTimes})");
                                    });
                                    ScriptItem.RowDataItem.TestCount++;
                                    if (res)
                                    {
                                        Dut.TestInfo.AddTestStep("DetailItem", ScriptItem.RowDataItem);
                                        //treeNode.TreeView.Invoke((MethodInvoker)delegate
                                        //{
                                        //    CurrentNode.ForeColor = Color.Green;
                                        //    CurrentNode.BackColor = Color.White;
                                        //});

                                        status = true;
                                        break;
                                    }
                                    else
                                    {
                                        Dut.TestInfo.AddTestStep("DetailItem", ScriptItem.RowDataItem);
                                    }
                                }
                            }
                               
                            if(status != true)
                            {
                                string tmp_failitem = Dut.DataCollection.GetMoreProp("Failitem");
                                if(tmp_failitem == "")
                                {
                                    if(string.IsNullOrEmpty(ScriptItem.ErrorCode))
                                    {
                                        Dut.DataCollection.SetMoreProp("Failitem", "Undefined");
                                        Bd.Logger.Fatal($"({ScriptItem.Description})Error Code is empty. Set Undefined");
                                    }
                                    else
                                    {
                                        Dut.DataCollection.SetMoreProp("Failitem", ScriptItem.ErrorCode);
                                    }
                                }
                                    

                                if (CurrentNode.Parent != null)
                                {
                                    for (int j = CurrentNode.Index + 1; j < CurrentNode.Parent.Nodes.Count; j++)
                                    {
                                        ChangeColorRecursive(CurrentNode.Parent.Nodes[j], Color.White, Color.LightGray);

                                    }
                                }

                                if (!string.IsNullOrEmpty(ScriptItem.FailStopGoto))
                                {
                                    Dut.DataCollection.SetMoreProp("FailStopGoto", ScriptItem.FailStopGoto);
                                    treeNode.TreeView.Invoke((MethodInvoker)delegate
                                    {
                                        CurrentNode.ForeColor = Color.Red;
                                        CurrentNode.BackColor = Color.White;
                                    });

                                    TimeSpan elapsedTime2 = DateTime.Now - startTime;
                                    double elapsedTimeSeconds2 = elapsedTime2.TotalMilliseconds / 1000.0;
                                    ((ScriptBase)CurrentNode.Tag).RowDataItem.TestTime = /*GlobalNew.stopwatch.Elapsed.TotalSeconds;*/ Math.Round(elapsedTimeSeconds2, 3);
                                    if (GlobalNew.FormMode == "1")
                                        ((ScriptBase)CurrentNode.Tag).RowDataItem.EslapseTime = Dut.stopwatch.Elapsed.TotalSeconds;
                                    else
                                        ((ScriptBase)CurrentNode.Tag).RowDataItem.EslapseTime = GlobalNew.stopwatch.Elapsed.TotalSeconds;


                                    if (Dut != null)
                                    {
                                        Dut.DataGridView.Invoke((MethodInvoker)delegate
                                        {
                                            InsertRow(Dut, (ScriptBase)CurrentNode.Tag);
                                        });
                                    }


                                    return 0;
                                }
                            }
                        }
                    }

                    catch (DumpException e)
                    {
                        treeNode.TreeView.Invoke((MethodInvoker)delegate
                        {
                            CurrentNode.ForeColor = Color.Green;
                            CurrentNode.BackColor = Color.White;
                        });

                        if (e.Message == "Break")
                        {
                            //ChangeColorRecursive(CurrentNode, Color.White, Color.LightGray);
                            //for (int j = i + 1; j < treeNode.Nodes.Count; j++)
                            //{
                            //    ChangeDataGridRecursive(treeNode.Nodes[j], Dut);
                            //}
                            break;
                        }


                        int temp = i + 1;
                        string JumpTo = e.Message;

                        LogDetail("======" + CurrentNode.Tag.ToString() + "->" + "Jump to" + JumpTo + "======");
                        try
                        {
                            if (JumpTo != null)
                                i = Int32.Parse(k[JumpTo]) - 1;
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show($"指定的Case ID不存在 {ex.Message}");
                            status = false;
                            return 0;
                        }


                        for (int j = temp; j <= i; j++)
                        {
                            //treeNode.Nodes[j].ForeColor = Color.Blue;
                            //treeNode.Nodes[j].BackColor = Color.White;
                            ChangeColorRecursive(treeNode.Nodes[j], Color.White, Color.LightGray);
                        }

                        status = true;
                    }
                    catch (Exception ex)
                    {

                        LogFail("======" + CurrentNode.Tag.ToString() + "->" + ex.ToString() + "======");
                    }

                    DateTime endTime = DateTime.Now;
                    TimeSpan elapsedTime = endTime - startTime;
                    double elapsedTimeSeconds = elapsedTime.TotalMilliseconds / 1000.0;
                    ((ScriptBase)CurrentNode.Tag).RowDataItem.TestTime = /*GlobalNew.stopwatch.Elapsed.TotalSeconds;*/ Math.Round(elapsedTimeSeconds, 3);
                    if (GlobalNew.FormMode == "1")
                        ((ScriptBase)CurrentNode.Tag).RowDataItem.EslapseTime = Dut.stopwatch.Elapsed.TotalSeconds;
                    else
                        ((ScriptBase)CurrentNode.Tag).RowDataItem.EslapseTime = GlobalNew.stopwatch.Elapsed.TotalSeconds;

                    if (((ScriptBase)CurrentNode.Tag).ShowItem)
                    {
                        if (Dut != null && Dut.DataGridView != null && Dut.DataGridView.Parent != null)
                        {

                            if (((ScriptBase)CurrentNode.Tag).ShowItem)
                            {
                                if (Dut != null && Dut.DataGridView != null && Dut.DataGridView.Parent != null)
                                {                                  
                                    Dut.DataGridView.Invoke((MethodInvoker)delegate
                                    {
                                        InsertRow(Dut, (ScriptBase)CurrentNode.Tag);
                                    });
                                }                                    
                            }
                        }                           
                    }

                    if(status)
                    {
                        treeNode.TreeView.Invoke((MethodInvoker)delegate
                        {
                            CurrentNode.ForeColor = Color.Green;
                            CurrentNode.BackColor = Color.White;
                        });
                    }
                    else
                    {
                        treeNode.TreeView.Invoke((MethodInvoker)delegate
                        {
                            CurrentNode.ForeColor = Color.Red;
                            CurrentNode.BackColor = Color.White;
                        });
                    }

                }
                else if (CurrentNode.Tag is ScriptContainer)
                {

                    if (CurrentNode.Tag is Container_Sequences)
                    {
                        Dut.DataCollection.SetMoreProp($"({((ScriptContainer)CurrentNode.Tag).ID})_Retry", $"00");

                        int ret = 0;
                        for (int s = 0; s < ((Container_Sequences)CurrentNode.Tag).Cycles; s++)
                        {
                            //ChangeColorRecursive(CurrentNode, Color.White, Color.Black);
                            //TT.Show(CurrentNode.Text, tp.Item1, CurrentNode.Bounds.Right, CurrentNode.Bounds.Bottom);
                            ((ScriptContainer)CurrentNode.Tag).RetryCount++;
                            ret = ((ScriptContainer)CurrentNode.Tag).Process(CurrentNode, Component);

                            //直接跳出不進行retry
                            if (Dut.DataCollection.GetMoreProp("FailStopGoto") != "")
                            {
                                break;
                            }

                            if (ret == 0)
                            {
                                //因為有cycle時這邊還是要設為fail
                                status = false;

                                if (failcontinue == false)
                                {
                                    if (((Container_Sequences)CurrentNode.Tag).RetryTimes == 0)
                                    {
                                        ChangeDataGridRecursive(CurrentNode, Dut);
                                    }
                                    else
                                    {
                                        for (int m = 0; m < ((Container_Sequences)CurrentNode.Tag).RetryTimes; m++)
                                        {
                                            Dut.DataCollection.SetMoreProp($"({((ScriptContainer)CurrentNode.Tag).ID})_Retry", $"{(m + 1).ToString("D2")}");
                                            treeNode.TreeView.Invoke((MethodInvoker)delegate
                                            {
                                                CurrentNode.Text = ($"{((ScriptContainer)CurrentNode.Tag).Description}({m + 1})");
                                            });

                                            ChangeColorRecursive(CurrentNode, Color.White, Color.LightGray);
                                            //retry前清除節點內的所有Rowdataitem的資料
                                            ClearRowDataItemRecursive(CurrentNode);

                                            ((ScriptContainer)CurrentNode.Tag).RetryCount++;
                                            ret = ((ScriptContainer)CurrentNode.Tag).Process(CurrentNode, Component);

                                            if (Dut.DataCollection.GetMoreProp("FailStopGoto") != "")
                                            {
                                                break;
                                            }

                                            if (ret == 1)
                                            {
                                                status = true;
                                                
                                                //retry 救回來清掉Failitem
                                                Dut.DataCollection.SetMoreProp("Failitem", "");
                                                m = ((Container_Sequences)CurrentNode.Tag).RetryTimes;
                                            }
                                            else
                                            {
                                                if (m == ((Container_Sequences)CurrentNode.Tag).RetryTimes - 1)
                                                    ChangeDataGridRecursive(CurrentNode, Dut);
                                            }
                                        }
                                    }

                                }
                            }
                            else
                            {
                                status = true;
                            }
                        }
                    }
                    else if(CurrentNode.Tag is Container_Post_Process)
                    {
                        if (1 == ((ScriptContainer)CurrentNode.Tag).Process(CurrentNode, Component))
                        {
                            treeNode.TreeView.Invoke((MethodInvoker)delegate
                            {
                                CurrentNode.ForeColor = Color.Green;
                            });
                            status = true;
                        }
                    }
                    else if (CurrentNode.Tag is Container_Condition_Jumper)
                    {
                        try
                        {
                            if (1 == ((ScriptContainer)CurrentNode.Tag).Process(CurrentNode, Component))
                            {
                                treeNode.TreeView.Invoke((MethodInvoker)delegate
                                {
                                    CurrentNode.ForeColor = Color.Green;
                                });
                                status = true;
                            }
                        }
                        catch (DumpException e)
                        {
                            treeNode.TreeView.Invoke((MethodInvoker)delegate
                            {
                                CurrentNode.ForeColor = Color.Green;
                                CurrentNode.BackColor = Color.White;
                            });
                            ChangeColorRecursive(CurrentNode, Color.White, Color.Blue);
                            if (e.Message == "Break")
                            {
                                break;
                            }


                            int temp = i + 1;
                            string JumpTo = e.Message;

                            LogDetail("======" + CurrentNode.Tag.ToString() + "->" + "Jump to" + JumpTo + "======");
                            try
                            {
                                if (JumpTo != null)
                                    i = Int32.Parse(k[JumpTo]) - 1;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"指定的Case ID不存在 {ex.Message}");
                                status = false;
                                return 0;
                            }


                            for (int j = temp; j <= i; j++)
                            {
                                //treeNode.Nodes[j].ForeColor = Color.Blue;
                                //treeNode.Nodes[j].BackColor = Color.White;
                                ChangeColorRecursive(treeNode.Nodes[j], Color.White, Color.LightGray);
                            }

                            status = true;
                        }
 
                    }
                    else
                    {
                        if (1 == ((ScriptContainer)CurrentNode.Tag).Process(CurrentNode, Component))
                        {
                            status = true;
                        }
                    }
                }

                treeNode.TreeView.Invoke((MethodInvoker)delegate
                {
                    CurrentNode.BackColor = Color.White;
                });
                
                ((Manufacture.CoreBase)CurrentNode.Tag).EndTime = DateTime.Now;
                if (((Manufacture.CoreBase)CurrentNode.Tag).StartTime.HasValue && ((Manufacture.CoreBase)CurrentNode.Tag).EndTime.HasValue)
                {
                    ((Manufacture.CoreBase)CurrentNode.Tag).DurationSec = Math.Round(
                        (((Manufacture.CoreBase)CurrentNode.Tag).EndTime.Value - ((Manufacture.CoreBase)CurrentNode.Tag).StartTime.Value).TotalSeconds,
                        3, System.MidpointRounding.AwayFromZero);
                }
                ((Manufacture.CoreBase)CurrentNode.Tag).Result = status ? "PASS" : "FAIL";
                Result &= status;
                
                Dut.DataCollection.SetMoreProp($"{((Manufacture.CoreBase)CurrentNode.Tag).Description}({((Manufacture.CoreBase)CurrentNode.Tag).ID})", Result?"PASS":"FAIL");

                if (status == false)
                {
                    if (CurrentNode.Tag is Container_Sequences)
                    {
                        if (failcontinue == false)
                        {
                            if (Dut.DataCollection.GetMoreProp("FailStopGoto") != "")
                            {
                                treeNode.TreeView.Invoke((MethodInvoker)delegate
                                {
                                    CurrentNode.ForeColor = Color.Red;
                                    CurrentNode.BackColor = Color.White;
                                });

                                int temp = i + 1;
                                string JumpTo = Dut.DataCollection.GetMoreProp("FailStopGoto");

                                if (!string.IsNullOrEmpty(JumpTo))
                                {
                                    if (k.ContainsKey(JumpTo))
                                    {
                                        Dut.DataCollection.SetMoreProp("FailStopGoto", "");
                                        if (JumpTo != null)
                                            i = Int32.Parse(k[JumpTo]) - 1;

                                        for (int j = temp; j <= i; j++)
                                        {
                                            ChangeColorRecursive(treeNode.Nodes[j], Color.White, Color.LightGray);
                                            ChangeDataGridRecursive(treeNode.Nodes[j], Dut);
                                        }
                                    }
                                    else
                                    {
                                        for (int j = temp; j < treeNode.Nodes.Count; j++)
                                        {
                                            ChangeColorRecursive(treeNode.Nodes[j], Color.White, Color.LightGray);
                                            ChangeDataGridRecursive(treeNode.Nodes[j], Dut);
                                        }
                                        //代表可能設定的跳轉不在這層Container中，直接在回到上層
                                        return 0;
                                    }
                                }
      

                            }

                            else
                            {
                                if (((Container_Sequences)CurrentNode.Tag).ExID != string.Empty)
                                {
                                    string s = (((Manufacture.CoreBase)CurrentNode.Tag).ExID);

                                    int temp = -1;
                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        if (k.ContainsKey(s))
                                        {
                                            if (s != null && s != string.Empty)
                                                temp = Int32.Parse(k[s]);
                                        }
                                    }

                                    if (temp != -1)
                                    {
                                        ScriptContainer Jumpobj = ((ScriptContainer)treeNode.Nodes[temp].Tag);

                                        //暫存異常處理前的Failitem
                                        string tmp_failitem = Dut.DataCollection.GetMoreProp("Failitem");
                                        try
                                        {
                                            treeNode.Nodes[temp].ForeColor = Color.Green;
                                            //treeNode.Nodes[temp].BackColor = Color.DarkBlue;

                                            if (Jumpobj.Process(treeNode.Nodes[temp], Component) == 1)
                                            {
                                                treeNode.TreeView.Invoke((MethodInvoker)delegate
                                                {
                                                    treeNode.Nodes[temp].ForeColor = Color.Green;
                                                    treeNode.Nodes[temp].BackColor = Color.White;
                                                });
                                            }
                                            else
                                            {
                                                treeNode.Nodes[temp].ForeColor = Color.Red;
                                            }

                                        }
                                        catch
                                        {
                                            status = false;
                                        }
                                        //異常處理完再寫回去，不記錄異常處理中發生的Fail
                                        Dut.DataCollection.SetMoreProp("Failitem", tmp_failitem);

                                        for (int j = i + 1; j < treeNode.Nodes.Count; j++)
                                        {
                                            if (temp == j)
                                                continue;
                                            ChangeColorRecursive(treeNode.Nodes[j], Color.White, Color.LightGray);
                                        }
                                    }

                                    if (status == false)
                                        break;
                                }
                                if (((Container_Sequences)CurrentNode.Tag).FailJump == "Continue")
                                {

                                }
                                else if (((Container_Sequences)CurrentNode.Tag).FailJump == "Abort")
                                {
                                    CurrentNode.TreeView.Invoke((MethodInvoker)delegate
                                    {
                                        CurrentNode.ForeColor = Color.White;
                                        CurrentNode.BackColor = Color.Red;
                                        CurrentNode.Text = "[Abort]"+CurrentNode.Text;
                                    });
                                    GlobalNew.g_shouldStop = true;

                                    Dut.LogMessage("Abort the process when an anomaly occurs.",MessageLevel.Error);
                                    if (status == false)
                                        break;
                                }
                                else if (((Container_Sequences)CurrentNode.Tag).FailJump == "Break")
                                {

                                    for (int j = i + 1; j < treeNode.Nodes.Count; j++)
                                    {                                                                           
                                        ChangeDataGridRecursive(treeNode.Nodes[j], Dut);
                                    }
                                    if (status == false)
                                        break;
                                }
                                else if (((Container_Sequences)CurrentNode.Tag).FailJump != string.Empty)
                                {
                                    treeNode.TreeView.Invoke((MethodInvoker)delegate
                                    {
                                        CurrentNode.ForeColor = Color.Red;
                                    });

                                    string s = (((Container_Sequences)CurrentNode.Tag).FailJump);

                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        if (k.ContainsKey(s))
                                        {
                                            int temp = i + 1;
                                            if (s != null && s != string.Empty)
                                                i = Int32.Parse(k[s]) - 1;

                                            for (int j = temp; j <= i; j++)
                                            {
                                                ChangeColorRecursive(treeNode.Nodes[j], Color.White, Color.LightGray);

                                                ChangeDataGridRecursive(treeNode.Nodes[j], Dut);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (CurrentNode.Tag is Container_RepeatUntil)
                    {
                        if (failcontinue == false)
                        {
                            if (Dut.DataCollection.GetMoreProp("FailStopGoto") != "")
                            {
                                treeNode.TreeView.Invoke((MethodInvoker)delegate
                                {
                                    CurrentNode.ForeColor = Color.Red;
                                    CurrentNode.BackColor = Color.White;
                                });

                                int temp = i + 1;
                                string JumpTo = Dut.DataCollection.GetMoreProp("FailStopGoto");

                                if (!string.IsNullOrEmpty(JumpTo))
                                {
                                    if (k.ContainsKey(JumpTo))
                                    {
                                        Dut.DataCollection.SetMoreProp("FailStopGoto", "");
                                        if (JumpTo != null)
                                            i = Int32.Parse(k[JumpTo]) - 1;

                                        for (int j = temp; j <= i; j++)
                                        {
                                            ChangeColorRecursive(treeNode.Nodes[j], Color.White, Color.LightGray);
                                            ChangeDataGridRecursive(treeNode.Nodes[j], Dut);
                                        }
                                    }
                                    else
                                    {
                                        for (int j = temp; j < treeNode.Nodes.Count; j++)
                                        {
                                            ChangeColorRecursive(treeNode.Nodes[j], Color.White, Color.LightGray);
                                            ChangeDataGridRecursive(treeNode.Nodes[j], Dut);
                                        }
                                        //代表可能設定的跳轉不在這層Container中，直接在回到上層
                                        return 0;
                                    }
                                }


                            }

                            else
                            {
                                if (((Container_RepeatUntil)CurrentNode.Tag).ExID != string.Empty)
                                {
                                    string s = (((Manufacture.CoreBase)CurrentNode.Tag).ExID);

                                    int temp = -1;
                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        if (k.ContainsKey(s))
                                        {
                                            if (s != null && s != string.Empty)
                                                temp = Int32.Parse(k[s]);
                                        }
                                    }

                                    if (temp != -1)
                                    {
                                        ScriptContainer Jumpobj = ((ScriptContainer)treeNode.Nodes[temp].Tag);

                                        //暫存異常處理前的Failitem
                                        string tmp_failitem = Dut.DataCollection.GetMoreProp("Failitem");
                                        try
                                        {
                                            treeNode.Nodes[temp].ForeColor = Color.Green;
                                            //treeNode.Nodes[temp].BackColor = Color.DarkBlue;

                                            if (Jumpobj.Process(treeNode.Nodes[temp], Component) == 1)
                                            {
                                                treeNode.TreeView.Invoke((MethodInvoker)delegate
                                                {
                                                    treeNode.Nodes[temp].ForeColor = Color.Green;
                                                    treeNode.Nodes[temp].BackColor = Color.White;
                                                });
                                            }
                                            else
                                            {
                                                treeNode.Nodes[temp].ForeColor = Color.Red;
                                            }

                                        }
                                        catch
                                        {
                                            status = false;
                                        }
                                        //異常處理完再寫回去，不記錄異常處理中發生的Fail
                                        Dut.DataCollection.SetMoreProp("Failitem", tmp_failitem);

                                        for (int j = i + 1; j < treeNode.Nodes.Count; j++)
                                        {
                                            if (temp == j)
                                                continue;
                                            ChangeColorRecursive(treeNode.Nodes[j], Color.White, Color.LightGray);
                                        }
                                    }

                                    if (status == false)
                                        break;
                                }
                                if (((Container_RepeatUntil)CurrentNode.Tag).FailJump == "Continue")
                                {

                                }
                                else if (((Container_RepeatUntil)CurrentNode.Tag).FailJump == "Abort")
                                {
                                    GlobalNew.g_shouldStop = true;
                                    Dut.LogMessage("Abort the process when an anomaly occurs.", MessageLevel.Error);
                                    if (status == false)
                                        break;
                                }
                                
                                else if (((Container_RepeatUntil)CurrentNode.Tag).FailJump == "Break")
                                {

                                    for (int j = i + 1; j < treeNode.Nodes.Count; j++)
                                    {
                                        ChangeDataGridRecursive(treeNode.Nodes[j], Dut);
                                    }
                                    if (status == false)
                                        break;
                                }
                                else if (((Container_RepeatUntil)CurrentNode.Tag).FailJump != string.Empty)
                                {
                                    treeNode.TreeView.Invoke((MethodInvoker)delegate
                                    {
                                        CurrentNode.ForeColor = Color.Red;
                                    });

                                    string s = (((Container_RepeatUntil)CurrentNode.Tag).FailJump);

                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        if (k.ContainsKey(s))
                                        {
                                            int temp = i + 1;
                                            if (s != null && s != string.Empty)
                                                i = Int32.Parse(k[s]) - 1;

                                            for (int j = temp; j <= i; j++)
                                            {
                                                ChangeColorRecursive(treeNode.Nodes[j], Color.White, Color.LightGray);

                                                ChangeDataGridRecursive(treeNode.Nodes[j], Dut);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (CurrentNode.Tag is ScriptBase)
                    {
                        treeNode.TreeView.Invoke((MethodInvoker)delegate
                        {
                            CurrentNode.ForeColor = Color.Red;
                        });
                        if (failcontinue == false)
                        {
                            string s = (((Manufacture.CoreBase)CurrentNode.Tag).ExID);

                            int temp = -1;
                            if (!string.IsNullOrEmpty(s))
                            {
                                if (k.ContainsKey(s))
                                {
                                    if (s != null && s != string.Empty)
                                        temp = Int32.Parse(k[s]);
                                }
                            }

                            if (temp != -1)
                            {
                                ScriptContainer Jumpobj = ((ScriptContainer)treeNode.Nodes[temp].Tag);

                                //暫存異常處理前的Failitem
                                string tmp_failitem = Dut.DataCollection.GetMoreProp("Failitem");
                                try
                                {
                                    treeNode.Nodes[temp].ForeColor = Color.Green;
                                    //treeNode.Nodes[temp].BackColor = Color.DarkBlue;

                                    if (Jumpobj.Process(treeNode.Nodes[temp], Component) == 1)
                                    {
                                        treeNode.TreeView.Invoke((MethodInvoker)delegate
                                        {
                                            treeNode.Nodes[temp].ForeColor = Color.Green;
                                            treeNode.Nodes[temp].BackColor = Color.White;
                                        });
                                    }

                                }
                                catch
                                {
                                    status = false;
                                }
                                //異常處理完再寫回去，不記錄異常處理中發生的Fail
                                Dut.DataCollection.SetMoreProp("Failitem", tmp_failitem);
                            }

                            if (status == false)
                                break;
                        }
                    }

                }
            }

            
            if (Result)
            {
                treeNode.TreeView.Invoke((MethodInvoker)delegate
                {
                    treeNode.ForeColor = Color.Green;
                });
                return 1;
            }
            else
            {
                if (treeNode.Tag is Container_Post_Process)
                {
                    treeNode.TreeView.Invoke((MethodInvoker)delegate
                    {
                        treeNode.Text = ($"{treeNode.Text}(Ignore Fail)");
                        treeNode.ForeColor = Color.Orange;
                    });
                    return 1;
                }

                treeNode.TreeView.Invoke((MethodInvoker)delegate
                {
                    treeNode.ForeColor = Color.Red;
                });
                return 0;
            }
        }

        static public int InsertRow(DUT_BASE DUT, ScriptBase TestScriptItem)
        {
            if(GlobalNew.FormMode == "0")
            {
                if (DUT.isSimu)
                    return 1;

                string uniqueValue = TestScriptItem.ID;
                int rowIndex = -1;

                foreach (var key in GlobalNew.DataGridViewsList.Keys)
                {

                    var dataGridView = GlobalNew.DataGridViewsList[key];

                    foreach (DataGridViewRow row in dataGridView.Rows)
                    {
                        if (row.Cells["ID"].Value.ToString() == uniqueValue)
                        {
                            rowIndex = row.Index;
                            break;
                        }
                    }

                    if (rowIndex != -1)
                    {

                        var targetRow = dataGridView.Rows[rowIndex];

                        dataGridView.CurrentCell = targetRow.Cells[0];

                        if (TestScriptItem.RowDataItem.TestResult == "" || TestScriptItem.RowDataItem.TestResult == null)
                        {
                            targetRow.Cells["Result"].Style.ForeColor = Color.Black;
                            //targetRow.Cells["Result"].Style.Font = new Font("Helvetica", 9, FontStyle.Regular);
                            targetRow.Cells["Result"].Value = "N/A";
                        }
                        else
                        {


                            targetRow.Cells["Result"].Value = TestScriptItem.RowDataItem.TestResult;




                            //targetRow.Cells["Result"].Style.Font = new Font("Helvetica", 9, FontStyle.Bold);



                            if (TestScriptItem.RowDataItem.TestResult == "PASS")
                                targetRow.Cells["Result"].Style.ForeColor = Color.Green;
                            else
                                targetRow.Cells["Result"].Style.ForeColor = Color.Red;
                        }

                        if (key == DUT.Description)
                        {
                            if (TestScriptItem.RowDataItem.Value == "" || TestScriptItem.RowDataItem.Value == null)
                            {
                                DUT.DataGridView.Rows[rowIndex].Cells["Value"].Value = "N/A";
                            }
                            else
                            {
                                DUT.DataGridView.Rows[rowIndex].Cells["Value"].Value = TestScriptItem.RowDataItem.Value;
                            }

                            if (TestScriptItem.RowDataItem.TestTime == 0 && (TestScriptItem.RowDataItem.Value == null || TestScriptItem.RowDataItem.Value == ""))
                            {
                                DUT.DataGridView.Rows[rowIndex].Cells["TestTime"].Value = "N/A";
                                DUT.DataGridView.Rows[rowIndex].Cells["Retry"].Value = "N/A";
                            }
                            else
                            {
                                DUT.DataGridView.Rows[rowIndex].Cells["TestTime"].Value = TestScriptItem.RowDataItem.TestTime;
                                DUT.DataGridView.Rows[rowIndex].Cells["Retry"].Value = TestScriptItem.RowDataItem.RetryTimes;
                            }

                            if (TestScriptItem.RowDataItem.EslapseTime == 0 && (TestScriptItem.RowDataItem.Value == null || TestScriptItem.RowDataItem.Value == ""))
                            {
                                DUT.DataGridView.Rows[rowIndex].Cells["Eslapse"].Value = "N/A";
                            }
                            else
                            {
                                DUT.DataGridView.Rows[rowIndex].Cells["Eslapse"].Value = TestScriptItem.RowDataItem.EslapseTime.ToString("F3");
                            }
                        }

                        break;
                    }
                }
               


                return 1;

            }
            else
            {
                if (DUT.isSimu)
                    return 1;

                string uniqueValue = TestScriptItem.ID;
                int rowIndex = -1;

                //foreach (var key in GlobalNew.DataGridViewsList.Keys)
                {

                    //var dataGridView = GlobalNew.DataGridViewsList[key];
                    var dataGridView = DUT.DataGridView;
                    foreach (DataGridViewRow row in dataGridView.Rows)
                    {
                        if (row.Cells["ID"].Value.ToString() == uniqueValue)
                        {
                            rowIndex = row.Index;
                            break;
                        }
                    }

                    if (rowIndex != -1)
                    {

                        var targetRow = dataGridView.Rows[rowIndex];

                        dataGridView.CurrentCell = targetRow.Cells[0];

                        if (TestScriptItem.RowDataItem.TestResult == "" || TestScriptItem.RowDataItem.TestResult == null)
                        {
                            targetRow.Cells["Result"].Style.ForeColor = Color.Black;
                            //targetRow.Cells["Result"].Style.Font = new Font("Helvetica", 9, FontStyle.Regular);
                            targetRow.Cells["Result"].Value = "N/A";
                        }
                        else
                        {


                            targetRow.Cells["Result"].Value = TestScriptItem.RowDataItem.TestResult;




                            //targetRow.Cells["Result"].Style.Font = new Font("Helvetica", 9, FontStyle.Bold);



                            if (TestScriptItem.RowDataItem.TestResult == "PASS")
                                targetRow.Cells["Result"].Style.ForeColor = Color.Green;
                            else
                                targetRow.Cells["Result"].Style.ForeColor = Color.Red;
                        }

                        //if (key == DUT.Description)
                        {
                            if (TestScriptItem.RowDataItem.Value == "" || TestScriptItem.RowDataItem.Value == null)
                            {
                                DUT.DataGridView.Rows[rowIndex].Cells["Value"].Value = "N/A";
                            }
                            else
                            {
                                DUT.DataGridView.Rows[rowIndex].Cells["Value"].Value = TestScriptItem.RowDataItem.Value;
                            }

                            if (TestScriptItem.RowDataItem.TestTime == 0 && (TestScriptItem.RowDataItem.Value == null || TestScriptItem.RowDataItem.Value == ""))
                            {
                                DUT.DataGridView.Rows[rowIndex].Cells["TestTime"].Value = "N/A";
                                DUT.DataGridView.Rows[rowIndex].Cells["Retry"].Value = "N/A";
                            }
                            else
                            {
                                DUT.DataGridView.Rows[rowIndex].Cells["TestTime"].Value = TestScriptItem.RowDataItem.TestTime;
                                DUT.DataGridView.Rows[rowIndex].Cells["Retry"].Value = TestScriptItem.RowDataItem.RetryTimes;
                            }

                            if (TestScriptItem.RowDataItem.EslapseTime == 0 && (TestScriptItem.RowDataItem.Value == null || TestScriptItem.RowDataItem.Value == ""))
                            {
                                DUT.DataGridView.Rows[rowIndex].Cells["Eslapse"].Value = "N/A";
                            }
                            else
                            {
                                DUT.DataGridView.Rows[rowIndex].Cells["Eslapse"].Value = TestScriptItem.RowDataItem.EslapseTime.ToString("F3");
                            }
                        }

                        //break;
                    }
                }

                return 1;

            }
   
        }
    }
}
