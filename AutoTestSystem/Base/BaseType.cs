using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Base
{
    //add by william
    public partial class ContainerBase : Manufacture.ContainerNode
    {
        public ContainerBase()
        {
            Description = "ContainerBase";
        }
    }

    public class ToolFunction : Manufacture.TerminalNode
    {
        public ToolFunction()
        {
            ClassName = GetType().ToString();
        }
    }
}
