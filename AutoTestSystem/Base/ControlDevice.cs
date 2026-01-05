using Manufacture;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoTestSystem.Base.DUT_BASE;

namespace AutoTestSystem.Base
{
    public abstract class ControlDeviceBase : Manufacture.Equipment, IDisposable
    {
        [Category("Base"), Description("Config Common parameter"), Editor(typeof(TableEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string CommandTable { get; set; }
        public abstract void Dispose();
        public abstract bool Init(string strParamInfo);
        public abstract bool SEND(string input);
        public virtual bool SEND(byte[] input)
        {
            return true;
        }
        public virtual bool Send(string input, string strActItem)
        {
            return true;
        }
        public virtual bool READ(ref string output)
        {
            return true;
        }
        public virtual bool READ(ref string output, int timeout)
        {
            return true;
        }

        public virtual bool READ(ref byte[] packet, int timeout)
        {
            return true;
        }
        public virtual void SetTimeout(int timeout_comport, int timeout_total)
        { }
        public virtual bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            return true;
        }
        public virtual bool ModbusSend(byte slaveID, ushort Address, ushort writeData)
        {
            return true;
        }

        public virtual bool ModbusRead(byte slaveID, ushort Address, ushort numRegisters, ref ushort[] DataRecAll)
        {
            return true;
        }

        public virtual void SetCheckstr(string str)
        {

        }
        public virtual bool READ(string ParamIn, ref string output)
        {
            return true;
        }

        public virtual bool Send_Read(string input, ref string output)
        {
            return true;
        }

        public virtual bool CommandCheckForScript(string ReadCmd, string Checkstring)
        {
            return true;
        }
        public virtual bool CheckParam()
        {
            return true;
        }

        public virtual void OPEN()
        {

        }
        public override void LogMessage(string message, MessageLevel mode = MessageLevel.Info)
        {
            switch (mode)
            {
                case MessageLevel.Debug:
                    MLogger.Value?.Debug($"[{Description}]  {message}");
                    break;
                case MessageLevel.Info:
                    MLogger.Value?.Info($"[{Description}] {message}");
                    break;
                case MessageLevel.Warn:
                    MLogger.Value?.Warn($"[{Description}]  {message}");
                    break;
                case MessageLevel.Error:
                    MLogger.Value?.Error($"[{Description}]  {message}");
                    break;
                case MessageLevel.Fatal:
                    MLogger.Value?.Fatal($"[{Description}]  {message}");
                    break;
                case MessageLevel.Raw:
                    MLogger.Value?.Raw($"{message}");
                    break;
            }

        }
        public virtual void SetModBusTimeout(int writetime, int readtime)
        {

        }
        public virtual void SetTimeout(int time)
        {

        }

        public virtual void RetryTimes(int time)
        {

        }
        public virtual bool Clear()
        {
            return true;
        }
        public virtual void ClearBuffer()
        {

        }
        public virtual void SetMEStcmd(string str)
        {

        }
        public virtual void SetMEStdata(string str)
        {

        }

        public virtual bool PerformAction(string strItemName, string strOutputPath, bool mode)
        {
            return true;
        }

        public virtual bool SendNonblock(string input, ref string output)
        {
            return true;
        }

        public virtual bool MultiSend_Read(string input, string strActItem, string checkStr, int waitTime, ref string strOutData)
        {
            return true;
        }

        public virtual bool MESDataprocess(string MEStCMD, string MEStData, string checkStr, ref string strOutData)
        {
            return true;
        }
    }
}
