using AutoTestSystem.Base;
using AutoTestSystem.Equipment.DosBase;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;


namespace AutoTestSystem.Script
{
    internal class Script_Condition_Execute : Script_ControlDevice_Base
    {
        string strActItem = string.Empty;
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("支援用%%方式做變數值取代")]
        public string Send_Command { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int WaitTime { get; set; } = 3;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string CheckStr { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(ActionList))]
        public string ActionItem { get; set; } = "";

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(ModeList))]
        public string ModeItem { get; set; } = "Block";

        [Category("Condition"), Description("Fail Goto Where.Do not log error codes"), TypeConverter(typeof(GotoList))]
        public string FAIL_GOTO { set; get; }
        [Category("Condition"), Description("Pass Goto Where"), TypeConverter(typeof(GotoList))]
        public string PASS_GOTO { set; get; }
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }     

        public override bool PreProcess()
        {
            if (Send_Command == null || Send_Command == string.Empty)
            {
                LogMessage("Send_Command can not be null.", MessageLevel.Error);
                return false;
            }

            if (CheckStr == null || CheckStr == string.Empty)
            {
                LogMessage("CheckStr can not be null.", MessageLevel.Error);
                return false;
            }

            strActItem = ActionItem;

            return true;
        }
        public override bool Process(ControlDeviceBase PCCmd, ref string output)
        {
            PCCmd.SetTimeout(WaitTime);
            if (ModeItem == "Non-Block")
            {
                PCCmd.SendNonblock(ReplaceProp(Send_Command), ref output);
                strOutData = output;
            }
            else
            {
                PCCmd.SetCheckstr(ReplaceProp(CheckStr));
                PCCmd.Send(ReplaceProp(Send_Command), strActItem);
                LogMessage($"Send:  {ReplaceProp(Send_Command)}\n");
                PCCmd.READ(ref output);
                LogMessage($"Read END:  {output}\n");
                strOutData = output;
            }
            return true;

        }
        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if(string.IsNullOrEmpty(PASS_GOTO) || string.IsNullOrEmpty(FAIL_GOTO))
            {
                if (result == "PASS" || Spec == "")
                    return true;
                else
                    return false;
            }
            else
            {
                if (result == "PASS" || Spec == "")
                    throw new DumpException(PASS_GOTO);
                else
                {
                    throw new DumpException(FAIL_GOTO);
                }
            }           
        }

    }

    public class GotoList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (Global_Memory.select_Node_Parent != null)
            {
                if (Global_Memory.select_Node_Parent.Nodes != null)
                {
                    List<string> List_ID = new System.Collections.Generic.List<string>();
                    bool flag = false;
                    List_ID.Add("");
                    List_ID.Add("Continue");
                    List_ID.Add("Break");
                    foreach (System.Windows.Forms.TreeNode _node in Global_Memory.select_Node_Parent.Nodes)
                    {
                        if (((dynamic)Global_Memory.mySelectedNode.Tag).ID == ((dynamic)_node.Tag).ID)
                        {
                            flag = true;
                            continue;
                        }

                        if (flag && _node.Tag.GetType() != Global_Memory.ExHandleType)
                        {
                            List_ID.Add(((dynamic)_node.Tag).Description + "(" + ((dynamic)_node.Tag).ID + ")");
                        }
                    }

                    return new StandardValuesCollection(List_ID);
                }
                else
                {
                    return new StandardValuesCollection(new int[] { 0 });
                }
            }
            else
            {
                return new StandardValuesCollection(new int[] { 0 });
            }
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;

        }

    }


}
