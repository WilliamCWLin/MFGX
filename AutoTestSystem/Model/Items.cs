/*
 * "AutoTestSystem.Model --> ItemsNew"
 *
 * Corpright William & Zhibin
 *
 *.##.......####.########.########..#######..##....##
 *.##........##.....##....##.......##.....##.###...##
 *.##........##.....##....##.......##.....##.####..##
 *.##........##.....##....######...##.....##.##.##.##
 *.##........##.....##....##.......##.....##.##..####
 *.##........##.....##....##.......##.....##.##...###
 *.########.####....##....########..#######..##....##
 *
 *  1. <ItemsNew.cs> is just a structor class
 *
 * 
 */


using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Model
{
    [Serializable]
    public  class ItemsNew
    {
        public string ItemName      = null;     //当前测试子项名字
        public string TestKeyword   = null;     //测试步骤对应的关键字，执行对应关键字下的代码段
        public string ErrorCode     = null;     //测试错误码
        public string RetryTimes    = null;     //测试失败retry次数
        public string TimeOut       = null;     //测试步骤超时时间
        public string SubStr1       = null;     //截取字符串 如截取abc中的b SubStr1=a，SubStr2=c
        public string SubStr2       = null;
        public string IfElse        = null;     //测试步骤结果是否做为if条件，决定else步骤是否执行
        public string For           = null;     //循环测试for(6)开始6次循环，ENDFOR结束
        public string Mode          = null;     //机种，根据机种决定哪些用例不跑，哪些用例需要跑
        public string ComdSend      = null;     //发送的测试命令
        public string ExpectStr     = null;     //期待的提示符，用来判断反馈是不是结束了
        public string CheckStr1     = null;     //检查反馈是否包含CheckStr1
        public string CheckStr2     = null;     //检查反馈是否包含CheckStr2
        public string Spec          = null;     //测试定义的Spec值
        public string Limit_min     = null;     //最小限值
        public string Limit_max     = null;     //最大限值
        public string unit          = null;     //测试值单位
        public string Bypass        = null;     //手动人为控制测试结果 1=pass，0||空=fail
        public string DllPlugin     = null;     //DLL插件位置，默认在程式根目录下
        public string StriptType    = null;     //腳本基底
        public string DeviceName    = null;     //腳本用的硬體裝置
        public string SpecRule      = null;     //腳本用標準化規格參數
        public string Enable        = null;     //腳本用標準化規格參數
        public string Prefix        = null;     //儲存數據前綴


    }



}
