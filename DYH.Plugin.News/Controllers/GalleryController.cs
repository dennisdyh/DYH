using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DYH.Core.Basic;
using DYH.Core.Plugin;

namespace DYH.Plugin.News.Controllers
{
    public class GalleryController : Controller
    {
        //
        // GET: /Gallery/
        public ActionResult Index()
        {
            ViewBag.Content = "Just a test! Gallery" + DateTime.Now.ToString();
            return View("~/Plugins/DYH.Plugin.News/Views/Gallery/Index.cshtml", model: null);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var finder = AppContext.Current.Resolve<IPluginFinder>();
            var info = finder.GetPluginDescriptorBySystemName<IPlugin>("DYH.Plugin.News");
            if (info.Installed)
            {
                filterContext.HttpContext.Response.Redirect("~/Home/Index", true);
            }
            base.OnActionExecuting(filterContext);
          
        }
    }
}
