/*
 * program entry point
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
 *  1. <Program.cs> is a program entry point
 *  2. EntryPoint: <Program.cs> entry point --> <MainForm.cs >
 * 
 */

/*****************************************************************************
*                    Includes Definitions
*****************************************************************************/
using AutoTestSystem.BLL;
using AutoTestSystem.Model;
using System;
using System.Reflection;
using System.Windows.Forms;


/*****************************************************************************
*                    Function code
*****************************************************************************/
namespace AutoTestSystem
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "Runin");

            string Weblogin;
            string FormMode;
            INIHelper iniConfig = new INIHelper(Global.IniConfigFile);
            Weblogin = iniConfig.Readini("Station", "Weblogin").Trim();
            FormMode = iniConfig.Readini("Station", "FormMode").Trim();
            


            if (mutex.WaitOne(0, false))
            {
                LoginForm Login_Form = null;

                if(Weblogin !="1")                    
                    Login_Form = new LoginForm(false);
                else
                    Login_Form = new LoginForm();

                DialogResult res = Login_Form.ShowDialog();
                if (res == DialogResult.OK)
                {

                    //Application.Run(new MainForm());
                    //MessageBox.Show("Login successful","Login Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (FormMode == "1")
                    {
                        string dllPath = "Manufacture.dll";
                        Version targetVersion = new Version("1.0.0.6");

                        if (!IsDllVersionGreaterThan(dllPath, targetVersion))
                        {
                            MessageBox.Show("Please update Manufacture.dll to version 1.0.0.6 or higher.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        Application.Run(new MFGX());
                    }
                    else
                        Application.Run(new MainForm());
                }                  
            }
            else
            {
                MainForm.f1.WindowState = FormWindowState.Normal;
                MainForm.f1.Activate();
                MainForm.f1.Visible = true;
            }
            System.Environment.Exit(0);
        }

        public static bool IsDllVersionGreaterThan(string dllPath, Version targetVersion)
        {
            Assembly assembly = Assembly.LoadFrom(dllPath);
            Version dllVersion = assembly.GetName().Version;

            return dllVersion >= targetVersion;
        }
    }
}