using AutoTestSystem.Equipment.Motion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTestSystem.Base
{
    public abstract class MotionBase : Manufacture.Equipment, IDisposable
    {
        public enum MOTION_MOVE_STATUS
        {
            NOTYET,
            MOVING,
            MOVEDONE,

        }

        public enum MOTION_MOTOR
        {
            Stepper_Motor,
            Servo_Motor

        }

        [JsonIgnore]
        [Browsable(false)]
        public string ErrorMessage { get; set; } = string.Empty;
        public abstract void Dispose();

        public abstract bool Init(string strParamInfo);

        public abstract bool StartAction(string strItemName, string strParamIn, ref string strOutput);
        public abstract bool ServoON();
        public abstract bool ServoOFF();
        public abstract bool SetCommandPos(double in_pos);
        public abstract bool GetCommandPos(ref double out_pos);
        public abstract bool EmgStop();
        public abstract bool SdStop();
        public abstract bool Pause();
        public abstract bool SyncHome(double in_start_vel, double in_max_vel, int Dir, int Timeout);
        public virtual bool SyncMotionDone()
        {
            throw new NotImplementedException();
        }

        public virtual bool SyncResetHomeDone()
        {
           throw new NotImplementedException();
        }
        public abstract bool Check_IO_StartStatus(Dictionary<int, int> Devices_IO_Status);
        public abstract bool Relative_Move(double value, double in_start_vel, double in_max_vel, double tacc, double dac);
        public abstract bool Absolute_Move(double value, double in_start_vel, double in_max_vel, double tacc, double dac);

        public virtual bool Relative_Move(string input)
        {
            throw new NotImplementedException();
        }
        public virtual bool Absolute_Move(string inptu)
        {
            throw new NotImplementedException();
        }
        public virtual bool Recieve_MotionDone(ref string output,int timeout)
        {
            throw new NotImplementedException();
        }
        public virtual bool SyncHome()
        {
            throw new NotImplementedException();
        }


        /*public virtual bool MotionStatus(int Bitposition, ref int status)
        {
            throw new NotImplementedException();
        }*/

        public virtual void GetMotionStatus(ref int status)
        { 
            throw new NotImplementedException();
        }
        public virtual bool MotionDone(ref int status)
        {
            throw new NotImplementedException();
        }
        /*public virtual bool MotionParamSet(ResetDirection Dir, SpecifyLocation Specify_location, ResetMode Mode, double Home_Position, double Stop_Position, int Stop_vel_H, int Stop_vel_L, int Acc, int Tcc)
        {
            throw new NotImplementedException();
        }*/

        public virtual bool HomeDone(ref int status)
        {
            throw new NotImplementedException();
        }

        public virtual string GetErrorMessage()
        {
            throw new NotImplementedException();
        }
        
        public virtual bool Broadcast(string writeData)
        {
            throw new NotImplementedException();
        }
        public virtual bool PostionLimitSet(double PositiveLimit, double NegativeLimit)
        {
            throw new NotImplementedException();
        }

        public virtual bool Trigger(TriggerMode CTRG, string HexAddress,string HexData, ref string Rec_Data)
        {
            throw new NotImplementedException();
        }
        public virtual bool GetCerrentPos(ref double out_pos)
        {
            throw new NotImplementedException();
        }
        public virtual bool GetCurrentPos(ref double out_pos)
        {
            throw new NotImplementedException();
        }
        public virtual bool Get_IO_Status(ref int status , ushort port_num )
        {
            throw new NotImplementedException();
        }
        public virtual bool Get_IO_Status(ref ushort[] IORecAll)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetGet_IO(string port_num, string status,ref ushort[] IORecAll)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetGet_IO(ushort port_num, int status, ref int output_status)
        {
            throw new NotImplementedException();
        }

        public virtual bool ReadParamFile(string strSavePath)
        {
            throw new NotImplementedException();
        }

        public virtual bool CheckConnect()
        {
            throw new NotImplementedException();
        }
        public virtual bool ReleaseConnect()
        {
            throw new NotImplementedException();
        }
        public virtual bool readRegisterToFile(string header, int startAddress, int count, string filePath, int format, uint dataLength)
        {
            throw new NotImplementedException();
        }
        public virtual bool writeRegisterFromFile(string filePath, uint dataLength)
        {
            throw new NotImplementedException();
        }
        public virtual bool readFromRegister(string header, int num, uint dataLength , bool other_format,ref double Output_Val)
        {
            throw new NotImplementedException();
        }
        public virtual bool writeToRegister(string header, int num, string regData_str, uint dataLength, bool other_format)
        {
            throw new NotImplementedException();
        }
        public enum ResetMode
        {
            Origin_Reset,
            Limit_Reset,
            Z_Reset,
            Torque_Reset,
            Immediately_Reset
        }
        public enum ResetDirection
        {
            Positive,
            Negative
        }

        public enum Z_Signal
        {
            ON,
            OFF
        }
        public enum TriggerMode
        {
            RS485_Trigger,
            Register_Trigger,
            IOTrigger
        }

        public class DIOEditor : System.Drawing.Design.UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                using (var DIODialog = new Motion_DIOparam())
                {
                    if (value == null)
                        value = string.Empty;

                    DIODialog.SetParam(value.ToString());
                    if (DIODialog.ShowDialog() == DialogResult.OK)
                    {
                        return DIODialog.GetParam();
                    }
                    else
                    {
                        MessageBox.Show($"The DIO param key or value exist \"Empty\",Please Check DIOparam From Setting", "SetDIOparam Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                return value; // 如果用戶取消選擇，返回原始值
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
        }
    }
}
