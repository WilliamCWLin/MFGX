using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Equipment.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Script
{
    public  class Script_Container_Base : ScriptContainer, IDisposable
    {

        public virtual void Dispose()
        {

        }

        public virtual bool PreProcess(string ActionItem, string Paraminput)
        {
            return true;
        }
        public virtual bool Process()
        {
            return true;
        }
        public virtual bool PostProcess(string strCheckSpec,ref string strDataout)
        {
            return true;
        }
    }
}
