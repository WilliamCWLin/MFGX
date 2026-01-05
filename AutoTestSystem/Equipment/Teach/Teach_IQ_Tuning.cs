using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace AutoTestSystem.Equipment.Teach
{

    class Teach_IQ_Tuning : TeachBase
    {
        [Category("Teach Form"), Description("JSON Format"), Editor(typeof(TeachView), typeof(UITypeEditor))]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TeachValue { get; set; } = "{X_ratio:0,Y_ratio:0}";


        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            return true;
        }


        public override bool UnInit()
        {
            return true;
        }
        public override T GetParametersFromJson<T>()
        {
            try
            {
                return base.GetParametersFromJson<T>();
            }
            catch
            {
                return null;
            }
        }

        protected override string GetJsonParamString()
        {
            return TeachValue;
        }

        public class IQParameters
        {
            public double X_ratio { get; set; }
            public double Y_ratio { get; set; }
            public string SE_PIN { get; set; }
            // 添加其他參數
        }
        public class TeachView : UITypeEditor
        {
            public string originalText;

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                var serializedData = JsonConvert.DeserializeObject<Dictionary<string, string>>(value.ToString());

                using (View form = new View())
                {
                    form.strJsonResult = value.ToString();
                    if (serializedData.TryGetValue("X_ratio", out var xRatio))
                    {
                        form.X_ratio_box.Text = (string)xRatio;
                    }
                    else
                    {
                        form.X_ratio_box.Text = ""; // 或者你想要的默認值
                    }

                    if (serializedData.TryGetValue("Y_ratio", out var yRatio))
                    {
                        form.Y_ratio_box.Text = (string)yRatio;
                    }
                    else
                    {
                        form.Y_ratio_box.Text = ""; // 或者你想要的默認值
                    }

                    if (serializedData.TryGetValue("SE_PIN", out var sePin))
                    {
                        form.PIN_RichTextbox.Text = (string)sePin;
                        form.PIN_Data = (string)sePin;
                    }
                    else
                    {
                        form.PIN_RichTextbox.Text = ""; // 或者你想要的默認值
                    }
                    //form.ShowDialog();
                    //form.Uninitialize();
                    IWindowsFormsEditorService editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

                    if (editorService != null)
                    {
                        DialogResult result = editorService.ShowDialog(form);

                        if (result == DialogResult.Cancel)
                        {
                            return form.strJsonResult;
                        }
                    }
                    return value;
                }

            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
        }
    }


}
