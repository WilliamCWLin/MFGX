using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AutoTestSystem.Model;
using System.Windows.Forms;

namespace AutoTestSystem.DAL
{
    class AsynchronousSocket : Communication
    {
        private Socket sClient;
        IPAddress ipAdd;
        // ManualResetEvent instances signal completion.
        ManualResetEvent connectDone = new ManualResetEvent(false);
        ManualResetEvent ResetDone = new ManualResetEvent(false);
        IPEndPoint remoteEP;
        int port;
        Thread connectThread;
        public AsynchronousSocket(string ip, int port)
        {
            if (!IPAddress.TryParse(ip, out ipAdd))
            {
                ipAdd = IPAddress.Parse("127.0.0.1");
            }
            this.port = port;
          
        }

        public override void Open()
        {
            connectThread = new Thread(StartClient);
            connectThread.Start();
        }
        public override void Open_MHS()
        { 
        
        }

        private void StartClient()
        {
            while (true)
            {
                try
                {
                    Global.SaveLog("Start" + "\r\n");
                    ResetDone.Reset();
                    connectDone.Reset();
                    sClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Global.SaveLog("new Socket" + "\r\n");
                    remoteEP = new IPEndPoint(ipAdd, port);
                    Global.SaveLog("new IPEndPoint:"+ ipAdd+" "+ port + "\r\n");
                    //sClient.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), sClient);
                    sClient.Connect(remoteEP);
                    Global.SaveLog("Start Connect success" + "\r\n");
                    bConnect = true;
                    Receive(sClient);
                    //connectDone.WaitOne();
                    //bConnect = true;
                    
                    ResetDone.WaitOne();
                }
                catch (Exception ex)
                {
                    bConnect = false;
                    Global.SaveLog(ex.ToString());
                    Global.SaveLog(ex.Message + "\r\n");
                    Thread.Sleep(1000);
                }
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket tClient = (Socket)ar.AsyncState;
                tClient.EndConnect(ar);
                bConnect = true;
              
                Global.SaveLog("Connect success" + "\r\n");
                Receive(sClient);
                //connectDone.Set();
            }
            catch (Exception e)
            {
                Global.SaveLog("Connect Exception:" + e.Message + "\r\n");
                bConnect = false;
                ResetDone.Set();
                Global.SaveLog(e.ToString());
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = client;
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Global.SaveLog(e.Message + "\r\n");
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket tClient = state.workSocket;
                // Read data from the remote device.
                int bytesRead = tClient.EndReceive(ar);
                if (bytesRead > 0)
                {
                    String result = "";
                    for (int i = 0; i < state.buffer.Length; i++)
                    {
                        result = result+state.buffer[i].ToString() + ",";                       
                    }
                    //MessageBox.Show("ReceiveCallback:" + result);   
                    GetOutPut(state.buffer, bytesRead);
                    
                    WriteByte(state.buffer, bytesRead);
                    tClient.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                      new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    bConnect = false;
                    throw new Exception("Socket Closed");
                    //ResetDone.Set();
                }
                
            }
            catch (Exception e)
            {
                bConnect = false;
                sClient.Close();
                ResetDone.Set();
                Global.SaveLog(e.Message + "\r\n");
            }
        }

        public override void Write(byte[] data)
        {
            try
            {
                //Global.OutputValue = int.MinValue;
                //Global.OutputValueF = float.NaN;
                sClient.BeginSend(data, 0, data.Length, 0,
                     new AsyncCallback(SendCallback), sClient);
            }
            catch (Exception ex)
            {
                Global.SaveLog(ex.Message);
                Global.OutputEvent.Set();
            }
        }

        public override void Write(String str)
        {
            if (bConnect == true)
                Send(sClient, str);
        }
        public override void WriteLine(String str)
        { 
        }

        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
            }
            catch
            {
                bConnect = false;
                sClient.Close();
                ResetDone.Set();
            }
        }
        public override void Close()
        {
            try
            {
                if (connectThread != null)
                    connectThread.Abort();
                sClient.Dispose();
            }
            catch(Exception ex)
            {
                Global.SaveLog(ex.Message);
            }
        }

        new public void read()
        {
            Global.SaveLog("Socket Read");
        }

        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 256;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }
    }
}
