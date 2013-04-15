using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Apache.Cassandra;
using DiedTool;

namespace M01Ant
{
    class Program
    {
        public static Dictionary<string, string> TmpUser = new Dictionary<string, string>();

        static void Main()
        {
            st:
            Console.WriteLine("\nPlease select action\n"+
                              "1:Process\n" +
                              "2:Show\n" +
                              "3:Test\n");
            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    Utility.ColoredConsoleWrite(ConsoleColor.Yellow, "Choose Process Data\n");
                    Console.WriteLine("1:Parse Forum\n" +
                                      "2:Parse Queue(default:5000)\n"+
                                      "3:Parse N^2\n" +
                                      "4:Parse N^3\n");
                    var sel = Console.ReadLine();
                    switch (sel)
                    {
                        case "1":
                            PraseFourm();
                            break;
                        case "2":
                            PraseQueue(5000);
                            break;
                        case "3":
                            PraseNnN();
                            break;
                        case "4":
                            PraseN3();
                            break;
                        default:
                            goto st;

                    }
                    break;
                case "2":
                    Utility.ColoredConsoleWrite(ConsoleColor.Yellow, "Choose Show Data\n");
                    Console.WriteLine("1:Show All Count\n" +
                                      "2:Show Relaction N^2\n" +
                                      "3:Show Relaction N^3)\n");
                    var show = Console.ReadLine();
                    switch (show)
                    {
                        case "1":
                            ShowAllCount();
                            break;
                        case "2":
                            ShowCounterOrder("M01UserRelactionN2", 500000);
                            break;
                        case "3":
                            ShowCounterOrder("M01UserRelactionN3", 500000);
                            break;
                        default:
                            goto st;
                    }
                    break;
                case "3":
                    TestT();
                    Utility.ColoredConsoleWrite(ConsoleColor.Yellow, "Bye-bye\n");
                    break;
                default:
                    Utility.ColoredConsoleWrite(ConsoleColor.Yellow, "Bye-bye\n");
                    break;
            }
            //InvokeStringMethod("Logging", "test");
            //Mobile01Tool.Testit();
            //Logging("Mobile01 bot start");
            //ThriftTool.ThriftTool.
            //PraseQueue(5000);
            
            //


        }


