using System;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;

namespace AMONetLib
{
    public class AMONet
    {
        public AMONet()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	/************************************************************************
	   Define Global Value
	************************************************************************/		
        public const short ERR_NoError                       =0;
        public const short ERR_EventError                   =-1;
        public const short ERR_LinkError                    =-2;
        public const short ERR_MNET_Ring_Used               =-3;
        public const short ERR_Invalid_Ring                 =-4;
        public const short ERR_Invalid_Slave                =-5;
        public const short ERR_Invalid_Hardware             =-6;
        public const short ERR_Value_Out_Range              =-8;
        public const short ERR_Invalid_Setting              =-9;
        
        public const short ERR_Axis_Communication          =-11;  
        public const short ERR_Axis_command                =-12;
        public const short ERR_Axis_Receive                =-13;
        public const short ERR_Invalid_Operating_Velocity  =-14;
        public const short ERR_PosOutOfRange               =-15;
        public const short ERR_Invalid_MaxVel              =-16;
        public const short ERR_Speed_Change                =-17;
        public const short ERR_SlowDown_Point              =-18;
        public const short ERR_Invalid_DIO                 =-19;
        public const short ERR_Invalid_Comparator          =-20;
        public const short ERR_Comparator_Config           =-21;
        public const short ERR_CompareSourceError          =-22;
        public const short ERR_CompareActionError          =-23;
        public const short ERR_CompareMethodError          =-24;
        public const short ERR_ComparatorRead              =-25;
        public const short ERR_LimitOutOfRange             =-26;
        //Added by W.Y.Z on 2012.08.15
        public const short ERR_Invalid_DIO_Channel         = -27;

        public const short ERR_Latch_Config                =-30;
        public const short ERR_LatchError                  =-31;
        public const short ERR_LatchRead                   =-32;
        public const short ERR_HomeConfig                  =-35;
        /////////////////////G94 BUS ERROR///////////////////
        public const short ERR_G94_RECEIVE_TimeOut         =-36;
        public const short ERR_G94_CPURead                 =-37;

        /////////////////////M4 ERROR///////////////////
        public const short ERR_M4_CPLDRead                 =-46;
        public const short ERR_M4_RegisterRead             =-47;
        public const short ERR_M4_CPLDWrite                =-48;
        public const short ERR_M4_RegisterWrite            =-49;
        public const short ERR_M4_InvalidAxisNo            =-50;
        public const short ERR_M4_MOFStatusErr             =-51;
        public const short ERR_M4_InvalidAxisSelect        =-52;
        public const short ERR_M4_MPGmode		           =-53;
        public const short ERR_M4_InvalidMpgEnable	       =-54;
        public const short ERR_M4_MOFConfigmode		       =-55;
        public const short ERR_M4_SpeedError    		   =-56;
        public const short ERR_M4_AxisArrayError		   =-57;

        public const short ERR_Invalid_DeviceNumber        =-58;
        public const short ERR_LoadDriver_Failed           =-59;
        public const short ERR_Resource_Failed             =-60;
        public const short ERR_Invalid_InputPulseMode      =-61;
        public const short ERR_Invalid_Logic               =-62;
        public const short ERR_Invalid_OutputPulseMode     =-63;
        public const short ERR_Invalid_FeedbackSource      =-64;
        public const short ERR_Invalid_ALMMode             =-65;
        public const short ERR_Invalid_ERCActiveTime       =-66;
        public const short ERR_Invalid_ERCUnactiveTime     =-67;
        public const short ERR_Invalid_SDMode              =-68;
        public const short ERR_Invalid_HomeMode            =-69;
        public const short ERR_Invalid_EZCount             =-70;
        public const short ERR_Invalid_ERCOperation        =-71;
        public const short ERR_Invalid_LatchNumber         =-72;
        public const short ERR_Device_NotOpened            =-73;
        public const short ERR_Watchdog_Started            =-74;
        public const short ERR_ConfigFileOpenError         =-75;
        public const short ERR_Invalid_Position_Change     =-76;
        public const short ERR_Invalid_StartVel            =-77;
        public const short ERR_Invalid_AccTime             =-78;
        public const short ERR_Invalid_DecTime             =-79;
        public const short ERR_Invalid_Ratio               =-80;
        public const short ERR_Invalid_ELMode              =-81;
        public const short ERR_Invalid_AccRange            =-82;
        public const short ERR_Invalid_DecRange            =-83;
        public const short ERR_Invalid_Memory              =-84;
        public const short ERR_Invalid_DIOValue            =-85;
        public const short ERR_Invalid_ORGLogic            =-86;
        public const short ERR_Invalid_EZLogic             =-87;
        public const short ERR_Invalid_LatchSetting        =-88;
        public const short ERR_Invalid_RelPosition         =-89;
        public const short ERR_Invalid_Baudrate            =-90;
        public const short ERR_No_Device_Initialized   	   =-91;
        public const short ERR_DeviceBusy					=-92;
        public const short ERR_Invalid_Table_Size			=-93;
        public const short ERR_Invalid_Compare_Pulse_Mode	=-94;
        public const short ERR_Invalid_Compare_Pulse_Width	=-95;
        public const short ERR_Invalid_Compare_Pulse_Logic	=-96;
        public const short ERR_Function_NotSupport         =-97;
        public const short ERR_Invalid_ORGOffset            = -98;
        public const short ERR_Invalid_FwMemoryMode        =-99;
        public const short ERR_AXIS_BUSY                   = -100;
        public const short ERR_M4_ErrCntBeyond             = -101;

        /////////////////////AMAX-2240 ERROR///////////////////
        public const short ERR_AMAX2240					=-200;
        public const short ERR_Invalid_Center_Position		=ERR_AMAX2240 - 1;
        public const short ERR_Invalid_End_Position		=ERR_AMAX2240 - 2;
        public const short ERR_Invalid_Path_Cmd_Function	=ERR_AMAX2240 - 3;
        public const short ERR_Invalid_Compare_Start_Data	=ERR_AMAX2240 - 4;
        public const short ERR_Invalid_Compare_Interval_Data	=ERR_AMAX2240 - 5;
        //Added by W.Y.Z on 2012.08.15
        public const short ERR_Invalid_RepeatAxis = ERR_AMAX2240 - 6;
        public const short ERR_Invalid_ZeroDistance = ERR_AMAX2240 - 7;
        public const short ERR_Invalid_PrivateID = ERR_AMAX2240 - 8;

        /////////////////////AMAX-2240 FIRMWARE ERROR///////////////////
        public const short ERR_AMAX2240_FIRM				=-250;
        // Command execution error
        public const short ERR_CommandCodeError		=ERR_AMAX2240_FIRM - 1;
        public const short ERR_CommandCountExceed		=ERR_AMAX2240_FIRM - 2;
        public const short ERR_CommandAddedCountError  =ERR_AMAX2240_FIRM - 3;
        public const short ERR_TerminalSymbolError	    =ERR_AMAX2240_FIRM - 4;
        public const short ERR_CmpoutBufferIsFull		=ERR_AMAX2240_FIRM - 5;
        public const short ERR_InterruptButNoData		=ERR_AMAX2240_FIRM - 6; //Engineer Error code

