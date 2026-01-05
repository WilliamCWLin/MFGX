using System;
using System.Windows.Forms;
using System.Threading;
using AutoTestSystem.DAL;
using System.Text.RegularExpressions;
using static AutoTestSystem.BLL.Bd;
using AutoTestSystem.Base;
using System.ComponentModel;
using Manufacture;
using System.Drawing.Design;
using System.Reflection;

namespace AutoTestSystem.DUT
{
    public class DUT_Simu2 : DUT_BASE
    {
        public DUT_Simu2() 
        {
            UpdateBrowsableAttributes();
        }

        public enum Mode
        {
            Mode1,
            Mode2,
            Mode3
        }
        private Mode _mode;


        [Category("Parameter"), Description("Mode")]
        public Mode Action
        {
            get { return _mode; }
            set
            {
                _mode = value;
                UpdateBrowsableAttributes();
            }
        }

        [Browsable(true)]
        [Category("Params"), Description("自訂顯示名稱"), Editor(typeof(JsonEditor), typeof(UITypeEditor)), DynamicBrowsable(false)]
        public string Data { get; set; }


        [Browsable(true)]
        [Category("Params"), Description("自訂顯示名稱"), Editor(typeof(JsonEditor), typeof(UITypeEditor)), DynamicBrowsable(false)]
        public string Data2 { get; set; }



        private void UpdateBrowsableAttributes()
        {

            switch (Action)
            {
                case Mode.Mode1:
                    PropertyHelper.SetBrowsable(this, nameof(Data), true);
                    PropertyHelper.SetBrowsable(this, nameof(Data2), false);
                    break;
                case Mode.Mode2:
                    PropertyHelper.SetBrowsable(this, nameof(Data), false);
                    PropertyHelper.SetBrowsable(this, nameof(Data2), true);
                    break;
                case Mode.Mode3:
                    PropertyHelper.SetBrowsable(this, nameof(Data), true);
                    PropertyHelper.SetBrowsable(this, nameof(Data2), true);
                    break;
            }

            PropertyHelper.UpdateBrowsableAttributes(this);
        }

        public override void Dispose()
        {
            return;
        }

        public override bool Init(string strParamInfo)
        {
            return true;
        }

        public override bool SEND(string input)
        {
            return true;
        }

        public override bool SEND(byte[] input)
        {
            return true;
        }

        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            return true;
        }

        public override bool UnInit()
        {
            return true;
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class DynamicBrowsableAttribute : Attribute
    {
        public bool IsBrowsable { get; set; }

        public DynamicBrowsableAttribute(bool isBrowsable)
        {
            IsBrowsable = isBrowsable;
        }
    }

    public static class PropertyHelper
    {
        public static void SetBrowsable(object obj, string propertyName, bool isBrowsable)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(obj)[propertyName];
            BrowsableAttribute browsable = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
            FieldInfo isBrowsableField = browsable.GetType().GetField("browsable", BindingFlags.NonPublic | BindingFlags.Instance);
            isBrowsableField.SetValue(browsable, isBrowsable);
            TypeDescriptor.Refresh(obj);
        }

        public static void UpdateBrowsableAttributes(object obj)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
            foreach (PropertyDescriptor property in properties)
            {
                DynamicBrowsableAttribute attribute = (DynamicBrowsableAttribute)property.Attributes[typeof(DynamicBrowsableAttribute)];
                if (attribute != null)
                {
                    BrowsableAttribute browsable = (BrowsableAttribute)property.Attributes[typeof(BrowsableAttribute)];
                    FieldInfo isBrowsableField = browsable.GetType().GetField("browsable", BindingFlags.NonPublic | BindingFlags.Instance);
                    isBrowsableField.SetValue(browsable, attribute.IsBrowsable);
                }
            }
            TypeDescriptor.Refresh(obj);
        }
    }
}
