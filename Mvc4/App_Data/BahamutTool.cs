using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Mvc4.App_Data
{
    public class BahamutTool
    {
        #region GetBahaTool
        public string[] GetRankList(string source)
        {
            int start = source.IndexOf("<table class=\"BH-table BH-table1\"> ", StringComparison.InvariantCulture) + 36;
            int end = source.IndexOf("</table>", StringComparison.InvariantCulture);
            source = source.Substring(start, end - start);
            return source.Split(new[] { "</tr>" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public string GetLink(string source)
        {
            int start = source.IndexOf("<a href=\"", StringComparison.InvariantCulture) + 9;
            int end = source.IndexOf("\" title=\"", StringComparison.InvariantCulture);
            return source.Substring(start, end - start);
        }

        public string[] GetPopularnArticle(string source)
        {
            var ar = source.Split(new[] { " | " }, StringSplitOptions.RemoveEmptyEntries);
            return new[] { GetNumberStr(ar[1]), GetNumberStr(ar[2]) };
        }

        public string GetHtmlDefault(string url)
        {
            string res = null;
            var wRq = WebRequest.Create(url);
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

        public string GetHtmlUtf8(string url)
        {
            string res = null;
            var wRq = WebRequest.Create(url);
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

        public static bool IsValidUrl(string url)
        {
            const string pattern = @"^(http|https|ftp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
            var reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return reg.IsMatch(url);
        }

        public static int GetNumber(string source)
        {
            int i;
            int.TryParse(Regex.Match(source, @"\d+").Value, out i);
            return i;
        }

        public static string GetNumberStr(string source)
        {
            return Regex.Match(source, @"\d+").Value;
        }
        #endregion
    }

    [Serializable]
    public class RankList
    {
        public int Rank { get; set; }
        //public int Popular { get; set; }
        //public int Article { get; set; }
        //public string Rank { get; set; }
        public string Popular { get; set; }
        public string Article { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public DateTime Date { get; set; }
        public string Change { get; set; }
    }

    public class ScoreList
    {
        public string Popular { get; set; }
        public string Article { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public int Date { get; set; }
    }
}