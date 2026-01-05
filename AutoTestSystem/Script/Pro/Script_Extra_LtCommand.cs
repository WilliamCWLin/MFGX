using AutoTestSystem.Base;
using AutoTestSystem.Equipment.ControlDevice;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.Script.Script_Extra_Generic_Command;

namespace AutoTestSystem.Script
{
    public enum LtCommand
    {
        InitCServer,
        GetFTSerRUN,
        GetFTStart,
        GetBoxNoEmpty,
        GetFTResult,
        GetFTStatusCode,
        RestFTInit,
        SetActiveStation,
        SetBoxNoEmpty,
        SetFTResult,
        SetFTStatusCode,
        SetRunMAC,
        SetRunCSN,
        SetAteResult,
        RestErrConnect,
        GetCommInfo
    }

    internal class Script_Extra_LtCommand : Script_Extra_Base
    {
        [Category("Command"), Description("選擇函式")]
        public LtCommand Function { get; set; } = LtCommand.SetFTResult;

        [Category("Command"), Description(
        @"InitCServer：初始化 CServer 資料，參數為配置檔路徑（例如 .\\ltcommlib.ini）。
        GetFTSerRUN：查詢 FTSer 是否正在運行。
        GetFTStart：讀取是否已開始進入自動掃碼測試流程。
        GetBoxNoEmpty：取得屏蔽箱狀態（0=空，1=有物品）。
        GetFTResult：取得測試結果值。
        GetFTStatusCode：取得屏蔽箱狀態碼。
        SetActiveStation：設定活動工位（僅支援 1 或 2），即後續 ROBOT 取放的工位。回傳值：0=成功，-1=參數錯誤。
        RestFTInit：強制將 FTInt 重設為初始狀態，允許空抓取（僅在有需求時使用）。
        SetBoxNoEmpty：設定屏蔽箱狀態（0=空，1=有物品）。
        SetFTResult：設定測試結果（1=PASS，2=FAIL，20=掃碼逾時，21=兩次相同，22=CheckFCD 異常）。
        SetFTStatusCode：設定屏蔽箱狀態碼，並將 bFTStart 設為 FALSE。
          狀態碼：20=開啟中，21=開啟失敗，22=開啟完成，30=關閉中，31=關閉失敗，32=關閉完成。
        SetRunMAC：更新 MAC 位址。
        SetRunCSN：更新 CSN（序號）。
        SetAteResult：更新 ATE 測試回傳字串。
        RestErrConnect：重置錯誤連線，允許主控端重新連接。
        GetCommInfo：取得介面資訊，例如版本、地址、站號等。")]

        public string Param { get; set; } = "";

        string strOutData = string.Empty;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {

            strOutData = "";

            return true;
        }
        public override bool Process(ref string output)
        {
            string replaceParam = ReplaceProp(Param);

            JObject resultJson = new JObject
            {
                ["Function"] = Function.ToString(),
                ["Parameter"] = replaceParam,
                ["Data"] = null // 預設為 null，後續根據需要填入
            };

            try
            {
                switch (Function)
                {
                    case LtCommand.InitCServer:
                        resultJson["Data"] = ltcommDll.InitCServer(replaceParam);
                        break;
                    case LtCommand.SetRunMAC:
                        ltcommDll.SetRunMAC(replaceParam);
                        resultJson["Data"] = "Set return Void";
                        break;
                    case LtCommand.SetRunCSN:
                        ltcommDll.SetRunCSN(replaceParam);
                        resultJson["Data"] = "Set return Void";
                        break;
                    case LtCommand.SetFTResult:
                        if (int.TryParse(replaceParam, out int result))
                        {
                            ltcommDll.SetFTResult(result);
                            resultJson["Data"] = "Set return Void";
                        }                           
                        else
                        {
                            resultJson["Data"] = $"參數轉換錯誤: {replaceParam} 不是有效的整數";
                            output = resultJson.ToString(Formatting.Indented);
                            strOutData = output;
                            LogMessage(strOutData);
                            return false;
                        }
                            break;
                    case LtCommand.SetActiveStation:
                        if (int.TryParse(replaceParam, out int station))
                        {
                            resultJson["Data"] = ltcommDll.SetActiveStation(station);
                            resultJson["Data"] = "Set return Void";
                        }

                        else
                        {
                            resultJson["Data"] = $"參數轉換錯誤: {replaceParam} 不是有效的整數";
                            output = resultJson.ToString(Formatting.Indented);
                            strOutData = output;
                            LogMessage(strOutData);
                            return false;
                        }
                        break;
                    case LtCommand.SetFTStatusCode:
                        if (int.TryParse(replaceParam, out int status))                            
                        {
                            ltcommDll.SetFTStatusCode(status);
                            resultJson["Data"] = "Set return Void";
                        }
                        else
                        {
                            resultJson["Data"] = $"參數轉換錯誤: {replaceParam} 不是有效的整數";
                            output = resultJson.ToString(Formatting.Indented);
                            strOutData = output;
                            LogMessage(strOutData);
                            return false;
                        }
                            break;
                    case LtCommand.SetBoxNoEmpty:
                        if (int.TryParse(replaceParam, out int box))
                        {
                            ltcommDll.SetBoxNoEmpty(box);
                            resultJson["Data"] = "Set return Void";
                        }                      
                        else
                        {
                            resultJson["Data"] = $"參數轉換錯誤: {replaceParam} 不是有效的整數";
                            output = resultJson.ToString(Formatting.Indented);
                            strOutData = output;
                            LogMessage(strOutData);
                            return false;
                        }
                            break;
                    case LtCommand.SetAteResult:
                        ltcommDll.SetAteResult(replaceParam);
                        resultJson["Data"] = "SetFunction Return Void";
                        break;
                    case LtCommand.RestFTInit:
                        ltcommDll.RestFTInit();
                        resultJson["Data"] = "SetFunction Return Void";
                        break;
                    case LtCommand.RestErrConnect:
                        ltcommDll.RestErrConnect();
                        resultJson["Data"] = "SetFunction Return Void";
                        break;
                    case LtCommand.GetCommInfo:
                        resultJson["Data"] = ltcommDll.GetCommInfo();
                        break;
                    case LtCommand.GetFTStart:

                        bool ftStart = ltcommDll.GetFTStart();
                        resultJson["Data"] = ftStart;

                        if (!ftStart)
                        {
                            output = resultJson.ToString(Formatting.Indented);
                            strOutData = output;
                            LogMessage(strOutData);
                            return false;
                        }

                        break;
                    case LtCommand.GetFTResult:
                        resultJson["Data"] = ltcommDll.GetFTResult();
                        break;
                    case LtCommand.GetFTStatusCode:
                        resultJson["Data"] = ltcommDll.GetFTStatusCode();
                        break;
                    case LtCommand.GetBoxNoEmpty:
                        resultJson["Data"] = ltcommDll.GetBoxNoEmpty();
                        break;
                    case LtCommand.GetFTSerRUN:
                        resultJson["Data"] = ltcommDll.GetFTSerRUN();
                        break;
                    default:
                        resultJson["Data"] = $"未處理的指令: {Function}";
                        output = resultJson.ToString(Formatting.Indented);
                        strOutData = output;
                        LogMessage(strOutData);
                        return false;
                }

                output = resultJson.ToString(Formatting.Indented);
                strOutData = output;
                LogMessage(strOutData);
                return true;
            }
            catch (Exception ex)
            {
                resultJson["Data"] = $"執行錯誤: {ex.Message}";
                output = resultJson.ToString(Formatting.Indented);
                strOutData = output;
                LogMessage(strOutData);
                return false;
            }
        }

        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
            {
                LogMessage("Spec check passed. or Spec is empty");
                return true;
            }
            else
            {
                LogMessage($"{result}", MessageLevel.Error);
                return false;
            }

        }
    }


    class ltcommDll
    {
        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitCServer(string iniFile);

        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetFTSerRUN();

        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetFTStart();


        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetBoxNoEmpty();


        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetFTResult();


        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetFTStatusCode();

        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetActiveStation(int value);

        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RestFTInit();


        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetBoxNoEmpty(int value);


        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetFTResult(int value);


        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetFTStatusCode(int value);


        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetRunMAC(string str1);


        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetRunCSN(string str1);

        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAteResult(string str1);
        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RestErrConnect();
        [DllImport("ltcommlib_transit.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetCommInfo();
    }
}
