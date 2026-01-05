using AutoTestSystem.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Equipment.CCD
{
    class Basler : CCDBase
    {
        public override bool Capture(string strSavePath)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            //string xx = GetMoreProp("UseDAQ");
            //LogMessage("UseDAQ is "+xx);
            return false;
        }

        public override bool Start()
        {
            throw new NotImplementedException();
        }

        public override bool UnInit()
        {
            throw new NotImplementedException();
        }
    }
}
