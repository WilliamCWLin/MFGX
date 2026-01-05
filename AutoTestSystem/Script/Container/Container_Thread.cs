using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Equipment.Motion;
using Manufacture;
using Newtonsoft.Json;
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
    public class Container_Thread : Script_Container_Base
    {
        [Browsable(false)]
        [Category("Params"), Description("RetryTimes")]
        public int RetryTimes { get; set; }

        // [Browsable(false)]
        [Category("Params"), Description("Cycles")]
        public int Cycles { get; set; }

        [Category("Params"), Description("Cycles")]
        public bool FailContinue { get; set; }

        [Category("Params"), Description("Doneflag")]
        public string Doneflag { get; set; }

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


        DataGridView DataGridView;
        public Container_Thread()
        {
            //Description = "MainThread";
            RetryTimes = 0;
            Cycles = 1;
            isRunning = 0;
            FailContinue = false;

            _DUT_Simu = new DUT_Simu();
        }

        public override int Process(TreeNode tn, object Component)
        {

            LogDetail("======" + Description + " Process Start." + "======");
            int result = -1;
            ChangeColorFromGrandparent(tn, Color.White, Color.LightGray);

            object[] tmpcontext = Component as object[];

            isRunning = 1;
            DUT_BASE tempDut = (DUT_BASE)tmpcontext[2];
            tempDut.DataCollection.SetMoreProp($"{Description}({ID})_Check", "");
            var task1 = Task.Factory.StartNew((node) =>
            {

                result = Run((TreeNode)node, Component);
            }, tn);

            task1.ContinueWith(t =>
            {
                object[] context = Component as object[];

                isRunning = 1;
                DUT_BASE dut = (DUT_BASE)context[2];
                if(!string.IsNullOrEmpty(Doneflag))
                    dut.DataCollection.SetMoreProp(Doneflag, "1");

                if (result == 0)
                    dut.DataCollection.SetMoreProp($"{Description}({ID})_Check", "FAIL");
                else
                    dut.DataCollection.SetMoreProp($"{Description}({ID})_Check", "PASS");

            }); // 這樣會確保 ContinueWith 在 UI 主線程上執行}

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
