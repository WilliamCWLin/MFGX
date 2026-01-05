/*
 * "AutoTestSystem --> Recipe Management UI"
 * 
 * Corpright William & Zhibin
 *
 *.##.......####.########.########..#######..##....##
 *.##........##.....##....##.......##.....##.###...##
 *.##........##.....##....##.......##.....##.####..##
 *.##........##.....##....######...##.....##.##.##.##
 *.##........##.....##....##.......##.....##.##..####
 *.##........##.....##....##.......##.....##.##...###
 *.########.####....##....########..#######..##....##
 *
 *  1. <RecipeManagement.cs> is Designer UI that you can edit script in here.
 *  2. If you have error "請確認已經參考包含此類型的組件。如果此類型是您開發專案的一部分，請確認此專案是否已使用您目前平台的設定或 [Any CPU] 成功建置。"
 *     Please use Any CPU to compiler
 *  3. EntryPoint:  MainForm.cs script design button --> RecipeManagement.cs  
 *
 * 
 */

/*****************************************************************************
*                    Includes Definitions
*****************************************************************************/
using AutoMapper;
using Automation.BDaq;
using AutoTestSystem.Base;
using AutoTestSystem.BLL;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Model;
using AutoTestSystem.Script;
using Manufacture;

using MvCamCtrl.NET;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ProTreeView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Manufacture.CoreBase;
using static ProTreeView.ProTreeView;

/*****************************************************************************
*                    Function code
*****************************************************************************/
//add by MTE_William
namespace AutoTestSystem
{
    public partial class RecipeManagement : Form
    {
        public bool IsEditing { get; set; }
        private Dictionary<string, string> CSVHeader = new Dictionary<string, string>();
        private Dictionary<string, object> Devices = new Dictionary<string, object>();
        string jsonStr_header = string.Empty;
        string jsonDevicePath = string.Empty;
        string jsonRecipePath = string.Empty;
        string start_jsonData = string.Empty;
        private Size originalSize;
        private Point originalLocation;
        private bool isMaximized;
        private bool isReadScriptSuccess = false;
        private bool isCompare = false;
        public MLog RecipeLOGGER;
        public MLog DeviceLOGGER;
        //private List<User> userList;
        public RecipeManagement()
        {
            //MainForm.test.StepTest();
            InitializeComponent();
            IsEditing = false;
            proTreeView_Devices.SetMode(FlowMode.HW_Mode);
            proTreeView_Devices.AddImage("camera.png", imageList1.Images["camera.png"]);
            proTreeView_Devices.AddImage("device.png", imageList1.Images["device.png"]);
            proTreeView_Devices.AddImage("up-and-down.png", imageList1.Images["up-and-down.png"]);
            Manufacture.Initialize.FindDerived(CCDList_CB, typeof(CCDBase));
            Manufacture.Initialize.FindDerived(MotorList_CB, typeof(MotionBase));
            Manufacture.Initialize.FindDerived(ControlList_CB, typeof(ControlDeviceBase));
            Manufacture.Initialize.FindDerived(IOList_CB, typeof(IOBase));
            Manufacture.Initialize.FindDerived(VISAList_CB, typeof(VisaBase));
            Manufacture.Initialize.FindDerived(DUTList_CB, typeof(DUT_BASE));

            proTreeView_Devices.Read_DeviceList(GlobalNew.DeviceListPath);


            proTreeView_RecipeProcess.SetMode(FlowMode.Mode3);
            SeqNode_CB.Items.Add((typeof(Manufacture.SeqNode)).Name);
            SeqItem_CB.Items.Add((typeof(Manufacture.SeqItem)).Name);

            if (Global_Memory.UserLevel < 3)
            {
                Recipe_Chose.Items.Add("Online");
                Recipe_Chose.Items.Add("Golden");
            }
            else
            {
                Recipe_Chose.Items.Add("Online");
                Recipe_Chose.Items.Add("Golden");
                Recipe_Chose.Items.Add("Debug");
            }

            Recipe_Chose.SelectedIndexChanged += Recipe_Chose_SelectedIndexChanged;

            string jsonPath = string.Empty;
            if (GlobalNew.RECIPENAME == "Golden")
                jsonPath = $@"{System.Environment.CurrentDirectory}\Config\Golden.json";
            else if (GlobalNew.RECIPENAME == "Debug")
                jsonPath = $@"{System.Environment.CurrentDirectory}\Config\Debug.json";
            else
                jsonPath = $@"{System.Environment.CurrentDirectory}\Config\{GlobalNew.STATIONNAME}.json";
            proTreeView_RecipeProcess.Read_ScriptRecipe(jsonPath);
            SN_TEXT2.Enabled = false;

            //==================================
            proTreeView_RecipePro.SetMode(FlowMode.Process_Mode);
            proTreeView_RecipePro.AddImage("camera.png", imageList1.Images["camera.png"]);
            proTreeView_RecipePro.AddImage("device.png", imageList1.Images["device.png"]);
            proTreeView_RecipePro.AddImage("up-and-down.png", imageList1.Images["up-and-down.png"]);
            proTreeView_RecipePro.AddImage("workflow.png", imageList1.Images["workflow.png"]);
            Manufacture.Initialize.FindDerived(comboBox_Sequences, typeof(Script_Container_Base));
            Manufacture.Initialize.FindDerived(comboBox_IOScript, typeof(ScriptIOBase));
            Manufacture.Initialize.FindDerived(comboBox_ExtraScript, typeof(Script_Extra_Base));
            Manufacture.Initialize.FindDerived(comboBox_DUTScript, typeof(ScriptDUTBase));
            Manufacture.Initialize.FindDerived(comboBox_CCDScript, typeof(Script_CCD_Base));
            Manufacture.Initialize.FindDerived(comboBox_CtrlScript, typeof(Script_ControlDevice_Base));
            proTreeView_RecipePro.MouseDoubleClickEvent += EventHandlerMethod;

            // 訂閱 DLL 中的事件
            //proTreeView_RecipePro.TreeViewKeyDownEvent += MyDLLTreeView_KeyDown;

            string jsonRecipePath = string.Empty;
            if (GlobalNew.RECIPENAME == "Golden")
                jsonRecipePath = $@"{System.Environment.CurrentDirectory}\Config\Golden.json";
            else if (GlobalNew.RECIPENAME == "Debug")
                jsonRecipePath = $@"{System.Environment.CurrentDirectory}\Config\Debug.json";
            else
                jsonRecipePath = $@"{System.Environment.CurrentDirectory}\Config\TestRecipePro.json";
            proTreeView_RecipePro.Read_Recipe(jsonRecipePath);



            label_RecipName.Text = Path.GetFileName(jsonPath);



            proTreeView_RecipeProcess.MouseDoubleClickEvent += EventHandlerMethod;


            // 初始化使用者列表
            //userList = new List<User>();

            // 設置 DataGridView 屬性
            UserdataGridView.AutoGenerateColumns = false;
            UserdataGridView.AllowUserToAddRows = false;
            UserdataGridView.RowTemplate.Height = 18;
            UserdataGridView.RowHeadersWidth = 25;
            UserdataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.Blue;
            UserdataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold); // 設定列標題的字體、大小和樣式;

            DataGridViewTextBoxColumn usernameColumn = new DataGridViewTextBoxColumn();
            usernameColumn.DataPropertyName = "Username"; // 屬性名稱要與 User 類別中的一致
            usernameColumn.HeaderText = "Username";
            usernameColumn.HeaderCell.Style.BackColor = Color.Blue;
            UserdataGridView.Columns.Add(usernameColumn);

            DataGridViewTextBoxColumn PasswordColumn = new DataGridViewTextBoxColumn();
            if (GlobalNew.Weblogin != "1")
            {
                PasswordColumn.DataPropertyName = "Password"; // 屬性名稱要與 User 類別中的一致
                PasswordColumn.HeaderText = "Password";
            }
            else
            {
                PasswordColumn.DataPropertyName = "Email"; // 屬性名稱要與 User 類別中的一致
                PasswordColumn.HeaderText = "Email";
            }
            PasswordColumn.HeaderCell.Style.BackColor = Color.Blue;
            if (Global_Memory.UserLevel >= (int)User_Level.ADMIN)
                PasswordColumn.Visible = true; // 設置為顯示
            else
            {
                PasswordColumn.Visible = false; // 設置為不顯示
                QBtn_DeleteUser.Enabled = false;
                QBtn_UserAdd.Enabled = false;
                QBtn_SaveUser.Enabled = false;
                UserdataGridView.Enabled = false;
            }

            UserdataGridView.Columns.Add(PasswordColumn);

            DataGridViewTextBoxColumn LevelColumn = new DataGridViewTextBoxColumn();
            if (GlobalNew.Weblogin != "1")
            {
                LevelColumn.DataPropertyName = "Level"; // 屬性名稱要與 User 類別中的一致
                LevelColumn.HeaderText = "Level";
            }
            else
            {
                LevelColumn.DataPropertyName = "Permission"; // 屬性名稱要與 User 類別中的一致
                LevelColumn.HeaderText = "Permission";
            }
            LevelColumn.HeaderCell.Style.BackColor = Color.Blue;
            UserdataGridView.Columns.Add(LevelColumn);

            if (GlobalNew.users != null)
            {
                if (GlobalNew.users.Count > 0)
                {

                    //只顯示同等級或以下的使用者
                    if (GlobalNew.Weblogin != "1")
                    {
                        var filteredUsers = GlobalNew.users
                        .Where(user => user.Level <= Global_Memory.UserLevel)
                        .ToList();
                        UserdataGridView.DataSource = new BindingList<User>(filteredUsers);
                    }
                    else
                    {
                        if (GlobalNew.UserLevel != 5)
                        {
                            UserdataGridView.Enabled = false;
                            QBtn_DeleteUser.Enabled = false;
                            QBtn_UserAdd.Enabled = false;
                            QBtn_SaveUser.Enabled = false;
                        }
                        else
                        {
                            var filteredUsers = GlobalNew.users.ToList();
                            UserdataGridView.DataSource = new BindingList<User>(filteredUsers);
                        }
                    }
                }
            }
        }
        public RecipeManagement(string FileName)
        {
            //MainForm.test.StepTest();
            InitializeComponent();
            InitRightPanelLayout();
            InitRecipeProRightPanelLayout();
            DeviceLOGGER = new MLog(richTextBox_DeviceListSetting);
            scRecipeTB.SplitterDistance = (int)(scRecipeTB.ClientSize.Height * 0.72);
            scDevicesTB.SplitterDistance = (int)(scDevicesTB.ClientSize.Height * 0.7);


            jsonDevicePath = $@"{System.Environment.CurrentDirectory}\Config\Recipe\{FileName}.json";
            jsonRecipePath = $@"{System.Environment.CurrentDirectory}\Config\Recipe\{FileName}.json";

            // 讀取 JSON 檔案的內容
            start_jsonData = File.ReadAllText(jsonRecipePath);
            //if (File.Exists(jsonRecipePath) == false)
            //{
            //    MessageBox.Show("The specified file path does not exist.");
            //    return;
            //}

            IsEditing = false;
            proTreeView_Devices.SetMode(FlowMode.HW_Mode);
            proTreeView_Devices.AddImage("camera.png", imageList1.Images["camera.png"]);
            proTreeView_Devices.AddImage("device.png", imageList1.Images["device.png"]);
            proTreeView_Devices.AddImage("up-and-down.png", imageList1.Images["up-and-down.png"]);
            Manufacture.Initialize.FindDerived(CCDList_CB, typeof(CCDBase));
            Manufacture.Initialize.FindDerived(MotorList_CB, typeof(MotionBase));
            Manufacture.Initialize.FindDerived(ControlList_CB, typeof(ControlDeviceBase));
            Manufacture.Initialize.FindDerived(IOList_CB, typeof(IOBase));
            Manufacture.Initialize.FindDerived(VISAList_CB, typeof(VisaBase));
            Manufacture.Initialize.FindDerived(DUTList_CB, typeof(DUT_BASE));
            Manufacture.Initialize.FindDerived(TeachList_CB, typeof(TeachBase));
            Manufacture.Initialize.FindDerived(ImageList_CB, typeof(Image_Base));
            string retdev = proTreeView_Devices.Read_DeviceList(jsonDevicePath);
            if (!retdev.Contains("success"))
            {
                isReadScriptSuccess = false;
            }
            else
                isReadScriptSuccess = true;
            //Uninit目前主流程中的裝置列表

            proTreeView_RecipeProcess.SetMode(FlowMode.Mode3);
            SeqNode_CB.Items.Add((typeof(Manufacture.SeqNode)).Name);
            SeqItem_CB.Items.Add((typeof(Manufacture.SeqItem)).Name);
            string jsonPath = $@"{System.Environment.CurrentDirectory}\Config\{GlobalNew.STATIONNAME}.json";
            //proTreeView_RecipeProcess.Read_ScriptRecipe(jsonPath);
            SN_TEXT2.Enabled = false;

            //==================================
            proTreeView_RecipePro.SetMode(FlowMode.Process_Mode);
            proTreeView_RecipePro.AddImage("camera.png", imageList1.Images["camera.png"]);
            proTreeView_RecipePro.AddImage("device.png", imageList1.Images["device.png"]);
            proTreeView_RecipePro.AddImage("up-and-down.png", imageList1.Images["up-and-down.png"]);
            proTreeView_RecipePro.AddImage("workflow.png", imageList1.Images["workflow.png"]);
            Manufacture.Initialize.FindDerived(comboBox_Sequences, typeof(Script_Container_Base));
            Manufacture.Initialize.FindDerived(comboBox_IOScript, typeof(ScriptIOBase));
            Manufacture.Initialize.FindDerived(comboBox_MotorScript, typeof(Script_1Mot1ComBase));
            Manufacture.Initialize.FindDerived(comboBox_MotorScript, typeof(Script_1MotBase));
            Manufacture.Initialize.FindDerived(comboBox_ExtraScript, typeof(Script_Extra_Base));
            Manufacture.Initialize.FindDerived(comboBox_DUTScript, typeof(ScriptDUTBase));
            Manufacture.Initialize.FindDerived(comboBox_CCDScript, typeof(Script_CCD_Base));
            Manufacture.Initialize.FindDerived(comboBox_CtrlScript, typeof(Script_ControlDevice_Base));
            Manufacture.Initialize.FindDerived(comboBox_ImageScript, typeof(Script_Image_Base));
            proTreeView_RecipePro.MouseDoubleClickEvent += EventHandlerMethod;
            proTreeView_RecipePro.MouseSelectClickEvent += EventSelectMethod;
            proTreeView_RecipePro.ProcessNodeMouseClick += MainProTreeNodeMouseClick;
            proTreeView_RecipePro.TreeViewKeyDownEvent += EventHandlerTreeKeyMethod;
            proTreeView_RecipePro.PropertyChangedMouseClick += EventHandlerPropertyGridMethod;
            proTreeView_Devices.MouseDoubleClickEvent += DeviceEventHandlerMethod;
            proTreeView_Devices.ProcessNodeMouseClick += DeviceMouseClickMethod;

            // 訂閱 DLL 中的事件
            //proTreeView_RecipePro.TreeViewKeyDownEvent += MyDLLTreeView_KeyDown;
            string sret = proTreeView_RecipePro.Read_Recipe(jsonRecipePath);
            if (!sret.Contains("success"))
            {
                qBtn_RecipeProSave.Enabled = false;
                QBtn_Save.Enabled = false;
                isReadScriptSuccess &= false;
            }
            else
                isReadScriptSuccess &= true;
            label2.Text = Path.GetFileName(jsonDevicePath);

            if (proTreeView_Devices != null)
            {
                if (proTreeView_Devices.GetTreeview().Nodes.Count == 0)
                {
                    TreeNode MESDeviceNode = CreateNodeWithObject("AutoTestSystem.Equipment.ControlDevice.HTTPMESCommand", "HTTPMESCommand", "device.png", true, obj =>
                    {
                        ((HTTPMESCommand)obj).URL = "http://172.16.1.43:8000/api/mes@1/transfer";
                    });
                    proTreeView_Devices.AddtoTree(MESDeviceNode);
                }
            }

            if (proTreeView_RecipePro != null)
            {
                if (proTreeView_RecipePro.GetTreeview().Nodes.Count == 0)
                {

                    // Create the main thread node
                    TreeNode mainThreadNode = CreateNodeWithObject("AutoTestSystem.Script.Container_MainThread", "MainThread", "workflow.png", true);

                    // Create sub-nodes using the helper function
                    TreeNode sequencesNode = CreateNodeWithObject("AutoTestSystem.Script.Container_Sequences", "TestProcess", "closebox.png", true);
                    //TreeNode snMesNode = CreateNodeWithObject("AutoTestSystem.Script.Container_Sequences", "MES_SN_CHECK", "closebox.png", true);
                    //TreeNode uploadMesNode = CreateNodeWithObject("AutoTestSystem.Script.Container_Sequences", "MES_DATA_UPLOAD", "closebox.png", true);
                    TreeNode postNode = CreateNodeWithObject("AutoTestSystem.Script.Container_Post_Process", "PostProcess", "execute.png", true);
                    TreeNode initNode = CreateNodeWithObject("AutoTestSystem.Script.Container_JIG_INIT", "HomeProcess", "tool-box.png", true);
                    ((Container_Sequences)sequencesNode.Tag).FailJump = $"PostProcess({((Container_Post_Process)postNode.Tag).ID})";

                    //TreeNode MESSNNode = CreateNodeWithObject("AutoTestSystem.Script.Script_HTTPMESCmd_Pro", "C002", "mainboard.png", true, obj =>
                    //{
                    //    ((Script_HTTPMESCmd_Pro)obj).Data = "%WorkID%;%ProductSN%;";
                    //    ((Script_HTTPMESCmd_Pro)obj).CheckStr = "OK";
                    //    ((Script_HTTPMESCmd_Pro)obj).MESCmd = "C002";
                    //    ((Script_HTTPMESCmd_Pro)obj).DeviceSel = "HTTPMESCommand";
                    //});
                    //TreeNode MESResultNode = CreateNodeWithObject("AutoTestSystem.Script.Script_HTTPMESCmd_Pro", "C003", "mainboard.png", true, obj =>
                    //{
                    //    ((Script_HTTPMESCmd_Pro)obj).Data = "%WorkID%;%ProductSN%;";
                    //    ((Script_HTTPMESCmd_Pro)obj).CheckStr = "OK";
                    //    ((Script_HTTPMESCmd_Pro)obj).MESCmd = "C003";
                    //    ((Script_HTTPMESCmd_Pro)obj).DeviceSel = "HTTPMESCommand";
                    //});
                    //TreeNode MESDataNode = CreateNodeWithObject("AutoTestSystem.Script.Script_HTTPMESCmd_Pro", "C004", "mainboard.png", true, obj =>
                    //{
                    //    ((Script_HTTPMESCmd_Pro)obj).Data = "%WorkID%;%ProductSN%;";
                    //    ((Script_HTTPMESCmd_Pro)obj).CheckStr = "OK";
                    //    ((Script_HTTPMESCmd_Pro)obj).MESCmd = "C004";
                    //    ((Script_HTTPMESCmd_Pro)obj).DeviceSel = "HTTPMESCommand";
                    //});


                    //snMesNode.Nodes.Add(MESSNNode);
                    //uploadMesNode.Nodes.Add(MESResultNode);
                    //uploadMesNode.Nodes.Add(MESDataNode);
                    //// Assemble nodes
                    //sequencesNode.Nodes.Add(snMesNode);
                    //sequencesNode.Nodes.Add(uploadMesNode);

                    mainThreadNode.Nodes.Add(sequencesNode);
                    mainThreadNode.Nodes.Add(postNode);
                    mainThreadNode.Nodes.Add(initNode);

                    // Expand all nodes and add to the TreeView
                    mainThreadNode.ExpandAll();
                    proTreeView_RecipePro.AddMainThread(mainThreadNode);

                }

            }

            //UnInitDevices();

            ////Init目前設定中的裝置列表並更新到主流程的裝置列表
            //Thread.Sleep(50);



            label_RecipName.Text = Path.GetFileName(jsonPath);


            proTreeView_RecipeProcess.MouseDoubleClickEvent += EventHandlerMethod;


            // 初始化使用者列表
            //userList = new List<User>();

            // 設置 DataGridView 屬性
            UserdataGridView.AutoGenerateColumns = false;
            UserdataGridView.AllowUserToAddRows = false;
            UserdataGridView.RowTemplate.Height = 18;
            UserdataGridView.RowHeadersWidth = 25;
            UserdataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.Blue;
            UserdataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold); // 設定列標題的字體、大小和樣式;

