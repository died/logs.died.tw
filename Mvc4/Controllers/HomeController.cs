using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
            Mobile01Tool.TempCounter=0;
            Mobile01Tool.TempLog = string.Empty;
            //ViewBag.Result = Mobile01Tool.ProcessUrl("http://www.mobile01.com/topicdetail.php?f=566&t=2982467"); 
            //ViewBag.Result = Mobile01Tool.ProcessForum("http://www.mobile01.com/forumtopic.php?c=2"); 
            Mobile01Tool.ProcessUrl("http://www.mobile01.com/topicdetail.php?f=18&t=2990078&m=f&r=9"); 
            //Mobile01Tool.ProcessForum("http://www.mobile01.com/forumtopic.php?c=2"); 
            Mobile01Tool.ProcessQueue();
            //Mobile01Tool.Testit();
            //ViewBag.Result = Mobile01Tool.TempLog;
            return View();
        }
    }
}
