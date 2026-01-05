using AutoTestSystem.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AutoTestSystem.BLL
{
    /// <summary>
    /// INI文件操作类
    /// </summary>
    public class INIHelper
    {
        public string FilePath;

        /// <summary>
        /// 文件必须是全路径，否则写ini文件不生效
        /// </summary>
        /// <param name="_filePath"></param>
        public INIHelper(string _filePath)
        {
            FilePath = _filePath;
        }

        /// <summary>
        /// 为INI文件中指定的节点取得字符串
        /// </summary>
        /// <param name="lpAppName">欲在其中查找关键字的节点名称</param>
        /// <param name="lpKeyName">欲获取的项名</param>
        /// <param name="lpDefault">指定的项没有找到时返回的默认值</param>
        /// <param name="lpReturnedString">指定一个字串缓冲区，长度至少为nSize</param>
        /// <param name="nSize">指定装载到lpReturnedString缓冲区的最大字符数量</param>
        /// <param name="lpFileName">INI文件完整路径</param>
        /// <returns>复制到lpReturnedString缓冲区的字节数量，其中不包括那些NULL中止字符</returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        /// <summary>
        /// 修改INI文件中内容
        /// </summary>
        /// <param name="lpApplicationName">欲在其中写入的节点名称</param>
        /// <param name="lpKeyName">欲设置的项名</param>
        /// <param name="lpString">要写入的新字符串</param>
        /// <param name="lpFileName">INI文件完整路径</param>
        /// <returns>非零表示成功，零表示失败</returns>
        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);

        /// <summary>
        /// 读取INI文件值
        /// </summary>
        /// <param name="section">节点名</param>
        /// <param name="key">键</param>
        /// <param name="def">未取到值时返回的默认值</param>
        /// <returns>读取的值</returns>
        public string Readini(string section, string key)//, string def, string filePath)
        {
            StringBuilder sb = new StringBuilder(1024);
            GetPrivateProfileString(section, key, null, sb, 255, FilePath);
            Global.SaveLog($"Read Global Variable: {section}: {key}: {sb.ToString()}");
            return sb.ToString();
        }
        public static string Readini(string section, string key,string filepath)//, string def, string filePath)
        {
            StringBuilder sb = new StringBuilder(1024);
            GetPrivateProfileString(section, key, null, sb, 255, filepath);
            return sb.ToString();
        }
        /// <summary>
        /// 写INI文件值
        /// </summary>
        /// <param name="section">欲在其中写入的节点名称</param>
        /// <param name="key">欲设置的项名</param>
        /// <param name="value">要写入的新字符串</param>
        /// <returns>非零表示成功，零表示失败</returns>
        public int Writeini(string section, string key, string value)//, string filePath)
        {
            CheckPath(FilePath);
            return WritePrivateProfileString(section, key, value, FilePath);
        }
        public static int Writeini(string section, string key, string value, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                MessageBox.Show("the profile does not exist!", "ERROR!");
                throw new ArgumentNullException("filePath");
            }
            return WritePrivateProfileString(section, key, value, path);
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="section">节点名</param>
        /// <param name="filePath">INI文件完整路径</param>
        /// <returns>非零表示成功，零表示失败</returns>
        public int DeleteSection(string section)//, string filePath)
        {
            return Writeini(section, null, null);//, filePath);
        }

        /// <summary>
        /// 删除键的值
        /// </summary>
        /// <param name="section">节点名</param>
        /// <param name="key">键名</param>
        /// <param name="filePath">INI文件完整路径</param>
        /// <returns>非零表示成功，零表示失败</returns>
        public int DeleteKey(string section, string key)//, string filePath)
        {
            return Writeini(section, key, null);//, filePath);
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public void CheckPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("the profile does not exist!", "ERROR!");
                throw new ArgumentNullException("filePath");
            }
        }

        public Dictionary<string, string> GetSectionKeyValuePairs(string section)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            string[] lines = File.ReadAllLines(FilePath);
            bool isInTargetSection = false;

            foreach (string line in lines)
            {
                // 检查是否在目标节点下
                if (line.Trim() == $"[{section}]")
                {
                    isInTargetSection = true;
                }
                else if (isInTargetSection && line.Contains("=") && !line.StartsWith(";"))
                {
                    // 如果在目标节点下且有键值对，则将键值对添加到字典中
                    string[] keyValue = line.Split('=');
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();
                    keyValuePairs[key] = value;
                }
                else if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    // 如果遇到下一個節點，則跳出循環
                    if (isInTargetSection)
                    {
                        break;
                    }
                }
            }

            return keyValuePairs;
        }
    }
}