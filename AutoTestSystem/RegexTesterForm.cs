using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System.Text;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Windows.Interop;

namespace AutoTestSystem
{
    public partial class RegexTesterForm : Form
    {
        private bool isMaximized = false;  // 用來記錄是否處於最大化狀態
        public string JsonData { get; private set; } = null;

        int mode = 0;
        public RichTextBox InputRichTextBox
        {
            get { return testStringInput; }
            set { testStringInput = value; }
        }
        public RegexTesterForm(string Data, string input,int m = 0)
        {
            InitializeComponent();
            InitializePlaceholders();

            mode = m;

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem addToNewGridMenuItem = new ToolStripMenuItem("添加到變數表");

            // 「清空文本框」選項
            ToolStripMenuItem clearRichTextBoxMenuItem = new ToolStripMenuItem("清空文本框");
            contextMenu.Items.Add(clearRichTextBoxMenuItem);
            contextMenu.Items.Add(addToNewGridMenuItem);

            resultGrid.ContextMenuStrip = contextMenu;

            // 點擊「清空文本框」的事件處理
            clearRichTextBoxMenuItem.Click += (s, e) =>
            {

                resultGrid.Columns.Clear();
                resultGrid.Rows.Clear();

            };
            addToNewGridMenuItem.Click += (s, e) =>
            {
                if (resultGrid.SelectedCells.Count > 0) // 如果有選中的單元格
                {
                    string regexValue = regexInput.Text;

                    foreach (DataGridViewCell selectedCell in resultGrid.SelectedCells)
                    {
                        int rowIndex = selectedCell.RowIndex;
                        int columnIndex = selectedCell.ColumnIndex;
                        string cellValue = selectedCell.Value?.ToString() ?? "";

                        // 檢查是否已經存在相同的 regexValue, rowIndex 和 columnIndex
                        bool exists = false;
                        foreach (DataGridViewRow existingRow in newResultGrid.Rows)
                        {
                            if (existingRow.Cells[0].Value?.ToString() == regexValue &&
                                (int)existingRow.Cells[2].Value == rowIndex &&
                                (int)existingRow.Cells[3].Value == columnIndex)
                            {
                                exists = true;
                                break;
                            }
                        }

                        if (exists)
                        {
                            continue; // 跳過已存在的項目
                        }

                        // 如果不存在，添加新行
                        var row = new DataGridViewRow();
                        row.CreateCells(newResultGrid);

                        row.Cells[0].Value = regexValue;
                        row.Cells[1].Value = cellValue;
                        row.Cells[2].Value = rowIndex;
                        row.Cells[3].Value = columnIndex;

                        newResultGrid.Rows.Add(row);
                    }

                    //MessageBox.Show("選中的數據已添加到新表！");
                }
                else
                {
                    MessageBox.Show("請選擇要添加的單元格！");
                }
            };

            // 為 testStringInput 建立右鍵選單
            ContextMenuStrip testStringContextMenu = new ContextMenuStrip();
            ContextMenuStrip regexStringContextMenu = new ContextMenuStrip();
            ToolStripMenuItem GenerateRegexMenuItem = new ToolStripMenuItem("Try Flexible Match");
            ToolStripMenuItem GenGroupRegexMenuItem = new ToolStripMenuItem("Try Flexible Group");
            ToolStripMenuItem GenerateExactRegexMenuItem = new ToolStripMenuItem("Try Exact Match");
            ToolStripMenuItem GenerateExactGroupRegexMenuItem = new ToolStripMenuItem("Try Exact Group");
            // 「清空文本框」選項
            ToolStripMenuItem clearInputRichTextBoxMenuItem = new ToolStripMenuItem("Clear");
            ToolStripMenuItem clearRegexInputRichTextBoxMenuItem = new ToolStripMenuItem("Clear");

            testStringContextMenu.Items.Add(GenerateRegexMenuItem);
            testStringContextMenu.Items.Add(GenGroupRegexMenuItem);
            testStringContextMenu.Items.Add(GenerateExactRegexMenuItem);
            testStringContextMenu.Items.Add(GenerateExactGroupRegexMenuItem);         
            testStringContextMenu.Items.Add(clearInputRichTextBoxMenuItem);
            testStringInput.ContextMenuStrip = testStringContextMenu;

            regexStringContextMenu.Items.Add(clearRegexInputRichTextBoxMenuItem);
            regexInput.ContextMenuStrip = regexStringContextMenu;
            // 點擊「清空文本框」的事件處理
            clearInputRichTextBoxMenuItem.Click += (s, e) =>
            {
                testStringInput.Clear();
            };
            clearRegexInputRichTextBoxMenuItem.Click += (s, e) =>
            {
                regexInput.Clear();
            };
            GenerateRegexMenuItem.Click += (s, e) =>
            {
                GenerateRegexFromSelection(false,0);
                regexInput.ForeColor = System.Drawing.Color.Black;
                // 手動觸發 TextChanged 事件
                Input_TextChanged(regexInput, EventArgs.Empty);

            };
            GenGroupRegexMenuItem.Click += (s, e) =>
            {
                //GenGroupRegexFromSelection();
                GenerateRegexFromSelection(true,0);
                regexInput.ForeColor = System.Drawing.Color.Black;
                // 手動觸發 TextChanged 事件
                Input_TextChanged(regexInput, EventArgs.Empty);
            };
            GenerateExactRegexMenuItem.Click += (s, e) =>
            {
                GenerateRegexFromSelection(false, 1);
                regexInput.ForeColor = System.Drawing.Color.Black;
                // 手動觸發 TextChanged 事件
                Input_TextChanged(regexInput, EventArgs.Empty);
            };
            GenerateExactGroupRegexMenuItem.Click += (s, e) =>
            {
                GenerateRegexFromSelection(true, 1);
                regexInput.ForeColor = System.Drawing.Color.Black;
                // 手動觸發 TextChanged 事件
                Input_TextChanged(regexInput, EventArgs.Empty);
            };
            // 綁定事件到選單項目
            // generateRegexMenuItem.Click += (s, e) => GenerateRegexFromSelection();

            newResultGrid.Columns.Add($"Regex", $"Regex");
            newResultGrid.Columns.Add($"Name", $"Name");
            newResultGrid.Columns.Add($"MatchIndex", $"MatchIndex");
            newResultGrid.Columns.Add($"GroupIndex", $"GroupIndex");
            newResultGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            newResultGrid.Columns[0].FillWeight = 2;
            newResultGrid.Columns[1].FillWeight = 1;
            newResultGrid.Columns[2].FillWeight = 1;
            newResultGrid.Columns[3].FillWeight = 1;
            if (!string.IsNullOrEmpty(input))
            {
                testStringInput.ForeColor = System.Drawing.Color.Black;
                testStringInput.Text = CleanMessage(input.Replace("\r\n", "\n").Replace("\r", "\n"));
            }
            if (m == 0)
                LoadJsonToDataGridView(Data, newResultGrid);
            else
            {
                regexInput.ForeColor = System.Drawing.Color.Black;
                regexInput.Text = Data;
            }
            // 手動觸發 TextChanged 事件
            Input_TextChanged(regexInput, EventArgs.Empty);
            if (m == 0)
            {
                tableLayoutPanel1.RowStyles[0].Height = 15;
                tableLayoutPanel1.RowStyles[1].Height = 35;
                tableLayoutPanel1.RowStyles[2].Height = 30;
                tableLayoutPanel1.RowStyles[3].Height = 30;
            }
            else
            {
                tableLayoutPanel1.RowStyles[0].Height = 30;
                tableLayoutPanel1.RowStyles[1].Height = 40;
                tableLayoutPanel1.RowStyles[2].Height = 30;
                tableLayoutPanel1.RowStyles[3].Height = 0;
            }

        }

