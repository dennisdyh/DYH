using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DYH.Plugin.Gallery.Controllers
{
    public class GalleryController : Controller
    {
        //
        // GET: /Gallery/

        public ActionResult Index()
        {
            ViewBag.Content = "Just a test! Gallery" + DateTime.Now.ToString();
            return View("~/Plugins/DYH.Plugin.Gallery/Views/Gallery/Index.cshtml", model: null);
            
        }

    }
}
