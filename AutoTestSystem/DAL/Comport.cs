using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.DAL
{
    public class Comport : Communication
    {
        public SerialPort SerialPort;

        //public Comport(SerialConnetInfo serialConnetInfo, string _logPath)
        //{
        //    serialPort = new SerialPort
        //    {
        //        PortName = serialConnetInfo.PortName,
        //        BaudRate = serialConnetInfo.BaudRate,
        //        //DataBits = serialConnetInfo.DataBits,
        //        Parity = serialConnetInfo.Parity,
        //        StopBits = serialConnetInfo.StopBits,
        //        ReadTimeout = serialConnetInfo.ReadTimeout,
        //        WriteTimeout = serialConnetInfo.WriteTimeout,
        //        WriteBufferSize = serialConnetInfo.WriteBufferSize,
        //        ReadBufferSize = serialConnetInfo.ReadBufferSize
        //    };
        //    logPath = _logPath;
        //    //this.serialPort.RtsEnable = true;
        //    //this.serialPort.DtrEnable = true;
        //    //this.serialPort.WriteBufferSize = 0x2000;
        //    //this.serialPort.ReadBufferSize = 0x2000;
        //    //OpenCOM();
        //    //this.serialPort.ReadTimeout = 0x1388;
        //    //this.serialPort.WriteTimeout = 0x1388;
        //    //Thread.Sleep(2000);
        //}

        public Comport(SerialConnetInfo serialConnetInfo, string _logPath = "")
        {
            SerialPort = new SerialPort
            {
                PortName = serialConnetInfo.PortName,
                BaudRate = serialConnetInfo.BaudRate,
                DataBits = serialConnetInfo.DataBits,
                Parity = serialConnetInfo.Parity,
                StopBits = serialConnetInfo.StopBits,
                ReadTimeout = serialConnetInfo.ReadTimeout,
                WriteTimeout = serialConnetInfo.WriteTimeout,
                WriteBufferSize = serialConnetInfo.WriteBufferSize,
                ReadBufferSize = serialConnetInfo.ReadBufferSize
            };
            logPath = _logPath;
        }

        public void OpenCOM()
        {
            try
            {
                if (SerialPort.IsOpen)
                {
                    Close();
                }

                SerialPort.Open();
                Logger.Info($"{SerialPort.PortName} serialPort.OpenCOM()!!");
            }
            catch (Exception ex)
            {
                Logger.Fatal($"{ex.ToString()}");
                //throw;
            }
        }

        public override bool Open()
        {
            try
            {
                if (SerialPort.IsOpen)
                {
                    Close();
                }
                SerialPort.Open();
                SerialPort.DataReceived += ComPort_DataReceived;
                Logger.Info($"{SerialPort.PortName} serialPort.Open()!!");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal($"{ex.ToString()}");
                //throw;
                return false;
            }
        }

        private void ComPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //serialPort.ReceivedBytesThreshold = 50;
                string readStr = SerialPort.ReadExisting();
                sReceiveAll += readStr;
                if (!string.IsNullOrEmpty(readStr))
                {
                    Logger.Debug(readStr);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal($"{ex.ToString()}");
                //throw;
            }
        }

        public override void Close()
        {
            try
            {
                Logger.Info($"{SerialPort.PortName} serialPort.Close!!");
                SerialPort.Close();
                //serialPort.Dispose();
            }         
            catch (Exception ex)
            {
                Logger.Fatal($"{ex.ToString()}");
                //throw;
            }
        }

        /// <summary>
        /// 串口数据写入
        /// </summary>
        public override void Write(string data)
        {
            try
            {
                SerialPort.Write(data);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                //throw;
            }
        }

        public override void Write(byte[] data)
        {
            try
            {
                SerialPort.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                //throw ex;
            }
        }

        public override void WriteLine(string sendstr)
        {
            SerialPort.WriteLine(sendstr);
        }

        /// <summary>
        /// 串口读数据
        /// </summary>
        public byte[] ReceiveData()
        {
            int bytes = SerialPort.BytesToRead;
            byte[] comBuffer = new byte[bytes];
            try
            {
                Thread.Sleep(100);
                SerialPort.Read(comBuffer, 0, bytes);
            }
            catch (Exception ex)
            {
                Logger.Fatal($"{SerialPort.PortName} Read Exception!!!--> {ex.ToString()}");
            }

            return comBuffer;
        }

        public override bool SendCommand(string command, ref string strRecAll, string DataToWaitFor, int timeout = 10)
        {
            try
            {
                Sleep(10);
                long lngStart = DateTime.Now.AddSeconds(timeout).Ticks;
                strRecAll = "";
                if (!string.IsNullOrEmpty(command))
                {//如果不发命令则不发送换行
                    command = command + "\n";
                }
                Logger.Debug($"{SerialPort.PortName.ToUpper()}SendComd-->{command}");
                SerialPort.DiscardInBuffer();
                SerialPort.Write(command);
                while (sReceiveAll.ToLower().IndexOf(DataToWaitFor.ToLower()) == -1)
                {
                    var lngCurTime = DateTime.Now.Ticks;
                    if (lngCurTime > lngStart)
                    {
                        strRecAll = sReceiveAll;
                        sReceiveAll = "";
                        Logger.Error(strRecAll);
                        Logger.Error($"Waiting for:\"{DataToWaitFor}\" TimeOut({timeout}),FAIL!!!");
                        return false;
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
                strRecAll = sReceiveAll;
                sReceiveAll = "";
                Logger.Info(strRecAll);
                Logger.Info($"Waiting for:\"{DataToWaitFor}\" succeed!!");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                return false;
                //throw;
            }
        }

        public bool SendCommandToFix(string command, ref string strRecAll, string DataToWaitFor, int timeout = 10)
        {
            strRecAll = "";
            try
            {
                Sleep(10);
                long lngStart = DateTime.Now.AddSeconds(timeout).Ticks;
                strRecAll = "";
                Logger.Debug($"{SerialPort.PortName.ToUpper()}SendComdToFix-->{command}");
                //command = command + "\r\n"; //治具不用加回车换行
                SerialPort.DiscardInBuffer();
                SerialPort.Write(command);
                while (sReceiveAll.ToLower().IndexOf(DataToWaitFor.ToLower()) == -1)
                {
                    sReceiveAll += SerialPort.ReadExisting();
                    var lngCurTime = DateTime.Now.Ticks;
                    if (lngCurTime > lngStart)
                    {
                        strRecAll = sReceiveAll;
                        sReceiveAll = "";
                        Logger.Error(strRecAll);
                        Logger.Error($"Waiting for:{DataToWaitFor} TimeOut({timeout}),FAIL!!!");
                        return false;
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
                strRecAll = sReceiveAll;
                sReceiveAll = "";
                Logger.Info(strRecAll);
                Logger.Info($"Waiting for:{DataToWaitFor} succeed!!");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                return false;
                //throw;
            }
        }

        public override void Dispose()
        {
            //Logger.Info($"serialPort.Dispose!!");
            ((IDisposable)SerialPort).Dispose();
        }

        public override string Read()
        {
            string readExisting = null;
            try
            {
                readExisting = SerialPort.ReadExisting();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                //throw;
            }
            finally
            {

                if (!string.IsNullOrEmpty(readExisting))
                {
                    Logger.Debug(readExisting);
                }
            }
            return readExisting;
        }

        ////MTE Peter add New Function//////
        public bool OpenCOM_CHK()
        {
            try
            {
                if (SerialPort.IsOpen)
                {
                    Close();
                }

                SerialPort.Open();
                Logger.Info($"{SerialPort.PortName} serialPort.OpenCOM()!!");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn($"{ex.Message}");
                Logger.Warn("can not find ComPort");
                return false;
                //throw;
            }
        }

        public  string ReadData()
        {
            string readExisting = null;
            try
            {
                readExisting = SerialPort.ReadLine();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
            finally
            {

                if (!string.IsNullOrEmpty(readExisting))
                {
                    Logger.Debug(readExisting);
                }
            }
            return readExisting;
        }

        public string ReadBytes(byte[] buffer)
        {

            try
            {
                SerialPort.Read(buffer, 0, buffer.Length);
                Console.WriteLine("Data received: " + BitConverter.ToString(buffer));
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }

            return BitConverter.ToString(buffer);
        }

        public  void SetReadTimeout(int time)
        {

            SerialPort.ReadTimeout = time;
        }

        public string ReadUARTData()
        {
            string readExisting = null;
            try
            {
                readExisting = SerialPort.ReadLine();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                throw;
            }
            finally
            {

                if (!string.IsNullOrEmpty(readExisting))
                {
                    Logger.Debug(readExisting);
                }
            }
            return readExisting;
        }
        ////MTE Peter add New Function//////
        ///
        public bool cleanBuffer()
        {
            try
            {
                SerialPort.DiscardOutBuffer();
                SerialPort.DiscardInBuffer();
 //               Logger.Info("Clean Buffer!!");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn($"{ex.Message}");
                Logger.Warn("Clean Buffer false");
                return false;
                //throw;
            }
            
        }
    }

   

    public class SerialConnetInfo
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; private set; } = 8;
        public StopBits StopBits { get; set; } = StopBits.One;
        public Parity Parity { get; set; } = Parity.None;
        public int WriteTimeout { get; set; } = 0x1388;
        public int ReadTimeout { get; set; } = 0x1388;
        public int WriteBufferSize { get; set; } = 0x400;
        public int ReadBufferSize { get; set; } = 0x400;
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