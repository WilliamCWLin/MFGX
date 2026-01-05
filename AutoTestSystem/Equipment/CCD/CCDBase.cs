using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Equipment.CCD
{
    public abstract class CCDBase
    {
        public abstract void Dispose();

        public abstract bool Init(string strParamInfo);

        public abstract bool UnInit();
        public abstract bool Capture(string strSavePath);

        public abstract bool Start();

        public virtual bool Set_Exposure(int value) {

            return true;
        }

        public virtual bool Set_Gain(int value)
        {

            return true;
        }

        public virtual bool SaveImage(string strSavePath)
        {
            throw new NotImplementedException();
        }

        public virtual bool SaveImage( int Format_Mode ,string strSavePath)
        {
            throw new NotImplementedException();
        }


        public virtual bool ReadParamFile(string strSavePath)
        {
            throw new NotImplementedException();
        }


    }
}
