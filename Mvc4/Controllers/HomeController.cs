using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Apache.Cassandra;
using DiedTool;

namespace Mvc4.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "修改此範本即可開始著手進行您的 ASP.NET MVC 應用程式。";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "您的應用程式描述頁面。";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "您的連絡頁面。";

            return View();
        }

        public ActionResult M01Test()
        {
            ViewBag.Message = "Mobile01 test";
            //Mobile01Tool.TempCounter=0;
            //Mobile01Tool.TempLog = string.Empty;
            //ViewBag.Result = Mobile01Tool.ProcessUrl("http://www.mobile01.com/topicdetail.php?f=566&t=2982467"); 
            //ViewBag.Result = Mobile01Tool.ProcessForum("http://www.mobile01.com/forumtopic.php?c=2"); 
            //Mobile01Tool.ProcessUrl("http://www.mobile01.com/topicdetail.php?f=18&t=2990078&m=f&r=9"); 
            //Mobile01Tool.ProcessForum("http://www.mobile01.com/forumtopic.php?c=2"); 
            //Mobile01Tool.ProcessQueue();
            //ViewBag.Result = Mobile01Tool.Testit();
            
            var re = ThriftTool.GetAllFromCF("M01UserRelaction", 120000);
            var resulr = "org count:" + re.Count +"<br>";
            re = re.Where(x => x.Columns.Count > 1).ToList();
            resulr += ">1 count:"+re.Count.ToString(CultureInfo.InvariantCulture) + "<br/>";
            //re.Sort((k1, k2) => Comparer<int>.Default.Compare(k2.Columns.Count, k1.Columns.Count));
            //foreach (var ks in re)
            //{
            //    resulr += "Columns.Count:" + ks.Columns.Count.ToString(CultureInfo.InvariantCulture) + "<br/>";
                    
            //        foreach (var keySlice in ks.Columns)
            //        {
            //            //if (keySlice.Counter_column.Value <= 1) continue;
            //            var key = ThriftTool.ToString(keySlice.Counter_column.Name);
            //            var val = keySlice.Counter_column.Value;
            //            resulr += ("key=" + ThriftTool.ToString(ks.Key) + " name=" + key + " value=" + val + "<br/>");
            //        }
            //}
            ViewBag.Result = MvcHtmlString.Create(resulr);
             
            //MvcHtmlString br = new MvcHtmlString("<br>");
            //ThriftTool.CounterAdd("65535", "M01UserRelaction", "65533", 1);
            //ViewBag.Result = WebTool.GetHtmlAsyncUtf8("http://www.died.tw");
            return View();
        }


        public ActionResult M01List()
        {
            ViewBag.Message = "Mobile01 test";

            //var re = ThriftTool.GetAllFromCF("M01UserRelaction", 80000);
            //var resulr = "org count:" + re.Count + "<br>";
            //re = re.Where(x => x.Columns.Count > 1).ToList();
            //resulr += ">1 count:" + re.Count.ToString(CultureInfo.InvariantCulture) + "<br/>";
            //re.Sort((k1, k2) => Comparer<int>.Default.Compare(k2.Columns.Count, k1.Columns.Count));
            //foreach (var ks in re)
            //{
            //    resulr += "Columns.Count:" + ks.Columns.Count.ToString(CultureInfo.InvariantCulture) + "<br/>";

            //    foreach (var keySlice in ks.Columns)
            //    {
            //        if (keySlice.Counter_column.Value <= 1) continue;
            //        var key = ThriftTool.ToString(keySlice.Counter_column.Name);
            //        var val = keySlice.Counter_column.Value;
            //        resulr += ("key=" + ThriftTool.ToString(ks.Key) + " name=" + key + " value=" + val + "<br/>");
            //    }
            //}
            //ViewBag.Result = MvcHtmlString.Create(resulr);

            //MvcHtmlString br = new MvcHtmlString("<br>");
            //ThriftTool.CounterAdd("65535", "M01UserRelaction", "65533", 1);
            //ViewBag.Result = WebTool.GetHtmlAsyncUtf8("http://www.died.tw");
            return View();
        }

        public ActionResult M01Chart()
        {
            return View();
        }
    }
}
