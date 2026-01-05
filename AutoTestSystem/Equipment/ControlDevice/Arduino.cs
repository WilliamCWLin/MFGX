using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class Arduino: ControlDeviceBase
    {

        [Category("Params"), Description("Select Comport"), TypeConverter(typeof(ComportList))]
        public string PortName { get; set; }

        [Category("Params"), Description("Set BaudRate")]
        public int baudrate { get; set; }

        Comport DeviceComport = null;
       
        
        public Arduino()
        {
            baudrate = 9600;
        }



        public override bool Init(string strParamInfo)
        {

            if (string.IsNullOrEmpty(PortName))
            {
                LogMessage("NO COM Port Name", MessageLevel.Error);
                //MessageBox.Show("NO COM Port Name", "Warning!!!");
                return false;
            }
            SerialConnetInfo DevieCOMinfo = new SerialConnetInfo { PortName = PortName, BaudRate = baudrate };
            DeviceComport = new Comport(DevieCOMinfo);

            if (!DeviceComport.OpenCOM_CHK())
            {
                LogMessage("Init COM Port Fail", MessageLevel.Info);
                return false;
            }
            else
                LogMessage("Init COM Port Pass", MessageLevel.Info);
           
          

            return true;

        }

        public override void OPEN()
        {
            DeviceComport.OpenCOM();
        }

        public override bool Status(ref string msg)
        {
            try
            {
                if (DeviceComport.SerialPort.IsOpen)
                {
                    msg = $"{DeviceComport.SerialPort.PortName}(OPEN)";
                    return true;
                }
                else
                {
                    msg = $"{DeviceComport.SerialPort.PortName}(CLOSE)";
                    return false;
                }
            }
            catch(Exception ex)
            {
                msg = $"{ex.Message}";
                return false;
            }

        }


public override bool UnInit()
        {
            if (DeviceComport == null)
                return false;
            DeviceComport.Close();

            return true;
        }

        public override bool SEND(string input)
        {

            DeviceComport.WriteLine(input);
            //MessageBox.Show("Arduino寫入完成:" + input);
            return true;
        }

        public override bool READ(ref string output)
        {
            
            output =DeviceComport.ReadData();
            //MessageBox.Show("Arduino讀取完成:" + output);
            if(string.IsNullOrEmpty(output))
                return false;
            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
        public override void ClearBuffer()
        {
            DeviceComport.cleanBuffer();
        }

        public override void SetTimeout(int time)
        {
            DeviceComport.SetReadTimeout(time);
        }

        public class ComportList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                string[] portNames = SerialPort.GetPortNames();
                if (portNames.Length > 0)
                {
                    return new StandardValuesCollection(portNames.ToArray());
                }
                else
                {
                    return new StandardValuesCollection(new int[] { });
                }
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }


    }
}
