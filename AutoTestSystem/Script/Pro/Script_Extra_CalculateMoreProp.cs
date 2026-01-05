
using AutoTestSystem.Model;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_CalculateMoreProp : Script_Extra_Base
    {
        [Category("Common Parameters"), Description("Calculate Content"), Editor(typeof(CommandEditor_MakeWriteLine), typeof(UITypeEditor))]
        public string content { get; set; } = "";

        public class CommandEditor_MakeWriteLine : UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                using (Form form = new Form())
                {
                    TextBox textBox = new TextBox
                    {
                        Multiline = true,
                        Dock = DockStyle.Fill,
                        Text = value?.ToString().Replace("\\n", Environment.NewLine)
                    };
                    form.Controls.Add(textBox);
                    form.ShowDialog();
                    return textBox.Text.Replace(Environment.NewLine, "\\n");
                }
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
        }

        public string parameter;

        public string strstringoutput = "";

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {

            return true;
        }
        public override bool Process(ref string strOutData)
        {
            Dictionary<string, string> outputdata = new Dictionary<string, string>();
            StringBuilder tempstrOutData = new StringBuilder ("");
            LogMessage("========CalculateMoreProp========");
            LogMessage(String.Format("Output Original Content: {0}", content));
            string oricontent = content.Replace("\\n", "\n");
            string[] segments = oricontent.Split('\n');
            foreach (string segment in segments)
            {
                string[] parts = segment.Split('=');
                parameter = parts[0];
                MatchCollection matches = Regex.Matches(parts[1], @"%([^%]+)%");
                string transcontent = parts[1];
                foreach (Match match in matches)
                {
                    string variable = match.Groups[1].Value;
                    string value = PopMoreData(variable);
                    if (value == "")
                    {
                        LogMessage(String.Format("Parameter: %{0}% is Empty !!", variable), MessageLevel.Error);
                        return false;
                    }
                    transcontent = transcontent.Replace($"%{variable}%", value);
                }

                //string fulltranscontent = parameter + '=' + transcontent;
                //LogMessage(String.Format("Output Translate Content: {0}", fulltranscontent));
                double result = EvaluateExpression(transcontent);

                string strresult = result.ToString();
                string fulltranscontent = strresult + '=' + transcontent;
                LogMessage(String.Format("Output Translate Content: {0}", fulltranscontent));

                outputdata.Add(parameter, strresult);
             

                bool ret = PushMoreData(parameter, strresult);
            }
            //JObject json = new JObject();
            //String jsonStr = json.ToString();
            String jsonStr = JsonConvert.SerializeObject(outputdata, Formatting.Indented);

            strOutData = jsonStr;
            strstringoutput = strOutData;


            //string ret = PopMoreData(content);
            //MessageBox.Show(ret);
            //if (ret)
            //{
            //    JObject json = new JObject();
            //    json[Key] = ReplaceValue;
            //    strOutData = json.ToString();
            //}             
            //else
            //{
            //    JObject json = new JObject();
            //    json["Err"] = $"Set {Key} = {ReplaceValue} Fail.";
            //    strOutData = json.ToString();

            //}

            return true;
        }
        public override bool PostProcess()
        {
            string result = CheckRule(strstringoutput, Spec);
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
        public static double EvaluateExpression(string expression)
        {
            DataTable table = new DataTable();
            table.Columns.Add("expression", string.Empty.GetType(), expression);
            DataRow row = table.NewRow();
            table.Rows.Add(row);
            return double.Parse((string)row["expression"]);
        }


    }
}
