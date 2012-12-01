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
            int start = source.IndexOf(sTag, StringComparison.InvariantCulture) + (!contain ? sTag.Length : 0);
            int end = source.IndexOf(eTag, start, StringComparison.InvariantCulture);
            return source.Substring(start, end - start + (contain ? sTag.Length + eTag.Length : 0));
        }

        public static string GetContent(string sTag, string eTag, string source)
        {
            return GetContent(sTag, eTag, source, false);
        }

        public static string GetHtmlUtf8(string url)
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
                        using (var sr = new StreamReader(s, Encoding.UTF8))
                        {
                            res = sr.ReadToEnd();
                        }
                    }
                }
            }
            return res;
        }

        public static string GetHtmlDefault(string url)
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
                        using (var sr = new StreamReader(s, Encoding.Default))
                        {
                            res = sr.ReadToEnd();
                        }
                    }
                }
            }
            return res;
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
