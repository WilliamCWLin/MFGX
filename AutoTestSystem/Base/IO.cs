using Automation.BDaq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.ComponentModel.TypeConverter;

namespace AutoTestSystem.Base
{
    public abstract class IOBase : Manufacture.Equipment, IDisposable
    {
        [Category("IO Parameters"), Description("IO Setting"), Editor(typeof(IOParamList), typeof(System.Drawing.Design.UITypeEditor))]
        public string SetIO_List { get; set; } = "";

        [Category("IO Parameters"), Description("IO Setting"), Editor(typeof(IOParamList), typeof(System.Drawing.Design.UITypeEditor))]
        public string GetIO_List { get; set; } = "";

        [Category("IO Parameters(Instant Analog Input)"), Description("AIO Setting"), Editor(typeof(IOParamList), typeof(System.Drawing.Design.UITypeEditor))]
        public string SetAIO_List { get; set; } = "";

        [Category("IO Parameters(Instant Analog Input)"), Description("AIO Setting"), Editor(typeof(IOParamList), typeof(System.Drawing.Design.UITypeEditor))]
        public string GetAIO_List { get; set; } = "";


        [Category("IO Parameters"), Description("Select IO ChannelCount")]
        public int ChannelCount { get; set; } = 16;

        public abstract void Dispose();

        public abstract bool Init(string strParamInfo);
        public virtual bool SETIO(int portNum, int bit, bool output)
        {
            return true;
        }

        public virtual bool SETIO(int bit, bool output)
        {
            return true;
        }

        public virtual bool GETDO(int portNum, int pos, ref bool status)
        {
            return true;
        }
        public virtual bool GETALLDO(ref bool[] status)
        {
            return true;
        }
        public virtual bool GETALLIO(ref bool[] status)
        {
            return true;
        }
        public virtual bool GETIO(int pos, ref bool status)
        {
            return true;
        }
        public virtual bool GETIO(int portNum, int pos, ref bool status)
        {
            return true;
        }
        public virtual bool WaveAiCtrl(string strParam, ref string Dataout)
        {
            return true;
        }
        public abstract bool InstantAI(string strSavePath, ref string Dataout);
        public virtual bool InstantAI(int channel, ref string Dataout)
        {
            return true;
        }

        public virtual bool GetAll_AI(ref string Dataout)
        {
            return true;
        }
        public virtual bool SetAll_AO(ref string Dataout)
        {
            return true;
        }
        public virtual bool InstantAO(int channel, double volt, ref string Dataout)
        {
            return true;
        }

        public virtual Dictionary<string, string> GetDIForm()
        {
            IOSetting IODialog = new IOSetting();
            return IODialog.ConvertDictionaryForm(GetIO_List);
        }
        public virtual Dictionary<string, string> GetDOForm()
        {
            IOSetting IODialog = new IOSetting();
            return IODialog.ConvertDictionaryForm(SetIO_List);
        }

    }
    public class InstantDoCtrlDeviceList : TypeConverter  //下拉式選單
    {
        public static readonly InstantDoCtrl DoCtrl = new InstantDoCtrl();
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (DoCtrl.SupportedDevices.Count > 0)
            {
                List<string> hwList = new List<string>();
                foreach (DeviceTreeNode node in DoCtrl.SupportedDevices)
                {
                    if (node.Description.Contains("Demo") == false)
                        hwList.Add(node.Description);
                }

                return new StandardValuesCollection(hwList.ToArray());
            }
            else
            {
                return new StandardValuesCollection(new int[] { 0 });
            }
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }


    public class InstantDiCtrlDeviceList : TypeConverter  //下拉式選單
    {
        public static readonly InstantDiCtrl DiCtrl = new InstantDiCtrl();
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {

            if (DiCtrl.SupportedDevices.Count > 0)
            {
                List<string> hwList = new List<string>();
                foreach (DeviceTreeNode node in DiCtrl.SupportedDevices)
                {
                    if (node.Description.Contains("Demo") == false)
                        hwList.Add(node.Description);
                }

                return new StandardValuesCollection(hwList.ToArray());
            }
            else
            {
                return new StandardValuesCollection(new int[] { 0 });
            }
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
       
    }

    public class IOParamList : System.Drawing.Design.UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            using (var IODialog = new IOSetting())
            {
                if (value == null)
                    value = string.Empty;

                IODialog.SetIOForm_JSON(value.ToString());
                if (IODialog.ShowDialog() == DialogResult.OK)
                {
                    //IODialog.ConvertDictionaryForm(IODialog.GetIOForm_JSON());
                    return IODialog.GetIOForm_JSON();
                }
                else
                {
                    MessageBox.Show($"The ModBusIO param key or value exist \"Empty\",Please Check ModBusIO param From Setting", "SetDIOparam Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return value; // 如果用戶取消選擇，返回原始值
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }

    public class BindDataMessage
    {
        public string ch { get; set; }
        public string loopCount { get; set; }

        public string interval_ms { get; set; }

    }
}