        // Motion command error
        public const short ERR_MotionHaveDone  		   =ERR_AMAX2240_FIRM - 7;
        public const short ERR_SurplusPulseNotEnough   =ERR_AMAX2240_FIRM - 8;
        public const short ERR_InvalidSpeedSection     =ERR_AMAX2240_FIRM - 9;
        public const short ERR_InterpolationMove       =ERR_AMAX2240_FIRM - 10;
        public const short ERR_InvalidSpeedPattern     =ERR_AMAX2240_FIRM - 11;
        public const short ERR_AMAX_FWDownloadExceed	=ERR_AMAX2240_FIRM - 12;
        public const short ERR_AMAX_FWUpdateFailed		=ERR_AMAX2240_FIRM - 13;
        public const short ERR_CntBufferIsFull			=ERR_AMAX2240_FIRM - 14;
        public const short ERR_CntMoveIsBusy			=ERR_AMAX2240_FIRM - 15;
        public const short ERR_CntBufferIsNull			=ERR_AMAX2240_FIRM - 16;
        public const short ERR_FwMemoryAllocateError    = ERR_AMAX2240_FIRM - 17;
        //Added by W.Y.Z on 2012.08.15
        public const short ERR_PassWrdErrorFirstly = ERR_AMAX2240_FIRM - 18;
        public const short ERR_PassWrdErrorAgain = ERR_AMAX2240_FIRM - 19;
        public const short ERR_PassWrdErrorthrice = ERR_AMAX2240_FIRM - 20;
        //AMAX2710 error code
        public const short  ERR_AMAX2710_ERROR          =-300;
        public const short  ERR_Module_Not_Initialize    =ERR_AMAX2710_ERROR-1;
        public const short  ERR_Module_Initialize_Fail    =ERR_AMAX2710_ERROR-2;
        public const short  ERR_AI_Channel_Error   =ERR_AMAX2710_ERROR-3;
        public const short  ERR_AI_Gain_Invalid      =ERR_AMAX2710_ERROR-4;
        public const short  ERR_AO_Channel_Error  =ERR_AMAX2710_ERROR-5;
        public const short  ERR_AO_Range_Error     =ERR_AMAX2710_ERROR-6;
        public const short  ERR_AO_Value_Invalid    =ERR_AMAX2710_ERROR-7;
        public const short  ERR_BUFFER_TOO_SMALL    =ERR_AMAX2710_ERROR-8;
        public const short ERR_MODE_UNMATCHED       = ERR_AMAX2710_ERROR - 9;
        //Communication status       
        public const short  Ring_St_Disconnected    =0x00;
        public const short  Ring_St_Connected       =0x01;
        public const short  Ring_St_Slave_Error     =0x02;
        public const short  Ring_St_Idle            =0x03;
        public const short  Ring_St_Error           =0x04;
        //AMAX1220 error code Added by W.Y.Z on 2012.08.15
        public const short ERR_AMAX1220_ERROR = -400;
        public const short ERR_M2_INVALID_AXISNO = ERR_AMAX1220_ERROR - 1;
        public const short ERR_M2_INVALID_GPID = ERR_AMAX1220_ERROR - 2;
        public const short ERR_M2_CannotFindInvalidGPID = ERR_AMAX1220_ERROR - 3;
        public const short ERR_M2_Axis_Already_In_GP = ERR_AMAX1220_ERROR - 4;
        public const short ERR_M2_Axis_not_exist_inGp = ERR_AMAX1220_ERROR - 5;
        public const short ERR_M2_Invalid_Dist_Array = ERR_AMAX1220_ERROR - 6;
        public const short ERR_M2_Invlaid_Center_Array = ERR_AMAX1220_ERROR - 7;
        public const short ERR_M2_Invalid_Axis_Count = ERR_AMAX1220_ERROR - 8;
        public const short ERR_M2_Invalid_Axis_Index = ERR_AMAX1220_ERROR - 9;

        //public const short ADM_SUCCESS(Status)  =((int)(Status>=0));


        /************************************************************************
             * define structure for AIO 
        ************************************************************************/
        [StructLayout(LayoutKind.Sequential)]
       public struct GAINLIST
       {
        	public ushort  usGainCde;
		    public float   fMaxGainVal;
			public float   fMinGainVal;
            public float   fUpLimit;
            public float   fDownLimit;
           [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
           public byte[]   szGainStr;
	    };

        [StructLayout(LayoutKind.Sequential)]
        public struct PT_AIOFeature
        {
	        public uint    dwModuleID;
	        public ushort  usMaxAISingle;
	        public ushort  usMaxAIDiff;
	        public ushort  usNumADBit;
	        public ushort  usNumAIByte;
	        public IntPtr  AIVolGainArray;
	        public IntPtr  AICurGainArray;
	        public ushort   usMaxAOChan;
	        public ushort   usNumDABit;
	        public ushort   usNumDAByte;
	        public IntPtr AOVolGainArray;
	        public IntPtr AOCurGainArray;
        };
	/************************************************************************
	   Define Function
	************************************************************************/

        ///////////////////////version///////////////////////////////////////////////////
        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_get_version", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _mnet_get_version(ref char version);

        /////////////////////system/////////////////////////////////////////////////////
        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_initial", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _mnet_initial();

        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_close", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _mnet_close();

        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_set_retry_times", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _mnet_set_retry_times(ushort RingNo, ushort RetryTimes);

