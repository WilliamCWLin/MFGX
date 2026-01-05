using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using Newtonsoft.Json;
using NModbus;
using NModbus.Serial;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.DAL.Communication;
using static AutoTestSystem.Equipment.Motion.Motion_DIOparam;

namespace AutoTestSystem.Equipment.Motion
{
    class Motion_Simu : MotionBase
    {
        private bool g_isHome = false;
        private double startPosition = 0.0;
        private double position = 0.0;
        private double targetPosition = 0.0;
        private DateTime _moveStartTime;
        private int _moveDurationMs = 3000; // 5秒
        public Motion_Simu()
        {
            //baudRate = 115200;
        }

      

        public override bool Init(string strParamInfo)
        {
        

            return true;
        }

        public override bool Status(ref string msg)
        {

            return true;

        }

        public override bool MotionDone(ref int status)
        {
            return true;
        }
        public override string GetErrorMessage()
        {
            return ErrorMessage;
        }

        public override bool UnInit()
        {
            
            return true;
        }

        public override bool SdStop()
        {
            return true;
        }

        public override bool ServoOFF()
        {
            return true;
        }

        public override bool ServoON()
        {

            return true;
        }
        public override bool GetCommandPos(ref double out_pos)
        {
            out_pos = position;
            return true;
        }
        public override bool GetCerrentPos(ref double out_pos)
        {
            out_pos = position;
            return true;
        }
        public override bool GetCurrentPos(ref double out_pos)
        {
            double elapsed = (DateTime.Now - _moveStartTime).TotalMilliseconds;
            if (elapsed >= _moveDurationMs)
            {
                position = targetPosition;
            }
            else
            {
                position = startPosition + (targetPosition - startPosition) * (elapsed / _moveDurationMs);
            }
            out_pos = position;
            return true;
        }

        public override bool SetCommandPos(double in_pos)
        {

            return true;
        }

        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            return true;
        }
        public override bool SyncHome(double in_start_vel, double in_max_vel, int Dir, int Timeout)
        {
            _moveStartTime = DateTime.Now;
            targetPosition = 0;
            g_isHome = true;
            return true;
        }

        public override bool SyncHome()
        {
            _moveStartTime = DateTime.Now;
            targetPosition = 0;
            g_isHome = true;
            return true;
        }

        public override bool Check_IO_StartStatus(Dictionary<int, int> Devices_IO_Status)
        {
            return true;
        }

        public override bool SyncMotionDone()
        {
            return true;
        }
        public override bool SyncResetHomeDone()
        {
            return true;
        }
        public override void GetMotionStatus(ref int status)
        {
            if (!g_isHome)
            {
                status = -1;
                //return false;                               
            }
            else
            {
                // Bitposition=0 代表移動狀態
                //if (Bitposition == 0)
                //{
                if ((DateTime.Now - _moveStartTime).TotalMilliseconds < _moveDurationMs)
                    status = 1; // 移動中
                else
                    status = 0; // 完成
                                //return true;
                                //}
                                // 其他狀態可自行擴充
                //status = 0;
            }
            //return true;
        }
        public override void Dispose()
        {
            
        }

        public override bool EmgStop()
        {
            return true;
        }

        public override bool Pause()
        {
            return true;
        }

        //public override bool Relative_Move(double value, double in_start_vel, double in_max_vel, double tacc, double dac)
        //{
        //    position += value;
        //    return true;
        //}

        //public override bool Absolute_Move(double value, double in_start_vel, double in_max_vel, double tacc, double dac)
        //{
        //    position = value;
        //    return true;
        //}
        public override bool Relative_Move(double value, double in_start_vel, double in_max_vel, double tacc, double dac)
        {
            if (!g_isHome)
            {
                ErrorMessage = "Please Home First";
                return false;
            }
            _moveDurationMs = (int)(((Math.Abs(value) / in_max_vel) * 1000));
            _moveStartTime = DateTime.Now;
            startPosition = position;
            targetPosition = position + value;
            
            return true;
        }

        public override bool Absolute_Move(double value, double in_start_vel, double in_max_vel, double tacc, double dac)
        {
            if (!g_isHome)
            {
                ErrorMessage = "Please Home First";
                return false;
            }
            _moveDurationMs = (int)(((Math.Abs(value- position) / in_max_vel) * 1000));
            _moveStartTime = DateTime.Now;
            startPosition = position;
            targetPosition = value;
            return true;
        }
    }
}
