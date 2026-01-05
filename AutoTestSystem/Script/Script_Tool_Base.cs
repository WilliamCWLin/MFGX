

using AutoTestSystem.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoTestSystem.Script
{
   public class Script_Tool_Base : ScriptBase,IDisposable
    {

        

        public virtual void Dispose()
        {

        }

        public virtual bool PreProcess(string ActionItem, string Paraminput)
        {
            return true;
        }
        public virtual bool Process(string keywordComment, string Type_mode)
        {
            return true;
        }
        public virtual bool PostProcess(string strCheckSpec, ref string strDataout)
        {

            return true;
        }        

    }
}
