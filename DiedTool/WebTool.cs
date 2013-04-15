using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiedTool
{
    public class WebTool
    {
        public static string GetContent(string sTag, string eTag, string source, bool contain)
        {
            if (string.IsNullOrEmpty(source)) return null;
            int start = source.IndexOf(sTag, StringComparison.InvariantCulture) + (!contain ? sTag.Length : 0);
            int end = source.IndexOf(eTag, start, StringComparison.InvariantCulture);
            if (start < 0 || end < 0 || (end-start<0)) return null;
            return source.Substring(start, end - start + (contain ? sTag.Length + eTag.Length : 0));
        }

        public static string GetContent(string sTag, string eTag, string source)
        {
            return GetContent(sTag, eTag, source, false);
        }

        public static string GetHtml(string url,Encoding encode)
        {
            string res = null;
            var wRq = (HttpWebRequest)WebRequest.Create(url);
            wRq.Method = "GET";
            wRq.UserAgent = "Mozilla/5.0+(Windows+NT+6.1;+WOW64)+AppleWebKit/536.11+(KHTML,+like+Gecko)+Chrome/20.0.1132.57+Safari/536.11";

            using (WebResponse wRs = wRq.GetResponse())
            {
                using (var s = wRs.GetResponseStream())
                {
                    if (s != null)
                    {
                        using (var sr = new StreamReader(s, encode))
                        {
                            res = sr.ReadToEnd();
                        }
                    }
                }
            }
            return res;
        }

        public static string GetHtmlUtf8(string url)
        {
            return GetHtml(url, Encoding.UTF8);
        }

        public static string GetHtmlDefault(string url)
        {
            return GetHtml(url, Encoding.Default);
        }

        public static string GetHtmlAsyncDefault(string url)
        {
            var task = MakeAsyncRequest(url, Encoding.Default);
            return task.Result;
        }

        public static string GetHtmlAsyncUtf8(string url)
        {
            var task = MakeAsyncRequest(url, Encoding.UTF8);
            return task.Result;
        }

        public static Task<string> MakeAsyncRequest(string url, Encoding encode)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "text/html";
            request.UserAgent = "Mozilla/5.0+(Windows+NT+6.1;+WOW64)+AppleWebKit/536.11+(KHTML,+like+Gecko)+Chrome/20.0.1132.57+Safari/536.11";
            request.Method = WebRequestMethods.Http.Get;
            request.Timeout = 10000;
            request.Proxy = null;   

            Task<WebResponse> task = Task.Factory.FromAsync(
                request.BeginGetResponse,
                asyncResult => request.EndGetResponse(asyncResult),null);

            return task.ContinueWith(t => ReadStreamFromResponse(t.Result, encode));
        }

        private static string ReadStreamFromResponse(WebResponse response, Encoding encode)
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                if (responseStream == null) return null;
                using (var sr = new StreamReader(responseStream, encode))
                {
                    //Need to return this response 
                    string strContent = sr.ReadToEnd();
                    return strContent;
                }
            }
        }

        public static string StripTagsCharArray(string source)
        {
            var array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            foreach (char @let in source)
            {
                if (@let == '<')
                {
                    inside = true;
                    continue;
                }
                if (@let == '>')
                {
                    inside = false;
                    continue;
                }
                if (inside) continue;
                array[arrayIndex] = @let;
                arrayIndex++;
            }
            return new string(array, 0, arrayIndex).Trim();
        }
    }
}
