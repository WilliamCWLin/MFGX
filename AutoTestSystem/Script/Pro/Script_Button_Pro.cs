
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;
using System.Windows.Forms;
using System.ComponentModel;

namespace AutoTestSystem.Script
{

    internal class Script_Button_Pro : Script_Extra_Base
    {
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(ButtonActionList))]
        public string ButtonAction { get; set; } = "Press";

        JArray Button_data;
        Button Button_param = new Button();

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess()
        {
            if (PopMoreData("Button_data") == null || PopMoreData("Button_data") == string.Empty)
            {
                LogMessage("Button_data can not be null.", MessageLevel.Error);
                return false;
            }
            else
                Button_data = JArray.Parse(PopMoreData("Button_data"));

            return true;
        }
        
        public override bool Process(ref string output)
        {
            Button_param.Zone = new string[Button_data.Count];
            Button_param.Result = new int[Button_data.Count];
            Button_param.Value = new int[Button_data.Count];

            for (int i = 0; i < Button_data.Count; i++)
            {
                Button_param.Zone[i] = (string)Button_data[i]["zone"];
                Button_param.Result[i] = (int)Button_data[i]["result"];
                Button_param.Value[i] = (int)Button_data[i]["button_level"];
            }

            output = ProcessData();
            strOutData = output;

            return true;
        }

        public override bool PostProcess()
        {
            PushMoreData("Button_data", string.Empty);
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
            {
                return true;
            }
            else
            {
                LogMessage($"{result}", MessageLevel.Error);
                return false;
            }

        }

        public string ProcessData()
        {
            var data = new Dictionary<string, object> { };
            
            switch (ButtonAction)
            {
                case "Press":
                    if (Button_param.Result.All(x => x == 1))
                    {
                        data.Add("errorCode", "0");
                    }  
                    else
                    {
                        data.Add("errorCode", "-1");
                        LogMessage($"At least one button is not Pressed .Result->Fail", MessageLevel.Warn);
                    }
                    break;

                case "Release":
                    if (Button_param.Result.All(x => x == 0))
                    {
                        data.Add("errorCode", "0");
                    }
                    else
                    {
                        data.Add("errorCode", "-1");
                        LogMessage($"At least one button is not Released .Result->Fail", MessageLevel.Warn);
                    }
                    break;
            }

            for (int i = 0; i < Button_data.Count; i++)
            {
                data.Add($"Button[{Button_param.Zone[i]}]", Button_param.Result[i].ToString());
            }

            string output = JsonConvert.SerializeObject(data);
            LogMessage($"Read END:  {output}");

            return output;

        }

        public class Button
        {
            public string[] Zone { get; set; }
            public int[] Result { get; set; }
            public int[] Value { get; set; }
        }

    }

    public class ButtonActionList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> typeList = new List<string>() { "Press", "Release", };
            
            return new StandardValuesCollection(typeList);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
