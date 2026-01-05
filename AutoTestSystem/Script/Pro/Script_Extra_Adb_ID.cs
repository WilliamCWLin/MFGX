using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Model;
using DocumentFormat.OpenXml.Office2010.Excel;
using Manufacture;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AutoTestSystem.Script
{
    /// <summary>
    /// 單一類（主程式直接用）：自行呼叫 adb.exe 輪詢偵測【唯一】新/移除的裝置，直到逾時才回 false。
    /// - 使用 static Dictionary<string StoreKey, string DeviceID> 常駐維護映射。
    /// - 以「這次在線序號 - 上次快照」判斷新增；「上次快照 - 這次在線」判斷移除。
    /// - AutoBind / AutoUnassign 都會在 Timeout 內重複掃描（每 Interval 毫秒一次）。
    /// - 支援可攜式 adb.exe：AdbPath → ADB_PATH（檔或資料夾）→ 同資料夾 adb.exe / .\platform-tools\adb.exe → PATH。
    /// - ★ Sticky AutoBind：若 StoreKey 已綁定且該 ID 仍在線，直接回傳該 ID（不會 NO_NEW）。
    /// </summary>
    internal class Script_Extra_Adb_ID : Script_Extra_Base
    {
        // ====== 靜態狀態（行程共享） ======
        private static readonly object _lock = new object();
        private static readonly Dictionary<string, string> _keyToId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static HashSet<string> _lastScanIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string strOutData = string.Empty;

        public static IReadOnlyDictionary<string, string> TableSnapshot
            => new Dictionary<string, string>(_keyToId);

        // ====== 參數 ======
        [Category("Common Parameters"), Description("執行模式：AutoBind / AutoUnassign / Discover / Get / UnassignByKey / Clear")]
        public string Mode { get; set; } = "AutoBind";

        [Category("Common Parameters"), Description("ADB 可執行檔路徑；留 'adb' 可自動偵測 ADB_PATH/同資料夾/PATH")]
        public string AdbPath { get; set; } = "adb";

        [Category("Common Parameters"), Description("總等待逾時 (ms)，在這段時間內持續偵測，直到找到唯一新/移除才返回 true")]
        public int Timeout { get; set; } = 20000;

        [Category("Common Parameters"), Description("每次輪詢的間隔 (ms)")]
        public int Interval { get; set; } = 500;

        [Category("Common Parameters"), Description("AutoBind/UnassignByKey/Get 用的 StoreKey（建議用 DUT Name 或你的自訂 Key）")]
        public string StoreKey { get; set; } = "%FixturePart%_ID";

        [Category("Options"), Description("AutoBind：若 StoreKey 已存在是否覆寫舊 ID")]
        public bool OverwriteIfExists { get; set; } = true;

        [Category("Options"), Description("AutoBind：若新ID已被其他 StoreKey 綁定，是否允許移轉")]
        public bool RebindIfIdBoundToOtherKey { get; set; } = true;

        public override void Dispose() { }
        public override bool PreProcess()
        {
            strOutData = string.Empty;
            return true;
        }

        public override bool Process(ref string strDataout)
        {
            try
            {
                string adb = ResolveAdbPath(AdbPath);
                string mode = (Mode ?? string.Empty).Trim().ToLowerInvariant();
                bool ret = true;
                switch (mode)
                {
                    case "autobind":

                        ret = DoAutoBind(adb, ref strDataout);
                        strOutData = strDataout;
                        return ret;
                    case "autounassign": return DoAutoUnassign(adb, ref strDataout);
                    case "discover": return DoDiscover(adb, ref strDataout);
                    case "get":
                        ret = DoGet(ref strDataout);
                        strOutData = strDataout;
                        return ret;
                    case "unassignbykey": return DoUnassignByKey(ref strDataout);
                    case "clear": return DoClear(ref strDataout);
                    default:
                        strDataout = JsonConvert.SerializeObject(new { ok = false, error = "UNKNOWN_MODE", mode = Mode });
                        LogMessage($"Unknown Mode: {Mode}", MessageLevel.Error);
                        return false;
                }
            }
            catch (Exception ex)
            {
                strDataout = JsonConvert.SerializeObject(new { ok = false, Exception = ex.Message });
                LogMessage($"Exception: {ex}", MessageLevel.Error);
                return false;
            }
        }

        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "" || Spec == string.Empty)
            {
                return true;
            }
            else
            {
                LogMessage($"CheckRule: {result}", MessageLevel.Error);
                return false;
            }
        }

        // ====== AutoBind：輪詢直到抓到【唯一】新ID後，綁到 StoreKey；
        // 同時支援 Sticky：若 StoreKey 已綁且該 ID 仍在線，直接回傳該 ID。
        private bool DoAutoBind(string adb, ref string strDataout)
        {
            if (string.IsNullOrWhiteSpace(StoreKey))
            {
                strDataout = JsonConvert.SerializeObject(new { ok = false, reason = "STOREKEY_EMPTY" });
                LogMessage("AutoBind failed: StoreKey empty", MessageLevel.Error);
                return false;
            }

            string key = ReplaceProp(StoreKey.Trim());
            var deadline = DateTime.UtcNow.AddMilliseconds(Math.Max(0, Timeout));
            var lastAdded = new List<string>();

            while (true)
            {
                int remaining = (int)Math.Max(0, (deadline - DateTime.UtcNow).TotalMilliseconds);
                int perCall = Math.Max(500, Math.Min(2000, remaining + 200));
                var now = ListOnlineSerials(adb, perCall);

                // ===== STICKY 快速路徑：key 已綁且該 id 仍在線 → 直接回傳，不會 NO_NEW
                string existingId = null;
                lock (_lock)
                {
                    _keyToId.TryGetValue(key, out existingId);
                }
                if (!string.IsNullOrEmpty(existingId))
                {
                    var nowSet = new HashSet<string>(now, StringComparer.OrdinalIgnoreCase);
                    if (nowSet.Contains(existingId))
                    {
                        // 更新快照再回傳（避免下一輪比較不一致）
                        lock (_lock) { _lastScanIds = new HashSet<string>(nowSet, StringComparer.OrdinalIgnoreCase); }
                        strDataout = JsonConvert.SerializeObject(new { ok = true, storeKey = key, id = existingId, sticky = true });

                        if (!string.IsNullOrEmpty(existingId))
                        {
                            PushMoreData("ID_Command", "-s " + existingId);
                        }
                        LogMessage($"Set[{key}]={existingId}");
                        LogMessage($"{strDataout}");
                        return true;
                    }
                }

                // ===== 正常「新裝置」偵測路徑
                List<string> added;
                lock (_lock)
                {
                    added = now.Except(_lastScanIds, StringComparer.OrdinalIgnoreCase).ToList();
                    _lastScanIds = new HashSet<string>(now, StringComparer.OrdinalIgnoreCase);
                }

                if (added.Count == 1)
                {
                    string id = added[0];
                    string prevKeyForId = null;
                    string prevIdForKey = null;

                    lock (_lock)
                    {
                        // 該 ID 是否被其他 Key 綁定？
                        foreach (var kv in _keyToId)
                        {
                            if (string.Equals(kv.Value, id, StringComparison.OrdinalIgnoreCase)) { prevKeyForId = kv.Key; break; }
                        }
                        if (prevKeyForId != null && !RebindIfIdBoundToOtherKey)
                        {
                            strDataout = JsonConvert.SerializeObject(new { ok = false, reason = "ID_ALREADY_BOUND_TO_OTHER_KEY", id, otherKey = prevKeyForId });
                            LogMessage($"AutoBind blocked: id {id} bound to {prevKeyForId}", MessageLevel.Error);
                            return false;
                        }
                        if (_keyToId.TryGetValue(key, out prevIdForKey) && !OverwriteIfExists)
                        {
                            strDataout = JsonConvert.SerializeObject(new { ok = false, reason = "KEY_ALREADY_BOUND", storeKey = key, currentId = prevIdForKey });
                            LogMessage($"AutoBind blocked: StoreKey {key} already -> {prevIdForKey}", MessageLevel.Error);
                            return false;
                        }
                        if (prevKeyForId != null && !string.Equals(prevKeyForId, key, StringComparison.OrdinalIgnoreCase))
                            _keyToId.Remove(prevKeyForId);
                        _keyToId[key] = id;
                    }

                    strDataout = JsonConvert.SerializeObject(new { ok = true, storeKey = key, id, rebindFromKey = prevKeyForId, replacedOldIdOfKey = prevIdForKey });

                    if (!string.IsNullOrEmpty(existingId))
                    {
                        PushMoreData("ID_Command", "-s " + existingId);
                    }
                    
                    LogMessage($"Set[{key}]={id}");
                    LogMessage($"{strDataout}");
                    return true;
                }

                // 尚未唯一：記住最後一次的候選，繼續等
                lastAdded = added;
                if (remaining <= 0)
                {
                    if (lastAdded.Count == 0)
                    {
                        strDataout = JsonConvert.SerializeObject(new { ok = false, reason = "NO_NEW", waitedMs = Timeout });
                        LogMessage("AutoBind timeout: NO_NEW");
                    }
                    else
                    {
                        strDataout = JsonConvert.SerializeObject(new { ok = false, reason = "MULTIPLE_NEW", serials = lastAdded, waitedMs = Timeout });
                        LogMessage($"AutoBind timeout: MULTIPLE_NEW -> {string.Join(",", lastAdded)}", MessageLevel.Error);
                    }
                    LogMessage($"{strDataout}");
                    return false;
                }
                Thread.Sleep(Math.Min(Interval, remaining));
            }
        }

        // ====== AutoUnassign：輪詢直到抓到【唯一】被拔除的 ID，並解除映射 ======
        private bool DoAutoUnassign(string adb, ref string strDataout)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(Math.Max(0, Timeout));
            var lastRemoved = new List<string>();

            while (true)
            {
                int remaining = (int)Math.Max(0, (deadline - DateTime.UtcNow).TotalMilliseconds);
                int perCall = Math.Max(500, Math.Min(2000, remaining + 200));
                var now = ListOnlineSerials(adb, perCall);

                List<string> removed;
                lock (_lock)
                {
                    removed = _lastScanIds.Except(now, StringComparer.OrdinalIgnoreCase).ToList();
                    _lastScanIds = new HashSet<string>(now, StringComparer.OrdinalIgnoreCase);
                }

                if (removed.Count == 1)
                {
                    string gone = removed[0];
                    string keyFound = null;
                    lock (_lock)
                    {
                        foreach (var kv in _keyToId.ToList())
                        {
                            if (string.Equals(kv.Value, gone, StringComparison.OrdinalIgnoreCase))
                            { keyFound = kv.Key; _keyToId.Remove(kv.Key); break; }
                        }
                    }

                    if (keyFound == null)
                    {
                        strDataout = JsonConvert.SerializeObject(new { ok = false, reason = "REMOVED_ID_NOT_IN_TABLE", id = gone });
                        LogMessage($"AutoUnassign: removed id {gone} not in table");
                        return false;
                    }

                    strDataout = JsonConvert.SerializeObject(new { ok = true, storeKey = keyFound, id = gone });
                    LogMessage($"AutoUnassign: {keyFound} -/-> {gone}");
                    return true;
                }

                lastRemoved = removed;
                if (remaining <= 0)
                {
                    if (lastRemoved.Count == 0)
                    {
                        strDataout = JsonConvert.SerializeObject(new { ok = false, reason = "NONE_REMOVED", waitedMs = Timeout });
                        LogMessage("AutoUnassign timeout: NONE_REMOVED");
                    }
                    else
                    {
                        strDataout = JsonConvert.SerializeObject(new { ok = false, reason = "MULTIPLE_REMOVED", serials = lastRemoved, waitedMs = Timeout });
                        LogMessage($"AutoUnassign timeout: MULTIPLE_REMOVED -> {string.Join(",", lastRemoved)}", MessageLevel.Error);
                    }
                    return false;
                }
                Thread.Sleep(Math.Min(Interval, remaining));
            }
        }

        // ====== Discover：輪詢直到抓到唯一新ID；不改映射 ======
        private bool DoDiscover(string adb, ref string strDataout)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(Math.Max(0, Timeout));
            var lastAdded = new List<string>();

            while (true)
            {
                int remaining = (int)Math.Max(0, (deadline - DateTime.UtcNow).TotalMilliseconds);
                int perCall = Math.Max(500, Math.Min(2000, remaining + 200));
                var now = ListOnlineSerials(adb, perCall);

                List<string> added;
                lock (_lock)
                {
                    added = now.Except(_lastScanIds, StringComparer.OrdinalIgnoreCase).ToList();
                    _lastScanIds = new HashSet<string>(now, StringComparer.OrdinalIgnoreCase);
                }

                if (added.Count == 1)
                {
                    strDataout = JsonConvert.SerializeObject(new { newDevice = true, serial = added[0] });
                    LogMessage($"Discover: NEW={added[0]}");
                    return true;
                }

                lastAdded = added;
                if (remaining <= 0)
                {
                    if (lastAdded.Count == 0)
                        strDataout = JsonConvert.SerializeObject(new { newDevice = false, reason = "NO_NEW", waitedMs = Timeout });
                    else
                        strDataout = JsonConvert.SerializeObject(new { newDevice = true, reason = "MULTIPLE_NEW", serials = lastAdded, waitedMs = Timeout });
                    LogMessage("Discover timeout");
                    return false;
                }
                Thread.Sleep(Math.Min(Interval, remaining));
            }
        }

        private bool DoUnassignByKey(ref string strDataout)
        {
            if (string.IsNullOrWhiteSpace(StoreKey))
            {
                strDataout = JsonConvert.SerializeObject(new { ok = false, reason = "STOREKEY_EMPTY" });
                LogMessage("UnassignByKey failed: StoreKey empty", MessageLevel.Error);
                return false;
            }
            string key = ReplaceProp(StoreKey);
            bool removed;
            string id = null;
            lock (_lock)
            {
                if (_keyToId.TryGetValue(key, out id))
                {
                    _keyToId.Remove(key);
                    _lastScanIds.Remove(id); // 讓它在不拔插下次也能被視為新裝置
                    removed = true;
                }
                else removed = false;
            }
            strDataout = JsonConvert.SerializeObject(new { ok = removed, storeKey = key?.Trim(), id = id });
            LogMessage(removed ? $"UnassignByKey OK: {key}={id}" : $"UnassignByKey NOT_FOUND: {key}", removed ? MessageLevel.Info : MessageLevel.Error);
            return removed;
        }

        private bool DoGet(ref string strDataout)
        {
            lock (_lock)
            {
                if (!string.IsNullOrWhiteSpace(StoreKey))
                {
                    var key = StoreKey.Trim();
                    if (_keyToId.TryGetValue(key, out var id))
                    { strDataout = JsonConvert.SerializeObject(new { storeKey = key, id }); return true; }
                    strDataout = JsonConvert.SerializeObject(new { storeKey = key, found = false });
                    return false;
                }
                else
                {
                    strDataout = JsonConvert.SerializeObject(new { table = _keyToId });
                    return true;
                }
            }
        }

        private bool DoClear(ref string strDataout)
        {
            int n;
            lock (_lock) { n = _keyToId.Count; _keyToId.Clear(); _lastScanIds.Clear(); }
            strDataout = JsonConvert.SerializeObject(new { ok = true, cleared = n });
            LogMessage("Clear table");
            return true;
        }

        // ====== ADB：僅回傳狀態為 device 的序號 ======
        private static List<string> ListOnlineSerials(string adbPath, int timeoutMs)
        {
            RunAdb(adbPath, "start-server", timeoutMs);
            string output = RunAdb(adbPath, "devices -l", timeoutMs);
            var list = new List<string>();
            using (var sr = new StringReader(output))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("List of devices")) continue;
                    line = line.Trim();
                    if (line.Length == 0) continue;
                    var parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) continue;
                    if (string.Equals(parts[1], "device", StringComparison.OrdinalIgnoreCase))
                        list.Add(parts[0]);
                }
            }
            return list;
        }

        private static string RunAdb(string adbPath, string args, int timeoutMs)
        {
            var psi = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (var p = System.Diagnostics.Process.Start(psi))
            {
                if (!p.WaitForExit(timeoutMs))
                {
                    try { p.Kill(); } catch { }
                    throw new Exception("ADB timeout: " + args);
                }

                string stdout = p.StandardOutput.ReadToEnd();
                string stderr = p.StandardError.ReadToEnd();

                if (p.ExitCode != 0 && string.IsNullOrEmpty(stdout))
                    throw new Exception("ADB error: " + stderr);

                return stdout;
            }
        }

        private static string ResolveAdbPath(string configured)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(configured) && (configured.Contains("\\") || configured.Contains("/")))
                    if (File.Exists(configured)) return configured;
            }
            catch { }
            try
            {
                string env = Environment.GetEnvironmentVariable("ADB_PATH");
                if (!string.IsNullOrEmpty(env))
                {
                    if (File.Exists(env)) return env;
                    if (Directory.Exists(env))
                    {
                        string envFile = Path.Combine(env, "adb.exe");
                        if (File.Exists(envFile)) return envFile;
                    }
                }
            }
            catch { }
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string local = Path.Combine(baseDir, "adb.exe");
                if (File.Exists(local)) return local;
                string pt = Path.Combine(baseDir, "platform-tools");
                string ptAdb = Path.Combine(pt, "adb.exe");
                if (File.Exists(ptAdb)) return ptAdb;
            }
            catch { }
            return string.IsNullOrWhiteSpace(configured) ? "adb" : configured;
        }
    }
}
