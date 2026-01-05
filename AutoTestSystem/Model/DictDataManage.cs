using AutoTestSystem.BLL;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class DataManager
{
    // 靜態字典
    private static Dictionary<string, string> dataDictionary = new Dictionary<string, string>();


    // 加載數據到字典
    public static void LoadData(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Bd.Logger.Debug($"文件路徑不存在: {filePath}");
            return;
        }

        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fs))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] columns = line.Split(',').Take(2).ToArray();
                    if (columns.Length == 2)
                    {
                        string key = columns[0].Trim();
                        string value = columns[1].Trim();

                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        {
                            if (!dataDictionary.ContainsKey(key))
                            {
                                dataDictionary.Add(key, value);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Bd.Logger.Debug($"讀取文件錯誤: {ex.Message}");
        }
    }


    // 通過鍵查找值
    public static string GetValueByKey(string key)
    {
        if (dataDictionary.TryGetValue(key, out string value))
        {
            return value;
        }
        return "Undefine";
    }

    // 通過值查找鍵
    public static string GetKeyByValue(string value)
    {
        var key = dataDictionary.FirstOrDefault(x => x.Value == value).Key;
        return key ?? "Undefine";
    }
}
