using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Base
{
    public abstract class CCDBase : Manufacture.Equipment
    {
        public abstract void Dispose();

        public virtual bool Init(string strParamInfo)
        {
            return true;
        }

        public abstract bool Capture(string strSavePath);

        public abstract bool Start();

        public virtual bool Set_Exposure(int value)
        {

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

        public virtual bool SaveImage(int Format_Mode, string strSavePath)
        {
            throw new NotImplementedException();
        }


        public virtual bool ReadParamFile(string strSavePath)
        {
            throw new NotImplementedException();
        }

        public virtual bool Priview(ImageData img)
        {
            throw new NotImplementedException();
        }

        public struct ImageData
        {

           
            public  int Size;
            public  int Width;
            public  int Height;
            public  ImageType type;
            public  string ImgPath;

        }

        public enum ImageType
        {
            A=1,
            B,
            C

        }


    }
}
