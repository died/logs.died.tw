using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Apache.Cassandra;

namespace DiedTool
{
    public class Mobile01Tool
    {
        public static int ErrorCounter;
        public static string TempLog;

        public static int ChangePage = 3000;
        public static int ChangeForum = 5000;
        public static int ChangeTopic = 2500;

        public static Dictionary<string, string> TmpUser = new Dictionary<string, string>();

        public static string Testit()
        {
            
            CqlResult cqlResult = ThriftTool.GetByCql("Select * from UrlQueue where 'Status'='" + QueueType.Done + "' and 'Type'='M01' limit 50000");
            //foreach (var row in cqlResult.Rows)
            //{
            //    var url = new UrlQueue();
            //    foreach (var col in row.Columns)
            //    {
            //        var name = ThriftTool.ToString(col.Name);

            //        switch (name)
            //        {
            //            case "Title":
            //                url.Title = ThriftTool.ToString(col.Value);
            //                break;
            //            case "Url":
            //                url.Url = ThriftTool.ToString(col.Value);
            //                break;
            //            case "Status":
            //                url.Status = ThriftTool.ToString(col.Value);
            //                break;
            //            case "Type":
            //                url.Type = ThriftTool.ToString(col.Value);
            //                break;
            //        }
            //    }


            //    //mark as done
            //    ThriftTool.AddColumn(url.Url, "UrlQueue", "Status", QueueType.Queue.ToString());
            //}
            return cqlResult.Rows.Count.ToString(CultureInfo.InvariantCulture);
            //ThriftTool.CounterAdd("65535", "M01UserRelaction", "65536", 1);
        }