        string CleanMessage(string msg)
        {
            return new string(msg.Where(c => c >= 0x20 || c == '\n' || c == '\r' || c == '\t').ToArray());
        }

        private string GeneratePatternWithVersionAndKeyDetection(string selectedText)
        {
            // 將每一行分割為陣列進行逐行處理
            string[] lines = selectedText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder patternBuilder = new StringBuilder();

            foreach (string line in lines)
            {
                string processedLine = line.Trim();

                if (processedLine.Contains(":") || processedLine.Contains("="))
                {
                    // 根據是否包含 ":" 或 "=" 進行分割，優先處理 ":"
                    string[] parts = processedLine.Contains(":")
                        ? processedLine.Split(new[] { ':' }, 2)
                        : processedLine.Split(new[] { '=' }, 2);

                    string leftPart = Regex.Escape(parts[0].Trim()); // 左側保留並進行轉義
                    string rightPart = parts[1].Trim().TrimEnd(',').TrimEnd(']').TrimEnd('}'); // 去除右側結尾的逗號

                    bool hasRightBrace = processedLine.EndsWith("}");
                    bool hasRightBracket = processedLine.EndsWith("]");
                    // 判斷右側的格式，靈活處理
                    if (Regex.IsMatch(rightPart, "^\"[^\"]*\"$")) // 如果右邊是用雙引號包裹的字串
                    {
                        rightPart = @"\""([^\""]*)\"""; // 捕獲引號內的內容，不包括引號
                    }
                    else if (Regex.IsMatch(rightPart, @"^-?\d+(\.\d+)?$")) // 如果右邊是浮點數或整數
                    {
                        rightPart = @"(-?\d+(?:\.\d+)?)";
                        //rightPart = @"(.+)";
                    }
                    else if (Regex.IsMatch(rightPart, @" ^ [a-zA-Z0-9_]+$")) // 如果右邊是純文本（例如版本號、命令）
                    {
                        rightPart = @"([a-zA-Z0-9_]+)"; // 只匹配字母、數字、下劃線的純文本
                        //rightPart = @"(.+)";
                    }
                    //else if (Regex.IsMatch(rightPart, @"^v\d+\.\d+\.\d+\.\d+(-\d+)?-g[a-f0-9]+$")) // 版本號匹配
                    //{
                    //    rightPart = @"v\d+\.\d+\.\d+\.\d+(-\d+)?-g[a-f0-9]+(?=\])"; // 確保版本號後不匹配 `]`
                    //}
                    else
                    {
                        if(hasRightBracket || hasRightBrace)
                        {
                            rightPart = @"(.*?)";
                        }
                        else
                            rightPart = @"(.*)"; // 預設捕獲任何內容
                    }

                    // 判斷是否有去掉的逗號
                    bool hasComma = processedLine.EndsWith(",");

                    // 如果右側原本有逗號，則將逗號也添加到正則中
                    if (hasComma)
                    {
                        rightPart += @",?"; // 允許結尾逗號，逗號是可選的
                    }

                    if (hasRightBrace || hasRightBracket)
                    {
                        if (hasRightBrace)
                        {
                            rightPart += @"\}";
                        }

                        if (hasRightBracket)
                        {
                            rightPart += @"\]";
                        }
                    }

                    // 組合成正則模式，保留冒號或等號
                    char delimiter = processedLine.Contains(":") ? ':' : '=';
                    patternBuilder.Append($"{leftPart}\\s*{delimiter}\\s*{rightPart}");
                }
                else
                {
                    if(lines.Length > 1)
                    // 如果行中既沒有冒號也沒有等號，直接轉義保留
                    patternBuilder.Append(Regex.Escape(processedLine) + @"(?=\r|\n|$)");
                    else if (lines.Length == 1)
                        patternBuilder.Append(processedLine);
                }

                // 為每行結尾添加空白匹配，保持靈活性
                patternBuilder.Append(@"\s*");
            }

            return patternBuilder.ToString();
        }

        private bool isSelecting = false;
        private int startPosition = 0;

        private void richTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            RichTextBox richTextBox = sender as RichTextBox;
            startPosition = richTextBox.GetCharIndexFromPosition(e.Location);
            isSelecting = true;
        }

        private void richTextBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                try
                {
                    RichTextBox richTextBox = sender as RichTextBox;
                    int currentPosition = richTextBox.GetCharIndexFromPosition(e.Location);

                    // 確保選取範圍包括最後一個字元
                    if (currentPosition >= richTextBox.Text.Length)
                    {
                        currentPosition = richTextBox.Text.Length - 1;
                    }

                    int selectionLength = Math.Abs(currentPosition - startPosition) + 1;

                    richTextBox.SelectionStart = Math.Min(startPosition, currentPosition);
                    richTextBox.SelectionLength = selectionLength;
                }
                catch(Exception)
                {

                }
            }
        }

        private void richTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            isSelecting = false;
        }

        private void GenerateRegexFromSelection(bool autoGroup,int mode)
        {
            // 將每一行分割為陣列進行逐行處理
            string[] lines = testStringInput.SelectedText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder patternBuilder = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                string processedLine = lines[i];
                string replaceline = string.Empty;

                // 處理大括號格式 {key:value} 或 {key}
                var braceMatches = Regex.Matches(processedLine, @"\{([^{}]+)\}");
                if (braceMatches.Count > 0)
                {
                    replaceline = ProcessBraceFormat(braceMatches, processedLine, autoGroup, mode);
                }
                else
                {
                    // 處理冒號或等號格式，如 key: value 或 "key": "value"
                    if (processedLine.Contains(":") || processedLine.Contains("="))
                    {
                        replaceline = ProcessColonOrEqualFormat(processedLine, autoGroup, mode);
                    }
                    else
                    {
                        // 如果沒有匹配到特定格式，直接轉義特殊符號
                        replaceline = EscapeSpecialCharacters(processedLine, autoGroup,mode);
                    }
                }

                // 處理多行的情況，保證換行匹配
                if (lines.Length > 1 && i != lines.Length - 1)
                {
                    replaceline += @"(?=\r|\n|$)\s*";
                }

                patternBuilder.Append(replaceline);
            }

            if (regexInput.Text.Contains("輸入正則表達式"))
                regexInput.Text = patternBuilder.ToString();
            else
                // 將產生的正則表達式顯示在輸入框中
                regexInput.Text += patternBuilder.ToString();
        }

        // 處理大括號格式 {key:value} 或 {key}
        private string ProcessBraceFormat(MatchCollection matches, string originalLine, bool autoGroup,int mode)
        {
            StringBuilder bracePattern = new StringBuilder();

            foreach (Match match in matches)
            {
                string content = match.Groups[1].Value;

                if (content.Contains(":"))
                {
                    string[] parts = content.Split(new[] { ':' }, 2);
                    string leftPart = EscapeSpecialCharacters(parts[0], autoGroup, mode);
                    string rightPart = GenerateValueRegex(parts[1].Trim(), autoGroup, mode);

                    bracePattern.Append($@"\{{{leftPart}\s*:\s*{rightPart}\}}");
                }
                else
                {
                    string escapedContent = EscapeSpecialCharacters(content, autoGroup, mode);
                    bracePattern.Append($@"\{{{escapedContent}\}}");
                }
            }

            // 補上前後大括號，如果原始行有的話
            if (originalLine.StartsWith("{{")) bracePattern.Insert(0, @"\{");
            if (originalLine.EndsWith("}}")) bracePattern.Append(@"\}");

            return bracePattern.ToString();
        }

        // 處理冒號或等號格式，如 key: value 或 "key": "value"
        private string ProcessColonOrEqualFormat(string line, bool autoGroup,int mode)
        {
            var matches = Regex.Matches(line, @"([\w\s\""]+)(:|=)\s*([^\n,\]]+)");
            StringBuilder colonPattern = new StringBuilder();
            int lastIndex = 0;

            foreach (Match match in matches)
            {
                // 將前面未被匹配的部分加入正則表達式
                if (match.Index > lastIndex)
                {
                    string prefix = line.Substring(lastIndex, match.Index - lastIndex);
                    colonPattern.Append(EscapeSpecialCharacters(prefix, autoGroup, mode));
                }
                string result = Regex.Replace(match.Groups[1].Value, @"([\(\)\+\*\?\|\^\$\{\}\[\]])", @"\$1");
                result = Regex.Replace(result, @"\s+", @"\s+");
                string key = result;
                string delimiter = match.Groups[2].Value;
                string value = match.Groups[3].Value;

                // 生成值的正則
                string valuePattern = GenerateValueRegex(value, autoGroup, mode);

                // 組合 Key-Value 的正則表達式
                colonPattern.Append($@"{key}\s*{Regex.Escape(delimiter)}\s*{valuePattern}");

                lastIndex = match.Index + match.Length;
            }

            // 處理最後未被匹配的部分
            if (lastIndex < line.Length)
            {
                string suffix = line.Substring(lastIndex);
                colonPattern.Append(EscapeSpecialCharacters(suffix, autoGroup, mode));
            }

            return colonPattern.ToString();
        }

        // 根據值的類型生成對應的正則表達式
        private string GenerateValueRegex(string value, bool autoGroup,int mode)
        {
            string groupWrapper(string pattern) => autoGroup ? $"({pattern})" : pattern;

            if (Regex.IsMatch(value, "^\"[^\"]*\"$")) // 雙引號字串
            {
                if(mode == 1)
                {
                    if (autoGroup)
                    {
                        // 提取雙引號內的值
                        var match = Regex.Match(value, "^\"([^\"]*)\"$");
                        if (match.Success)
                        {
                            return $"\"({match.Groups[1].Value})\"";
                        }
                        else
                        {
                            return $"({value})"; // 如果沒有匹配到，返回原始值
                        }
                    }
                    else
                        return value;
                }
                else
                {
                    if (autoGroup)
                        return @"""([^""]*)""";
                    else
                        return @"""[^""]*""";
                }

            }
            else if (Regex.IsMatch(value, @"^-?\d+(\.\d+)?$")) // 數字（整數或浮點數）
            {
                if (mode == 1)
                {
                    return groupWrapper(value);
                }
                else
                    return groupWrapper(@"-?\d+(?:\.\d+)?");
            }
            else if (Regex.IsMatch(value, @"^0x[0-9a-fA-F]+$")) // 十六進制數字
            {
                if (mode == 1)
                {
                    return groupWrapper(value);
                }
                else
                    return groupWrapper(@"0x[0-9a-fA-F]+");
            }
            else if (Regex.IsMatch(value, @"^\d{1,2}\s[a-zA-Z]{3}\s\d{4}\s\d{2}:\d{2}:\d{2}$")) // 日期時間格式
            {
                if (mode == 1)
                {
                    return groupWrapper(value);
                }
                else
                    return groupWrapper(@"\d{1,2}\s[a-zA-Z]{3}\s\d{4}\s\d{2}:\d{2}:\d{2}");
            }
            else if (Regex.IsMatch(value, @"^v\d+\.\d+\.\d+\.\d+(-\d+)?-g[a-f0-9]+$")) // 版本號格式
            {
                if (mode == 1)
                {
                    return groupWrapper(EscapeSpecialCharacters(value, autoGroup, mode));
                }
                else
                    return groupWrapper(@"v\d+\.\d+\.\d+\.\d+(?:-\d+)?-g[a-f0-9]+");
            }
            else if (Regex.IsMatch(value, @"^[a-zA-Z0-9_]+$")) // 純文字
            {
                if (mode == 1)
                {
                    return groupWrapper(value);
                }
                else
                    return groupWrapper(@"[a-zA-Z0-9_]+");
            }
            else if (value == "?")
            {
                return @"\?";
            }
            else
            {
                if (mode == 1)
                {
                    return groupWrapper(EscapeSpecialCharacters(value, autoGroup,mode));
                }
                else
                    return groupWrapper(@".*"); // 預設匹配任意字串
            }
        }



        // 轉義正則表達式中的特殊符號
        private string EscapeSpecialCharacters(string input, bool autoGroupNumbers,int mode)
        {
            string result = Regex.Replace(input, @"([\(\)\+\*\?\|\^\$\{\}\[\]])", @"\$1");

            if (mode == 0)
            {
                // 替換掉任何大小寫字元，不含空格或特殊符號
                result = Regex.Replace(result, @"[a-zA-Z]+", @"[a-zA-Z]+");

                if (autoGroupNumbers)
                    result = Regex.Replace(result, @"-?\d+(\.\d+)?", @"(-?\d+(?:\.\d+)?)");
                else
                    result = Regex.Replace(result, @"-?\d+(\.\d+)?", @"-?\d+(?:\.\d+)?");
            }
            else if (mode == 1)
            {
                // 使用正則表達式匹配數字並回填原始格式
                result = Regex.Replace(result, @"-?\d+(\.\d+)?", match =>
                {
                    string value = match.Value;
                    return autoGroupNumbers ? $"({value})" : value;
                });
            }

            result = Regex.Replace(result, @"\s+", @"\s+");

            return result;
        }
        private void GenGroupRegexFromSelection()
        {
            string selectedText = testStringInput.SelectedText;

            if (string.IsNullOrWhiteSpace(selectedText))
            {
                MessageBox.Show("請先框選文字！");
                return;
            }

            // 不明確模式（更寬鬆的匹配）
            // 基於選取的文字進行初步處理
            string ambiguousPattern = GeneratePatternWithVersionAndKeyDetection(selectedText);
            regexInput.Text = ambiguousPattern;

        }
        // 這個函數將判斷需要補獲的值並修改為捕獲型正則
        public static string ConvertToCaptureGroupRegex(string inputRegex)
        {
            // 替換 ':' 右側的內容為捕獲組
            inputRegex = Regex.Replace(inputRegex, @"(?<=:)(\S+)", m => $"({m.Value})");

            // 替換 '()' 內部的內容為捕獲組，若已經是捕獲組則跳過
            inputRegex = Regex.Replace(inputRegex, @"\(([^)]+)\)", m => $"({m.Groups[1].Value})");

            // 替換 "" 內部的內容為捕獲組
            inputRegex = Regex.Replace(inputRegex, @"(?<=\“)([^”]+)(?=\”)", m => $"({m.Value})");

            // 替換 '=' 右側的內容為捕獲組
            inputRegex = Regex.Replace(inputRegex, @"(?<=\=)(\S+)", m => $"({m.Value})");

            return inputRegex;
        }
        private const string RegexPlaceholder = "輸入正則表達式";
        private const string TestStringPlaceholder = "輸入測試字符串";

        private void InitializePlaceholders()
        {
            AddPlaceholder(regexInput, RegexPlaceholder);
            AddPlaceholder(testStringInput, TestStringPlaceholder);
        }

        private void AddPlaceholder(RichTextBox richTextBox, string placeholderText)
        {
            if (string.IsNullOrEmpty(richTextBox.Text) || richTextBox.Text == placeholderText)
            {
                richTextBox.Text = placeholderText;
                richTextBox.ForeColor = System.Drawing.Color.Gray;
            }
        }

        private void RemovePlaceholder(RichTextBox richTextBox, string placeholderText)
        {
            if (richTextBox.ForeColor == System.Drawing.Color.Gray && richTextBox.Text == placeholderText)
            {
                richTextBox.Text = string.Empty;
                richTextBox.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void regexInput_GotFocus(object sender, EventArgs e)
        {
            RemovePlaceholder(regexInput, RegexPlaceholder);
        }

        private void regexInput_LostFocus(object sender, EventArgs e)
        {
            AddPlaceholder(regexInput, RegexPlaceholder);
        }

        private void testStringInput_GotFocus(object sender, EventArgs e)
        {
            RemovePlaceholder(testStringInput, TestStringPlaceholder);
        }

        private void testStringInput_LostFocus(object sender, EventArgs e)
        {
            AddPlaceholder(testStringInput, TestStringPlaceholder);
        }

        private bool suppressTextChanged = false;
        private MatchCollection matches;
        private void Input_TextChanged(object sender, EventArgs e)
        {
            if (suppressTextChanged) return;
            statusLabel.Text = string.Empty;
            resultGrid.Columns.Clear();
            resultGrid.Rows.Clear();

            if (regexInput.ForeColor == System.Drawing.Color.Gray || testStringInput.ForeColor == System.Drawing.Color.Gray)
                return;

            string regexPattern = regexInput.Text;
            string testString = testStringInput.Text;

            try
            {
                if (string.IsNullOrEmpty(regexPattern))
                {

                    return;
                }

                int selectionStart = testStringInput.SelectionStart;
                int selectionLength = testStringInput.SelectionLength;

                var regex = new Regex(regexPattern, RegexOptions.Multiline);  // 使用 Multiline 选项
                var matches = regex.Matches(testString);
                this.matches = matches;
                // 清除以前的高亮顏色
                //testStringInput.SelectAll();

                //if (string.IsNullOrEmpty(testStringInput.Text) || testStringInput.Text == "輸入測試字符串")
                //{
                //    testStringInput.SelectionBackColor = System.Drawing.Color.White;  // 恢復到默認顏色
                //    testStringInput.SelectionColor = System.Drawing.Color.Gray;  // 恢復到默認顏色
                //}
                //else
                //{
                //    testStringInput.SelectionBackColor = System.Drawing.Color.White;  // 恢復到默認顏色
                //    testStringInput.SelectionColor = System.Drawing.Color.Black;  // 恢復到默認顏色
                //}


                resultGrid.Columns.Clear();
                resultGrid.Rows.Clear();



                if (matches.Count > 0)
                {
                    for (int i = 0; i < matches[0].Groups.Count; i++)
                    {
                        resultGrid.Columns.Add($"Group{i}", $"Group[{i}]");
                    }
                    foreach (DataGridViewColumn column in resultGrid.Columns)
                    {
                        column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }

                }

                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        var row = new DataGridViewRow();
                        row.CreateCells(resultGrid);

                        for (int i = 0; i < match.Groups.Count; i++)
                        {
                            row.Cells[i].Value = match.Groups[i].Value;
                        }

                        resultGrid.Rows.Add(row);
                    }
                    //// 取得整個匹配 (group[0]) 的起始位置和長度
                    //int matchStartIndex = match.Index;
                    //int matchLength = match.Length;
                    //// 標註整個匹配的範圍為藍色背景
                    //testStringInput.Select(matchStartIndex, matchLength);
                    //testStringInput.SelectionBackColor = System.Drawing.Color.Gold;

                    //// 遍歷每個子群組 (group[1] 及後續群組)
                    //for (int groupIndex = 1; groupIndex < match.Groups.Count; groupIndex++)
                    //{
                    //    Group group = match.Groups[groupIndex];

                    //    if (group.Success)
                    //    {
                    //        // 獲取子群組的起始位置和長度 (已相對於整個字串)
                    //        int groupStartIndex = group.Index;
                    //        int groupLength = group.Length;

                    //        // 標註子群組的範圍為綠黃色背景
                    //        testStringInput.Select(groupStartIndex, groupLength);
                    //        testStringInput.SelectionBackColor = System.Drawing.Color.Yellow;
                    //    }
                    //}
                    //// 恢復用戶的光標位置和選取範圍
                    //testStringInput.SelectionStart = selectionStart;
                    //testStringInput.SelectionLength = selectionLength;
                    //testStringInput.ScrollToCaret();
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"正則表達式錯誤: {ex.Message}";
            }
        }
        public string SaveDataGridViewToJson(DataGridView dataGridView)
        {
            // 建立分組的列表
            List<RegexGroup> regexGroups = new List<RegexGroup>();

            // 遍歷 DataGridView 的行
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.IsNewRow) continue; // 跳過新行

                // 獲取欄位數據
                string regex = row.Cells["Regex"].Value?.ToString();
                string name = row.Cells["Name"].Value?.ToString();

                if (string.IsNullOrEmpty(regex) || string.IsNullOrEmpty(name))
                    continue; // 如果某些值為空則跳過

                if (!int.TryParse(row.Cells["MatchIndex"].Value?.ToString(), out int matchIndex))
                    continue; // 跳過格式錯誤的行

                if (!int.TryParse(row.Cells["GroupIndex"].Value?.ToString(), out int groupIndex))
                    continue; // 跳過格式錯誤的行

                // 尋找是否已存在該 Regex 分組
                var regexGroup = regexGroups.FirstOrDefault(g => g.Regex == regex);

                if (regexGroup == null)
                {
                    // 新建分組
                    regexGroup = new RegexGroup { Regex = regex };
                    regexGroups.Add(regexGroup);
                }

                // 添加數據到分組中
                regexGroup.Items.Add(new MyClass
                {
                    Name = name,
                    MatchIndex = matchIndex,
                    GroupIndex = groupIndex
                });
            }
            if (regexGroups.Count > 0)
                // 將數據序列化為 JSON
                return JsonConvert.SerializeObject(regexGroups);
            else
                return string.Empty;
        }
        public void LoadJsonToDataGridView(string json, DataGridView dataGridView)
        {
            try
            {
                // 檢查 JSON 是否為空或無效
                if (string.IsNullOrWhiteSpace(json))
                {
                    //MessageBox.Show("提供的 JSON 是空的或無效的！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 檢查 DataGridView 是否為空
                if (dataGridView == null)
                {
                    MessageBox.Show("DataGridView 控件未正確初始化！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 清空 DataGridView
                dataGridView.Rows.Clear();

                // 嘗試反序列化 JSON
                List<RegexGroup> regexGroups;
                try
                {
                    regexGroups = JsonConvert.DeserializeObject<List<RegexGroup>>(json);
                }
                catch (JsonException ex)
                {
                    regexInput.Text = json;
                    regexInput.ForeColor = System.Drawing.Color.Black;
                    return;
                }

                // 如果反序列化後為空，提示用戶
                if (regexGroups == null || regexGroups.Count == 0)
                {

                    MessageBox.Show("JSON 數據為空或格式不正確，無法載入到 DataGridView！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 遍歷分組並添加到 DataGridView
                foreach (var group in regexGroups)
                {
                    // 檢查每個 RegexGroup 是否有有效的 Regex 和 Items
                    if (string.IsNullOrWhiteSpace(group.Regex))
                    {
                        MessageBox.Show("某些 Regex 值為空，已跳過！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    if (group.Items == null || group.Items.Count == 0)
                    {
                        MessageBox.Show($"Regex '{group.Regex}' 下無有效的項目，已跳過！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    foreach (var item in group.Items)
                    {
                        // 檢查 Item 屬性是否為空
                        if (string.IsNullOrWhiteSpace(item.Name))
                        {
                            MessageBox.Show($"Regex '{group.Regex}' 的某些 Name 值為空，已跳過！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }

                        // 確保 MatchIndex 和 GroupIndex 有有效的數值
                        if (item.MatchIndex < 0 || item.GroupIndex < 0)
                        {
                            MessageBox.Show($"Regex '{group.Regex}' 的項目 Index 值無效，已跳過！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }

                        // 添加到 DataGridView
                        dataGridView.Rows.Add(group.Regex, item.Name, item.MatchIndex, item.GroupIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                // 捕獲其他未處理的錯誤
                MessageBox.Show($"載入數據時發生錯誤！\n錯誤信息：{ex.Message}", "系統錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            regexInput.Text = string.Empty;
            testStringInput.Text = string.Empty;
            AddPlaceholder(regexInput, RegexPlaceholder);
            AddPlaceholder(testStringInput, TestStringPlaceholder);

            resultGrid.Rows.Clear();
            newResultGrid.Rows.Clear();
            statusLabel.Text = string.Empty;
        }

        private void resultGrid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if(e.ColumnIndex>= 0 && e.RowIndex >= 0)
                    resultGrid.CurrentCell = resultGrid[e.ColumnIndex, e.RowIndex];
            }
            else if (e.Button == MouseButtons.Left)
            {
                // 確保點擊的不是標題行或無效區域
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    suppressTextChanged = true; 
                    try
                    {
                        // 取得點擊的列索引 (ColumnIndex) 與行索引 (RowIndex)
                        int rowIndex = e.RowIndex;
                        int columnIndex = e.ColumnIndex;

                        // 清除先前的標註
                        testStringInput.SelectAll();
                        testStringInput.SelectionBackColor = System.Drawing.Color.White;
                        regexInput.SelectAll();
                        regexInput.SelectionBackColor = System.Drawing.Color.White;
                        // 查找對應的 Match 和 Group
                        if (rowIndex < matches.Count)
                        {
                            Match match = matches[rowIndex];

                            if (match.Success && columnIndex < match.Groups.Count)
                            {
                                Group group = match.Groups[columnIndex];

                                if (group.Success)
                                {
                                    // 標註群組範圍為紅色背景
                                    testStringInput.Select(group.Index, group.Length);
                                    testStringInput.SelectionBackColor = System.Drawing.Color.GreenYellow;
                                    testStringInput.ScrollToCaret();

                                    // 高亮正則表達式中的捕獲群組
                                    HighlightRegexGroup(regexInput.Text, columnIndex);
                                }
                            }
                        }
                        // 滾動到選中範圍

                        // 恢復光標位置
                        //testStringInput.SelectionStart = testStringInput.Text.Length;
                        //testStringInput.SelectionLength = 0;
                        //testStringInput.SelectionBackColor = System.Drawing.Color.White;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"標註過程中出現錯誤: {ex.Message}");
                    }
                    finally
                    {
                        suppressTextChanged = false; // 恢复 Input_TextChanged
                    }
                }
            }
        }
        private void HighlightRegexGroup(string pattern, int groupIndex)
        {
            try
            {
                // 找出所有捕獲群組，簡單處理非命名群組
                //var groupMatches = Regex.Matches(pattern, @"\(([^?].*?)\)");
                var groupMatches = Regex.Matches(pattern, @"(?<!\\)\((?!\?)(?>[^()\\]+|\\.|(?<open>\()|(?<-open>\)))+(?(open)(?!))\)");

                if (groupIndex > 0 && groupIndex <= groupMatches.Count)
                {
                    Match groupMatch = groupMatches[groupIndex - 1];  // Group[0] 是整體匹配，群組從 1 開始
                    regexInput.Select(groupMatch.Index, groupMatch.Length);
                    regexInput.SelectionBackColor = System.Drawing.Color.LightGreen;
                    regexInput.ScrollToCaret();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"高亮正則群組過程中出現錯誤: {ex.Message}");
            }
        }
        // 當 testStringInput 雙擊時，切換 SplitContainer 佈局
        private void testStringInput_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (isMaximized)
            {
                // 如果已經最大化，恢復原來的布局

                if (mode == 0)
                {
                    tableLayoutPanel1.RowStyles[0].Height = 15;
                    tableLayoutPanel1.RowStyles[1].Height = 35;
                    tableLayoutPanel1.RowStyles[2].Height = 30;
                    tableLayoutPanel1.RowStyles[3].Height = 30;
                }
                else
                {
                    tableLayoutPanel1.RowStyles[0].Height = 30;
                    tableLayoutPanel1.RowStyles[1].Height = 40;
                    tableLayoutPanel1.RowStyles[2].Height = 30;
                    tableLayoutPanel1.RowStyles[3].Height = 0;
                }
                isMaximized = false;
            }
            else
            {
                // 最大化，將第二行占滿
                tableLayoutPanel1.RowStyles[0].Height = 0;
                tableLayoutPanel1.RowStyles[1].Height = 100;
                tableLayoutPanel1.RowStyles[2].Height = 0;
                tableLayoutPanel1.RowStyles[3].Height = 0;
                
                isMaximized = true;
            }
        }

        private void RegexTesterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 顯示確認對話框
            var dialogResult = MessageBox.Show(
                "要保存當前的數據嗎？",
                "保存確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            // 根據用戶選擇執行操作
            if (dialogResult == DialogResult.Yes)
            {
                if (mode == 0)
                    // 調用保存方法
                    JsonData = SaveDataGridViewToJson(newResultGrid);
                else
                    JsonData = regexInput.Text;

                this.DialogResult = DialogResult.OK;
            }
            else
            {
                // 用戶選擇不保存
                this.DialogResult = DialogResult.Cancel;

            }

        }

        private void testStringInput_MouseUp(object sender, MouseEventArgs e)
        {
            // 獲取選中的文字
            string selectedText = InputRichTextBox.SelectedText;

            // 如果選中的文字不為空，則根據選擇的文字生成正則表達式
            if (!string.IsNullOrEmpty(selectedText))
            {
                // 先記下目前選取的文字範圍位置
                int selectionStart = InputRichTextBox.SelectionStart;
                int selectionLength = InputRichTextBox.SelectionLength;

                // 1. 點擊時選取所有文字
                InputRichTextBox.SelectAll();

                // 2. 清除所有選取區域的顏色
                InputRichTextBox.SelectionBackColor = System.Drawing.Color.White;  // 恢復到默認顏色
                InputRichTextBox.SelectionColor = System.Drawing.Color.Black;       // 設置文字顏色為黑色

                // 3. 取消選取範圍，將光標返回到原本位置
                InputRichTextBox.SelectionLength = 0;  // 取消選取區域

                // 4. 將光標恢復到之前的選擇位置
                InputRichTextBox.Select(selectionStart, selectionLength);

                // 處理選擇文本中的特殊字符，將其轉換為適用於正則表達式的格式
                //string flexiblePattern = Regex.Replace(selectedText, @"[\(\)\.\+\*\?\|\^\$\{\}\[\]]", @"\$0");

                //// 處理常見的模式
                //flexiblePattern = Regex.Replace(flexiblePattern, @"\d+", @"\d+");  // 數字
                //flexiblePattern = Regex.Replace(flexiblePattern, @"\s+", @"\s+");  // 空白字符

                //string ambiguousPattern = GeneratePatternWithVersionAndKeyDetection(selectedText);
                //// 顯示生成的正則表達式
                //regexInput.Text = ambiguousPattern;
                //regexInput.ForeColor = System.Drawing.Color.Black;

                //Input_TextChanged(regexInput, EventArgs.Empty);
            }
        }       
    }
    public class RegexGroup
    {
        public string Regex { get; set; }
        public List<MyClass> Items { get; set; } = new List<MyClass>();
    }

    public class MyClass
    {
        public string Name { get; set; }
        public int MatchIndex { get; set; }
        public int GroupIndex { get; set; }
    }

}


