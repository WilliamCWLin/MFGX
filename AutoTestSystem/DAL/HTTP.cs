using System;
using System.IO;
using System.Net;
using System.Text;

namespace AutoTestSystem.DAL
{
    class HTTP
    {
        //url:POST请求地址
        //postData:json格式的请求报文,例如：{"key1":"value1","key2":"value2"}
        public string Post(string Url, string jsonParas)
        {
            //创建一个HTTP请求 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);            
            request.Method = "POST";    //Post请求方式             
            request.ContentType = "application/json";   //内容类型             
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(jsonParas);  //将Json字符串转化为字节             
            request.ContentLength = payload.Length;  //设置请求的ContentLength

            //发送请求，获得请求流
            Stream writer;
            try
            {
                writer = request.GetRequestStream();//获取用于写入请求数据的Stream对象
            }
            catch (Exception)
            {
                writer = null;
                Console.Write("连接服务器失败!");
            }
            //将请求参数写入流
            writer.Write(payload, 0, payload.Length);
            writer.Close();//关闭请求流
                        
            HttpWebResponse response;
            try
            {
                //获得响应流
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;
            }
            Stream s = response.GetResponseStream();            
            StreamReader sRead = new StreamReader(s);
            string postContent = sRead.ReadToEnd();
            sRead.Close();
            return response.StatusCode.GetHashCode().ToString() + "+" + postContent;          
        }


        public string PostUrl(string url, string postData)
        {

            string result = "";
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                //  req.Timeout = 800;//设置请求超时时间，单位为毫秒
                req.ContentType = "application/json";
                // req.CookieContainer = cookie;
                byte[] data = Encoding.UTF8.GetBytes(postData);
                req.ContentLength = data.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();
                //获取响应内容
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
                return result;
            }
            catch (WebException webce)
            {
                result = (webce.Message.ToString());
                return result.Replace("The remote server returned an error: (500) Internal Server Error.", "服务器出现故障无法连接");
            }
            catch (Exception ce)
            {
                return result = ce.ToString();
            }

        }



        //url:GET请求地址
        public string HttpGet(string Url)
        {
            string retString = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url); ;
                request.Method = "GET";
                request.Timeout = 10000;
                request.KeepAlive = true;
                request.ContentType = "application/json;charset=UTF-8";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                return retString;
            }
            catch (WebException webce)
            {
                retString = (webce.Message.ToString());
                return retString.Replace("The remote server returned an error: (500) Internal Server Error.", "服务器出现故障无法连接");
            }
            catch (Exception ce)
            {
                return retString = ce.ToString();
            }
        }
    }
}
