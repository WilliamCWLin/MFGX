using System.Collections.Generic;

public class ConfigurationManager
{
    public int TreePropertyShow { get; private set; }
    public int TreeShow { get; private set; }
    public int TitleBarcodeShow { get; private set; }
    public int TitleShow { get; private set; }
    public int SignalLightShow { get; private set; }
    public int esTime { get; private set; }
    public int AbortLimit { get; private set; }
    
    public string LogPath { get; private set; }
    public string ServerLogPath { get; private set; }
    public int RunMode { get; private set; }
    public string LogFileDefineName { get; private set; }
    public string FixturePort { get; private set; }
    public int LogUseItemCode { get; private set; }

    public ConfigurationManager(Dictionary<string, string> envirConfig)
    {
        TreePropertyShow = GetIntValue(envirConfig, "UI_TreePropertyShow", 0);
        TreeShow = GetIntValue(envirConfig, "UI_TreeShow", 0);
        TitleBarcodeShow = GetIntValue(envirConfig, "UI_TitleBarcodeShow", 0);
        SignalLightShow = GetIntValue(envirConfig, "UI_SignalLightShow", 0);
        esTime = GetIntValue(envirConfig, "UI_TimeShow", 0);
        AbortLimit = GetIntValue(envirConfig, "UI_AbortLimit", 0);
        TitleShow = GetIntValue(envirConfig, "UI_TitleShow", 0);
        RunMode = GetIntValue(envirConfig, "UI_RunMode", 0);
        //LogPath = GetStringValue(envirConfig, "Common_LogPath", string.Empty);
        //ServerLogPath = GetStringValue(envirConfig, "Common_ServerLogPath", string.Empty);
        LogFileDefineName = GetStringValue(envirConfig, "Common_LogFileName", string.Empty);
        FixturePort = GetStringValue(envirConfig, "Common_FixturePort", string.Empty);
        LogUseItemCode = GetIntValue(envirConfig, "Common_LogUseItemCode", 0);
    }

    private int GetIntValue(Dictionary<string, string> config, string key, int defaultValue)
    {
        if (config.TryGetValue(key, out var value) && int.TryParse(value, out var intValue))
        {
            return intValue;
        }
        return defaultValue;
    }

    private bool GetBoolValue(Dictionary<string, string> config, string key, bool defaultValue)
    {
        if (config.TryGetValue(key, out var value) && bool.TryParse(value, out var boolValue))
        {
            return boolValue;
        }
        return defaultValue;
    }

    private string GetStringValue(Dictionary<string, string> config, string key, string defaultValue)
    {
        if (config.TryGetValue(key, out var value))
        {
            return value;
        }
        return defaultValue;
    }
}