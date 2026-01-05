using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Script
{
    public abstract class ScriptDUTMotionBase : ScriptBase, IDisposable
    {
        public abstract void Dispose();


        public abstract bool PreProcess(DUT_BASE DUTDevice, MotionBase MotionDev);
        public abstract bool Process(DUT_BASE DUTDevice, MotionBase MotionDev);
        public abstract bool PostProcess(DUT_BASE DUTDevice, MotionBase MotionDev);
    }
}
