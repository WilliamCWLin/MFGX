using AutoTestSystem.Model;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTestSystem.DAL
{
    /// <summary>
    /// 扩展方法和公共方法类
    /// </summary>
    public static class HttpAsync
    {
        //public static string logPath;

        public static async Task<string> GetStringSafeAsync(this HttpClient client, Uri uri)
        {
            int retry = 0;
            var str = "";

            while (retry < 10)
            {
                try
                {
                    retry++;
                    var bytes = await client.GetByteArrayAsync(uri);
                    str = System.Text.Encoding.UTF8.GetString(bytes);
                    return str;
                }
                catch (Exception ex)
                {
                    Global.SaveLog($"HttpClient GetByteArrayAsync 请求异常次数{retry},Retey. {ex.ToString()}");
                    Thread.Sleep(1000);
                }
            }
            return str;
        }

        public static async Task<string> GetStringSafeAsync(this HttpContent client)
        {
            int retry = 0;
            var str = "";

            while (retry < 10)
            {
                try
                {
                    retry++;
                    var bytes = await client.ReadAsByteArrayAsync();
                    str = System.Text.Encoding.UTF8.GetString(bytes);
                    return str;
                }
                catch (Exception ex)
                {
                    Global.SaveLog($"HttpContent ReadAsByteArrayAsync 异常次数{retry},Retey. {ex.ToString()}");
                    //throw;
                }
            }
            return str;
        }

        public static byte[] ToByteArray(this string str)
        {
            return System.Text.ASCIIEncoding.ASCII.GetBytes(str);
        }


        public static string ToDetailsString(this OutletInfo outlet)
        {
            return $"{outlet.Index} - {outlet.Name} - {(outlet.IsOn ? "ON" : "OFF")}";
        }


        public static string ToDetailsString(this SwitchInfo switchInfo)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Switch: {switchInfo.Name}");
            foreach (var o in switchInfo.Outlets)
            {
                sb.AppendLine(o.ToDetailsString());
            }
            return sb.ToString();
        }


    }
}
