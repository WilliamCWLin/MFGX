using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Equipment.IO
{
    public abstract class IOBase : IDisposable
    {
        public abstract void Dispose();

        public abstract bool Init(string strParamInfo);

        public abstract bool UnInit();
        public abstract bool SETIO(int bit,bool output);
        public abstract bool GETIO(int pos, ref bool status);

        public abstract bool InstantAI(string strSavePath, ref string Dataout);

    }
    public class BindDataMessage
    {
        public string ch { get; set; }

    }
}
