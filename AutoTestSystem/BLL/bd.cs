using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

using Manufacture;
using Newtonsoft.Json;
//using Excel = Microsoft.Office.Interop.Excel;

namespace AutoTestSystem.BLL
{
    public static class Bd
    {
        //public static ILog LoggerOrigin = LogManager.GetLogger(typeof(Bd));
        public static MLog LoggerOrigin = new MLog("Bd");

        public static AsyncLocal<MLog> BDLogger = new AsyncLocal<MLog>();

        //public static ILog Logger
        //{
        //    get { return ModalthreadLocalLogger.Value ?? LoggerOrigin; }
        //}
        //public static void SetLoggerForCurrentThread(ILog logger)
        //{
        //    ModalthreadLocalLogger.Value = logger;
        //}
        public static void SetLoggerForCurrentThread(MLog logger)
        {
            BDLogger.Value = logger;
        }
        public static MLog Logger
        {
            get { return BDLogger.Value ?? LoggerOrigin; }
        }
        public static void CheckFolder(string path)
        {
            try
            {
                if (Directory.Exists(path)) return;
                Directory.CreateDirectory(path);
                Logger.Info($"Create Directory {path} succeed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
        }

        /// <summary>
        ///     创建初始文件夹,如果不存在.
        /// </summary>
        public static bool CreateFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                File.Create(path);
                Logger.Info($"Create file {path} succeed.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                throw;
            }
        }

        /// <summary>
        ///     修改ToolStripItem:调用方法：
        ///     myToolStripLabel.InvokeOnToolStripItem(label => label.Text = "Updated!");
        ///     myToolStripProgressBar.InvokeOnToolStripItem(bar => bar.PerformStep());
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="action"></param>
        public static void InvokeOnToolStripItem<T>(this T item, Action<T> action)
            where T : ToolStripItem
        {
            var parent = item.GetCurrentParent();
            if (parent.InvokeRequired)
                parent.Invoke(action, item);
            else
                action(item);
        }

        /// <summary>
        ///     检查字符串是否在测试值中出现过
        /// </summary>
        /// <param name="str">被搜寻的字符串</param>
        /// <param name="checkStr">搜寻的字符串</param>
        /// <returns></returns>
        public static bool CheckStr(this string str, string checkStr)
        {
            if (str.Contains(checkStr))
            {
                if (!string.IsNullOrEmpty(checkStr))
                {
                    Logger.Info($"Check Contain checkStr:{checkStr} ,pass.");
                }
                return true;
            }
            else
            {
                Logger.Error($"Check Contain checkStr:{checkStr} ,fail..");
                return false;
            }
        }

        /// <summary>
        /// 替换字符串中字符cellLogPath.Replacea(new char[] { '\\', '/', ':', '*', '?', '<', '>', '|', '"' });
        /// </summary>
        /// <param name="str"></param>
        /// <param name="oldStr"></param>
        /// <param name="newStr"></param>
        /// <returns></returns>
        public static string ReplaceStr(this string str, string[] oldStr, string newStr = " ")
        {
            foreach (var item in oldStr)
            {
                str = str.Replace(item, newStr);
            }
            return str;
        }
        public static string ReplaceStr(this string str, char[] oldStr, char newStr = ' ')
        {
            foreach (var item in oldStr)
            {
                str = str.Replace(item, newStr);
            }
            return str;
        }
        /// <summary>
        ///     压缩文件
        /// </summary>
        /// <param name="filePath">需要压缩的文件夹全路径</param>
        /// <param name="zipPath">压缩后文件的全路径</param>
        /// <param name="deletedAfterCompress">压缩后是否删除原文件</param>
        /// <returns></returns>
        public static bool CompressFile(string filePath, string zipPath, bool deletedAfterCompress = false)
        {
            var rReturn = false;
            var compCommand = $@"{System.Environment.CurrentDirectory}\7z.exe a -tzip {zipPath} {filePath}"; //压缩DOS指令
            if (RunDosCmd(compCommand, out var errors).Contains("Everything is Ok"))
            {
                rReturn = true;
                if (deletedAfterCompress) Directory.Delete(filePath);
            }
            else
            {
                Logger.Error(errors);
            }

            return rReturn;
        }

        /// <summary>
        ///     写SN到文件并复制到ATE目标路径
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="sourcefilePath">源文件绝对路径</param>
        /// <param name="destfilePath">目标文件绝对路径</param>
        /// <returns></returns>
        public static bool WriteSNandMoveFile(string sn, string sourcefilePath, string destfilePath)
        {
            var rReturn = false;
            try
            {
                if (File.Exists(sourcefilePath)) File.Delete(sourcefilePath);
                using (var sw = new StreamWriter(sourcefilePath, false, Encoding.Default))
                {
                    sw.WriteLine(sn);
                }
                var passToPath = destfilePath;
                if (File.Exists(passToPath)) File.Delete(passToPath);
                File.Move(sourcefilePath, passToPath);
                rReturn = true;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                rReturn = false;
            }
            return rReturn;
        }

        /// <summary>
        ///     等待并读取csv文件
        /// </summary>
        /// <param name="timeOut">超时时间</param>
        /// <param name="filepath">文件绝对路径</param>
        /// <param name="csvLines">csv文件内容</param>
        /// <returns></returns>
        public static bool WaitingCSVlog(string timeOut, string filepath, string str, out string[] csvLines)
        {
            Sleep(3000);
            var rReturn = false;
            var lngStart = DateTime.Now.AddSeconds(int.Parse(timeOut)).Ticks;
            var dir = new DirectoryInfo(filepath);
            while (DateTime.Now.Ticks <= lngStart)
            {
                // 返回目录中所有文件和子目录
                if (dir.GetFileSystemInfos().Length > 0)
                {
                    Logger.Debug($"find log number:{dir.GetFileSystemInfos().Length}.");
                    var files = Directory.GetFileSystemEntries(filepath);
                    foreach (var file in files)
                    {
                        if (file.Contains(str) && file.EndsWith(".csv"))
                        {
                            Logger.Debug($"find csv File:{file}");
                            using (var sr = new StreamReader(file))
                            {
                                csvLines = sr.ReadToEnd().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                            }
                            return true;
                        }
                    }
                }
                Thread.Sleep(1000);
            }
            Logger.Error($"Waiting csv log timeout{timeOut}.FAIL!!! ");
            csvLines = null;
            return rReturn;
        }

        #region 蓝牙

        /// <summary>
        ///     搜索蓝牙并配对
        /// </summary>
        /// <param name="devName"></param>
        /// <param name="devAddress"></param>
        /// <param name="searchRetry">搜索蓝牙重试次数</param>
        /// <returns></returns>
        public static bool BTConnection(string devName, string devAddress, int searchRetry)
        {
            var rReturn = false;
            var retry = 0;
            try
            {
                //Thread.Sleep(3000);
                Logger.Debug("Searching Blue-tooth......");
                var bluetoothClient = new BluetoothClient();
                BluetoothRadio.PrimaryRadio.Mode = RadioMode.Discoverable;
                var Inquiry_Time = TimeSpan.FromSeconds(5);
                bluetoothClient.InquiryLength = Inquiry_Time;
                //Thread.Sleep(3000);
                while (retry <= searchRetry)
                {
                    retry++;
                    //BluetoothDeviceInfo[] bluetoothDeviceInfo = bluetoothClient.DiscoverDevices(15, false, false, false, true);
                    var bluetoothDeviceInfo = bluetoothClient.DiscoverDevices(15);

                    Logger.Debug(
                        $"Find {bluetoothDeviceInfo.Length} Bluetooth.Target Bluetooth is:{devName}, Bt_mac:{devAddress}.");
                    for (var i = 0; i < bluetoothDeviceInfo.Length; i++)
                    {
                        Logger.Debug($"this name of BT{i}: {bluetoothDeviceInfo[i].DeviceName}");
                        if (bluetoothDeviceInfo[i].DeviceName == devName
                            && bluetoothDeviceInfo[i].DeviceAddress == BluetoothAddress.Parse(devAddress))
                        {
                            Logger.Debug($"Target Bluetooth is founded: {devName},Ready to match.");
                            var bdi = new BluetoothDeviceInfo(bluetoothDeviceInfo[i].DeviceAddress);
                            if (!bdi.Authenticated)
                            {
                                //string pair = rd.Pin; /* PIN for your dongle */
                                rReturn = BluetoothSecurity.PairRequest(bdi.DeviceAddress, "0000");
                                Logger.Debug($"Bluetooth pair {(rReturn ? "succeed" : "failed")}！");
                                if (rReturn) return true;
                            }
                        }
                    }

                    Thread.Sleep(10);
                }

                Logger.Error($"Target Bluetooth not been founded：{devName}！");
            }
            catch (Exception ex)
            {
                Logger.Fatal("Search Bluetooth Exception:" + ex);
                //throw;
            }
            finally
            {
                if (rReturn)
                {
                    if (BluetoothSecurity.RemoveDevice(BluetoothAddress.Parse(devAddress)))
                        Logger.Info("BT Remove Device Success !!");
                    else
                        Logger.Error("BT Remove Device Fail !!");
                }
                //else
                //{
                //    using (Comport DutCom = new Comport(DUTCOMinfo, ""))
                //    {//配对失败后，重启蓝牙，retry。
                //        string recvStr = "";
                //        DutCom.SendCommand2("3 0", ref recvStr, "", 1);
                //        DutCom.SendCommand2("4 0", ref recvStr, "", 1);
                //        Thread.Sleep(1000);
                //        DutCom.SendCommand2("2 0", ref recvStr, "Open connection", 1);
                //    }
                //}
            }

            return rReturn;
        }

        #endregion 蓝牙

        /// <summary>
        ///     对象序列化成Json文件并保存
        /// </summary>
        /// <param name="station">序列化对象</param>
        /// <param name="writeJsonPath">保存成Json文件的路径</param>
        /// <returns></returns>
        public static bool JsonSerializer(object station, out string JsonStr, string writeJsonPath = "", bool printJson = true)
        {
            var jsonClintContent = "";
            var result = false;
            try
            {
                // 序列化Json 配置，忽略值为null字段
                var setting = new JsonSerializerSettings();
                JsonConvert.DefaultSettings = () =>
                {
                    setting.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                    setting.DateFormatString = "yyyy-MM-dd HH:mm:ss"; //!日期类型默认格式化处理
                    setting.NullValueHandling = NullValueHandling.Ignore; //!空值处理,忽略空值
                    // setting.DefaultValueHandling = DefaultValueHandling.Ignore;
                    setting.Formatting = Formatting.Indented;
                    return setting;
                };
                jsonClintContent =
                   JsonConvert.SerializeObject(station, setting)
                   //.Replace("\"test\": \"\"", "\"test\": null")
                   .Replace("\"speed\": \"\"", "\"speed\": null")
                   //.Replace("\"name\": \"\"", "\"name\": null")
                   //.Replace("\"unit\": \"\"", "\"unit\": null")
                   .Replace("\"limit_max\": \"\"", "\"limit_max\": null")
                   .Replace("\"limit_min\": \"\"", "\"limit_min\": null")
                    .Replace("\"voltage\": \"\"", "\"voltage\": null")
                   .Replace("\"nor_version\": \"\"", "\"nor_version\": null")
                   .Replace("\"frequency\": \"\"", "\"frequency\": null")
                   .Replace("\"chain\": \"\"", "\"chain\": null")
                   .Replace("\"radio\": \"\"", "\"radio\": null")
                    .Replace("\"evm\": \"\"", "\"evm\": null")
                   .Replace("\"freq_error\": \"\"", "\"freq_error\": null")
                   .Replace("\"lo_leakage\": \"\"", "\"lo_leakage\": null")
                   .Replace("\"power_accuracy\": \"\"", "\"power_accuracy\": null")
                   .Replace("\"spectral_flatness\": \"\"", "\"spectral_flatness\": null")
                   .Replace("\"spectrum_mask\": \"\"", "\"spectrum_mask\": null")
                   .Replace("\"sym_clk_error\": \"\"", "\"sym_clk_error\": null")
                   .Replace("\"power_spec\": \"\"", "\"power_spec\": null")
                   .Replace("\"measured_power\": \"\"", "\"measured_power\": null")
                   .Replace("\"goal_power\": \"\"", "\"goal_power\": null")
                   .Replace("\"gain\": \"\"", "\"gain\": null")
                     .Replace("\"absolute_power\": \"\"", "\"absolute_power\": null")
                   .Replace("\"path_loss\": \"\"", "\"path_loss\": null")
                   .Replace("\"rx_power\": \"\"", "\"rx_power\": null")
                   .Replace("\"per\": \"\"", "\"per\": null")
                   .Replace("\"delta_f0_fn_max\": \"\"", "\"delta_f0_fn_max\": null")
                   .Replace("\"delta_f1_f0\": \"\"", "\"delta_f1_f0\": null")
                   .Replace("\"delta_f1_avg\": \"\"", "\"delta_f1_avg\": null")
                   .Replace("\"delta_f2_avg\": \"\"", "\"delta_f2_avg\": null")
                   .Replace("\"delta_f2_max\": \"\"", "\"delta_f2_max\": null")
                   .Replace("\"delta_fn_fn5_max\": \"\"", "\"delta_fn_fn5_max\": null")
                   .Replace("\"fn_max\": \"\"", "\"fn_max\": null")
                   .Replace("\"ini_freq_error\": \"\"", "\"ini_freq_error\": null")
                   .Replace("\"per_test_power\": \"\"", "\"per_test_power\": null")
                   .Replace("\"power\": \"\"", "\"power\": null")
                   .Replace("\"rx_per\": \"\"", "\"rx_per\": null")
                   .Replace("\"ratio_of_f2_to_f1\": \"\"", "\"ratio_of_f2_to_f1\": null");

                if (printJson)
                {
                    Logger.Debug(jsonClintContent);
                }
                if (!string.IsNullOrEmpty(writeJsonPath))
                {
                    File.WriteAllText(writeJsonPath, jsonClintContent);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                Logger.Fatal(ex.ToString());
            }
            JsonStr = jsonClintContent;
            return result;
        }

        /// <summary>
        ///     使用本地映射盘的方式操作共享文件夹,并复制文件到共享文件夹
        /// </summary>
        /// <param name="mapDrive">y映射的本地盘符名Z</param>
        /// <param name="netWorkUser"></param>
        /// <param name="netWorkPwd"></param>
        /// <param name="netWorkPath"></param>
        /// <param name="strFromPath">要上传的文件路径</param>
        /// <param name="strToPath"></param>
        public static void CopyLogToServer(string mapDrive, string netWorkUser, string netWorkPwd, string netWorkPath,
            string strFromPath, string strToPath)
        {
            var comLine =
                $@"net use {mapDrive}: /del /y&net use {mapDrive}: {netWorkPath} {netWorkPwd} /USER:{netWorkUser}";
            try
            {
                //net use Z: /del&net use Z: \\10.177.4.201\NOKIA_LOG Luxshare /USER:Luxshare\nokia_test
                //string FileName = Path.GetFileName(logfile);
                //string destPath = $@"{mapDrive}:\{Global.STATIONNAME}\{Global.FIXTURENAME}\{DateTime.Now.ToString("yyyyMMdd")}";
                if (RunDosCmd(comLine, out var output).Contains("命令成功完成"))
                {
                    if (!Directory.Exists(strToPath)) Directory.CreateDirectory(strToPath);
                    //File.Copy(logfile, destPath + @"\" + FileName, true);
                    File.Copy(strFromPath, strToPath, true);
                    Logger.Info("Upload test log to logServer success.");
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("Upload test log to logServer fail:" + ex);
                // throw;
            }
            finally
            {
                RunDosCmd($"net use {mapDrive}: /del /y", out var output); // 删除盘符
            }
        }

        #region 测试功能函数

        /// <summary>
        ///     结束进程
        /// </summary>
        /// <param name="processName"></param>
        public static bool KillProcess(string processName)
        {
            try
            {
                //Process[] localAll = Process.GetProcesses();
                var myProc = Process.GetProcessesByName(processName); //获取所有进程
                if (myProc.Length == 0) return true;
                foreach (var thisProc in myProc)
                {
                    if (thisProc.CloseMainWindow()) continue;
                    thisProc.Kill();
                    Logger.Debug($"Kill process: {processName}...");
                }

                Thread.Sleep(500);
                var myProc2 = Process.GetProcessesByName(processName); //获取所有进程
                return myProc2.Length == 0;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                //return false;
                throw;
            }
        }

        /// <summary>
        ///     启动进程
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool StartProcess(string processName, string fileName)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(fileName);
                var myProc = Process.GetProcessesByName(processName); //获取所有进程
                if (myProc.Length > 0) return true;

                if (fileInfo.Directory != null)
                {
                    var p = new Process { StartInfo = { WorkingDirectory = fileInfo.Directory.ToString(), FileName = fileInfo.Name } };
                    Logger.Debug(p.StartInfo.WorkingDirectory + "  " + p.StartInfo.FileName);
                    p.Start();
                }

                Thread.Sleep(2000);
                var myProc2 = Process.GetProcessesByName(processName); //获取所有进程
                return myProc2.Length > 0;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                //return false;
                throw;
            }
        }

        /// <summary>
        ///     重启进程
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool RestartProcess(string processName, string fileName)
        {

            try
            {
                FileInfo fileInfo = new FileInfo(fileName);
                if (KillProcess(processName))
                {
                    if (fileInfo.Directory != null)
                    {
                        var p = new Process { StartInfo = { WorkingDirectory = fileInfo.Directory.ToString(), FileName = fileInfo.Name } };
                        p.Start();
                    }

                    Thread.Sleep(1000);
                    var myProc = Process.GetProcessesByName(processName); //获取所有进程
                    return myProc.Length > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                throw;
            }
        }

        public static void Sleep(int millisecondsTimeout)
        {
            //Logger.Debug($"Waiting {millisecondsTimeout}ms.....");
            Thread.Sleep(millisecondsTimeout);
        }

        public static void Sleep(string secondsTimeout)
        {
            Logger.Debug($"Waiting {secondsTimeout}s.....");
            Thread.Sleep(TimeSpan.FromSeconds(int.Parse(secondsTimeout)));
        }

        /// <summary>
        ///     清空目录下所有文件
        /// </summary>
        public static bool ClearDirectory(string dirPath)
        {
            var rReturn = false;
            if (Directory.Exists(dirPath))
            {
                var dir = new DirectoryInfo(dirPath);
                var fileInfo = dir.GetFileSystemInfos(); //返回目录中所有文件和子目录
                foreach (var i in fileInfo)
                    if (i is DirectoryInfo) //判断是否文件夹
                    {
                        var subDir = new DirectoryInfo(i.FullName)
                        {
                            Attributes = FileAttributes.Normal & FileAttributes.Directory
                        };
                        //去除文件夹只读属性
                        subDir.Delete(true); //删除子目录和文件
                    }
                    else
                    {
                        File.SetAttributes(i.FullName, FileAttributes.Normal);
                        File.Delete(i.FullName); //删除指定文件
                    }

                rReturn = true;
            }
            else
            {
                rReturn = true;
            }

            return rReturn;
        }

        /// <summary>
        ///     清空目录下指定类型的文件
        /// </summary>
        /// <param name="dirPath">目录路径</param>
        /// <param name="extension">扩展名，后缀.txt</param>
        /// <returns></returns>
        public static bool ClearDirectory(string dirPath, string extension)
        {
            var rReturn = false;
            if (Directory.Exists(dirPath))
            {
                var dir = new DirectoryInfo(dirPath);
                var fileInfo = dir.GetFileSystemInfos(); //返回目录中所有文件和子目录
                foreach (var i in fileInfo)
                {
                    if (i.Extension != extension) continue;
                    File.SetAttributes(i.FullName, FileAttributes.Normal); // 去除文件只读属性
                    File.Delete(i.FullName); //删除指定文件
                }

                rReturn = true;
            }
            else
            {
                rReturn = true;
            }

            return rReturn;
        }

        /// <summary>
        ///     用反射的方法设置对象变量的值
        /// </summary>
        /// <param name="_object">对象实例</param>
        /// <param name="varName">变量名</param>
        /// <param name="varNewValue">新的变量值</param>
        // ReSharper disable once InconsistentNaming
        public static void SetVerReflection(object _object, string varName, string varNewValue)
        {
            try
            {
                if (string.IsNullOrEmpty(varName) || varNewValue == null) return;
                //Type myType = typeof(object);
                var myFieldInfo = _object.GetType().GetField(varName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (myFieldInfo != null)
                {
                    myFieldInfo.SetValue(_object, varNewValue);
                    Logger.Info($"Set variable:{varName}={varNewValue}");
                }
                else
                {
                    Logger.Equals($"Set variable Fail:{varName} doesn't exist!");
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.ToString());
                throw;
            }
        }

        /// <summary>
        ///     用反射的方法获取对象变量的值
        /// </summary>
        /// <param name="_object">对象,实例</param>
        /// <param name="varName">变量名</param>
        /// <returns></returns>
        public static string GetVerReflection(object _object, string varName)
        {
            string varValue = null;
            try
            {
                if (!string.IsNullOrEmpty(varName))
                {
                    var myFieldInfo = _object.GetType().GetField(varName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    if (myFieldInfo != null)
                    {
                        if (myFieldInfo.GetValue(_object) != null) varValue = myFieldInfo.GetValue(_object).ToString();
                        Logger.Debug($"Get Reflect variable:{varName}={varValue}");
                    }
                    else
                    {
                        Logger.Debug($"Get Reflect variable value Fail:{varName} does't exist!");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.ToString());
                //throw;
            }

            return varValue;
        }

        /// <summary>
        /// C#反射遍历对象字段，去掉值为fieldValue的字段
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="model">对象</param>
        public static T ForeachClassFields<T>(T model, string fieldValue)
        {
            var instance = Activator.CreateInstance<T>();
            //PropertyInfo[] PropertyList = t.GetProperties();
            FieldInfo[] FieldList = model.GetType().GetFields();
            foreach (FieldInfo item in FieldList)
            {
                string name = item.Name;
                var myFieldInfo = instance.GetType().GetField(name);
                var Value = item.GetValue(model);
                if (Value != null)
                {
                    if (Value.ToString() == fieldValue)
                    {
                        myFieldInfo.SetValue(instance, null);
                    }
                    else
                    {
                        myFieldInfo.SetValue(instance, Value);
                    }
                }
            }
            return instance;
        }


        private static string RunDosCmd(string cmd)
        {
            Logger.Debug($"DOSCommand-->{cmd}");
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments = "/C " + cmd,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            //process.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
            process.Start();
            var net = "";
            net = process.StandardOutput.ReadToEnd();
            process.Close();
            Logger.Debug(net);
            return net;
        }

        public static string RunDosCmd(string cmd, out string errors) //, string directoryPath = @"C:\Windows\System32")
        {
            Logger.Debug($"DOSCommand-->{cmd}");
            //说明：不管命令是否成功均执行exit命令，否则当调用ReadToEnd()方法时，会处于假死状态
            using (var p = new Process())
            {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/C " + cmd;
                p.StartInfo.UseShellExecute = false; //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true; //接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true; //由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true; //重定向标准错误输出
                p.StartInfo.CreateNoWindow = true; //不显示程序窗口
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //p.StartInfo.WorkingDirectory = directoryPath;
                var error = "";
                p.ErrorDataReceived += (sender, e) => { error += e.Data; };
                p.Start();
                p.StandardInput.WriteLine(cmd);
                p.StandardInput.AutoFlush = true;
                p.StandardInput.Close();
                p.BeginErrorReadLine(); //获取cmd窗口的输出信息
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                p.Close();
                Logger.Debug(output);
                errors = error;
                return output;
            }
        }

        /// <summary>
        ///     运行DOS命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="errors">命令错误信息</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public static string RunDosCmd(string cmd, out string errors, int timeout = 3000) //, string directoryPath = @"C:\Windows\System32")
        {
            Logger.Debug($"DOSCommand-->{cmd}");
            using (var process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                //process.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
                process.StartInfo.Arguments = "/c " + cmd;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true; //接受来自调用程序的输入信息
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true; //不显示程序窗口
                var output = new StringBuilder();
                var error = new StringBuilder();

                var outputWaitHandle = new AutoResetEvent(false);
                var errorWaitHandle = new AutoResetEvent(false);
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                            outputWaitHandle.Set();
                        else
                            output.AppendLine(e.Data);
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                            errorWaitHandle.Set();
                        else
                            error.AppendLine(e.Data);
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                        // Process completed. Check process.ExitCode here.
                        Logger.Debug("Process completed");
                    else
                        // Timed out.
                        Logger.Debug("RunDOSCmd output--> Timeout..." + error);
                    Logger.Debug("RunDOSCmd output-->" + output);
                    errors = error.ToString();
                    return output.ToString();
                }
            }
        }

        public static void RunDosCmd(string[] cmd, out string output)
        {
            //说明：不管命令是否成功均执行exit命令，否则当调用ReadToEnd()方法时，会处于假死状态
            using (var p = new Process())
            {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false; //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true; //接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true; //由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true; //重定向标准错误输出
                p.StartInfo.CreateNoWindow = true; //不显示程序窗口
                p.Start(); //启动程序
                foreach (var t in cmd) p.StandardInput.WriteLine(t);
                p.StandardInput.AutoFlush = true;
                p.StandardInput.Close();
                //获取cmd窗口的输出信息
                output = p.StandardOutput.ReadToEnd();
                Logger.Debug(output);
                p.WaitForExit(); //等待程序执行完退出进程
                var errorMessage = p.StandardError.ReadToEnd();
                p.StandardError.Close();
                if (string.IsNullOrEmpty(errorMessage))
                {
                    // Flag = true;
                }
                else
                {
                    Logger.Debug(errorMessage);
                }

                p.Close();
            }
        }

        /// <summary>
        ///     确认窗口
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <param name="bts">默认0:只有OK button</param>
        /// <returns></returns>
        public static bool ConfirmMessageBox(string text, string title, MessageBoxButtons bts = 0)
        {
            if (string.IsNullOrEmpty(title)) title = "please confirm(Xác nhận)";
            var dr = MessageBox.Show(text, title, bts, MessageBoxIcon.Exclamation);
            return dr == DialogResult.Yes || dr == DialogResult.OK;
        }

        /// <summary>
        ///     统计子字符串的总数
        /// </summary>
        /// <param name="str"></param>
        /// <param name="strSub"></param>
        /// <returns></returns>
        private static int GetTotalSubstr(string str, string strSub)
        {
            if (str.Contains(strSub))
                return str.Split(new[] { strSub }, StringSplitOptions.None).Length - 1;
            return 0;
        }

        [DllImport("kernel32.dll")]
        public static extern uint WinExec(string path, uint uCmdShow);

        /// <summary>
        ///     每秒Ping IP地址一次，ping通立即返回true，超过times后返回失败
        /// </summary>
        public static bool PingIP(string address, int times)
        {
            var rResult = false;
            WinExec("arp -d", 0);
            for (var i = times; i > 0; i--)
            {
                var pingReply = Ping(address);
                if (pingReply.Status == 0)
                {
                    Logger.Debug(
                        $"Reply from {pingReply.Address}：bytes={pingReply.Buffer.Length} time={pingReply.RoundtripTime} TTL={pingReply.Options.Ttl} {pingReply.Status}");
                    rResult = true;
                    break;
                }

                Logger.Debug($"ping {address} ：{pingReply.Status}");
                if (i == 1)
                {
                    Logger.Debug($"ping {address} ：Fail！！！！！");
                    rResult = false;
                }

                Thread.Sleep(1000);
            }

            return rResult;
        }

        public static PingReply Ping(string address)
        {
            Ping ping = null;
            try
            {
                ping = new Ping();
                return ping.Send(address, 2000);
            }
            catch (PingException ex)
            {
                if (ex.InnerException != null) Logger.Debug(ex.InnerException.ToString());
                return Ping(address);
            }
            finally
            {
                if (ping != null)
                {
                    // 2.0 下ping 的一个bug，需要显示转型后释放
                    IDisposable disposable = ping;
                    disposable.Dispose();
                    ping.Dispose();
                }
            }
        }

        /// <summary>
        ///     截取两个子字符中间的字符
        /// </summary>
        /// <param name="sourse"></param>
        /// <param name="startStr"></param>
        /// <param name="endStr"></param>
        /// <returns></returns>
        public static string GetMidStr(string sourse, string startStr, string endStr)
        {
            var result = string.Empty;
            try
            {
                if (startStr != null)
                {
                    var startIndex = sourse.IndexOf(startStr, StringComparison.Ordinal);
                    if (startIndex == -1)
                        return result;
                    var tmpStr = sourse.Substring(startIndex + startStr.Length);
                    if (endStr != null)
                    {
                        var endIndex = tmpStr.IndexOf(endStr, StringComparison.Ordinal);
                        if (endIndex == -1)
                            return result;
                        result = tmpStr.Remove(endIndex);
                    }
                    else
                    {
                        return tmpStr;
                    }
                }
                else
                {
                    var tmpStr = sourse;
                    if (endStr != null)
                    {
                        var endIndex = tmpStr.IndexOf(endStr, StringComparison.Ordinal);
                        if (endIndex == -1)
                            return result;
                        result = tmpStr.Remove(endIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                //throw ex;
            }
            return result;
        }

        /// <summary>
        ///     截取sub1和sub2中间字符串
        public static string GetSubStringOfMid(string souce, string sub1, string sub2)
        {
            int p1 = -1, p2, start = -1, length = 0;

            if (!string.IsNullOrEmpty(sub1))
            {
                p1 = souce.IndexOf(sub1);
                start = p1 + sub1.Length;
            }
            else
            {
                start = 0;
                p1 = 0; //从字符串的0位置开始截取
            }
            if (p1 == -1) //找不到字符串返回空
                return null;

            if (!string.IsNullOrEmpty(sub2))
                p2 = souce.IndexOf(sub2, start); //从sub1位置开始查找sub2
            else
                return souce.Substring(start); //一直截取到字符串末尾

            if (p1 == -1 || p2 == -1) //找不到字符串返回空
                return null;

            length = p2 - start;
            return souce.Substring(start, length).Replace("\n", "").Replace("\t", "").Replace("\r", "")
                .Replace("\r\n", "").Trim();
        }

        /// <summary>
        ///     截取sub1和sub2第n次出现的中间字符串
        /// </summary>
        /// <param name="souce"></param>
        /// <param name="sub1"></param>
        /// <param name="sub2"></param>
        /// <param name="no">第几次出现</param>
        /// <returns></returns>
        public static string GetSubStringOfMid(string souce, string sub1, string sub2, int no)
        {
            var p1 = GetPosition(souce, sub1, no);
            var p2 = GetPosition(souce, sub2, no);
            if (p1 == 0 || p2 == 0)
                return "";

            var star = p1 + sub1.Length;
            if (p2 - star < 0)
                return "";
            return souce.Substring(star, p2 - star).Trim();
        }

        /// <summary>
        ///     获取当前字符在字符串中第no次出现的位置
        /// </summary>
        /// <param name="s">字符串</param>
        /// <param name="key">字符</param>
        /// <param name="no">第几次出现</param>
        /// <returns>返回位置</returns>
        public static int GetPosition(string souces, string key, int no)
        {
            var pos = 0; //!出现的次数,每出现一次+1
            if (no == 0) //!如果是查找第0次出现则直接返回
                return 0;

            for (var i = 0; i < souces.Length; i++) //!遍历
                if (souces.IndexOf(key, i) > -1) //!查找到关键字
                {
                    i = souces.IndexOf(key, i);
                    pos++; //!出现次数+1
                    if (pos >= no) //!如果是需要的出现次数,则返回当前位置
                        return i;
                }

            return 0;
        }

        /// <summary>
        ///     截取字符串，获取测试值
        /// </summary>
        /// <param name="revStr"></param>
        /// <param name="SubStr1"></param>
        /// <param name="SubStr2"></param>
        /// <returns></returns>
        public static string GetValue(string revStr, string SubStr1, string SubStr2)
        {
            var TestValue = "";
            if (!string.IsNullOrEmpty(SubStr1) || !string.IsNullOrEmpty(SubStr2)) //需要提取TestValue
            {
                TestValue = GetSubStringOfMid(revStr, SubStr1, SubStr2);
                if (string.IsNullOrEmpty(TestValue))
                    Logger.Debug("Error! Get TestValue IsNullOrEmpty.");
                else
                    Logger.Debug($"GetTestValue:{TestValue}");
            }

            return TestValue;
        }

        /// <summary>
        ///     截取字符串，获取测试值
        /// </summary>
        /// <param name="revStr"></param>
        /// <param name="SubStr1"></param>
        /// <param name="SubStr2"></param>
        /// <param name="Round">如果是数字字符串，是否取整</param>
        /// <returns></returns>
        public static string
            GetValue(string revStr, string SubStr1, string SubStr2, bool Round = false) //, out string TestValue)
        {
            string testValue = null;
            double? temp = null;
            if (!string.IsNullOrEmpty(SubStr1) || !string.IsNullOrEmpty(SubStr2)) //需要提取TestValue
            {
                testValue = GetSubStringOfMid(revStr, SubStr1, SubStr2);
                if (string.IsNullOrEmpty(testValue))
                {
                    Logger.Debug("Error! Get TestValue IsNullOrEmpty.");
                }
                else
                {
                    Logger.Debug($"GetTestValue: {testValue}{(Round ? ",need integer" : "")}");
                    temp = Round ? Math.Round(double.Parse(testValue)) : double.Parse(testValue);
                }
            }

            testValue = temp.ToString();
            return testValue;
        }

        /// <summary>
        ///     比较规格上下限
        /// </summary>
        /// <param name="limitMin">下限值</param>
        /// <param name="limitMax">上限值</param>
        /// <param name="value">测试值</param>
        /// <param name="round">取整=true，默认false不取整</param>
        /// <returns></returns>
        public static bool CompareLimit(string limitMin, string limitMax, string value, bool round = false)
        {
            var rReturn = false;

            if (string.IsNullOrEmpty(limitMin) && string.IsNullOrEmpty(limitMax))
            {
                // 不需比较最大值和最小值直接返回true
                return true;
            }

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (double.TryParse(value, out double tempResult) == false)
            {
                return false;
            }

            var temp = round ? Math.Round(Convert.ToDouble(value)) : Convert.ToDouble(value);

            if (string.IsNullOrEmpty(limitMin) && !string.IsNullOrEmpty(limitMax)) //只需比较最大值
            {
                rReturn = temp <= double.Parse(limitMax);
                Logger.Debug("Compare Limit_max...");
            }

            if (!string.IsNullOrEmpty(limitMin) && string.IsNullOrEmpty(limitMax)) //只需比较最小值
            {
                rReturn = temp > double.Parse(limitMin);
                Logger.Debug("Compare Limit_min...");
            }

            if (!string.IsNullOrEmpty(limitMin) && !string.IsNullOrEmpty(limitMax)) //需要比较最小值和最大值
            {
                Logger.Debug("Compare Limit_min and Limit_max...");
                var rReturn1 = temp >= double.Parse(limitMin);
                var rReturn2 = temp <= double.Parse(limitMax);

                rReturn = rReturn1 & rReturn2;
            }
            return rReturn;
        }

        /// <summary>
        ///     检查Spce值
        /// </summary>
        /// <param name="spec"></param>
        /// <param name="testValue"></param>
        /// <returns></returns>
        public static bool CheckSpec(string spec, string testValue)
        {
            if (string.IsNullOrEmpty(testValue)) return false;

            var rReturn = false;
            if (!string.IsNullOrEmpty(spec))
            {
                // Spec值有多种情况，属于包含关系
                if (spec.Contains("{") && spec.Contains("}") && spec.Contains(testValue))
                {
                    Logger.Debug("check Spec contain pass");
                    rReturn = true;
                }
                else if (testValue == spec)
                {
                    // Spec值只有一种，检查==
                    Logger.Debug("check Spec == pass");
                    rReturn = true;
                }
            }
            else
            {
                Logger.Debug("check Spec fail");
                rReturn = false;
            }
            return rReturn;
        }

        #endregion 测试功能函数

        #region CSV文件操作

        public static void CreatCSVFile(string csvFilePath, string[] colHeader)
        {
            if (!File.Exists(csvFilePath))
            {
                File.Create(csvFilePath).Close();
                File.SetAttributes(csvFilePath, FileAttributes.Normal);
                Thread.Sleep(500);
                if (File.Exists(csvFilePath))
                {
                    var rowList = new List<string[]>();
                    rowList.Add(colHeader);
                    WriteCSV(csvFilePath, false, rowList);
                }
            }
        }

        public static void CreatCSVFile(string csvFilePath, string columnFilePath)
        {
            string[] colHeader;
            if (!File.Exists(csvFilePath))
            {
                File.Create(csvFilePath).Close();
                File.SetAttributes(csvFilePath, FileAttributes.Normal);
                Thread.Sleep(500);
                if (File.Exists(csvFilePath))
                {
                    var rowList = new List<string[]>();
                    using (var sr = new StreamReader(columnFilePath))
                    {
                        colHeader = sr.ReadToEnd().Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    }

                    rowList.Add(colHeader);
                    WriteCSV(csvFilePath, false, rowList);
                }
            }
            else
            {
                //Logger.Debug($"csv file have been created:{csvFilePath}");
            }
        }

        public static void WriteCSV(string filePathName, bool append, List<string[]> ls)
        {
            KillProcess("Excel");
            if (File.Exists(filePathName))
            {
                var fileWriter = new StreamWriter(filePathName, append, Encoding.GetEncoding(-0));
                foreach (var strArr in ls) fileWriter.WriteLine(string.Join(",", strArr));
                fileWriter.Flush();
                fileWriter.Close();
            }
            else
            {
                Logger.Debug($"the File:{filePathName} does not exist！");
            }
        }

        public static List<string[]> ReadCSV(string filePathName)
        {
            var ls = new List<string[]>();
            var fileReader = new StreamReader(filePathName);
            var strLine = "";
            while (strLine != null)
            {
                strLine = fileReader.ReadLine();
                if (strLine != null && strLine.Length > 0)
                    ls.Add(strLine.Split(','));
                else
                    Thread.Sleep(1);
            }

            fileReader.Close();
            return ls;
        }

        public static void DataGridViewToCSV(DataGridView dataGridView, bool append, string csvPath)
        {
            var sw = new StreamWriter(csvPath, append, Encoding.GetEncoding(-0));
            var strLine = "";
            try
            {
                //表头
                //for (int i = 0; i < dataGridView.ColumnCount; i++)
                //{
                //    if (i > 0)
                //        strLine += ",";
                //    strLine += dataGridView.Columns[i].HeaderText;
                //}
                //strLine.Remove(strLine.Length - 1);
                //sw.WriteLine(strLine);
                //strLine = "";
                //表的内容
                for (var j = 0; j < dataGridView.Rows.Count; j++)
                {
                    strLine = "";
                    var colCount = dataGridView.Columns.Count;
                    for (var k = 0; k < colCount; k++)
                    {
                        if (k > 0 && k < colCount)
                            strLine += ",";
                        if (dataGridView.Rows[j].Cells[k].Value == null)
                        {
                            strLine += "";
                        }
                        else
                        {
                            var cell = dataGridView.Rows[j].Cells[k].Value.ToString().Trim();
                            //防止里面含有特殊符号
                            cell = cell.Replace("\"", "\"\"");
                            cell = "\"" + cell + "\"";
                            strLine += cell;
                        }
                    }

                    sw.Flush();
                    sw.WriteLine(strLine);
                }

                sw.Close();
                Logger.Debug($"Export test result to {csvPath} succeed");
            }
            catch (Exception ex)
            {
                Logger.Debug($"Export test result to{csvPath} error!:{ex.Message} ");
                if (ReadCSV(csvPath).Count >= 65535)
                {
                    var renamePath = csvPath.Insert(csvPath.LastIndexOf("."), DateTime.Now.ToString("yyyy-MM-dd-HHmm"));
                    // 重命名
                    var fi = new FileInfo(csvPath); //result.csv
                    fi.MoveTo(renamePath); //result2020121115.csv
                }
            }
        }

        #endregion CSV文件操作

        /// <summary>
        /// 取取本机指定网段的IPV4地址
        /// </summary>
        /// <param name="networkSegment">如10.90.</param>
        /// <returns></returns>
        public static string GetAllIpv4Address(string networkSegment)
        {
            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name); // 获取本机所有IPV4地址
            foreach (IPAddress ipa in ipadrlist)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork && ipa.ToString().StartsWith(networkSegment))
                    //Console.WriteLine(ipa.ToString());
                    return ipa.ToString();
            }
            return "";
        }

        /// <summary>
        /// 关闭指定的excel文件,需要在非管理员模式下运行才可以
        /// </summary>
        /// <param name="workbookName">excel文件名</param>
        /// <param name="isSave">关闭时是否保存</param>
        //public static void CloseExcelWorkbook(string workbookName, bool isSave = false)
        //{
        //    try
        //    {
        //        Process[] plist = Process.GetProcessesByName("Excel", ".");
        //        if (plist.Length > 1)
        //            throw new Exception("More than one Excel process running.");
        //        else if (plist.Length == 0)
        //            return;

        //        Object obj = Marshal.GetActiveObject("Excel.Application");
        //        Microsoft.Office.Interop.Excel.Application excelAppl = (Excel.Application)obj;
        //        Excel.Workbooks workbooks = excelAppl.Workbooks;
        //        foreach (Excel.Workbook wkbk in workbooks)
        //        {
        //            if (wkbk.Name == workbookName)
        //                wkbk.Close(isSave);
        //        }
        //        //dispose
        //        //workbooks.Close(); //this would close all workbooks
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //        if (workbooks != null)
        //            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(workbooks);
        //        //excelAppl.Quit(); //would close the excel application
        //        System.Runtime.InteropServices.Marshal.FinalReleaseComObject(excelAppl);
        //        GC.Collect();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Fatal(ex.ToString());
        //    }
        //}

        public static string GetSHA256(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"{filePath} isn't exist");
            }
            byte[] by;
            SHA256Managed Sha256 = new SHA256Managed();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                by = Sha256.ComputeHash(stream);
            }
            return BitConverter.ToString(by).Replace("-", "").ToUpper();
            //return Convert.ToBase64String(by);      
        }

        public static byte[] GetSHA256Byte(string filePath)
        {
            byte[] by;
            SHA256Managed Sha256 = new SHA256Managed();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                by = Sha256.ComputeHash(stream);
            }
            return by;
        }

        public static void BinaryWrite(byte[] bytes, string writePath)
        {
            if (File.Exists(writePath))
            {
                // 去除文件只读属性
                File.SetAttributes(writePath, FileAttributes.Normal);
                File.Delete(writePath);
            }
            using (var stream = new FileStream(writePath, FileMode.CreateNew))
            {
                using (var bw = new BinaryWriter(stream))
                {
                    bw.Write(bytes);
                }
            }
        }

        public static string BinaryRead(string readPath)
        {
            if (!File.Exists(readPath))
            {
                MessageBox.Show($"{readPath} isn't exist");
            }
            byte[] reads;
            using (var stream = new FileStream(readPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var br = new BinaryReader(stream))
                {
                    reads = br.ReadBytes(100);
                }
            }
            return BitConverter.ToString(reads).Replace("-", "").ToUpper();
        }

    }
}