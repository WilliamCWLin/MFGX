using AutoTestSystem.Script;
using Manufacture;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AutoTestSystem.Model
{
    // 相容舊用法的資料結構（一般精簡模式用）
    public class SlimNode
    {
        public string NodeId { get; set; }
        public string ClassName { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public List<string> PathIds { get; set; }
        public List<string> PathNames { get; set; }
        public Dictionary<string, object> Props { get; set; }
        public List<SlimNode> Children { get; set; }
        public bool IsMinimal { get; set; }

        public SlimNode()
        {
            PathIds = new List<string>();
            PathNames = new List<string>();
            Props = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            Children = new List<SlimNode>();
            IsMinimal = false;
        }
    }

    public static class SlimTreeExporter
    {
        // ===== 一般精簡模式 =====

        public static List<SlimNode> BuildSlimTree(TreeView tv, Dictionary<string, string[]> propsMap)
        {
            return BuildSlimTree(tv, propsMap, false);
        }

        public static List<SlimNode> BuildSlimTree(TreeView tv, Dictionary<string, string[]> propsMap, bool includeMinimalAncestors)
        {
            var roots = new List<SlimNode>();
            if (tv == null) return roots;

            var idStack = new Stack<string>();
            var nameStack = new Stack<string>();

            for (int i = 0; i < tv.Nodes.Count; i++)
            {
                VisitNode(tv.Nodes[i], roots, idStack, nameStack, propsMap, 0, includeMinimalAncestors);
            }
            return roots;
        }

        public static string ExportSlimJson(TreeView tv, Dictionary<string, string[]> propsMap)
        {
            return ExportSlimJson(tv, propsMap, false);
        }

        public static string ExportSlimJson(TreeView tv, Dictionary<string, string[]> propsMap, bool includeMinimalAncestors)
        {
            var roots = BuildSlimTree(tv, propsMap, includeMinimalAncestors);
            return JsonConvert.SerializeObject(roots, Formatting.Indented);
        }

        public static void ExportSlimJsonToFile(TreeView tv, Dictionary<string, string[]> propsMap, string outputPath, bool includeMinimalAncestors)
        {
            string json = ExportSlimJson(tv, propsMap, includeMinimalAncestors);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath));
            System.IO.File.WriteAllText(outputPath, json);
        }
        // ==== 新增：完整保留階層，但屬性照白名單輸出 ====

        // TreeView 版本：從整棵樹根開始
        public static string ExportStrictJsonKeepAll(TreeView tv, Dictionary<string, string[]> propsMap)
        {
            var list = BuildStrictTreeKeepAll(tv, propsMap);
            return JsonConvert.SerializeObject(list, Formatting.Indented);
        }

        // 指定根節點版本（你可以只匯出某一個子樹）
        public static string ExportStrictJsonKeepAll(TreeNode root, Dictionary<string, string[]> propsMap)
        {
            var list = BuildStrictTreeKeepAll(root, propsMap);
            return JsonConvert.SerializeObject(list, Formatting.Indented);
        }

        // 也給你一個寫檔版本（整棵樹）
        public static void ExportStrictJsonKeepAllToFile(TreeView tv, Dictionary<string, string[]> propsMap, string outputPath)
        {
            var json = ExportStrictJsonKeepAll(tv, propsMap);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath));
            System.IO.File.WriteAllText(outputPath, json, Encoding.UTF8);
        }

        // 也給你一個寫檔版本（指定根）
        public static void ExportStrictJsonKeepAllToFile(TreeNode root, Dictionary<string, string[]> propsMap, string outputPath)
        {
            var json = ExportStrictJsonKeepAll(root, propsMap);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath));
            System.IO.File.WriteAllText(outputPath, json, Encoding.UTF8);
        }

        // 內部：整棵樹
        public static List<Dictionary<string, object>> BuildStrictTreeKeepAll(TreeView tv, Dictionary<string, string[]> propsMap)
        {
            var roots = new List<Dictionary<string, object>>();
            if (tv == null) return roots;
            for (int i = 0; i < tv.Nodes.Count; i++)
                VisitNodeStrictKeepAll(tv.Nodes[i], roots, propsMap);
            return roots;
        }

        // 內部：指定根
        public static List<Dictionary<string, object>> BuildStrictTreeKeepAll(TreeNode root, Dictionary<string, string[]> propsMap)
        {
            var roots = new List<Dictionary<string, object>>();
            if (root == null) return roots;
            VisitNodeStrictKeepAll(root, roots, propsMap);
            return roots;
        }

        // 核心：不裁枝，所有節點都輸出一個物件；屬性仍照白名單。
        private static void VisitNodeStrictKeepAll(
            TreeNode tn,
            List<Dictionary<string, object>> outList,
            Dictionary<string, string[]> propsMap)
        {
            if (tn == null || tn.Tag == null) return;

            // 先準備目前這個節點的物件
            var current = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // 若此型別在白名單，就填屬性；否則保持空物件
            string[] resolvedProps;
            if (TryResolvePropsForType(tn.Tag.GetType(), propsMap, out resolvedProps))
                FillProps(tn.Tag, resolvedProps, current);

            // 遞迴子節點（不論本節點是否在白名單，都要保留階層）
            var children = new List<Dictionary<string, object>>();
            for (int i = 0; i < tn.Nodes.Count; i++)
                VisitNodeStrictKeepAll(tn.Nodes[i], children, propsMap);

            if (children.Count > 0)
                current["Children"] = children;

            outList.Add(current);
        }

        private static void VisitNode(
            TreeNode tn,
            List<SlimNode> outList,
            Stack<string> idStack,
            Stack<string> nameStack,
            Dictionary<string, string[]> propsMap,
            int level,
            bool includeMinimalAncestors)
        {
            if (tn == null || tn.Tag == null) return;

            var cb = tn.Tag as CoreBase;
            if (cb == null) return;

            Type tagType = tn.Tag.GetType();
            string className = tagType.Name;

            idStack.Push(cb.ID);
            nameStack.Push(cb.Description);

            // 解析白名單
            string[] resolvedProps = null;
            bool inWhitelist = TryResolvePropsForType(tagType, propsMap, out resolvedProps);

            // ★ 只收 isTestItem == true，或 類型是 TerminalNode（即使沒有 isTestItem 也要存）
            bool keepThis = HasTrueIsTestItem(tn.Tag) || IsTerminalType(tagType);

            SlimNode current = null;

            // ★ 只有「在白名單」且「符合條件」才輸出本節點
            if (inWhitelist && keepThis)
            {
                current = new SlimNode
                {
                    NodeId = cb.ID,
                    ClassName = className,
                    Name = cb.Description,
                    Level = level
                };

                var ids = idStack.ToArray();
                var names = nameStack.ToArray();
                for (int k = ids.Length - 1; k >= 0; k--) current.PathIds.Add(ids[k]);
                for (int k = names.Length - 1; k >= 0; k--) current.PathNames.Add(names[k]);

                FillProps(tn.Tag, resolvedProps, current.Props);
                outList.Add(current);
            }

            // 子節點：如果本節點沒被收錄，就把符合條件的子孫直接掛到 outList
            List<SlimNode> childTarget = (current != null) ? current.Children : outList;

            for (int i = 0; i < tn.Nodes.Count; i++)
            {
                VisitNode(tn.Nodes[i], childTarget, idStack, nameStack, propsMap, level + 1, includeMinimalAncestors);
            }

            idStack.Pop();
            nameStack.Pop();
        }
        // === 只輸出：isTestItem == true 的 Container 以及其子孫中的 TerminalNode ===
        // （整棵樹）
        public static string ExportStrictJson_FilterContainersAndTerminals(TreeView tv, Dictionary<string, string[]> propsMap)
        {
            var roots = new List<Dictionary<string, object>>();
            if (tv != null)
            {
                for (int i = 0; i < tv.Nodes.Count; i++)
                {
                    VisitNodeStrict_Filter(tv.Nodes[i], roots, propsMap, /*underKeptContainer*/ false);
                }
            }
            return JsonConvert.SerializeObject(roots, Formatting.Indented);
        }

        // （指定某顆子樹 root）
        public static string ExportStrictJson_FilterContainersAndTerminals(TreeNode root, Dictionary<string, string[]> propsMap)
        {
            var roots = new List<Dictionary<string, object>>();
            VisitNodeStrict_Filter(root, roots, propsMap, /*underKeptContainer*/ false);
            return JsonConvert.SerializeObject(roots, Formatting.Indented);
        }

        // 遞迴：只輸出 isTestItem==true 的 Container 與其底下的 TerminalNode
        private static void VisitNodeStrict_Filter(
            TreeNode tn,
            List<Dictionary<string, object>> outList,
            Dictionary<string, string[]> propsMap,
            bool underKeptContainer)
        {
            if (tn == null || tn.Tag == null) return;

            // 型別判斷
            Type t = tn.Tag.GetType();
            bool isContainer = IsContainerType(t);
            bool isTerminal = IsTerminalType(t);

            // 解析白名單（嚴格白名單：非白名單型別就算合條件也不輸出，但可以繼續往下走）
            string[] resolvedProps;
            bool inWhitelist = TryResolvePropsForType(t, propsMap, out resolvedProps);

            // ---- 分支邏輯 ----
            if (isContainer)
            {
                // Container 決定是否「被保留」
                bool keepThis = HasIsTestItemTrue(tn.Tag);  // 只有 isTestItem==true 的 Container 會被輸出

                if (keepThis && inWhitelist)
                {
                    // 建立當前 Container 節點（只填白名單鍵）
                    var current = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    FillProps(tn.Tag, resolvedProps, current);

                    // 收集其子孫中的 TerminalNode（非 Terminal 的中間節點不輸出，但繼續往下找）
                    var children = new List<Dictionary<string, object>>();
                    for (int i = 0; i < tn.Nodes.Count; i++)
                    {
                        VisitNodeStrict_Filter(tn.Nodes[i], children, propsMap, /*underKeptContainer*/ true);
                    }

                    if (children.Count > 0)
                        current["Children"] = children;

                    outList.Add(current);
                }
                else
                {
                    // 不保留此 Container，本身不輸出；但是其子孫仍可能存在「另外一顆 isTestItem==true 的 Container」，所以要繼續往下搜。
                    for (int i = 0; i < tn.Nodes.Count; i++)
                    {
                        VisitNodeStrict_Filter(tn.Nodes[i], outList, propsMap, /*underKeptContainer*/ false);
                    }
                }
                return;
            }

            // 非 Container：如果在「被保留的 Container」之下，且是 Terminal，就輸出；否則不輸出
            if (underKeptContainer && isTerminal && inWhitelist)
            {
                var node = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                FillProps(tn.Tag, resolvedProps, node);
                outList.Add(node);
                // TerminalNode 一般不需要再遞迴，但保守起見仍可下探（若有需要）
            }

            // 無論是否輸出本身，皆繼續往下找（以涵蓋 Terminal 可能在更深層）
            for (int c = 0; c < tn.Nodes.Count; c++)
            {
                VisitNodeStrict_Filter(tn.Nodes[c], outList, propsMap, underKeptContainer);
            }
        }

        // ===== 型別判斷（容器 / 終端） =====
        private static bool IsContainerType(Type t)
        {
            if (t == null) return false;
            // 以實際型別為主，備援名稱關鍵字
            return typeof(Manufacture.ContainerNode).IsAssignableFrom(t)
                   || t.Name.IndexOf("Container", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsTerminalType(Type t)
        {
            if (t == null) return false;
            return typeof(Manufacture.TerminalNode).IsAssignableFrom(t)
                   || t.Name.IndexOf("Terminal", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // 判斷 isTestItem（支援屬性/欄位，大小寫不敏感；true/y/1/on/enable 皆視為 true）
        private static bool HasIsTestItemTrue(object obj)
        {
            if (obj == null) return false;

            object val;
            // 優先找 "isTestItem"，再找 "IsTestItem"
            if (TryGetMemberValue(obj, "isTestItem", out val) || TryGetMemberValue(obj, "IsTestItem", out val))
            {
                bool b;
                if (BoolTryParse(val, out b)) return b;
            }
            return false;
        }

        // 解析布林（與你 HTML 版同一邏輯，這裡複製一份在 Exporter 內使用）
        //private static bool BoolTryParse(object v, out bool b)
        //{
        //    try
        //    {
        //        if (v == null) { b = false; return false; }
        //        if (v is bool) { b = (bool)v; return true; }
        //        string s = Convert.ToString(v).Trim().ToLowerInvariant();
        //        if (s == "true" || s == "t" || s == "1" || s == "yes" || s == "y" || s == "on" || s == "enable" || s == "enabled") { b = true; return true; }
        //        if (s == "false" || s == "f" || s == "0" || s == "no" || s == "n" || s == "off" || s == "disable" || s == "disabled") { b = false; return true; }
        //    }
        //    catch { }
        //    b = false; return false;
        //}
        // ===== 嚴格白名單模式（只有白名單鍵 + Children） =====

        // === 只輸出「某個 TreeNode 當根」的嚴格白名單 JSON ===
        public static string ExportStrictJson(TreeNode root, Dictionary<string, string[]> propsMap)
        {
            var list = BuildStrictTree(root, propsMap);
            return JsonConvert.SerializeObject(list, Formatting.Indented);
        }

        public static List<Dictionary<string, object>> BuildStrictTree(TreeNode root, Dictionary<string, string[]> propsMap)
        {
            var roots = new List<Dictionary<string, object>>();
            if (root == null) return roots;

            VisitNodeStrict(root, roots, propsMap);
            return roots;
        }

        private static void VisitNodeStrict(
            TreeNode tn,
            List<Dictionary<string, object>> outList,
            Dictionary<string, string[]> propsMap)
        {
            if (tn == null || tn.Tag == null) return;

            var cb = tn.Tag as CoreBase;
            if (cb == null) return;

            Type tagType = tn.Tag.GetType();

            string[] resolvedProps = null;
            bool inWhitelist = TryResolvePropsForType(tagType, propsMap, out resolvedProps);

            // ★ 只收 isTestItem == true，或 類型是 TerminalNode（即使沒有 isTestItem 也要存）
            bool keepThis = HasTrueIsTestItem(tn.Tag) || IsTerminalType(tagType);

            Dictionary<string, object> current = null;

            if (inWhitelist && keepThis)
            {
                current = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                FillProps(tn.Tag, resolvedProps, current);
            }

            // 子節點目標（若本節點未被保留，就把符合條件的子孫直接掛到 outList）
            List<Dictionary<string, object>> children = (current != null) ? new List<Dictionary<string, object>>() : outList;

            for (int i = 0; i < tn.Nodes.Count; i++)
            {
                VisitNodeStrict(tn.Nodes[i], children, propsMap);
            }

            if (current != null)
            {
                if (children.Count > 0) current["Children"] = children;
                outList.Add(current);
            }
        }

        // ===== 型別解析（支援繼承，最具體優先） =====

        private static bool TryResolvePropsForType(Type actualType, Dictionary<string, string[]> propsMap, out string[] props)
        {
            props = null;
            if (actualType == null || propsMap == null || propsMap.Count == 0) return false;

            Type t = actualType;
            while (t != null)
            {
                string key = t.Name; // 若專案有同名類，改成 t.FullName 並同步修改 propsMap
                if (propsMap.ContainsKey(key))
                {
                    props = propsMap[key];
                    return true;
                }
                t = t.BaseType;
            }
            return false;
        }

        // 在 SlimTreeExporter 裡新增：
        private static bool TryParseJsonObjectOrArray(string s, out object parsed)
        {
            parsed = null;
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim();
            if (!(s.StartsWith("{") || s.StartsWith("["))) return false;
            try
            {
                parsed = JsonConvert.DeserializeObject<object>(s);
                return parsed != null;
            }
            catch { return false; }
        }

        // 用這個覆蓋你現有的 FillProps（其餘內容不變）
        private static void FillProps(object obj, string[] propNames, Dictionary<string, object> dst)
        {
            if (obj == null || propNames == null) return;

            for (int i = 0; i < propNames.Length; i++)
            {
                string spec = propNames[i];
                if (string.IsNullOrWhiteSpace(spec)) continue;

                string alias = null;
                int aliasSep = spec.IndexOf("=>", StringComparison.Ordinal);
                string path = spec;
                if (aliasSep >= 0)
                {
                    path = spec.Substring(0, aliasSep).Trim();
                    alias = spec.Substring(aliasSep + 2).Trim();
                }

                object value;
                if (TryGetValueByPath(obj, path, out value))
                {
                    string outKey = !string.IsNullOrEmpty(alias) ? alias : LastSegment(path);

                    // DurationSec 四捨五入三位
                    if (value != null &&
                        (string.Equals(outKey, "DurationSec", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(LastSegment(path), "DurationSec", StringComparison.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            double d = Convert.ToDouble(value);
                            value = Math.Round(d, 3, MidpointRounding.AwayFromZero);
                        }
                        catch { }
                    }

                    // ★ 把 Spec / OutputData 這類「JSON 字串」轉為物件，避免輸出時出現 \n
                    if (value is string)
                    {
                        string s = (string)value;

                        // 只在看起來像 JSON（{ 或 [ 開頭）時才嘗試轉為物件
                        if (string.Equals(outKey, "Spec", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(outKey, "OutputData", StringComparison.OrdinalIgnoreCase) ||
                            (s != null && (s.TrimStart().StartsWith("{") || s.TrimStart().StartsWith("["))))
                        {
                            object parsed;
                            if (TryParseJsonObjectOrArray(s, out parsed))
                            {
                                value = parsed; // 轉成 JObject / JArray -> 序列化時就是內嵌物件，沒有 \n
                            }
                        }
                    }

                    dst[outKey] = value;
                }
            }
        }


        private static string LastSegment(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            int idx = path.LastIndexOf('.');
            return (idx >= 0) ? path.Substring(idx + 1) : path;
        }

        private static bool TryGetValueByPath(object obj, string path, out object value)
        {
            value = null;
            if (obj == null || string.IsNullOrEmpty(path)) return false;

            string[] segs = path.Split('.');
            object cur = obj;

            for (int i = 0; i < segs.Length; i++)
            {
                if (cur == null) return false;

                object next;
                if (!TryGetMemberValue(cur, segs[i], out next))
                    return false;

                cur = next;
            }

            value = cur;
            return true;
        }

        private static bool TryGetMemberValue(object obj, string name, out object value)
        {
            value = null;
            if (obj == null || string.IsNullOrEmpty(name)) return false;

            var t = obj.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public;

            // 屬性
            var pi = t.GetProperty(name, flags);
            if (pi != null && pi.CanRead)
            {
                try { value = pi.GetValue(obj, null); return true; } catch { }
            }

            // 欄位
            var fi = t.GetField(name, flags);
            if (fi != null)
            {
                try { value = fi.GetValue(obj); return true; } catch { }
            }

            return false;
        }

        // ===== 判斷是否為 Terminal 類型（無 isTestItem 也要保存） =====
        //private static bool IsTerminalType(Type t)
        //{
        //    if (t == null) return false;
        //    // 以 base class 判斷為主，再以名稱包含 Terminal 備援
        //    return typeof(Manufacture.TerminalNode).IsAssignableFrom(t)
        //        || t.Name.IndexOf("Terminal", StringComparison.OrdinalIgnoreCase) >= 0;
        //}

        // ===== isTestItem 過濾 =====

        // 不分大小寫找 IsTestItem / isTestItem 屬性或欄位；轉 bool 後回傳
        private static bool HasTrueIsTestItem(object obj)
        {
            if (obj == null) return false;

            var t = obj.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;

            // 屬性
            var pi = t.GetProperty("IsTestItem", flags);
            if (pi != null && pi.CanRead)
            {
                try
                {
                    object v = pi.GetValue(obj, null);
                    bool b; if (BoolTryParse(v, out b)) return b;
                }
                catch { }
            }

            // 欄位
            var fi = t.GetField("IsTestItem", flags);
            if (fi != null)
            {
                try
                {
                    object v = fi.GetValue(obj);
                    bool b; if (BoolTryParse(v, out b)) return b;
                }
                catch { }
            }

            return false;
        }

        private static bool BoolTryParse(object v, out bool b)
        {
            try
            {
                if (v == null) { b = false; return false; }
                if (v is bool) { b = (bool)v; return true; }

                var s = Convert.ToString(v);
                if (string.IsNullOrWhiteSpace(s)) { b = false; return false; }
                s = s.Trim();

                // 常見字串/數字形式
                if (s.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("t", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("on", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("enable", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("enabled", StringComparison.OrdinalIgnoreCase))
                { b = true; return true; }

                if (s.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("f", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("off", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("0", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("disable", StringComparison.OrdinalIgnoreCase) ||
                    s.Equals("disabled", StringComparison.OrdinalIgnoreCase))
                { b = false; return true; }
            }
            catch { }

            b = false; return false;
        }
    }


    public static class SlimTreeHtmlExporter
    {
        // ===== 對外 API：三種輸出入口 =====

        // 1) 全部根節點（原本的用法）
        public static void ExportStrictHtml(
            TreeView tv,
            Dictionary<string, string[]> propsMap,
            string outputPath,
            string title,
            bool expandAll)
        {
            if (tv == null) throw new ArgumentNullException(nameof(tv));
            ExportStrictHtmlCore(Enumerate(tv.Nodes), propsMap, outputPath, title, expandAll);
        }

        // 2) 指定單一子樹
        public static void ExportStrictHtml(
            TreeNode root,
            Dictionary<string, string[]> propsMap,
            string outputPath,
            string title,
            bool expandAll)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            ExportStrictHtmlCore(new[] { root }, propsMap, outputPath, title, expandAll);
        }

        // 3) 指定多個子樹
        public static void ExportStrictHtml(
            IEnumerable<TreeNode> roots,
            Dictionary<string, string[]> propsMap,
            string outputPath,
            string title,
            bool expandAll)
        {
            if (roots == null) throw new ArgumentNullException(nameof(roots));
            ExportStrictHtmlCore(roots, propsMap, outputPath, title, expandAll);
        }

        private static IEnumerable<TreeNode> Enumerate(TreeNodeCollection nodes)
        {
            foreach (TreeNode n in nodes) yield return n;
        }

        // ===== Summary 資料（內部） =====
        private class SumRow
        {
            public string AnchorId;
            public string Title;
            public bool? Enable;
            public string DurationText;
            public int? Retry;
            public string StartText;
            public string EndText;
            public string Verdict;     // PASS / FAIL / ""
            public bool IsContainer;   // 總表只保留 Container
        }

        // ===== 核心：組 HTML =====
        private static void ExportStrictHtmlCore(
            IEnumerable<TreeNode> roots,
            Dictionary<string, string[]> propsMap,
            string outputPath,
            string title,
            bool expandAll)
        {
            // 前處理：收集總表列 + 指派每個節點 anchorId
            var rows = new List<SumRow>();
            var anchorMap = new Dictionary<TreeNode, string>();
            int seq = 0;
            foreach (var root in roots)
                PreCollectSummary(root, propsMap, 0, rows, anchorMap, ref seq);

            var sb = new StringBuilder(1024 * 128);
            var pageTitle = string.IsNullOrEmpty(title) ? "Test Result" : title;

            // Head + CSS
            sb.AppendLine("<!doctype html>");
            sb.AppendLine("<html lang=\"zh-Hant\"><head><meta charset=\"utf-8\">");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            sb.Append("<title>").Append(Html(pageTitle)).AppendLine("</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("html,body{font-family:-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Helvetica,Arial,\"Noto Sans CJK TC\",sans-serif;line-height:1.5;background:#0b1020;color:#f1f5f9;margin:0}");
            sb.AppendLine(".wrap{max-width:1200px;margin:28px auto;padding:0 16px}");
            sb.AppendLine("h1{font-size:20px;margin:0 0 12px}");
            sb.AppendLine(".toolbar{display:flex;gap:12px;align-items:center;margin:12px 0 16px;flex-wrap:wrap}");
            sb.AppendLine(".search{flex:1 1 420px;min-width:0}");
            sb.AppendLine(".toolbar label{margin-left:auto;white-space:nowrap}");
            sb.AppendLine(".search input{width:100%;padding:10px 12px;border:1px solid #334155;border-radius:10px;background:#0f172a;color:#e2e8f0}");
            sb.AppendLine(".badge{font-size:12px;border-radius:999px;padding:2px 8px;border:1px solid #334155;background:#0b1222;color:#cbd5e1}");
            sb.AppendLine(".badge.dur{border-color:#2563eb;background:#0b1b3a;color:#93c5fd}");
            sb.AppendLine(".badge.retry{border-color:#f59e0b;background:#1a1505;color:#fde68a}");
            sb.AppendLine(".badge.pass{border-color:#16a34a;background:#07210c;color:#86efac}");
            sb.AppendLine(".badge.fail{border-color:#ef4444;background:#2a0d0d;color:#fca5a5}");
            sb.AppendLine(".muted{color:#94a3b8}");
            sb.AppendLine(".ok{background:rgba(34,197,94,.18);border:1px solid rgba(34,197,94,.35);border-radius:6px;padding:0 6px}");
            sb.AppendLine(".bad{background:rgba(239,68,68,.25);border:1px solid rgba(239,68,68,.45);border-radius:6px;padding:0 6px}");
            sb.AppendLine("mark{background:#f59e0b33;border-radius:4px;padding:0 2px}");
            sb.AppendLine(".node.disabled{opacity:.55;filter:grayscale(.35);}");   // Enable 未勾選 → 灰化

            // Summary 區塊
            sb.AppendLine(".summary{background:#0f172a;border:1px solid #1e293b;border-radius:12px;padding:12px;margin:8px 0 18px;box-shadow:0 2px 10px rgba(0,0,0,.25)}");
            sb.AppendLine(".summary .stoolbar{display:flex;gap:12px;align-items:center;margin-bottom:10px}");
            sb.AppendLine(".summary .stoolbar input[type=text]{flex:1;padding:8px 10px;border:1px solid #334155;border-radius:8px;background:#0b1326;color:#e2e8f0}");
            sb.AppendLine(".summary table{width:100%;border-collapse:collapse;font-size:13px}");
            sb.AppendLine(".summary thead th{position:sticky;top:0;background:#0f172a;padding:8px;border-bottom:1px solid #334155;text-align:left}");
            sb.AppendLine(".summary tbody td{padding:8px;border-top:1px dashed #243042;vertical-align:top}");
            sb.AppendLine(".summary tbody tr:hover{background:#0b1326;cursor:pointer}");
            sb.AppendLine(".summary .nm a{color:#93c5fd;text-decoration:none;font-weight:700}");
            sb.AppendLine(".summary .nm a:hover{text-decoration:underline}");

            sb.AppendLine(".summary tbody tr.cont{background:#0f172a;border-left:0;}");
            sb.AppendLine(".summary tbody tr.cont:hover{background:#0b1326;}");

            // Tree 區塊
            sb.AppendLine(".tree{list-style:none;margin:0;padding-left:0}");
            sb.AppendLine(".node{background:#0f172a;border:1px solid #1e293b;border-radius:12px;margin:8px 0;box-shadow:0 2px 10px rgba(0,0,0,.25)}");
            sb.AppendLine(".row{display:flex;align-items:center;gap:10px;padding:10px 12px;border-bottom:1px solid #1e293b}");
            sb.AppendLine(".toggle{cursor:pointer;user-select:none;font-size:14px;line-height:1;width:18px;text-align:center;color:#94a3b8}");
            sb.AppendLine(".title{font-weight:600;color:#e2e8f0}");
            sb.AppendLine(".badges{margin-left:auto;display:flex;gap:6px;align-items:center}");
            sb.AppendLine(".props{padding:10px 12px;display:none}");
            sb.AppendLine(".props table{width:100%;border-collapse:collapse;font-size:13px}");
            sb.AppendLine(".props th{text-align:left;padding:6px 8px;border-bottom:1px solid #334155;color:#93c5fd;font-weight:600}");
            sb.AppendLine(".props td{padding:6px 8px;border-bottom:1px dashed #243042;vertical-align:top}");
            sb.AppendLine(".props td.k{width:200px;color:#94a3b8}");
            sb.AppendLine(".props table.sub{width:100%;border-collapse:collapse;font-size:13px}");
            sb.AppendLine(".children{list-style:none;margin:0;padding:8px 14px 12px 24px;display:none;border-top:1px solid #1e293b}");
            sb.AppendLine(".node.expanded .props,.node.expanded>.children{display:block}");
            sb.AppendLine(".toggle::before{content:'▶';display:inline-block;transform:rotate(0deg);transition:.15s transform}");
            sb.AppendLine(".node.expanded .toggle::before{transform:rotate(90deg)}");
            sb.AppendLine(".flash{animation:flash 1.2s ease-out 1}");
            sb.AppendLine("@keyframes flash{0%{box-shadow:0 0 0 rgba(59,130,246,0.0)}30%{box-shadow:0 0 0 3px rgba(59,130,246,0.35)}100%{box-shadow:0 0 0 rgba(59,130,246,0.0)}}");
            sb.AppendLine("</style></head><body><div class=\"wrap\">");

            sb.Append("<h1>").Append(Html(pageTitle)).AppendLine("</h1>");

            // Summary：只列 Container
            sb.AppendLine("<div class=\"summary\">");
            sb.AppendLine("  <div class=\"stoolbar\">");
            sb.AppendLine("    <input id=\"qs\" type=\"text\" placeholder=\"搜尋總表...（名稱）\">");
            sb.AppendLine("    <label class=\"muted\"><input id=\"failOnly\" type=\"checkbox\"> 只顯示 FAIL</label>");
            sb.AppendLine("  </div>");
            sb.AppendLine("  <div class=\"stablewrap\">");
            sb.AppendLine("    <table class=\"sum\"><thead><tr>");
            sb.AppendLine("      <th style=\"width:90px\">Enable</th><th>名稱</th><th style=\"width:100px\">耗時(s)</th><th style=\"width:80px\">重試</th><th style=\"width:180px\">開始</th><th style=\"width:180px\">結束</th><th style=\"width:90px\">結果</th>");
            sb.AppendLine("    </tr></thead><tbody id=\"sumBody\">");

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                sb.Append("<tr class=\"")
                  .Append(r.IsContainer ? "cont" : "")
                  .Append("\" data-title=\"").Append(Html((r.Title ?? "").ToLowerInvariant()))
                  .Append("\" data-verdict=\"").Append(Html(r.Verdict ?? ""))
                  .Append("\" data-target=\"").Append(r.AnchorId).Append("\">");

                // Enable
                sb.Append("<td><input type=\"checkbox\" class=\"en\" data-target=\"").Append(r.AnchorId).Append("\"");
                if (r.Enable.HasValue && r.Enable.Value) sb.Append(" checked");
                sb.Append("></td>");

                // 名稱（可點跳到樹）
                sb.Append("<td class=\"nm\"><a href=\"#").Append(r.AnchorId).Append("\" data-target=\"").Append(r.AnchorId).Append("\">")
                  .Append(Html(r.Title)).Append("</a></td>");

                // 其他欄
                sb.Append("<td>").Append(Html(r.DurationText ?? "")).Append("</td>");
                sb.Append("<td>").Append(r.Retry.HasValue ? r.Retry.Value.ToString() : "").Append("</td>");
                sb.Append("<td>").Append(Html(r.StartText ?? "")).Append("</td>");
                sb.Append("<td>").Append(Html(r.EndText ?? "")).Append("</td>");
                sb.Append("<td>");
                if (r.Verdict == "FAIL") sb.Append("<span class=\"badge fail\">FAIL</span>");
                else if (r.Verdict == "PASS") sb.Append("<span class=\"badge pass\">PASS</span>");
                else sb.Append("<span class=\"muted\">-</span>");
                sb.Append("</td>");

                sb.AppendLine("</tr>");
            }
            sb.AppendLine("    </tbody></table>");
            sb.AppendLine("  </div>");
            sb.AppendLine("</div>");

            // 樹的搜尋工具列
            sb.AppendLine("<div class=\"toolbar\"><div class=\"search\"><input id=\"q\" placeholder=\"搜尋樹狀節點... (即時過濾)\"></div>");
            sb.AppendLine("<label class=\"muted\"><input id=\"toggleAll\" type=\"checkbox\"> 全部展開</label></div>");

            // 樹
            sb.AppendLine("<ul class=\"tree\" id=\"tree\">");
            foreach (var root in roots)
                RenderNodeStrict(root, sb, propsMap, expandAll, 0, anchorMap);
            sb.AppendLine("</ul>");

            // JS（含 IE polyfill）
            sb.AppendLine("<script>(function(){");
            sb.AppendLine("if(!Element.prototype.matches){Element.prototype.matches=Element.prototype.msMatchesSelector||Element.prototype.webkitMatchesSelector||function(s){var m=(this.document||this.ownerDocument).querySelectorAll(s),i=m.length;while(--i>=0&&m.item(i)!==this){}return i>-1;};}");
            sb.AppendLine("if(!Element.prototype.closest){Element.prototype.closest=function(s){var el=this;while(el&&el.nodeType===1){if(el.matches(s))return el;el=el.parentElement;}return null;};}");

            sb.AppendLine("var q=document.getElementById('q');var qs=document.getElementById('qs');var failOnly=document.getElementById('failOnly');var toggleAll=document.getElementById('toggleAll');");

            // Summary filter
            sb.AppendLine("function filterSummary(){var term=(qs&&qs.value?qs.value.trim().toLowerCase():'');var fail=(failOnly&&failOnly.checked);var rows=[].slice.call(document.querySelectorAll('#sumBody tr'));");
            sb.AppendLine("for(var i=0;i<rows.length;i++){var r=rows[i];var t=r.getAttribute('data-title')||'';var v=r.getAttribute('data-verdict')||'';");
            sb.AppendLine("var hit=(!term || t.indexOf(term)>=0) && (!fail || v==='FAIL');");
            sb.AppendLine("r.style.display=hit?'table-row':'none';}}");
            sb.AppendLine("if(qs){qs.addEventListener('input',filterSummary);} if(failOnly){failOnly.addEventListener('change',filterSummary);}");

            // 初始 Enable 灰化
            sb.AppendLine("function applyInitialEnable(){var cbs=document.querySelectorAll('#sumBody input.en');for(var i=0;i<cbs.length;i++){var cb=cbs[i];var id=cb.getAttribute('data-target');var node=document.getElementById(id);if(!cb.checked&&node){node.classList.add('disabled');}}}");
            sb.AppendLine("applyInitialEnable();");

            // Summary click → jump
            sb.AppendLine("function expandAncestors(el){var p=el;while(p){if(p.classList&&p.classList.contains('node'))p.classList.add('expanded');p=p.parentElement;if(!p)break;p=p.closest('.node');}}");
            sb.AppendLine("function jumpTo(id){var t=document.getElementById(id);if(!t)return;expandAncestors(t);t.scrollIntoView({behavior:'smooth',block:'center'});t.classList.remove('flash');void t.offsetWidth;t.classList.add('flash');}");
            sb.AppendLine("document.getElementById('sumBody').addEventListener('click',function(e){var a=e.target.closest('a[data-target]');var tr=e.target.closest('tr');var id=(a&&a.getAttribute('data-target'))||(tr&&tr.getAttribute('data-target'));if(id){e.preventDefault();jumpTo(id);}});");

            // Enable 勾選 → 灰化樹節點（僅視覺）
            sb.AppendLine("document.getElementById('sumBody').addEventListener('change',function(e){var t=e.target;if(t&&t.classList.contains('en')){var id=t.getAttribute('data-target');var node=document.getElementById(id);if(node){if(t.checked)node.classList.remove('disabled');else node.classList.add('disabled');}}});");

            // 樹的展開/搜尋
            sb.AppendLine("function bindToggles(){var t=document.querySelectorAll('.toggle');for(var i=0;i<t.length;i++){t[i].onclick=function(){var p=this.closest('.node');if(p){p.classList.toggle('expanded');}}}}");
            sb.AppendLine("bindToggles();");
            sb.AppendLine("if(toggleAll){toggleAll.checked=" + (expandAll ? "true" : "false") + ";toggleAll.addEventListener('change',function(){var n=document.querySelectorAll('.node');for(var i=0;i<n.length;i++){if(this.checked)n[i].classList.add('expanded');else n[i].classList.remove('expanded');}});}");

            sb.AppendLine("function escapeRegExp(s){return s.replace(/[.*+?^${}()|[\\]\\\\]/g,'\\\\$&');}");
            sb.AppendLine("function clearMarks(el){var t=el.querySelector('.title');if(!t)return;var raw=t.getAttribute('data-raw');if(raw!==null){t.innerHTML=raw;}else{t.setAttribute('data-raw',t.innerHTML);}}");
            sb.AppendLine("function markTerms(el,terms){var t=el.querySelector('.title');if(!t)return;var raw=t.getAttribute('data-raw');if(raw===null){raw=t.innerHTML;t.setAttribute('data-raw',raw);}var html=raw;for(var i=0;i<terms.length;i++){if(!terms[i])continue;var re=new RegExp('('+escapeRegExp(terms[i])+')','ig');html=html.replace(re,'<mark>$1</mark>');}t.innerHTML=html;}");
            sb.AppendLine("function filterTree(){var term=(q&&q.value?q.value.trim().toLowerCase():'');var terms=term.split(/\\s+/).filter(Boolean);var nodes=[].slice.call(document.querySelectorAll('.node'));var show=new Set();");
            sb.AppendLine("for(var i=0;i<nodes.length;i++){var n=nodes[i];clearMarks(n);var txt=(n.getAttribute('data-text')||'').toLowerCase();var hit=!terms.length||terms.every(function(t){return txt.indexOf(t)>=0;});if(hit){show.add(n);var p=n.parentElement;while(p){if(p.classList&&p.classList.contains('node'))show.add(p);p=p.parentElement;}}}");
            sb.AppendLine("for(var i=0;i<nodes.length;i++){var n=nodes[i];if(show.has(n)){n.style.display='block';n.classList.add('expanded');if(terms.length)markTerms(n,terms);}else{n.style.display='none';}}}");
            sb.AppendLine("if(q){q.addEventListener('input',filterTree);}");

            sb.AppendLine("})();</script>");

            sb.AppendLine("</div></body></html>");

            var dir = System.IO.Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir)) System.IO.Directory.CreateDirectory(dir);
            System.IO.File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }

        // ===== 前處理：彙整總表列 + 指派 Anchor =====
        private static void PreCollectSummary(
            TreeNode tn,
            Dictionary<string, string[]> propsMap,
            int level,
            List<SumRow> outRows,
            Dictionary<TreeNode, string> anchorMap,
            ref int seq)
        {
            if (tn == null || tn.Tag == null) return;
            var cb = tn.Tag as CoreBase; if (cb == null) return;

            string[] propsForThis;
            bool inWhitelist = TryResolvePropsForType(tn.Tag.GetType(), propsMap, out propsForThis);

            // 每個節點都配一個 anchor
            string myId = "n" + (++seq);
            anchorMap[tn] = myId;

            // 只把「Container」加入總表

            bool isContainer = IsContainerType(tn.Tag.GetType());
            if (inWhitelist && isContainer)
            {
                var kvs = ExtractPropsInOrder(tn.Tag, propsForThis);

                // ★ NEW: 用 isTestItem 決定是否列入總表（預設不列入）
                bool include = false;
                int iTI = IndexOfKey(kvs, "isTestItem");   // 大小寫不敏感
                if (iTI >= 0)
                {
                    bool b;
                    if (BoolTryParse(kvs[iTI].Value, out b)) include = b;
                }
                if (!include)
                {
                    // 不加入總表，但仍會往下遞迴處理子節點
                    goto CHILDREN;
                }

                // 標題
                string title = GetTitleFromKVs(kvs);

                // Enable
                bool? en = null;
                int iEn = IndexOfKey(kvs, "Enable");
                if (iEn >= 0) { bool b; if (BoolTryParse(kvs[iEn].Value, out b)) en = b; }

                // 耗時
                string durText = null;
                int iDur = IndexOfKey(kvs, "DurationSec");
                if (iDur >= 0 && kvs[iDur].Value != null)
                { double d; if (DoubleTryParse(kvs[iDur].Value, out d)) durText = d.ToString("0.000"); }

                // 重試
                int? retry = null;
                int iRetry = IndexOfKey(kvs, "RetryCount");
                if (iRetry >= 0 && kvs[iRetry].Value != null)
                { int r; if (IntTryParse(kvs[iRetry].Value, out r)) retry = r; }

                // 起訖
                string startText = null, endText = null;
                int iStart = IndexOfKey(kvs, "StartTime"); if (iStart >= 0) startText = FormatDateTime(kvs[iStart].Value);
                int iEnd = IndexOfKey(kvs, "EndTime"); if (iEnd >= 0) endText = FormatDateTime(kvs[iEnd].Value);

                // Verdict：Container 優先用 Result，無才回退規格
                string verdict = "";
                int iRes = IndexOfKey(kvs, "Result");
                if (iRes >= 0) verdict = VerdictFromResultString(kvs[iRes].Value);
                if (string.IsNullOrEmpty(verdict))
                {
                    int specIdx = IndexOfKey(kvs, "Spec");
                    int outIdx = IndexOfKey(kvs, "OutputData");
                    Dictionary<string, object> outDict = null;
                    if (outIdx >= 0 && kvs[outIdx].Value is string)
                        TryParseOutputDataDict((string)kvs[outIdx].Value, out outDict);
                    if (specIdx >= 0 && kvs[specIdx].Value is string)
                    {
                        int fails; AnalyzeSpecFails((string)kvs[specIdx].Value, outDict, out fails);
                        verdict = (fails > 0) ? "FAIL" : "PASS";
                    }
                }

                outRows.Add(new SumRow
                {
                    AnchorId = myId,
                    Title = title,
                    Enable = en,
                    DurationText = durText,
                    Retry = retry,
                    StartText = startText,
                    EndText = endText,
                    Verdict = verdict,
                    IsContainer = true
                });
            }

        CHILDREN:
            for (int i = 0; i < tn.Nodes.Count; i++)
                PreCollectSummary(tn.Nodes[i], propsMap, level + 1, outRows, anchorMap, ref seq);
        }

        private static bool IsContainerType(Type t)
        {
            if (t == null) return false;
            return typeof(Manufacture.ContainerNode).IsAssignableFrom(t)
                || t.Name.IndexOf("Container", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string FormatDateTime(object v)
        {
            if (v == null) return null;
            if (v is DateTime) return ((DateTime)v).ToString("yyyy-MM-dd HH:mm:ss");
            if (v is DateTime?)
            {
                var dn = (DateTime?)v;
                return dn.HasValue ? dn.Value.ToString("yyyy-MM-dd HH:mm:ss") : null;
            }
            return null;
        }

        // ===== 詳細樹：渲染（依白名單 + 規格標示） =====
        private static void RenderNodeStrict(
            TreeNode tn,
            StringBuilder sb,
            Dictionary<string, string[]> propsMap,
            bool expanded,
            int depth,
            Dictionary<TreeNode, string> anchorMap)
        {
            if (tn == null || tn.Tag == null) return;
            var cb = tn.Tag as CoreBase; if (cb == null) return;

            string[] propsForThis;
            if (!TryResolvePropsForType(tn.Tag.GetType(), propsMap, out propsForThis))
            {
                for (int i = 0; i < tn.Nodes.Count; i++)
                    RenderNodeStrict(tn.Nodes[i], sb, propsMap, expanded, depth + 1, anchorMap);
                return;
            }

            var kvs = ExtractPropsInOrder(tn.Tag, propsForThis);

            // OutputData → dict（供 Spec 對照）
            Dictionary<string, object> outputDict = null;
            int outIdx = IndexOfKey(kvs, "OutputData");
            if (outIdx >= 0 && kvs[outIdx].Value is string)
                TryParseOutputDataDict((string)kvs[outIdx].Value, out outputDict);

            // 預先計算 Spec fail 數
            int specFailCount = 0;
            bool hasSpec = false;
            int specIdx = IndexOfKey(kvs, "Spec");
            if (specIdx >= 0 && kvs[specIdx].Value is string)
            {
                hasSpec = true;
                AnalyzeSpecFails((string)kvs[specIdx].Value, outputDict, out specFailCount);
            }

            // 標題
            string title = GetTitleFromKVs(kvs);

            // 徽章：Duration / Retry / Verdict
            string durBadge = null;
            int iDur = IndexOfKey(kvs, "DurationSec");
            if (iDur >= 0 && kvs[iDur].Value != null)
            { double d; if (DoubleTryParse(kvs[iDur].Value, out d)) durBadge = d.ToString("0.000") + " s"; }

            string retryBadge = null;
            int iRetry = IndexOfKey(kvs, "RetryCount");
            if (iRetry >= 0 && kvs[iRetry].Value != null)
            { int r; if (IntTryParse(kvs[iRetry].Value, out r) && r > 0) retryBadge = "x" + r.ToString(); }

            string verdictBadgeClass = null, verdictBadgeText = null;
            bool isCont = IsContainerType(tn.Tag.GetType());
            if (isCont)
            {
                int iRes = IndexOfKey(kvs, "Result");
                string vStr = (iRes >= 0) ? VerdictFromResultString(kvs[iRes].Value) : "";
                if (vStr == "PASS") { verdictBadgeClass = "pass"; verdictBadgeText = "PASS"; }
                else if (vStr == "FAIL") { verdictBadgeClass = "fail"; verdictBadgeText = "FAIL"; }
            }
            if (verdictBadgeClass == null && hasSpec)
            {
                if (specFailCount > 0) { verdictBadgeClass = "fail"; verdictBadgeText = "FAIL"; }
                else { verdictBadgeClass = "pass"; verdictBadgeText = "PASS"; }
            }

            // 搜尋文字
            var filterText = new StringBuilder();
            filterText.Append(title);
            for (int f = 0; f < kvs.Count; f++)
                if (kvs[f].Value != null) filterText.Append(' ').Append(ToFlatString(kvs[f].Value));

            // li 開頭 + anchor
            string anchorId; if (!anchorMap.TryGetValue(tn, out anchorId)) { anchorId = "n0"; }

            sb.Append("<li id=\"").Append(anchorId).Append("\" class=\"node")
              .Append(expanded ? " expanded" : "").Append("\" data-text=\"")
              .Append(Html(filterText.ToString().ToLowerInvariant())).Append("\">");

            // header
            sb.Append("<div class=\"row\">")
              .Append("<span class=\"toggle\" title=\"展開/收合\"></span>")
              .Append("<span class=\"title\">").Append(Html(title)).Append("</span>")
              .Append("<span class=\"badges\">");
            if (!string.IsNullOrEmpty(durBadge)) sb.Append("<span class=\"badge dur\" title=\"耗時(秒)\">").Append(Html(durBadge)).Append("</span>");
            if (!string.IsNullOrEmpty(retryBadge)) sb.Append("<span class=\"badge retry\" title=\"重試次數\">").Append(Html(retryBadge)).Append("</span>");
            if (!string.IsNullOrEmpty(verdictBadgeClass)) sb.Append("<span class=\"badge ").Append(verdictBadgeClass).Append("\" title=\"規格檢核或結果\">").Append(verdictBadgeText).Append("</span>");
            sb.Append("</span></div>");

            // 屬性表
            bool hasTableRow = false;
            var table = new StringBuilder();
            for (int k = 0; k < kvs.Count; k++)
            {
                string key = kvs[k].Key;
                if (StringEqualsAny(key, "Description", "DurationSec", "RetryCount")) continue;

                object v = kvs[k].Value;
                string vs;

                if (string.Equals(key, "Spec", StringComparison.OrdinalIgnoreCase) && v is string)
                {
                    int rowFails;
                    if (!TryRenderSpecHtml((string)v, outputDict, out vs, out rowFails)) vs = Html(Convert.ToString(v));
                }
                else if (string.Equals(key, "OutputData", StringComparison.OrdinalIgnoreCase) && v is string)
                {
                    if (!TryRenderOutputDataHtml((string)v, out vs)) vs = Html(Convert.ToString(v));
                }
                else
                {
                    vs = FormatValue(v);
                }

                table.Append("<tr><td class=\"k\">").Append(Html(key)).Append("</td><td>").Append(vs).Append("</td></tr>");
                hasTableRow = true;
            }

            if (hasTableRow)
                sb.Append("<div class=\"props\"><table><tbody>").Append(table.ToString()).Append("</tbody></table></div>");

            // 子節點
            RenderChildrenStrict(tn, sb, propsMap, expanded, depth + 1, anchorMap);

            sb.Append("</li>");
        }

        private static bool RenderChildrenStrict(
            TreeNode tn,
            StringBuilder sb,
            Dictionary<string, string[]> propsMap,
            bool expanded,
            int depth,
            Dictionary<TreeNode, string> anchorMap)
        {
            var buffer = new StringBuilder();
            int count = 0;
            for (int i = 0; i < tn.Nodes.Count; i++)
            {
                int oldLen = buffer.Length;
                RenderNodeStrict(tn.Nodes[i], buffer, propsMap, expanded, depth + 1, anchorMap);
                if (buffer.Length > oldLen) count++;
            }

            if (count > 0)
            {
                sb.Append("<ul class=\"children\">").Append(buffer.ToString()).Append("</ul>");
                return true;
            }
            return false;
        }

        // ===== 規格解析與渲染 =====
        // ===== 規格解析與渲染（修正版：支援多種 SpecType / ConditionType） =====
        // === 用這個覆蓋你現有的 TryRenderSpecHtml ===
        private static bool TryRenderSpecHtml(string specJson, Dictionary<string, object> outputData, out string html, out int failCount)
        {
            html = null; failCount = 0;
            if (string.IsNullOrWhiteSpace(specJson)) return false;

            try
            {
                // 1) SpecParamsContainer
                SpecParamsContainer cont = JsonConvert.DeserializeObject<SpecParamsContainer>(specJson);
                if (cont != null && cont.specParams != null && cont.specParams.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.Append("<div class=\"spec\"><table class=\"sub\"><thead><tr>")
                      .Append("<th>Name</th><th>Type</th><th>Min</th><th>Max</th><th>SpecValue</th><th>MES</th><th>CSV</th><th>Value</th><th>Result</th>")
                      .Append("</tr></thead><tbody>");

                    for (int i = 0; i < cont.specParams.Count; i++)
                    {
                        var s = cont.specParams[i];
                        string name = Coalesce(s.Name, s.NameA, s.NameB);

                        object oVal; bool hasVal = TryGetOutputValue(outputData, name, out oVal);
                        string valueText = hasVal ? ToFlatString(oVal) : "(missing)";

                        bool? pass = null;
                        string valueCellClass = "";

                        // Range
                        if (s.SpecType == SpecType.Range && hasVal)
                        {
                            double vNum, min, max;
                            if (DoubleTryParse(oVal, out vNum) && DoubleTryParse(s.MinLimit, out min) && DoubleTryParse(s.MaxLimit, out max))
                            {
                                if (vNum < min || vNum > max) { pass = false; valueCellClass = "bad"; failCount++; }
                                else { pass = true; valueCellClass = "ok"; }
                                valueText = vNum.ToString("0.###");
                            }
                        }
                        else
                        {
                            // Equal/NotEqual（用名稱比對，避免 enum 不同專案差異）
                            string typeName = s.SpecType.ToString();
                            bool isEq = string.Equals(typeName, "Equal", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(typeName, "Equals", StringComparison.OrdinalIgnoreCase);
                            bool isNE = string.Equals(typeName, "NotEqual", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(typeName, "NotEquals", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(typeName, "NE", StringComparison.OrdinalIgnoreCase);

                            if ((isEq || isNE) && hasVal)
                            {
                                bool p = EvalEqualOrNotEqual(oVal, s.SpecValue, isNE, out valueText);
                                pass = p; valueCellClass = p ? "ok" : "bad"; if (!p) failCount++;
                            }
                        }

                        sb.Append("<tr>")
                          .Append("<td>").Append(Html(name)).Append("</td>")
                          .Append("<td>").Append(Html(s.SpecType.ToString())).Append("</td>")
                          .Append("<td>").Append(Html(FormatNum(s.MinLimit))).Append("</td>")
                          .Append("<td>").Append(Html(FormatNum(s.MaxLimit))).Append("</td>")
                          .Append("<td>").Append(Html(NullSafe(s.SpecValue))).Append("</td>")
                          .Append("<td>").Append(Html(NullSafe(s.Mes))).Append("</td>")
                          .Append("<td>").Append(Html(NullSafe(s.Csv))).Append("</td>")
                          .Append("<td>").Append("<span class=\"").Append(valueCellClass).Append("\">").Append(Html(valueText)).Append("</span></td>")
                          .Append("<td>").Append(pass.HasValue ? (pass.Value ? "<span class=\"ok\">PASS</span>" : "<span class=\"bad\">FAIL</span>") : "<span class=\"muted\">-</span>").Append("</td>")
                          .Append("</tr>");
                    }

                    sb.Append("</tbody></table></div>");
                    html = sb.ToString();
                    return true;
                }

                // 2) ConditionList
                ConditionList cl = JsonConvert.DeserializeObject<ConditionList>(specJson);
                if (cl != null && cl.Conditions != null && cl.Conditions.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.Append("<div class=\"spec\"><table class=\"sub\"><thead><tr>")
                      .Append("<th>Name</th><th>Type</th><th>Min</th><th>Max</th><th>SpecValue</th><th>Goto</th><th>Value</th><th>Result</th>")
                      .Append("</tr></thead><tbody>");

                    for (int i = 0; i < cl.Conditions.Count; i++)
                    {
                        var c = cl.Conditions[i];
                        string name = Coalesce(c.Name, c.NameA, c.NameB);

                        object oVal; bool hasVal = TryGetOutputValue(outputData, name, out oVal);
                        string valueText = hasVal ? ToFlatString(oVal) : "(missing)";

                        bool? pass = null;
                        string valueCellClass = "";

                        // Range
                        if (c.SpecType == ConditionType.Range && hasVal)
                        {
                            double vNum, min, max;
                            if (DoubleTryParse(oVal, out vNum) && DoubleTryParse(c.MinLimit, out min) && DoubleTryParse(c.MaxLimit, out max))
                            {
                                if (vNum < min || vNum > max) { pass = false; valueCellClass = "bad"; failCount++; }
                                else { pass = true; valueCellClass = "ok"; }
                                valueText = vNum.ToString("0.###");
                            }
                        }
                        else
                        {
                            // Equal/NotEqual
                            string typeName = c.SpecType.ToString();
                            bool isEq = string.Equals(typeName, "Equal", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(typeName, "Equals", StringComparison.OrdinalIgnoreCase);
                            bool isNE = string.Equals(typeName, "NotEqual", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(typeName, "NotEquals", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(typeName, "NE", StringComparison.OrdinalIgnoreCase);

                            if ((isEq || isNE) && hasVal)
                            {
                                bool p = EvalEqualOrNotEqual(oVal, c.SpecValue, isNE, out valueText);
                                pass = p; valueCellClass = p ? "ok" : "bad"; if (!p) failCount++;
                            }
                        }

                        sb.Append("<tr>")
                          .Append("<td>").Append(Html(name)).Append("</td>")
                          .Append("<td>").Append(Html(c.SpecType.ToString())).Append("</td>")
                          .Append("<td>").Append(Html(NullSafe(c.MinLimit))).Append("</td>")
                          .Append("<td>").Append(Html(NullSafe(c.MaxLimit))).Append("</td>")
                          .Append("<td>").Append(Html(NullSafe(c.SpecValue))).Append("</td>")
                          .Append("<td>").Append(Html(NullSafe(c.Goto))).Append("</td>")
                          .Append("<td>").Append("<span class=\"").Append(valueCellClass).Append("\">").Append(Html(valueText)).Append("</span></td>")
                          .Append("<td>").Append(pass.HasValue ? (pass.Value ? "<span class=\"ok\">PASS</span>" : "<span class=\"bad\">FAIL</span>") : "<span class=\"muted\">-</span>").Append("</td>")
                          .Append("</tr>");
                    }

                    sb.Append("</tbody></table></div>");
                    html = sb.ToString();
                    return true;
                }
            }
            catch { /* ignore */ }

            return false;
        }


        // 供總表徽章/統計用（同上邏輯，僅計 FAIL）
        // === 用這個覆蓋你現有的 AnalyzeSpecFails（讓標題徽章/總表也能算到 NotEqual/Equal） ===
        private static void AnalyzeSpecFails(string specJson, Dictionary<string, object> outputData, out int failCount)
        {
            failCount = 0;
            if (string.IsNullOrWhiteSpace(specJson)) return;

            try
            {
                // SpecParamsContainer
                var cont = JsonConvert.DeserializeObject<SpecParamsContainer>(specJson);
                if (cont != null && cont.specParams != null)
                {
                    for (int i = 0; i < cont.specParams.Count; i++)
                    {
                        var s = cont.specParams[i];
                        string name = Coalesce(s.Name, s.NameA, s.NameB);

                        object oVal; if (!TryGetOutputValue(outputData, name, out oVal)) continue;

                        if (s.SpecType == SpecType.Range)
                        {
                            double v, min, max;
                            if (DoubleTryParse(oVal, out v) && DoubleTryParse(s.MinLimit, out min) && DoubleTryParse(s.MaxLimit, out max))
                                if (v < min || v > max) failCount++;
                        }
                        else
                        {
                            string t = s.SpecType.ToString();
                            bool isEq = string.Equals(t, "Equal", StringComparison.OrdinalIgnoreCase) || string.Equals(t, "Equals", StringComparison.OrdinalIgnoreCase);
                            bool isNE = string.Equals(t, "NotEqual", StringComparison.OrdinalIgnoreCase) || string.Equals(t, "NotEquals", StringComparison.OrdinalIgnoreCase) || string.Equals(t, "NE", StringComparison.OrdinalIgnoreCase);

                            if (isEq || isNE)
                            {
                                string dummy;
                                bool pass = EvalEqualOrNotEqual(oVal, s.SpecValue, isNE, out dummy);
                                if (!pass) failCount++;
                            }
                        }
                    }
                    return;
                }

                // ConditionList
                var cl = JsonConvert.DeserializeObject<ConditionList>(specJson);
                if (cl != null && cl.Conditions != null)
                {
                    for (int i = 0; i < cl.Conditions.Count; i++)
                    {
                        var c = cl.Conditions[i];
                        string name = Coalesce(c.Name, c.NameA, c.NameB);

                        object oVal; if (!TryGetOutputValue(outputData, name, out oVal)) continue;

                        if (c.SpecType == ConditionType.Range)
                        {
                            double v, min, max;
                            if (DoubleTryParse(oVal, out v) && DoubleTryParse(c.MinLimit, out min) && DoubleTryParse(c.MaxLimit, out max))
                                if (v < min || v > max) failCount++;
                        }
                        else
                        {
                            string t = c.SpecType.ToString();
                            bool isEq = string.Equals(t, "Equal", StringComparison.OrdinalIgnoreCase) || string.Equals(t, "Equals", StringComparison.OrdinalIgnoreCase);
                            bool isNE = string.Equals(t, "NotEqual", StringComparison.OrdinalIgnoreCase) || string.Equals(t, "NotEquals", StringComparison.OrdinalIgnoreCase) || string.Equals(t, "NE", StringComparison.OrdinalIgnoreCase);

                            if (isEq || isNE)
                            {
                                string dummy;
                                bool pass = EvalEqualOrNotEqual(oVal, c.SpecValue, isNE, out dummy);
                                if (!pass) failCount++;
                            }
                        }
                    }
                }
            }
            catch { }
        }


        // ===== 單列判定：SpecParam =====
        private static void EvaluateSpecParam(
            SpecParam s,
            Dictionary<string, object> outputData,
            out bool? pass,
            out string shownValue,
            out string valueCellClass)
        {
            pass = null;
            valueCellClass = "";
            shownValue = "(missing)";

            string name = Coalesce(s.Name, s.NameA, s.NameB);

            object oVal;
            bool hasVal = TryGetOutputValue(outputData, name, out oVal);
            string valStr = hasVal ? ToFlatString(oVal) : null;

            // 預設顯示值
            shownValue = hasVal ? valStr : "(missing)";

            // 判定
            switch (s.SpecType)
            {
                case SpecType.Bypass:
                    pass = true;
                    break;

                case SpecType.Range:
                    {
                        double vNum, min, max;
                        if (hasVal && DoubleTryParse(oVal, out vNum) &&
                            DoubleTryParse(s.MinLimit, out min) &&
                            DoubleTryParse(s.MaxLimit, out max))
                        {
                            pass = (vNum >= min && vNum <= max);
                            shownValue = vNum.ToString("0.###");
                        }
                        break;
                    }

                case SpecType.Equal:
                    {
                        if (!string.IsNullOrEmpty(s.SpecValue) && hasVal)
                        {
                            double a, b;
                            if (DoubleTryParse(oVal, out a) && DoubleTryParse(s.SpecValue, out b))
                                pass = (Math.Abs(a - b) < 1e-12);
                            else
                                pass = string.Equals(valStr ?? "", s.SpecValue ?? "", StringComparison.OrdinalIgnoreCase);
                        }
                        break;
                    }

                case SpecType.NotEqual:
                    {
                        if (!string.IsNullOrEmpty(s.SpecValue) && hasVal)
                        {
                            double a, b;
                            if (DoubleTryParse(oVal, out a) && DoubleTryParse(s.SpecValue, out b))
                                pass = !(Math.Abs(a - b) < 1e-12);
                            else
                                pass = !string.Equals(valStr ?? "", s.SpecValue ?? "", StringComparison.OrdinalIgnoreCase);
                        }
                        break;
                    }

                case SpecType.GreaterThan:
                    {
                        double vNum, min;
                        if (hasVal && DoubleTryParse(oVal, out vNum) && DoubleTryParse(s.MinLimit, out min))
                        {
                            pass = (vNum > min);
                            shownValue = vNum.ToString("0.###");
                        }
                        break;
                    }

                case SpecType.LessThan:
                    {
                        double vNum, max;
                        if (hasVal && DoubleTryParse(oVal, out vNum) && DoubleTryParse(s.MaxLimit, out max))
                        {
                            pass = (vNum < max);
                            shownValue = vNum.ToString("0.###");
                        }
                        break;
                    }

                case SpecType.Contain:
                    {
                        if (!string.IsNullOrEmpty(s.SpecValue) && hasVal)
                            pass = (valStr ?? "").IndexOf(s.SpecValue, StringComparison.OrdinalIgnoreCase) >= 0;
                        break;
                    }

                case SpecType.Regex:
                    {
                        if (!string.IsNullOrEmpty(s.SpecValue) && hasVal)
                        {
                            try
                            {
                                pass = System.Text.RegularExpressions.Regex.IsMatch(valStr ?? "", s.SpecValue);
                            }
                            catch { pass = null; }
                        }
                        break;
                    }
            }

            if (pass.HasValue)
                valueCellClass = pass.Value ? "ok" : "bad";
        }

        // ===== 單列判定：Condition =====
        private static void EvaluateCondition(
            Condition c,
            Dictionary<string, object> outputData,
            out bool? pass,
            out string shownValue,
            out string valueCellClass)
        {
            pass = null;
            valueCellClass = "";
            shownValue = "(missing)";

            string name = Coalesce(c.Name, c.NameA, c.NameB);

            object oVal;
            bool hasVal = TryGetOutputValue(outputData, name, out oVal);
            string valStr = hasVal ? ToFlatString(oVal) : null;

            shownValue = hasVal ? valStr : "(missing)";

            switch (c.SpecType)
            {
                case ConditionType.Bypass:
                    pass = true;
                    break;

                case ConditionType.Range:
                    {
                        double vNum, min, max;
                        if (hasVal && DoubleTryParse(oVal, out vNum) &&
                            DoubleTryParse(c.MinLimit, out min) &&
                            DoubleTryParse(c.MaxLimit, out max))
                        {
                            pass = (vNum >= min && vNum <= max);
                            shownValue = vNum.ToString("0.###");
                        }
                        break;
                    }

                case ConditionType.Equal:
                    {
                        if (!string.IsNullOrEmpty(c.SpecValue) && hasVal)
                        {
                            double a, b;
                            if (DoubleTryParse(oVal, out a) && DoubleTryParse(c.SpecValue, out b))
                                pass = (Math.Abs(a - b) < 1e-12);
                            else
                                pass = string.Equals(valStr ?? "", c.SpecValue ?? "", StringComparison.OrdinalIgnoreCase);
                        }
                        break;
                    }

                case ConditionType.GreaterThan:
                    {
                        double vNum, min;
                        if (hasVal && DoubleTryParse(oVal, out vNum) && DoubleTryParse(c.MinLimit, out min))
                        {
                            pass = (vNum > min);
                            shownValue = vNum.ToString("0.###");
                        }
                        break;
                    }

                case ConditionType.LessThan:
                    {
                        double vNum, max;
                        if (hasVal && DoubleTryParse(oVal, out vNum) && DoubleTryParse(c.MaxLimit, out max))
                        {
                            pass = (vNum < max);
                            shownValue = vNum.ToString("0.###");
                        }
                        break;
                    }

                case ConditionType.Contains:   // enum 名稱是 Contains
                    {
                        if (!string.IsNullOrEmpty(c.SpecValue) && hasVal)
                            pass = (valStr ?? "").IndexOf(c.SpecValue, StringComparison.OrdinalIgnoreCase) >= 0;
                        break;
                    }
            }

            if (pass.HasValue)
                valueCellClass = pass.Value ? "ok" : "bad";
        }

        // ===== OutputData 與一般值渲染 =====
        private static bool TryRenderOutputDataHtml(string json, out string html)
        {
            html = null;
            if (string.IsNullOrWhiteSpace(json)) return false;

            try
            {
                Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (dict != null && dict.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<div class=\"output\"><table class=\"sub\"><thead><tr>")
                      .Append("<th>Key</th><th>Value</th></tr></thead><tbody>");

                    foreach (KeyValuePair<string, object> kv in dict)
                    {
                        sb.Append("<tr><td>").Append(Html(kv.Key)).Append("</td><td>")
                          .Append(FormatValue(kv.Value)).Append("</td></tr>");
                    }

                    sb.Append("</tbody></table></div>");
                    html = sb.ToString();
                    return true;
                }
            }
            catch { }

            return false;
        }

        private static bool TryParseOutputDataDict(string json, out Dictionary<string, object> dict)
        {
            dict = null;
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                return dict != null;
            }
            catch { return false; }
        }

        private static bool TryGetOutputValue(Dictionary<string, object> outputData, string name, out object value)
        {
            value = null;
            if (outputData == null || string.IsNullOrEmpty(name)) return false;

            object v;
            if (outputData.TryGetValue(name, out v)) { value = v; return true; }

            foreach (KeyValuePair<string, object> kv in outputData)
            {
                if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase))
                { value = kv.Value; return true; }
            }
            return false;
        }

        // ===== 反射 / 公用 =====
        private static List<KeyValuePair<string, object>> ExtractPropsInOrder(object obj, string[] propsForThis)
        {
            var kvs = new List<KeyValuePair<string, object>>();
            for (int p = 0; p < propsForThis.Length; p++)
            {
                string spec = propsForThis[p];
                string alias = null;
                int cut = spec.IndexOf("=>", StringComparison.Ordinal);
                string path = (cut >= 0) ? spec.Substring(0, cut).Trim() : spec;
                if (cut >= 0) alias = spec.Substring(cut + 2).Trim();

                object val;
                if (TryGetValueByPath(obj, path, out val))
                {
                    string key = string.IsNullOrEmpty(alias) ? LastSegment(path) : alias;

                    // DurationSec → 四捨五入三位
                    if (val != null &&
                        (string.Equals(key, "DurationSec", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(LastSegment(path), "DurationSec", StringComparison.OrdinalIgnoreCase)))
                    {
                        try { double d = Convert.ToDouble(val); val = Math.Round(d, 3, MidpointRounding.AwayFromZero); } catch { }
                    }

                    kvs.Add(new KeyValuePair<string, object>(key, val));
                }
            }
            return kvs;
        }

        private static string GetTitleFromKVs(List<KeyValuePair<string, object>> kvs)
        {
            int idxDesc = IndexOfKey(kvs, "Description");
            if (idxDesc >= 0 && kvs[idxDesc].Value != null)
            {
                var s = Convert.ToString(kvs[idxDesc].Value);
                if (!string.IsNullOrEmpty(s)) return s;
            }
            for (int i = 0; i < kvs.Count; i++)
            {
                var s = kvs[i].Value as string;
                if (!string.IsNullOrEmpty(s)) return s;
            }
            return "(節點)";
        }

        private static bool TryResolvePropsForType(Type actualType, Dictionary<string, string[]> propsMap, out string[] props)
        {
            props = null;
            if (actualType == null || propsMap == null || propsMap.Count == 0) return false;

            Type t = actualType;
            while (t != null)
            {
                string key = t.Name; // 若有同名類，可改用 FullName 並在 propsMap 用 FullName
                if (propsMap.ContainsKey(key))
                {
                    props = propsMap[key];
                    return true;
                }
                t = t.BaseType;
            }
            return false;
        }

        private static bool TryGetValueByPath(object obj, string path, out object value)
        {
            value = null;
            if (obj == null || string.IsNullOrEmpty(path)) return false;

            string[] segs = path.Split('.');
            object cur = obj;

            for (int i = 0; i < segs.Length; i++)
            {
                if (cur == null) return false;
                object next;
                if (!TryGetMemberValue(cur, segs[i], out next))
                    return false;
                cur = next;
            }

            value = cur;
            return true;
        }

        private static bool TryGetMemberValue(object obj, string name, out object value)
        {
            value = null;
            if (obj == null || string.IsNullOrEmpty(name)) return false;

            Type t = obj.GetType();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            PropertyInfo pi = t.GetProperty(name, flags);
            if (pi != null && pi.CanRead)
            { try { value = pi.GetValue(obj, null); return true; } catch { } }

            FieldInfo fi = t.GetField(name, flags);
            if (fi != null)
            { try { value = fi.GetValue(obj); return true; } catch { } }

            return false;
        }
        // === 新增：共用比較（Equal / NotEqual） ===
        private static bool EvalEqualOrNotEqual(object oVal, string specValue, bool isNotEqual, out string valueText)
        {
            // 允許 null 視為空字串
            string vs = oVal == null ? "" : Convert.ToString(oVal);
            valueText = vs;

            // 優先嘗試數值比較（兩邊都能轉 double）
            double dv, ds;
            bool vIsNum = DoubleTryParse(vs, out dv);
            bool sIsNum = DoubleTryParse(specValue, out ds);

            bool eq;
            if (vIsNum && sIsNum)
                eq = Math.Abs(dv - ds) < 1e-12;
            else
                eq = string.Equals(vs ?? "", specValue ?? "", StringComparison.OrdinalIgnoreCase);

            return isNotEqual ? !eq : eq;
        }
        private static string VerdictFromResultString(object v)
        {
            if (v == null) return "";
            string s = Convert.ToString(v);
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.Trim();

            if (s.Equals("PASS", StringComparison.OrdinalIgnoreCase)
                || s.Equals("OK", StringComparison.OrdinalIgnoreCase)
                || s.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase)
                || s.IndexOf("PASS", StringComparison.OrdinalIgnoreCase) >= 0)
                return "PASS";

            if (s.Equals("FAIL", StringComparison.OrdinalIgnoreCase)
                || s.Equals("FAILED", StringComparison.OrdinalIgnoreCase)
                || s.Equals("NG", StringComparison.OrdinalIgnoreCase)
                || s.Equals("ERROR", StringComparison.OrdinalIgnoreCase)
                || s.IndexOf("FAIL", StringComparison.OrdinalIgnoreCase) >= 0)
                return "FAIL";

            return "";
        }

        // ===== 通用格式化/工具 =====
        private static string FormatValue(object v)
        {
            if (v == null) return "<span class=\"muted\">(null)</span>";

            if (v is DateTime)
                return Html(((DateTime)v).ToString("yyyy-MM-dd HH:mm:ss"));
            if (v is DateTime?)
            {
                DateTime? dn = (DateTime?)v;
                return dn.HasValue ? Html(dn.Value.ToString("yyyy-MM-dd HH:mm:ss")) : "<span class=\"muted\">(null)</span>";
            }

            if (v is double || v is float || v is decimal)
            {
                try { double d = Convert.ToDouble(v); return Html(d.ToString("0.###")); } catch { return Html(Convert.ToString(v)); }
            }

            if (v is IEnumerable && !(v is string))
            {
                try { return Html(JsonConvert.SerializeObject(v, Formatting.None)); } catch { }
            }

            if (!(v is string) && !(v is ValueType))
            {
                try { return Html(JsonConvert.SerializeObject(v, Formatting.None)); } catch { }
            }

            return Html(Convert.ToString(v));
        }

        private static string ToFlatString(object v)
        {
            if (v == null) return "";
            if (v is DateTime) return ((DateTime)v).ToString("yyyy-MM-dd HH:mm:ss");
            if (v is DateTime?) { DateTime? dn = (DateTime?)v; return dn.HasValue ? dn.Value.ToString("yyyy-MM-dd HH:mm:ss") : ""; }
            if (v is IEnumerable && !(v is string))
            { try { return JsonConvert.SerializeObject(v, Formatting.None); } catch { } }
            return Convert.ToString(v);
        }

        private static bool BoolTryParse(object v, out bool b)
        {
            try
            {
                if (v == null) { b = false; return false; }
                if (v is bool) { b = (bool)v; return true; }
                string s = Convert.ToString(v).Trim().ToLowerInvariant();
                if (s == "true" || s == "t" || s == "1" || s == "yes" || s == "y" || s == "on" || s == "enable" || s == "enabled") { b = true; return true; }
                if (s == "false" || s == "f" || s == "0" || s == "no" || s == "n" || s == "off" || s == "disable" || s == "disabled") { b = false; return true; }
            }
            catch { }
            b = false; return false;
        }

        private static bool IntTryParse(object v, out int x)
        {
            try
            {
                if (v == null) { x = 0; return false; }
                if (v is int) { x = (int)v; return true; }
                if (v is long || v is short || v is byte || v is double || v is float || v is decimal)
                {
                    x = Convert.ToInt32(v);
                    return true;
                }

                string s = Convert.ToString(v);
                if (string.IsNullOrWhiteSpace(s)) { x = 0; return false; }
                s = s.Trim();

                return int.TryParse(s, System.Globalization.NumberStyles.Integer,
                                    System.Globalization.CultureInfo.InvariantCulture, out x)
                    || int.TryParse(s, System.Globalization.NumberStyles.Integer,
                                    System.Globalization.CultureInfo.CurrentCulture, out x);
            }
            catch
            {
                x = 0;
                return false;
            }
        }

        private static bool DoubleTryParse(object v, out double d)
        {
            try
            {
                if (v == null) { d = 0; return false; }
                if (v is double) { d = (double)v; return true; }
                if (v is float) { d = Convert.ToDouble(v); return true; }
                if (v is int || v is long || v is short || v is decimal) { d = Convert.ToDouble(v); return true; }
                string s = Convert.ToString(v);
                if (string.IsNullOrWhiteSpace(s)) { d = 0; return false; }
                s = s.Trim();
                return double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d)
                    || double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out d);
            }
            catch { d = 0; return false; }
        }

        private static string NullSafe(string s) { return string.IsNullOrEmpty(s) ? "" : s; }

        private static string FormatNum(double d)
        {
            return double.IsNaN(d) || double.IsInfinity(d) ? "" : d.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string Coalesce(params string[] vals)
        {
            if (vals == null) return "";
            for (int i = 0; i < vals.Length; i++) if (!string.IsNullOrEmpty(vals[i])) return vals[i];
            return "";
        }

        private static int IndexOfKey(List<KeyValuePair<string, object>> list, string key)
        {
            for (int i = 0; i < list.Count; i++)
                if (string.Equals(list[i].Key, key, StringComparison.OrdinalIgnoreCase)) return i;
            return -1;
        }

        private static bool StringEqualsAny(string a, string b1, string b2, string b3)
        {
            return string.Equals(a, b1, StringComparison.OrdinalIgnoreCase)
                || string.Equals(a, b2, StringComparison.OrdinalIgnoreCase)
                || string.Equals(a, b3, StringComparison.OrdinalIgnoreCase);
        }

        private static string LastSegment(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            int idx = path.LastIndexOf('.');
            return (idx >= 0) ? path.Substring(idx + 1) : path;
        }

        private static string Html(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                    .Replace("\"", "&quot;").Replace("'", "&#39;");
        }
    }



}
