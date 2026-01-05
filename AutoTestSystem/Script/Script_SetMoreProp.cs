
using AutoTestSystem.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_SetMoreProp : Script_Tool_Base
    {
       
        string Send_data = string.Empty;
        string TmpKeyword = string.Empty;
        string str_ret = string.Empty;

        string[] Send_comment = null;
        float Calc = 0;
        string[] input_array = null;

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess(string keywordComment, string strParamInput)
        {                             
            return true;
        }
        public override bool Process(string keywordComment,string Type_mode)       
        {
            if (Type_mode == string.Empty)
                return true;
            int IntType_mode = int.Parse(Type_mode);
            if (IntType_mode == 0) //0_Write_Memory
            {
                Send_comment = keywordComment.Split(';');//
                int number = Send_comment.Length;
                for (int i = 0; i < number; i++)
                {
                    //int count = 0;
                    int start = -1;
                    string name = string.Empty;
                    string value = string.Empty;                   
                    Send_data = Send_comment[i];                   
                    PushMoreData(TmpKeyword + i, Send_data);
                    TmpKeyword = PopMoreData(TmpKeyword + i);
                    int size = TmpKeyword.Length;
                    string[] arr = Send_data.Split('=');
                    if (size > 0)
                    {
                        if (TmpKeyword.Contains("%"))
                        {
                            //TmpKeyword = replaceWithProp(TmpKeyword);
                            if(TmpKeyword == string.Empty)
                            {
                                Logger.Error($" ERROR!!! Input Item are not define or not Exit at System please check again!!");
                                return false;
                            }
                            size = TmpKeyword.Length;
                            string delimiter = "=";
                            start = Send_data.IndexOf(delimiter);
                            if (start >= 0)
                            {
                                name = Send_data.Substring(0, start);
                                value = TmpKeyword.Substring(0, size);
                            }else
                            {
                                Logger.Error($"Warning! the Data type incorrect!!!.");
                                return false;
                            }
                        }
                        else
                        {
                            name = arr[0];
                            value = arr[1];
                        }
                        if(name != string.Empty && value != string.Empty)
                            PushMoreData(name, value);
                        else
                        {
                            Logger.Warn($"Warning! the Data is Empty!!!.");
                            return false;
                        }

                    }
                    else
                    {
                        Logger.Error($" ERROR!!! Input Item are not define or not Exit at System please check again!!");
                        return false;
                    }
                }
            }
            else if (IntType_mode == 1)
            {                              
                int Count = 0;               
                string value2 = string.Empty;                
                PushMoreData(TmpKeyword, keywordComment);
                keywordComment.Replace(" ","");
                input_array = keywordComment.Split(',');
                Count = input_array.Count();
                if(Count !=4 && Count != 3)
                {
                    Logger.Debug($"Warning! Input String hasn't been defined {keywordComment}");
                    return false;
                }
                else
                {
                    if (input_array[0].Contains("%"))
                        //input_array[0] = replaceWithProp(input_array[0]);
                    if(input_array[2].Contains("%"))
                        //input_array[2] = replaceWithProp(input_array[2]);
                    if(input_array[0] == "" || input_array[2] == "")
                    {
                        Logger.Error($" ERROR!!! Input Item are not define or not Exit at System please check again!!");
                        return false;
                    }
                    if (input_array[1] == "ADD")
                    {
                        Calc = float.Parse(input_array[0]) + float.Parse(input_array[2]);
                        Logger.Info($"Calculation => {input_array[0]}{input_array[1]}{input_array[2]} = {Calc}");
                    }
                    if (input_array[1] == "SUB")
                    {
                        Calc = float.Parse(input_array[0]) - float.Parse(input_array[2]);
                        Logger.Info($"Calculation => {input_array[0]}{input_array[1]}{input_array[2]} = {Calc}");
                    }
                    if (input_array[1] == "MUL")
                    {
                        Calc = float.Parse(input_array[0]) * float.Parse(input_array[2]);
                        Logger.Info($"Calculation => {input_array[0]}{input_array[1]}{input_array[2]} = {Calc}");
                    }
                    if (input_array[1] == "DIV")
                    {
                        if (float.Parse(input_array[2]) != 0)
                        {
                            Calc = float.Parse(input_array[0]) / float.Parse(input_array[2]);
                            Logger.Info($"Calculation => {input_array[0]}{input_array[1]}{input_array[2]} = {Calc}");
                        }else
                        {
                            Logger.Warn($"Warning! This Div Type Can't Divided by Zero.");
                            return false;
                        }
                    }
                    if (input_array[1] == "Range")
                    {
                        if (input_array[3].Contains("%"))
                            //input_array[3] = replaceWithProp(input_array[3]);
                        if (input_array[3] == "")
                        {
                            Logger.Error($" ERROR!!! Input Item are not define or not Exit at System please check again!!");
                            return false;
                        }
                        if (float.Parse(input_array[0]) > float.Parse(input_array[2]) && float.Parse(input_array[0]) < float.Parse(input_array[3]))
                        {
                            str_ret = "Range-PASS";
                            Logger.Info($"Calculation =>{input_array[0]} is within the valid range .Result->PASS");
                        }
                        else
                        {
                            str_ret = "Range-FAIL";
                            Logger.Info($"Calculation =>{input_array[0]} is NOT within the valid range .Result->FAIL");
                            return false;
                        }
                    }
                    if (input_array[1] == ">")
                    {
                        if(float.Parse(input_array[0]) > float.Parse(input_array[2]))
                        {
                            str_ret = "GreaterThan-PASS";
                            Logger.Info($"Calculation =>{input_array[0]}{input_array[1]}{input_array[2]} .Result->PASS");
                        }
                        else
                        {
                            str_ret = "GreaterThan-FAIL";                            
                            Logger.Info($"Calculation =>{input_array[0]} is not Greater than {input_array[2]} .Result->FAIL");
                            return false;
                        }
                    }
                    if (input_array[1] == "<")
                    {
                        if (float.Parse(input_array[0]) < float.Parse(input_array[2]))
                        {
                            str_ret = "LessThan-PASS";
                            Logger.Info($"Calculation =>{input_array[0]}{input_array[1]}{input_array[2]} .Result->PASS");
                        }
                        else
                        {
                            str_ret = "LessThan-FAIL";                           
                            Logger.Info($"Calculation =>{input_array[0]} is not Less than {input_array[2]} .Result->FAIL");
                            return false;
                        }
                    }
                    if ( Count == 4 && input_array[1] != "Range" && input_array[3] != string.Empty)
                    {
                        Calc = (float)Math.Round(Calc,2);
                        value2 = Calc.ToString();
                        PushMoreData(input_array[3], value2);
                    }else
                    {
                        if (Count != 3 && input_array[3] == string.Empty)
                        {
                            Logger.Warn($"Warning! This New Prop hasn't been defined.");
                            return false;
                        }
                    }                   
                }

            }

                return true;
        }
        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            string[] output_ret = null;
            if(input_array == null)
                return true;
            if (input_array[1] == "Range" || input_array[1] == "<" || input_array[1] == ">")
            {
                strDataout = str_ret;
                output_ret  = strDataout.Split('-');
                if (output_ret[1].Contains("PASS"))
                    return true;
                else
                   return false;
            }

            var data = new Dictionary<string, object>
                    {
                        {input_array[3], Calc},
                    };

            string jsonStr;          
            try
            {
                jsonStr = JsonConvert.SerializeObject(data);                             
            }
            catch (Exception ex)
            {
                Logger.Error($"轉換為 JSON 字串時出現錯誤: {ex.Message}");
                return false;
            }

            string ret = CheckRule(jsonStr, strCheckSpec);
            Logger.Info($"CheckRule: {ret}");

            strDataout = jsonStr;

            if (ret == "PASS")
                return true;
            else
                return false;
        }

    }
}