        [DllImport("AMONet.dll",
			 EntryPoint = "_1202_open", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _1202_open(ref short existcard);

        [DllImport("AMONet.dll",
			 EntryPoint = "_1202_close", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _1202_close(short CardNo);

        [DllImport("AMONet.dll",
			 EntryPoint = "_1202_lio_read", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _1202_lio_read(short CardNo);

        [DllImport("AMONet.dll",
			 EntryPoint = "_1202_lio_write", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _1202_lio_write(short CardNo,byte Value);


        /////////////////////status /////////////////////////////////////////////////////
        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_get_ring_status", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _mnet_get_ring_status(ushort RingNo,ref ushort Status);

        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_get_com_status", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _mnet_get_com_status(ushort RingNo);

        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_enable_soft_watchdog", 
			 CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_enable_soft_watchdog(ushort CardNo, ref IntPtr User_hEvent);

        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_set_ring_quality_param", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _mnet_set_ring_quality_param(ushort RingNo,ushort ContinueErr,ushort ErrorRate);

        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_get_error_device", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _mnet_get_error_device(ushort RingNo);

     

        /////////////////////ring operation//////////////////////////////////////////////
       

       

        

        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_get_slave_info", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _mnet_get_slave_info(ushort RingNo, ushort SlaveIP);

        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_start_ring", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _mnet_start_ring(ushort RingNo);

        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_stop_ring", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _mnet_stop_ring(ushort RingNo);


        [DllImport("AMONet.dll",EntryPoint = "_mnet_get_attr_device_info",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_get_attr_device_info(ushort RingNo, ushort SlaveIP, ref uint Info);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_get_baudrate",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_get_baudrate(ushort RingNo, ref ushort BaudRate);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_download_fw",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_download_fw(ushort RingNo, ushort DeviceIP, uint fwData, uint DataID);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_get_fw_version",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_get_fw_version(ushort RingNo, ushort DeviceIP, ref char FwVersion);
       
/////////////////////io slave operation//////////////////////////////////////////
        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_io_output", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _mnet_io_output(ushort RingNo, ushort DeviceIP,byte PortNo,byte Val);

        [DllImport("AMONet.dll",
			 EntryPoint = "_mnet_io_input", 
			 CallingConvention = CallingConvention.StdCall)]
		public static extern short _mnet_io_input(ushort RingNo, ushort DeviceIP,byte PortNo);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_io_input_wd", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_io_input_wd(ushort RingNo, ushort DeviceIP, ushort PortNo, ref ushort Value);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_io_output_wd", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_io_output_wd(ushort RingNo, ushort DeviceIP, ushort PortNo, ushort Value);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_initial", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_initial(ushort RingNo, ushort DeviceIP);
 /////////////////////Axis slave operation//////////////////////////////////////////

       

//Pulse Input/Output Configuration
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_pls_outmode",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_pls_outmode(ushort RingNo, ushort DeviceIP, ushort pls_outmode);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_pls_iptmode",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_pls_iptmode(ushort RingNo, ushort DeviceIP, ushort pls_iptmode, ushort pls_logic);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_feedback_src",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_feedback_src(ushort RingNo, ushort DeviceIP, ushort Src);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_svon",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_svon( ushort RingNo, ushort DeviceIP, ushort ON_OFF);

//Motion Interface I/O
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_alm",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_alm( ushort RingNo, ushort DeviceIP, ushort alm_logic, ushort alm_mode);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_inp",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_inp( ushort RingNo, ushort DeviceIP, ushort inp_enable, ushort inp_logic);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_erc",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_erc( ushort RingNo, ushort DeviceIP, ushort erc_logic,ushort erc_on_time, ushort erc_off_time);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_erc_on",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_erc_on( ushort RingNo, ushort SlaveIP, short on_off);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_ralm",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_ralm( ushort RingNo, ushort DeviceIP, ushort ON_OFF);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_sd",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_sd( ushort RingNo, ushort DeviceIP, short enable,short sd_logic, short sd_latch, short sd_mode);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_pcs",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_pcs( ushort RingNo, ushort DeviceIP, ushort PCS_logic);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_el",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_el(ushort RingNo, ushort DeviceIP, ushort el_mode);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_autoerc",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_autoerc(ushort RingNo, ushort DeviceIP, ushort ON_OFF);


        

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_dio_output",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_dio_output( ushort RingNo, ushort DeviceIP, ushort DoNO, ushort ON_OFF);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_dio_input",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_dio_input( ushort RingNo, ushort DeviceIP, ushort DiNO);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_get_dio_output", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_get_dio_output(ushort RingNo, ushort DeviceIP, ushort DoNO,ref ushort ON_OFF);



        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_sd_stop",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_sd_stop( ushort RingNo, ushort DeviceIP);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_emg_stop",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_emg_stop( ushort RingNo, ushort DeviceIP);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_loadconfig",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_loadconfig(ushort RingNo, ushort DeviceIP, String szFileName);
/////////////////IO Monitor
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_get_io_status",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_get_io_status( ushort RingNo, ushort DeviceIP, ref uint IO_status);

//Motion Status
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_motion_done",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_motion_done( ushort RingNo, ushort DeviceIP, ref ushort MoSt);


//Single Axis Speed 
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_tmove_speed",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_tmove_speed( ushort RingNo, ushort DeviceIP, double StrVel, double MaxVel, double Tacc,double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_smove_speed",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_smove_speed( ushort RingNo, ushort DeviceIP, double StrVel, double MaxVel, double Tacc,double Tdec,double SVacc,double SVdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_v_change",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_v_change( ushort RingNo, ushort DeviceIP, double NewVel, double Time);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_fix_speed_range",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_fix_speed_range( ushort RingNo, ushort DeviceIP, double MaxVel);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_unfix_speed_range",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_unfix_speed_range( ushort RingNo, ushort DeviceIP);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_p_change",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_p_change(ushort RingNo, ushort DeviceIP, int position);

//Single Axis Motion
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_v_move",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_v_move( ushort RingNo, ushort DeviceIP, byte Dir);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_start_r_move",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_start_r_move( ushort RingNo, ushort DeviceIP, int Distance);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_start_a_move",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_start_a_move( ushort RingNo, ushort DeviceIP, int Pos);

//Simultaneous Axis motion
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_r_move_all",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_r_move_all(ushort TotalDevice, ushort[] RingNoArray, ushort[] DeviceIPArray, int[] DistArray);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_a_move_all",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_set_a_move_all(ushort TotalDevice, ushort[] RingNoArray, ushort[] DeviceIPArray, int[] PosArray);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_sync_stop_mode",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_set_sync_stop_mode(ushort RingNo, ushort DeviceIP, ushort stop_mode);
        //I16 PASCAL _mnet_m1_start_move_all(U16 RingNo, U16 FirstDeviceIP);
        //I16 PASCAL _mnet_m1_stop_move_all(U16 RingNo, U16 FirstDeviceIP);

//Position Compare and Latch
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_comparator_mode",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_comparator_mode(ushort RingNo, ushort DeviceIP, short CompNo, short CmpSrc, short CmpMethod, short CmpAction);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_comparator_data",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_comparator_data(ushort RingNo, ushort DeviceIP, short CompNo, double Pos);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_trigger_comparator",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_trigger_comparator( ushort RingNo, ushort DeviceIP, ushort CmpSrc ,ushort CmpMethod);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_trigger_comparator_data",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_trigger_comparator_data( ushort RingNo, ushort DeviceIP,  double Pos);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_get_comparator_data",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_get_comparator_data( ushort RingNo, ushort DeviceIP, short CompNo, ref double Pos);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_soft_limit",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_soft_limit( ushort RingNo, ushort DeviceIP, int PLimit, int MLimit);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_enable_soft_limit",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_enable_soft_limit( ushort RingNo, ushort DeviceIP,byte Action);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_disable_soft_limit",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_disable_soft_limit( ushort RingNo, ushort DeviceIP);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_ltc_logic",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_ltc_logic( ushort RingNo, ushort DeviceIP,  ushort ltc_logic);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_get_latch_data",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_get_latch_data(ushort RingNo, ushort DeviceIP, short LatchNo, ref double Pos);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_start_soft_ltc",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_start_soft_ltc( ushort RingNo, ushort DeviceIP);



/////////////////Counter Operating
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_command",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_command( ushort RingNo, ushort DeviceIP, int Cmd);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_get_command",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_get_command(ushort RingNo, ushort DeviceIP, ref int Cmd);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_reset_command",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_reset_command( ushort RingNo, ushort DeviceIP);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_get_position",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_get_position( ushort RingNo, ushort DeviceIP,ref int Pos);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_position",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_set_position(ushort RingNo, ushort DeviceIP, int Pos);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_reset_position",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_reset_position( ushort RingNo, ushort DeviceIP);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_get_error_counter",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_get_error_counter( ushort RingNo, ushort DeviceIP,ref int ErrCnt);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_reset_error_counter",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_reset_error_counter( ushort RingNo, ushort DeviceIP);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_get_current_speed",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_get_current_speed( ushort RingNo, ushort DeviceIP, ref double speed);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_move_ratio",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_move_ratio(ushort RingNo, ushort DeviceIP, double ratio);



////////////////Home
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_start_home_move",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_start_home_move( ushort RingNo, ushort DeviceIP, byte Dir);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_set_home_config",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_set_home_config( ushort RingNo, ushort SlaveIP, ushort home_mode,ushort org_logic, ushort ez_logic, ushort ez_count,ushort erc_out);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_start_absread",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_start_absread( ushort RingNo, ushort DeviceIP);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_check_absread_Status",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_check_absread_Status( ushort RingNo, ushort DeviceIP,ref ushort Status);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_get_absread_Position",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_get_absread_Position( ushort RingNo, ushort DeviceIP,ref int AbsPosition);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_error_status",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_error_status( ushort RingNo, ushort DeviceIP,ref uint ErrSt);


        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_change_EL_level",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_change_EL_level( ushort RingNo, ushort DeviceIP,uint level);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m1_exchange_EL",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m1_exchange_EL( ushort RingNo, ushort DeviceIP,uint OFForON);


        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_start_home_search", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_start_home_search(ushort RingNo, ushort DeviceIP,  byte Dir, int ORGOffset);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_start_home_z", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_start_home_z(ushort RingNo, ushort DeviceIP, byte Dir);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_enable_home_reset", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_enable_home_reset(ushort RingNo, ushort DeviceIP, ushort Enable);


        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_start_home_escape", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_start_home_escape(ushort RingNo, ushort DeviceIP,  byte Dir);




        ////////////////////////group///////////////////////////
//  [10/10/2014 deng]
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_add_axis", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_add_axis(ushort RingNo, ushort DeviceIP,  ref ushort GpID);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_remove_axis", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_remove_axis(ushort RingNo, ushort DeviceIP,  ushort GpID);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_reset", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_reset(ushort RingNo,ushort GpID);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_start_tr_line", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_start_tr_line(ushort RingNo, ushort GpID, int[] OffsetDistArray, ushort ElementCnt, double StrVel, double MaxVel, double Tacc, double Tdec, Byte IsConti);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_start_sr_line", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_start_sr_line(ushort RingNo, ushort GpID, int[] OffsetDistArray, ushort ElementCnt, double StrVel, double MaxVel, double Tacc, double Tdec, Byte IsConti);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_start_ta_line", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_start_ta_line(ushort RingNo, ushort GpID, int[] DistArray, ushort ElementCnt, double StrVel, double MaxVel, double Tacc, double Tdec, Byte IsConti);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_start_sa_line", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_start_sa_line(ushort RingNo, ushort GpID, int[] DistArray, ushort ElementCnt, double StrVel, double MaxVel, double Tacc, double Tdec, Byte IsConti);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_start_tr_arc", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_start_tr_arc(ushort RingNo, ushort GpID, ushort[] AxisArray, int OffSetCen_X, int OffSetCen_Y, int OffSetEnd_X, int OffSetEnd_Y, ushort DIR, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_start_sr_arc", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_start_sr_arc(ushort RingNo, ushort GpID, ushort[] AxisArray, int OffSetCen_X, int OffSetCen_Y, int OffSetEnd_X, int OffSetEnd_Y, ushort DIR, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_start_ta_arc", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_start_ta_arc(ushort RingNo, ushort GpID, ushort[] AxisArray, int Cen_X, int Cen_Y, int End_X, int End_Y, ushort DIR, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_start_sa_arc", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_start_sa_arc(ushort RingNo, ushort GpID, ushort[] AxisArray, int Cen_X, int Cen_Y, int End_X, int End_Y, ushort DIR, double StrVel, double MaxVel, double Tacc, double Tdec);
        
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_start_tr_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_start_tr_arc_3p(ushort RingNo, ushort GpID, ushort[] AxisArray, int OffsetRx, int OffsetRy, int OffsetEx, int OffsetEy, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_start_sr_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_start_sr_arc_3p(ushort RingNo, ushort GpID, ushort[] AxisArray, int OffsetRx, int OffsetRy, int OffsetEx, int OffsetEy, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_start_ta_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_start_ta_arc_3p(ushort RingNo, ushort GpID, ushort[] AxisArray, int Rx, int Ry, int Ex, int Ey, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_start_sa_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_start_sa_arc_3p(ushort RingNo, ushort GpID, ushort[] AxisArray, int Rx, int Ry, int Ex, int Ey, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_stop_dec", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_stop_dec(ushort RingNo, ushort GpID);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_gp_stop_emg", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_gp_stop_emg(ushort RingNo, ushort GpID);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m1_set_abs_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_set_abs_mode(ushort RingNo, ushort DeviceIP, ushort AbsMode);

        //[DllImport("AMONet.dll", EntryPoint = "_mnet_m1_set_trigger_comparator_pulse", CallingConvention = CallingConvention.StdCall)]
        //public static extern short _mnet_m1_set_trigger_comparator_pulse(ushort RingNo, ushort DeviceIP,  ushort PulseMode, ushort Logic, ushort PulseWidth);


//----------------------- ADAM-2242 -----------------------------------------------------------

/////////////////////Axis slave operation//////////////////////////////////////////
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_initial",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_initial(ushort RingNo, ushort DeviceIP);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_com_wdg_mode",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_com_wdg_mode(ushort RingNo, ushort DeviceIP, ushort StopMode);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_loadconfig",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_loadconfig(ushort RingNo, ushort DeviceIP, String szFileName);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_fw_memory", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_fw_memory(ushort RingNo, ushort DeviceIP, ushort Mode);

//Pulse Input/Output Configuration
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_pls_outmode",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_pls_outmode(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort pls_outmode);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_pls_iptmode",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_pls_iptmode(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort pls_iptmode, ushort pls_logic);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_feedback_src",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_feedback_src(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort Src);
        
//Motion Interface I/O
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_alm",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_alm( ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort alm_logic, ushort alm_mode);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_inp",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_inp( ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort inp_enable, ushort inp_logic);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_erc",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_erc( ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort erc_logic,ushort erc_on_time, ushort erc_off_time);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_erc_on",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_erc_on( ushort RingNo, ushort SlaveIP, ushort AxisNo, short on_off);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_autoerc",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_autoerc(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort ON_OFF);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_ralm",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_ralm( ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort ON_OFF);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_sd",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_sd( ushort RingNo, ushort DeviceIP, ushort AxisNo, short enable,short sd_logic, short sd_latch, short sd_mode);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_pcs",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_pcs( ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort PCS_logic);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_el",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_el(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort el_mode);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_svon",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_svon( ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort ON_OFF);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_sd_stop",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_sd_stop( ushort RingNo, ushort DeviceIP, ushort AxisNo);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_emg_stop",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_emg_stop( ushort RingNo, ushort DeviceIP, ushort AxisNo);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_pause_motion", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_pause_motion(ushort RingNo, ushort DeviceIP, ushort AxisNo);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_resume_motion", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_resume_motion(ushort RingNo, ushort DeviceIP, ushort AxisNo);
    
        // dan.yang [12/29/2012]
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_abs_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_abs_mode(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort AbsMode);


/////////////////IO Monitor
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_get_io_status",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_get_io_status( ushort RingNo, ushort DeviceIP, ushort AxisNo, ref uint IO_status);

//Motion Status
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_motion_done",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_motion_done( ushort RingNo, ushort DeviceIP, ushort AxisNo, ref ushort MoSt);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_error_status",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_error_status(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref uint ErrSt);


//Single Axis Speed 
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_tmove_speed",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_tmove_speed( ushort RingNo, ushort DeviceIP, ushort AxisNo, double StrVel, double MaxVel, double Tacc,double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_smove_speed",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_smove_speed( ushort RingNo, ushort DeviceIP, ushort AxisNo, double StrVel, double MaxVel, double Tacc,double Tdec,double SVacc,double SVdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_v_change",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_v_change( ushort RingNo, ushort DeviceIP, ushort AxisNo, double NewVel, double Time);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_fix_speed_range",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_fix_speed_range( ushort RingNo, ushort DeviceIP, ushort AxisNo, double MaxVel);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_unfix_speed_range",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_unfix_speed_range( ushort RingNo, ushort DeviceIP, ushort AxisNo);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_p_change",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_p_change(ushort RingNo, ushort DeviceIP, ushort AxisNo, int position);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_p_change_r", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_p_change_r(ushort RingNo, ushort DeviceIP, ushort AxisNo, int position);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_p_change_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_p_change_all(ushort RingNo, ushort DeviceIP, ushort[] AxesArray, int[] NewDistArray, ushort AxesCnt);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_p_change_r_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_p_change_r_all(ushort RingNo, ushort DeviceIP, ushort[] AxesArray, int[] NewDistArray, ushort AxesCnt);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_v_change_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_v_change_all(ushort RingNo, ushort DeviceIP, ushort[] AxesArray, double[] NewVelArray, double[] NewTimeArray, ushort AxesCnt);

//Single Axis Motion
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_v_move",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_v_move( ushort RingNo, ushort DeviceIP, ushort AxisNo, byte Dir);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_r_move",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_r_move( ushort RingNo, ushort DeviceIP, ushort AxisNo, int Distance);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_a_move",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_a_move( ushort RingNo, ushort DeviceIP, ushort AxisNo, int Pos);

//Simultaneous motion
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_r_move_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_r_move_all(ushort RingNo, ushort DeviceIP, ushort TotalAxesNum, ushort[] AxesArray, int[] DistArray);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_a_move_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_a_move_all(ushort RingNo, ushort DeviceIP, ushort TotalAxesNum, ushort[] AxesArray, int[] DistArray);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_v_move_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_v_move_all(ushort RingNo, ushort DeviceIP, ushort TotalAxesNum, ushort[] AxesArray, ushort[] DirArray);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_start_move_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_move_all(ushort RingNo, ushort DeviceIP, ushort FirstAxisNo);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_stop_move_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_stop_move_all(ushort RingNo, ushort DeviceIP, ushort FirstAxisNo);




//Position Compare and Latch
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_comparator_mode",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_comparator_mode(ushort RingNo, ushort DeviceIP, ushort AxisNo, short CompNo, short CmpSrc, short CmpMethod, short CmpAction);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_comparator_data",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_comparator_data(ushort RingNo, ushort DeviceIP, ushort AxisNo, short CompNo, double Pos);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_trigger_comparator",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_trigger_comparator( ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort CmpSrc ,ushort CmpMethod);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_trigger_comparator_pulse",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_trigger_comparator_pulse(ushort RingNo, ushort DeviceIP, ushort AxisNo,ushort PulseMode, ushort Logic, ushort PulseWidth);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_trigger_comparator_table",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_trigger_comparator_table(ushort RingNo, ushort DeviceIP, ushort AxisNo, double[] TableArray, int ArraySize);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_trigger_comparator_auto",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_trigger_comparator_auto(ushort RingNo, ushort DeviceIP, ushort AxisNo, double Start, double End, int Interval);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_trigger_comparator_data",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_trigger_comparator_data(ushort RingNo, ushort DeviceIP, ushort AxisNo, double Pos);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_get_trigger_comparator_data",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_get_trigger_comparator_data(ushort RingNo, ushort DeviceIP, ushort AxisNo,ref double Pos);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_get_comparator_data",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_comparator_data(ushort RingNo, ushort DeviceIP, ushort AxisNo, short CompNo, ref double Pos);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_reset_trigger_comparator_level", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_reset_trigger_comparator_level(ushort RingNo, ushort DeviceIP, ushort AxisNo);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_trigger_comparator_level", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_trigger_comparator_level(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref ushort CmpLevel);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_trigger_comparator_auto_endless", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_trigger_comparator_auto_endless(ushort RingNo, ushort DeviceIP, ushort AxisNo, double Start,  int Interval);



        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_soft_limit",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_soft_limit( ushort RingNo, ushort DeviceIP, ushort AxisNo, int PLimit, int MLimit);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_enable_soft_limit",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_enable_soft_limit( ushort RingNo, ushort DeviceIP, ushort AxisNo,byte Action);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_disable_soft_limit",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_disable_soft_limit( ushort RingNo, ushort DeviceIP, ushort AxisNo);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_steplose_check",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_steplose_check( ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort Tolerance, ushort CmpAction, ushort Enable);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_ltc_logic",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_ltc_logic( ushort RingNo, ushort DeviceIP, ushort AxisNo,  ushort ltc_logic);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_ltc_enable", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_ltc_enable(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort ltc_enable);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_get_latch_data",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_get_latch_data(ushort RingNo, ushort DeviceIP, ushort AxisNo, short LatchNo, ref double Pos);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_soft_ltc",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_soft_ltc( ushort RingNo, ushort DeviceIP, ushort AxisNo);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_move_ratio",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_move_ratio(ushort RingNo, ushort DeviceIP, ushort AxisNo, double ratio);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_backlash_comp",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_backlash_comp(ushort RingNo, ushort DeviceIP, ushort AxisNo, short BcompPulse, short Mode);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_suppress_vibration",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_suppress_vibration(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort ReverseTime, ushort ForwardTime);



/////////////////Counter Operating
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_command",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_command( ushort RingNo, ushort DeviceIP, ushort AxisNo, int Cmd);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_get_command",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_get_command( ushort RingNo, ushort DeviceIP, ushort AxisNo, ref int Cmd);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_reset_command",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_reset_command( ushort RingNo, ushort DeviceIP, ushort AxisNo);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_get_position",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_get_position( ushort RingNo, ushort DeviceIP, ushort AxisNo, ref int Pos);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_position",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_position( ushort RingNo, ushort DeviceIP, ushort AxisNo, int Pos);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_reset_position",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_reset_position( ushort RingNo, ushort DeviceIP, ushort AxisNo);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_get_error_counter",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_get_error_counter( ushort RingNo, ushort DeviceIP, ushort AxisNo, ref int ErrCnt);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_reset_error_counter",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_reset_error_counter( ushort RingNo, ushort DeviceIP, ushort AxisNo);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_get_current_speed",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_get_current_speed( ushort RingNo, ushort DeviceIP, ushort AxisNo, ref double speed);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_get_rpls",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_get_rpls(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref int rpls);



////////////////Home
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_home_move",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_home_move( ushort RingNo, ushort DeviceIP, ushort AxisNo, byte Dir);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_home_config",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_home_config( ushort RingNo, ushort SlaveIP, ushort AxisNo, ushort home_mode,ushort org_logic, ushort ez_logic, ushort ez_count, ushort erc_out);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_home_search",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_home_search(ushort RingNo, ushort DeviceIP, ushort AxisNo, byte Dir, int ORGOffset);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_home_z",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_home_z(ushort RingNo, ushort DeviceIP, ushort AxisNo, byte Dir);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_enable_home_reset",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_enable_home_reset(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort Enable);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_start_home_escape", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_home_escape(ushort RingNo, ushort DeviceIP, ushort AxisNo, byte Dir);

////////////////Multi Axis Motion
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_tr_move_xy",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_tr_move_xy(ushort RingNo, ushort DeviceIP, int DistX, int DistY, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_ta_move_xy",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_ta_move_xy(ushort RingNo, ushort DeviceIP, int PosX, int PosY, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sr_move_xy",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_sr_move_xy(ushort RingNo, ushort DeviceIP, int DistX, int DistY, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sa_move_xy",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_sa_move_xy(ushort RingNo, ushort DeviceIP, int PosX, int PosY, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_tr_move_zu",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_tr_move_zu(ushort RingNo, ushort DeviceIP, int DistZ, int DistU, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_ta_move_zu",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_ta_move_zu(ushort RingNo, ushort DeviceIP, int PosZ, int PosU, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sr_move_zu",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_sr_move_zu(ushort RingNo, ushort DeviceIP, int DistZ, int DistU, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sa_move_zu",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_sa_move_zu(ushort RingNo, ushort DeviceIP, int PosZ, int PosU, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_tr_line2",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_tr_line2(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int DistX, int DistY, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_ta_line2",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_ta_line2(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int PosX, int PosY, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sr_line2",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_sr_line2(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int DistX, int DistY, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sa_line2",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_sa_line2(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int PosX, int PosY, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_tr_line3",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_tr_line3(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int DistX, int DistY, int DistZ, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_ta_line3",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_ta_line3(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int PosX, int PosY, int PosZ, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sr_line3",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_sr_line3(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int DistX, int DistY, int DistZ, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sa_line3",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_sa_line3(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int PosX, int PosY, int PosZ, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_tr_line4",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_tr_line4(ushort RingNo, ushort DeviceIP, int DistX, int DistY, int DistZ, int DistU, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_ta_line4",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_ta_line4(ushort RingNo, ushort DeviceIP, int PosX, int PosY, int PosZ, int PosU, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sr_line4",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_sr_line4(ushort RingNo, ushort DeviceIP, int DistX, int DistY, int DistZ, int DistU, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sa_line4",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_sa_line4(ushort RingNo, ushort DeviceIP, int PosX, int PosY, int PosZ, int PosU, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_tr_arc_xy",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_tr_arc_xy(ushort RingNo, ushort DeviceIP, int OffsetCx, int OffsetCy, int OffsetEx, int OffsetEy, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_ta_arc_xy",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_ta_arc_xy(ushort RingNo, ushort DeviceIP, int Cx, int Cy, int Ex, int Ey, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sr_arc_xy",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_sr_arc_xy(ushort RingNo, ushort DeviceIP, int OffsetCx, int OffsetCy, int OffsetEx, int OffsetEy, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sa_arc_xy",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_sa_arc_xy(ushort RingNo, ushort DeviceIP, int Cx, int Cy, int Ex, int Ey, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_tr_arc_zu",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_tr_arc_zu(ushort RingNo, ushort DeviceIP, int OffsetCz, int OffsetCu, int OffsetEz, int OffsetEu, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_ta_arc_zu",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_ta_arc_zu(ushort RingNo, ushort DeviceIP, int Cz, int Cu, int Ez, int Eu, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sr_arc_zu",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_sr_arc_zu(ushort RingNo, ushort DeviceIP, int OffsetCz, int OffsetCu, int OffsetEz, int OffsetEu, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sa_arc_zu",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_sa_arc_zu(ushort RingNo, ushort DeviceIP, int Cz, int Cu, int Ez, int Eu, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);


        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_tr_arc2",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_tr_arc2(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int OffsetCx, int OffsetCy, int OffsetEx, int OffsetEy, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_ta_arc2",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_ta_arc2(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int Cx, int Cy, int Ex, int Ey, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sr_arc2",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_sr_arc2(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int OffsetCx, int OffsetCy, int OffsetEx, int OffsetEy, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_sa_arc2",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_sa_arc2(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int Cx, int Cy, int Ex, int Ey, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_start_tr_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_tr_arc_3p(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int OffsetRx, int OffsetRy, int OffsetEx, int OffsetEy, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_start_ta_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_ta_arc_3p(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int Rx, int Ry, int Ex, int Ey, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_start_sr_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_sr_arc_3p(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int OffsetRx, int OffsetRy, int OffsetEx, int OffsetEy, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_start_sa_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_sa_arc_3p(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, int Rx, int Ry, int Ex, int Ey, double StrVel, double MaxVel, double Tacc, double Tdec);



        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_path_move_speed",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_path_move_speed(ushort RingNo, ushort DeviceIP, ushort TorS, double strVel, double maxVel, double tAcc, double tDec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_path_arc_data",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_path_arc_data(ushort RingNo, ushort DeviceIP, ushort[] AxisArray,	ushort CmdFunc, int[] CenArray, int[] EndArray, ushort Dir,	double StrVel, double MaxVel, short EnableDec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_set_path_line_data",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_set_path_line_data(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, ushort CmdFunc, int[] DistArray, double StrVel, double MaxVel, ushort EnableDec);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_start_path",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_start_path(ushort RingNo, ushort DeviceIP);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_get_path_status",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_get_path_status(ushort RingNo, ushort DeviceIP, ref uint CurIndex,ref ushort CurCmdFunc,ref uint StockCmdCount,ref uint FreeSpaceCount);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_m4_reset_path",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_m4_reset_path(ushort RingNo, ushort DeviceIP);


        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_eeprom", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_eeprom(ushort RingNo, ushort DeviceIP, ushort PrivateID, uint PassWrd_1, uint PassWrd_2, ref uint Data_1, ref uint Data_2);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_eeprom", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_eeprom(ushort RingNo, ushort DeviceIP, ushort PrivateID, uint PassWrd_1, uint PassWrd_2,uint Data_1, uint Data_2);



/*   AMAX 2710  */
//////////////////////////////////////////////////////////////////////////
//  Analog input and output slave
        /*
                [DllImport("AMONet.dll",EntryPoint = "_mnet_ai_check_event",CallingConvention = CallingConvention.StdCall)]		
         *      public static extern short _mnet_ai_check_event(ushort RingNo, ushort DeviceIP, ref ushort EventType, uint Misseconds);
                [DllImport("AMONet.dll",EntryPoint = "_mnet_ai_enable_event",CallingConvention = CallingConvention.StdCall)]		
         *      public static extern short _mnet_ai_enable_event(ushort RingNo, ushort DeviceIP, ushort EventType, ushort EventCount, ushort Enabled);
                [DllImport("AMONet.dll",EntryPoint = "_mnet_ai_start",CallingConvention = CallingConvention.StdCall)]		
         *      public static extern short _mnet_ai_start(ushort RingNo, ushort DeviceIP, ushort GainCode, uint ConvNum, void* pBuf, double SampleRate);
	
         
         *      [DllImport("AMONet.dll",EntryPoint = "_mnet_ai_stop",CallingConvention = CallingConvention.StdCall)]	
         *      public static extern short _mnet_ai_stop(ushort RingNo, ushort DeviceIP);
                [DllImport("AMONet.dll",EntryPoint = "_mnet_ai_transfer",CallingConvention = CallingConvention.StdCall)]		
         *      public static extern short _mnet_ai_transfer(ushort RingNo, ushort DeviceIP, ushort DataType, ref ushort Overrun, uint Start, uint Count, void* pBuf);
        */


        [DllImport("AMONet.dll",EntryPoint = "_mnet_aio_initial",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_aio_initial(ushort RingNo, ushort DeviceIP);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_aio_get_feature",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_aio_get_feature(ushort RingNo, ushort DeviceIP,ref PT_AIOFeature pFeature);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_ai_config",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_ai_config(ushort RingNo, ushort DeviceIP, ushort usChan, ushort usGain);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_mai_config",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_mai_config(ushort RingNo, ushort DeviceIP, ushort usNumChan, ushort usStartChan, ushort[] usGain);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_ai_set_con_mode", CallingConvention = CallingConvention.StdCall)]	
        public static extern short _mnet_ai_set_con_mode(ushort RingNo, ushort DeviceIP, byte[] ChanConfig, ushort usSize);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_ai_get_con_mode", CallingConvention = CallingConvention.StdCall)]	
        public static extern short _mnet_ai_get_con_mode(ushort RingNo, ushort DeviceIP, byte[]  ChanConfig, ref ushort usSize);

        [DllImport("AMONet.dll",EntryPoint = "_mnet_ai_binary_in",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_ai_binary_in(ushort RingNo, ushort DeviceIP, ushort usChan, ref ushort InputData);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_mai_binary_in",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_mai_binary_in(ushort RingNo, ushort DeviceIP, ushort usNumChan, ushort usStartChan, ushort[] pData);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_ai_voltage_in",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_ai_voltage_in(ushort RingNo, ushort DeviceIP, ushort usChan, ref float InputData);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_mai_voltage_in",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_mai_voltage_in(ushort RingNo, ushort DeviceIP, ushort usNumChan, ushort usStartChan, float[] pData);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_ai_current_in", CallingConvention = CallingConvention.StdCall)]	
        public static extern short  _mnet_ai_current_in(ushort RingNo, ushort DeviceIP, ushort usChan, ref float InputData);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_mai_current_in", CallingConvention = CallingConvention.StdCall)]	
        public static extern short  _mnet_mai_current_in(ushort RingNo, ushort DeviceIP, ushort usNumChan, ushort usStartChan, float[] pData);


        [DllImport("AMONet.dll",EntryPoint = "_mnet_ao_config",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_ao_config(ushort RingNo, ushort DeviceIP, ushort usChan, ushort AoRange);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_ao_binary_out",CallingConvention = CallingConvention.StdCall)]		
        public static extern short _mnet_ao_binary_out(ushort RingNo, ushort DeviceIP, ushort usChan, ushort OutData);
        [DllImport("AMONet.dll",EntryPoint = "_mnet_ao_voltage_out",CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_ao_voltage_out(ushort RingNo, ushort DeviceIP, ushort usChan, float OutData);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_ao_current_out", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_ao_current_out(ushort RingNo, ushort DeviceIP, ushort usChan, float OutData);

        /////////////////Synchronous Operating
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_sync_option", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_sync_option(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort mode);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_sync_signal_source", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_sync_signal_source(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort SourceAxisNo);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_sync_signal_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_sync_signal_mode(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort mode);

        //Break
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_break_on", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_break_on(ushort RingNo, ushort DeviceIP, ushort usChan, ushort usOnOff);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_auto_break", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_auto_break(ushort RingNo, ushort DeviceIP, ushort usChan);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_auto_break_status", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_auto_break_status(ushort RingNo, ushort DeviceIP, ushort usChan, ref int status);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_svon_brktime", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_svon_brktime(ushort RingNo, ushort DeviceIP, ushort usChan, uint data);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_svoff_brktime", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_svoff_brktime(ushort RingNo, ushort DeviceIP, ushort usChan, uint data);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_svon_break_time", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_svon_break_time(ushort RingNo, ushort DeviceIP, ushort usChan, ref uint data);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_svoff_break_time", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_svoff_break_time(ushort RingNo, ushort DeviceIP, ushort usChan, ref uint data);

        //================2 axis module [yuzhi.wang 2012.06.08]=====================================================//
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_initial", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_initial(ushort RingNo, ushort DeviceIP);

        //Pulse Input/Output Configuration
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_pls_outmode", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_pls_outmode(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort pls_outmode);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_pls_iptmode", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_pls_iptmode(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort pls_iptmode,ushort pls_logic);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_feedback_src", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_feedback_src(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort src);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_svon", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_svon(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort ON_OFF);

        //Motion Interface I/O
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_alm", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_alm(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort alm_logic, ushort alm_mode);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_inp", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_inp(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort inp_enable, ushort inp_logic);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_erc", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_erc(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort erc_logic, ushort erc_on_time, ushort erc_off_time);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_erc_on", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_erc_on(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort ON_OFF);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_ralm", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_ralm(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort ON_OFF);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_sd", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_sd(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort sd_enable,ushort sd_logic,ushort sd_latch,ushort sd_mode);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_pcs", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_pcs(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort pcs_logic);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_el", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_el(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort el_mode);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_autoerc", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_autoerc(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort ON_OFF);

        // dan.yang [12/29/2012]
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_abs_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_abs_mode(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort AbsMode);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_dio_output", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_dio_output(ushort RingNo, ushort DeviceIP, ushort DoNo, ushort ON_OFF);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_dio_input", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_dio_input(ushort RingNo, ushort DeviceIP, ushort DiNo);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_dio_channel_output", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_dio_channel_output(ushort RingNo, ushort DeviceIP, ushort Channel, ushort OutData);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_dio_channel_input", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_dio_channel_input(ushort RingNo, ushort DeviceIP, ushort Channel, ref ushort InData);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_sd_stop", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_sd_stop(ushort RingNo, ushort DeviceIP, ushort AxisNo);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_emg_stop", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_emg_stop(ushort RingNo, ushort DeviceIP, ushort AxisNo);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_loadconfig", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_loadconfig(ushort RingNo, ushort DeviceIP, string szFileName);

        //Motion IO Status/State
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_get_io_status", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_get_io_status(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref uint IO_status);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_motion_done", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_motion_done(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref ushort MotionState);

        //Single Axis Speed 
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_tmove_speed", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_tmove_speed(ushort RingNo, ushort DeviceIP, ushort AxisNo, double StrVel,double MaxVel,double Tacc,double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_smove_speed", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_smove_speed(ushort RingNo, ushort DeviceIP, ushort AxisNo, double StrVel, double MaxVel, double Tacc, double Tdec,double SVacc,double SVdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_v_change", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_v_change(ushort RingNo, ushort DeviceIP, ushort AxisNo, double NewVel, double time);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_fix_speed_range", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_fix_speed_range(ushort RingNo, ushort DeviceIP, ushort AxisNo, double MaxVel);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_unfix_speed_range", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_unfix_speed_range(ushort RingNo, ushort DeviceIP, ushort AxisNo);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_p_change", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_p_change(ushort RingNo, ushort DeviceIP, ushort AxisNo, int position);

        //Single Axis Motion
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_v_move", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_v_move(ushort RingNo, ushort DeviceIP, ushort AxisNo, byte Dir);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_r_move", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_r_move(ushort RingNo, ushort DeviceIP, ushort AxisNo, int Distance);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_a_move", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_a_move(ushort RingNo, ushort DeviceIP, ushort AxisNo, int Position);

        //Simultaneous Axis motion
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_r_move_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_r_move_all(ushort RingNo, ushort DeviceIP, ushort TotalAxesNum, ushort[] DeviceArray, int[] DistArray);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_a_move_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_a_move_all(ushort RingNo, ushort DeviceIP, ushort TotalAxesNum, ushort[] DeviceArray, int[] PosArray);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_sync_stop_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_sync_stop_mode(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort StopMode);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_v_move_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_v_move_all(ushort RingNo, ushort DeviceIP, ushort TotalAxesNum, ushort[] AxesArray, ushort[] DirArray);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_move_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_move_all(ushort RingNo, ushort DeviceIP, ushort FirstAxisNo);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_stop_move_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_stop_move_all(ushort RingNo, ushort DeviceIP, ushort FirstAxisNo);

        //Position Compare and Latch
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_comparator_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_comparator_mode(ushort RingNo, ushort DeviceIP, ushort AxisNo, short CmpNo,short CmpSrc,short CmpMethod,short CmpAction);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_comparator_data", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_comparator_data(ushort RingNo, ushort DeviceIP, ushort AxisNo, short CmpNo, double Pos);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_trigger_comparator", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_trigger_comparator(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort CmpSrc, ushort CmpMethod);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_trigger_comparator_data", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_trigger_comparator_data(ushort RingNo, ushort DeviceIP, ushort AxisNo, double data);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_get_comparator_data", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_get_comparator_data(ushort RingNo, ushort DeviceIP, ushort AxisNo, short CmpNo, ref double data);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_get_trigger_comparator_data", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_get_trigger_comparator_data(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref double Pos);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_trigger_comparator_pulse", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_trigger_comparator_pulse(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort PulseMode, ushort Logic, ushort PulseWidth);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_reset_trigger_comparator_level", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_reset_trigger_comparator_level(ushort RingNo, ushort DeviceIP, ushort AxisNo);

        //Soft Limit
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_soft_limit", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_soft_limit(ushort RingNo, ushort DeviceIP, ushort AxisNo, int PLimit, int NLimit);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_enable_soft_limit", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_enable_soft_limit(ushort RingNo, ushort DeviceIP, ushort AxisNo, byte Action);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_disable_soft_limit", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_disable_soft_limit(ushort RingNo, ushort DeviceIP, ushort AxisNo);

        //Latch
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_ltc_logic", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_ltc_logic(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort ltc_logic);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_get_latch_data", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_get_latch_data(ushort RingNo, ushort DeviceIP, ushort AxisNo, short LatchNo, ref double Pos);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_soft_ltc", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_soft_ltc(ushort RingNo, ushort DeviceIP);

        /////////////////Counter Operating
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_command", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_command(ushort RingNo, ushort DeviceIP, ushort AxisNo, int Cmd);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_get_command", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_get_command(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref int Cmd);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_reset_command", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_reset_command(ushort RingNo, ushort DeviceIP, ushort AxisNo);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_position", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_position(ushort RingNo, ushort DeviceIP, ushort AxisNo, int Pos);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_get_position", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_get_position(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref int Pos);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_reset_position", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_reset_position(ushort RingNo, ushort DeviceIP, ushort AxisNo);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_get_error_counter", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_get_error_counter(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref int ErrCnt);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_reset_error_counter", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_reset_error_counter(ushort RingNo, ushort DeviceIP, ushort AxisNo);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_get_current_speed", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_get_current_speed(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref double speed);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_move_ratio", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_move_ratio(ushort RingNo, ushort DeviceIP, ushort AxisNo, double ratio);

        ////////////////Home
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_home_move", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_home_move(ushort RingNo, ushort DeviceIP, ushort AxisNo, byte Dir);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_home_escape", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_home_escape(ushort RingNo, ushort DeviceIP, ushort AxisNo, byte Dir);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_home_config", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_home_config(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort home_mode, ushort org_logic, ushort ez_logic, ushort ez_count, ushort erc_out);

        ////////////////Interpolation Motion
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_groupno", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_groupno(ushort[] RingNoArray, ushort[] DeviceIPArray, ushort DeviceCount, ushort GroupNo);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_ipo_tmove_speed", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_ipo_tmove_speed(ushort[] RingNoArray, ushort[] DeviceIPArray, ushort DeviceCount, double StrVel,double MaxVel,double Tacc,double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_ipo_smove_speed", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_ipo_smove_speed(ushort[] RingNoArray, ushort[] DeviceIPArray, ushort DeviceCount, double StrVel, double MaxVel, double Tacc, double Tdec,double SVacc,double SVdec);

      
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_tr_arc_xy", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_tr_arc_xy(ushort RingNo, ushort DeviceIP, int OffsetCx, int OffsetCY, int OffsetEx, int OffsetEy, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_sr_arc_xy", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_sr_arc_xy(ushort RingNo, ushort DeviceIP, int OffsetCx, int OffsetCY, int OffsetEx, int OffsetEy, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_ta_arc_xy", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_ta_arc_xy(ushort RingNo, ushort DeviceIP, int Cx, int CY, int Ex, int Ey, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_sa_arc_xy", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_sa_arc_xy(ushort RingNo, ushort DeviceIP, int Cx, int CY, int Ex, int Ey, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_sa_arc_xy", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_sa_arc2(ushort RingNo, ushort DeviceIP, int Cx, int CY, int Ex, int Ey, ushort Dir, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_tr_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_tr_arc_3p(ushort RingNo, ushort DeviceIP, ushort[] AxisNoArray, int OffsetRx, int OffsetRy, int OffsetEx, int OffsetEy, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_ta_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_ta_arc_3p(ushort RingNo, ushort DeviceIP, ushort[] AxisNoArray, int Rx, int Ry, int Ex, int Ey, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_sr_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_sr_arc_3p(ushort RingNo, ushort DeviceIP, ushort[] AxisNoArray, int OffsetRx, int OffsetRy, int OffsetEx, int OffsetEy, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_sa_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_sa_arc_3p(ushort RingNo, ushort DeviceIP, ushort[] AxisNoArray, int Rx, int Ry, int Ex, int Ey, double StrVel, double MaxVel, double Tacc, double Tdec);

        //Compare
        //[DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_trigger_comparator_table", CallingConvention = CallingConvention.StdCall)]
        //public static extern short _mnet_m2_set_trigger_comparator_table(ushort RingNo, ushort DeviceIP, ushort AxisNo, double[] TableArray, short Size);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_trigger_comparator_auto", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_trigger_comparator_auto(ushort RingNo, ushort DeviceIP, ushort AxisNo, double Start, double End,int Interval);

        //Break
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_break_on", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_break_on(ushort RingNo, ushort DeviceIP, ushort usChan, ushort usOnOff);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_auto_break", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_auto_break(ushort RingNo, ushort DeviceIP, ushort usChan);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_get_auto_break_status", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_get_auto_break_status(ushort RingNo, ushort DeviceIP, ushort usChan, ref int status);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_svon_brktime", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_svon_brktime(ushort RingNo, ushort DeviceIP, ushort usChan, uint data);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_set_svoff_brktime", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_set_svoff_brktime(ushort RingNo, ushort DeviceIP, ushort usChan, uint data);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_get_svon_break_time", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_get_svon_break_time(ushort RingNo, ushort DeviceIP, ushort usChan, ref uint data);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_get_svoff_break_time", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_get_svoff_break_time(ushort RingNo, ushort DeviceIP, ushort usChan, ref uint data);

        //Group Motion
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_add_axis", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_add_axis(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref ushort GpID);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_remove_axis", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_remove_axis(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort GpID);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_reset", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_reset(ushort RingNo,ushort GpID);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_start_tr_line", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_start_tr_line(ushort RingNo, ushort GpID, int[] OffsetDistArray, ushort ElementCnt, double StrVel, double MaxVel, double Tacc, double Tdec, Byte IsConti);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_start_sr_line", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_start_sr_line(ushort RingNo, ushort GpID, int[] OffsetDistArray, ushort ElementCnt, double StrVel, double MaxVel, double Tacc, double Tdec, Byte IsConti);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_start_ta_line", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_start_ta_line(ushort RingNo, ushort GpID, int[] DistArray, ushort ElementCnt, double StrVel, double MaxVel, double Tacc, double Tdec, Byte IsConti);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_start_sa_line", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_start_sa_line(ushort RingNo, ushort GpID, int[] DistArray, ushort ElementCnt, double StrVel, double MaxVel, double Tacc, double Tdec, Byte IsConti);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_start_tr_arc", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_start_tr_arc(ushort RingNo, ushort GpID, ushort[] AxisArray, int OffSetCen_X, int OffSetCen_Y, int OffSetEnd_X, int OffSetEnd_Y, ushort DIR, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_start_sr_arc", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_start_sr_arc(ushort RingNo, ushort GpID, ushort[] AxisArray, int OffSetCen_X, int OffSetCen_Y, int OffSetEnd_X, int OffSetEnd_Y, ushort DIR, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_start_ta_arc", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_start_ta_arc(ushort RingNo, ushort GpID, ushort[] AxisArray, int Cen_X, int Cen_Y, int End_X, int End_Y, ushort DIR, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_start_sa_arc", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_start_sa_arc(ushort RingNo, ushort GpID, ushort[] AxisArray, int Cen_X, int Cen_Y, int End_X, int End_Y, ushort DIR, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_start_tr_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_start_tr_arc_3p(ushort RingNo, ushort GpID, ushort[] AxisArray, int OffsetRx, int OffsetRy, int OffsetEx, int OffsetEy, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_start_sr_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_start_sr_arc_3p(ushort RingNo, ushort GpID, ushort[] AxisArray, int OffsetRx, int OffsetRy, int OffsetEx, int OffsetEy, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_start_ta_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_start_ta_arc_3p(ushort RingNo, ushort GpID, ushort[] AxisArray, int Rx, int Ry, int Ex, int Ey, double StrVel, double MaxVel, double Tacc, double Tdec);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_start_sa_arc_3p", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_start_sa_arc_3p(ushort RingNo, ushort GpID, ushort[] AxisArray, int Rx, int Ry, int Ex, int Ey, double StrVel, double MaxVel, double Tacc, double Tdec);

     
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_stop_dec", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_stop_dec(ushort RingNo, ushort GpID);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_stop_emg", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_stop_emg(ushort RingNo, ushort GpID);

       
        
        
        
        
        
        
        //Added by W.Y.Z on 2013.11.20
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_soft_emg_stop", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_soft_emg_stop(ushort RingNo, ushort DeviceIP,ushort AxisNo);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_io_status_ex", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_io_status_ex(ushort RingNo, ushort DeviceIP,ushort[] AxisNoArray,uint[] IO_statusArray,ushort Count);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_command_ex", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_command_ex(ushort RingNo, ushort DeviceIP,ushort[] AxisNoArray,int[] CmdArray,ushort Count);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_position_ex", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_position_ex(ushort RingNo, ushort DeviceIP,ushort[] AxisNoArray,int[] PosArray,ushort Count);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_motion_done_ex", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_motion_done_ex(ushort RingNo, ushort DeviceIP,ushort[] AxisNoArray,int[] MoStArray,ushort Count);
        
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_error_status", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_error_status(ushort RingNo, ushort DeviceIP,ushort AxisNo,ref uint ErrSt);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_rist_status", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_rist_status(ushort RingNo, ushort DeviceIP,ushort AxisNo,ref uint IntSt);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_home_search", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_home_search(ushort RingNo, ushort DeviceIP,ushort AxisNo,byte Dir,int ORGOffset);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_home_z", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_home_z(ushort RingNo, ushort DeviceIP,ushort AxisNo,byte Dir);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_enable_home_reset", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_enable_home_reset(ushort RingNo, ushort DeviceIP,ushort AxisNo,ushort Enable);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_enable_interrupt", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_enable_interrupt(ushort RingNo, ushort DeviceIP, ushort AxisNo, uint IntEn);

        //Added by W.Y.Z on 2013.11.21
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_rist_status", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_rist_status(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref uint IntSt);

        //Added by W.Y.Z on 2013.11.26
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_errcnt_limit", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_errcnt_limit(ushort RingNo, ushort DeviceIP, ushort AxisNo, int Llimt, int Rlimt);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_check_errcnt_limit", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_check_errcnt_limit(ushort RingNo, ushort DeviceIP, ushort AxisNo);

        //Added by W.Y.Z on 2014.05.15 for New functions
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_moveall_start_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_moveall_start_mode(ushort RingNo, ushort DeviceIP, ushort StartMode, ushort start_FallorRise);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_moveall_start_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_moveall_start_mode(ushort RingNo, ushort DeviceIP, ref ushort StartMode,ref ushort start_FallorRise);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_moveall_stop_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_moveall_stop_mode(ushort RingNo, ushort DeviceIP, ushort StopMode, ushort stop_FallorRise);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_moveall_stop_mode", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_moveall_stop_mode(ushort RingNo, ushort DeviceIP, ref ushort StopMode, ref ushort stop_FallorRise);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_pulsext_time", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_pulsext_time(ushort RingNo, ushort DeviceIP, ushort AxisNo, uint data);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_pulsext_time", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_pulsext_time(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref uint data);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_ltcbuffer", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_ltcbuffer(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort ltcbuf_en, ushort ltc_mode, ushort ltc_logic, ushort ltcdif_en, ushort ltc_src, ushort buf_size);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_ltcbuffer_cnt", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_ltcbuffer_cnt(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref ushort ltcbuf_cnt);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_ltcbuffer_status", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_ltcbuffer_status(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref ushort ltcbuf_en, ref ushort ltc_mode, ref ushort ltc_logic, ref ushort ltcdif_en, ref ushort ltc_src, ref ushort full_flg);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_read_ltcbuffer_start", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_read_ltcbuffer_start(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort startaddr, ushort read_cnt);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_read_ltcbuffer_rate", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_read_ltcbuffer_rate(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref ushort read_rate);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_read_ltcbuffer_finish", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_read_ltcbuffer_finish(ushort RingNo, ushort DeviceIP, ushort AxisNo, int[] ltcbuf);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_clr_ltcbuffer", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_clr_ltcbuffer(ushort RingNo, ushort DeviceIP, ushort AxisNo);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_ipo_v_change", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_ipo_v_change(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, double[] NewVelArray, double[] TimeArray, ushort Count);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_ipo_v_change", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_ipo_v_change(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, double[] NewVelArray, double[] TimeArray, ushort Count);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_cwccw_dir", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_cwccw_dir(ushort RingNo, ushort DeviceIP,ushort AxisNo,ushort Dir);
        [DllImport("AMONet.dll",
          EntryPoint = "_mnet_get_error_table",
          CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_get_error_table(ushort RingNo, uint[] ErrorTable);

        [DllImport("AMONet.dll",
            EntryPoint = "_mnet_get_ring_cyclic_time",
             CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_get_ring_cyclic_time(ushort RingNO, ref double Time);

        [DllImport("AMONet.dll",
          EntryPoint = "_mnet_enable_soft_watchdog_ex",
            CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_enable_soft_watchdog_ex(ushort RingNO, ref ushort Flag, uint MillionSecond);

        [DllImport("AMONet.dll",
             EntryPoint = "_mnet_set_ring_config",
             CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_set_ring_config(ushort RingNO, ushort BaudRate);
        [DllImport("AMONet.dll",
             EntryPoint = "_mnet_reset_ring",
             CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_reset_ring(ushort RingNo);
        [DllImport("AMONet.dll",
             EntryPoint = "_mnet_get_ring_active_table",
             CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_get_ring_active_table(ushort RingNo, uint[] DevTable);
        [DllImport("AMONet.dll", 
            EntryPoint = "_mnet_get_cpld_version", 
            CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_get_cpld_version(ushort RingNo, ushort DeviceIP, ref uint CpldVersion);
        [DllImport("AMONet.dll",
            EntryPoint = "_mnet_io_channel_output",
             CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_io_channel_output(ushort RingNo, ushort DeviceIP, ushort Channel, ushort OutData);
        [DllImport("AMONet.dll",
           EntryPoint = "_mnet_io_channel_input",
             CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_io_channel_input(ushort RingNo, ushort DeviceIP, ushort Channel, ref ushort InData);
        [DllImport("AMONet.dll",
         EntryPoint = "_mnet_io_memory_output",
            CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_io_memory_output(ushort RingNo, ref uint data_out_array);
        [DllImport("AMONet.dll",
         EntryPoint = "_mnet_io_memory_input",
            CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_io_memory_input(ushort RingNo, ref uint data_in_array);
        [DllImport("AMONet.dll",
            EntryPoint = "_mnet_get_last_error", 
            CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_get_last_error(ushort RingNo, ref ulong LastError);
        [DllImport("AMONet.dll",
            EntryPoint = "_mnet_m1_dio_support",
            CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m1_dio_support(ushort RingNo, ushort DeviceIP, ref ushort NorY);
        [DllImport("AMONet.dll",
            EntryPoint = " _mnet_m4_set_emg_reaction",
            CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_emg_reaction(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort emg_mode);
        [DllImport("AMONet.dll", 
            EntryPoint = " _mnet_m4_get_comparator_level", 
            CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_comparator_level(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref ushort Level);
        [DllImport("AMONet.dll", 
            EntryPoint = "_mnet_m4_set_path_dwell_data", 
            CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_path_dwell_data(ushort RingNo, ushort DeviceIP, ushort[] AxisArray, ushort ArraySize, ushort CmdFunc, double DelayTime, ushort EnableCMPOut);
        [DllImport("AMONet.dll",
            EntryPoint = "_mnet_m4_set_moveall_method",
            CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_moveall_method(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort StartMethod, ushort StopMethod, ushort ErrStopEnable);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_start_absread", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_start_absread(ushort RingNo, ushort DeviceIP, ushort AxisNo);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_check_absread", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_check_absread(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref ushort Status);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_absread", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_absread(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref int Position);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_ai_scale", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_ai_scale(ushort RingNo, ushort DeviceIP, ushort usChan, ushort reading, ref double analog);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_ao_scale", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_ao_scale(ushort RingNo, ushort DeviceIP, ushort usChan, float analog, ref ushort writing);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_tr_move_xy", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_tr_move_xy(ushort RingNo, ushort DeviceIP, int DistX, int DistY, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_sr_move_xy", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_sr_move_xy(ushort RingNo, ushort DeviceIP, int DistX, int DistY, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_ta_move_xy", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_ta_move_xy(ushort RingNo, ushort DeviceIP, int PosX, int PosY, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_start_sa_move_xy", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_start_sa_move_xy(ushort RingNo, ushort DeviceIP, int PosX, int PosY, double StrVel, double MaxVel, double Tacc, double Tdec);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_start_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_start_all(ushort RingNo);
        [DllImport("AMONet.dll", EntryPoint = "_mnet_m2_gp_stop_all", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m2_gp_stop_all(ushort RingNo);

        //[DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_moveall_mode", CallingConvention = CallingConvention.StdCall)]
        //public static extern short _mnet_m4_set_moveall_mode(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort mode, ushort RiseorFall);
        //[DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_moveall_mode", CallingConvention = CallingConvention.StdCall)]
        //public static extern short _mnet_m4_get_moveall_mode(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref ushort mode, ref ushort RiseorFall);

        [DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_INStop_enable", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_INStop_enable(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort enable);
	[DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_INStop_react", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_INStop_react(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort react);
	[DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_INStop_source", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_INStop_source(ushort RingNo, ushort DeviceIP, ushort AxisNo, ushort source);
	[DllImport("AMONet.dll", EntryPoint = "_mnet_m4_set_INStop_offset", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_set_INStop_offset(ushort RingNo, ushort DeviceIP, ushort AxisNo, int offset);
	[DllImport("AMONet.dll", EntryPoint = "_mnet_m4_get_INStop_offset", CallingConvention = CallingConvention.StdCall)]
        public static extern short _mnet_m4_get_INStop_offset(ushort RingNo, ushort DeviceIP, ushort AxisNo, ref int offset);
    }
}
