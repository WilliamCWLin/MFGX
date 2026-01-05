using AutoTestSystem.Base;
using AutoTestSystem.DUT;
//using AutoTestSystem.Equipment.VISA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Script
{
    public  class ScriptVisaBase : ScriptBase,IDisposable
    {
        public virtual void Dispose()
        {

        }

        public virtual bool PreProcess(string ActionItem, string Paraminput)
        {
            return true;
        }
        public virtual bool Process(VisaBase Device)
        {
            return true;
        }
        public virtual bool PostProcess(string strCheckSpec,ref string strDataout)
        {
            return true;
        }
    }
}
