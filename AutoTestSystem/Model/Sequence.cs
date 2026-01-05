/*
 * "AutoTestSystem.Model --> Sequence"
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
 *  1. <Sequence.cs> is just a structor class
 *
 * 
 */




using System;
using System.Collections.Generic;

namespace AutoTestSystem.Model
{
    [Serializable]
    public class Sequence
    {
        public string SeqName = null;
        // public bool IsTest = true;                                 //! 是否测试
        // public bool IsTestFinished = false;                        //! 测试完成标志
        // public bool TestResult = true;                             //! 测试结果
        public int TotalNumber = 0;                                   //! 测试大项item总数量
        // public int TestSerial = 0;                                 //! 测试大项在所有中的序列号
        // public string TestVersion = null;                          //! 测试程序版本
        // public string SystemName = null;                           //! 测试系统名称 SystemName
        public List<Items> SeqItems = new List<Items>();
        // public string start_time = null;
        // public string finish_time = null;

        public void Clear()
        {
          //IsTest = true;
          //IsTestFinished = false;
          //TestResult = true;
          //start_time = null;
          //finish_time = null;
        }
    }

    [Serializable]
    public class Items
    {
        //public int testNumber     = 0;                              //! 当前测试项序列号
        //public bool tResult       = true;                           //! 测试项测试结果
        //public bool isTest        = true;                           //! 是否测试,不测试的跳过
        //public int startIndex     = 0;                              //! 需要执行的step index
        //public DateTime startTime = new DateTime();                 //! 测试项的开始时间
        //public DateTime EndTime   = new DateTime();                 //! 测试项的结束时间

        public string ItemName      = null;                           //! 当前测试step名字
        public string TestKeyword   = null;                           //! 测试步骤对应的关键字，执行对应关键字下的代码段
        public string ErrorCode     = null;                           //! 测试错误码
        public string RetryTimes    = null;                           //! 测试失败retry次数
        public string TimeOut       = null;                           //! 测试步骤超时时间
        public string SubStr1       = null;                           //! 截取字符串 如截取abc中的b SubStr1=a，SubStr2=c
        public string SubStr2       = null;
        public string IfElse        = null;                           //! 测试步骤结果是否做为if条件，决定else步骤是否执行
        public string For           = null;                           //! 循环测试for(6)开始6次循环，ENDFOR结束
        public string Mode          = null;                           //! 机种，根据机种决定哪些用例不跑，哪些用例需要跑
        public string ComdSend      = null;                           //! 发送的测试命令
        public string ExpectStr     = null;                           //! 期待的提示符，用来判断反馈是不是结束了
        public string CheckStr1     = null;                           //! 检查反馈是否包含CheckStr1
        public string CheckStr2     = null;                           //! 检查反馈是否包含CheckStr2
        public string Spec          = null;                           //! 测试定义的Spec值
        public string Limit_mix     = null;                           //! 最小限值
        //public string TestValue     = null;                         //! 测试得到的值
        public string Limit_max     = null;                           //! 最大限值
        //public string ElapsedTime   = null;                         //! 测试步骤耗时
        //public string ErrorDetails  = null;                         //! 测试错误码详细描述
        public string unit          = null;                           //! 测试值单位
        public string Bypass        = null;                           //! 手动人为控制测试结果 1=pass，0||空=fail
        public string MES_var       = null;                           //! 上传MES信息的变量名字
        //public string FTC           = null;                         //! 失败继续 fail to continue。1=继续，0||空=不继续
        //public string TestKeyword   = null;                         //! 测试步骤对应的关键字，执行对应关键字下的代码段         
        //public string Spec          = null;                         //! 测试定义的Spec值

        public void Clear()
        {
          //  tResult       = true;
          //  isTest        = true;
          //  startIndex    = 0;
          //  TestValue     = null;
          //  ElapsedTime   = null;
          //  startTime     = new DateTime();
          
        }
    }

    public class MesInfo
    {
        public string serial;
        public string Fix_Num;
        public string start_time;
        public string finish_time;
        public string test_time;
        public string status;
        public string error_code;
    }
}