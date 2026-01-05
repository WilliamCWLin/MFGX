using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using AutoTestSystem.Model;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class Torque : ControlDeviceBase
    {
        Comport DeviceComport = null;
        bool m_initial = false;

        [Browsable(false)]
        public object ComportDev { get; private set; }

        [Category("Params"), Description("Select Comport"), TypeConverter(typeof(ComportList))]
        public string PortName { get; set; }

        [Category("Params"), Description("Set BaudRate")]
        public int BaudRate { get; set; }

        public Torque()
        {
            BaudRate = 9600;
        }
        public override bool Init(string strParamInfo)
        {
            if(PortName != null)
            { 
                SerialConnetInfo DevieCOMinfo = new SerialConnetInfo { PortName = PortName, BaudRate = BaudRate };
                DeviceComport = new Comport(DevieCOMinfo);

                if (!DeviceComport.OpenCOM_CHK())
                    return false;

                m_initial = true;
            }
            else
            {
                LogMessage($"Init Fail.",MessageLevel.Error);
                return false;
            }

            return true;

        }

        public override void OPEN()
        {
            DeviceComport.OpenCOM();
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
            if (!m_initial)
                return false;
            DeviceComport.WriteLine(input);
            //MessageBox.Show("Arduino寫入完成:" + input);
            return true;
        }

        public override bool READ(ref string output)
        {
            if (!m_initial)
                return false;
            output =DeviceComport.Read();
            //MessageBox.Show("Arduino讀取完成:" + output);

            if (string.IsNullOrEmpty(output))
                return false;
            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
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
                    return new StandardValuesCollection(new int[] {  });
                }
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }
    }
}