            DataGridViewTextBoxColumn usernameColumn = new DataGridViewTextBoxColumn();
            usernameColumn.DataPropertyName = "Username"; // 屬性名稱要與 User 類別中的一致
            usernameColumn.HeaderText = "Username";
            usernameColumn.HeaderCell.Style.BackColor = Color.Blue;
            UserdataGridView.Columns.Add(usernameColumn);

            DataGridViewTextBoxColumn PasswordColumn = new DataGridViewTextBoxColumn();
            if (GlobalNew.Weblogin != "1")
            {
                PasswordColumn.DataPropertyName = "Password"; // 屬性名稱要與 User 類別中的一致
                PasswordColumn.HeaderText = "Password";
            }
            else
            {
                PasswordColumn.DataPropertyName = "Email"; // 屬性名稱要與 User 類別中的一致
                PasswordColumn.HeaderText = "Email";
            }
            PasswordColumn.HeaderCell.Style.BackColor = Color.Blue;
            if (Global_Memory.UserLevel >= (int)User_Level.ADMIN)
                PasswordColumn.Visible = true; // 設置為顯示
            else
            {
                PasswordColumn.Visible = false; // 設置為不顯示
                QBtn_DeleteUser.Enabled = false;
                QBtn_UserAdd.Enabled = false;
                QBtn_SaveUser.Enabled = false;
                UserdataGridView.Enabled = false;
            }
            if (Global_Memory.UserLevel >= (int)User_Level.RD)
            {
                QBtn_Save.Enabled = true;
                qBtn_RecipeProSave.Enabled = true;
            }
            else
            {
                QBtn_Save.Enabled = false;
                qBtn_RecipeProSave.Enabled = false;
            }
            if (Global_Memory.UserLevel >= (int)User_Level.PE)
            {
                QBtn_Save.Enabled = true;
            }
            UserdataGridView.Columns.Add(PasswordColumn);

            DataGridViewTextBoxColumn LevelColumn = new DataGridViewTextBoxColumn();
            if (GlobalNew.Weblogin != "1")
            {
                LevelColumn.DataPropertyName = "Level"; // 屬性名稱要與 User 類別中的一致
                LevelColumn.HeaderText = "Level";
            }
            else
            {
                LevelColumn.DataPropertyName = "Permission"; // 屬性名稱要與 User 類別中的一致
                LevelColumn.HeaderText = "Permission";
            }
            LevelColumn.HeaderCell.Style.BackColor = Color.Blue;
            UserdataGridView.Columns.Add(LevelColumn);

