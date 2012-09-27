using System.Web.Mvc;
using Thrift.Transport;

namespace Mvc4.Controllers
{
    public class BahamutController : Controller
    {
        //
        // GET: /Bahamut/

        public ActionResult Index()
        {
            ViewBag.Title = "遊戲排行";
            return View();
        }

        public ActionResult Chart()
        {
            ViewBag.Title = "";
            return View();
        }
        //
        // GET: /Bahamut/Details/5

        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        ////
        //// GET: /Bahamut/Create

        //public ActionResult Create()
        //{
        //    return View();
        //}

        ////
        //// POST: /Bahamut/Create

        //[HttpPost]
        //public ActionResult Create(FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add insert logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        ////
        //// GET: /Bahamut/Edit/5

        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        ////
        //// POST: /Bahamut/Edit/5

        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        ////
        //// GET: /Bahamut/Delete/5

        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        ////
        //// POST: /Bahamut/Delete/5

        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
