using Renci.SshNet;
using System;
using System.Collections;
using System.IO;

namespace AutoTestSystem.BLL
{
    /// <summary>
    /// SFTP操作类
    /// </summary>
    public class SFTPHelper : IDisposable
    {
        #region 字段或属性

        private SftpClient sftp;

        /// <summary>
        /// SFTP连接状态
        /// </summary>
        public bool Connected { get { return sftp.IsConnected; } }

        #endregion 字段或属性

        #region 构造

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">端口</param>
        /// <param name="user">用户名</param>
        /// <param name="pwd">密码</param>
        public SFTPHelper(string ip, string user, string pwd, string port = "22")
        {
            sftp = new SftpClient(ip, Int32.Parse(port), user, pwd);
        }

        #endregion 构造

        #region 连接SFTP

        /// <summary>
        /// 连接SFTP
        /// </summary>
        /// <returns>true成功</returns>
        public bool Connect()
        {
            try
            {
                if (!Connected)
                {
                    sftp.Connect();
                }
                return true;
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("连接SFTP失败，原因：{0}", ex.Message));
                throw new Exception(string.Format("Connect SFTP failed，cause：{0}", ex.Message));
            }
        }

        #endregion 连接SFTP

        #region 断开SFTP

        /// <summary>
        /// 断开SFTP
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (sftp != null && Connected)
                {
                    sftp.Disconnect();
                }
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("断开SFTP失败，原因：{0}", ex.Message));
                throw new Exception(string.Format("Disconnect SFTP failed，cause：{0}", ex.Message));
            }
        }

        #endregion 断开SFTP

        #region SFTP上传文件

        /// <summary>
        /// SFTP上传文件
        /// </summary>
        /// <param name="localPath">本地路径</param>
        /// <param name="remotePath">远程路径</param>
        public void Put(string localPath, string remotePath)
        {
            try
            {
                using (var file = File.OpenRead(localPath))
                {
                    Connect();
                    sftp.UploadFile(file, remotePath);
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("SFTP文件上传失败，原因：{0}", ex.Message));
                throw new Exception(string.Format("SFTP file upload failed，cause：{0}", ex.Message));
            }
        }

        #endregion SFTP上传文件

        #region SFTP获取文件

        /// <summary>
        /// SFTP获取文件
        /// </summary>
        /// <param name="remotePath">远程路径</param>
        /// <param name="localPath">本地路径</param>
        public void Get(string remotePath, string localPath)
        {
            try
            {
                Connect();
                var byt = sftp.ReadAllBytes(remotePath);
                Disconnect();
                File.WriteAllBytes(localPath, byt);
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("SFTP文件获取失败，原因：{0}", ex.Message));
                throw new Exception(string.Format("SFTP file download failed，cause：{0}", ex.Message));
            }
        }

        #endregion SFTP获取文件

        #region 删除SFTP文件

        /// <summary>
        /// 删除SFTP文件
        /// </summary>
        /// <param name="remoteFile">远程路径</param>
        public void Delete(string remoteFile)
        {
            try
            {
                Connect();
                sftp.Delete(remoteFile);
                Disconnect();
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("SFTP文件删除失败，原因：{0}", ex.Message));
                throw new Exception(string.Format("SFTP file delete failed，cause：{0}", ex.Message));
            }
        }

        #endregion 删除SFTP文件

        #region 获取SFTP文件列表

        /// <summary>
        /// 获取SFTP文件列表
        /// </summary>
        /// <param name="remotePath">远程目录</param>
        /// <param name="fileSuffix">文件后缀</param>
        /// <returns></returns>
        public ArrayList GetFileList(string remotePath, string fileSuffix)
        {
            try
            {
                Connect();
                var files = sftp.ListDirectory(remotePath);
                Disconnect();
                var objList = new ArrayList();
                foreach (var file in files)
                {
                    string name = file.Name;
                    if (name.Length > (fileSuffix.Length + 1) && fileSuffix == name.Substring(name.Length - fileSuffix.Length))
                    {
                        objList.Add(name);
                    }
                }
                return objList;
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("SFTP文件列表获取失败，原因：{0}", ex.Message));
                throw new Exception(string.Format("SFTP get file list failed，cause：{0}", ex.Message));
            }
        }

        #endregion 获取SFTP文件列表

        #region 移动SFTP文件

        /// <summary>
        /// 移动SFTP文件
        /// </summary>
        /// <param name="oldRemotePath">旧远程路径</param>
        /// <param name="newRemotePath">新远程路径</param>
        public void Move(string oldRemotePath, string newRemotePath)
        {
            try
            {
                Connect();
                sftp.RenameFile(oldRemotePath, newRemotePath);
                Disconnect();
            }
            catch (Exception ex)
            {
                // TxtLog.WriteTxt(CommonMethod.GetProgramName(), string.Format("SFTP文件移动失败，原因：{0}", ex.Message));
                throw new Exception(string.Format("SFTP move remote file failed，casue：{0}", ex.Message));
            }
        }

        public void Dispose()
        {
            ((IDisposable)sftp).Dispose();
        }

        #endregion 移动SFTP文件
    }
}