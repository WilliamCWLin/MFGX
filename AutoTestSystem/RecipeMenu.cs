using AutoTestSystem.Base;
using AutoTestSystem.Model;
using AutoTestSystem.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTestSystem
{
    public partial class RecipeMenu : Form
    {
        bool isedit = false;
        
        string NowMarkFolder = "";
        public RecipeMenu()
        {
            InitializeComponent();
            isedit = false;
            RecipeMenuDataGridView.ColumnHeadersHeight = 30;
            RecipeMenuDataGridView.AllowUserToAddRows = false;

            RecipeMenuDataGridView.EnableHeadersVisualStyles = false;
            RecipeMenuDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            RecipeMenuDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            RecipeMenuDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.LightBlue;
            RecipeMenuDataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //RecipeMenuDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            RecipeMenuDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            RecipeMenuDataGridView.RowHeadersWidth = 30;
            LoadDataFromJson();
        }

        private void BTN_ADDRecipe_Click(object sender, EventArgs e)
        {
            // 實例化新表單並顯示
            using (MenuEditForm entryForm = new MenuEditForm())
            {
                if (entryForm.ShowDialog() == DialogResult.OK)
                {
                    // 檢查是否有重複的
                    foreach (DataGridViewRow row in RecipeMenuDataGridView.Rows)
                    {
                        if (row.Cells["Project"].Value.ToString().Equals(entryForm.Project) && row.Cells["Station"].Value.ToString().Equals(entryForm.Station) && row.Cells["Mode"].Value.ToString().Equals(entryForm.Mode))
                        {
                            // 如果有重複，顯示提示並退出方法
                            MessageBox.Show("The prescription already exists, please set a unique name.", "Duplicate Prescription Name", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                            return;
                        }
                    }
                    if (entryForm.Project.Equals("") || entryForm.Station.Equals("") || entryForm.Mode.Equals("") || entryForm.Fixture.Equals(""))
                    {
                        // 如果有重複，顯示提示並退出方法
                        MessageBox.Show("Setting field cannot be empty..", "Field cannot be empty", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    // 創建資料夾
                    string folderPath = Path.Combine(Application.StartupPath, "config", "Recipe");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    // 在資料夾內創建 Recipe.json 檔案

                    string Proj = entryForm.Project;
                    string mode = entryForm.Mode;
                    string sta = entryForm.Station;
                    string fix = entryForm.Fixture;
                    string nowtime = entryForm.CreationTime;
                    string fileName = $@"{Proj}_{sta}_{mode}_{fix}_{nowtime}.json";
                    string filePath = Path.Combine(Application.StartupPath, "config", "Recipe", fileName);
                    // 從 EntryForm 獲取數據並添加到 DataGridView
                    RecipeMenuDataGridView.Rows.Add(entryForm.Project, entryForm.Station, entryForm.Fixture, entryForm.Mode, entryForm.Memo, entryForm.CreationTime, entryForm.Creator, $@"Config\Recipe\{fileName}");

                    var recipe = new ProTreeView.Recipe
                    {
                        Memo = entryForm.Memo,
                        CreationTime = nowtime,
                        Creator = entryForm.Creator,
                        Project = entryForm.Project,
                        Mode = entryForm.Mode,
                        Station = entryForm.Station,
                        Fixture = entryForm.Fixture,
                        FilePath = $@"Config\Recipe\{fileName}",
                        ExeVersion = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                        ConfigVersion = entryForm.VersionTextbox.Text,
                        Checksum = "NEW_EMPTY"
                    };
                    SaveRecipeInfo(filePath, recipe);
                }
            }
        }
        private void Delete_Click(object sender, EventArgs e)
        {
            if (RecipeMenuDataGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = RecipeMenuDataGridView.SelectedRows[0];
                string Proj = selectedRow.Cells["Project"].Value.ToString();
                string mode = selectedRow.Cells["Mode"].Value.ToString();
                string sta = selectedRow.Cells["Station"].Value.ToString();
                string fix = selectedRow.Cells["Fixture"].Value.ToString();
                string time = selectedRow.Cells["CreationTime"].Value.ToString();
                string file = selectedRow.Cells["FilePath"].Value.ToString();
                // Ask for confirmation before deletion
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the recipe and all its data?", "Confirm Deletion", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    string fileName = $@"{Proj}_{sta}_{mode}_{fix}_{time}.json";
                    string filePath = Path.Combine(Application.StartupPath, "config", "recipe", fileName);

                    if (GlobalNew.CurrentRecipePath.Contains(file))
                    {
                        MessageBox.Show("Unable to remove the prescription of the project in use.\n無法移除使用中的專案處方");
                        return;
                    }
                    string Backup_Path = Path.GetDirectoryName(filePath) + "/Delete";
                    if (!Directory.Exists(Backup_Path))
                        Directory.CreateDirectory(Backup_Path);

                    Backup_Path = $"{Backup_Path}/(Delete_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}){Path.GetFileName(filePath)}";
                    File.Copy(filePath, Backup_Path);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath); // Deletes the specified file
                    }

                    RecipeMenuDataGridView.Rows.Remove(selectedRow);

                    if (GlobalNew.CurrentRecipePath.Contains(Proj) && GlobalNew.CurrentRecipePath.Contains(mode) && GlobalNew.CurrentRecipePath.Contains(sta) && GlobalNew.CurrentRecipePath.Contains(fix))
                    {
                        isedit = true;
                    }

                    //SaveDataToJson();
                }         
            }
            
        }


        private void BTN_EditRecipe_Click(object sender, EventArgs e)
        {
            // 獲取選定的行
            if (RecipeMenuDataGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = RecipeMenuDataGridView.SelectedRows[0];
                string version = "V0.0.1.0";
                string folderPath = Path.Combine(Application.StartupPath, "config", "Recipe");
                string oldFileName = $@"{selectedRow.Cells["Project"].Value}_{selectedRow.Cells["Station"].Value}_{selectedRow.Cells["Mode"].Value}_{selectedRow.Cells["Fixture"].Value}_{selectedRow.Cells["CreationTime"].Value}.json";
                string fileName = $@"{selectedRow.Cells["Project"].Value}_{selectedRow.Cells["Station"].Value}_{selectedRow.Cells["Mode"].Value}_{selectedRow.Cells["Fixture"].Value}_{selectedRow.Cells["CreationTime"].Value}.json";
                {
                    string FilePath = Path.Combine(Application.StartupPath, "config", "Recipe", fileName);
                    var json = File.ReadAllText(FilePath);

                    DeserialErrors.Clear();

                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        MissingMemberHandling = MissingMemberHandling.Error,
                        Error = HandleDeserializationErrorPro
                    };
                    var recipe = JsonConvert.DeserializeObject<ProTreeView.RecipeInfo>(json, settings);

                    Version CurrentVersion = Assembly.GetEntryAssembly().GetName().Version;
                    version = recipe.Info.ConfigVersion;
                    if (recipe.Info.ExeVersion != null)
                    {
                        Version recipeVersion = new Version(recipe.Info.ExeVersion);
                        if (recipeVersion > CurrentVersion)
                        {
                            MessageBox.Show($"Recipe Version ({recipe.Info.ExeVersion}) is greater than the current exe version ({CurrentVersion}).");
                            return;
                        }
                    }

                    if (DeserialErrors.Count > 0)
                    {
                        string errorMessages = string.Join("\n", DeserialErrors);
                        MessageBox.Show(errorMessages, $"程式版本與處方建立版本不同，禁止修改!");

                        return;

                    }


                }
                // 實例化 EntryForm 並將選定行的數據傳遞給它
                using (MenuEditForm entryForm = new MenuEditForm(
                    selectedRow.Cells["Memo"].Value?.ToString() ?? "",
                    selectedRow.Cells["Project"].Value?.ToString() ?? "",
                    selectedRow.Cells["Mode"].Value?.ToString() ?? "",
                    selectedRow.Cells["Station"].Value?.ToString() ?? "",
                    selectedRow.Cells["Fixture"].Value?.ToString() ?? "",
                    selectedRow.Cells["CreationTime"].Value?.ToString() ?? "",
                    version
))
                {
                    if (entryForm.ShowDialog() == DialogResult.OK)
                    {
                        // 檢查是否有重複的
                        foreach (DataGridViewRow row in RecipeMenuDataGridView.Rows)
                        {
                            if (row.Cells["Project"].Value.ToString().Equals(entryForm.Project) && row.Cells["Station"].Value.ToString().Equals(entryForm.Station) && row.Cells["Mode"].Value.ToString().Equals(entryForm.Mode) && row.Cells["Fixture"].Value.ToString().Equals(entryForm.Fixture) && row.Cells["Memo"].Value.ToString().Equals(entryForm.Memo) && version.Equals(entryForm.Version))
                            {
                                // 如果有重複，顯示提示並退出方法
                                MessageBox.Show("The prescription already exists, please set a unique name.", "Duplicate Prescription Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                        // 更新 DataGridView 中的數據
                        selectedRow.Cells["Memo"].Value = entryForm.Memo;
                        selectedRow.Cells["CreationTime"].Value = entryForm.CreationTime;
                        selectedRow.Cells["Creator"].Value = entryForm.Creator;
                        selectedRow.Cells["Project"].Value = entryForm.Project;
                        selectedRow.Cells["Mode"].Value = entryForm.Mode;
                        selectedRow.Cells["Station"].Value = entryForm.Station;
                        selectedRow.Cells["Fixture"].Value = entryForm.Fixture;





                        // 如果資料夾名稱有變更，則重命名資料夾
                        string newFolderPath = Path.Combine(Application.StartupPath, "config", "Recipe");

                        // 構建新的檔案名稱和路徑
                        string newFileName = $@"{entryForm.Project}_{entryForm.Station}_{entryForm.Mode}_{entryForm.Fixture}_{entryForm.CreationTime}.json";
                        string newFilePath = Path.Combine(Application.StartupPath, "config", "Recipe", newFileName);

                        selectedRow.Cells["FilePath"].Value = $@"Config\Recipe\{newFileName}";


                        try
                        {
                            // 資料夾名稱沒有變更，檢查檔案名稱是否需要更新                            
                            string oldFilePath = Path.Combine(folderPath, oldFileName);
                            if (oldFileName != newFileName)
                            {
                                // 檔案名稱有變更，進行重命名
                                File.Move(oldFilePath, newFilePath);
                            }

                            var json = File.ReadAllText(newFilePath);

                            var recipe = JsonConvert.DeserializeObject<ProTreeView.RecipeInfo>(json);


                            recipe.Info.Memo = entryForm.Memo;
                            recipe.Info.CreationTime = entryForm.CreationTime.ToString();
                            recipe.Info.Creator = entryForm.Creator;
                            recipe.Info.Project = entryForm.Project;
                            recipe.Info.Mode = entryForm.Mode;
                            recipe.Info.Station = entryForm.Station;
                            recipe.Info.Fixture = entryForm.Fixture;
                            recipe.Info.FilePath = $@"Config\Recipe\{newFileName}";
                            recipe.Info.ConfigVersion = entryForm.Version;
                            //var recipe = new ProTreeView.Recipe
                            //{
                            //    Memo = entryForm.Memo,
                            //    CreationTime = entryForm.CreationTime.ToString(),
                            //    Creator = entryForm.Creator,
                            //    Project = entryForm.Project,
                            //    Mode = entryForm.Mode,
                            //    Station = entryForm.Station,
                            //    Fixture = entryForm.Fixture,
                            //    FilePath = $@"Config\Recipe\{newFileName}"
                            //};

                            SaveRecipeInfo(newFilePath, recipe.Info);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"error occurred: {ex.Message}");
                        }

                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a row to edit.");
            }
        }
        private void SaveRecipeInfo(string path, ProTreeView.Recipe infodata)
        {
            // 初始化 json 字串
            string json = "";

            // 檢查指定路徑的文件是否存在
            if (File.Exists(path))
            {
                // 讀取文件內容
                json = File.ReadAllText(path);
            }

            // 定義共用的 JsonSerializerSettings
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

            // 反序列化 json 字串到 RecipeInfo 物件
            var data = JsonConvert.DeserializeObject<ProTreeView.RecipeInfo>(json, settings);

            // 如果反序列化結果為 null，則創建新的 RecipeInfo 物件
            if (data == null)
            {
                data = new ProTreeView.RecipeInfo
                {
                    Process = new List<Manufacture.CoreBase>(),
                    Devices = new List<Manufacture.CoreBase>(),
                    Info = infodata
                };
            }
            else
            {
                // 如果已存在數據，則更新 Info 屬性
                data.Info = infodata;
            }

            // 序列化整個 data 物件，包含類型信息
            string finalJson = JsonConvert.SerializeObject(data, Formatting.Indented, settings);

            // 將最終的 json 字串寫入文件
            File.WriteAllText(path, finalJson);
        }
        //private void SaveDataToJson()
        //{
        //    var recipes = new List<ProTreeView.Recipe>();

        //    foreach (DataGridViewRow row in RecipeMenuDataGridView.Rows)
        //    {
        //        if (!row.IsNewRow)
        //        {
        //            var recipe = new ProTreeView.Recipe
        //            {
        //                Memo = row.Cells["Memo"].Value?.ToString(),
        //                CreationTime = row.Cells["CreationTime"].Value?.ToString(),
        //                Creator = row.Cells["Creator"].Value?.ToString(),                                            
        //                Project = row.Cells["Project"].Value?.ToString(),
        //                Mode = row.Cells["Mode"].Value?.ToString(),
        //                Station = row.Cells["Station"].Value?.ToString(),
        //                Fixture = row.Cells["Fixture"].Value?.ToString(),
        //                FilePath = row.Cells["FilePath"].Value?.ToString()
        //            };
        //            recipes.Add(recipe);

        //            // 檢查這行是否是當前選用的處方
        //            //if (/* 條件來確定這行是否是當前選用的處方 */)
        //            //{
        //            //    currentPrescription = recipe.Name;
        //            //}
        //        }
        //    }

        //    var dataToSave = new
        //    {
        //        Recipes = recipes,
        //        CurrentRecipePath = GlobalNew.CurrentRecipePath,
        //        CurrentDevicePath = GlobalNew.CurrentRecipePath,
        //        CurrentProject = GlobalNew.CurrentProject,
        //        CurrentMode = GlobalNew.CurrentMode,
        //        CurrentStation = GlobalNew.CurrentStation,
        //        CurrentFixture = GlobalNew.CurrentFixture,

        //    };

        //    string json = JsonConvert.SerializeObject(dataToSave, Formatting.Indented);
        //    File.WriteAllText("Config/Recipes.config", json);
        //}

        private void LoadDataFromJson()
        {
            //if (File.Exists("Config/Recipes.config"))
            //{
            //    string json = File.ReadAllText("Config/Recipes.config");
            //    var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            //    var recipesList = JsonConvert.DeserializeObject<List<Recipe>>(jsonData["Recipes"].ToString());
            //    RecipeMenuDataGridView.Rows.Clear();
            //    foreach (var recipe in recipesList)
            //    {
            //        RecipeMenuDataGridView.Rows.Add(recipe.Project, recipe.Station, recipe.Fixture, recipe.Mode, recipe.Memo, recipe.CreationTime, recipe.Creator);
            //    }

            //    MarkCurrentSelectedProject();
            //    // 設置當前選用的處方
            //    //GlobalNew.CurrentSelectedProject = jsonData["CurrentRecipe"].ToString();
            //    // 這裡您可以添加代碼來處理當前選用的處方，例如選擇相應的行或進行其他操作
            //}
            string path = "Config\\Recipe";
            if (!Directory.Exists(path)) // 如果資料夾不存在
            {
                Directory.CreateDirectory(path); // 則創建資料夾
            }
            // 獲取資源夾中所有的JSON檔案
            string[] jsonFiles = Directory.GetFiles("Config\\Recipe", "*.json");

            foreach (var file in jsonFiles)
            {
                try
                {
                    // 讀取檔案內容
                    string jsonContent = File.ReadAllText(file);

                    // 反序列化JSON內容為RecipeInfo物件
                    var recipeInfo = JsonConvert.DeserializeObject<ProTreeView.RecipeInfo>(jsonContent);

                    // 檢查recipeInfo是否為null
                    if (recipeInfo != null && recipeInfo.Info != null)
                    {
                        // 從recipeInfo物件中提取Info屬性
                        var info = recipeInfo.Info;
                        if (info.FilePath != file)
                        {
                            MessageBox.Show($"{file} File Format Error!");
                            continue;
                        }
                        // 將信息添加到DataGridView中
                        RecipeMenuDataGridView.Rows.Add(
                            info.Project,
                            info.Station,
                            info.Fixture,
                            info.Mode,
                            info.Memo,
                            info.CreationTime,
                            info.Creator,
                            info.FilePath
                        );
                    }
                }
                catch (Exception) { }
            }

            MarkCurrentSelectedProject();
        }

        private void BTN_APPLY_Click(object sender, EventArgs e)
        {
            if (RecipeMenuDataGridView.SelectedRows.Count > 0)
            {
                string prj = RecipeMenuDataGridView.SelectedRows[0].Cells["Project"].Value.ToString();
                string mode = RecipeMenuDataGridView.SelectedRows[0].Cells["Mode"].Value.ToString();
                string sta = RecipeMenuDataGridView.SelectedRows[0].Cells["Station"].Value.ToString();
                string fix = RecipeMenuDataGridView.SelectedRows[0].Cells["Fixture"].Value.ToString();
                string time = RecipeMenuDataGridView.SelectedRows[0].Cells["CreationTime"].Value.ToString();

                //=============需將devices重新載入並Uninit再將新的init=============
                string FileName = $"{prj}_{sta}_{mode}_{fix}_{time}";
                RecipeManagement NewManagement = new RecipeManagement(FileName);
                if (!NewManagement.GetReadRecipeStatus())
                {
                    MessageBox.Show("版本或功能類別存在差異，套用失敗請選擇其它處方或重新編輯", $"異常");

                    return;
                }



                //===================================================
                
                string fileName = $@"{prj}_{sta}_{mode}_{fix}_{time}.json";
                GlobalNew.CurrentRecipePath = $@"Config\Recipe\{fileName}";
                GlobalNew.CurrentProject = prj;
                GlobalNew.CurrentMode = mode;
                GlobalNew.CurrentFixture = fix;
                GlobalNew.CurrentStation = sta;
                string tempoldRecipe = GlobalNew.CurrentRecipePath;
                var json = File.ReadAllText(tempoldRecipe);
                var recipe = JsonConvert.DeserializeObject<ProTreeView.RecipeInfo>(json);
                GlobalNew.CurrentConfigVersion = recipe.Info.ConfigVersion;
                //=============需將devices重新載入並Init=============

                //string ret = ProTreeView.ProTreeView.Load_Devices(GlobalNew.CurrentDevicePath, GlobalNew.Devices);     //! ProTreeView load device

                //if (!NewManagement.InitDevices())
                //    MessageBox.Show("InitDevices Fail");
                //===================================================
                NewManagement.SaveHeader();

                MarkCurrentSelectedProject();


                //如果選了另一個處方要通知Mainform更新
                //if (tempoldRecipe != GlobalNew.CurrentRecipePath)
                //{

                //裝置被編輯過則需UNINIT後移除，並以新的選
                //if (!RecipeManagement.StaticUnInitDevices(GlobalNew.Devices))
                //{
                //    MessageBox.Show("Uninitialization of the old device failed.");
                //    DialogResult = DialogResult.No;

                //    return;
                //}
                //if (!RecipeManagement.InitDevices(GlobalNew.CurrentDevicePath, GlobalNew.Devices))
                //{
                //    MessageBox.Show("Initialization of the device failed.");
                //    DialogResult = DialogResult.No;

                //    return;
                //}

                    
                if (isedit == true)
                    DialogResult = DialogResult.Yes;
                else
                    DialogResult = DialogResult.No;
                //}
                //else
                //{
                //    //如果選同一處方但有進去修改按過Save則一樣通知Mainform更新
                //    if (isedit == true)
                //        DialogResult = DialogResult.Yes;
                //    else
                //        DialogResult = DialogResult.No;
                //}
                //DialogResult result = MessageBox.Show("Process settings have been edited, do you want to apply the changes?\n流程已重新編輯並儲存是否重新套用到生產清單",
                //                       "Ask",
                //                       MessageBoxButtons.YesNo,
                //                       MessageBoxIcon.Question);
                //if (result == DialogResult.Yes)
                //{

                //    isedit = true;
                //}
            }
            else
            {
                MessageBox.Show("Please select a recipe first.");
            }
        }

        private void MarkCurrentSelectedProject()
        {
            foreach (DataGridViewRow row in RecipeMenuDataGridView.Rows)
            {
                row.DefaultCellStyle.BackColor = Color.White; // 設置為預設的背景顏色
            }

            foreach (DataGridViewRow row in RecipeMenuDataGridView.Rows)
            {
                if(row.Cells["Project"].Value.ToString() != null)
                {
                    string Path = row.Cells["FilePath"].Value.ToString();
                    bool contains = Path.IndexOf(GlobalNew.CurrentRecipePath, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (contains)
                    {
                        NowMarkFolder = $"{row.Cells["Project"].Value.ToString()}\\{row.Cells["Project"].Value.ToString()}_{row.Cells["Station"].Value.ToString()}_{row.Cells["Mode"].Value.ToString()}_{row.Cells["Fixture"].Value.ToString()}";
                        // 設置背景顏色來標記當前選用的處方
                        row.DefaultCellStyle.BackColor = Color.LightGreen; // 您可以選擇您喜歡的顏色
                        break; // 如果只有一個當前選用的處方，找到後就可以停止搜尋
                    }
                }

            }
        }


        private void RecipeMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(isedit)
            {
                DialogResult = DialogResult.Yes;
            }
        }

        private void RecipeMenuDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // 確保點擊的不是列頭
            if (e.RowIndex >= 0)
            {
                // 獲取選中行的數據
                //string RecipeName = RecipeMenuDataGridView.Rows[e.RowIndex].Cells["Project"].Value.ToString();
                // 這裡添加您想要執行的代碼
                // 例如，打開一個新的編輯表單，並將選中的處方名稱傳遞給它
                string Project = RecipeMenuDataGridView.Rows[e.RowIndex].Cells["Project"].Value?.ToString();
                string Mode = RecipeMenuDataGridView.Rows[e.RowIndex].Cells["Mode"].Value?.ToString();
                string Station = RecipeMenuDataGridView.Rows[e.RowIndex].Cells["Station"].Value?.ToString();
                string fix = RecipeMenuDataGridView.Rows[e.RowIndex].Cells["Fixture"].Value?.ToString();
                string time = RecipeMenuDataGridView.Rows[e.RowIndex].Cells["CreationTime"].Value?.ToString();
                string FileName = $"{Project}_{Station}_{Mode}_{fix}_{time}";
                RecipeManagement Management = new RecipeManagement(FileName);

                if (!Management.GetReadRecipeStatus())
                {
                    MessageBox.Show("版本或功能類別存在差異，無法開啟編輯", $"異常");

                    return;
                }
                    
                DialogResult result = Management.ShowDialog();

                if (result == DialogResult.Yes)
                {
                    string jsonRecipePath = Path.Combine(System.Environment.CurrentDirectory, "Config", $"{FileName}_Recipe.json");
                    string normalizedJsonRecipePath = Path.GetFullPath(jsonRecipePath);
                    string normalizedCurrentRecipePath = Path.GetFullPath(GlobalNew.CurrentRecipePath);

                    if (string.Equals(normalizedJsonRecipePath, normalizedCurrentRecipePath, StringComparison.OrdinalIgnoreCase))
                    {
                        isedit = true;
                    }
                }
            }
        }

        private void RecipeMenuDataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            // 按下Ctrl+C，複製選中行的兩個JSON
            if (e.Control && e.KeyCode == Keys.C)
            {
                e.Handled = true;
                CopySelectedRecipeJson();
            }
            // 按下Ctrl+V，粘貼兩個JSON到選中行
            else if (e.Control && e.KeyCode == Keys.V)
            {
                e.Handled = true;
                PasteRecipeJson();
            }
        }

        private void CopySelectedRecipeJson()
        {
            if (RecipeMenuDataGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = RecipeMenuDataGridView.SelectedRows[0];
                string path = selectedRow.Cells["FilePath"].Value.ToString();
                // 讀取兩個JSON文件的內容
                string recipeJsonPath = Path.Combine(Application.StartupPath, $"{path}"); 

                if (File.Exists(recipeJsonPath) )//)
                {
                    string recipeJsonContent = File.ReadAllText(recipeJsonPath);
                    //string deviceJsonContent = File.ReadAllText(deviceJsonPath);

                    // 將JSON內容合併並放入剪貼板
                    Clipboard.SetText(recipeJsonContent);
                    //copydata = Clipboard.GetText();
                }
            }
        }

        private void PasteRecipeJson()
        {
            // 詢問用戶是否確定進行數據貼上的動作
            DialogResult result = MessageBox.Show("Are you sure you want to paste the copied data into the selected row?", "Paste Data", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            string copydata = Clipboard.GetText();

            if (copydata == string.Empty)
            {
                MessageBox.Show("Copy Data is Null!!");
                return; 
            }

            DeserialErrors.Clear();
            if (RecipeMenuDataGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = RecipeMenuDataGridView.SelectedRows[0];
                string selpath = selectedRow.Cells["FilePath"].Value.ToString();

                string seldata = File.ReadAllText(selpath);
                if (!File.Exists(selpath))
                    return;

                // 定義共用的 JsonSerializerSettings
                //var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    MissingMemberHandling = MissingMemberHandling.Error,
                    Error = HandleDeserializationErrorPro
                };
                // 反序列化 json 字串到 RecipeInfo 物件
                var data = JsonConvert.DeserializeObject<ProTreeView.RecipeInfo>(seldata, settings);
                var cpydata = JsonConvert.DeserializeObject<ProTreeView.RecipeInfo>(copydata, settings);

                if (DeserialErrors.Count > 0)
                {
                    string errorMessages = string.Join("\n", DeserialErrors);
                    if (errorMessages.Length > 1200) // Adjust the length as needed
                    {
                        errorMessages = errorMessages.Substring(0, 1200) + "..."; // Truncate and add ellipsis
                    }
                    MessageBox.Show(errorMessages, $"類別存在差異(處方生成版本{(data.Info.ExeVersion ?? "NULL")}) (目前版本{Assembly.GetEntryAssembly().GetName().Version.ToString()})");
                    DialogResult isContinue = MessageBox.Show($"是否繼續？ 如果繼續複製 將會遺失缺少的部分\n請確實修正差異部分造成的影響\n\n{errorMessages}", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (isContinue == DialogResult.Yes)
                    {
                        //MessageBox.Show("重新編輯，請確認修正以下類別造成的差異\n" + errorMessages);
                    }
                    else
                    {
                        return;
                    }
                }

                // 如果反序列化結果為 null，則創建新的 RecipeInfo 物件
                if (data == null || cpydata==null)
                {
                    return;
                }
                else
                {
                    // 只更新Recipe跟Devices，Info中只更新ChecksumInfo
                    data.Process = cpydata.Process;
                    data.Devices = cpydata.Devices;

                    //如果存在差異必須在複製到新處方時重新計算Checksum值，否則載入時會Checksum失敗
                    //if(DeserialErrors.Count > 0)
                    //{
                    //    string strProcess_json = JsonConvert.SerializeObject(cpydata.Process, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                    //    string strDev_json = JsonConvert.SerializeObject(cpydata.Devices, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                    //    data.Info.Checksum = ProTreeView.ProTreeView.ComputeChecksum(strDev_json);

                    //}
                    //else
                    //{
                        data.Info.Checksum = cpydata.Info.Checksum;

                    //}
                }

                // 序列化整個 data 物件，包含類型信息
                string finalJson = JsonConvert.SerializeObject(data, Formatting.Indented, settings);

                string recipeJsonPath = Path.Combine(Application.StartupPath, $"{selpath}");

                // 將最終的 json 字串寫入文件
                File.WriteAllText(recipeJsonPath, finalJson);

                copydata = string.Empty;

            }
        }
        private static List<string> DeserialErrors = new List<string>();
        private static void HandleDeserializationErrorPro(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            // 如果是由於未知的型別引起的錯誤，則忽略該錯誤
            if (e.ErrorContext.Error is JsonSerializationException jsonEx)
            {
                string errorMessage = $"錯誤原因: {e.ErrorContext.Error.Message}, 錯誤位置: {jsonEx.LineNumber}";
                DeserialErrors.Add(errorMessage);

            }

            // 如果錯誤信息包含缺少的成員信息，則提取並記錄該信息
            //if (e.ErrorContext.Error.Message.Contains("Could not find member"))
            //{
            //    var missingProperties = GetMissingProperties(e.ErrorContext.Error.Message);
            //    string errorMessage = $"Warning: JSON file contains properties that are not present in the class: {string.Join(", ", missingProperties)}";
            //    deserializationErrors.Add(errorMessage);
            //}

            e.ErrorContext.Handled = true;
        }
    }


}