            if (GlobalNew.users != null)
            {
                if (GlobalNew.users.Count > 0)
                {

                    //只顯示同等級或以下的使用者
                    if (GlobalNew.Weblogin != "1")
                    {
                        var filteredUsers = GlobalNew.users
                        .Where(user => user.Level <= Global_Memory.UserLevel)
                        .ToList();
                        UserdataGridView.DataSource = new BindingList<User>(filteredUsers);
                    }
                    else
                    {
                        if (GlobalNew.UserLevel != 5)
                        {
                            UserdataGridView.Enabled = false;
                            QBtn_DeleteUser.Enabled = false;
                            QBtn_UserAdd.Enabled = false;
                            QBtn_SaveUser.Enabled = false;
                        }
                        else
                        {
                            var filteredUsers = GlobalNew.users.ToList();
                            UserdataGridView.DataSource = new BindingList<User>(filteredUsers);
                        }
                    }
                }
            }
        }

        private void InitRightPanelLayout()
        {
            // 建立垂直排列用 TableLayoutPanel
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                ColumnCount = 1,
                RowCount = 0
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            splitContainer_DevList_Level1.Panel2.Controls.Clear();
            splitContainer_DevList_Level1.Panel2.Controls.Add(tlp);

            // 要排列的 GroupBox
            GroupBox[] groups = new[]
            {
        CCD_groupBox,
        Motor_groupBox,
        ControlDevices_groupBox,
        IOs_groupBox,
        DUT_groupBox,
        VISA_groupBox,
        TEACH_groupBox,
        Image_groupBox
    };

            foreach (var g in groups)
            {
                // === 將 GroupBox 內部 ComboBox+Button 組改成 TableLayoutPanel ===
                ConvertGroupBoxLayout(g);

                // === Dock/Anchor 排列 GroupBox ===
                g.Dock = DockStyle.Top;
                g.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                tlp.Controls.Add(g);
                tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlp.RowCount++;
            }

            // 最底下 Save 按鈕
            QBtn_Save.Dock = DockStyle.Top;
            QBtn_Save.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tlp.Controls.Add(QBtn_Save);
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlp.RowCount++;
        }

        /// <summary>
        /// 把 GroupBox 裡「一個 ComboBox + 一個 Button」轉成 TableLayout 佈局，讓 ComboBox 寬度會自動跟隨變化
        /// </summary>
        private void ConvertGroupBoxLayout(GroupBox gb)
        {
            // 若裡面不是剛好兩個控制項，不處理
            if (gb.Controls.Count != 2) return;
            var cb = gb.Controls.OfType<ComboBox>().FirstOrDefault();
            var btn = gb.Controls.OfType<Button>().FirstOrDefault();
            if (cb == null || btn == null) return;

            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
            };
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            gb.Controls.Clear();
            cb.Dock = DockStyle.Fill;
            btn.AutoSize = true;
            inner.Controls.Add(cb, 0, 0);
            inner.Controls.Add(btn, 1, 0);
            gb.Controls.Add(inner);
        }
        /// <summary>
        /// RecipeProPage → 將 panelProRecipeItemPanel 內的 GroupBox
        /// 自動排列並讓內部 ComboBox+Button 支援縮放
        /// </summary>
        /// <summary>
        /// RecipeProPage → 將 panelProRecipeItemPanel 內的 GroupBox 自動排列；
        /// 下方按鈕（Init/Save/Export）與 Checkbox（ShowTip/HideBar）放在底部 GroupBox+TableLayoutPanel
        /// </summary>
        /// <summary>
        /// RecipeProPage → 自動排列 Script GroupBox 以及底端操作按鈕＋Checkbox
        /// </summary>
        private void InitRecipeProRightPanelLayout()
        {
            panelProRecipeItemPanel.Controls.Clear();

            // ========== 下方按鈕+checkbox（TableLayoutPanel）==========
            var btnTlp = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(10),
                AutoSize = true
            };
            btnTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.0f));
            btnTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.0f));

            //btnTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            //btnTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            //btnTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            btnTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // row0 buttons
            btnTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // row1 checkbox

            qBtn_SingleRun.Dock = DockStyle.Fill;
            qBtn_RecipeProSave.Dock = DockStyle.Fill;
            //qBtn_RecipeProExport.Dock = DockStyle.Fill;

            btnTlp.Controls.Add(qBtn_SingleRun, 0, 0);
            btnTlp.Controls.Add(qBtn_RecipeProSave, 1, 0);
            //btnTlp.Controls.Add(qBtn_RecipeProExport, 2, 0);

            checkBox_showtip.Visible = true;
            checkBoxHideBar.Visible = true;
            btnTlp.Controls.Add(checkBox_showtip, 0, 1);
            btnTlp.Controls.Add(checkBoxHideBar, 1, 1);

            // ========== 上半 Script GroupBox (scroll) ==========
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                ColumnCount = 1,
                RowCount = 0
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            GroupBox[] groups =
            {
        groupBox_RecipePro,
        groupBox_IOScript,
        groupBox_MotionScript,
        groupBox_ExtraScript,
        groupBox_DUTScript,
        groupBox_CCDScript,
        groupBox_CtrlScript,
        groupBox_ImageScript
    };

            foreach (var g in groups)
            {
                ConvertGroupBoxLayout(g);
                g.Dock = DockStyle.Top;
                g.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                tlp.Controls.Add(g);
                tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlp.RowCount++;
            }

            // ⭐ 加入順序要：先 tlp(填滿)，再 btnTlp(top)
            panelProRecipeItemPanel.Controls.Add(tlp);
            panelProRecipeItemPanel.Controls.Add(btnTlp);
        }
        public TreeNode CreateNodeWithObject(string objectType, string description, string iconKey, bool isChecked, Action<Manufacture.CoreBase> additionalSetup = null)
        {
            // Create object
            Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, objectType);
            obj.Description = description;
            obj.Enable = true;
            obj.NodeIcon = iconKey;

            // Apply additional setup if provided
            additionalSetup?.Invoke(obj);

            // Create and return the TreeNode
            TreeNode node = new TreeNode
            {
                Tag = obj,
                Text = obj.Description,
                ImageKey = iconKey,
                SelectedImageKey = iconKey,
                Checked = isChecked
            };

            return node;
        }

        public bool GetReadRecipeStatus()
        {
            return isReadScriptSuccess;
        }
        private void EventSelectMethod(object sender, EventArgs e)
        {
            richTextBox_journal.Clear();
            // 取得 TVProcess.SelectedNode
            TreeNode clickedNode = ((TreeNode)sender);
            string accumulatedJournal = string.Empty;
            TraverseNodesJournal(clickedNode, ref accumulatedJournal);
            richTextBox_journal.AppendText(accumulatedJournal);
        }
        private void TraverseNodesJournal(TreeNode node, ref string JournalLog)
        {
            ProcessNode(node, ref JournalLog);

            foreach (TreeNode childNode in node.Nodes)
            {
                TraverseNodesJournal(childNode, ref JournalLog);
            }
        }

        private void ProcessNode(TreeNode node, ref string JournalLog)
        {
            if (node.Tag != null && node.Checked && node.Tag is ScriptBase)
            {
                ScriptBase tagObject = (ScriptBase)node.Tag;

                if (!string.IsNullOrEmpty(tagObject.Journal))
                {
                    try
                    {
                        // 解析 JSON 並轉換為可讀格式
                        var logs = JsonConvert.DeserializeObject<List<LogEntry>>(tagObject.Journal);
                        foreach (var log in logs)
                        {
                            JournalLog += $"{log.Date}({tagObject.Description})\n-{log.Log}\tby ({log.User})\n\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        // 處理例外
                    }
                }
            }
        }

        private async void EventHandlerMethod(object sender, EventArgs e)
        {
            TreeNode clickedNode = ((TreeNode)sender);
            DialogResult result = MessageBox.Show(
            $"確認是否執行 {clickedNode.Text} 的單步動作?",
            "確認執行動作",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
            );

            // 根據使用者的選擇執行相應的動作
            if (result == DialogResult.Yes)
            {
                GlobalNew.g_recipesteprun = true;
                GlobalNew.g_shouldStop = false;
                if (clickedNode.Tag is Manufacture.SeqItem)
                {
                    //複製參數到ItemsNew物件
                    var config = new MapperConfiguration(cfg => cfg.CreateMap<Manufacture.SeqItem, ItemsNew>());
                    var mapper = new Mapper(config);
                    ItemsNew tempItems = mapper.Map<ItemsNew>(clickedNode.Tag as Manufacture.SeqItem);

                    tempItems.ComdSend = tempItems.ComdSend.Replace("<SN>", MainForm.SN);
                    tempItems.ComdSend = tempItems.ComdSend.Replace("<TIME>", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                    tempItems.ComdSend = tempItems.ComdSend.Replace("<MesMac>", MainForm.MesMac);
                    tempItems.ComdSend = tempItems.ComdSend.Replace("<MesMBSN>", MainForm.MesMBSN);
                    tempItems.ComdSend = tempItems.ComdSend.Replace("<MesSW>", MainForm.MesSW);
                    tempItems.ComdSend = tempItems.ComdSend.Replace("<MesHW>", MainForm.MesHW);
                    tempItems.ComdSend = tempItems.ComdSend.Replace("<File path>", MainForm.SN);
                    tempItems.ComdSend = tempItems.ComdSend.Replace("<Share path>", GlobalNew.SharePath);

                    BlockingForm blockingForm = new BlockingForm();

                    // 顯示 BlockingForm
                    blockingForm.Show();
                    clickedNode.ForeColor = Color.Gray;
                    try
                    {
                        //DUT_BASE tempDUT = new DUT_BASE();
                        // 非同步執行 StepTest
                        List<string> stepTestResult = await Task.Run(() => MainForm.test.StepTest(tempItems, GlobalNew.Devices, 10));

                        // 顯示 MessageBox
                        //MessageBox.Show($"Result:{stepTestResult[0].ToUpper()}\nValue:{stepTestResult[1]}");
                        proTreeView_RecipeProcess.RecipeLogMessage($"{tempItems.ItemName}\nResult:{stepTestResult[0].ToUpper()}\nValue:{stepTestResult[1]}");
                        // 關閉 BlockingForm，這裡使用 BeginInvoke 以確保在 UI 線程上執行
                        blockingForm.BeginInvoke(new Action(() => blockingForm.Close()));

                    }
                    catch (Exception ex)
                    {
                        // 處理異常
                        MessageBox.Show($"發生錯誤: {ex.Message}");

                        // 關閉 BlockingForm，這裡使用 BeginInvoke 以確保在 UI 線程上執行
                        blockingForm.BeginInvoke(new Action(() => blockingForm.Close()));
                    }
                    clickedNode.ForeColor = Color.Black;
                }
                else if (clickedNode.Tag is Manufacture.SeqNode)
                {
                    Manufacture.SeqNode Seq = (Manufacture.SeqNode)clickedNode.Tag;
                    BlockingForm blockingForm = new BlockingForm();
                    blockingForm.Show();
                    clickedNode.ForeColor = Color.Gray;
                    for (int i = 0; i < clickedNode.Nodes.Count; i++)
                    {
                        //複製參數到ItemsNew物件
                        var config = new MapperConfiguration(cfg => cfg.CreateMap<Manufacture.SeqItem, ItemsNew>());
                        var mapper = new Mapper(config);
                        ItemsNew tempItems = mapper.Map<ItemsNew>(clickedNode.Nodes[i].Tag as Manufacture.SeqItem);

                        tempItems.ComdSend = tempItems.ComdSend.Replace("<SN>", MainForm.SN);
                        tempItems.ComdSend = tempItems.ComdSend.Replace("<TIME>", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                        tempItems.ComdSend = tempItems.ComdSend.Replace("<MesMac>", MainForm.MesMac);
                        tempItems.ComdSend = tempItems.ComdSend.Replace("<MesMBSN>", MainForm.MesMBSN);
                        tempItems.ComdSend = tempItems.ComdSend.Replace("<MesSW>", MainForm.MesSW);
                        tempItems.ComdSend = tempItems.ComdSend.Replace("<MesHW>", MainForm.MesHW);
                        tempItems.ComdSend = tempItems.ComdSend.Replace("<File path>", MainForm.SN);
                        tempItems.ComdSend = tempItems.ComdSend.Replace("<Share path>", GlobalNew.SharePath);

                        //BlockingForm blockingForm = new BlockingForm();

                        //// 顯示 BlockingForm
                        //blockingForm.Show();
                        clickedNode.Nodes[i].ForeColor = Color.Gray;
                        try
                        {
                            // 非同步執行 StepTest
                            List<string> stepTestResult = await Task.Run(() => MainForm.test.StepTest(tempItems, GlobalNew.Devices, 10));

                            // 顯示 MessageBox
                            //MessageBox.Show($"Result:{stepTestResult[0].ToUpper()}\nValue:{stepTestResult[1]}");
                            proTreeView_RecipeProcess.RecipeLogMessage($"{tempItems.ItemName}\nResult:{stepTestResult[0].ToUpper()}\nValue:{stepTestResult[1]}");

                            // 關閉 BlockingForm，這裡使用 BeginInvoke 以確保在 UI 線程上執行
                            //blockingForm.BeginInvoke(new Action(() => blockingForm.Close()));

                        }
                        catch (Exception ex)
                        {
                            // 處理異常
                            MessageBox.Show($"發生錯誤: {ex.Message}");

                            // 關閉 BlockingForm，這裡使用 BeginInvoke 以確保在 UI 線程上執行
                            //blockingForm.BeginInvoke(new Action(() => blockingForm.Close()));
                        }
                        clickedNode.Nodes[i].ForeColor = Color.Black;
                    }
                    clickedNode.ForeColor = Color.Black;
                    blockingForm.BeginInvoke(new Action(() => blockingForm.Close()));
                }
                else if (clickedNode.Tag is ScriptBase)
                {
                    BlockingForm blockingForm = new BlockingForm();
                    blockingForm.Show();
                    clickedNode.BackColor = Color.LightGray;
                    try
                    {
                        //DUT_Simu dd = new DUT_Simu();
                        DUT_BASE tempDUT = null;
                        foreach (var value in Devices.Values)
                        {
                            if (value is DUT_BASE)
                            {
                                if (((DUT_BASE)value).Enable == false)
                                    continue;
                                tempDUT = (DUT_BASE)value;

                                break;
                            }

                        }

                        if (tempDUT == null)
                        {
                            MessageBox.Show(
                                "請至少加入一個DUT裝置再運行單步執行.\nPlease add at least one DUT handle device before executing a single step.",
                                "Warning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                            blockingForm.BeginInvoke(new Action(() => blockingForm.Close()));
                            return;
                        }

                        //tempDUT.DUTLogger = Log4NetHelper.GetLogger(this.GetType(), richTextBox_RecipeProLog);
                        //單步視為重新測試需清空數據以防前次跳轉功能資訊導致判定異常
                        tempDUT.DataCollection.Clear();
                        tempDUT.isSimu = true;
                        ScriptBase scriptItem = (ScriptBase)clickedNode.Tag;
                        scriptItem.RowDataItem.Clear();
                        scriptItem.SetDataDestination(tempDUT);
                        tempDUT.SetConfig_Param();
                        if(tempDUT.LOGGER == null)
                        tempDUT.LOGGER = new MLog(richTextBox_RecipeProLog);
                        bool ret = await Task.Run(() =>
                        {
                            if (GlobalNew.FormMode == "1")
                            {
                                SetMLoggerThread(tempDUT.LOGGER);
                                Bd.SetLoggerForCurrentThread(tempDUT.LOGGER);
                            }

                            return scriptItem.Action(Devices);
                        });
                        if (ret == true)
                        {
                            clickedNode.ForeColor = Color.Green;
                            clickedNode.BackColor = Color.White;
                        }
                        else
                        {
                            clickedNode.ForeColor = Color.Red;
                            clickedNode.BackColor = Color.White;
                        }
                    }
                    catch (Exception ex)
                    {
                        clickedNode.ForeColor = Color.Red;
                        clickedNode.BackColor = Color.White;
                        MessageBox.Show($"{ex.Message}");
                    }

                    clickedNode.BackColor = Color.White;
                    try
                    {
                        blockingForm.BeginInvoke(new Action(() => blockingForm.Close()));
                    }
                    catch(Exception)
                    {
                        
                    }

                }
                else if (clickedNode.Tag is Manufacture.ContainerNode)
                {
                    BlockingForm blockingForm = new BlockingForm();
                    blockingForm.Show();
                    clickedNode.BackColor = Color.LightGray;
                    try
                    {
                        DUT_BASE tempDUT = null;
                        foreach (var value in Devices.Values)
                        {
                            if (value is DUT_BASE)
                            {
                                if (((DUT_BASE)value).Enable == false)
                                    continue;
                                tempDUT = (DUT_BASE)value;

                                break;
                            }
                        }

                        if (tempDUT == null)
                        {
                            MessageBox.Show(
                                "請至少加入一個DUT裝置再運行單步執行.\nPlease add at least one DUT handle device before executing a single step.",
                                "Warning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                            blockingForm.BeginInvoke(new Action(() => blockingForm.Close()));
                            return;
                        }

                        tempDUT.DataCollection.Clear();
                        tempDUT.isSimu = true;
                        //tempDUT.LOGGER = new MLog(richTextBox_RecipeProLog);
                        ScriptContainer scriptContainer = (ScriptContainer)clickedNode.Tag;
                        bool TestResult = false;
                        tempDUT.SetConfig_Param();
                        object[] context = new object[] { proTreeView_RecipePro, false, tempDUT, TestResult, Devices };
                        if (tempDUT.LOGGER == null)
                            tempDUT.LOGGER = new MLog(richTextBox_RecipeProLog);
                        //int ret = await Task.Run(() => ScriptContainer.Run(clickedNode, context));
                        int ret = await Task.Run(() =>
                        {
                            if (GlobalNew.FormMode == "1")
                            {
                                SetMLoggerThread(tempDUT.LOGGER);
                                Bd.SetLoggerForCurrentThread(tempDUT.LOGGER);
                            }
                            proTreeView_RecipeProcess.RecipeLogMessage($"Single Step Run {scriptContainer.Description}");
                            return scriptContainer.Process(clickedNode, context);
                        });

                        if (ret == 1)
                        {
                            clickedNode.ForeColor = Color.Green;
                            clickedNode.BackColor = Color.White;
                        }
                        else
                        {
                            clickedNode.ForeColor = Color.Red;
                            clickedNode.BackColor = Color.White;
                        }
                    }
                    catch (Exception ex)
                    {
                        clickedNode.ForeColor = Color.Red;
                        clickedNode.BackColor = Color.White;
                        MessageBox.Show($"{ex.Message}");
                    }

                    clickedNode.BackColor = Color.White;
                    
                    try
                    {
                        blockingForm.BeginInvoke(new Action(() => blockingForm.Close()));
                    }
                    catch (Exception)
                    {
                        
                    }
                    //Container_Sequences Container = (Container_Sequences)clickedNode.Tag;
                    //for (int m = 0; m < ((Container_Sequences)clickedNode.Tag).RetryTimes; m++)
                    //{
                    //    if (ProTV.InvokeRequired)
                    //    {
                    //        ProTV.BeginInvoke(new Action(() =>
                    //        {
                    //            ((ScriptContainer)treeNode.Nodes[i].Tag).toolTip.ShowAlways = true;
                    //            ((ScriptContainer)treeNode.Nodes[i].Tag).toolTip.Show($"{m + 1}", ProTV, treeNode.Nodes[i].Bounds.Right, treeNode.Nodes[i].Bounds.Top + 5);
                    //        }));
                    //    }
                    //    else
                    //    {
                    //        ((ScriptContainer)clickedNode.Tag).toolTip.ShowAlways = true;
                    //        ((ScriptContainer)clickedNode.Tag).toolTip.Show($"{m + 1}", ProTV, clickedNode.Bounds.Right, clickedNode.Bounds.Top + 5);
                    //    }
                    //    ChangeColorRecursive(clickedNode, Color.White, Color.Black);
                    //    ret = ((ScriptContainer)clickedNode.Tag).Process(clickedNode, Component);
                    //    if (ret == 1)
                    //    {
                    //        status = true;
                    //        m = ((Container_Sequences)treeNode.Nodes[i].Tag).RetryTimes;
                    //    }

                    //}
                }

                GlobalNew.g_recipesteprun = false;
            }
        }

        private void RoundCorners(Control control, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(control.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(control.Width - radius, control.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, control.Height - radius, radius, radius, 90, 90);

            control.Region = new Region(path);
        }
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private const int SC_MOVE = 0xF010;
        private const int WM_SYSCOMMAND = 0x0112;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        private void RecipeManagement_Load(object sender, EventArgs e)
        {
            //RoundCorners(this, 50);
            scRecipeTB.SplitterDistance = (int)(scRecipeTB.Height * 0.72);
            splitContainer_RecipePro.SplitterDistance = (int)(splitContainer_RecipePro.Width * 0.71);
            splitContainer_DevList_Level1.SplitterDistance = (int)(splitContainer_DevList_Level1.ClientSize.Width * 0.75);
            splitContainer_DeviceListLevel1.SplitterDistance = (int)(splitContainer_DeviceListLevel1.Width * 0.07);
            originalSize = this.ClientSize;

            originalLocation = this.Location;
            isMaximized = true;

            ManagementTabControl.SelectedIndex = 3;
            panel_Focus.Height = RecipeBtn.Height;
            panel_Focus.Top = RecipeBtn.Top;
        }
        private void DeviceEventHandlerMethod(object sender, EventArgs e)
        {
            TreeNode clickedNode = ((TreeNode)sender);
            DialogResult result = MessageBox.Show(
            $"確認是否執行 {clickedNode.Text} 的單步動作?",
            "確認執行動作",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
            );

            // 根據使用者的選擇執行相應的動作
            if (result == DialogResult.Yes)
            {
                if (clickedNode.Tag is TerminalNode)
                {
                    try
                    {
                        TerminalNode tempDUT = (TerminalNode)clickedNode.Tag;

                        tempDUT.Show();
                        proTreeView_Devices.NodePropertyGrid.SelectedObject = clickedNode.Tag;
                    }

                    catch (Exception ex)
                    {

                        MessageBox.Show($"{ex.Message}");
                    }


                }


            }
        }

        private void DevicesBtn_Click(object sender, EventArgs e)
        {
            ManagementTabControl.SelectedIndex = 0;
            panel_Focus.Height = DevicesBtn.Height;
            panel_Focus.Top = DevicesBtn.Top;
        }


        private void MainThreadBtn_Click(object sender, EventArgs e)
        {
            Manufacture.CoreBase obj = Manufacture.Initialize.CreatMaster();
            TreeNode Node = new TreeNode();
            Node.Tag = obj;
            Node.Text = obj.Description;
            proTreeView_Devices.AddtoTree(Node);
        }

        private void Motor_AddBtn_Click(object sender, EventArgs e)
        {
            if (MotorList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Equipment.Motion." + MotorList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;

                //Logger.Warn("Image save in root DIR");
                proTreeView_Devices.AddtoTree(Node);
            }
        }

        private void CCD_AddBtn_Click(object sender, EventArgs e)
        {
            if (CCDList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Equipment.CCD." + CCDList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;
                obj.NodeIcon = "camera.png";
                proTreeView_Devices.AddtoTree(Node);
            }
        }

        private void Control_AddBtn_Click(object sender, EventArgs e)
        {
            if (ControlList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Equipment.ControlDevice." + ControlList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;
                obj.NodeIcon = "device.png";
                //Logger.Warn("Image save in root DIR");
                proTreeView_Devices.AddtoTree(Node);
            }
        }

        private void IO_AddBtn_Click(object sender, EventArgs e)
        {
            if (IOList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Equipment.IO." + IOList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;
                obj.NodeIcon = "up-and-down.png";
                //Logger.Warn("Image save in root DIR");
                proTreeView_Devices.AddtoTree(Node);
            }
        }
        private void DUT_AddBtn_Click(object sender, EventArgs e)
        {
            if (DUTList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.DUT." + DUTList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;
                obj.NodeIcon = "device.png";
                //Logger.Warn("Image save in root DIR");
                proTreeView_Devices.AddtoTree(Node);
            }
        }


        private void VISA_AddBtn_Click(object sender, EventArgs e)
        {
            if (VISAList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Equipment.VISA." + VISAList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;
                obj.NodeIcon = "TCP_Visa.png";
                //Logger.Warn("Image save in root DIR");
                proTreeView_Devices.AddtoTree(Node);
            }
        }

        public static bool StaticUnInitDevices(Dictionary<string, object> devs)
        {
            try
            {
                foreach (var value in devs.Values)
                {
                    bool result = ((dynamic)value).UnInit();
                    if (value is DUT_BASE)
                    {
                        ((dynamic)value).DataGridView.Columns.Clear();
                        ((dynamic)value).DataGridView.Visible = false;
                        Control parentControl = ((dynamic)value).DataGridView.Parent;
                        if (parentControl != null)
                            parentControl.Controls.Remove(((dynamic)value).DataGridView);

                        ((dynamic)value).DataCollection.Clear();
                    }
                }

                devs.Clear();
                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred during device un-initialization: {ex.Message}");
                return false;
            }
            //try
            //{
            //    List<string> keysToRemove = new List<string>();

            //    foreach (var kvp in devs)
            //    {
            //        bool result;
            //        string key = kvp.Key;
            //        var value = kvp.Value;
            //        switch (value)
            //        {
            //            case DUT_BASE D:
            //                D.DataGridView.Columns.Clear();
            //                D.DataGridView.Visible = false;
            //                Control parentControl = D.DataGridView.Parent;
            //                if (parentControl != null)
            //                    parentControl.Controls.Remove(D.DataGridView);

            //                result = D.UnInit();
            //                if (!result)
            //                    MessageBox.Show($"{D.Description} Uninit Fail");
            //                else
            //                    keysToRemove.Add(key);

            //                break;
            //            case IOBase I:
            //                result = I.UnInit();
            //                if (!result)
            //                    MessageBox.Show($"{I.Description} Uninit Fail");
            //                else
            //                    keysToRemove.Add(key);

            //                break;
            //            case ControlDeviceBase D:
            //                result = D.UnInit();
            //                if (!result)
            //                    MessageBox.Show($"{D.Description} Uninit Fail");
            //                else
            //                    keysToRemove.Add(key);
            //                break;

            //            case CCDBase C:
            //                result = C.UnInit();

            //                if (!result)                               
            //                    MessageBox.Show($"{C.Description} Uninit Fail");
            //                else
            //                    keysToRemove.Add(key);
            //                break;
            //            case VisaBase V:
            //                result = V.UnInit();

            //                if (!result)
            //                    MessageBox.Show($"{V.Description} Uninit Fail");
            //                else
            //                    keysToRemove.Add(key);
            //                break;
            //        }
            //    }
            //    GlobalNew.Devices.Clear();
            //    return true;
            //    //if (GlobalNew.Devices.Count == keysToRemove.Count)
            //    //{
            //    //    GlobalNew.Devices.Clear();
            //    //    return true;
            //    //}
            //    //else
            //    //{
            //    //    // 在迴圈外部移除成功 UnInit 的對象
            //    //    foreach (var key in keysToRemove)
            //    //    {
            //    //        GlobalNew.Devices.Remove(key);
            //    //    }
            //    //    return false;
            //    //}



            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error occurred during device un-initialization: {ex.Message}");
            //    return false;
            //}
        }
        public static bool InitDevices(string CurrentDevicePath, Dictionary<string, object> devs)
        {
            string ret = ProTreeView.ProTreeView.Load_Devices(CurrentDevicePath, GlobalNew.Devices);

            bool InitialSuccess = true;

            if (ret == "Load_Devices success")
            {
                foreach (var value in devs.Values)
                {
                    bool result = ((dynamic)value).Init("");

                    if (!result)
                    {
                        MessageBox.Show($"{((dynamic)value).Description} Init Fail");
                        InitialSuccess = false;
                    }
                }
            }
            else
            {
                MessageBox.Show("Load Devices Fail");
                return false;
            }

            return InitialSuccess;
        }
        public bool UnInitDevices()
        {
            try
            {
                foreach (var value in Devices.Values)
                {
                    bool result = ((dynamic)value).UnInit();
                    if (!result)
                    {
                        proTreeView_Devices.LogMessage($"{((dynamic)value).Description} UnInit Fail");
                        //MessageBox.Show($"{((dynamic)value).Description} UnInit Fail");
                    }
                }

                Devices.Clear();
                GlobalNew.Devices.Clear();

                return true;

            }
            catch (Exception ex)
            {
                proTreeView_Devices.LogMessage($"Error occurred during device un-initialization: {ex.Message}");
                return false;
            }
        }
        public bool InitDevices()
        {
            bool ret = proTreeView_Devices.Load_Devices(Devices);

            if (ret == true)
            {
                //這邊是為了讓處方選單中的裝置列能選擇，所以在關閉窗方視窗時也要清掉
                proTreeView_Devices.Load_Devices(GlobalNew.Devices);
                bool AllInitResult = true;
                foreach (var value in Devices.Values)
                {
                    bool result = ((dynamic)value).Init("");
                    if (value is DUT_BASE)
                    {

                    }
                    if (!result)
                    {
                        MessageBox.Show($"{((dynamic)value).Description} Init Fail");
                        AllInitResult = false;
                    }
                }

                if (!AllInitResult)
                    return false;
            }
            else
            {
                MessageBox.Show("Load Devices Fail");
                return false;
            }

            return true;
        }


        private void QBtn_CCD_Add_Click(object sender, EventArgs e)
        {
            if (CCDList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Equipment.CCD." + CCDList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;
                obj.NodeIcon = "camera.png";
                proTreeView_Devices.AddtoTree(Node);
            }
        }

        private void QBtn_MotorAdd_Click(object sender, EventArgs e)
        {
            if (MotorList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Equipment.Motion." + MotorList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;

                //Logger.Warn("Image save in root DIR");
                proTreeView_Devices.AddtoTree(Node);
            }
        }

        private void QBtn_Control_Click(object sender, EventArgs e)
        {
            if (ControlList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Equipment.ControlDevice." + ControlList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;
                obj.NodeIcon = "device.png";
                //Logger.Warn("Image save in root DIR");
                proTreeView_Devices.AddtoTree(Node);
            }
        }

        private void QBtn_IO_Click(object sender, EventArgs e)
        {
            if (IOList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Equipment.IO." + IOList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;
                obj.NodeIcon = "up-and-down.png";
                //Logger.Warn("Image save in root DIR");
                proTreeView_Devices.AddtoTree(Node);
            }
        }

        private void QBtn_VISA_Click(object sender, EventArgs e)
        {
            if (VISAList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Equipment.VISA." + VISAList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;
                obj.NodeIcon = "TCP_Visa.png";
                //Logger.Warn("Image save in root DIR");
                proTreeView_Devices.AddtoTree(Node);
            }
        }

        private void QBtn_DUT_Click(object sender, EventArgs e)
        {
            if (DUTList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.DUT." + DUTList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;
                obj.NodeIcon = "device.png";
                //Logger.Warn("Image save in root DIR");
                proTreeView_Devices.AddtoTree(Node);
            }
        }
        public static bool AreJsonEqual(string jsonStr1, string jsonStr2)
        {
            // 檢查輸入是否為空或 null
            if (string.IsNullOrEmpty(jsonStr1) || string.IsNullOrEmpty(jsonStr2))
            {
                return false;
            }
            try
            {
                // 將 JSON 字符串轉換為 JArray
                JArray array1 = JArray.Parse(jsonStr1);
                JArray array2 = JArray.Parse(jsonStr2);


                // 提取 SharedName 欄的值
                var sharedNames1 = new HashSet<string>(array1.Select(obj => (string)obj["SharedName"]));
                var sharedNames2 = new HashSet<string>(array2.Select(obj => (string)obj["SharedName"]));

                // 比較 SharedName 欄的值
                return sharedNames1.SetEquals(sharedNames2);
            }
            catch
            {
                return false;
            }

        }
        private void QBtn_Save_Click(object sender, EventArgs e)
        {
            if (GlobalNew.ProtreeON == "1")
            {
                string referenceJson = null;
                GlobalNew.Devices.Clear();
                proTreeView_Devices.Load_Devices(GlobalNew.Devices);
                //檢查所有Enable的DUT中的MultiDeviceTable設定的名稱是否一致
                foreach (var value in GlobalNew.Devices)
                {
                    if (value.Value is DUT_BASE && ((DUT_BASE)(value.Value)).Enable)
                    {
                        string multiDeviceTable = ((DUT_BASE)(value.Value)).MultiDeviceTable;

                        if (referenceJson == null)
                        {
                            // 將第一個元素作為參考
                            referenceJson = multiDeviceTable;

                        }
                        else
                        {
                            if(!string.IsNullOrEmpty(referenceJson))
                            {
                                if (multiDeviceTable == null || !AreJsonEqual(multiDeviceTable, referenceJson))
                                {
                                    MessageBox.Show($"{((DUT_BASE)(value.Value)).Description}的MultiDeviceTable 資料與列表中其它DUT中的多工裝置命名不相同.Saved Fail");
                                    return;                                
                                }
                            }
                            else
                            {
                                if(!string.IsNullOrEmpty(multiDeviceTable))
                                {
                                    MessageBox.Show($"{((DUT_BASE)(value.Value)).Description}的MultiDeviceTable 資料與列表中其它DUT中的多工裝置命名不相同.Saved Fail");
                                    return;
                                }
                            }
                        }
                    }
                }
                //檢查MultiDeviceTable設定的裝置是否有在目前樹狀裝置列表中，因為有可能裝置被移除但MultiDeviceTable沒更新

                foreach (var dev in GlobalNew.Devices)
                {
                    if (dev.Value is DUT_BASE && ((DUT_BASE)(dev.Value)).Enable)
                    {
                        string multiDeviceTable = ((DUT_BASE)(dev.Value)).MultiDeviceTable;
                        if (!string.IsNullOrEmpty(multiDeviceTable))
                        {
                            JArray data = JArray.Parse(multiDeviceTable);

                            foreach (var item in data)
                            {
                                string deviceObject = (string)item["DeviceObject"];
                                bool isExist = false;
                                foreach (var value in GlobalNew.Devices)
                                {
                                    if (deviceObject == ((CoreBase)(value.Value)).Description)
                                    {
                                        isExist = true;

                                    }
                                }
                                if (!isExist)
                                {
                                    MessageBox.Show($"{((DUT_BASE)(dev.Value)).Description}中MultiDeviceTable設定的{deviceObject}的裝置物不存在裝置列表中.Saved Fail");
                                    return;
                                }
                            }
                        }
                    }
                }
                ////Uninit目前主流程中的裝置列表
                //StaticUnInitDevices(GlobalNew.Devices);

                ////Uninit目前處方中使用的裝置列表
                //UnInitDevices();

                ////Init目前設定中的裝置列表並更新到主流程的裝置列表
                //Thread.Sleep(50);
                //if (InitDevices())
                {
                    //Save_DeviceList同步更新裝置列表到檔案
                    if (proTreeView_Devices.Save_DeviceList(jsonDevicePath))
                    {
                        MessageBox.Show($"Saved successfully");
     
                        //InitialGridView();
                    }
                    else
                        MessageBox.Show($"{jsonDevicePath} Saved Fail");
                }

                
                //else
                //{
                //    string message = "Some device initialization failed, do you want to continue saving?";

                //    DialogResult result = MessageBox.Show(message, "Confirm Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                //    if (result == DialogResult.Yes)
                //    {
                //        if (proTreeView_Devices.Save_DeviceList(jsonDevicePath))
                //            MessageBox.Show($"Saved successfully");
                //    }
                //}

                IsEditing = true;
            }
            else
            {
                //Uninit目前主流程中的裝置列表
                UnInitDevices();

                //Init目前設定中的裝置列表並更新到主流程的裝置列表
                Thread.Sleep(50);
                if (InitDevices())
                {
                    //Save_DeviceList同步更新裝置列表到檔案
                    if (proTreeView_Devices.Save_DeviceList(GlobalNew.DeviceListPath))
                    {
                        MessageBox.Show($"Saved & Initialize successfully");
                    }
                    else
                        MessageBox.Show($"{GlobalNew.DeviceListPath} Saved Fail");
                }
                else
                {
                    string message = "Some device initialization failed, do you want to continue saving?";

                    DialogResult result = MessageBox.Show(message, "Confirm Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        if (proTreeView_Devices.Save_DeviceList(GlobalNew.DeviceListPath))
                            MessageBox.Show($"Saved successfully");
                    }
                }

                IsEditing = true;
            }

        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            string close_jsonData = File.ReadAllText(jsonRecipePath);
            //if(close_jsonData.Equals(start_jsonData))
            //{
            //    Close();
            //}
            //else
            //{

            bool RecipeisSame = proTreeView_RecipePro.CompareFile(jsonRecipePath, 1);
            bool DevicesSame = proTreeView_Devices.CompareFile(jsonDevicePath, 0);
            if (!RecipeisSame || !DevicesSame)
            {
                DialogResult CheckClose = MessageBox.Show("have unsaved changes. Do you want to leave this page?\n有未儲存的變更，請確認是否離開此頁面",
                            "Ask",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                if (CheckClose == DialogResult.No)
                {
                    return;
                }
                else
                {
                    Close();
                }
            }
            else
            {
                Close();
            }
        }

        //private void panel1_MouseDown(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        ReleaseCapture();
        //        SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        //        this.BeginInvoke((MethodInvoker)delegate
        //        {
        //            this.WindowState = FormWindowState.Maximized;
        //        });
        //    }
        //}
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Clicks == 2) return;   // 避免與 DoubleClick 衝突

                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        //protected override void WndProc(ref Message m)
        //{

        //    if (m.Msg == WM_SYSCOMMAND && (m.WParam.ToInt32() & 0xFFF0) == SC_MOVE)
        //    {
        //        // 只有在特定情況下才最大化窗體
        //        if (this.WindowState != FormWindowState.Maximized)
        //        {
        //            this.WindowState = FormWindowState.Maximized;
        //        }
        //        return;
        //    }
        //    base.WndProc(ref m);
        //}
        private void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 雙擊縮放
                if (this.WindowState == FormWindowState.Maximized)
                {
                    // 還原大小 & 置中
                    this.WindowState = FormWindowState.Normal;
                    this.CenterToScreen();
                }
                else
                {
                    this.WindowState = FormWindowState.Maximized;
                }
            }
        }
        #region UserPage
        private void UserBtn_Click(object sender, EventArgs e)
        {
            if (GlobalNew.Weblogin == "1")
            {
                if (Global_Memory.UserLevel < 5)
                {
                    MessageBox.Show("權限不足");
                    return;
                }
            }
            ManagementTabControl.SelectedIndex = 2;
            panel_Focus.Height = UserBtn.Height;
            panel_Focus.Top = UserBtn.Top;
            //if (Global_Memory.UserLevel >= (int)User_Level.ADMIN)
            //{

            //}
            //else
            //{
            //    MessageBox.Show("權限不足");
            //}

        }

        private void QBtn_DeleteUser_Click(object sender, EventArgs e)
        {
            if (UserdataGridView.SelectedRows.Count > 0)
            {
                User selectedUser = (User)UserdataGridView.SelectedRows[0].DataBoundItem;
                GlobalNew.users.Remove(selectedUser);

                // 更新 DataGridView
                RefreshDataGridView();
            }
        }

        private void QBtn_UserAdd_Click(object sender, EventArgs e)
        {
            User newUser = new User();
            GlobalNew.users.Add(newUser);

            // 更新 DataGridView
            RefreshDataGridView();
        }

        private void RefreshDataGridView()
        {
            UserdataGridView.DataSource = new BindingList<User>(GlobalNew.users);
            UserdataGridView.Refresh();
        }

        private void UserdataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                User editedUser = (User)UserdataGridView.Rows[e.RowIndex].DataBoundItem;
                // 更新 DataGridView
                RefreshDataGridView();
            }
        }

        private void UserdataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // 當單元格的格式化時觸發，你可以在這裡進行解密等相應的處理
            // 這裡只是一個示例，實際上你可能需要更複雜的邏輯
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                User displayedUser = (User)UserdataGridView.Rows[e.RowIndex].DataBoundItem;

                // 在這裡對密碼進行解密
                //displayedUser.Password = EncryptionHelper.DecryptString(displayedUser.Password);

                // 更新 DataGridView
                RefreshDataGridView();
            }
        }

        private void QBtn_SaveUser_Click(object Sender, EventArgs e)
        {
            if (GlobalNew.Weblogin != "1")
            {
                string jsonString = JsonConvert.SerializeObject(GlobalNew.users);
                bool ret = EncryptionHelper.EncryptStringToFile(jsonString, "users_en");
                if (ret)
                    MessageBox.Show("Save Successfull");
                else
                    MessageBox.Show("Save Fail");
            }
            else
            {
                bool error_status = false;
                string name_updata = string.Empty;
                string level_updata = string.Empty;
                string result = string.Empty;
                foreach (var Users_list in GlobalNew.users.ToList())
                {
                    name_updata = Users_list.username;
                    level_updata = Users_list.permission.ToString();
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://10.1.10.107/sla/api/updatePermission");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer" + " " + GlobalNew.UserToken);
                    httpWebRequest.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        JObject jReq = new JObject();
                        jReq.Add("username", name_updata);
                        jReq.Add("permission", level_updata);

                        streamWriter.Write(jReq.ToString());
                    }
                    try
                    {
                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            result = streamReader.ReadToEnd();
                        }
                        if (!result.Contains("success"))
                        {
                            MessageBox.Show("level_updata Fail!!!");
                            break;
                        }
                    }
                    catch (Exception e1)
                    {
                        MessageBox.Show($"Updata Error=>{e1.Message} !!!");
                        error_status = true;
                        break;
                    }
                }
                if (!error_status)
                    if (result.Contains("success"))
                        MessageBox.Show("level_updata success!!!");
            }
        }
        #endregion

        #region RecipePage
        private void RecipeBtn_Click(object sender, EventArgs e)
        {
            ManagementTabControl.SelectedIndex = 3;
            panel_Focus.Height = RecipeBtn.Height;
            panel_Focus.Top = RecipeBtn.Top;
        }
        private void LoadRecipBtn_Click(object sender, EventArgs e)
        {
            proTreeView_Devices.Read_DeviceList();
        }


        private void AddSeqNodeBtn_Click(object sender, EventArgs e)
        {
            if (SeqNode_CB.SelectedIndex >= 0 && proTreeView_RecipeProcess != null)
            {
                Manufacture.SeqNode obj = new Manufacture.SeqNode();
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.SeqName;

                proTreeView_RecipeProcess.AddSrcipt(Node);
            }
        }

        private void LoadRecipeBtn_Click(object sender, EventArgs e)
        {
            proTreeView_RecipeProcess.Read_ScriptRecipe();
        }

        private void SaveRecipeBtn_Click(object sender, EventArgs e)
        {
            if (CheckRecipeDevInDeviceList())
            {
                string jsonPath = $@"{System.Environment.CurrentDirectory}\Config\{GlobalNew.STATIONNAME}.json";
                IsEditing = proTreeView_RecipeProcess.Save_ScriptRecipe(jsonPath);
            }
            else
                MessageBox.Show($"Save Fail", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);


        }
        private void AddSeqItemBtn_Click(object sender, EventArgs e)
        {
            if (SeqItem_CB.SelectedIndex >= 0 && proTreeView_RecipeProcess != null)
            {
                Manufacture.SeqItem obj = new Manufacture.SeqItem();
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.ItemName;

                proTreeView_RecipeProcess.AddSrcipt(Node);
            }
        }


        public bool SaveHeader()
        {
            try
            {
                foreach (TreeNode treeNode in proTreeView_RecipePro.GetTreeview().Nodes)
                {
                    RecursiveTree(treeNode);
                }

                string CSVFile_path = $"{GlobalNew.csvLogPath}\\{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result.csv";//csv path
                string CSVFile_path_PASS = $"{GlobalNew.csvLogPath}\\PASS_{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result.csv";
                string CSVFile_path_FAIL = $"{GlobalNew.csvLogPath}\\FAIL_{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result.csv";
                string CSVFile_Backuppath = $"{GlobalNew.csvLogPath}\\Backup\\{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result_{System.DateTime.Now.ToString("yyyyMMddHHmmss")}.csv";//csv path
                string CSVFile_Backuppath_PASS = $"{GlobalNew.csvLogPath}\\Backup\\PASS_{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result_{System.DateTime.Now.ToString("yyyyMMddHHmmss")}.csv";
                string CSVFile_Backuppath_FAIL = $"{GlobalNew.csvLogPath}\\Backup\\FAIL_{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result_{System.DateTime.Now.ToString("yyyyMMddHHmmss")}.csv";
                SaveHeaderFile(CSVFile_path, CSVFile_Backuppath);
                SaveHeaderFile(CSVFile_path_PASS, CSVFile_Backuppath_PASS);
                SaveHeaderFile(CSVFile_path_FAIL, CSVFile_Backuppath_FAIL);

                CSVHeader.Clear();//清除Header資料
                return true;
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"Check Recipe Item Name In ItemList: {ex.Message}");
                return false;
            }
        }

        private void RecursiveTree(TreeNode treeNode)
        {
            bool EnableFlag = true;
            string Spec = string.Empty;
            string Prefix = string.Empty;
            string ItemName = string.Empty;
            string PrefixName = string.Empty;
            if (treeNode.Tag is ScriptBase)
            {
                EnableFlag = ((ScriptBase)treeNode.Tag).Enable;
                if (EnableFlag)
                {
                    Spec = ((ScriptBase)treeNode.Tag).Spec;
                    if (Spec != "" && Spec != null)
                    {
                        ItemName = ((ScriptBase)treeNode.Tag).Description;
                        Prefix = ((ScriptBase)treeNode.Tag).Prefix;

                        if (Prefix != "" && Prefix != null)
                            PrefixName = $"{Prefix}_{ItemName}";
                        else
                            PrefixName = ItemName;
                        HeaderList(Spec, PrefixName);
                    }
                }
            }
            foreach (TreeNode n in treeNode.Nodes)
            {
                RecursiveTree(n);
            }
        }

        private void HeaderList(string jsonstr, string PrefixName)
        {

            //JObject obj_test = JsonConvert.DeserializeObject<JObject>(jsonstr);
            SpecParamsContainer specParams2 = JsonConvert.DeserializeObject<SpecParamsContainer>(jsonstr);

            if (specParams2.specParams == null)
            {
                RecipeSettingLogger.Error($"HeaderList Item({PrefixName})  Spec({jsonstr}) Content Error.");
                MessageBox.Show($"HeaderList Item({PrefixName})  Spec({jsonstr}) Content Error.");
                return;
            }
            foreach (var specitem in specParams2.specParams)
            {
                if (specitem.Name != null)
                {
                    specitem.Name = PrefixName + "_" + specitem.Name;


                    if (!CSVHeader.ContainsKey("ProductSN"))
                    {
                        CSVHeader.Add("ProductSN", "");
                        CSVHeader.Add("ProjectName", "");
                        CSVHeader.Add("StationName", "");
                        CSVHeader.Add("FixtureName", "");
                        CSVHeader.Add("FixturePart", "");
                        CSVHeader.Add("Result", "");
                        CSVHeader.Add("Failitem", "");
                        CSVHeader.Add("StartTime", "");
                        CSVHeader.Add("EndTime", "");
                        CSVHeader.Add("EndTotalTime", "");
                        int SN2_Length;
                        bool success = int.TryParse(GlobalNew.SN2_Length, out SN2_Length);
                        // 檢查轉換是否成功，並且 result 是否大於 0
                        if (success && SN2_Length > 0)
                        {
                            CSVHeader.Add("SN", "");
                        }
                    }
                    if (specitem.Csv != "OFF")
                    {
                        if (CSVHeader.ContainsKey(specitem.Name))
                            CSVHeader[specitem.Name] = "";
                        else
                            CSVHeader.Add(specitem.Name, "");
                    }

                }
            }

        }

        private void SaveHeaderFile(string csv_path, string csvBKPPath)
        {
            if (CSVHeader != null)
            {
                if (File.Exists(csv_path))
                {
                    StreamReader csv_cmd = null;
                    try
                    {
                        string[] row = null;
                        using (csv_cmd = new StreamReader(csv_path))
                        {
                            string line = csv_cmd.ReadLine();
                            row = line.Split(',');
                        }
                        // Check if each key in CSVHeader exists in row
                        bool allKeysExist = true;
                        foreach (string key in CSVHeader.Keys)
                        {
                            if (!row.Contains(key))
                            {
                                allKeysExist = false;
                                break;
                            }
                        }

                        if (!allKeysExist || CSVHeader.Count() != row.Count())
                        {
                            string directoryName = Path.GetDirectoryName(csvBKPPath);
                            if (!Directory.Exists(directoryName))
                            {
                                Directory.CreateDirectory(directoryName);
                            }
                            //MessageBox.Show("The CSV Header Titel name is not Equal with CSV File Title name, Move CSV File to Backup File!!", "CSV file Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            File.Move(csv_path, csvBKPPath);
                        }
                    }
                    catch (IOException E1)
                    {
                        // 處理例外狀況
                        Console.WriteLine("發生錯誤: " + E1.Message);
                        MessageBox.Show($"CSV Error: {E1.Message},Please Close {Path.GetFileName(csv_path)}", "CSV File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception E2)
                    {
                        MessageBox.Show("Error:" + E2, "CSV Save Header Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                jsonStr_header = JsonConvert.SerializeObject(new { headers = CSVHeader.Keys });
                File.WriteAllText("Config\\Header.json", jsonStr_header);

            }
        }
        private void SaveHeaderFile()
        {
            if (CSVHeader != null)
            {
                string csv_path = $"Output\\csv_file\\{System.DateTime.Now.ToString("yyyy")}\\{System.DateTime.Now.ToString("MM")}\\Result.csv";
                string csvBKPPath = $"Output\\csv_file\\Backup\\Result_bkp_{System.DateTime.Now.ToString("yyyyMMddHHmmss")}.csv";
                if (File.Exists(csv_path))
                {

                    try
                    {
                        string[] row = null;
                        using (StreamReader csv_cmd = new StreamReader(csv_path))
                        {
                            string line = csv_cmd.ReadLine();
                            row = line.Split(',');
                        }
                        if (CSVHeader.Count() != row.Count())
                        {
                            string directoryName = Path.GetDirectoryName(csvBKPPath);
                            if (!Directory.Exists(directoryName))
                            {
                                Directory.CreateDirectory(directoryName);
                            }
                            MessageBox.Show("The CSV Header Titel name is not Equal with CSV File Titel name, Move CSV File to Backup File!!", "CSV file Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            File.Move(csv_path, csvBKPPath);
                        }
                    }
                    catch (Exception E2)
                    {
                        MessageBox.Show("Error:" + E2, "CSV Header", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                jsonStr_header = JsonConvert.SerializeObject(new { headers = CSVHeader.Keys });
                File.WriteAllText("Config\\Header.json", jsonStr_header);
                CSVHeader.Clear();//清除Header資料
            }
        }
        public bool CheckRecipeDevInDeviceList()
        {
            try
            {
                List<string> devlist = proTreeView_Devices.GetTreeDeviceList();
                string jsonstr = "";
                string PrefixName = "";
                proTreeView_RecipeProcess.GetTreeScriptJSON(ref jsonstr);
                JArray seqItems = JArray.Parse(jsonstr);

                foreach (JObject seqItem in seqItems)
                {
                    string seqName = (string)seqItem["SeqName"];

                    foreach (JObject item in seqItem["SeqItems"])
                    {
                        bool Enable = (bool)item["Enable"];
                        string itemName = (string)item["ItemName"];
                        string deviceName = (string)item["DeviceName"];
                        string SpecRule = (string)item["SpecRule"];
                        string Prefix = (string)item["Prefix"];
                        if (Enable)
                        {
                            if (SpecRule != "" && SpecRule != null)
                            {
                                if (Prefix != "" && Prefix != null)
                                    PrefixName = $"{Prefix}_{itemName}";
                                else
                                    PrefixName = itemName;

                                HeaderList(SpecRule, PrefixName);
                            }
                        }
                        if (deviceName == "" || deviceName == null)
                        {
                            continue;
                        }

                        bool ret = devlist.Contains(deviceName);
                        if (ret == false)
                        {
                            MessageBox.Show($"Seq:[{seqName}] Item:[{itemName}] Device:[{deviceName}] not found in DeviceList", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }

                    }
                }
                SaveHeaderFile();

                return true;
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"Check Recipe Device Name In DeviceList: {ex.Message}");
                return false;
            }
        }
        private void Recipe_Chose_SelectedIndexChanged(object sender, EventArgs e)
        {

            string jsonPath = string.Empty;
            GlobalNew.RECIPENAME = Recipe_Chose.SelectedItem.ToString();
            if (GlobalNew.RECIPENAME == "Golden")
                jsonPath = $@"{System.Environment.CurrentDirectory}\Config\Golden.json";
            else if (GlobalNew.RECIPENAME == "Debug")
                jsonPath = $@"{System.Environment.CurrentDirectory}\Config\Debug.json";
            else
                jsonPath = $@"{System.Environment.CurrentDirectory}\Config\{GlobalNew.STATIONNAME}.json";
            proTreeView_RecipeProcess.Read_ScriptRecipe(jsonPath);
        }

        private void QBtn_AddSeqNode_Click(object sender, EventArgs e)
        {
            if (SeqNode_CB.SelectedIndex >= 0 && proTreeView_RecipeProcess != null)
            {
                Manufacture.SeqNode obj = new Manufacture.SeqNode();
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.SeqName;

                proTreeView_RecipeProcess.AddSrcipt(Node);
            }
        }

        private void QBtn_AddSeqItem_Click(object sender, EventArgs e)
        {
            if (SeqItem_CB.SelectedIndex >= 0 && proTreeView_RecipeProcess != null)
            {
                Manufacture.SeqItem obj = new Manufacture.SeqItem();
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.ItemName;

                proTreeView_RecipeProcess.AddSrcipt(Node);
            }
        }

        private void QBtn_SaveRecipe_Click(object sender, EventArgs e)
        {
            if (CheckRecipeDevInDeviceList())
            {
                string jsonPath = string.Empty;
                //GlobalNew.DUTJSONNAME = Recipe_Chose.SelectedItem.ToString();
                if (GlobalNew.RECIPENAME == "Golden")
                    jsonPath = $@"{System.Environment.CurrentDirectory}\Config\Golden.json";
                else if (GlobalNew.RECIPENAME == "Debug")
                    jsonPath = $@"{System.Environment.CurrentDirectory}\Config\Debug.json";
                else
                    jsonPath = $@"{System.Environment.CurrentDirectory}\Config\{GlobalNew.STATIONNAME}.json";
                //string jsonPath = $@"{System.Environment.CurrentDirectory}\Config\{GlobalNew.STATIONNAME}.json";
                IsEditing = proTreeView_RecipeProcess.Save_ScriptRecipe(jsonPath);
            }
            else
                MessageBox.Show($"Save Fail", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #endregion RecipePage
        #region RecipePagePro



        #endregion RecipePagePro

        private void qBtn_Sequences_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                // 取得剪貼簿中的文字
                string clipboardText = Clipboard.GetText();

                // 將文字分行
                var lines = clipboardText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // 檢查第一行是否以 "Item" 開頭
                if (lines.Length > 0 && lines[0].StartsWith("Item"))
                {
                    // 移除第一行
                    var remainingLines = lines.Skip(1).ToArray();

                    // 倒置剩余的行
                    Array.Reverse(remainingLines);

                    Manufacture.CoreBase _obj = new Container_Sequences();
                    _obj.Description = "TestProcess";
                    _obj.nodes_list.Clear();
                    _obj.NodeIcon = "closebox.png";
                    foreach (string des in remainingLines)
                    {
                        if (string.IsNullOrEmpty(des))
                            continue;

                        if (comboBox_Sequences.SelectedIndex >= 0 && proTreeView_RecipePro != null)
                        {
                            Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Script." + comboBox_Sequences.Text);
                            if (obj is Container_JIG_INIT)
                                obj.NodeIcon = "tool-box.png";
                            if (obj is Container_ExceptionHandling)
                                obj.NodeIcon = "first-aid.png";
                            if (obj is Container_Post_Process)
                                obj.NodeIcon = "execute.png";
                            if (obj is Container_Condition_Jumper)
                                obj.NodeIcon = "icons8-do-not-mix-96.png";
                            if (obj is Container_Thread)
                                obj.NodeIcon = "thread.png";
                            if (obj is Container_RepeatUntil)
                                obj.NodeIcon = "Retry.png";
                            obj.Description = Regex.Replace(des, @"[^\w]", "_");
                            TreeNode Node = new TreeNode();
                            Node.Tag = obj;
                            Node.Text = obj.Description;
                            proTreeView_RecipePro.AddtoTree(Node);
                        }

                    }

                    Clipboard.Clear();

                }
                else
                {
                    if (comboBox_Sequences.SelectedIndex >= 0 && proTreeView_RecipePro != null)
                    {
                        Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Script." + comboBox_Sequences.Text);
                        if (obj is Container_JIG_INIT)
                            obj.NodeIcon = "tool-box.png";
                        if (obj is Container_ExceptionHandling)
                            obj.NodeIcon = "first-aid.png";
                        if (obj is Container_Post_Process)
                            obj.NodeIcon = "execute.png";
                        if (obj is Container_Condition_Jumper)
                            obj.NodeIcon = "icons8-do-not-mix-96.png";
                        if (obj is Container_Thread)
                            obj.NodeIcon = "thread.png";
                        if (obj is Container_RepeatUntil)
                            obj.NodeIcon = "Retry.png";
                        TreeNode Node = new TreeNode();
                        Node.Tag = obj;
                        Node.Text = obj.Description;
                        Type x = Type.GetType("Container_MainThread");
                        proTreeView_RecipePro.AddtoTree(Node);
                    }
                }
            }
            else
            {
                if (comboBox_Sequences.SelectedIndex >= 0 && proTreeView_RecipePro != null)
                {
                    Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Script." + comboBox_Sequences.Text);
                    if (obj is Container_JIG_INIT)
                        obj.NodeIcon = "tool-box.png";
                    if (obj is Container_ExceptionHandling)
                        obj.NodeIcon = "first-aid.png";
                    if (obj is Container_Post_Process)
                        obj.NodeIcon = "execute.png";
                    if (obj is Container_Condition_Jumper)
                        obj.NodeIcon = "icons8-do-not-mix-96.png";
                    if (obj is Container_Thread)
                        obj.NodeIcon = "thread.png";
                    if (obj is Container_RepeatUntil)
                        obj.NodeIcon = "Retry.png";

                    TreeNode Node = new TreeNode();
                    Node.Tag = obj;
                    Node.Text = obj.Description;
                    Type x = Type.GetType("Container_MainThread");
                    proTreeView_RecipePro.AddtoTree(Node);
                }
            }

        }

        private void qBtn_RecipeProSave_Click(object sender, EventArgs e)
        {
            SaveHeader();
            IsEditing = proTreeView_RecipePro.Save_Recipe(jsonRecipePath, jsonDevicePath);
        }

        private void qBtn_AddMainthread_Click(object sender, EventArgs e)
        {
            if (proTreeView_RecipePro != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Script.Container_MainThread");
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;
                Node.ImageKey = "workflow.png";
                Node.SelectedImageKey = "workflow.png";
                obj.NodeIcon = "workflow.png";
                proTreeView_RecipePro.AddMainThread(Node);
            }
        }
        private void qBtnAddIOScript_Click(object sender, EventArgs e)
        {
            if (comboBox_IOScript.SelectedIndex >= 0 && proTreeView_RecipeProcess != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, $"AutoTestSystem.Script.{comboBox_IOScript.Text}");
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;

                proTreeView_RecipePro.AddtoTree(Node);
            }
        }

        private void qBtnAddExtraScript_Click(object sender, EventArgs e)
        {
            if (comboBox_ExtraScript.SelectedIndex >= 0 && proTreeView_RecipeProcess != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, $"AutoTestSystem.Script.{comboBox_ExtraScript.Text}");

                if (obj is Script_Extra_Jumper)
                    obj.NodeIcon = "uml.png";
                if (obj is Script_Extra_Generic_Command)
                    obj.NodeIcon = "Terminal.png";
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;

                proTreeView_RecipePro.AddtoTree(Node);
            }
        }

        private void qProRun_Click(object sender, EventArgs e)
        {
            //tabControlDUT.Visible = true;
            proTreeView_RecipePro.Visible = false;
            InitialGridView();
            proTreeView_RecipePro.ClearNodeColor();

            string FailStopGoto = "";
            List<string> xx = Global_Memory.HW_LIST.Select(item => item.Key).ToList();

            foreach (var value in Devices.Values)
            {
                Thread.Sleep(10);
                if (value is DUT_BASE)
                {
                    ProTreeView.ProTreeView temptreeview = new ProTreeView.ProTreeView();
                    temptreeview.SetMode(FlowMode.Process_Mode);
                    ((DUT_BASE)value).DataGridView.Focus();
                    //string jsonRecipePath = $@"{System.Environment.CurrentDirectory}\Config\TestRecipePro.json";
                    temptreeview.Read_Recipe(jsonRecipePath);
                    foreach (TreeNode n in temptreeview.GetTreeview().Nodes)
                    {
                        if (n.Tag is Container_MainThread == true)
                        {
                            Container_MainThread T = (Container_MainThread)n.Tag;
                            object[] context = new object[] { temptreeview, T.FailContinue, value };
                            if (1 == T.Process(n, context))
                            {
                                n.BackColor = Color.White;
                            }
                        }
                    }
                }
            }

            //foreach (TreeNode n in proTreeView_RecipePro.GetTreeview().Nodes)
            //{
            //    if (n.Tag is Container_MainThread == true)
            //    {
            //        Container_MainThread T = (Container_MainThread)n.Tag;
            //        object[] context = new object[] { proTreeView_RecipePro , T.FailContinue , FailStopGoto };
            //        if (1 == T.Process(n, context))
            //        {
            //            n.BackColor = Color.White;
            //        }
            //    }
            //}
        }

        private void MyDLLTreeView_KeyDown(object sender, KeyEventArgs e)
        {

            //if (e.KeyCode == Keys.F2)
            //{
            //    tabControlDUT.Visible = true;
            //    proTreeView_RecipePro.Visible = false;

            //    InitialGridView();

            //    if (Devices.Count > 0)
            //    {
            //        foreach (var value in Devices.Values)
            //        {
            //            if(value is DUT_BASE)
            //            {
            //                DUT_BASE temp = (DUT_BASE)value;
            //                //
            //                temp.DataGridView.Focus();
            //            }

            //        }
            //    }

            //    SN_TEXT2.Enabled = true;
            //    SN_TEXT2.Focus();
            //}
        }
        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                //tabControlDUT.Visible = false;
                proTreeView_RecipePro.Visible = true;
                proTreeView_RecipePro.Focus();
                SN_TEXT2.Enabled = false;
            }
        }
        public static int Number = 0;
        public void ClearDataGridTestData(DUT_BASE DUT)
        {

            foreach (string key in GlobalNew.DataGridViewsList.Keys)
            {
                foreach (DataGridViewRow row in GlobalNew.DataGridViewsList[key].Rows)
                {
                    row.Cells[DUT.Description + "_Result"].Value = "";
                    row.Cells[DUT.Description + "_Result"].Style.ForeColor = Color.Black;

                    if (key == DUT.Description)
                    {
                        row.Cells["Value"].Value = "";
                        row.Cells["TestTime"].Value = "";
                    }


                }

            }
        }
        public void InitialGridView()
        {
            currentRun = 0;
            //tabControlDUT.TabPages.Clear();
            //GlobalNew.DataGridViewsList.Clear();
            //foreach (var value in Devices.Values)
            //{
            //    foreach (TabPage tabPage in tabControlDUT.TabPages)
            //    {
            //        // 清除每個TabPage中現有的DataGridView控制項
            //        foreach (Control control in tabPage.Controls.OfType<DataGridView>().ToList())
            //        {
            //            tabPage.Controls.Remove(control);
            //            control.Dispose();
            //        }
            //    }
            //}


            int count = Devices.Count;

            foreach (var value in Devices.Values)
            {
                if (value is DUT_BASE)
                {
                    DUT_BASE temp = (DUT_BASE)value;
                    temp.DataGridView.Columns.Clear();

                    TabPage tabPage = new TabPage(temp.Description);
                    //tabControlDUT.TabPages.Add(tabPage);

                    DataGridView dataGridView = temp.DataGridView;
                    tabPage.Controls.Add(dataGridView);
                    // 註冊事件
                    dataGridView.RowsAdded += new DataGridViewRowsAddedEventHandler(dataGridView_RowsAdded);
                    dataGridView.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView_CellFormatting);
                    dataGridView.KeyDown += new KeyEventHandler(dataGridView_KeyDown);
                    // 設定外觀
                    dataGridView.Dock = DockStyle.Fill;


                    dataGridView.AllowUserToAddRows = false;

                    dataGridView.EnableHeadersVisualStyles = false;
                    dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
                    dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                    dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.LightBlue;
                    dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                    dataGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                    dataGridView.DefaultCellStyle.Font = new Font("Helvetica", 9, FontStyle.Regular);

                    dataGridView.DefaultCellStyle.SelectionBackColor = dataGridView.DefaultCellStyle.BackColor;
                    dataGridView.DefaultCellStyle.SelectionForeColor = dataGridView.DefaultCellStyle.ForeColor;
                    //dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.Single;
                    dataGridView.GridColor = Color.FromArgb(226, 226, 226);

                    dataGridView.ReadOnly = true;

                    dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // 設定列
                    dataGridView.Columns.Add("No", "No");
                    dataGridView.Columns.Add("ID", "ID");
                    dataGridView.Columns["ID"].Visible = false;
                    dataGridView.Columns.Add("Item", "Item");
                    dataGridView.Columns.Add("Spec", "Spec");
                    dataGridView.Columns.Add("Value", "Value");

                    //if(GlobalNew.Devices.Count == 1)
                    //{
                    dataGridView.Columns.Add("Result", "Result");
                    //dataGridView.Columns[temp2.Description + "_Result"].HeaderCell.Style.Font = new Font("Arial", 9, FontStyle.Bold);
                    dataGridView.Columns["Result"].HeaderCell.Style.ForeColor = Color.Blue;
                    //dataGridView.Columns[temp2.Description + "_Result"].HeaderCell.Style.BackColor = Color.LightBlue;
                    dataGridView.Columns["Result"].Width = 80;
                    //}
                    //else
                    //{
                    //    // 設定結果列
                    //    foreach (var value2 in GlobalNew.Devices.Values)
                    //    {
                    //        if (value2 is DUT_BASE)
                    //        {
                    //            DUT_BASE temp2 = (DUT_BASE)value2;
                    //            dataGridView.Columns.Add(temp2.Description + "_Result", temp2.Description + "_Result");
                    //            //dataGridView.Columns[temp2.Description + "_Result"].HeaderCell.Style.Font = new Font("Arial", 9, FontStyle.Bold);
                    //            dataGridView.Columns[temp2.Description + "_Result"].HeaderCell.Style.ForeColor = Color.Blue;
                    //            //dataGridView.Columns[temp2.Description + "_Result"].HeaderCell.Style.BackColor = Color.LightBlue;
                    //            dataGridView.Columns[temp2.Description + "_Result"].Width = 80;
                    //        }
                    //    }
                    //}


                    dataGridView.Columns.Add("TestTime", "TestTime(s)");
                    dataGridView.Columns.Add("Eslapse", "Eslapse(s)");
                    dataGridView.Columns["Eslapse"].Visible = false;
                    dataGridView.Columns.Add("Retry", "Retry");
                    dataGridView.Columns["Retry"].Visible = false;
                    dataGridView.Columns["No"].Width = 50;
                    dataGridView.Columns["Item"].Width = 100;
                    dataGridView.Columns["Spec"].Width = 100;
                    dataGridView.Columns["Value"].Width = 100;
                    dataGridView.Columns["TestTime"].Width = 80;
                    dataGridView.Columns["Eslapse"].Width = 80;
                    dataGridView.Columns["Retry"].Width = 80;
                    // 設定文字對齊和欄高
                    dataGridView.Columns[3].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    dataGridView.Columns[4].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                    dataGridView.RowTemplate.Height = 120;  // 設定每一行的高度為 120

                    // 將 DataGridView 加入至 GlobalNew.DataGridViewsList
                    if (GlobalNew.DataGridViewsList.ContainsKey(temp.Description))
                        GlobalNew.DataGridViewsList[temp.Description] = dataGridView;
                    else
                        GlobalNew.DataGridViewsList.Add(temp.Description, dataGridView);

                    //dataGridView.BorderStyle = BorderStyle.None;
                    //dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;

                    Number = 0;
                    TraverseTreeViewNodes(proTreeView_RecipePro.GetTreeview().Nodes, dataGridView);
                    //dataGridView.Invalidate();
                }


            }
        }
        private void TraverseTreeViewNodes(TreeNodeCollection nodes, DataGridView dv)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag != null && node.Checked && node.Tag is ScriptBase)
                {
                    ScriptBase tagObject = (ScriptBase)node.Tag;

                    if (tagObject.ShowItem == true)
                    {
                        string ShowSpec = string.Empty;

                        try
                        {
                            if (tagObject.Spec != string.Empty)
                            {
                                SpecParamsContainer specParams2 = JsonConvert.DeserializeObject<SpecParamsContainer>(tagObject.Spec);

                                foreach (var param in specParams2.specParams)
                                {

                                    switch (param.SpecType)
                                    {
                                        case SpecType.Range:
                                            ShowSpec += $"{param.MinLimit} < {param.Name} < {param.MaxLimit}\n";
                                            break;

                                        case SpecType.Equal:
                                            ShowSpec += $"{param.Name} = {param.SpecValue}\n";
                                            break;
                                        case SpecType.GreaterThan:
                                            ShowSpec += $"{param.Name} > {param.SpecValue}\n";

                                            break;
                                        case SpecType.LessThan:
                                            ShowSpec += $"{param.Name} < {param.SpecValue}\n";
                                            break;
                                        default:
                                            ShowSpec += "";
                                            break;
                                    }
                                }
                            }

                        }
                        catch (Newtonsoft.Json.JsonReaderException)
                        {
                            ShowSpec += "無法解析輸入數據為 JSON 格式";
                        }
                        catch (Exception ex)
                        {
                            ShowSpec += $"處理數據時出現錯誤: {ex.Message}";
                        }

                        ShowSpec = ShowSpec.TrimEnd('\n');

                        ScriptBase.DataItem newItem = new ScriptBase.DataItem();
                        newItem.No = Number++;
                        newItem.Item = tagObject.Description;
                        if (ShowSpec == string.Empty)
                            newItem.Spec = "N/A";
                        else
                            newItem.Spec = ShowSpec;
                        //newItem.DutList = new List<string> { "Dut1", "Dut2" };
                        newItem.TestResult = "PASS";
                        //newItem.TestTime = DateTime.Now;
                        //string dutListStr = string.Join(",", newItem.DutList);
                        object[] rowValues = { newItem.No, tagObject.ID, newItem.Item, newItem.Spec/*, newItem.Value, "PASS", newItem.TestResult, newItem.TestTime */};
                        dv.Rows.Add(rowValues);

                        //dv.Rows[dv.Rows.Count - 1].Cells[0].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        //dv.Rows[dv.Rows.Count - 1].Cells[1].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        //dv.Rows[dv.Rows.Count - 1].Cells[2].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        //dv.Rows[dv.Rows.Count - 1].Cells[3].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        dv.Rows[dv.Rows.Count - 1].Cells[2].Style.BackColor = Color.Aquamarine;
                        dv.Rows[dv.Rows.Count - 1].Cells[3].Style.BackColor = Color.Aquamarine;
                        //dv.Rows[dv.Rows.Count - 1].Cells[2].Style.ForeColor = Color.White;
                        //dv.Rows[dv.Rows.Count - 1].Cells[3].Style.ForeColor = Color.White;
                    }
                }

                if (node.Nodes.Count > 0)
                {
                    TraverseTreeViewNodes(node.Nodes, dv);
                }
            }
        }
        private void dataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            DataGridView dataGridView = sender as DataGridView;
            if (dataGridView != null)
            {
                int rowIndex = e.RowIndex;
                DataGridViewRow row = dataGridView.Rows[rowIndex];
                // 将所有单元格的文本居中对齐
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }
        private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView dataGridView = sender as DataGridView;
            if (dataGridView != null)
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.IsNewRow)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            cell.Style.Font = new Font("宋体", 20f, FontStyle.Regular);
                        }
                    }
                }

            }
        }
        private void GetCurrentFocus()
        {
            Control focusedControl = this.ActiveControl;

            if (focusedControl != null)
            {
                string controlName = focusedControl.Name;
                MessageBox.Show($"The currently focused control is: {controlName}");
            }
            else
            {
                MessageBox.Show("No control currently has focus.");
            }
        }
        private void ManagementTabControl_Selected(object sender, TabControlEventArgs e)
        {
            TabPage selectedTabPage = e.TabPage;
            if (selectedTabPage != null)
            {
                if (selectedTabPage.Text == "RecipeProPage")
                {
                    proTreeView_RecipePro.GetTreeview().Select();
                }
            }
        }

        private void SN_TEXT_KeyUp(object sender, KeyEventArgs e)
        {
            string sn = SN_TEXT.Text;
            //tabControlDUT.Visible = true;
            proTreeView_RecipePro.Visible = false;
            //InitialGridView();
            proTreeView_RecipePro.ClearNodeColor();

            string FailStopGoto = "";

            Thread.Sleep(10);

            ProTreeView.ProTreeView temptreeview = new ProTreeView.ProTreeView();
            temptreeview.SetMode(FlowMode.Process_Mode);
            ClearDataGridTestData(((DUT_BASE)Devices[proTreeView_Devices.GetTreeview().Nodes[0].Text]));
            ((DUT_BASE)Devices[proTreeView_Devices.GetTreeview().Nodes[0].Text]).DataGridView.Focus();
            //string jsonRecipePath = $@"{System.Environment.CurrentDirectory}\Config\TestRecipePro.json";
            temptreeview.Read_Recipe(jsonRecipePath);
            foreach (TreeNode n in temptreeview.GetTreeview().Nodes)
            {
                if (n.Tag is Container_MainThread == true)
                {
                    Container_MainThread T = (Container_MainThread)n.Tag;
                    object[] context = new object[] { temptreeview, T.FailContinue, Devices[proTreeView_Devices.GetTreeview().Nodes[0].Text] };
                    if (1 == T.Process(n, context))
                    {
                        n.BackColor = Color.White;
                    }
                }
            }


        }
        static int count = 0;
        private void SN_TEXT2_KeyUp(object sender, KeyEventArgs e)
        {

            //DUT_BASE dUT_BASE = ((DUT_BASE)GlobalNew.Devices[proTreeView_Devices.GetTreeview().Nodes[1].Text]);


            //string sn = SN_TEXT.Text;
            //tabControlDUT.Visible = true;
            //proTreeView_RecipePro.Visible = false;

            //proTreeView_RecipePro.ClearNodeColor();

            //string FailStopGoto = "";
            //List<string> xx = Global_Memory.HW_LIST.Select(item => item.Key).ToList();

            //Thread.Sleep(10);

            //ProTreeView.ProTreeView temptreeview = new ProTreeView.ProTreeView();
            //temptreeview.SetMode(FlowMode.Process_Mode);
            //ClearDataGridTestData(dUT_BASE);
            //dUT_BASE.DataGridView.Focus();
            //string jsonRecipePath = $@"{System.Environment.CurrentDirectory}\Config\TestRecipePro.json";
            //temptreeview.Read_Recipe(jsonRecipePath);
            //foreach (TreeNode n in temptreeview.GetTreeview().Nodes)
            //{
            //    if (n.Tag is Container_MainThread == true)
            //    {
            //        Container_MainThread T = (Container_MainThread)n.Tag;
            //        object[] context = new object[] { temptreeview, T.FailContinue, FailStopGoto, dUT_BASE };

            //        if (1 == T.Process(n, context))
            //        {
            //            n.BackColor = Color.White;
            //        }
            //    }
            //}
            //count++;
            //SN_TEXT2.Focus();
        }
        static int currentRun = 0;
        private void SN_TEXT2_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e != null && e.KeyCode != Keys.Enter) return;

            //string sn = SN_TEXT2.Text;
            //SN_TEXT2.Text = "";
            //DUT_BASE dUT_BASE = null;

            //for (int i = 0;i< GlobalNew.Devices.Count;i++)
            //{

            //    dUT_BASE = ((DUT_BASE)GlobalNew.Devices[proTreeView_Devices.GetTreeview().Nodes[currentRun % GlobalNew.Devices.Count].Text]);
            //    if (dUT_BASE.isRunning)
            //    {
            //        if (i == GlobalNew.Devices.Count-1)
            //        {
            //            return;
            //        }
            //    }              
            //    else
            //    {
            //        tabControlDUT.SelectTab(currentRun);
            //        currentRun++;

            //        break; 
            //    }                 
            //}
            //if (currentRun == GlobalNew.Devices.Count)
            //    currentRun = 0;

            //string TestSN = SN_TEXT.Text;
            //tabControlDUT.Visible = true;
            ////proTreeView_RecipePro.Visible = false;

            ////proTreeView_RecipePro.ClearNodeColor();

            //ProTreeView.ProTreeView temptreeview = new ProTreeView.ProTreeView();
            //temptreeview.SetMode(FlowMode.Process_Mode);
            //ClearDataGridTestData(dUT_BASE);
            //dUT_BASE.DataGridView.Focus();
            //string jsonRecipePath = $@"{System.Environment.CurrentDirectory}\Config\TestRecipePro.json";
            //temptreeview.Read_Recipe(jsonRecipePath);
            //foreach (TreeNode n in temptreeview.GetTreeview().Nodes)
            //{
            //    if (n.Tag is Container_MainThread == true)
            //    {
            //        Container_MainThread T = (Container_MainThread)n.Tag;

            //        dUT_BASE.isRunning = true;
            //        bool allAreRunning = GlobalNew.Devices.Values.Cast<DUT_BASE>().All(dut => dut.isRunning/* || dut.Ignore*/);
            //        if(allAreRunning)
            //            SN_TEXT2.Enabled = false;

            //        //if (dUT_BASE.Ignore)
            //        //{
            //        //    dUT_BASE.isRunning = false;
            //        //    //SN_TEXT2.Enabled = true;
            //        //    SN_TEXT2.Focus();
            //        //    return;
            //        //}

            //        object[] context = new object[] { temptreeview, T.FailContinue, dUT_BASE };

            //        Task ta= T.Act(n, context);
            //        ta.ContinueWith(t =>
            //        {
            //            dUT_BASE.isRunning = false;
            //            SN_TEXT2.Enabled = true;
            //            SN_TEXT2.Focus();

            //            // 例如，顯示訊息或執行其他操作

            //        }, TaskScheduler.FromCurrentSynchronizationContext()); // 這樣會確保 ContinueWith 在 UI 主線程上執行
            //        //{
            //        //    n.BackColor = Color.White;
            //        //}
            //    }
            //}
            //SN_TEXT2.Focus();
            //SN_TEXT2.Focus();
        }

        private void qBtn_SingleRun_Click(object sender, EventArgs e)
        {
            //Uninit目前主流程中的裝置列表
            StaticUnInitDevices(GlobalNew.Devices);
            //Uninit目前處方中的裝置列表
            UnInitDevices();

            if (InitDevices())
                MessageBox.Show("Initial Success!!");
            //foreach (TreeNode n in proTreeView_RecipePro.GetTreeview().Nodes)
            //{
            //    if (n.Tag is Container_MainThread == true)
            //    {
            //        Container_MainThread T = (Container_MainThread)n.Tag;
            //        if (T.isRunning == 0)
            //        {
            //            proTreeView_RecipePro.ClearNodeColor();


            //            DUT_BASE tempDUT = null;
            //            foreach (var value in GlobalNew.Devices.Values)
            //            {
            //                if(value is DUT_BASE)
            //                {
            //                    tempDUT = (DUT_BASE)value;
            //                    break;
            //                }

            //            }

            //            bool TestResult = false;
            //            //DUT_BASE tempDUT = new DUT_Simu();
            //            object[] context = new object[] { proTreeView_RecipePro, T.FailContinue, tempDUT, TestResult };
            //            //if (1 == T.Process(n, context))
            //            //{
            //            //    n.BackColor = Color.White;
            //            //}
            //            qBtn_SingleRun.Text = "Pause";
            //            Task ta = T.Act(n, context);


            //            ta.ContinueWith(t =>
            //            {
            //                qBtn_SingleRun.Text = "SingleRun";

            //            }, TaskScheduler.FromCurrentSynchronizationContext()); // 這樣會確保 ContinueWith 在 UI 主線程上執行}

            //        }
            //        else if (T.isRunning == 1)
            //        {
            //            qBtn_SingleRun.Text = "Continue";
            //            T.T_Pause();
            //        }
            //        else if (T.isRunning == 2)
            //        {
            //            qBtn_SingleRun.Text = "Pause";
            //            T.T_Continue();
            //        }
            //    }
            //}           
        }

        private void qBtnAddDUTScript_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                // 取得剪貼簿中的文字
                string clipboardText = Clipboard.GetText();

                // 將文字分行
                var lines = clipboardText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // 檢查第一行是否以 "Command" 開頭
                if (lines.Length > 0 && lines[0].StartsWith("Command"))
                {
                    string devsel = "";
                    foreach (var value in GlobalNew.Devices.Values)
                    {
                        if (value is DUT_BASE)
                        {
                            DUT_BASE tempDUT = (DUT_BASE)value;
                            devsel = tempDUT.Description;//當作預設
                            break;
                        }
                    }

                    foreach (string des in lines.Skip(1))
                    {
                        if (string.IsNullOrEmpty(des))
                            continue;

                        if (comboBox_DUTScript.SelectedIndex >= 0 && proTreeView_RecipeProcess != null)
                        {
                            Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, $"AutoTestSystem.Script.{comboBox_DUTScript.Text}");
                            obj.NodeIcon = "smart-devices.png";
                            obj.Description = Regex.Replace(des, @"[^\w]", "_");

                            dynamic dynamicObj = obj;

                            try
                            {
                                if (dynamicObj is Script_DUT_CGI)
                                    dynamicObj.JSONData = des;
                                else if (dynamicObj is Script_DUT_CommandCheck)
                                    dynamicObj.P2_Send_Command = des;
                                else
                                    dynamicObj.Send_Command = des;

                                dynamicObj.DeviceSel = devsel;
                            }
                            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
                            {
                                return;
                            }
                            TreeNode Node = new TreeNode();
                            Node.Tag = obj;
                            Node.Text = obj.Description;
                            proTreeView_RecipePro.AddtoTree(Node);
                        }

                    }

                    Clipboard.Clear();

                }
                else
                {
                    if (comboBox_DUTScript.SelectedIndex >= 0 && proTreeView_RecipeProcess != null)
                    {
                        Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, $"AutoTestSystem.Script.{comboBox_DUTScript.Text}");
                        obj.NodeIcon = "smart-devices.png";
                        TreeNode Node = new TreeNode();
                        Node.Tag = obj;
                        Node.Text = obj.Description;

                        proTreeView_RecipePro.AddtoTree(Node);
                    }
                }
            }
            else
            {
                if (comboBox_DUTScript.SelectedIndex >= 0 && proTreeView_RecipeProcess != null)
                {
                    Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, $"AutoTestSystem.Script.{comboBox_DUTScript.Text}");
                    obj.NodeIcon = "smart-devices.png";
                    TreeNode Node = new TreeNode();
                    Node.Tag = obj;
                    Node.Text = obj.Description;

                    proTreeView_RecipePro.AddtoTree(Node);
                }
            }
        }

        private void qBtnAddCCDScript_Click(object sender, EventArgs e)
        {
            if (comboBox_CCDScript.SelectedIndex >= 0 && proTreeView_RecipeProcess != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, $"AutoTestSystem.Script.{comboBox_CCDScript.Text}");
                obj.NodeIcon = "camera3.png";
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;

                proTreeView_RecipePro.AddtoTree(Node);
            }
        }

        private void qBtnAddCtrlScript_Click(object sender, EventArgs e)
        {
            if (comboBox_CtrlScript.SelectedIndex >= 0 && proTreeView_RecipeProcess != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, $"AutoTestSystem.Script.{comboBox_CtrlScript.Text}");
                if (obj is Script_Condition_Execute)
                    obj.NodeIcon = "uml.png";
                else
                    obj.NodeIcon = "mainboard.png";
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;

                proTreeView_RecipePro.AddtoTree(Node);
            }
        }

        private void RecipeManagement_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void RecipeManagement_Shown(object sender, EventArgs e)
        {
            //須將主介面流程中的裝置UNINIT，以防與目前選擇的處方裝置有衝突
            StaticUnInitDevices(GlobalNew.Devices);

            //初始化目前處方中已有的裝置
            InitDevices();
        }

        private void RecipeManagement_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (GlobalNew.ProtreeON == "1")
            {
                //Uninit目前處方中的裝置列表
                UnInitDevices();
            }

            // 如果 IsEditing 为 true，则询问用户是否要保存更改
            //if (IsEditing)
            //{
            //    DialogResult result = MessageBox.Show("Process settings have been edited, do you want to apply the changes?\n流程已重新編輯並儲存是否重新套用到生產清單",
            //                                           "Ask",
            //                                           MessageBoxButtons.YesNo,
            //                                           MessageBoxIcon.Question);
            //    if (result == DialogResult.Yes)
            //    {
            //        DialogResult = DialogResult.Yes;
            //    }
            //}
            //else
            //{
            //    DialogResult CheckClose = MessageBox.Show("Process settings have been edited, do you want to apply the changes?\n流程尚未儲存是否離開此頁面",
            //               "Ask",
            //               MessageBoxButtons.YesNo,
            //               MessageBoxIcon.Question);
            //    if (CheckClose == DialogResult.No)
            //    {
            //        return;
            //    }
            //    else
            //    {
            //        DialogResult = DialogResult.Yes;
            //    }
            //}
        }

        private void checkBox_showtip_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_showtip.Checked)
            {
                proTreeView_RecipePro.ShowTip(true);
            }
            else
            {
                // 執行取消勾選時的動作
                proTreeView_RecipePro.ShowTip(false);
            }
        }

        private void checkBoxHideBar_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxHideBar.Checked)
            {
                splitContainer_DeviceListLevel1.Panel1Collapsed = true; // 隱藏 Panel1
            }
            else
            {
                // 執行取消勾選時的動作
                splitContainer_DeviceListLevel1.Panel1Collapsed = false;
            }
        }


        private void MaximizeBtn_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
                Maximize.Text = "🗗"; // 還原符號
            }
            else if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                Maximize.Text = "🗖"; // 最大化符號
            }
        }

        private void MinimizeBtn_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void qBtnAddMotorScript_Click(object sender, EventArgs e)
        {
            if (comboBox_MotorScript.SelectedIndex >= 0 && proTreeView_RecipeProcess != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, $"AutoTestSystem.Script.{comboBox_MotorScript.Text}");
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;

                proTreeView_RecipePro.AddtoTree(Node);
            }
        }

        private void QBtn_TEACH_Click(object sender, EventArgs e)
        {
            if (TeachList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Equipment.Teach." + TeachList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;
                obj.NodeIcon = "equipment2.png";
                //Logger.Warn("Image save in root DIR");
                proTreeView_Devices.AddtoTree(Node);
            }
        }

        //========================Compare File========================
        private List<Control> originalControls = new List<Control>();
        private bool isOriginalControlsSaved = false;
        ProTreeView.ProTreeView proTreeView_CompareRecipePro = null;
        private int originalSplitterDistance;

        private void EventHandlerTreeKeyMethod(object sender, KeyEventArgs e)
        {
            string Key_up = e.KeyData.ToString();
            if (Key_up == "D, Control" || Key_up == "R, Control")
            {
                if (!isOriginalControlsSaved)
                {


                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Title = "Select a file";
                    openFileDialog.Filter = "All files (*.*)|*.*";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // 記錄原本的控制項
                        originalSplitterDistance = splitContainer_RecipePro.SplitterDistance;

                        foreach (Control control in splitContainer_RecipePro.Panel2.Controls)
                        {
                            originalControls.Add(control);
                        }
                        isOriginalControlsSaved = true;

                        string selectedFilePath = openFileDialog.FileName;
                        proTreeView_CompareRecipePro = new ProTreeView.ProTreeView();
                        proTreeView_CompareRecipePro.SetMode(FlowMode.Process_Mode);
                        proTreeView_CompareRecipePro.AddImage("camera.png", imageList1.Images["camera.png"]);
                        proTreeView_CompareRecipePro.AddImage("device.png", imageList1.Images["device.png"]);
                        proTreeView_CompareRecipePro.AddImage("up-and-down.png", imageList1.Images["up-and-down.png"]);
                        proTreeView_CompareRecipePro.AddImage("workflow.png", imageList1.Images["workflow.png"]);
                        proTreeView_CompareRecipePro.Drop(false);//禁止拖曳防止ID衝突
                        string sret = proTreeView_CompareRecipePro.Read_Recipe(selectedFilePath);
                        proTreeView_CompareRecipePro.ProcessNodeMouseClick += MainProTreeNodeMouseClick;
                        // 移除原有控制項
                        splitContainer_RecipePro.Panel2.Controls.Clear();
                        // 將新的控制項顯示在最上層
                        proTreeView_CompareRecipePro.BringToFront();
                        // 添加新的控制項
                        proTreeView_CompareRecipePro.Dock = DockStyle.Fill;
                        splitContainer_RecipePro.Panel2.Controls.Add(proTreeView_CompareRecipePro);

                        // 設置兩邊同寬
                        splitContainer_RecipePro.SplitterDistance = splitContainer_RecipePro.Width / 2;
                        scRecipeTB.Panel2Collapsed = true;

                        if (Key_up == "D, Control")
                        {
                            isCompare = true;
                            CompareAndHighlightNodes(proTreeView_RecipePro.GetTreeview().Nodes, proTreeView_CompareRecipePro.GetTreeview().Nodes);
                        }
                        if (Key_up == "R, Control")
                        {
                            isCompare = false;
                        }
                    }
                    else
                    {
                        // 還原之前的控制項配置
                        splitContainer_RecipePro.Panel2.Controls.Clear();
                        foreach (Control control in originalControls)
                        {
                            splitContainer_RecipePro.Panel2.Controls.Add(control);
                        }
                        scRecipeTB.Panel2Collapsed = false;
                        splitContainer_RecipePro.SplitterDistance = originalSplitterDistance;
                        proTreeView_CompareRecipePro = null;
                        proTreeView_RecipePro.ClearNodeColor();
                    }
                }
                else
                {
                    // 還原之前的控制項配置
                    splitContainer_RecipePro.Panel2.Controls.Clear();
                    foreach (Control control in originalControls)
                    {
                        splitContainer_RecipePro.Panel2.Controls.Add(control);
                    }
                    scRecipeTB.Panel2Collapsed = false;
                    splitContainer_RecipePro.SplitterDistance = originalSplitterDistance;
                    isOriginalControlsSaved = false;
                    if (isCompare == true)
                        proTreeView_RecipePro.ClearNodeColor();
                    proTreeView_CompareRecipePro = null;

                }
            }


        }
        private void DeviceMouseClickMethod(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right) // 檢查是否為右鍵點擊
            {
                proTreeView_Devices.GetTreeview().SelectedNode = e.Node; // 選取該節點
                contextMenuStrip1.Show(proTreeView_Devices.GetTreeview(), e.Location); // 在點擊處顯示選單
            }
        }
            
        private void MainProTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                if (sender == proTreeView_RecipePro)
                {
                    if (proTreeView_CompareRecipePro != null)
                    {
                        // 根據Tag中的ID屬性找到並觸發C的TreeView節點點擊事件
                        TreeNode targetNode = FindNodeById(proTreeView_CompareRecipePro.GetTreeview().Nodes, ((CoreBase)e.Node.Tag).ID);
                        if (targetNode != null)
                        {
                            // 觸發C的TreeView點擊事件
                            proTreeView_CompareRecipePro.GetTreeview().SelectedNode = targetNode;
                        }
                    }


                    //c.TreeView_NodeMouseClick(sender, e);
                }
                else if (sender == proTreeView_CompareRecipePro)
                {
                    // 根據Tag中的ID屬性找到並觸發C的TreeView節點點擊事件
                    TreeNode targetNode = FindNodeById(proTreeView_RecipePro.GetTreeview().Nodes, ((CoreBase)e.Node.Tag).ID);
                    if (targetNode != null)
                    {
                        // 觸發C的TreeView點擊事件
                        proTreeView_RecipePro.GetTreeview().SelectedNode = targetNode;
                    }

                }
            }
        }
        private TreeNode FindNodeById(TreeNodeCollection nodes, string id)
        {
            foreach (TreeNode node in nodes)
            {
                if (((CoreBase)node.Tag).ID == id)
                {
                    return node;
                }
                TreeNode foundNode = FindNodeById(node.Nodes, id);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            return null;
        }

        private void CompareAndHighlightNodes(TreeNodeCollection nodesA, TreeNodeCollection nodesB)
        {
            foreach (TreeNode nodeA in nodesA)
            {
                TreeNode matchingNodeB = FindNodeById(nodesB, ((CoreBase)nodeA.Tag).ID);
                if (matchingNodeB != null)
                {
                    // 將物件序列化為JSON字串
                    string jsonA = JsonConvert.SerializeObject((CoreBase)nodeA.Tag);
                    string jsonB = JsonConvert.SerializeObject((CoreBase)matchingNodeB.Tag);

                    // 比較JSON字串是否相同                  
                    if (jsonA == jsonB)
                    {
                        nodeA.BackColor = Color.White;
                        matchingNodeB.BackColor = Color.White;
                    }
                    else
                    {
                        nodeA.BackColor = Color.Red;
                        matchingNodeB.BackColor = Color.Red;
                    }
                }
                else
                {
                    nodeA.BackColor = Color.Red;
                }

                CompareAndHighlightNodes(nodeA.Nodes, nodesB);
            }
        }

        private void EventHandlerPropertyGridMethod(object s, PropertyValueChangedEventArgs e)
        {
            if (proTreeView_CompareRecipePro != null)
                if (isCompare)
                    CompareAndHighlightNodes(proTreeView_RecipePro.GetTreeview().Nodes, proTreeView_CompareRecipePro.GetTreeview().Nodes);
        }

        private void uninitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMLoggerThread(DeviceLOGGER);
            bool result = ((dynamic)proTreeView_Devices.GetTreeview().SelectedNode.Tag).UnInit();

        }
        private void TeachToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TerminalNode tempDUT = (TerminalNode)proTreeView_Devices.GetTreeview().SelectedNode.Tag;

            tempDUT.Show();
            //proTreeView_Devices.NodePropertyGrid.SelectedObject = proTreeView_Devices.GetTreeview().SelectedNode.Tag;

        }
        private void initToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMLoggerThread(DeviceLOGGER);
            bool result = ((dynamic)proTreeView_Devices.GetTreeview().SelectedNode.Tag).Init("");
            // 判斷是否執行成功，失敗時顯示訊息
            if (!result)
            {
                MessageBox.Show("Init 失敗！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Init 成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void externalToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                bool result = ((dynamic)proTreeView_Devices.GetTreeview().SelectedNode.Tag).UnInit();


                if (proTreeView_Devices.GetTreeview().SelectedNode.Tag is DUT_SERIALPORT|| proTreeView_Devices.GetTreeview().SelectedNode.Tag is SerialPortDevice)
                {
                    // 取得目前程式的執行目錄
                    string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    // 合併目錄和 ttermpro.exe 路徑
                    string ttermpath = System.IO.Path.Combine(currentDirectory, "Utility\\teraterm\\ttermpro.exe");

                    string Com = ((dynamic)proTreeView_Devices.GetTreeview().SelectedNode.Tag).PortName;
                    // 使用正則表達式提取數字部分
                    string numericPort = Regex.Match(Com, @"\d+").Value;

                    int BaudRate = ((dynamic)proTreeView_Devices.GetTreeview().SelectedNode.Tag).BaudRate;


                    // 要傳遞給 ttermpro.exe 的參數
                    string arguments = $"/C={numericPort} /BAUD={BaudRate}";  // 例如連接 COM4，波特率 9600
                                                                              // 確認檔案存在
                    if (System.IO.File.Exists(ttermpath))
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = ttermpath,   // 使用程式目錄中的 ttermpro.exe
                            Arguments = arguments,  // 傳遞參數
                            UseShellExecute = true  // 如果需要額外的 shell 操作，設為 true
                        };

                        Process.Start(psi);
                    }
                    else
                    {
                        MessageBox.Show(ttermpath +".path is not exist");
                    }
                }
                else if (proTreeView_Devices.GetTreeview().SelectedNode.Tag is Arduino)
                {
                    // 取得目前程式的執行目錄
                    string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    // 合併目錄和 ttermpro.exe 路徑
                    string ttermpath = System.IO.Path.Combine(currentDirectory, "Utility\\teraterm\\ttermpro.exe");

                    string Com = ((dynamic)proTreeView_Devices.GetTreeview().SelectedNode.Tag).PortName;
                    // 使用正則表達式提取數字部分
                    string numericPort = Regex.Match(Com, @"\d+").Value;

                    int BaudRate = ((dynamic)proTreeView_Devices.GetTreeview().SelectedNode.Tag).baudrate;


                    // 要傳遞給 ttermpro.exe 的參數
                    string arguments = $"/C={numericPort} /BAUD={BaudRate}";  // 例如連接 COM4，波特率 9600
                                                                              // 確認檔案存在
                    if (System.IO.File.Exists(ttermpath))
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = ttermpath,   // 使用程式目錄中的 ttermpro.exe
                            Arguments = arguments,  // 傳遞參數
                            UseShellExecute = true  // 如果需要額外的 shell 操作，設為 true
                        };

                        Process.Start(psi);
                    }
                    else
                    {
                        MessageBox.Show(ttermpath + ".path is not exist");
                    }
                }
                else if (proTreeView_Devices.GetTreeview().SelectedNode.Tag is MES)
                {
                    string p = ((dynamic)proTreeView_Devices.GetTreeview().SelectedNode.Tag).DLL_PATH;
                    string fullPath = Path.GetFullPath(p);
                    string directoryPath = Path.GetDirectoryName(fullPath);
                    // 取得目前程式的執行目錄
                    string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    // 合併目錄和 ttermpro.exe 路徑
                    string terminalpath = System.IO.Path.Combine(directoryPath, "Terminal.exe");

                    if (System.IO.File.Exists(terminalpath))
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = terminalpath,   // 使用程式目錄中的 ttermpro.exe
                            UseShellExecute = true  // 如果需要額外的 shell 操作，設為 true
                        };

                        Process.Start(psi);
                    }
                    else
                    {
                        MessageBox.Show(terminalpath + ".path is not exist");
                    }
                }
                else if(proTreeView_Devices.GetTreeview().SelectedNode.Tag is ADV_USB4704
                    || proTreeView_Devices.GetTreeview().SelectedNode.Tag is ADV_USB4761
                    || proTreeView_Devices.GetTreeview().SelectedNode.Tag is ADV_USB4751)
                {                                               // 確認檔案存在
                    if (System.IO.File.Exists("C:\\Advantech\\public\\Navigator\\navigator.exe"))
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = "C:\\Advantech\\public\\Navigator\\navigator.exe",   // 使用程式目錄中的 ttermpro.exe
                            UseShellExecute = true  // 如果需要額外的 shell 操作，設為 true
                        };

                        Process.Start(psi);
                    }
                    else
                    {
                        MessageBox.Show("C:\\Advantech\\public\\Navigator\\navigator.exe" + ".path is not exist");
                    }
                
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void QBtn_Image_Click(object sender, EventArgs e)
        {
            if (ImageList_CB.SelectedIndex >= 0 && proTreeView_Devices != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, "AutoTestSystem.Equipment.Image." + ImageList_CB.Text);
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;
                obj.NodeIcon = "camera.png";
                proTreeView_Devices.AddtoTree(Node);
            }
        }

        private void qBtnAddImageScript_Click(object sender, EventArgs e)
        {
            if (comboBox_ImageScript.SelectedIndex >= 0 && proTreeView_RecipeProcess != null)
            {
                Manufacture.CoreBase obj = Manufacture.Initialize.CreatObject(this, $"AutoTestSystem.Script.{comboBox_ImageScript.Text}");
                obj.NodeIcon = "camera3.png";
                TreeNode Node = new TreeNode();
                Node.Tag = obj;
                Node.Text = obj.Description;

                proTreeView_RecipePro.AddtoTree(Node);
            }
        }

        private void qBtn_RecipeProExport_Click(object sender, EventArgs e)
        {

        }


        //private void splitContainer_DeviceListLevel1_Panel1_MouseLeave(object sender, EventArgs e)
        //{
        //    splitContainer_DeviceListLevel1.Panel1Collapsed = true; // 隱藏 Panel1
        //}

        //private void splitContainer_DeviceListLevel1_Panel2_MouseEnter(object sender, EventArgs e)
        //{
        //    splitContainer_DeviceListLevel1.Panel1Collapsed = false; // 顯示 Panel1
        //}

        //private void splitContainer_DeviceListLevel1_Panel2_MouseLeave(object sender, EventArgs e)
        //{
        //    splitContainer_DeviceListLevel1.Panel1Collapsed = false; // 顯示 Panel1
        //}
    }

    public class GUIComponent
    {
        public ProTreeView.ProTreeView PTView { get; set; }
        public Container_MainThread MainThread { get; set; }
        public DataGridView DataGridView { get; set; }
    }
}
