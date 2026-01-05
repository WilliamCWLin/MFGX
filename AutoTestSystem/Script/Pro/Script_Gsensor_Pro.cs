
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

    internal class Script_Gsensor_Pro : Script_Extra_Base
    {
        string strOutData = string.Empty;
        uint OUT_X_L, OUT_X_H, OUT_Y_L, OUT_Y_H, OUT_Z_L, OUT_Z_H;

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess()
        {
            bool pass_fail = true;

            if (PopMoreData("Gsensor_X_L") == null || PopMoreData("Gsensor_X_L") == string.Empty)
            {
                LogMessage("Gsensor_X_L can not be null.", MessageLevel.Error);
                pass_fail =  false;
            }
            else
                OUT_X_L = Convert.ToUInt32(PopMoreData("Gsensor_X_L"), 16);

            if (PopMoreData("Gsensor_X_H") == null || PopMoreData("Gsensor_X_H") == string.Empty)
            {
                LogMessage("Gsensor_X_H can not be null.", MessageLevel.Error);
                pass_fail = false;
            }
            else
                OUT_X_H = Convert.ToUInt32(PopMoreData("Gsensor_X_H"), 16);

            if (PopMoreData("Gsensor_Y_L") == null || PopMoreData("Gsensor_Y_L") == string.Empty)
            {
                LogMessage("Gsensor_Y_L can not be null.", MessageLevel.Error);
                pass_fail = false;
            }
            else
                OUT_Y_L = Convert.ToUInt32(PopMoreData("Gsensor_Y_L"), 16);

            if (PopMoreData("Gsensor_Y_H") == null || PopMoreData("Gsensor_Y_H") == string.Empty)
            {
                LogMessage("Gsensor_Y_H can not be null.", MessageLevel.Error);
                pass_fail = false;
            }
            else
                OUT_Y_H = Convert.ToUInt32(PopMoreData("Gsensor_Y_H"), 16);

            if (PopMoreData("Gsensor_Z_L") == null || PopMoreData("Gsensor_Z_L") == string.Empty)
            {
                LogMessage("Gsensor_Z_L can not be null.", MessageLevel.Error);
                pass_fail = false;
            }
            else
                OUT_Z_L = Convert.ToUInt32(PopMoreData("Gsensor_Z_L"), 16);

            if (PopMoreData("Gsensor_Z_H") == null || PopMoreData("Gsensor_Z_H") == string.Empty)
            {
                LogMessage("Gsensor_Z_H can not be null.", MessageLevel.Error);
                pass_fail = false;
            }
            else
                OUT_Z_H = Convert.ToUInt32(PopMoreData("Gsensor_Z_H"), 16);

            return pass_fail;
        }
        
        public override bool Process(ref string output)
        {
            double X, Y, Z;
            var data = new Dictionary<string, object> { };
            
            try
            {
                if (OUT_X_H >= 0b_1000_0000)
                    X = TwosComplement(OUT_X_H << 8 | OUT_X_L) * 0.244 / 4000;
                else
                    X = (OUT_X_H << 8 | OUT_X_L) * 0.244 / 4000;

                if (OUT_Y_H >= 0b_1000_0000)
                    Y = TwosComplement(OUT_Y_H << 8 | OUT_Y_L) * 0.244 / 4000;
                else
                    Y = (OUT_Y_H << 8 | OUT_Y_L) * 0.244 / 4000;

                if (OUT_Z_H >= 0b_1000_0000)
                    Z = TwosComplement(OUT_Z_H << 8 | OUT_Z_L) * 0.244 / 4000;
                else
                    Z = (OUT_Z_H << 8 | OUT_Z_L) * 0.244 / 4000;

                data.Add("errorCode", "0");
                data.Add("x_axis", X.ToString("0.000"));
                data.Add("y_axis", Y.ToString("0.000"));
                data.Add("z_axis", Z.ToString("0.000"));
            }
            catch (Exception)
            {
                data.Add("errorCode", "-1");
            }

            output = JsonConvert.SerializeObject(data);
            strOutData = output;

            return true;
        }

        public override bool PostProcess()
        {      
            PushMoreData("Gsensor_X_L", string.Empty);
            PushMoreData("Gsensor_X_H", string.Empty);
            PushMoreData("Gsensor_Y_L", string.Empty);
            PushMoreData("Gsensor_Y_H", string.Empty);
            PushMoreData("Gsensor_Z_L", string.Empty);
            PushMoreData("Gsensor_Z_H", string.Empty);            
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS")
                return true;
            else
                return false;

        }

        static int TwosComplement(uint input)
        {
            int output = -((int)(input ^ Convert.ToUInt32(0xFFFF)) + 1);

            return output;
        }

    }
}
