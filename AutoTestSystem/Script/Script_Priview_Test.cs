
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using OpenCvSharp;

using System.Drawing;
using AutoTestSystem.Base;

namespace AutoTestSystem.Script
{
    internal class Script_Priview_Test : Script_CCD_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;


        //Message message_param = null;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess(string ActionItem, string Paraminput)
        {

            strActItem = ActionItem;
            strParam = Paraminput;

            //message_param = JsonConvert.DeserializeObject<Message>(strParam);

            return true;
        }
        public override bool Process(CCDBase CCD)
        {
            //MessageBox.Show(message_param.Content, message_param.Title);
            //Mat image = new Mat();
            // image = Cv2.ImRead("sfr.jpg", ImreadModes.Color);
            //Cv2.ImShow("test", image);
            //Cv2.WaitKey(0);
            CCDBase.ImageData test1;

            
            return true;
        }
        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            
            return true;

        }




        //public class Message
        //{
           
        //    public string Title { get; set; }
        //    public string Content { get; set; }

        //}

    }
}
