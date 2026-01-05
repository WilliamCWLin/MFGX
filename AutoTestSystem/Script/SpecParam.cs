using Manufacture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Script
{
    public class SpecParam
    {
        public string Name { get; set; }

        public string NameA { get; set; }

        public string NameB { get; set; }
        public SpecType SpecType { get; set; }
        public string SpecValue { get; set; }
        public string Mes { get; set; }
        public string Csv { get; set; }
        public double MaxLimit { get; set; }
        public double MinLimit { get; set; }
    }
    public class SpecParamsContainer
    {
        public List<SpecParam> specParams { get; set; }
    }
    public class ConditionList
    {
        public List<Condition> Conditions { get; set; }
    }

    public class Condition
    {
        public string Name { get; set; }

        public string NameA { get; set; }

        public string NameB { get; set; }
        public ConditionType SpecType { get; set; }
        public string SpecValue { get; set; }
        public string Goto { get; set; }
        public string MaxLimit { get; set; }
        public string MinLimit { get; set; }
    }

    public enum SpecType
    {
        Bypass,  //不卡規格
        Range,  // 上下限
        Equal,   // 字符相等
        GreaterThan,    // 大于
        LessThan,        // 小于
        NotEqual,
        Contain,
        Regex
    }

    public enum ConditionType
    {
        Bypass,  
        Range,  
        Equal,  
        GreaterThan,   
        LessThan,
        Contains
    }
}
