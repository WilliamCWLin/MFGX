using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AutoTestSystem.BLL
{
    public enum FileListStyle
    {
        UnixStyle,
        WindowsStyle,
        Unknown
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FileStruct
    {
        public string Flags;
        public string Owner;
        public string Group;
        public bool IsDirectory;
        public DateTime CreateTime;
        public string Name;
    }

    public class FTPHelper
    {
        //private string ftpPassWord;
        //private string ftpUser;
        //private string host = null;
        private string ftpUser = null;

        private string ftpPassWord = null;

        private FtpWebRequest ftpRequest = null;
        private FtpWebResponse ftpResponse = null;
        private Stream ftpStream = null;
        private int bufferSize = 2048;

        /* Construct Object */

        //public FTPHelper(string hostIP, string userName, string password)
        //{
        //    host = hostIP; user = userName; pass = password;
        //}

        public FTPHelper(string ftpUser, string ftpPassWord)
        {
            this.ftpUser = ftpUser;
            this.ftpPassWord = ftpPassWord;
        }

        #region ftp下载文件

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="remoteFilePath"></param>
        /// <param name="localFilePath"></param>
        public void DownloadFile(string remoteFilePath, string localFilePath)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(remoteFilePath);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(ftpUser, ftpPassWord);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Get the FTP Server’s Response Stream */
                ftpStream = ftpResponse.GetResponseStream();
                /* Open a File Stream to Write the Downloaded File */
                FileStream localFileStream = new FileStream(localFilePath, FileMode.Create);
                /* Buffer for the Downloaded Data */
                byte[] byteBuffer = new byte[bufferSize];
                int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);

                /* Download the File by Writing the Buffered Data Until the Transfer is Complete */
                try
                {
                    while (bytesRead > 0)
                    {
                        //SetLableTxt(label1, "正在下载更新...... ", Color.Red); //更新界面label控件的委托
                        localFileStream.Write(byteBuffer, 0, bytesRead);
                        bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                /* Resource Cleanup */
                localFileStream.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + remoteFilePath, "ERROR", 0, MessageBoxIcon.Error);
            }
            return;
        }

        /// <summary>
        /// 下载目录
        /// </summary>
        /// <param name="ftpDir"></param>
        /// <param name="saveDir"></param>
        public void DownloadDir(string ftpDir, string saveDir)
        {
            List<FileStruct> filelist = DirectoryListDetailed(ftpDir);
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            foreach (FileStruct file in filelist)
            {
                if (file.IsDirectory)
                {
                    DownloadDir(ftpDir + "/" + file.Name, saveDir + @"\" + file.Name);
                }
                else
                {
                    if (File.Exists(saveDir + @"\" + file.Name))
                    {
                        File.Delete(saveDir + @"\" + file.Name);
                    }
                    DownloadFile(ftpDir + "/" + file.Name, saveDir + @"\" + file.Name);
                }
            }
        }

        public List<FileStruct> DirectoryListDetailed(string ftpUri)
        {
            List<FileStruct> list = new List<FileStruct>();
            FileStruct item = new FileStruct();
            string[] DirectoryLists = GetDirectoryListDetailed(ftpUri);
            FileListStyle style = GuessFileListStyle(DirectoryLists);
            if (style == FileListStyle.UnixStyle)
            {
                // 按照liunx目录方式去解析
                for (int i = 0; i < DirectoryLists.Length; i++)
                {
                    if (DirectoryLists[i] != "." || DirectoryLists[i] != "..")
                    {
                        item = ParseFileStructFromUnixStyleRecord(DirectoryLists[i]);
                        list.Add(item);
                    }
                }
            }
            else if (style == FileListStyle.WindowsStyle)
            {
                // 按照Windows目录方式去解析
                for (int i = 0; i < DirectoryLists.Length; i++)
                {
                    item = ParseFileStructFromWindowsStyleRecord(DirectoryLists[i]);
                    list.Add(item);
                }
            }
            return list;
        }

        private FileListStyle GuessFileListStyle(string[] DirectoryLists)
        {
            foreach (string str in DirectoryLists)
            {
                if ((str.Length > 10) && Regex.IsMatch(str.Substring(0, 10), "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                {
                    return FileListStyle.UnixStyle;
                }
                if ((str.Length > 8) && Regex.IsMatch(str.Substring(0, 8), "[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
                {
                    return FileListStyle.WindowsStyle;
                }
            }
            return FileListStyle.Unknown;
        }

        /// <summary>
        ///  /* List Directory Contents in Detail (Name, Size, Created, etc.) */
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public string[] GetDirectoryListDetailed(string directory)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(directory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(ftpUser, ftpPassWord);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server’s Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string directoryRaw = null;
                /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                try
                {
                    while (ftpReader.Peek() != -1)
                    {
                        directoryRaw += ftpReader.ReadLine() + "|";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return the Directory Listing as a string Array by Parsing ‘directoryRaw’ with the Delimiter               you Append (I use | in This Example) */
                try
                {
                    string[] directoryList = directoryRaw.Split("|".ToCharArray());
                    return directoryList;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            /* Return an Empty string Array if an Exception Occurs */
            return new string[] { "" };
        }

        private FileStruct ParseFileStructFromUnixStyleRecord(string Record)
        {
            FileStruct file = new FileStruct();
            string[] temp = Record.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            file.Flags = temp[0];
            file.IsDirectory = temp[0] == "d" ? true : false;
            file.Owner = temp[2];
            file.Group = temp[3];
            file.Name = temp[8];
            return file;
        }

        private FileStruct ParseFileStructFromWindowsStyleRecord(string Record)
        {
            FileStruct file = new FileStruct();
            string[] temp = Record.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //file.Flags = temp[0];
            file.IsDirectory = temp[2] == "<DIR>" ? true : false;
            //file.Owner = temp[2];
            //file.Group = temp[3];
            file.Name = temp[3];
            return file;
        }

        /* Download File */

        #endregion ftp下载文件

        public void UploadFile(string From, string To)
        {
            //string From = @"F:\Kaushik\Test.xlsx";
            //string To = "ftp://192.168.1.103:24/directory/Test.xlsx";

            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(ftpUser, ftpPassWord);
                client.UploadFile(To, WebRequestMethods.Ftp.UploadFile, From);
            }
        }

        public void FtpUploadAsync(Uri uri, string filePath)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Credentials = new NetworkCredential(ftpUser, ftpPassWord);
                webClient.UploadFileAsync(uri, WebRequestMethods.Ftp.UploadFile, filePath);
            }
        }

        public void DeleteFTPFolder(string Folderpath)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Folderpath);
            request.Method = WebRequestMethods.Ftp.RemoveDirectory;
            request.Credentials = new System.Net.NetworkCredential(ftpUser, ftpPassWord); ;
            request.GetResponse().Close();
        }

        /// <summary>
        /// Below code is used to check if the folder exists or not.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public bool DoesFtpDirectoryExist(string dirPath)
        {
            bool isexist = false;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(dirPath);
                request.Credentials = new NetworkCredential(ftpUser, ftpPassWord);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    isexist = true;
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        return false;
                    }
                }
            }
            return isexist;
        }

        /// <summary>
        /// create a folder on FTP
        /// </summary>
        /// <returns></returns>
        public bool CreateFolder(string host, string folderName)
        {
            //string host = "ftp://192.168.1.103:24";
            //string UserId = "VISION-PC";
            //string Password = "vision";
            //string path = "/Index";
            bool IsCreated = true;
            try
            {
                WebRequest request = WebRequest.Create(host + folderName);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(ftpUser, ftpPassWord);
                using (var resp = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine(resp.StatusCode);
                }
            }
            catch (Exception)
            {
                IsCreated = false;
                throw;
            }
            return IsCreated;
        }
    }
}