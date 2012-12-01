using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apache.Cassandra;

namespace DiedTool
{
    public class Mobile01Tool
    {
        public static int TempCounter;
        public static string TempLog;

        public static int ChangePage = 2300;
        public static int ChangeForum = 5000;
        public static int ChangeTopic = 4000;

        public static void Testit()
        {
            Console.WriteLine("Processing Url = " );
            //ThriftTool.CounterAdd("65535", "M01UserRelaction", "65536", 1);
        }

        public static void ProcessUrl(string url, int thisPage, string aid, string aowner)
        {
            var thisUrl = url;
            if (thisPage != 1)
                thisUrl = url + "&p=" + thisPage;

            Console.WriteLine("Processing Url = " + thisUrl);
            
            //TempLog += "page=" + thisPage + new HtmlString("<BR>");
            //var aid = string.Empty;
            //var aowner = string.Empty;

            try
            {
                var forum = thisUrl.Split(new[] { "f=" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('&')[0];

                //Get Content
                var source = WebTool.GetHtmlUtf8(thisUrl);
                TempLog = source;
                source = WebTool.GetContent("<div class=\"forum-content\">", "<div class=\"sidebar\">", source);

                //Get Title
                var title = WebTool.GetContent("<h2 class=\"topic\">", "</h2>", source);
                //Get Page
                var page = WebTool.GetContent("<p class=\"numbers\">", "</p>", source);
                //var pageNow = WebTool.GetContent("<span>", "</span>", page);
                var pageTotal = WebTool.GetContent("共", "頁", page);

                //process post -begin-
                source = WebTool.GetContent("<div class=\"single-post\">", "<div class=\"pagination\">", source);
                var ar = source.Split(new[] { "<div class=\"single-post\">" }, StringSplitOptions.RemoveEmptyEntries);

                //Get all
                foreach (var s in ar)
                {
                    ProcessPost(s, ref aid, ref aowner, forum, title);
                }
                //process post -end-
                //ThriftTool.TransportClose(ref _transport);

                //TempCounter++;

                if (thisPage < int.Parse(pageTotal))
                {
                    Thread.Sleep(ChangePage);
                    ProcessUrl(url, thisPage + 1, aid, aowner);
                }
                //else
                //{
                //    ThriftTool.TransportClose();
                //}
                
            }
            catch (Exception ex)
            {
                TempLog += ex.Message;
                //throw;
            }



        }

        public static void ProcessUrl(string url)
        {
            ProcessUrl(url, 1, null, null);
        }

        public static void ProcessForum(string forumUrl, int thisPage)
        {
            var thisUrl = forumUrl;
            if (thisPage != 1)
                thisUrl = forumUrl + "&p=" + thisPage;
            //Get Content
            Console.WriteLine("Processing Forum = " + thisUrl);
            var source = WebTool.GetHtmlUtf8(thisUrl);

            //Get Page
            var page = WebTool.GetContent("<p class=\"numbers\">", "</p>", source);
            //var pageNow = WebTool.GetContent("<span>", "</span>", page);
            var pageTotal = WebTool.GetContent("共", "頁", page);

            source = WebTool.GetContent("<table summary=\"文章列表\">", "</table>", source);
            source = WebTool.GetContent("<tbody>", "</tbody>", source).Replace("\n", string.Empty); //topic list
            var arTopic = source.Split(new[] { "<tr>" }, StringSplitOptions.RemoveEmptyEntries);
            arTopic = arTopic.Where(x => x.Trim().Length > 10).ToArray();
            foreach (var s in arTopic)
            {
                var topicUrl = WebTool.GetContent("<a href=\"", "\"", s);
                var topicTitle = WebTool.StripTagsCharArray((s.Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries)[0])).Trim();
                QueuePage(topicUrl, topicTitle);
            }
            //return arTopic.Count();

            //ThriftTool.TransportClose();

            //go to next page
            if (thisPage < int.Parse(pageTotal))
            {
                Thread.Sleep(ChangePage);
                ProcessForum(forumUrl, thisPage + 1);
            }
            //else
            //{
            //    ThriftTool.TransportClose();
            //}
            
        }

        public static void ProcessForum(string forumUrl)
        {
            ProcessForum(forumUrl, 1);
        }

        private static void ProcessPost(string post, ref string levelOneUid, ref string levelOnePid, string forum, string title)
        {
            var uid = WebTool.GetContent("userinfo.php?id=", "&", post);
            var content = WebTool.GetContent("<div class=\"single-post-content\">", "<div class=\"single-post-content-sig\">", post);
            var pid = WebTool.GetContent("<div id=\"ct", "\"", post);
            var tmpar = WebTool.GetContent("<div class=\"date\">", "</div>", post).Split('#');
            var pdate = tmpar[0].Trim();
            var plevel = tmpar[1].Trim();
            var aid = string.Empty;
            var aowner = string.Empty;
            if (plevel == "1")
            {
                levelOnePid = pid;
                levelOneUid = uid;
            }
            else
            {
                aid = levelOnePid;      //article = self
                aowner = levelOneUid;   //article owner = self
                //add counter
                //need check if new***
                if (!ThriftTool.CheckExist(uid, "M01UserRelaction"))
                    ThriftTool.CounterAdd(uid, "M01UserRelaction", levelOneUid, 1);
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

            //TempLog += "topic Pid=" + topic.Pid + new HtmlString("<BR>") ;
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

        public static void ProcessQueue()
        {
            ProcessQueue(5);
        }

        public static void ProcessQueue(int limit)
        {
            //var result = string.Empty;

            //int tempi = 0;

            CqlResult cqlResult = ThriftTool.GetByCql("Select * from UrlQueue where 'Status'='" + QueueType.Queue.ToString() + "' and 'Type'='M01' limit "+limit);
            //result = cqlResult.Rows.Count.ToString();
            //CqlResult cqlResult = ThriftTool.GetByCql("select top 1 * from UrlQueue where Status=" + (int)QueueType.Queue , client);
            //CqlResult cqlResult = client.execute_cql_query(ThriftTool.ToByte("select * from BahamutGames where Title='" + gameList[i] + "'"), Compression.NONE);
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
                    //result = url.Url;
                    //process topic
                    //result += url.Url;

                }

                if (url.Url != null)
                {
                    //result = "http://www.mobile01.com/" + url.Url;
                    ProcessUrl("http://www.mobile01.com/" + url.Url);
                    //tempi++;
                }

                //mark as done
                ThriftTool.AddColumn(url.Url, "UrlQueue", "Status", QueueType.Done.ToString());
            }
            //return tempi.ToString();
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

    public enum QueueType
    {
        Queue,
        Done
    }
}
