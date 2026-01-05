using AutoTestSystem.Base;
using AutoTestSystem.Equipment.Teach;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using AutoTestSystem.Model; // Size / Point

namespace AutoTestSystem
{
    public partial class MutiAIForm : Form
    {
        private readonly string DevicesName;
        private readonly List<string> _aiNameSource;
        private string _jsonDataField; // 仍保留（若你要輸出選擇結果）
        private ContextMenuStrip _ctxMenu;

        // ComboBox 的空白選項（你可改成 "— 清空 —"）
        private const string BlankOptionText = "";

        public MutiAIForm(string devicesName, List<string> aiNameSource = null, string preloadJson = null)
        {
            DevicesName = devicesName ?? string.Empty;
            _aiNameSource = aiNameSource;

            // 視窗外觀與尺寸
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1200, 800);
            this.MinimumSize = new Size(900, 600);
            this.Text = "AI 定義名稱設定";
            this.AutoScaleMode = AutoScaleMode.Dpi;

            EnsureDataGridViewExists();
            InitializeDataGridView();
            ///InitializeContextMenu();

            // 預載 JSON（若有）
            if (!string.IsNullOrEmpty(preloadJson))
                LoadDataGridViewFromJson(preloadJson);

            // 基本互動設定
            AI_DataGridView.EditMode = DataGridViewEditMode.EditOnEnter;
            AI_DataGridView.MultiSelect = true;
            AI_DataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 事件掛載
            AI_DataGridView.DefaultValuesNeeded += AI_DataGridView_DefaultValuesNeeded;       // 新列預設值
            AI_DataGridView.CellClick += AI_DataGridView_CellClick;                           // 點選 AIName 展開下拉
            AI_DataGridView.CurrentCellDirtyStateChanged += AI_DataGridView_CurrentCellDirtyStateChanged; // 只對 ComboBox Commit
            AI_DataGridView.CellValueChanged += AI_DataGridView_CellValueChanged;             // 選到空白 → 清空
            AI_DataGridView.CellMouseDown += AI_DataGridView_CellMouseDown;                   // 右鍵/行首選列
            AI_DataGridView.DataError += AI_DataGridView_DataError;                           // 靜默
            AI_DataGridView.KeyDown += AI_DataGridView_KeyDown;                               // Delete 刪列

            this.FormClosing += MutiAISelect_FormClosing;                                     // 關閉前總驗證
        }

