
using AutoTestSystem.Model;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    internal class Script_Extra_CheckThreadResult : Script_Extra_Base
    {

        [Category("Thread1"), Description("Key")]
        public int Timeout1 { get; set; } = 15000;

        [Category("Thread1"), Description("Fail Goto Where.Do not log error codes"), TypeConverter(typeof(ThreadList))]
        public string Thread1 { set; get; } = string.Empty;



        [Category("Thread2"), Description("Key")]
        public int Timeout2 { get; set; } = 15000;

        [Category("Thread2"), Description("Fail Goto Where.Do not log error codes"), TypeConverter(typeof(ThreadList))]
        public string Thread2 { set; get; } = string.Empty;



        [Category("Thread3"), Description("Key")]
        public int Timeout3 { get; set; } = 15000;

        [Category("Thread3"), Description("Fail Goto Where.Do not log error codes"), TypeConverter(typeof(ThreadList))]
        public string Thread3 { set; get; } = string.Empty;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public Script_Extra_CheckThreadResult()
        {
            Description = "CheckThreadResult";
        }
        public override bool PreProcess()
        {

            return true;
        }
        public override bool Process(ref string strOutData)
        {
            // 使用一個函數來統一處理執行緒檢查
            bool CheckThreadResult(string threadName, int timeout)
            {
                if (string.IsNullOrEmpty(threadName)) return true; // 如果 threadName 為空，則視為通過

                string result = string.Empty;
                Stopwatch stopwatch = Stopwatch.StartNew(); // 簡化 stopwatch 初始化

                while (result != "-1")
                {
                    if (stopwatch.ElapsedMilliseconds >= timeout)
                        return false; // 超時則失敗

                    result = PopMoreData(threadName + "_Check");

                    if (result == "PASS") return true;
                    if (result == "FAIL") return false;

                    Thread.Sleep(10); // 避免 busy-waiting
                }

                return false;
            }

            // 依次檢查所有執行緒
            bool result1 = CheckThreadResult(Thread1, Timeout1);
            bool result2 = CheckThreadResult(Thread2, Timeout2);
            bool result3 = CheckThreadResult(Thread3, Timeout3);

            return result1 && result2 && result3;

        }
        public override bool PostProcess()
        {
            return true;

        }


    }

    public class ThreadList : TypeConverter  //下拉式選單
    {
        /// <summary>
        /// 遞迴查找所有上層節點中的 Container_Thread，找到即停止
        /// </summary>
        private void GetContainerThreadsFromAncestors(TreeNode node, List<string> listID)
        {
            if (node == null || node.Parent == null) return;

            // 遍歷當前節點的所有兄弟節點
            foreach (TreeNode siblingNode in node.Parent.Nodes)
            {
                if (siblingNode.Tag is Container_Thread container)
                {
                    string description = $"{container.Description}({container.ID})";
                    if (!listID.Contains(description)) // 避免重複加入
                    {
                        listID.Add(description);
                    }
                }
            }

            // 如果已經找到，則停止遞迴
            if (listID.Count > 0) return;

            // 繼續向上一層尋找
            GetContainerThreadsFromAncestors(node.Parent, listID);
        }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> listID = new List<string>();

            // 遞迴往上層查找 Container_Thread
            GetContainerThreadsFromAncestors(Global_Memory.mySelectedScriptNode, listID);

            // 確保 StandardValuesCollection 能接受 ICollection
            ICollection result = listID.Count > 0 ? (ICollection)listID : new string[] { "0" };

            return new StandardValuesCollection(result);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;

        }

    }
}
