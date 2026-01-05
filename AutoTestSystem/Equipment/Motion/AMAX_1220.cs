using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AMONetLib;
using Automation.BDaq;
using AutoTestSystem.Base;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Equipment.Motion
{
    internal class  AMAX_1220: MotionBase
    {
        private short m_CardNum = 0;
        private ushort status = 1;
        private string _strParamInfoPath;
        List<string> Ring_Status_List = new List<string>(); // 定义一个动态列表
        List<string> IO_Status_List = new List<string>(); // 定义一个动态列表


        [Category("Parameter"), Description("Config Path"), Editor(typeof(Manufacture.FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string Config_path
        {
            get { return _strParamInfoPath; }
            set { _strParamInfoPath = value; }
        }

        [Category("Params"), Description("Axis Number")]
        [ReadOnly(true)]
        public ushort m_AxisNo { get; set; }



        [Category("Params"), Description("DeviceIP")]
        [ReadOnly(true)]
        public ushort m_DeviceIP { get; set; }


        [Category("Params"), Description("Ring Number")] 
        [ReadOnly(true)]
        public ushort m_RingNo { get; set; }

        [Category("Params"), Description("ALM Logic")]
        [ReadOnly(true)]
        public string nAlmLogic { get; set; }    


        [Category("Params"), Description("SV Status")]
        [ReadOnly(true)]
        public string m_bSvon { get; set; }

        public AMAX_1220()
        {
            m_RingNo = 0;
            m_DeviceIP = 0;
            m_AxisNo = 0;          
        }



        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            short nRet = 0;
            string Config_path_absolutePath;                                 

            try
             {

               m_CardNum = AMONet._mnet_initial(); //If PCI1202, you also can use _1202_open(&m_CardNum) instead.

                  if (m_CardNum == 0)
                      {
                       
                       LogMessage("No AMONet device has found.", MessageLevel.Info);
                               return false;
                      }                    

                      nRet = AMONet._mnet_set_ring_config(m_RingNo, 3);
                      if (!ErrorHandler(nRet))
                          return false;

                      nRet = AMONet._mnet_reset_ring(m_RingNo);
                       if (!ErrorHandler(nRet))
                          return false;

                      nRet = AMONet._mnet_start_ring(m_RingNo);
                       if (!ErrorHandler(nRet))
                               return false;
             }
             catch (Exception e2)
             {
                      
                     LogMessage("Please Check Driver is exist or not??" + e2.Message, MessageLevel.Error);
                     return false;
             }
            try
            {
                nRet = AMONet._mnet_m2_initial(m_RingNo, m_DeviceIP);
                if (!ErrorHandler(nRet))
                {
                    LogMessage("mnet_Error!!!=>" + nRet, MessageLevel.Error);
                    return false;
                }
               
                nRet = AMONet._mnet_m2_set_feedback_src(m_RingNo, m_DeviceIP, m_AxisNo, 1);
                if (!ErrorHandler(nRet))
                    return false;

                nRet = AMONet._mnet_m2_set_tmove_speed(m_RingNo, m_DeviceIP, m_AxisNo, 10, 800, 0.3, 0.3);
                if (!ErrorHandler(nRet))
                    return false;

                nRet = AMONet._mnet_m2_reset_position(m_RingNo, m_DeviceIP, m_AxisNo);
                if (!ErrorHandler(nRet))
                    return false;

                nRet = AMONet._mnet_m2_reset_command(m_RingNo, m_DeviceIP, m_AxisNo);
                if (!ErrorHandler(nRet))
                    return false;               

                nRet = AMONet._mnet_set_ring_quality_param(m_RingNo, 50, 100);
                if (!ErrorHandler(nRet))
                    return false;
               
            }
            catch(Exception e2)
            {
                LogMessage(e2.Message, MessageLevel.Error);
                return false;
            }
            

            if (Config_path != null)
                Config_path_absolutePath = Path.Combine(Directory.GetCurrentDirectory(), Config_path);
            //Config_path_absolutePath = Config_path;
            else
                Config_path_absolutePath = "";
            /////////////////////////READ PARAM FILE//////////////////////
            if (!string.IsNullOrEmpty(Config_path_absolutePath))
            {
                if (ReadParamFile(Config_path_absolutePath))
                {
                    LogMessage("Read Config File success!", MessageLevel.Info);
                }
                else
                    LogMessage("Read Config File Fail", MessageLevel.Error);
            }
            /////////////////////////READ PARAM FILE////////////////////// 
            uint m_IO_status = 0;
            ushort Ring_Status = 0;
            string m_IO_status_binVal = string.Empty;
            string Ring_Status_binVal = string.Empty;
            string tmpArray = string.Empty;
            

            try
            {
                nRet = AMONet._mnet_get_ring_status(m_RingNo, ref Ring_Status);
                if (!ErrorHandler(nRet))
                {
                    LogMessage("get_ring_status Fail=>" + nRet, MessageLevel.Error);
                    return false;
                }

                Ring_Status_binVal = Convert.ToString((int)Ring_Status, 2).PadLeft(16, '0');

                /*Axis CMD EMPTY:Bit[0],Reserved:Bit[1],input change:Bit[2],IO Device Error:Bit[3],Axis Device Error:Bit[4],Master Setting Error:Bit[5],
                 Master Operating Error:Bit[6],Reserved:Bit[7],Reserved:Bit[8],New Axis CMD:Bit[9],New Axis Data:Bit[10],
                Reserved:Bit[11],IO cycle Busy:Bit[12],Soft reset:Bit[13],Axis cycle Busy:Bit[14],Reserved:Bit[15]*/

                string[] Ring_Status_Bit = Ring_Status_binVal.ToCharArray().Select(x => x.ToString()).Reverse().ToArray();
                
                for (int i = 0; i <= Ring_Status_Bit.Length - 1; i++)
                {
                    Ring_Status_List.Add(Ring_Status_Bit[i]);
                }

                nRet = AMONet._mnet_m2_get_io_status(m_RingNo, m_DeviceIP, m_AxisNo, ref m_IO_status);
                if (!ErrorHandler(nRet))
                {
                    LogMessage("get_io_status Fail=>" + nRet, MessageLevel.Error);
                    return false;
                }

                m_IO_status_binVal = Convert.ToString((int)m_IO_status, 2).PadLeft(16, '0');

                /*RDY:Bit[0],ALM:Bit[1],+EL:Bit[2],-EL:Bit[3],ORG:Bit[4],DIR:Bit[5],
                 EMG:Bit[6],PCS:Bit[7],ERC:Bit[8],EZ:Bit[9],CLR:Bit[10],
                Latch:Bit[11],SD:Bit[12],INP:Bit[13],SVON:Bit[14],RALM:Bit[15]*/
                string[] IO_Status_Bit = m_IO_status_binVal.ToCharArray().Select(x => x.ToString()).Reverse().ToArray();

                for (int i = 0; i <= IO_Status_Bit.Length - 1; i++)
                {
                    IO_Status_List.Add(IO_Status_Bit[i]);                   
                }

                /*ALM_status = Convert.ToInt32(IO_Status_Bit[13], 2);
                INP_status = ((Convert.ToInt32(binVal, 2) >> inp_pos) & 0x001);*/

                nAlmLogic =IO_Status_List[1];
                m_bSvon =IO_Status_List[14];
                LogMessage("AlmLogic is " + nAlmLogic + "\r\n", MessageLevel.Warn);
                LogMessage("Svon is " + m_bSvon + "\r\n", MessageLevel.Warn);              



                if (IO_Status_List[1] == "1")
                {
                    LogMessage("ALM_status is enable => 1"+"\r\n", MessageLevel.Warn);                  
                }
                if(IO_Status_List[13] == "0")
                {
                    LogMessage("INP_status is disable => 0" + "\r\n", MessageLevel.Warn);
                }
                if (IO_Status_List[4] == "1")
                {
                    LogMessage("ORG_status is enable => 1" + "\r\n", MessageLevel.Warn);
                }
                if (IO_Status_List[14] == "1")
                {
                    LogMessage("SVON_status is enable => 1" + "\r\n", MessageLevel.Warn);
                }
                    
            }catch(Exception e2)
            {
                LogMessage("get_io_status ERROR=>" + e2.Message, MessageLevel.Error);
                return false;
            }

            return true;         

            }
        private bool ErrorHandler(short nRet)
        {
            if (nRet < 0)
            {
                MessageBox.Show("Operation failed with error code:" + nRet, "Operation_Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;

        }
        public override bool Pause()
        {
            throw new NotImplementedException();
        }

        public override bool EmgStop()
        {
            //throw new NotImplementedException();
            short rtn = 0;
            rtn = AMONet._mnet_m2_emg_stop(m_RingNo, m_DeviceIP, m_AxisNo);

            if (!ErrorHandler(rtn))
                return false;

            return true;
        }

        public override bool SdStop()
        {
            short rtn = 0;
            rtn = AMONet._mnet_m2_sd_stop(m_RingNo, m_DeviceIP, m_AxisNo);

            if (!ErrorHandler(rtn))
                return false;

            return true;

        }

        public override bool ServoOFF()
        {
            short nRet = 0;
            nRet = AMONet._mnet_m2_set_svon(m_RingNo, m_DeviceIP, m_AxisNo, 0);
            if (!ErrorHandler(nRet))
                return false;

            return true;
        }

        public override bool ServoON()
        {
            short nRet = 0;

            nRet = AMONet._mnet_m2_set_svon(m_RingNo, m_DeviceIP, m_AxisNo, 1);
                if (!ErrorHandler(nRet))
                    return false;

            return true;       
        }
        public override bool Relative_Move(double value, double in_start_vel, double in_max_vel, double tacc, double dac)
        {
            
            short nRet = 0;
            int INT_move_value = Convert.ToInt32(value);
            nRet = AMONet._mnet_m2_set_tmove_speed(m_RingNo, m_DeviceIP, m_AxisNo, in_start_vel, in_max_vel, tacc, dac);//10 500 0.3 0.3
            if (!ErrorHandler(nRet))
                return false;

            nRet = AMONet._mnet_m2_start_r_move(m_RingNo, m_DeviceIP, m_AxisNo, INT_move_value);
            if (!ErrorHandler(nRet))
            {
                MessageBox.Show("Relative_Move failed with error code:" + nRet, "Relative_Move", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
                
            do
            {
                nRet = AMONet._mnet_m2_motion_done(m_RingNo, m_DeviceIP, m_AxisNo, ref status);

            } while (status != 0);

            return true;
        }

        public override bool Absolute_Move(double value, double in_start_vel, double in_max_vel, double tacc, double dac)
        {
            throw new NotImplementedException();
        }
        public override bool GetCommandPos(ref double out_pos)
        {
            //throw new NotImplementedException();
            short nRet = 0;
            int pos_value = 0;
            nRet = AMONet._mnet_m2_get_position(m_RingNo, m_DeviceIP, m_AxisNo, ref pos_value);
            if (!ErrorHandler(nRet))
                return false;

            out_pos = pos_value;

            return true;
        }

        public override bool SetCommandPos(double in_pos)
        {
            //throw new NotImplementedException();
            short nRet = 0;
            int pos_value = (int)Math.Round(in_pos);
            
            nRet = AMONet._mnet_m2_set_position(m_RingNo, m_DeviceIP, m_AxisNo, pos_value);
            if (!ErrorHandler(nRet))
                return false;

            return true;
        }

        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            throw new NotImplementedException();
        }

        public override bool SyncHome(double in_start_vel, double in_max_vel,int Dir,int Home_Timeout)
        {
            bool TimeOut = false;
            short rtn = 0;
            Stopwatch stopwatch = new Stopwatch();

            rtn = AMONet._mnet_m2_set_tmove_speed(m_RingNo, m_DeviceIP, m_AxisNo, in_start_vel, in_max_vel, 0.5, 0.5);
            // Set Home configuration
            rtn = AMONet._mnet_m2_set_home_config(m_RingNo, m_DeviceIP, m_AxisNo, 0, 0, 0, 0, 0);

            //// Start homing
            rtn = AMONet._mnet_m2_start_home_move(m_RingNo, m_DeviceIP, m_AxisNo, (byte)Dir);
            if (!ErrorHandler(rtn))
            {
                //show m_RingNo, m_DeviceIP, m_AxisNo 參數
                MessageBox.Show("m_RingNo:" + m_RingNo.ToString()+"\r\n"+ "m_DeviceIP:" + m_DeviceIP.ToString()+ "m_AxisNo:" + m_AxisNo.ToString(),"home_move",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return false;
            }

            stopwatch.Start();

            do
            {
                rtn = AMONet._mnet_m2_motion_done(m_RingNo, m_DeviceIP, m_AxisNo, ref status);
                if(stopwatch.ElapsedMilliseconds > Home_Timeout)
                {
                    EmgStop();
                    MessageBox.Show($"EmgStop => Home Time Out!!!");
                    TimeOut = true;
                    break;
                }

            } while (status != 0);

            if (TimeOut)
                return false;
            return true;
        }

        public override bool Check_IO_StartStatus(Dictionary<int, int> Devices_IO_Status)
        {
            //int status = 0;
            
            foreach (KeyValuePair<int, int> pair in Devices_IO_Status)
            {
                int key = pair.Key;
                int value = pair.Value;
                int port_status = 0;
                // 处理键/值对
                
                Get_IO_Status(ref port_status, (ushort)key);
                if (value != port_status)
                    return false;
                
            }

            return true;


        }

        public override bool SyncMotionDone()
        {
            throw new NotImplementedException();
        }

        public override bool SyncResetHomeDone()
        {
            throw new NotImplementedException();
        }
        public override bool UnInit()
        {
            //throw new NotImplementedException();
            try
            {
                AMONet._mnet_stop_ring(m_RingNo);
                AMONet._mnet_close();//If PCI1202, you also can use _1202_close(0) instead.
            }catch(Exception ex)
            {
                Console.WriteLine("UnInit Fail!!!Please check driver is exit or not" + ex.Message);
                return false;                
            }

            return true;
        }

        public override bool Get_IO_Status(ref int status, ushort port_num)
        {
            short nRet = 0;
            bool pass_fail_flag = true;

            try
            {
                nRet = AMONet._mnet_m2_dio_input(m_RingNo, m_DeviceIP, 0); //DI Input
            }catch(Exception ex)
            {
                Console.WriteLine("UnInit Fail!!!Please check driver is exit or not" + ex.Message);
                pass_fail_flag = false;

            }
            if (((nRet >> port_num) & 0x01) == 1)
            {
                status = 1; //DI ON
            }
            else
            {
                status = 0; //DI OFF

            }

                return pass_fail_flag;
        }

        public class AxisNo_List : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                string[] Axis_count = { "x_Axis", "y_Axis" };
                if (Axis_count.Length > 0)
                {
                    return new StandardValuesCollection(Axis_count.ToArray());
                }
                else
                {
                    return new StandardValuesCollection(new int[] {});
                }                               
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }
        }

        public class AlmLogic_List : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                string[] AlmLogic_List_count = { "Active High", "Active Low" };
                if (AlmLogic_List_count.Length > 0)
                {
                    return new StandardValuesCollection(AlmLogic_List_count.ToArray());
                }
                else
                {
                    return new StandardValuesCollection(new int[] {});
                }
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }
        }

        public class SV_Status_List : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                string[] AlmLogic_List_count = { "ON", "OFF" };
                if (AlmLogic_List_count.Length > 0)
                {
                    return new StandardValuesCollection(AlmLogic_List_count.ToArray());
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

        public override bool ReadParamFile(string strSavePath)
        {

            short nRet = 0;
            try
            {
                if (strSavePath != string.Empty)
                {
                    nRet = AMONet._mnet_m2_loadconfig(m_RingNo, m_DeviceIP, strSavePath);
                    if (!ErrorHandler(nRet))
                    {
                        LogMessage("Open Parameter File Name Fail !!! =>"+ nRet, MessageLevel.Error);
                        return false;
                    }
                }
                else
                {
                    LogMessage("NO parameter file Name !!!", MessageLevel.Error);
                    return false;

                }
            }catch(Exception ex)
            {
                LogMessage("Open File Error !!! Please Check Driver is exist or not??" + ex.Message,MessageLevel.Error);
                return false;
            }

            return true;
        }

    }
}
