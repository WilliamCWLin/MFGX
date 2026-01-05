using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Equipment.DosBase
{
    public abstract class DosBase : IDisposable
    {
        public abstract void Dispose();
        public abstract bool Init(string strParamInfo);
        public abstract bool UnInit();
        public abstract bool Send(string input,string strActItem);
        public abstract void SetTimeout(int time);
        public abstract bool READ(ref string output);
        public abstract void SetCheckstr(string str);
    }
}