        static void TestP()     //get date and replay
        {
            var source = WebTool.GetHtmlAsyncUtf8("http://www.mobile01.com/forumtopic.php?c=16&s=20");

            source = WebTool.GetContent("<table summary=\"文章列表\">", "</table>", source);
            source = WebTool.GetContent("<tbody>", "</tbody>", source).Replace("\n", string.Empty); //topic list
            var arTopic = source.Split(new[] { "<tr>" }, StringSplitOptions.RemoveEmptyEntries);
            arTopic = arTopic.Where(x => x.Trim().Length > 10).ToArray();
            foreach (var s in arTopic)
            {
                var reply = WebTool.GetContent("<td width=\"7%\" class=\"reply\">", "</td>", s).Replace(",", string.Empty);
                var postDate = Convert.ToDateTime(WebTool.StripTagsCharArray(WebTool.GetContent("<td width=\"17%\" class=\"authur\">", "</p>", s)));
                var url = WebTool.GetContent("<td class=\"subject\">", "</a>", s);
                var topicUrl = WebTool.GetContent("<a href=\"", "\"", url);
                var topicTitle = WebTool.StripTagsCharArray((url.Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries)[0])).Trim();
                Console.WriteLine("topicUrl=" + topicUrl + "\ntopicTitle=" + HttpUtility.HtmlDecode(topicTitle) +
                                  "\nreply=" + reply + "\ndate=" + postDate);
                if (postDate>Convert.ToDateTime("2013-03-15"))
                {
                    Console.WriteLine("Date over 2013-03-15" + postDate);
                }
            }
        }


        static void TestT()
        {
            var source = WebTool.GetHtmlAsyncUtf8("http://www.mobile01.com/topicdetail.php?f=568&t=3299999");

            string aid = null;
            string aowner = null;
            string forum = null;
            TmpUser = new Dictionary<string, string>();
            
            source = WebTool.GetContent("<div class=\"forum-content\">", "<div class=\"sidebar\">", source);

            //Get Title
            var title = WebTool.GetContent("<h2 class=\"topic\">", "</h2>", source);
            //Get Page
            var page = WebTool.GetContent("<p class=\"numbers\">", "</p>", source);

            //process post -begin-
            source = WebTool.GetContent("<div class=\"single-post\">", "<div class=\"pagination\">", source);
            var ar = source.Split(new[] { "<div class=\"single-post\">" }, StringSplitOptions.RemoveEmptyEntries);

            //Get all
            foreach (var s in ar)
            {
                ProcessPost(s, ref aid, ref aowner, forum, title);
            }
        }

        static void ProcessPost(string post, ref string levelOneUid, ref string levelOnePid, string forum, string title)
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

            Console.WriteLine(TmpUser==null);
            if(!TmpUser.ContainsKey(uName)) TmpUser.Add(uName,uid);

            Console.WriteLine("pid=" + pid + " uid=" + uid + " name=" + uName);
            //if (!string.IsNullOrEmpty(replyTo)) Console.WriteLine("\tblockquote=" + replyTo);


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
                if(string.IsNullOrEmpty(replyTo))
                {
                    //ThriftTool.CounterAdd(uid, "M01UserRelaction", levelOneUid, 1);
                }
                else
                {

                    Console.WriteLine("\tblockquote=" + TmpUser[replyTo]);

                }
                //
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
            //if (blockquite != null) 

            //SaveTopic(topic);
        }

        #region Parse
        static List<string> LoadingFourmList()
        {
            var lineList = new List<string>();
            string line;
            var file = new StreamReader("forumlist.txt");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Length <= 10) continue;
                if (line.Substring(0, 1) != "#") lineList.Add(line);
            }

            file.Close();
            return lineList;
        }

        static void PraseFourm()
        {
            Utility.Logging("Start Prasing Forum", Utility.DebugLevel.Info);
            var list = LoadingFourmList();
            foreach (var fourmUrl in list)
            {
                Utility.Logging("Prasing Forum : " + fourmUrl, Utility.DebugLevel.Info);
                Mobile01Tool.ProcessForum(fourmUrl);
                Thread.Sleep(Mobile01Tool.ChangeForum);
            }
        }

        static void PraseQueue(int limit)
        {
            Utility.Logging("Prasing Queue limit : " + limit, Utility.DebugLevel.Info);
            Mobile01Tool.ProcessQueue(limit);
        }


        static void PraseNnN()
        {
            var count = 0;
            //Utility.Logging("Calculating N vs N from: " + count, Utility.DebugLevel.Info);
            //var re = ThriftTool.GetAllFromCF("M01UserRelaction", 120000);
            var re = ThriftTool.GetAllFromCF("M01UserRelaction", 100);
            foreach (var node in re)
            {
                if (count % 1000 == 0) Utility.Logging("Calculating N vs N from: " + count, Utility.DebugLevel.Info);
                var key = ThriftTool.ToString(node.Key);
                Utility.Logging("Key= " + key, Utility.DebugLevel.Info);
                
                foreach (var keySlice in node.Columns)
                {
                    var columnName = ThriftTool.ToString(keySlice.Counter_column.Name);
                    Utility.Logging("column.Name= " + columnName + " value=" + keySlice.Counter_column.Value, Utility.DebugLevel.Info);
                    if (key == columnName) continue;
                    Utility.Logging("go check> " + key + "_" + columnName, Utility.DebugLevel.Info);
                    Check2NodeValue(key, columnName, keySlice.Counter_column.Value);
                }
                count++;
            }
            Utility.Logging("total count: " + count, Utility.DebugLevel.Info);
        }

        static void PraseN3()
        {
            var count = 0;
            //Utility.Logging("Calculating N vs N from: " + count, Utility.DebugLevel.Info);
            var re = ThriftTool.GetAllFromCF("M01UserRelactionN2", 500000);
            foreach (var node in re)
            {
                //Utility.Logging("key="+ ThriftTool.ToString(node.Key), Utility.DebugLevel.Info);
                if (count % 1000 == 0) Utility.Logging("Calculating N vs N vs N from: " + count, Utility.DebugLevel.Info);
                var ar = ThriftTool.ToString(node.Key).Split('_');
                //Utility.Logging("n1=" + ar[0] + " n2="+ar[1], Utility.DebugLevel.Info);
                ProcessN3(ar[0], ar[1]);

                count++;
            }
            Utility.Logging("total count: " + count, Utility.DebugLevel.Info);
        }

        static void ProcessN3(string n1, string n2)
        {
            CqlResult n1Result = ThriftTool.GetByCql("Select * from M01UserRelaction where key='" + n1 + "' limit 50000");
            CqlResult n2Result = ThriftTool.GetByCql("Select * from M01UserRelaction where key='" + n2 + "' limit 50000");
            var n1List = n1Result.Rows.First().Columns.Select(col => ThriftTool.ToString(col.Name)).Where(name => name != "KEY").ToList();
            var n2List = n2Result.Rows.First().Columns.Select(col => ThriftTool.ToString(col.Name)).Where(name => name != "KEY").ToList();
            foreach (var n3 in n1List.Intersect(n2List))
            {
                //Utility.Logging("n3=" + n3, Utility.DebugLevel.Info);
                if (n1 == n3 || n2 == n3) continue;
                var sum = Get2NodeValue(n1, n2) + Get2NodeValue(n2, n3) + Get2NodeValue(n1, n3);
                var orderList = new List<int> { int.Parse(n1), int.Parse(n2), int.Parse(n3) };
                orderList.Sort();
                var key = orderList[2] + "_" + orderList[1] + "_" + orderList[0];
                ThriftTool.CounterAdd(key, "M01UserRelactionN3", "sum", sum);
                ThriftTool.CounterAdd(key, "M01UserRelactionN3", orderList[2] + "_" + orderList[1], Get2NodeValue(orderList[2].ToString(CultureInfo.InvariantCulture), orderList[1].ToString(CultureInfo.InvariantCulture)));
                ThriftTool.CounterAdd(key, "M01UserRelactionN3", orderList[2] + "_" + orderList[0], Get2NodeValue(orderList[2].ToString(CultureInfo.InvariantCulture), orderList[0].ToString(CultureInfo.InvariantCulture)));
                ThriftTool.CounterAdd(key, "M01UserRelactionN3", orderList[1] + "_" + orderList[0], Get2NodeValue(orderList[1].ToString(CultureInfo.InvariantCulture), orderList[0].ToString(CultureInfo.InvariantCulture)));
            }
        }
        #endregion

        #region Show
        static void ShowAllCount()
        {
            Utility.Logging("Total User :" + GetCfCount("M01UserRelaction", 400000), Utility.DebugLevel.Info);
            Utility.Logging("Total User :" + ThriftTool.GetKeyCount("M01UserRelaction","1640858",  400000), Utility.DebugLevel.Info);
            //Utility.Logging("Total N2 Relaction :" + GetCfCount("M01UserRelactionN2", 150000), Utility.DebugLevel.Info);
            //Utility.Logging("Total N3 Relaction :" + GetCfCount("M01UserRelactionN3", 150000), Utility.DebugLevel.Info);
            //Utility.Logging("Total Topic :" + GetCfCount("M01Topic", 500000), Utility.DebugLevel.Info);
        }

        static int GetCfCount(string cf, int limit)
        {
            var sqlStr = "select count(*) from \"M01UserRelaction\" limit 400000";
            //var sqlStr = "Select count(*) from '" + cf + "' limit " + limit;
            CqlResult n1Result = ThriftTool.GetByCql(sqlStr);
            Console.WriteLine("Type="+ n1Result.Type);
            Console.WriteLine("Num="+ n1Result.Num);
            foreach (var row in n1Result.Rows)
            {
                
                Console.WriteLine("row.Key="+ ThriftTool.ToString(row.Key));
                Console.WriteLine("row.Columns.Count="+row.Columns.Count);
                foreach (var col in row.Columns)
                {
                    Console.WriteLine("Type=" + col.Value.GetType());
                    Console.WriteLine("\tcol.Name=" + ThriftTool.ToString(col.Name));
                    Console.WriteLine("\tcol.Value=" + ThriftTool.ToLong(col.Value));
                }
            }

            return ThriftTool.ToInt(n1Result.Rows.First().Columns.First().Value);
        }

        static void ShowCounterOrder(string cf, int limit)
        {
            Utility.Logging("ShowCounterOrder CF:" + cf + " limit:" + limit, Utility.DebugLevel.Info);
            var ar = new Dictionary<string, long>();
            var re = ThriftTool.GetAllFromCF(cf, limit);
            Utility.Logging("org count:" + re.Count, Utility.DebugLevel.Info);
            
            foreach (var ks in re)
            {
                
                foreach (var keySlice in ks.Columns)
                {
                    if (ThriftTool.ToString(keySlice.Counter_column.Name) != "sum") continue;
                    var key = ThriftTool.ToString(ks.Key);
                    var val = keySlice.Counter_column.Value;
                    ar.Add(key, val);
                }
            }

            var res = from pair in ar orderby pair.Value descending select pair;
            foreach (var r in res.Take(500))
            {
                Console.WriteLine("key:" + r.Key + "\tvalue:" + r.Value);
            }
            Utility.Logging("end", Utility.DebugLevel.Info);
        }
        #endregion

        #region Utility
        static void Check2NodeValue(string n1, string n2, long val)
        {
            if (n1 == n2) return;
            var secondVal = ThriftTool.GetSingleCounter(ThriftTool.ToByte(n2), "M01UserRelaction", n1);

            var org1 = n1;
            var org2 = n2;
            //save N1>N2
            if (Int32.Parse(n2) > Int32.Parse(n1)) Utility.Swap(ref n1, ref n2);
            var key = n1 + "_" + n2;

            var sum = val + secondVal;
            Utility.Logging("\tKey: " + key, Utility.DebugLevel.Info);
            Utility.Logging("\tsecondVal: " + secondVal, Utility.DebugLevel.Info);
            Utility.Logging("\tsum: " + sum, Utility.DebugLevel.Info);
            Utility.Logging("\t" + org1 + ">" + org2 + ": " + val, Utility.DebugLevel.Info);
            Utility.Logging("\t" + org2 + ">" + org1 + ": " + secondVal, Utility.DebugLevel.Info);

            //ThriftTool.CounterAdd(key, "M01UserRelactionN2", "sum", val + secondVal);
            //ThriftTool.CounterAdd(key, "M01UserRelactionN2", org1 + " > " + org2, val);
            //ThriftTool.CounterAdd(key, "M01UserRelactionN2", org2 + " > " + org1, secondVal);

        }

        static long Get2NodeValue(string n1, string n2)
        {
            if (int.Parse(n2) > int.Parse(n1)) Utility.Swap(ref n1, ref n2);
            return ThriftTool.GetSingleCounter(ThriftTool.ToByte(n1 + "_" + n2), "M01UserRelactionN2", "sum");
        }

        /// <summary>
        /// Dynamic Calling Method by String
        /// </summary>
        /// <param name="methodName">Method Name </param>
        /// <param name="stringParam">Parameter</param>
        public static void InvokeStringMethod(string methodName, string stringParam)
        {
            Type calledType = typeof(Program);

            calledType.InvokeMember(
                        methodName,
                        BindingFlags.InvokeMethod | BindingFlags.Public |
                            BindingFlags.Static,
                        null,
                        null,
                        new Object[] { stringParam });

        }  
        #endregion








    }


}
