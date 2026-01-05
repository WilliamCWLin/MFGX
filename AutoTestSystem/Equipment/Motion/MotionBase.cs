using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Equipment.Motion
{
    public abstract class MotionBase : IDisposable
    {
        public abstract void Dispose();

        public abstract bool Init(string strParamInfo);

        public abstract bool StartAction(string strItemName, string strParamIn, ref string strOutput);
        public abstract bool UnInit();
        public abstract bool ServoON();
        public abstract bool ServoOFF();
        public abstract bool SetCommandPos(double in_pos);
        public abstract bool GetCommandPos(ref double out_pos);
        public abstract bool EmgStop();
        public abstract bool SdStop();
        public abstract bool Pause();
        public abstract bool SyncHome(double in_start_vel, double in_max_vel);
        public abstract bool SyncMotionDone();
        public abstract bool Relative_Move(int value);

        public virtual bool Get_IO_Status(ref int status , ushort port_num )
        {
            throw new NotImplementedException();
        }

        
    }
}
