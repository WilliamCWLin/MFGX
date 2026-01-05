using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Equipment.ControlDevice
{
    public abstract class ControlDeviceBase : IDisposable
    {
        public abstract void Dispose();
        public abstract bool Init(string strParamInfo);
        public abstract bool UnInit();
        public abstract bool SEND(string input);
        public virtual bool READ(ref string output)
        {
            return true;
        }

        public virtual bool READ(string ParamIn, ref string output)
        {
            return true;
        }

        public virtual bool Send_Read(string input, ref string output)
        {
            return true;
        }


        public virtual void OPEN() 
        { 
      
        }

        public virtual void SetTimeout(int time)
        {

        }

    }
}
