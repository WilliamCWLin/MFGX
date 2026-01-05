
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using System.Linq.Dynamic.Core;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_GetDataTableProp : Script_Extra_Base
    {
        string strOutData = string.Empty;

        [Category("Common Params"), Description("Select First Row Data")]
        public bool OnlyGetFirstFind { get; set; } = true;

        [Category("LINQ Search"), Description("Ex:Project==\"5005\"")]
        public string Where { get; set; } = "";

        [Category("LINQ Search"), Description("Ex:ModelName Contain \"256E\"")]
        public string Contain { get; set; } = "";

        [Category("LINQ Search"), Description("A is contained in B.   Ex:ModelName Contained \"256E,2323,2323,2323\" ")]
        public string Contained { get; set; } = "";

        [Category("LINQ Search"), Description("Ex1:ProductSN descending  Ex2:ProductSN ascending")]
        public string OrderBy { get; set; } = "";

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            strOutData = string.Empty;
            return true;
        }
        public override bool Process(ref string OutData)
        {
            string JsonTableData = HandleDevice.TableData;
            string WhereReplace = ReplaceProp(Where);
            string ContainReplace = ReplaceProp(Contain);
            string OrderByReplace = ReplaceProp(OrderBy);
            string ContainedReplace = ReplaceProp(Contained); // 使用一個變數來設定查詢條件
            var resultDictionary = new Dictionary<string, string>();
            try
            {
                // 判斷 JSON 字串是否有效
                var rows = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(JsonTableData);
                if (rows == null || rows.Count == 0)
                {
                    LogMessage($"JsonTableData:{JsonTableData} is null or count = 0");
                    return false; // 若無結果，返回空字典
                }
                // 建立查詢
                var query = rows.AsQueryable();
                // 添加查詢條件
                if (!string.IsNullOrEmpty(WhereReplace))
                {
                    query = query.Where(WhereReplace);
                }

                if (!string.IsNullOrEmpty(ContainReplace))
                {
                    var conditions = ContainReplace.Split(new[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var condition in conditions)
                    {
                        var parts = condition.Split(new[] { "Contain" }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var value = parts[1].Trim().Trim('"');
                            query = query.Where(row => row.ContainsKey(key) && row[key].Contains(value));
                        }
                        else
                        {
                            LogMessage($"Contained Search Format Error!");
                            return false;
                        }
                    }
                }

                // 解析 SearchCondition 來取出查詢字串和欄位
                if (!string.IsNullOrEmpty(ContainedReplace))
                {
                    var parts = ContainedReplace.Split(new[] { " Contained " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        var searchString = parts[1].Trim();
                        //var fieldsToCheck = parts[0].Trim().Split(new[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);
                        var fieldsToCheck = parts[0].Trim().Replace(" ", "").Trim().Split(new[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);
                        //query = query.Where(row => fieldsToCheck.Any(field => row.ContainsKey(field) && searchString.Contains(row[field])));
                        query = query.Where(row => fieldsToCheck.All(field => row.ContainsKey(field) && searchString.Contains(row[field])));
                    }
                    else
                    {
                        LogMessage($"Contained Search Format Error!");
                        return false;
                    }
                }
                // 添加排序條件
                if (!string.IsNullOrEmpty(OrderByReplace))
                {
                    query = query.OrderBy(OrderByReplace);
                }
                // 執行查詢
                var result = query.ToList();
                int rowIndex = 1;

                LogMessage($"Search Condition->Where[{WhereReplace}],OrderBy[{OrderByReplace}],Find Count {result.Count}");

                if (result.Count <= 0)
                {
                    return false;
                }

                foreach (var row in result)
                {
                    // 將整行數據組合成一個字符串
                    var rowData = string.Join(", ", row.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                    LogMessage($"Find Row {rowIndex}: {rowData}");
                }

                var jsonData = new Dictionary<string, string>();

                foreach (var row in result)
                {
                    foreach (var kvp in row)
                    {
                        if (OnlyGetFirstFind)
                        {
                            var key = $"{Prefix}{kvp.Key}";
                            var value = kvp.Value.ToString(); // 取得列的值並轉為字符串
                            PushMoreData(key, value);
                            jsonData[key] = value;
                            // 記錄成功加入的結果
                            LogMessage($"SetMoreProp [{key}]={value}");
                        }
                        else
                        {
                            // 格式化 key: "RowX_<column_name>"
                            var key = $"{Prefix}Row{rowIndex}_{kvp.Key}";
                            var value = kvp.Value.ToString(); // 取得列的值並轉為字符串
                            PushMoreData(key, value);
                            jsonData[key] = value;
                            // 記錄成功加入的結果
                            LogMessage($"SetMoreProp [{key}]={value}");
                        }
                    }

                    if (OnlyGetFirstFind)
                        break;

                    rowIndex++;
                }

                // 將字典轉換為 JSON 字串
                OutData = JsonConvert.SerializeObject(jsonData,Formatting.Indented);
                strOutData = OutData;
            }
            catch (JsonException ex)
            {
                // 處理 JSON 反序列化錯誤
                LogMessage($"JSON 解析錯誤: {ex.Message}");
                return false;
            }
            catch (InvalidOperationException ex)
            {
                // 處理查詢處理錯誤，如重覆的鍵
                LogMessage(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                // 捕捉其他可能的異常
                LogMessage($"查詢處理錯誤: {ex.Message}");
                return false;
            }

            return true;

        }
        public override bool PostProcess()
        {
            LogMessage($"Check Spec:{Spec}");

            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "" || Spec == string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


    }
}
