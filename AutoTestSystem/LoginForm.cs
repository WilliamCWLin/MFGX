using AutoTestSystem.BLL;
using AutoTestSystem.Model;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//Add by MTE William
namespace AutoTestSystem
{

    public partial class LoginForm : Form
    {
        private IUserRepository userRepository;
        private bool m_bypass = false;
        //private List<User> Users;
        public LoginForm()
        {
            IUserRepository userRepository = new JsonUserRepository();
            this.userRepository = userRepository;           

            InitializeComponent();
        }
        public LoginForm(bool bypass)
        {
            
            IUserRepository userRepository = new JsonUserRepository("users_en");
            this.userRepository = userRepository;

            InitializeComponent();

            if(bypass)
            {
                Manufacture.Global_Memory.UserLevel = 2;
                GlobalNew.UserLevel = 2;
                GlobalNew.CurrentUser = "MTE";
                Manufacture.Global_Memory.User = "MTE";
            }

            m_bypass = bypass;
        }
        public bool AuthenticateUser(string username, string password)
        {
            User user = null;
            string Weblogin;
            INIHelper iniConfig = new INIHelper(Global.IniConfigFile);
            Weblogin = iniConfig.Readini("Station", "Weblogin").Trim();
            string UserToken = string.Empty;
            string UserName = string.Empty;
            if (Weblogin != "1")
            {
                Console.WriteLine("Use Local Post to give user level");
                user = userRepository.GetUserByUsername(username);
                if (user != null && user.Password == password)
                {
                    Console.WriteLine($"Welcome, {username}! Your level: {user.Level}");
                    Manufacture.Global_Memory.UserLevel = user.Level;
                    Manufacture.Global_Memory.User = user.username;
                    GlobalNew.UserLevel = user.Level;
                    GlobalNew.CurrentUser = user.username;
                    return true;
                }
                else
                {
                    Console.WriteLine("Invalid username or password.");
                    return false;
                }
            }
            else
            {
                string strURL = "https://10.1.10.107/sla/login";
                try
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(strURL);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    httpWebRequest.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        JObject jReq = new JObject();
                        jReq.Add("username", username);
                        jReq.Add("password", password);

                        streamWriter.Write(jReq.ToString());
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        string result = streamReader.ReadToEnd();                      
                            try
                            {
                                JObject jResponse = JObject.Parse(result);
                                Console.WriteLine($"Welcome, {username}! Your level: {jResponse["permission"]}");
                                UserName = username;

                                if ((int)jResponse["permission"] == 5)
                                {                                   
                                    GlobalNew.UserToken = $"{jResponse["token"]}";
                                    GlobalNew.userList = userRepository.GetListuser();

                                    string jsonData = GlobalNew.userList;
                                    // 初始化时从 JSON 文件读取用户数据
                                    //user = JsonConvert.DeserializeObject<List<User>>(jsonData);
                                    GlobalNew.users = JsonConvert.DeserializeObject<List<User>>(jsonData);
                                }

                                Manufacture.Global_Memory.UserLevel = (int)jResponse["permission"];
                                GlobalNew.UserLevel = (int)jResponse["permission"];
                                GlobalNew.CurrentUser = user.username;
                                Manufacture.Global_Memory.User = user.username;
                            return true;
                            }
                            catch (Exception e1)
                            {
                                Console.WriteLine("Invalid username or password=>" + e1.Message);
                                return false;
                            }                       
                    }
                }
                catch (Exception e2)
                {
                    Console.WriteLine("SLA-Web Error=>" + e2.Message);
                    MessageBox.Show($"SLA-Web Error=>" + e2.Message);
                    return false;
                }
            }
            
        }

        private void LoginBtn_Click(object sender, EventArgs e)
        {
            //if (m_bypass)
            //{
            //    Manufacture.Global_Memory.UserLevel = 1;
            //    GlobalNew.UserLevel = 1;
            //    GlobalNew.CurrentUser = user.username;
            //    DialogResult = DialogResult.OK;
            //    this.Close();
            //}
            if(tbpassword.Text == "")
            {
                Manufacture.Global_Memory.UserLevel = 0;
                GlobalNew.UserLevel = 0;
                GlobalNew.CurrentUser = tbusername.Text;
                Manufacture.Global_Memory.User = tbusername.Text;
                DialogResult = DialogResult.OK;
                this.Close();
                return;
            }
            if (tbusername.Text == "" || tbpassword.Text == "")
            {
                MessageBox.Show("Invalid User Name or Password");
            }
            else
            {
                bool ret = AuthenticateUser(tbusername.Text, tbpassword.Text);
                if (ret)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }else
                {
                    //DialogResult = DialogResult.Cancel;
                    MessageBox.Show("Invalid User Name or Password");
                }
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

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        private void LoginForm_Load(object sender, EventArgs e)
        {
            RoundCorners(this, 25);
            tbusername.Focus();
            tbusername.Select();
            if(m_bypass)
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
            
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tbpassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbpassword.Text == "")
                {
                    Manufacture.Global_Memory.UserLevel = 0;
                    GlobalNew.UserLevel = 0;
                    GlobalNew.CurrentUser = tbusername.Text;
                    Manufacture.Global_Memory.User = tbusername.Text;
                    DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                }
                // 執行登入判斷的程式碼
                if (tbusername.Text == "" || tbpassword.Text == "")
                {
                    MessageBox.Show("Invalid User Name or Password");
                }
                else
                {
                    bool ret = AuthenticateUser(tbusername.Text, tbpassword.Text);
                    if (ret)
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        //DialogResult = DialogResult.Cancel;
                        MessageBox.Show("Invalid User Name or Password");
                    }
                }
            }
        }

        private void tbusername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbpassword.Text == "")
                {
                    Manufacture.Global_Memory.UserLevel = 0;
                    GlobalNew.UserLevel = 0;
                    GlobalNew.CurrentUser = tbusername.Text;
                    Manufacture.Global_Memory.User = tbusername.Text;
                    DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                }
            }
        }
    }


    public class User
    {
        public string username { get; set; }
        public string Password { get; set; }
        public string email { get; set; }
        public int permission { get; set; }

        public int Level { get; set; }
    }

    public interface IUserRepository
    {
        User GetUserByUsername(string username);
        string GetListuser();
    }

    public class JsonUserRepository : IUserRepository
    {
        private List<User> users;

        public JsonUserRepository()
        {
            string jsonData = string.Empty;
            string result = string.Empty;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://10.1.10.107/sla/api/testConnection");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

                    

            if (result.Contains("success"))
            {
                Console.WriteLine("SLA connect success");
            }
            else
            {
                Console.WriteLine("SLA connect fail");
            }
        }

        public JsonUserRepository(string filePath)
        {
           
           if (!File.Exists(filePath))
           {
              return;
           }
           string jsonData = EncryptionHelper.DecryptFile(filePath);                   
            // 初始化时从 JSON 文件读取用户数据
            users = JsonConvert.DeserializeObject<List<User>>(jsonData);
            GlobalNew.users = JsonConvert.DeserializeObject<List<User>>(jsonData);
        }

        public User GetUserByUsername(string username)
        {
            if (users == null || users.Count == 0)
            {
                //throw new InvalidOperationException("用户集合为空");
                return null;
            }
            return users.Find(u => u.username == username);
        }

        public string GetListuser()
        {
            string result = string.Empty;
            //User userInfo = null;
            List<string> userInfoList = new List<string>();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://10.1.10.107/sla/api/users");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer" + " " + GlobalNew.UserToken);
            httpWebRequest.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                string streamresult = streamReader.ReadToEnd();               
                if (IsJson(streamresult))
                {
                    result = streamresult;                                  
                }
                else
                {
                    result = "";                    
                }
                return result;
            }                  
        }
        static bool IsJson(string input)
        {
            try
            {
                JsonConvert.DeserializeObject(input);
                return true;
            }
            catch(JsonException)
            {
                return false;
            }
        }
    }
}
