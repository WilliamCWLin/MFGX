using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.DevicesUI.Teach;
using AutoTestSystem.DevicesUI.IO;
using AutoTestSystem.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;

namespace AutoTestSystem.Equipment.Teach
{
    public class TCPTeach : TeachBase, IDisposable
    {
        [Category("Select TCP Devices")]
        [Description("編輯路徑清單")]
        [Editor(typeof(SelectTCPDevices), typeof(UITypeEditor))]
        public List<string> SelectedDevices { get; set; } = new List<string>();


        public override bool Init(string jsonParam)
        {            
            return true;
        }

        public override bool UnInit()
        {
            return true;
        }

        public override bool Show()
        {
            using (var form = new TCPIOViewerForm(this))
            {
                form.ShowDialog();
            }

            return true;
        }
        private bool EnsureDevicesSelected()
        {
            if (SelectedDevices.Any()) return true;

            using (var dlg = new DeviceSelectionForm(SelectedDevices))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    SelectedDevices = dlg.GetSelectedKeys();
                }
            }
            if (!SelectedDevices.Any())
            {
                MessageBox.Show("請至少選擇一個TCP連線再進行教學", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        protected override string GetJsonParamString()
        {
            throw new NotImplementedException();
        }

    }
    public class SelectTCPDevices : System.Drawing.Design.UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var TCPTeach = context.Instance as TCPTeach;

            using (var dlg = new DeviceSelectionForm(TCPTeach.SelectedDevices))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    return dlg.GetSelectedKeys();
                }
            }

            return TCPTeach.SelectedDevices; // 如果用戶取消選擇，返回原始值
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
