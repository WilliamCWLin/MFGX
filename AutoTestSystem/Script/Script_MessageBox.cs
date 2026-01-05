
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_MessageBox : Script_Extra_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        Messagebox_show frm = new Messagebox_show();


        Message message_param = null;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess(string ActionItem, string Paraminput)
        {

            strActItem = ActionItem;
            strParam = Paraminput;

            message_param = JsonConvert.DeserializeObject<Message>(strParam);

            return true;
        }
        public override bool Process()
        {
            //MessageBox.Show(message_param.Content, message_param.Title);
            int ret;
            frm.SetLabelText(message_param.Content, message_param.Title, message_param.Sataus);
            ret = (int)frm.ShowDialog();
            if(ret != 1)
                return false;

            return true;
        }
        public override bool PostProcess()
        {
            
            return true;

        }




        public class Message
        {

            public string Title { get; set; }
            public string Content { get; set; }

            public int Sataus { get; set; }

        }

    }
}