        // === 新列預設值：AIName 空白 ===
        private void AI_DataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["AIName"].Value = BlankOptionText;
        }

        // === 只針對 ComboBox 髒狀態提交 ===
        private void AI_DataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (!AI_DataGridView.IsCurrentCellDirty) return;
            var cell = AI_DataGridView.CurrentCell;
            if (cell is DataGridViewComboBoxCell)
                AI_DataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        // === 點第一欄先 BeginEdit 再展開下拉 ===
        private void AI_DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (AI_DataGridView.Columns[e.ColumnIndex].Name == "AIName")
            {
                AI_DataGridView.BeginEdit(true);
                if (AI_DataGridView.EditingControl is DataGridViewComboBoxEditingControl cb)
                    cb.DroppedDown = true;
            }
        }

        // === 選到空白選項時，清空該列 ===
        private void AI_DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (AI_DataGridView.Columns[e.ColumnIndex].Name != "AIName") return;

            var val = AI_DataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            var text = Convert.ToString(val);

            if (string.Equals(text, BlankOptionText, StringComparison.Ordinal))
                ClearRow(e.RowIndex);
        }

        private void ClearRow(int rowIndex)
        {
            var row = AI_DataGridView.Rows[rowIndex];
            if (row == null || row.IsNewRow) return;

            row.Cells["AIName"].Value = BlankOptionText;
            row.ErrorText = "";
        }

        // === 右鍵/行首選列（CellMouseDown 可避免 HitTest -1） ===
        private void AI_DataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.Button == MouseButtons.Right)
            {
                AI_DataGridView.ClearSelection();
                var row = AI_DataGridView.Rows[e.RowIndex];
                if (!row.IsNewRow) row.Selected = true;

                int targetColIndex = e.ColumnIndex < 0 ? 0 : e.ColumnIndex; // 只有一欄：AIName
                if (targetColIndex >= 0 && targetColIndex < AI_DataGridView.Columns.Count)
                    AI_DataGridView.CurrentCell = row.Cells[targetColIndex];
            }
        }

        // === DataError 靜默處理 ===
        private void AI_DataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        // === Delete 鍵刪除選取列 ===
        private void AI_DataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedRowsSafe();
                e.Handled = true;
            }
        }

        private void DeleteSelectedRowsSafe()
        {
            if (AI_DataGridView.IsCurrentCellInEditMode)
            {
                try { AI_DataGridView.EndEdit(); } catch { /* ignore */ }
            }

            var toDelete = new List<DataGridViewRow>();
            foreach (DataGridViewRow r in AI_DataGridView.SelectedRows)
            {
                if (!r.IsNewRow) toDelete.Add(r);
            }

            if (toDelete.Count == 0)
            {
                var cur = AI_DataGridView.CurrentCell;
                if (cur != null && cur.RowIndex >= 0)
                {
                    var row = AI_DataGridView.Rows[cur.RowIndex];
                    if (!row.IsNewRow) toDelete.Add(row);
                }
            }

            foreach (var r in toDelete)
            {
                try { AI_DataGridView.Rows.Remove(r); } catch { /* ignore */ }
            }
        }

        #region 初始化 DataGridView 與欄位
        private void EnsureDataGridViewExists()
        {
            if (AI_DataGridView == null)
            {
                AI_DataGridView = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    AllowUserToAddRows = true,
                    AllowUserToDeleteRows = true,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    Name = "AI_DataGridView"
                };
                this.Controls.Add(AI_DataGridView);
            }
        }

        private void InitializeDataGridView()
        {
            AI_DataGridView.Columns.Clear();

            var aiNameColumn = new DataGridViewComboBoxColumn
            {
                Name = "AIName",
                HeaderText = "AI 定義名稱",
                FlatStyle = FlatStyle.Flat,
                DataSource = BuildAINameDataSource()
            };
            aiNameColumn.DisplayStyleForCurrentCellOnly = true;
            AI_DataGridView.Columns.Add(aiNameColumn);
        }

        private List<string> BuildAINameDataSource()
        {
            var list = (_aiNameSource != null && _aiNameSource.Count > 0)
                ? new List<string>(_aiNameSource)
                : AI_NameList() ?? new List<string>();

            if (!list.Contains(BlankOptionText))
                list.Insert(0, BlankOptionText);

            return list;
        }

        public List<string> AI_NameList()
        {
            var names = new List<string>();

            if (string.IsNullOrWhiteSpace(DevicesName)) return names;
            if (!GlobalNew.Devices.ContainsKey(DevicesName)) return names;

            try
            {
                if (GlobalNew.Devices[DevicesName] is IOTeach ioTeach)
                    names.AddRange(ioTeach.GetAIKeys());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"取得 AI 名稱清單時發生例外：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return names;
        }
        #endregion

        #region JSON 輸出/載入（僅 AIName）
        public string GetDataGridViewAsJson()
        {
            try
            {
                var data = AI_DataGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow)
                    .Select(r => new DeviceAIItem
                    {
                        AI_Name = r.Cells["AIName"].Value?.ToString()
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x.AI_Name) && x.AI_Name != BlankOptionText)
                    .ToList();

                if (data.Count == 0)
                    return string.Empty;

                return JsonConvert.SerializeObject(data, Formatting.Indented);
            }
            catch
            {
                return string.Empty;
            }
        }

        public void LoadDataGridViewFromJson(string jsonData)
        {
            if (string.IsNullOrWhiteSpace(jsonData)) return;

            try
            {
                var data = JsonConvert.DeserializeObject<List<DeviceAIItem>>(jsonData);
                if (data == null) return;

                foreach (var item in data)
                {
                    int rowIndex = AI_DataGridView.Rows.Add();
                    var row = AI_DataGridView.Rows[rowIndex];

                    row.Cells["AIName"].Value = item.AI_Name ?? BlankOptionText;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"載入 JSON 時發生錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 關閉前總驗證與結果
        private void MutiAISelect_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 是否有任何資料列（含空白）
            bool hasAny = AI_DataGridView.Rows
               .Cast<DataGridViewRow>()
               .Any(row => !row.IsNewRow && row.Cells.Cast<DataGridViewCell>()
                   .Any(cell => cell.Value != null && !string.IsNullOrWhiteSpace(Convert.ToString(cell.Value))));

            if (hasAny)
            {
                var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (DataGridViewRow row in AI_DataGridView.Rows)
                {
                    if (row.IsNewRow) continue;

                    string aiName = row.Cells["AIName"].Value?.ToString();

                    // 空白列略過
                    if (string.IsNullOrEmpty(aiName) || aiName == BlankOptionText)
                        continue;

                    // 名稱不重複
                    if (!names.Add(aiName))
                    {
                        MessageBox.Show("AI 定義名稱不可重複。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Cancel = true;
                        return;
                    }
                }

                // 產出 JSON（若你需要）
                var json = GetDataGridViewAsJson();
                _jsonDataField = json;
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                _jsonDataField = string.Empty;
            }
        }

        public string JsonResult => _jsonDataField;
        #endregion

        #region 模型
        public class DeviceAIItem
        {
            public string AI_Name { get; set; }
        }
        #endregion

    }
}