        /// <summary>
        /// 處理文章
        /// </summary>
        /// <param name="url"></param>
        /// <param name="thisPage"></param>
        /// <param name="aid"></param>
        /// <param name="aowner"></param>
        public static bool ProcessUrl(string url, int thisPage, string aid, string aowner)
        {
            var pageTotal = string.Empty;
            var thisUrl = url;
            if (thisPage != 1)
                thisUrl = url + "&p=" + thisPage;

            Utility.Logging("Processing Url = " + thisUrl,Utility.DebugLevel.Info);

            try
            {
                var forum = thisUrl.Split(new[] { "f=" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('&')[0];

                //Get Content
                var source = WebTool.GetHtmlAsyncUtf8(thisUrl);

                //if (!string.IsNullOrEmpty(source))
                //{
                    //if (source.IndexOf("action=\"error.php\"", System.StringComparison.Ordinal) == -1) return;
                //Console.WriteLine("Processing Url start");

                    source = WebTool.GetContent("<div class=\"forum-content\">", "<div class=\"sidebar\">", source);

                    //Get Title
                    var title = WebTool.GetContent("<h2 class=\"topic\">", "</h2>", source);
                    //Get Page
                    var page = WebTool.GetContent("<p class=\"numbers\">", "</p>", source);
                    //var pageNow = WebTool.GetContent("<span>", "</span>", page);
                    pageTotal = WebTool.GetContent("共", "頁", page);

                    //process post -begin-
                    source = WebTool.GetContent("<div class=\"single-post\">", "<div class=\"pagination\">", source);
                    var ar = source.Split(new[] { "<div class=\"single-post\">" }, StringSplitOptions.RemoveEmptyEntries);

                    //Get all
                    foreach (var s in ar)
                    {
                        ProcessPost(s, ref aid, ref aowner, forum, title);
                    }
                //}


                //process post -end-
                //ThriftTool.TransportClose(ref _transport);

                if (thisPage >= int.Parse(pageTotal)) return true;
                Thread.Sleep(ChangePage);
                return ProcessUrl(url, thisPage + 1, aid, aowner);
            }
            catch (Exception ex)
            {
                Utility.Logging("ProcessUrl Error:" + ex.Message);
                return false;
            }
        }

        public static bool ProcessUrl(string url)
        {
            return ProcessUrl(url, 1, null, null);
        }

        /// <summary>
        /// 處理列表
        /// </summary>
        /// <param name="forumUrl"></param>
        /// <param name="thisPage"></param>
        public static void ProcessForum(string forumUrl, int thisPage)
        {
            var thisUrl = forumUrl;
            if (thisPage != 1)
                thisUrl = forumUrl + "&p=" + thisPage;
            //Get Content
            Utility.Logging("Processing Forum = " + thisUrl,Utility.DebugLevel.Info);

            var source = WebTool.GetHtmlAsyncUtf8(thisUrl);
              
            //Get Page
            var page = WebTool.GetContent("<p class=\"numbers\">", "</p>", source);
            var pageTotal = WebTool.GetContent("共", "頁", page);

            source = WebTool.GetContent("<table summary=\"文章列表\">", "</table>", source);
            source = WebTool.GetContent("<tbody>", "</tbody>", source).Replace("\n", string.Empty); //topic list

            var arTopic = source.Split(new[] { "<tr>" }, StringSplitOptions.RemoveEmptyEntries);
            arTopic = arTopic.Where(x => x.Trim().Length > 10).ToArray();
            foreach (var s in arTopic)
            {
                var reply = WebTool.GetContent("<td width=\"7%\" class=\"reply\">", "</td>", s).Replace(",", string.Empty);
                var postDate = Convert.ToDateTime(WebTool.StripTagsCharArray(WebTool.GetContent("<td width=\"17%\" class=\"authur\">", "</p>", s)));
                //if (postDate)
                if (int.Parse(reply) >= 500) continue;
                var url = WebTool.GetContent("<td class=\"subject\">", "</a>", s);
                var topicUrl = WebTool.GetContent("<a href=\"", "\"", url);
                var topicTitle = WebTool.StripTagsCharArray((url.Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries)[0])).Trim();
                QueuePage(topicUrl, topicTitle);
            }

            //go to next page
            if (thisPage < int.Parse(pageTotal))
            {
                Thread.Sleep(ChangePage);
                ProcessForum(forumUrl, thisPage + 1);
            }

        }

        public static void ProcessForum(string forumUrl)
        {
            TmpUser = new Dictionary<string, string>();
            ProcessForum(forumUrl, 1);
        }

        private static void ProcessPost(string post, ref string levelOneUid, ref string levelOnePid, string forum, string title)
        {
            var uid = WebTool.GetContent("userinfo.php?id=", "&", post);
            var uName = WebTool.StripTagsCharArray(WebTool.GetContent("<div class=\"fn\">", "</div>", post));
            var content = WebTool.GetContent("<div class=\"single-post-content\">", "<div class=\"single-post-content-sig\">", post);
            var pid = WebTool.GetContent("<div id=\"ct", "\"", post);
            var tmpar = WebTool.GetContent("<div class=\"date\">", "</div>", post).Split('#');
            var pdate = tmpar[0].Trim();
            var plevel = tmpar[1].Trim();
            var aid = string.Empty;
            var aowner = string.Empty;
            var blockquite = WebTool.GetContent("<blockquote>", "</blockquote>", post);
            var replyTo = WebTool.GetContent("<b>", " wrote:</b>", blockquite);
            if (!TmpUser.ContainsKey(uName)) TmpUser.Add(uName, uid);

            if (plevel == "1")
            {
                levelOnePid = pid;
                levelOneUid = uid;
            }
            else
            {
                aid = levelOnePid;      
                aowner = levelOneUid;   
             
                //add counter
                ThriftTool.CounterAdd(uid, "M01UserRelaction",string.IsNullOrEmpty(replyTo) ? levelOneUid : TmpUser[replyTo], 1);
            }
            var topic = new M01Topic
            {
                Forum = forum,
                Pid = pid,
                Uid = uid,
                Content = content,
                Pdate = pdate,
                Plevel = plevel,
                Aid = aid,
                Aowner = aowner,
                Title = title
            };

            SaveTopic(topic);
        }

        private static void SaveTopic(M01Topic topic)
        {
            ThriftTool.AddColumn(topic.Pid, "M01Topic", "Forum", topic.Forum);
            ThriftTool.AddColumn(topic.Pid, "M01Topic", "Pdate", topic.Pdate);
            ThriftTool.AddColumn(topic.Pid, "M01Topic", "Pid", topic.Pid);
            ThriftTool.AddColumn(topic.Pid, "M01Topic", "Uid", topic.Uid);
            ThriftTool.AddColumn(topic.Pid, "M01Topic", "Content", topic.Content);
            ThriftTool.AddColumn(topic.Pid, "M01Topic", "Plevel", topic.Plevel);
            ThriftTool.AddColumn(topic.Pid, "M01Topic", "Aid", topic.Aid);
            ThriftTool.AddColumn(topic.Pid, "M01Topic", "Aowner", topic.Aowner);
            ThriftTool.AddColumn(topic.Pid, "M01Topic", "Title", topic.Title);
        }

        public static string GetRelactionCount()
        {
            var br = MvcHtmlString.Create("<br/>");
            string resulr = string.Empty;
            CqlResult cqlResult = ThriftTool.GetByCql("Select * from M01UserRelaction limit 100000");
            // cqlResult.Rows
            resulr = cqlResult.Rows.Count.ToString(CultureInfo.InvariantCulture) + br;
            //var fir = cqlResult.Rows.First();
            //foreach (var col in fir.Columns)
            //{
            //    resulr += "key:" + ThriftTool.ToString(fir.Key) + " name:" + ThriftTool.ToString(col.) + " value:" + ThriftTool.ToInt(col.Value) + br+Environment.NewLine;
            //}


            //var re = ThriftTool.GetAllFromCF("M01UserRelaction", 5);
            //foreach (var ks in re)
            //{
            //    foreach (var keySlice in ks.Columns)
            //    {
            //        var key = ThriftTool.ToString(keySlice.Counter_column.Name);
            //        var val = keySlice.Counter_column.Value;
            //        resulr+=("\r\n key="+ThriftTool.ToString(ks.Key)+ "name='" + key + " value=" + val);
            //    }
            //}
            
            return resulr;
        }

        public static void ProcessQueue()
        {
            ProcessQueue(5);
        }

        public static void ProcessQueue(int limit)
        {
            CqlResult cqlResult = ThriftTool.GetByCql("Select * from UrlQueue where 'Status'='" + QueueType.Queue.ToString() + "' and 'Type'='M01' limit "+limit);

            if (cqlResult.Rows.Count < limit) Utility.Logging("select result count:" + cqlResult.Rows.Count);

            foreach (var row in cqlResult.Rows)
            {
                var url = new UrlQueue();
                foreach (var col in row.Columns)
                {
                    var name = ThriftTool.ToString(col.Name);

                    switch (name)
                    {
                        case "Title":
                            url.Title = ThriftTool.ToString(col.Value);
                            break;
                        case "Url":
                            url.Url = ThriftTool.ToString(col.Value);
                            break;
                        case "Status":
                            url.Status = ThriftTool.ToString(col.Value);
                            break;
                        case "Type":
                            url.Type = ThriftTool.ToString(col.Value);
                            break;
                    }
                }

                //Console.WriteLine("Title:" + url.Title);
                //Console.WriteLine("Url:" + url.Url);
                //Console.WriteLine("Status:" + url.Status);
                //Console.WriteLine("Type:" + url.Type);

                if (url.Url != null)
                {
                    Thread.Sleep(ChangeTopic);
                    ProcessUrl("http://www.mobile01.com/" + url.Url);
                }

                //mark as done
                ThriftTool.AddColumn(url.Url, "UrlQueue", "Status", QueueType.Done.ToString());
            }
        }

        private static void QueuePage(string url, string title)
        {

            var urlQueue = new UrlQueue
            {
                Url = url,
                Title = title,
                Type = "M01",
                Status = QueueType.Queue.ToString()
            };
            SaveQueue(urlQueue);
        }

        private static void SaveQueue(UrlQueue url)
        {
            ThriftTool.AddColumn(url.Url, "UrlQueue", "Url", url.Url);
            ThriftTool.AddColumn(url.Url, "UrlQueue", "Title", url.Title);
            ThriftTool.AddColumn(url.Url, "UrlQueue", "Type", url.Type);
            ThriftTool.AddColumn(url.Url, "UrlQueue", "Status", url.Status);
        }

    }

    [Serializable]
    public class M01Topic
    {
        public string Forum { get; set; }
        public string Uid { get; set; }
        public string Pid { get; set; }
        public string Aid { get; set; }
        public string Aowner { get; set; }
        public string Content { get; set; }
        public string Pdate { get; set; }
        public string Plevel { get; set; }
        public string Title { get; set; }
    }

    [Serializable]
    public class UrlQueue
    {
        public string Url { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
    }

    [Serializable]
    public class M01Relaction
    {
        public M01RelactionNode Nodes { get; set; }
        public M01RelactionEdge Edges { get; set; }
    }

    [Serializable]
    public class M01RelactionNode
    {
        public List<RelactionNode> NodeList { get; set; }
    }

    [Serializable]
    public class M01RelactionEdge
    {
        public List<RelactionEdge> EdgeList { get; set; }
    }

    [Serializable]
    public class RelactionEdge
    {
        public string id { get; set; }
        public string source { get; set; }
        public string target { get; set; }
        public int weight { get; set; }
    }

    [Serializable]
    public class RelactionNode
    {
        public string id { get; set; }
        public int weight { get; set; }
    }

    public enum QueueType
    {
        Queue,
        Done
    }
}
